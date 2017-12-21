using Bettha_Scoring.Models;
using System;
using System.Data;
using System.Data.Entity;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using MathNet.Numerics.Distributions;

namespace Bettha_Scoring.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            #region gera todas as possiveis combinacoes de respostas superfit
            var idsSuperfit = new int[] { 15, 16, 17, 18, 19, 20 };
            var posiveisRespostas = new List<int[]>(120);
            foreach (var posicao1 in idsSuperfit)
                foreach (var posicao2 in idsSuperfit)
                    foreach (var ultima in idsSuperfit)
                    {
                        if (posicao1 == posicao2 || posicao1 == ultima || posicao2 == ultima)
                            continue;

                        posiveisRespostas.Add(new int[] { posicao1, posicao2, 0, 0, 0, ultima });

                    }
            #endregion

            var db = new ApplicationDbContext();

            db.Configuration.AutoDetectChangesEnabled = false;

            var allUsers = db.Usuarios.Where(x => x.executions.Any()).Include(x => x.executions).ToList();

            var allScoresList = new List<double>(60000);
            
            #region seis respostas superfit from db to arrays
            foreach (var user in allUsers)
            {
                var userExecs = user.executions.ToList();

                //if (execucoes.Any() == false) continue;

                var execucoesComCompetencia = userExecs.Where(x => x.execution_competence_evaluations.Any()).ToList();

                var execucoesComRespostas = userExecs.Where(x => x.execution_answers.Any()).ToList();

                if (execucoesComCompetencia.Any() == false)
                    continue;

                var lastExec = execucoesComCompetencia.Count > 1 ?
                    execucoesComCompetencia.OrderByDescending(x => x.id).First() :
                    execucoesComCompetencia.First();

                var caracteristicas = lastExec.execution_competence_evaluations.Where(x => idsSuperfit.Contains(x.test_competences.id)).ToList();

                if (caracteristicas.Count != 6)
                    continue;

                user.arrayCaract = caracteristicas.OrderByDescending(x => x.score).Select(x => x.test_competence_id).ToArray();
                user.scoresTeste = caracteristicas.Select(x => Convert.ToDouble(x.score)).ToArray();

                allScoresList.AddRange(user.scoresTeste);

                //externals.Add(user.id.ToString() + ";" + exec.external.ToString());

                ////as respostas para fazer o match style parece que estão nesta colecao "execution answers"
                //var extraScores = lastExec.execution_answers.Select(x => x.extra_score.ToString("F2")).ToList();
                //var questionsIds = lastExec.execution_answers.Select(x => x.test_section_question_id.ToString()).ToList();
                //var optionsIds = lastExec.execution_answers.Select(x => x.test_section_question_option_id.ToString()).ToList();
                //var options = lastExec.execution_answers.Select(x => x.test_section_question_options.score).ToList();

                continue;

            } 
            #endregion

            #region calculo da media e desvpad
            double average = allScoresList.Average();
            double sum = allScoresList.Sum(x => Math.Pow(x - average, 2));
            double desvPad = Math.Sqrt((sum) / (allScoresList.Count - 1)); 
            #endregion

            var candidatos = new List<application_users>();
            var empresas = new List<application_users>();

            var distNormalPadrao = Normal.WithMeanStdDev(0.0, 1.0);
            var distNormal = Normal.WithMeanStdDev(average, desvPad);

            #region separa candidatos de empresas (calculando os z scores)
            foreach (var usr in allUsers.Where(x => x.arrayCaract != null).ToList())
            {
                #region calculo do z score
                usr.zScores = new double[usr.scoresTeste.Length];
                for (int i = 0; i < usr.scoresTeste.Length; i++)
                {
                    usr.zScores[i] = distNormal.CumulativeDistribution(usr.scoresTeste[i]);
                    usr.zScores[i] = distNormalPadrao.InverseCumulativeDistribution(usr.zScores[i]);

                    //cand.zScores[i] = Normal.CDF(avg, desvPad, cand.scoresTeste[i]);
                    //cand.zScores[i] = Normal.InvCDF(0.0, 1.0, cand.scoresTeste[i]);
                } 
                #endregion

                #region separacao
                if (usr.application_id >= 5 && usr.application_id != 10)
                    empresas.Add(usr);
                else
                    candidatos.Add(usr); 
                #endregion
            } 
            #endregion


            #region calcula os matches (em dist) entre candidatos e empresas
            StreamWriter swDist = new StreamWriter(HostingEnvironment.ApplicationPhysicalPath + "distanciasReais.txt", false);
            foreach (var cand in candidatos)
                foreach (var empresa in empresas)
                {
                    var dist = this.calculaDistanciaEuc(cand.zScores, empresa.zScores);

                    swDist.WriteLine(String.Join(";", cand.id, empresa.id, dist.ToString("F2")));
                }
            swDist.Close();
            #endregion

            #region calcula os matches entre candidatos e empresas
            StreamWriter matchReal = new StreamWriter(HostingEnvironment.ApplicationPhysicalPath + "MatchReal.txt", false);
            foreach (var cand in candidatos)
            {
                foreach (var empresa in empresas)
                {
                    int score = this.calculaScore(cand.arrayCaract, empresa.arrayCaract);

                    matchReal.WriteLine(String.Join(";", cand.id, empresa.id, score));
                }
            }
            matchReal.Close();
            #endregion

            #region calcula os matches entre candidatos e possiveis combinacoes
            StreamWriter saida2 = new StreamWriter(HostingEnvironment.ApplicationPhysicalPath + "userReal_empresasUnif.txt", false);
            foreach (var cand in candidatos)
                foreach (var possivel in posiveisRespostas)
                {
                    int score = this.calculaScore(cand.arrayCaract, possivel);

                    if (score < 30)
                        continue;

                    saida2.WriteLine(String.Join(";", cand.id, String.Join(";", possivel), score));
                }
            saida2.Close();
            #endregion

            #region calcula todos matches possiveis
            StreamWriter matchUnif = new StreamWriter(HostingEnvironment.ApplicationPhysicalPath + "MatchUniforme.txt", false);
            foreach (var possivel1 in posiveisRespostas)
                foreach (var possivel2 in posiveisRespostas)
                {
                    int score = this.calculaScore(possivel1, possivel2);

                    matchUnif.WriteLine(String.Join(";", String.Join(";", possivel1), String.Join(";", possivel2), score));
                }
            matchUnif.Close();
            #endregion

            #region calcula todos matches entre possiveis respostas e empresas
            //StreamWriter saida4 = new StreamWriter(HostingEnvironment.ApplicationPhysicalPath + "userUnif_empresasReal.txt", false);
            //foreach (var cand in posiveisRespostas)
            //    foreach (var empresa in empresas)
            //    {
            //        int score = this.calculaScore(cand, empresa.arrayCaract);

            //        saida4.WriteLine(String.Join(";", String.Join(";", cand), empresa.id, score));
            //    }
            //saida4.Close();
            #endregion

            

            return View();
        }

        private int calculaScore (int[] arrayCand, int[] arrayEmpresa)
        {
            int score = 0;
            int min = 8;    //minima concordancia/discordancia
            int max = 10;   //maxima concordancia/discordancia

            //regras 1 e 2
            if (arrayCand[0] == arrayEmpresa[0])
                score += max;
            else
                score += 0;

            //regras 3 e 4
            if (arrayCand[1] == arrayEmpresa[1])
                score += max;
            else
                score += 0;

            //regras 5 e 6
            if (arrayCand[5] == arrayEmpresa[5])
                score += max;
            else
                score += 0;

            //regra 7
            if (arrayCand[0] == arrayEmpresa[1])
                score += min;

            //regra 8
            if (arrayCand[1] == arrayEmpresa[0])
                score += min;

            //regra 9
            if (arrayCand[5] == arrayEmpresa[0])
                score -= max;

            //regra 10
            if (arrayCand[5] == arrayEmpresa[1])
                score -= min;

            //regra 11
            if (arrayCand[0] == arrayEmpresa[5])
                score -= max;

            //regra 12
            if (arrayCand[1] == arrayEmpresa[5])
                score -= min;

            return score;

        }

        private double calculaDistanciaEuc(double[] scoresCand, double[] scoresEmpresa)
        {
            double potencia = 2.0;
            double indice = 1 / potencia;
            double score = 0;


            for (int i = 0; i < scoresCand.Length; i++)
            {
                score += Math.Pow(scoresCand[i] - scoresEmpresa[i], potencia);
                //score += Math.Abs(scoresCand[i] - scoresEmpresa[i]);
            }

            score = Math.Pow(score, indice);

            return score;

        }

        private double Phi(double x)
        {
            // constants
            double a1 = 0.254829592;
            double a2 = -0.284496736;
            double a3 = 1.421413741;
            double a4 = -1.453152027;
            double a5 = 1.061405429;
            double p = 0.3275911;

            // Save the sign of x
            int sign = 1;
            if (x < 0)
                sign = -1;
            x = Math.Abs(x) / Math.Sqrt(2.0);

            // A&S formula 7.1.26
            double t = 1.0 / (1.0 + p * x);
            double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);

            return 0.5 * (1.0 + sign * y);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
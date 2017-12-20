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
            
            #region seis respostas superfit from db to arrays
            foreach (var user in allUsers)
            {
                var exec = user.executions.Count > 1 ? 
                    user.executions.OrderByDescending(x => x.id).First() : 
                    user.executions.First();

                var caracteristicas = exec.execution_competence_evaluations.Where(x => idsSuperfit.Contains(x.test_competences.id)).ToList();

                if (caracteristicas.Count != 6)
                    continue;

                user.arrayCaract = caracteristicas.OrderByDescending(x => x.score).Select(x => x.test_competence_id).ToArray();
                user.scoresTeste = caracteristicas.Select(x => Convert.ToDouble(x.score)).ToArray();

                //externals.Add(user.id.ToString() + ";" + exec.external.ToString());

                continue;

            } 
            #endregion

            var candidatos = new List<application_users>();
            var empresas = new List<application_users>();

            #region separa candidatos de empresas
            foreach (var usr in allUsers.Where(x => x.arrayCaract != null).ToList())
            {
                if (usr.application_id >= 5 && usr.application_id != 10)
                    empresas.Add(usr);
                else
                    candidatos.Add(usr);
            } 
            #endregion

            #region calcula os matches (em dist) entre candidatos e empresas
            StreamWriter swDist = new StreamWriter(HostingEnvironment.ApplicationPhysicalPath + "distanciasReais.txt", false);
            foreach (var cand in candidatos)
                foreach (var empresa in empresas)
                {
                    var dist = this.calculaDistanciaEuc(cand.scoresTeste, empresa.scoresTeste);

                    swDist.WriteLine(String.Join(";", cand.id, empresa.id, dist.ToString("F2")));
                }
            swDist.Close();
            #endregion

            #region calcula os matches entre candidatos e empresas
            StreamWriter matchReal = new StreamWriter(HostingEnvironment.ApplicationPhysicalPath + "MatchReal.txt", false);
            foreach (var cand in candidatos)
                foreach (var empresa in empresas)
                {
                    int score = this.calculaScore(cand.arrayCaract, empresa.arrayCaract);

                    matchReal.WriteLine(String.Join(";", cand.id, empresa.id, score));
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

        public int calculaScore (int[] arrayCand, int[] arrayEmpresa)
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

        public double calculaDistanciaEuc(double[] scoresCand, double[] scoresEmpresa)
        {
            double score = 0;

            for (int i = 0; i < scoresCand.Length; i++)
            {
                score += Math.Pow(scoresCand[i] - scoresEmpresa[i], 2);
            }

            score = Math.Pow(score, 0.5);

            return score;

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
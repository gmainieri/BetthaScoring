using Bettha_Scoring.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Bettha_Scoring.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var db = new ApplicationDbContext();

            var allUsers = db.Usuarios.Where(x => x.executions.Any()).ToList();
            

            foreach(var user in allUsers)
            {
                var exec = user.executions.OrderBy(x => x.id).Last();

                var caracteristicas = exec.execution_competence_evaluations.Where(x => x.test_competences.key.Contains("superfit")).ToList();

                if (caracteristicas.Count != 6)
                    continue;

                user.arrayCaract = caracteristicas.OrderBy(x => x.score).Select(x => x.test_competence_id).ToArray();

                //externals.Add(user.id.ToString() + ";" + exec.external.ToString());

                continue;

            }

            var candidatos = new List<application_users>();
            var empresas = new List<application_users>();
            var linha = new List<string>(50000);


            foreach (var usr in allUsers.Where(x => x.arrayCaract != null).ToList())
            {
                if (usr.application_id >= 5 && usr.application_id != 10)
                    empresas.Add(usr);
                else
                    candidatos.Add(usr);
            }

            foreach(var cand in candidatos)
                foreach(var empresa in empresas)
                {
                    int score = 0;

                    //regras 1 e 2
                    if (cand.arrayCaract[0] == empresa.arrayCaract[0])
                        score += 10;

                    //regras 3 e 4
                    if (cand.arrayCaract[1] == empresa.arrayCaract[1])
                        score += 10;

                    //regras 5 e 6
                    if (cand.arrayCaract[2] == empresa.arrayCaract[2])
                        score += 10;

                    //regra 7
                    if (cand.arrayCaract[0] == empresa.arrayCaract[1])
                        score += 8;

                    //regra 8
                    if (cand.arrayCaract[1] == empresa.arrayCaract[0])
                        score += 8;

                    //regra 9
                    if (cand.arrayCaract[5] == empresa.arrayCaract[0])
                        score -= 10;

                    //regra 10
                    if (cand.arrayCaract[5] == empresa.arrayCaract[1])
                        score -= 8;

                    //regra 11
                    if (cand.arrayCaract[0] == empresa.arrayCaract[5])
                        score -= 10;

                    //regra 12
                    if (cand.arrayCaract[1] == empresa.arrayCaract[5])
                        score -= 8;

                    linha.Add(String.Join(";", cand.id, empresa.id, score));

                }

            var merged = String.Join("\n", linha);

            return View();
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
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

            var teste = db.Tests.Where(x => x.name != null).ToList();

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
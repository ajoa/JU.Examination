using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JU.Examination.Controllers
{
    public class HomeController : Controller
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
           return RedirectToAction("Info");
        }

        public ActionResult Info()
        {
            return View();
        }

    }
}
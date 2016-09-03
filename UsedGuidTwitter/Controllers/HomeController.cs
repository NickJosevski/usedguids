using System;
using System.Web.Mvc;

namespace UsedGuidTwitter.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Guid = Guid.NewGuid();

            return View();
        }
    }
}

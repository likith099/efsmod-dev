using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace aspnet_get_started.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
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

        public ActionResult Diagnostics()
        {
            return View();
        }

        public ActionResult FamilyPortal()
        {
            // Set sample data - in real implementation, this would come from database/services
            ViewBag.UserEmail = "user@example.com"; // Get from authentication
            ViewBag.HouseholdId = "0002615634";
            ViewBag.Parents = ""; // Load from database
            ViewBag.OtherMembers = ""; // Load from database
            ViewBag.ChildrenNeedingCare = ""; // Load from database
            ViewBag.HouseholdSize = 0; // Calculate from database

            return View();
        }
    }
}
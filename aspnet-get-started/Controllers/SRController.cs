using System.Web.Mvc;

namespace aspnet_get_started.Controllers
{
    public class SRController : Controller
    {
        public ActionResult Start()
        {
            ViewBag.Title = "Service Request - Start";
            return View();
        }
    }
}

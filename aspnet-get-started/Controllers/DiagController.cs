using System;
using System.Reflection;
using System.Web.Mvc;

namespace aspnet_get_started.Controllers
{
    public class DiagController : Controller
    {
        [HttpGet]
        public ContentResult Health()
        {
            var asm = Assembly.GetExecutingAssembly();
            var name = asm.GetName();
            var now = DateTime.UtcNow.ToString("o");
            return Content($"OK {now} | {name.Name} {name.Version}", "text/plain");
        }

        [HttpGet]
        public ContentResult Env()
        {
            var auth = Environment.GetEnvironmentVariable("WEBSITE_AUTH_ENABLED") ?? "<null>";
            return Content($"WEBSITE_AUTH_ENABLED={auth}", "text/plain");
        }
    }
}

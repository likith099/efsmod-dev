using System.Web.Mvc;

namespace aspnet_get_started.Controllers
{
    public class SRController : Controller
    {
        /// <summary>
        /// Check if user is authenticated via Azure App Service Easy Auth or localhost session
        /// </summary>
        private bool IsUserAuthenticated()
        {
            // Check standard ASP.NET authentication
            if (User.Identity.IsAuthenticated)
                return true;
                
            // Check localhost development session
            if (Request.Url.Host.Contains("localhost") || Request.Url.Host == "127.0.0.1")
            {
                return Session["IsAuthenticated"] != null && (bool)Session["IsAuthenticated"];
            }
                
            // Check Azure App Service Easy Auth headers
            var clientPrincipal = Request.Headers["X-MS-CLIENT-PRINCIPAL"];
            return !string.IsNullOrEmpty(clientPrincipal);
        }
        
        /// <summary>
        /// Get authenticated user name from various sources
        /// </summary>
        private string GetUserName()
        {
            if (User.Identity.IsAuthenticated)
                return User.Identity.Name;
                
            // Check localhost development session
            if (Request.Url.Host.Contains("localhost") || Request.Url.Host == "127.0.0.1")
            {
                return Session["UserName"]?.ToString() ?? "Development User";
            }
                
            // Try Azure App Service headers
            return Request.Headers["X-MS-CLIENT-PRINCIPAL-NAME"] ?? 
                   Request.Headers["X-MS-CLIENT-PRINCIPAL-ID"] ?? 
                   "User";
        }
        public ActionResult Start()
        {
            ViewBag.Title = "Service Request - Start";
            
            // Pass authentication status to the view for proper navigation display
            ViewBag.IsAuthenticated = IsUserAuthenticated();
            ViewBag.UserName = GetUserName();
            
            return View();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace aspnet_get_started.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// Check if user is authenticated via Azure App Service Easy Auth
        /// </summary>
        private bool IsUserAuthenticated()
        {
            // Check standard ASP.NET authentication
            if (User.Identity.IsAuthenticated)
                return true;
                
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
                
            // Try Azure App Service headers
            return Request.Headers["X-MS-CLIENT-PRINCIPAL-NAME"] ?? 
                   Request.Headers["X-MS-CLIENT-PRINCIPAL-ID"] ?? 
                   "User";
        }
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
            // Check if user is authenticated
            if (!IsUserAuthenticated())
            {
                // User is not authenticated, redirect to login
                var returnUrl = Request.Url.ToString();
                return RedirectToAction("Login", new { returnUrl = returnUrl });
            }
            
            // Set authentication status and user data
            ViewBag.IsAuthenticated = IsUserAuthenticated();
            ViewBag.UserName = GetUserName();
            ViewBag.UserEmail = GetUserName(); // In a real app, you'd get email separately
            
            // Set sample household data - in real implementation, this would come from database/services
            ViewBag.HouseholdId = "0002615634";
            ViewBag.Parents = ""; // Load from database
            ViewBag.OtherMembers = ""; // Load from database
            ViewBag.ChildrenNeedingCare = ""; // Load from database
            ViewBag.HouseholdSize = 0; // Calculate from database
            
            // Sample counts for stats cards
            ViewBag.ParentCount = 0;
            ViewBag.ChildrenCount = 0;
            ViewBag.OtherMembersCount = 0;

            return View();
        }

        public ActionResult Login()
        {
            // Get the return URL from query string or default to FamilyPortal
            var returnUrl = Request.QueryString["returnUrl"] ?? Url.Action("FamilyPortal", "Home", null, Request.Url.Scheme);
            
            // Redirect to Azure AD login via App Service Easy Auth
            // Using your Azure AD Client ID: 7facd66f-0a8b-4757-823a-61e23d4909e2
            var loginUrl = $"/.auth/login/aad?post_login_redirect_url={Uri.EscapeDataString(returnUrl)}";
            return Redirect(loginUrl);
        }

        public ActionResult Logout()
        {
            // Redirect to Azure AD logout via App Service Easy Auth
            var logoutUrl = "/.auth/logout?post_logout_redirect_url=" + Uri.EscapeDataString(Url.Action("Index", "Home", null, Request.Url.Scheme));
            return Redirect(logoutUrl);
        }
    }
}
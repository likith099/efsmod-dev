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
        public ActionResult Index()
        {
            // Set authentication status and user data for the layout
            ViewBag.IsAuthenticated = IsUserAuthenticated();
            ViewBag.UserName = GetUserName();
            
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
            ViewBag.Parents = "John Doe, Jane Doe"; // Sample data
            ViewBag.OtherMembers = ""; // Load from database
            ViewBag.ChildrenNeedingCare = "Emma Doe (Age 4), Liam Doe (Age 2)"; // Sample data
            ViewBag.HouseholdSize = 4; // Calculate from database
            
            // Sample counts for stats cards
            ViewBag.ParentCount = 2;
            ViewBag.ChildrenCount = 2;
            ViewBag.OtherMembersCount = 0;

            return View();
        }

        public ActionResult Login()
        {
            // Get the return URL from query string or default to FamilyPortal
            var returnUrl = Request.QueryString["returnUrl"] ?? Url.Action("FamilyPortal", "Home", null, Request.Url.Scheme);
            
            // Check if running on localhost (development)
            if (Request.Url.Host.Contains("localhost") || Request.Url.Host == "127.0.0.1")
            {
                // For localhost development, simulate login by setting session
                Session["IsAuthenticated"] = true;
                Session["UserName"] = "Development User";
                Session["UserEmail"] = "dev@example.com";
                
                // Redirect to the return URL
                return Redirect(returnUrl);
            }
            else
            {
                // Production: Redirect to Azure AD login via App Service Easy Auth
                // Using your Azure AD Client ID: 7facd66f-0a8b-4757-823a-61e23d4909e2
                var loginUrl = $"/.auth/login/aad?post_login_redirect_url={Uri.EscapeDataString(returnUrl)}";
                return Redirect(loginUrl);
            }
        }

        public ActionResult Logout()
        {
            // Check if running on localhost (development)
            if (Request.Url.Host.Contains("localhost") || Request.Url.Host == "127.0.0.1")
            {
                // For localhost development, clear session
                Session.Clear();
                Session.Abandon();
                
                // Redirect to home page
                return RedirectToAction("Index", "Home");
            }
            else
            {
                // Production: Redirect to Azure AD logout via App Service Easy Auth
                var logoutUrl = "/.auth/logout?post_logout_redirect_url=" + Uri.EscapeDataString(Url.Action("Index", "Home", null, Request.Url.Scheme));
                return Redirect(logoutUrl);
            }
        }
    }
}
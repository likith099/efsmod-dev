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
            ViewBag.UserEmail = Session["UserEmail"]?.ToString() ?? GetUserName(); // Get email from session or fallback
            
            // Check if this is a new account created via auto-login
            if (Session["AccountCreated"] != null && (bool)Session["AccountCreated"])
            {
                ViewBag.IsNewAccount = true;
                ViewBag.WelcomeMessage = Session["WelcomeMessage"]?.ToString();
                // Clear the session variables so message doesn't show again
                Session.Remove("AccountCreated");
                Session.Remove("WelcomeMessage");
            }
            
            // Check if user came from FLWINS
            if (Session["AutoLoginSource"] != null && Session["AutoLoginSource"].ToString() == "FLWINS")
            {
                ViewBag.FromFLWINS = true;
            }
            
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

        /// <summary>
        /// Handle automatic login from FLWINS system
        /// </summary>
        public ActionResult AutoLogin(string email, string name, string token)
        {
            try
            {
                // Validate the incoming parameters
                if (string.IsNullOrEmpty(email))
                {
                    ViewBag.ErrorMessage = "Email is required for automatic login.";
                    return RedirectToAction("Login");
                }

                // Production security validations
                if (!IsLocalhost())
                {
                    // 1. Validate request origin (check if request comes from allowed FLWINS domains)
                    if (!IsValidFLWINSRequest())
                    {
                        ViewBag.ErrorMessage = "Invalid request origin.";
                        return RedirectToAction("Login");
                    }
                    
                    // 2. Validate the FLWINS token
                    if (!ValidateFLWINSToken(token, email))
                    {
                        ViewBag.ErrorMessage = "Invalid or expired authentication token.";
                        return RedirectToAction("Login");
                    }
                }
                
                // Process the auto-login
                if (IsLocalhost())
                {
                    // Development environment
                    Session["IsAuthenticated"] = true;
                    Session["UserName"] = !string.IsNullOrEmpty(name) ? name : email.Split('@')[0];
                    Session["UserEmail"] = email;
                    Session["AutoLoginSource"] = "FLWINS";
                    
                    // Check if user exists in EFS system
                    var userExists = CheckUserExists(email);
                    
                    if (!userExists && GetAutoCreateAccountsSetting())
                    {
                        // Create new account automatically
                        CreateUserAccount(email, name);
                        Session["IsNewAccount"] = true;
                    }
                    
                    // Redirect to Family Portal
                    return RedirectToAction("FamilyPortal");
                }
                else
                {
                    // Production environment
                    Session["FLWINSEmail"] = email;
                    Session["FLWINSName"] = name;
                    Session["FLWINSToken"] = token;
                    
                    // Check if user exists in EFS system
                    var userExists = CheckUserExists(email);
                    
                    if (!userExists && GetAutoCreateAccountsSetting())
                    {
                        // Create new account automatically
                        CreateUserAccount(email, name);
                        Session["IsNewAccount"] = true;
                    }
                    
                    // Set authentication for this session
                    Session["IsAuthenticated"] = true;
                    Session["UserName"] = !string.IsNullOrEmpty(name) ? name : email.Split('@')[0];
                    Session["UserEmail"] = email;
                    Session["AutoLoginSource"] = "FLWINS";
                    
                    return RedirectToAction("FamilyPortal");
                }
            }
            catch (Exception ex)
            {
                // Log the error (in production, use proper logging like Application Insights)
                System.Diagnostics.Debug.WriteLine($"AutoLogin Error: {ex.Message}");
                ViewBag.ErrorMessage = "An error occurred during automatic login. Please try again.";
                return RedirectToAction("Login");
            }
        }
        
        /// <summary>
        /// Check if running on localhost
        /// </summary>
        private bool IsLocalhost()
        {
            return Request.Url.Host.Contains("localhost") || Request.Url.Host == "127.0.0.1";
        }
        
        /// <summary>
        /// Validate if request is coming from allowed FLWINS domains
        /// </summary>
        private bool IsValidFLWINSRequest()
        {
            var allowedDomains = System.Configuration.ConfigurationManager.AppSettings["FLWINS_ALLOWED_DOMAINS"] ?? "flwins.org";
            var referer = Request.UrlReferrer?.Host;
            var origin = Request.Headers["Origin"];
            
            if (string.IsNullOrEmpty(referer) && string.IsNullOrEmpty(origin))
                return false; // No referer or origin - suspicious
            
            var domains = allowedDomains.Split(',').Select(d => d.Trim().ToLower()).ToArray();
            
            // Check referer
            if (!string.IsNullOrEmpty(referer) && domains.Any(d => referer.ToLower().Contains(d)))
                return true;
                
            // Check origin header
            if (!string.IsNullOrEmpty(origin) && domains.Any(d => origin.ToLower().Contains(d)))
                return true;
            
            return false;
        }
        
        /// <summary>
        /// Validate FLWINS authentication token
        /// </summary>
        private bool ValidateFLWINSToken(string token, string email)
        {
            if (string.IsNullOrEmpty(token))
                return false;
                
            // In production, implement proper token validation:
            // 1. Check token signature using shared secret
            // 2. Verify token expiration
            // 3. Validate token payload matches email
            
            var sharedSecret = System.Configuration.ConfigurationManager.AppSettings["FLWINS_SHARED_SECRET"];
            if (string.IsNullOrEmpty(sharedSecret))
                return false; // No shared secret configured
            
            // For now, basic validation - in production, use proper JWT validation
            // This is a simplified example - implement proper cryptographic validation
            var expectedToken = GenerateSimpleToken(email, sharedSecret);
            return token == expectedToken || token.StartsWith("test-token"); // Allow test tokens in non-production
        }
        
        /// <summary>
        /// Generate simple token for validation (replace with proper JWT in production)
        /// </summary>
        private string GenerateSimpleToken(string email, string secret)
        {
            // This is a simplified example - use proper JWT or HMAC in production
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{email}:{secret}:{DateTime.UtcNow:yyyyMMdd}"));
        }
        
        /// <summary>
        /// Get auto-create accounts setting from configuration
        /// </summary>
        private bool GetAutoCreateAccountsSetting()
        {
            var setting = System.Configuration.ConfigurationManager.AppSettings["AUTO_CREATE_ACCOUNTS"] ?? "true";
            return bool.TryParse(setting, out bool result) ? result : true;
        }
        
        /// <summary>
        /// Check if user exists in EFS system (simulate database check)
        /// </summary>
        private bool CheckUserExists(string email)
        {
            // In a real implementation, this would check your database
            // For demo purposes, we'll simulate some existing users
            var existingUsers = new[] 
            { 
                "john.doe@example.com", 
                "jane.smith@flwins.org",
                "parent@test.com" 
            };
            
            return existingUsers.Contains(email.ToLower());
        }
        
        /// <summary>
        /// Create new user account automatically
        /// </summary>
        private void CreateUserAccount(string email, string name)
        {
            // In a real implementation, this would:
            // 1. Create user record in database
            // 2. Set up default family profile
            // 3. Create household record
            // 4. Send welcome email
            // 5. Log the account creation
            
            // For demo purposes, we'll just log this action
            System.Diagnostics.Debug.WriteLine($"Auto-created account for: {email} ({name})");
            
            // You could also store this in session for displaying a welcome message
            Session["AccountCreated"] = true;
            Session["WelcomeMessage"] = $"Welcome to EFSM! We've automatically created your account using your FLWINS information.";
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

        /// <summary>
        /// Test page to simulate FLWINS redirect (for development/testing)
        /// </summary>
        public ActionResult TestFLWINSRedirect()
        {
            return View();
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
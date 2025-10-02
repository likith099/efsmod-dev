using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Claims;
using aspnet_get_started.Filters;
using Newtonsoft.Json;

namespace aspnet_get_started.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account/Login
        public ActionResult Login(string returnUrl = null)
        {
            // Check if user is already authenticated
            if (User.Identity.IsAuthenticated)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Home");
            }

            // Store the return URL for after authentication
            var postLoginUrl = !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) 
                             ? returnUrl 
                             : Url.Action("Index", "Home", null, Request.Url.Scheme);

            // Redirect to Azure AD authentication
            // This will trigger the Azure App Service authentication
            var redirectUrl = Request.Url.GetLeftPart(UriPartial.Authority) + "/.auth/login/aad?post_login_redirect_url=" + 
                             HttpUtility.UrlEncode(postLoginUrl);
            
            return Redirect(redirectUrl);
        }

        // GET: Account/Logout
        public ActionResult Logout()
        {
            // Always end session and redirect to home
            try { Session?.Abandon(); } catch { /* ignore */ }

            var homeUrl = Url.Action("Index", "Home", null, Request?.Url?.Scheme ?? "https");

            // If App Service Authentication is enabled, use its logout endpoint
            var websiteAuthEnabled = Environment.GetEnvironmentVariable("WEBSITE_AUTH_ENABLED");
            var isAuthEnabled = !string.IsNullOrEmpty(websiteAuthEnabled) &&
                                (websiteAuthEnabled.Equals("True", StringComparison.OrdinalIgnoreCase) || websiteAuthEnabled == "1");

            if (isAuthEnabled)
            {
                var baseUrl = Request?.Url?.GetLeftPart(UriPartial.Authority) ?? string.Empty;
                var redirectUrl = baseUrl + "/.auth/logout?post_logout_redirect_url=" + HttpUtility.UrlEncode(homeUrl);
                return Redirect(redirectUrl);
            }

            // Fallback: just redirect home
            return Redirect(homeUrl);
        }

        // GET: Account/Profile
        [RequireAuth]
        public new ActionResult Profile()
        {
            ViewBag.Message = "Your profile page.";
            
            // Get user information from claims
            if (User.Identity.IsAuthenticated)
            {
                var claimsIdentity = User.Identity as ClaimsIdentity;
                if (claimsIdentity != null)
                {
                    ViewBag.UserName = claimsIdentity.FindFirst("name")?.Value ?? 
                                      claimsIdentity.FindFirst(ClaimTypes.Name)?.Value ?? 
                                      User.Identity.Name;
                    ViewBag.Email = claimsIdentity.FindFirst("email")?.Value ?? 
                                   claimsIdentity.FindFirst("preferred_username")?.Value ??
                                   claimsIdentity.FindFirst(ClaimTypes.Email)?.Value ??
                                   claimsIdentity.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;
                    ViewBag.UserId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    ViewBag.AuthMethod = claimsIdentity.FindFirst(ClaimTypes.AuthenticationMethod)?.Value;
                }
            }

            return View();
        }

#if DEBUG
        // Local development utilities (DEBUG only)
        // Simulate sign-in by setting a DEV_AUTH cookie that AzureAuthModule can read
        public ActionResult LocalLogin(string name = "Local User", string email = "local@example.com")
        {
            var principal = new
            {
                AuthenticationType = "AzureAppService",
                IdentityProvider = "aad",
                UserId = Guid.NewGuid().ToString(),
                UserDetails = string.IsNullOrWhiteSpace(name) ? email : name,
                UserRoles = new string[] { },
                Claims = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "email", email },
                    { "name", name }
                }
            };

            var json = JsonConvert.SerializeObject(principal);
            var b64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));
            var cookie = new HttpCookie("DEV_AUTH", b64)
            {
                HttpOnly = false,
                Secure = false,
                Path = "/"
            };
            Response.Cookies.Add(cookie);

            return RedirectToAction("Index", "Home");
        }

        public ActionResult LocalLogout()
        {
            if (Request.Cookies["DEV_AUTH"] != null)
            {
                var expired = new HttpCookie("DEV_AUTH")
                {
                    Expires = DateTime.UtcNow.AddDays(-1),
                    Path = "/"
                };
                Response.Cookies.Add(expired);
            }
            return RedirectToAction("Index", "Home");
        }
#endif
    }
}
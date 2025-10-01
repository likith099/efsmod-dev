using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Claims;
using aspnet_get_started.Filters;

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
            // Clear any local authentication
            if (User.Identity.IsAuthenticated)
            {
                // Redirect to Azure AD logout
                var redirectUrl = Request.Url.GetLeftPart(UriPartial.Authority) + "/.auth/logout?post_logout_redirect_url=" + 
                                 HttpUtility.UrlEncode(Url.Action("Index", "Home", null, Request.Url.Scheme));
                
                return Redirect(redirectUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Profile
        [RequireAuth]
        public ActionResult Profile()
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
    }
}
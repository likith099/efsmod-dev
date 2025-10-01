using System;
using System.Web;
using System.Web.Mvc;

namespace aspnet_get_started.Filters
{
    public class OptionalAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            // Always return true - we handle authorization in the action
            return true;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            // Don't do anything - let the action handle it
        }
    }

    public class RequireAuthAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                var returnUrl = filterContext.HttpContext.Request.RawUrl;
                var loginUrl = new UrlHelper(filterContext.RequestContext).Action("Login", "Account", new { returnUrl = returnUrl });
                filterContext.Result = new RedirectResult(loginUrl);
            }
            
            base.OnActionExecuting(filterContext);
        }
    }
}
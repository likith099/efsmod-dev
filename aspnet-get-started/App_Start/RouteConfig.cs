using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace aspnet_get_started
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Custom route for OIDC signin callback
            routes.MapRoute(
                name: "SigninOidc",
                url: "signin-oidc",
                defaults: new { controller = "Home", action = "SigninOidc" }
            );

            // Test route to verify signin-oidc routing
            routes.MapRoute(
                name: "TestSigninRoute",
                url: "test-signin-route",
                defaults: new { controller = "Home", action = "TestSigninRoute" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}

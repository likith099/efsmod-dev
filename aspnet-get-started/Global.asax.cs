using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace aspnet_get_started
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_Error()
        {
            var ex = Server.GetLastError();
            try
            {
                var appData = Server.MapPath("~/App_Data");
                if (!System.IO.Directory.Exists(appData))
                {
                    System.IO.Directory.CreateDirectory(appData);
                }
                var logPath = System.IO.Path.Combine(appData, "errors.log");
                var msg = string.Format("[{0:u}] {1}\r\n{2}\r\n\r\n", DateTime.UtcNow, ex?.Message, ex?.ToString());
                System.IO.File.AppendAllText(logPath, msg);
            }
            catch { /* ignore logging failures */ }
        }
    }
}

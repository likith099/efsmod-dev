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
            try { AreaRegistration.RegisterAllAreas(); }
            catch (System.Exception ex) { LogStartupError("RegisterAllAreas", ex); }

            try { FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters); }
            catch (System.Exception ex) { LogStartupError("RegisterGlobalFilters", ex); }

            try { RouteConfig.RegisterRoutes(RouteTable.Routes); }
            catch (System.Exception ex) { LogStartupError("RegisterRoutes", ex); }

            try { BundleConfig.RegisterBundles(BundleTable.Bundles); }
            catch (System.Exception ex) { LogStartupError("RegisterBundles", ex); }
        }

        private void LogStartupError(string stage, System.Exception ex)
        {
            try
            {
                var appData = Server.MapPath("~/App_Data");
                if (!System.IO.Directory.Exists(appData))
                {
                    System.IO.Directory.CreateDirectory(appData);
                }
                var logPath = System.IO.Path.Combine(appData, "errors.log");
                var msg = string.Format("[Startup:{0}] [{1:u}] {2}\r\n{3}\r\n\r\n", stage, System.DateTime.UtcNow, ex?.Message, ex?.ToString());
                System.IO.File.AppendAllText(logPath, msg);
            }
            catch { /* ignore logging failures */ }
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

#if DEBUG
            // In Debug, show a simple text response for quick diagnosis (for root requests)
            try
            {
                Response.Clear();
                Response.ContentType = "text/plain";
                Response.StatusCode = 500;
                Response.Write("Application Error:\r\n\r\n" + ex);
                Response.End();
            }
            catch { /* ignore */ }
#endif
        }
    }
}

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Web;
using Newtonsoft.Json;

namespace aspnet_get_started.Modules
{
    public class AzureAuthModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.PostAuthenticateRequest += Context_PostAuthenticateRequest;
        }

        private void Context_PostAuthenticateRequest(object sender, EventArgs e)
        {
            var app = sender as HttpApplication;
            if (app?.Context == null) return;

            var httpContext = app.Context;
            try
            {
                // Skip diagnostics and static resources to simplify debugging
                var path = httpContext.Request?.Url?.AbsolutePath ?? string.Empty;
                var lower = path.ToLowerInvariant();
                if (lower.StartsWith("/diag/") || lower.Contains("/content/") || lower.Contains("/scripts/") || lower.EndsWith(".ico") || lower.Contains(".axd"))
                {
                    return;
                }
            var principalHeader = httpContext.Request.Headers["X-MS-CLIENT-PRINCIPAL"];
            string principalJson = null;

            if (!string.IsNullOrEmpty(principalHeader))
            {
                try
                {
                    var principalBytes = Convert.FromBase64String(principalHeader);
                    principalJson = System.Text.Encoding.UTF8.GetString(principalBytes);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"AzureAuthModule header decode error: {ex.Message}");
                }
            }

            // Local development fallback via cookie
            if (principalJson == null)
            {
                var devCookie = httpContext.Request.Cookies["DEV_AUTH"];
                if (devCookie != null && !string.IsNullOrEmpty(devCookie.Value))
                {
                    try
                    {
                        var b64 = devCookie.Value;
                        var jsonBytes = Convert.FromBase64String(b64);
                        principalJson = System.Text.Encoding.UTF8.GetString(jsonBytes);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"AzureAuthModule dev cookie decode error: {ex.Message}");
                    }
                }
            }

                if (!string.IsNullOrEmpty(principalJson))
                {
                    try
                    {
                        var principal = JsonConvert.DeserializeObject<ClientPrincipal>(principalJson);

                    if (principal != null && !string.IsNullOrEmpty(principal.UserId))
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, principal.UserId),
                            new Claim(ClaimTypes.Name, principal.UserDetails ?? principal.UserId),
                            new Claim(ClaimTypes.AuthenticationMethod, principal.IdentityProvider ?? "aad")
                        };

                        if (principal.UserRoles != null)
                        {
                            foreach (var role in principal.UserRoles)
                            {
                                claims.Add(new Claim(ClaimTypes.Role, role));
                            }
                        }

                        if (principal.Claims != null)
                        {
                            foreach (var claim in principal.Claims)
                            {
                                claims.Add(new Claim(claim.Key, claim.Value?.ToString() ?? string.Empty));
                            }
                        }

                        var identity = new ClaimsIdentity(claims, "AzureAppService");
                        httpContext.User = new ClaimsPrincipal(identity);
                    }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"AzureAuthModule error: {ex.Message}");
                        LogModuleError("Deserialize/SetPrincipal", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AzureAuthModule outer error: {ex.Message}");
                LogModuleError("Outer", ex);
            }
        }

        private void LogModuleError(string stage, Exception ex)
        {
            try
            {
                var appData = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data");
                if (!System.IO.Directory.Exists(appData))
                {
                    System.IO.Directory.CreateDirectory(appData);
                }
                var logPath = System.IO.Path.Combine(appData, "errors.log");
                var msg = string.Format("[Module:{0}] [{1:u}] {2}\r\n{3}\r\n\r\n", stage, DateTime.UtcNow, ex?.Message, ex?.ToString());
                System.IO.File.AppendAllText(logPath, msg);
            }
            catch { /* ignore logging failures */ }
        }

        public void Dispose() { }
    }

    public class ClientPrincipal
    {
        public string AuthenticationType { get; set; }
        public string IdentityProvider { get; set; }
        public string UserId { get; set; }
        public string UserDetails { get; set; }
        public string[] UserRoles { get; set; }
        public Dictionary<string, object> Claims { get; set; }
    }
}

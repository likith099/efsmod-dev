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
            var principalHeader = httpContext.Request.Headers["X-MS-CLIENT-PRINCIPAL"];

            if (!string.IsNullOrEmpty(principalHeader))
            {
                try
                {
                    var principalBytes = Convert.FromBase64String(principalHeader);
                    var principalJson = System.Text.Encoding.UTF8.GetString(principalBytes);
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
                }
            }
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

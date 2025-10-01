using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace aspnet_get_started.Controllers
{
    public class ProvisionController : Controller
    {
        public class ProvisionRequest
        {
            public string email { get; set; }
            public string displayName { get; set; }
            public string redirectPath { get; set; } // e.g., "/SR/Start"
        }

        public class ProvisionResponse
        {
            public string status { get; set; }
            public string ssoUrl { get; set; }
            public string message { get; set; }
            public object graphResult { get; set; }
        }

        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> Create()
        {
            string body;
            using (var reader = new System.IO.StreamReader(Request.InputStream))
            {
                body = await reader.ReadToEndAsync();
            }

            ProvisionRequest model = null;
            try
            {
                model = JsonConvert.DeserializeObject<ProvisionRequest>(body);
            }
            catch (Exception)
            {
                return Json(new ProvisionResponse { status = "error", message = "Invalid JSON payload" });
            }

            if (model == null || string.IsNullOrWhiteSpace(model.email))
            {
                return Json(new ProvisionResponse { status = "error", message = "'email' is required" });
            }

            var baseUrl = Request?.Url?.GetLeftPart(UriPartial.Authority)?.TrimEnd('/') ?? string.Empty;
            var redirectPath = string.IsNullOrWhiteSpace(model.redirectPath) ? "/SR/Start" : model.redirectPath;
            var ssoUrl = baseUrl + "/.auth/login/aad?post_login_redirect_url=" +
                        Uri.EscapeDataString(baseUrl + redirectPath);

            object graphResult = null;
            string graphStatus = "skipped";

            try
            {
                var tenantId = Environment.GetEnvironmentVariable("EFSM_GRAPH_TENANT_ID");
                var clientId = Environment.GetEnvironmentVariable("EFSM_GRAPH_CLIENT_ID");
                var clientSecret = Environment.GetEnvironmentVariable("EFSM_GRAPH_CLIENT_SECRET");

                if (!string.IsNullOrWhiteSpace(tenantId) && !string.IsNullOrWhiteSpace(clientId) && !string.IsNullOrWhiteSpace(clientSecret))
                {
                    // Invite as B2B guest to EFSM tenant
                    var token = await GraphHelper.GetAppTokenAsync(tenantId, clientId, clientSecret);
                    var invitePayload = new
                    {
                        invitedUserEmailAddress = model.email,
                        invitedUserDisplayName = string.IsNullOrWhiteSpace(model.displayName) ? model.email : model.displayName,
                        inviteRedirectUrl = ssoUrl,
                        sendInvitationMessage = false
                    };

                    var http = new HttpClient();
                    var req = new HttpRequestMessage(HttpMethod.Post, "https://graph.microsoft.com/v1.0/invitations");
                    req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    req.Content = new StringContent(JsonConvert.SerializeObject(invitePayload), Encoding.UTF8, "application/json");

                    var resp = await http.SendAsync(req);
                    var respText = await resp.Content.ReadAsStringAsync();
                    graphStatus = resp.StatusCode.ToString();
                    try
                    {
                        graphResult = JsonConvert.DeserializeObject(respText);
                    }
                    catch
                    {
                        graphResult = respText;
                    }
                }
            }
            catch (Exception ex)
            {
                graphResult = new { error = ex.Message };
                graphStatus = "error";
            }

            return Json(new ProvisionResponse
            {
                status = "ok",
                ssoUrl = ssoUrl,
                message = graphStatus == "skipped" ? "Provisioning endpoint configured without Graph; returning SSO URL only." : $"Graph invite result: {graphStatus}",
                graphResult = graphResult
            });
        }
    }
}

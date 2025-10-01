using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace aspnet_get_started
{
    public static class GraphHelper
    {
        public static async Task<string> GetAppTokenAsync(string tenantId, string clientId, string clientSecret)
        {
            using (var http = new HttpClient())
            {
                var tokenUrl = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";
                var body = new StringContent($"client_id={Uri.EscapeDataString(clientId)}&scope={Uri.EscapeDataString("https://graph.microsoft.com/.default")}&client_secret={Uri.EscapeDataString(clientSecret)}&grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");
                var resp = await http.PostAsync(tokenUrl, body);
                var txt = await resp.Content.ReadAsStringAsync();
                if (!resp.IsSuccessStatusCode)
                {
                    throw new Exception($"Token request failed: {(int)resp.StatusCode} {resp.ReasonPhrase} - {txt}");
                }
                var json = JObject.Parse(txt);
                return json.Value<string>("access_token");
            }
        }
    }
}

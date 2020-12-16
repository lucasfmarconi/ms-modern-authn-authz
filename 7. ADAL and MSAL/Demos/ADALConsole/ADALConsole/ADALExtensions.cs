using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ADALConsole
{
    public static class ADALExtensions
    {
        /// <summary>
        /// Returns Base64 encoded SAML token using the AAD OBO exchange of a JWT token.
        /// </summary>
        /// <param name="auth"></param>
        /// <param name="jwtToken"></param>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        /// <param name="resourceId"></param>
        /// <param name="replyUrl"></param>
        /// <returns></returns>
        public static async Task<string> AcquireSamlFromJWT(
            this AuthenticationContext auth,
            string jwtToken,
            string clientId,
            string clientSecret,
            string resourceId,
            string replyUrl
            )
        {

            var http = new HttpClient();
            var reqParams = new Dictionary<string, string>
            {
                {"grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer" },
                {"assertion", jwtToken },
                {"client_id" , clientId },
                {"client_secret", HttpUtility.UrlEncode(clientSecret) },
                {"resource", HttpUtility.UrlEncode(resourceId) },
                {"redirect_uri", HttpUtility.UrlEncode(replyUrl) },
                {"requested_token_use", "on_behalf_of" },
                {"requested_token_type", "urn:ietf:params:oauth:token-type:saml2" }
            };
            var body = reqParams.Aggregate("", (s, v1) => $"{s}&{v1.Key}={v1.Value}").Substring(1); // skip initial &
            var resp = await http.PostAsync($"{auth.Authority}oauth2/token", new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded"));
            if (resp.IsSuccessStatusCode)
            {
                var authResult = await resp.Content.ReadAsStringAsync();
                var saml = (string) JObject.Parse(authResult)["access_token"];
                return saml;
            }
            else
                throw new Exception(resp.ReasonPhrase);
        }
    }
}

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using karmaMaintainer.models;
using Newtonsoft.Json;

namespace karmaMaintainer.services
{
    public class OauthAuthenticationService : IAuthenticationService
    {
        private static volatile OauthAuthenticationService instance;
        private static object syncRoot = new Object();
        public TokenInfo Token;
        private static readonly string REDDIT_AUTH_URL = "https://www.reddit.com/api/v1/access_token";
        private static readonly string REDDIT_OAUTH_BASE_URL = "https://oauth.reddit.com";

        private OauthAuthenticationService()
        {
            
        }

        public static IAuthenticationService GetAuthenticationService()
        {
            if (instance == null)
            {
                lock (syncRoot)
                {
                    if (instance == null)
                    {
                        instance = new OauthAuthenticationService();
                    }
                }
            }
            return instance;
        }
        public async Task<TokenInfo> Authenticate(string userName, string password, string appId, string secret)
        {
            try
            {
                var x = await GetToken(userName, password, appId, secret);
                Token = new TokenInfo()
                {
                    token = x.access_token,
                    tokenType = x.token_type,
                    expiresAt = DateTime.Now.AddSeconds(x.expires_in),
                    baseUsageUrl = REDDIT_OAUTH_BASE_URL
                };
                return Token;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new TokenInfo();
            }
        }

        private async Task<AccessRequestResponse> GetToken(string userName, string password, string appId, string secret)
        {
            var url =
                $"{REDDIT_AUTH_URL}?grant_type=password&username={userName}&password={password}";
            var byteArray = Encoding.ASCII.GetBytes($"{appId}:{secret}");
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(("application/json")));
            client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("basic", Convert.ToBase64String(byteArray));
            var resp = await client.PostAsync(url, null);
            if (resp.IsSuccessStatusCode)
            {
                var stringVal = await resp.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<AccessRequestResponse>(stringVal);
            }
            else
            {
                throw new HttpRequestException("Error requesting Authentication token. Check your settings.");
            }
            
        }
    }
}
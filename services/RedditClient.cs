using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.PortableExecutable;
using System.Threading;
using System.Threading.Tasks;
using karmaMaintainer.models;
using Newtonsoft.Json;

namespace karmaMaintainer.services
{
    public class RedditClient : IRedditClient
    {
        private TokenInfo _token;
        private HttpClient _client;
        private DateTime _lastRequest;

        public RedditClient(TokenInfo token)
        {
            _token = token;
            _client = new HttpClient();
            _client.BaseAddress = new Uri(_token.baseUsageUrl);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(("application/json")));
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_token.tokenType, _token.token);
            _client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("karmaMaintainer", "1.0"));
        }

        public async Task<MeResponse> me()
        {
            var resp = await _client.GetAsync($"{_token.baseUsageUrl}/api/v1/me");
            var stringResp = await resp.Content.ReadAsStringAsync();
            if (resp.IsSuccessStatusCode)
            {
                _lastRequest = DateTime.Now;
                return JsonConvert.DeserializeObject<MeResponse>(stringResp);
            }
            else
            {
                throw new HttpRequestException("Error requesting User Info. Check your settings.");   
            }
        }

        public async Task<CommentResponse> comments(string user, string before, string after, int limit)
        {
            var timeSinceLastRequest = DateTime.Now.Subtract(_lastRequest);
            if (timeSinceLastRequest.Seconds < 1)
            {
                Thread.Sleep(0);
            }
            if (limit > 100)
            {
                limit = 100;
            }
            if (limit == 0)
            {
                limit = 50;
            }
            var url = $"/user/{user}/comments?limit={limit}";
            if (!string.IsNullOrEmpty(before))
            {
                url += $"&before={before}";
            }
            if (!string.IsNullOrEmpty(after))
            {
                url += $"&after={after}";
            }
            url += $"&show=all";
            var resp = await _client.GetAsync(url);
            var stringResp = await resp.Content.ReadAsStringAsync();
            _lastRequest = DateTime.Now;
            var comments = generateCommentsResponse(stringResp);
            comments.IsSuccess = resp.IsSuccessStatusCode;
            if (!resp.IsSuccessStatusCode)
            {
                Console.WriteLine(resp.ReasonPhrase);
            }
            var headers = resp.Headers.GetEnumerator();
            while (headers.MoveNext())
            {
                var headerValues = string.Join(",", headers.Current.Value);
            }
            headers.Dispose();
            return comments;
        }

        private CommentResponse generateCommentsResponse(string response)
        {
            var results = JsonConvert.DeserializeObject<dynamic>(response);
            var commentList = new List<Comment>();
            foreach (var comment in results.data.children)
            {
                commentList.Add(new Comment()
                {
                    created = comment.data.created,
                    createdDt = ConvertFromUnixTimestamp((int)comment.data.created_utc),
                    ups = comment.data.ups,
                    downs = comment.data.downs,
                    id = comment.data.id,
                    score = comment.data.score,
                    link_url = comment.data.link_url
                });
            }
            var resp = new CommentResponse
            {
                after = results.data.after,
                before = results.data.before,
                children = commentList.ToArray()
            };

            return resp;
        }

        private DateTime ConvertFromUnixTimestamp(int created)
        {
            var origin = new DateTime(1970,1,1,0,0,0,0, DateTimeKind.Utc);
            return origin.AddSeconds(created);
        }
    }
}
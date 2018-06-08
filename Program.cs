using System;
using System.Threading.Tasks;
using karmaMaintainer.services;

namespace karmaMaintainer
{
    class Program
    {
        private static readonly string REDDIT_USER = Environment.GetEnvironmentVariable("REDDIT_USER");
        private static readonly string REDDIT_PASSWORD = Environment.GetEnvironmentVariable("REDDIT_PASSWORD");
        private static readonly string REDDIT_APP_ID = Environment.GetEnvironmentVariable("REDDIT_APP_ID");
        private static readonly string REDDIT_APP_SECRET = Environment.GetEnvironmentVariable("REDDIT_APP_SECRET");

        
        static async Task Main(string[] args)
        {
            IAuthenticationService authService = OauthAuthenticationService.GetAuthenticationService();
            var token = await authService.Authenticate(REDDIT_USER, REDDIT_PASSWORD, REDDIT_APP_ID, REDDIT_APP_SECRET);
            Console.WriteLine($"Granted token: {token.token.ToString()}\nExpires At: {token.expiresAt.ToShortTimeString()}");
            IRedditClient client = new RedditClient(token);
            var me = await client.me();
            Console.WriteLine($"Begin processing for /u/{me.name}\nHas {me.link_karma} link karma, and {me.comment_karma} comment karma.");
            var comments = await client.comments(REDDIT_USER, null, null, 25);
            Console.WriteLine($"Request Success: {comments.IsSuccess}");
            while (!string.IsNullOrEmpty(comments.after) && comments.children.Length != 0)
            {
                Console.WriteLine($"Processing {comments.children.Length} comments.");
                foreach (var comment in comments.children)
                {
                    if (comment.ups < 0)
                    {
                        Console.WriteLine($"Comment {comment.id}, created at {comment.createdDt} has a score of {comment.score}");
                    }
                }
                comments = await client.comments(REDDIT_USER, null, comments.after, 25);
                Console.WriteLine($"Request Success: {comments.IsSuccess}");
                Console.WriteLine($"After is: {comments.after}");
            }
            
        }
    }
}
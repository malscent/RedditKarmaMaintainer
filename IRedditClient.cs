using System.Threading.Tasks;
using karmaMaintainer.models;

namespace karmaMaintainer
{
    public interface IRedditClient
    {
        Task<MeResponse> me();
        Task<CommentResponse> comments(string user, string before, string after, int limit);
    }
}
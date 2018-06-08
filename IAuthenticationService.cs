using System;
using System.Threading.Tasks;
using karmaMaintainer.models;

namespace karmaMaintainer
{
    public interface IAuthenticationService
    {
        Task<TokenInfo> Authenticate(String userName, String password, String appId, String secret);
    }
}
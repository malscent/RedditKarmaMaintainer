using System;

namespace karmaMaintainer.models
{
    public class TokenInfo
    {
        public String token { get; set; }
        public String tokenType { get; set; }
        public DateTime expiresAt { get; set; }
        public String baseUsageUrl { get; set; }
    }
}
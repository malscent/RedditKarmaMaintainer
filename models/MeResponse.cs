using System;

namespace karmaMaintainer.models
{
    public class MeResponse
    {
        public string name { get; set; }
        public double created { get; set; }
        public double created_utc { get; set; }
        public int comment_karma { get; set; }
        public int link_karma { get; set; }
    }
}
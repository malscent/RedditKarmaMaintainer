using System;

namespace karmaMaintainer.models
{
    public class CommentResponse
    {
        public bool IsSuccess { get; set; }
        public string after { get; set; }
        public string before { get; set; }
        public Comment[] children { get; set; }
    }
}
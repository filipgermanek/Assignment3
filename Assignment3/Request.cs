using System;
namespace Assignment3
{
    public class Request
    {
        public Request()
        {
        }
        public string Method { get; set; }
        public string Path { get; set; }
        public long Date { get; set; }
        public Category Body { get; set; }
    }
}

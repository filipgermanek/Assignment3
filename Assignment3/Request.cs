using System;
namespace Assignment3
{
    public class Request
    {
        public Request()
        {
        }
        public REQUEST_METHOD Method { get; set; }
        public string Path { get; set; }
        public string Date { get; set; }
        public Category Body { get; set; }
    }
}

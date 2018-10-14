using System;
namespace Assignment3
{
    public class Request
    {
        public Request()
        {
        }
        public REQUEST_METHOD? Method { get; set; }
        public String Path { get; set; }
        public long? Date { get; set; }
        public Category Body { get; set; }
    }
}

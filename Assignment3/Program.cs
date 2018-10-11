using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Assignment3
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new TcpListener(IPAddress.Parse("127.0.0.1"), 5000);
            server.Start();
            List<Category> categories = new List<Category>
            {
                new Category { Cid = 1, Name = "Beverages" },
                new Category { Cid = 2, Name = "Condiments" },
                new Category { Cid = 3, Name = "Confections" }
            };
            Console.WriteLine("Server started ...");
            categories.ForEach((Category obj) => Console.WriteLine("category: " + obj.Cid + " " + obj.Name));
            while (true) 
            {

                var client = server.AcceptTcpClient();

                var strm = client.GetStream();

                var buffer = new byte[client.ReceiveBufferSize];
               
                var readCnt = strm.Read(buffer, 0, buffer.Length);

                var msg = Encoding.UTF8.GetString(buffer, 0, readCnt);

                //creates an object which we can validate
                Request request = Newtonsoft.Json.JsonConvert.DeserializeObject<Request>(msg);


                Console.WriteLine($"Request: path: {request.Path} method:{request.Method} body:{request.Body} date:{request.Date}");

                var payload = Encoding.UTF8.GetBytes(msg.ToUpper());

                strm.Write(payload, 0, payload.Length);

                strm.Close();

                client.Close();
            }
        }
        static bool IsRequestValid(Request request) {
            return false;
        }

    }
}

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
        static List<Category> categories = new List<Category>
            {
                new Category { Cid = 1, Name = "Beverages" },
                new Category { Cid = 2, Name = "Condiments" },
                new Category { Cid = 3, Name = "Confections" }
            };
        static void Main(string[] args)
        {
            var server = new TcpListener(IPAddress.Parse("127.0.0.1"), 5000);
            server.Start();

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
                HandleRequest(client, strm, request);
            }
        }
        static void HandleRequest(TcpClient client, NetworkStream strm, Request request) {
           
            if (request.Path != null && request.Path.Length > 5) {
                string path_formatted = request.Path.Substring(5);
                if (request.Method.Equals(REQUEST_METHOD.read))
                {
                    if (path_formatted.Equals("categories"))
                    {
                        //read all categories
                        var response = new
                        {
                            status = "1 Ok",
                            body = categories.ToJson()
                        }.ToJson();
                        var payload = Encoding.UTF8.GetBytes(response);
                        strm.Write(payload, 0, payload.Length);
                        strm.Close();
                        client.Close();
                    }
                    else if (request.Path.Contains("cateogries"))
                    {
                        //read category with provided id
                        //TODO get id and convert to int
                    }
                    return;
                }
                else if (request.Method.Equals(REQUEST_METHOD.update))
                {
                    Console.WriteLine("method is update");
                    return;
                }
                else if (request.Method.Equals(REQUEST_METHOD.create))
                {
                    Console.WriteLine("method is create");
                    return;
                }
                else if (request.Method.Equals(REQUEST_METHOD.delete))
                {
                    Console.WriteLine("method is delete");
                    return;
                } else if (request.Method.Equals(REQUEST_METHOD.echo)) {

                }
                Console.WriteLine("method is invalid");
            }
           //invalid
        }
    }
    public static class Util
    {
        public static string ToJson(this object data)
        {
            return JsonConvert.SerializeObject(data,
            new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
        }
    }
}

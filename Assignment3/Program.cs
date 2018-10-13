using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
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

            if (request.Path != null && request.Path.Length > 5)
            {
                string path_formatted = request.Path.Substring(5);
                Console.WriteLine("path formatted " + path_formatted);
                if (request.Method.Equals(REQUEST_METHOD.read))
                {
                    if (path_formatted.Equals("categories"))
                    {
                        Console.WriteLine("in cat");
                        //read all categories
                        var response = new
                        {
                            status = "1 Ok",
                            body = categories.ToJson()
                        }.ToJson();
                        SendResponse(strm, client, response);
                    }
                    else if (path_formatted.Contains("categories"))
                    {
                        //read category with provided id
                        //TODO get id and convert to int
                        int id = GetIdFromPath(path_formatted);
                        Console.WriteLine("extracted id: " + id);
                        Category requestedCategory = null;
                        foreach (Category category in categories)
                        {
                            if (category.Cid == id)
                            {
                                requestedCategory = category;
                            }
                        }
                        if (requestedCategory != null)
                        {
                            var response = new
                            {
                                status = "1 Ok",
                                body = requestedCategory.ToJson()
                            }.ToJson();
                            SendResponse(strm, client, response);
                        }
                        else
                        {
                            var response = new
                            {
                                status = "5 Not found"
                            }.ToJson();
                            SendResponse(strm, client, response);
                        }
                    }
                }
                else if (request.Method.Equals(REQUEST_METHOD.update))
                {
                    if (path_formatted.Equals("categories")) {
                        var response = new
                        {
                            status = "4 Bad Request"
                        }.ToJson();
                        SendResponse(strm, client, response);
                    } else {
                        int id = GetIdFromPath(path_formatted);
                        Category category = categories.First(cat => cat.Cid == id);
                        //string result = myList.Single(s => s == search);
                        if (path_formatted.Contains("categories") && category != null)
                        {
                            categories.Remove(category);
                            category.Cid = request.Body.Cid;
                            category.Name = request.Body.Name;
                            categories.Add(category);
                            categories.ForEach((Category obj) => Console.WriteLine("category: " + obj.Cid + " " + obj.Name));
                            var response = new
                            {
                                status = "3 Updated",
                                body = category.ToJson()
                            }.ToJson();
                            SendResponse(strm, client, response);
                        }
                        else
                        {
                            var response = new
                            {
                                status = "5 Not found"
                            }.ToJson();
                            SendResponse(strm, client, response);
                        }
                    }
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
                }
                else if (request.Method.Equals(REQUEST_METHOD.echo))
                {
                    return;
                }
                else {
                    Console.WriteLine("method is invalid");
                }
            }
           //invalid
        }

        static int GetIdFromPath(string path) {
            string resultString = Regex.Match(path, @"\d+").Value;
            int id = Int32.Parse(resultString);
            return id;
        }

        static void SendResponse(NetworkStream strm, TcpClient client, string response) {
            var payload = Encoding.UTF8.GetBytes(response);
            strm.Write(payload, 0, payload.Length);
            strm.Close();
            client.Close();
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

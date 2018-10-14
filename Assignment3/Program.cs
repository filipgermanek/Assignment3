using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
        protected static void Main(string[] args)
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
                try {
                    Request request = Newtonsoft.Json.JsonConvert.DeserializeObject<Request>(msg);
                    HandleRequest(client, strm, request);
                } catch(Exception e) {
                    Console.WriteLine("exception: " + e.Message);
                    string status = "";
                    if (e.Message.Contains("REQUEST_METHOD")) {
                        //error parsing method to enum means method in request has illegal value
                        status = "Illegal method";
                    } else if (e.Message.Contains("Assignment3.Category")) {
                        //error parsing body to Category class. Means body has illegral format
                        status = "Illegal body";
                    } else if (e.Message.Contains("date")) {
                        status = "Illegal date";
                    }
                    var response = new
                    {
                        status
                    }.ToJson();
                    SendResponse(strm, client, response);
                }
            }
        }

        static void HandleRequest(TcpClient client, NetworkStream strm, Request request) {
            string status = "";
            if (request.Method == null) {
                status = "Missing method ";
            }
            if (request.Date == null) {
                status += "Missing date";
            }
            if (status != "")
            {
                var response = new
                {
                    status
                }.ToJson();
                SendResponse(strm, client, response);
            }
            else if (request.Path != null && request.Path.Length > 5 && request.Path.Contains("api/categories"))
            {
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
                        SendResponse(strm, client, response);
                    }
                    else if (path_formatted.Contains("categories"))
                    {
                        int? id = GetIdFromPath(path_formatted);
                        if (id == null) {
                            var response = new
                            {
                                status = "4 Bad request"
                            }.ToJson();
                            SendResponse(strm, client, response);
                        } else {
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
                }
                else if (request.Method.Equals(REQUEST_METHOD.update))
                {
                    if (request.Body == null)
                    {
                        var response = new
                        {
                            status = "Missing body"
                        }.ToJson();
                        SendResponse(strm, client, response);
                    }
                    else
                    {
                        if (path_formatted.Equals("categories"))
                        {
                            var response = new
                            {
                                status = "4 Bad Request"
                            }.ToJson();
                            SendResponse(strm, client, response);
                        }
                        else
                        {
                            int? id = GetIdFromPath(path_formatted);
                            Category category = null;
                            foreach (Category c in categories)
                            {
                                if (c.Cid == id)
                                {
                                    category = c;
                                }
                            }
                            //string result = myList.Single(s => s == search);
                            if (path_formatted.Contains("categories") && category != null)
                            {
                                categories.Remove(category);
                                category.Cid = request.Body.Cid;
                                category.Name = request.Body.Name;
                                categories.Add(category);
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
                }
                else if (request.Method.Equals(REQUEST_METHOD.create))
                {
                    if (request.Body == null)
                    {
                        var response = new
                        {
                            status = "Missing body"
                        }.ToJson();
                        SendResponse(strm, client, response);
                    }
                    else
                    {
                        if (path_formatted.Equals("categories") && request.Body != null)
                        {
                            int maxId = categories.Max(i => i.Cid);
                            Category newCategory = new Category
                            {
                                Cid = maxId + 1,
                                Name = request.Body.Name
                            };
                            categories.Add(newCategory);
                            var response = new
                            {
                                status = "2 Created",
                                body = newCategory.ToJson()
                            }.ToJson();
                            SendResponse(strm, client, response);
                        }
                        else
                        {
                            var response = new
                            {
                                status = "4 Bad Request"
                            }.ToJson();
                            SendResponse(strm, client, response);
                        }
                    }
                }
                else if (request.Method.Equals(REQUEST_METHOD.delete))
                {
                    if (path_formatted.Equals("categories") || !path_formatted.Contains("categories")) 
                    {
                        var response = new
                        {
                            status = "4 Bad Request"
                        }.ToJson();
                        SendResponse(strm, client, response);
                    }
                    else {
                        int? id = GetIdFromPath(path_formatted);
                        Category category = null;
                        foreach (Category c in categories)
                        {
                            if (c.Cid == id)
                            {
                                category = c;
                            }
                        }
                        if (category == null) {
                            var response = new
                            {
                                status = "5 Not Found"
                            }.ToJson();
                            SendResponse(strm, client, response);
                        } else {
                            categories.Remove(category);
                            var response = new
                            {
                                status = "1 Ok"
                            }.ToJson();
                            SendResponse(strm, client, response);
                        }
                    }
                }
                else if (request.Method.Equals(REQUEST_METHOD.echo))
                {
                    if (request.Body == null)
                    {
                        var response = new
                        {
                            status = "Missing body"
                        }.ToJson();
                        SendResponse(strm, client, response);
                    }
                    else
                    {
                        var response = new
                        {
                            body = request.Body
                        }.ToJson();
                        SendResponse(strm, client, response);
                    }
                }
                else {
                    var response = new
                    {
                        status = "Illegal method"
                    }.ToJson();
                    SendResponse(strm, client, response);
                }
            } else {
                var response = new
                {
                    status = "4 Bad Request"
                }.ToJson();
                SendResponse(strm, client, response);
            }
        }
        static int? GetIdFromPath(string path) {
            string resultString = Regex.Match(path, @"\d+").Value;
            if (resultString == "") {
                return null;
            } else {
                int id = Int32.Parse(resultString);
                return id;
            }
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

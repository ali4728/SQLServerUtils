using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SQLServerUtils
{
    class SimpleHttpServer
    {
        protected static List<string> serverList;
        protected readonly string url;
        private Dictionary<string, MethodInfo> httpRequestHandlers;
        private HttpListener httpListner;

        public SimpleHttpServer(string url)
        {
            this.url = url;
            serverList = getServerList(ConfigurationManager.AppSettings["ServerListFilePath"]);
            httpListner = new HttpListener();
            httpListner.Prefixes.Add(url);

            httpRequestHandlers = GetType()
                .GetMethods()
                .Where(y => y.GetCustomAttributes(false).OfType<RequestMapping>().Any())
                .ToDictionary(y => y.GetCustomAttributes(false).OfType<RequestMapping>().First().ToString());
        }

        public void Start()
        {
            Console.WriteLine($"Listening {url}");
            httpListner.Start();
            Task.Factory.StartNew(Listner);
        }

        public void Stop()
        {
            httpListner.Stop();
        }

        private async void Listner()
        {
            for (;;)
            {
                var context = await httpListner.GetContextAsync();
                Task.Factory.StartNew(() => processHttpRequest(context));
            }
        }

        private void processHttpRequest(HttpListenerContext context)
        {
            var response = context.Response;
            //Console.WriteLine($"Thread ID {Thread.CurrentThread.ManagedThreadId} start");
            Console.WriteLine("HttpMethod: " + context.Request.HttpMethod + " RawUrl: " + context.Request.RawUrl);
            //Console.WriteLine(context.Request.HttpMethod);

            MethodInfo handler;
            string requestHash = $"{context.Request.Url.AbsolutePath} - {context.Request.HttpMethod}";
            if (httpRequestHandlers.TryGetValue(requestHash, out handler))
            {
                try
                {
                    handler.Invoke(this, new object[]{context});
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex}");
                }
            }
            else
            {
                sendAnswerWithCode(context.Response, 404);
            }
            //Console.WriteLine($"Thread ID {Thread.CurrentThread.ManagedThreadId} stop");
        }

        protected static void sendStaticResourceWithCode(HttpListenerResponse response, string resourceText, string mime, int statusCode)
        {
            var buffer = Encoding.UTF8.GetBytes(resourceText);
            response.ContentLength64 = buffer.Length;
            response.Headers.Add("Content-Type", mime);
            response.StatusCode = statusCode;
            var output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }

        protected static void sendStaticResource(HttpListenerResponse response, string resourceText, string mime)
        {
            sendStaticResourceWithCode(response, resourceText, mime, 200);
        }

        protected static void sendAnswerWithCode(HttpListenerResponse response, int statusCode)
        {
            response.Headers.Clear();
            response.SendChunked = false;
            response.StatusCode = statusCode;
            response.Headers.Add("Server", String.Empty);
            response.Headers.Add("Date", String.Empty);
            response.Close();
            return;
        }

        protected static List<string> getServerList(string ServerListFilePath)
        {
            List<string> sl = File.ReadAllLines(ServerListFilePath).ToList();
            return sl;
        }

        private static void addServer(string servername)
        {            
            using (StreamWriter sw = File.AppendText(ConfigurationManager.AppSettings["ServerListFilePath"]))
            {
                sw.WriteLine(servername);                
            }
        }
        public static List<string> updateServerListPublic(string servername)
        {
            addServer(servername);

            serverList = getServerList(ConfigurationManager.AppSettings["ServerListFilePath"]);

            return serverList;
        }
        public static List<string> getServerListPublic()
        {
            return serverList;
        }
    }
}

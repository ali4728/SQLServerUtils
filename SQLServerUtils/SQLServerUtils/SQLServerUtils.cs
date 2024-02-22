using System;
using System.Configuration;


namespace SQLServerUtils
{
    class Program
    {
        static class MainClass
        {
            static void Main()
            {

                string port = ConfigurationManager.AppSettings["Port"];
                string url = $"http://localhost:{port}/";

                TheHttpServer httpServer = new TheHttpServer(url);
                httpServer.Start();
                Console.ReadKey();
                httpServer.Stop();
            }
        }
    }
}

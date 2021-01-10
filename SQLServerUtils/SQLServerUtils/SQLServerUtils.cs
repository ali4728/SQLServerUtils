using System;


namespace SQLServerUtils
{
    class Program
    {
        static class MainClass
        {
            static void Main()
            {
                TheHttpServer httpServer = new TheHttpServer("http://localhost:8084/");
                httpServer.Start();
                Console.ReadKey();
                httpServer.Stop();
            }
        }
    }
}

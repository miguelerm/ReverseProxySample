using Microsoft.Owin.Hosting;
using System;

namespace ReverseProxy
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using (WebApp.Start<Startup>("http://+:8001"))
            {
                Console.WriteLine("Poxy Iniciado");
                Console.ReadLine();
            }
        }
    }
}
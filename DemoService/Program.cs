using Microsoft.Owin.Hosting;
using System;

namespace DemoService
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using (WebApp.Start<Startup>("http://*:8002"))
            {
                Console.WriteLine("Instancia 'A' iniciada");

                using (WebApp.Start<Startup>("http://*:8003"))
                {
                    Console.WriteLine("Instancia 'B' iniciada");
                    Console.ReadLine();
                }
            }
        }
    }
}
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DemoService
{
    internal class Program
    {

        public static readonly int Port;

        static Program()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            Port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
        }
        
        private static void Main(string[] args)
        {
            var url = $"http://*:{Port}";
            using (WebApp.Start<Startup>(url))
            {
                Console.WriteLine("Servicio iniciado :" + url);

                using (var connection = new HubConnection("http://localhost:8001/signalr", new Dictionary<string, string> { { "service", "demo" }, { "port", Port.ToString() } }))
                {
                    var serviceHub = connection.CreateHubProxy("ServiceHub");
                    Task.Factory.StartNew(IniciarConexion(connection));
                    Console.ReadLine();
                    connection.Stop();
                }
            }
        }

        private static Action IniciarConexion(HubConnection connection)
        {
            return () =>
            {
                while(true)
                {
                    try
                    {
                        connection.Start().Wait();
                        Console.WriteLine("Conectado al proxy");
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("No se pudo conectar al proxy:{0}", ex.GetBaseException());
                    }

                    Task.Delay(1000).Wait();
                }
                
            };
        }

        
    }
}
using Microsoft.AspNet.SignalR;
using ReverseProxy.Handlers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReverseProxy.Hubs
{
    public class ServiceHub : Hub
    {
        public override Task OnConnected()
        {

            Connect();
            Log("connected");
            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            Connect();
            Log("reconnected");
            return base.OnReconnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            Disconect();
            Log("disconected");
            return base.OnDisconnected(stopCalled);
        }

        private void Connect()
        {
            var port = 0;
            var service = Context.QueryString["service"];

            if (!int.TryParse(Context.QueryString["port"], out port) || string.IsNullOrWhiteSpace(service))
            {
                return;
            }

            ProxyHandler.Hosts.AddOrUpdate(
                service,
                key => new ConcurrentQueue<int>(new[] { port }),
                (key, current) => {
                    current.Enqueue(port);
                    return current;
                }
            );
        }

        private void Disconect()
        {
            var port = 0;
            var service = Context.QueryString["service"];

            if (!int.TryParse(Context.QueryString["port"], out port) || string.IsNullOrWhiteSpace(service))
            {
                return;
            }

            ConcurrentQueue<int> ports;
            if (ProxyHandler.Hosts.TryGetValue(service, out ports))
            {
                var newPorts = ports.ToArray().Where(x => x != port).ToList();
                while (!ports.IsEmpty)
                {
                    var tempPort = 0;
                    ports.TryDequeue(out tempPort);
                }

                newPorts.ForEach(ports.Enqueue);
            }
        }

        private void Log(string status)
        {
            var port = Context.QueryString["port"];
            var service = Context.QueryString["service"];

            Console.WriteLine("Service {0} {2} on {1}", service, port, status);
        }
    }
}

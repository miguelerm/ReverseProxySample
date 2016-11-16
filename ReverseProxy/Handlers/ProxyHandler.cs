using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ReverseProxy.Handlers
{
    public class ProxyHandler : DelegatingHandler
    {
        public static readonly ConcurrentDictionary<string, ConcurrentQueue<int>> Hosts = new ConcurrentDictionary<string, ConcurrentQueue<int>>();


        // basado en https://blog.kloud.com.au/2013/11/24/do-it-yourself-web-api-proxy/
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            ConcurrentQueue<int> ports = null;
            int port = 80;
            int indexOfSlash = -1;
            int startIndexForSlash = 0;
            int forwardPathIndex = -1;
            string path = request.RequestUri.LocalPath;
            string forwardPath = null;
            string servicio = null;

            if (path.StartsWith("/"))
            {
                startIndexForSlash = 1;
            }

            indexOfSlash = path.IndexOf("/", startIndexForSlash);

            if (indexOfSlash >= 0)
            {
                servicio = path.Substring(startIndexForSlash, indexOfSlash - 1);
            }
            else
            {
                servicio = path.Substring(startIndexForSlash, path.Length - startIndexForSlash);
            }

            if (string.IsNullOrWhiteSpace(servicio))
            {
                return NotFound(request);
            }

            if (!Hosts.ContainsKey(servicio))
            {
                return NotFound(request);
            }

            if (!Hosts.TryGetValue(servicio, out ports))
            {
                return ServiceUnavailable(request);
            }

            if (ports.IsEmpty)
            {
                return ServiceUnavailable(request);
            }

            if (!ports.TryDequeue(out port))
            {
                return ServiceUnavailable(request);
            }

            ports.Enqueue(port);

            forwardPathIndex = startIndexForSlash + servicio.Length;

            if (path.Length > forwardPathIndex)
            {
                forwardPath = path.Substring(forwardPathIndex);
            }
            else
            {
                forwardPath = "/";
            }

            var forwardUri = new UriBuilder("http", "localhost", port, forwardPath, request.RequestUri.Query);
            request.RequestUri = forwardUri.Uri;
            Console.WriteLine("Url: {0}", request.RequestUri);
            // Evitar violacion de protocolo, enviando explicitamente el content nulo para GET y TRACE.
            if (request.Method == HttpMethod.Get || request.Method == HttpMethod.Trace) request.Content = null;

            HttpClient client = new HttpClient();
            return client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        }

        private static Task<HttpResponseMessage> NotFound(HttpRequestMessage request)
        {
            var mensaje = "Recurso no encontrado";
            var response = request.CreateErrorResponse(HttpStatusCode.NotFound, mensaje);
            response.Content = new StringContent(mensaje);
            return Task.FromResult(response);
        }

        private static Task<HttpResponseMessage> ServiceUnavailable(HttpRequestMessage request)
        {
            var mensaje = "Servicio no disponible";
            var response = request.CreateErrorResponse(HttpStatusCode.ServiceUnavailable, mensaje);
            response.Content = new StringContent(mensaje);
            return Task.FromResult(response);
        }
    }
}
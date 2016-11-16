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
        private static readonly ConcurrentDictionary<string, ConcurrentQueue<int>> hosts = new ConcurrentDictionary<string, ConcurrentQueue<int>>
        (
            new Dictionary<string, ConcurrentQueue<int>>()
            {
                { "demo", new ConcurrentQueue<int>(new int [] { 8002, 8003 }) }
            }
        );

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

            if (!hosts.ContainsKey(servicio))
            {
                return NotFound(request);
            }

            if (!hosts.TryGetValue(servicio, out ports))
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
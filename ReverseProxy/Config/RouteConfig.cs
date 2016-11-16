using ReverseProxy.Handlers;
using System.Net.Http;
using System.Web.Http;

namespace ReverseProxy.Config
{
    internal static class RouteConfig
    {
        internal static void RegisterRoutes(HttpRouteCollection routes)
        {
            // Todas las rutas deben ejecutar el ProxyHandler
            routes.MapHttpRoute(
                name: "Proxy",
                routeTemplate: "{*path}",
                handler: HttpClientFactory.CreatePipeline
                    (
                        innerHandler: new HttpClientHandler(), // no debería de usarse
                        handlers: new DelegatingHandler[]
                        {
                            new ProxyHandler()
                        }
                    ),
                defaults: new { path = RouteParameter.Optional },
                constraints: null
            );
        }
    }
}
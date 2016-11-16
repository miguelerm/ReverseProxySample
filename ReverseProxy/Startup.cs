using Owin;
using ReverseProxy.Config;
using System.Web.Http;

namespace ReverseProxy
{
    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var httpconfig = new HttpConfiguration();
            RouteConfig.RegisterRoutes(httpconfig.Routes);
            appBuilder.UseWebApi(httpconfig);
        }
    }
}
using Owin;
using ReverseProxy.Config;
using System.Web.Http;

namespace ReverseProxy
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();

            var httpconfig = new HttpConfiguration();
            RouteConfig.RegisterRoutes(httpconfig.Routes);
            app.UseWebApi(httpconfig);
        }
    }
}
using DemoService.Config;
using Owin;
using System.Web.Http;

namespace DemoService
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
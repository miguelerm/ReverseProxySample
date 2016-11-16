using System;
using System.Web.Http;

namespace DemoService.Controllers
{
    public class ItemsController : ApiController
    {
        public IHttpActionResult Get()
        {
            Console.WriteLine("Consultando");
            return Ok(new[] { 1, 2, 3, 4 });
        }
    }
}
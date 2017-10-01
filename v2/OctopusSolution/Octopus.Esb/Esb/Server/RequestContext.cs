using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Octopus.Esb.Server
{
    public class RequestContext
    {
		public HttpContext HttpContext { get; set; }
		public RouteData RouteData { get; set; }
    }
}

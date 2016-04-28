using System.Web;
using System.Web.Routing;
using HTB.DevFx.Reflection;
using Octopus.Esb.Config;

namespace Octopus.Esb.Server
{
	public class ServiceContext : CallContext
	{
		public HttpContextBase HttpContext { get; set; }
		public RequestContext RequestContext { get; set; }
		public IValueProvider ValueProvider { get; set; }
		public bool Responsed { get; set; }
		internal ServiceItemSetting Setting { get; set; }
	}
}

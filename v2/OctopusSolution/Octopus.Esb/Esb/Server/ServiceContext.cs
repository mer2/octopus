using HTB.DevFx.Reflection;
using Microsoft.AspNetCore.Http;
using Octopus.Esb.Config;

namespace Octopus.Esb.Server
{
	public class ServiceContext : CallContext
	{
		public string ServiceName { get; set; }
		public string MethodName { get; set; }
		public HttpContext HttpContext { get; set; }
		public IValueProvider ValueProvider { get; set; }
		public bool Responsed { get; set; }
		public ServiceItemSetting Setting { get; set; }

		public static ServiceContext Current => ServiceFactory.ServiceContextCurrent.Value;
	}
}
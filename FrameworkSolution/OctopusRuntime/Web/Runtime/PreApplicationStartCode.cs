using System.Web;
using System.Web.Hosting;
using HTB.DevFx.Utils;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Octopus.Web.Mvc;
using Octopus.Web.Runtime;

[assembly: PreApplicationStartMethod(typeof(PreApplicationStartCode), "Start")]

namespace Octopus.Web.Runtime
{
	public static class PreApplicationStartCode
	{
		private static bool started;
		public static void Start() {
			if(started) {
				return;
			}
			started = true;

			StartInternal();
		}

		private static void StartInternal() {
			var type = ReflectionHelper.CreateType("HTB.DevFx.Web.HttpModuleWrapper, HTB.DevFx", typeof(IHttpModule), false);
			if(type != null) {
				DynamicModuleUtility.RegisterModule(type);
			}
			ContextItemHandleAttribute.Startup();
			HostingEnvironment.RegisterVirtualPathProvider(new EmbeddedVirtualPathProvider());
		}
	}
}

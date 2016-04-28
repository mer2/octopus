using System.Web;
using System.Web.Hosting;
using Octopus.Web.AdminBase;
using Octopus.Web.Mvc;

[assembly: PreApplicationStartMethod(typeof(PreApplicationStartCode), "Start")]

namespace Octopus.Web.AdminBase
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
			HostingEnvironment.RegisterVirtualPathProvider(new EmbeddedVirtualPathProvider());
		}
	}
}

using System;
using HTB.DevFx;
using Microsoft.AspNetCore.Hosting;

namespace Octopus.Web
{
    public static class AspnetCoreExtensions
    {
		public static IWebHostBuilder UseEsb(this IWebHostBuilder builder) {
		    if (builder == null) {
			    throw new ArgumentNullException(nameof(builder));
		    }
			var service = ObjectService.GetObject<WebHostService>();
			service.Init(builder);
			return builder;
	    }
	}
}
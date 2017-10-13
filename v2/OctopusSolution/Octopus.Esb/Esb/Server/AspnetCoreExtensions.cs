using System;
using System.Threading.Tasks;
using HTB.DevFx;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
[assembly: HostingStartup(typeof(Octopus.Esb.Server.Startup))]

namespace Octopus.Esb.Server
{
    public static class AspnetCoreExtensions
    {
	    internal static IApplicationBuilder UseEsb(this IApplicationBuilder app) {
		    if (app == null) {
			    throw new ArgumentNullException(nameof(app));
		    }
			return RegisterMiddleware(app);
	    }

	    internal static IApplicationBuilder RegisterMiddleware(IApplicationBuilder app) {
		    var factory = ObjectService.GetObject<IServiceFactory>();
		    return app.UseWhen(context => factory.IsHandleable(context), appBuilder => {
			    appBuilder.Use((ctx, next) => {
				    factory.ProcessRequest(ctx);
				    return Task.CompletedTask;
			    });
		    });
	    }

		public static IWebHostBuilder UseEsb(this IWebHostBuilder builder) {
		    if (builder == null) {
			    throw new ArgumentNullException(nameof(builder));
		    }
			return builder.UseSetting(WebHostDefaults.HostingStartupAssembliesKey, "Octopus.Esb");
	    }

	}

	internal class Startup : IHostingStartup
	{
		public void Configure(IWebHostBuilder builder) {
			builder.ConfigureServices(services => {
				services.AddSingleton<HttpContextHolder>();
				services.AddSingleton<IStartupFilter, StartupFilter>();
			});
		}
	}

	internal class StartupFilter : IStartupFilter
	{
		public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) {
			return app => {
				app.UseMiddleware<HttpContextHolder>();
				AspnetCoreExtensions.RegisterMiddleware(app);
				next(app);
			};
		}
	}
}
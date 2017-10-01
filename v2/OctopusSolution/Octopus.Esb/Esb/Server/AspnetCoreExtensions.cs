using System;
using System.Threading.Tasks;
using HTB.DevFx;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Octopus.Esb.Server
{
    public static class AspnetCoreExtensions
    {
	    public static IApplicationBuilder UseEsb(this IApplicationBuilder app) {
		    if (app == null) {
			    throw new ArgumentNullException(nameof(app));
		    }
			return RegisterMiddleware(app);
	    }

	    internal static IApplicationBuilder RegisterMiddleware(IApplicationBuilder app) {
		    var factory = ObjectService.GetObject<IServiceFactory>();
		    return app.UseWhen(context => factory.IsHandleable(context), appBuilder => {
			    appBuilder.Use((ctx, donext) => {
				    factory.ProcessRequest(ctx);
				    return Task.CompletedTask;
			    });
		    });
	    }

		internal static IWebHostBuilder UseEsb(this IWebHostBuilder builder) {
		    if (builder == null) {
			    throw new ArgumentNullException(nameof(builder));
		    }
		    return builder.ConfigureServices((ctx, services) => {
			    services.AddSingleton<IStartupFilter, StartupFilter>();
		    });
	    }

	    internal class StartupFilter : IStartupFilter
		{
			public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) {
				return app => RegisterMiddleware(app);
			}
		}
	}
}
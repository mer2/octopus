using System;
using System.Threading.Tasks;
using HTB.DevFx;
using HTB.DevFx.Core;
using HTB.DevFx.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Octopus.Esb.Server;
using Octopus.Web.Config;

namespace Octopus.Web
{
    internal class WebHostService : ServiceBase<WebHostServiceSetting>, IWebHostService, IStartupFilter
	{
	    public void Init(IWebHostBuilder builder) {
			//添加需要自动启动的程序集
			var assemblyNames = this.Setting.StartupAssemblyNames;
		    if (assemblyNames != null && assemblyNames.Length > 0) {
				var names = string.Join(';', assemblyNames);
				builder.UseSetting(WebHostDefaults.HostingStartupAssembliesKey, names);
		    }
			//初始化侦听地址
			var port = this.Setting.Port;
		    if (port > 0) {
			    var url = this.Setting.Address;
			    if (string.IsNullOrEmpty(url)) {
				    url = "http://*";
			    }
			    url += ":" + this.Setting.Port;
			    builder.UseUrls(url);
		    } else {
				this.Setting.Port = 5000;//默认为5000端口
		    }

		    builder.ConfigureServices(services => {
				//把自己加进去
			    services.AddSingleton<IStartupFilter>(this);

				var filters = this.Setting.StartupFilterTypes;
			    if (filters != null && filters.Length > 0) {
				    foreach (var filter in filters) {
						var instance = this.ObjectService.GetOrCreateObject<IStartupFilter>(filter.TypeName);
					    if (instance != null) {
							services.AddSingleton(instance);
					    }
				    }
			    }
				var middlewares = this.Setting.MiddlewareTypes;
			    if (middlewares != null && middlewares.Length > 0) {
				    foreach (var middleware in middlewares) {
						var typeName = this.ObjectService.GetTypeName(middleware.TypeName);
					    if (!string.IsNullOrEmpty(typeName)) {
							var type = TypeHelper.CreateType(typeName, typeof(IMiddleware), false);
						    if (type != null) {
								services.AddSingleton(type);
								middleware.Type = type;
						    }
					    }
				    }
			    }
		    });
	    }

		public int Port => this.Setting.Port;

		Action<IApplicationBuilder> IStartupFilter.Configure(Action<IApplicationBuilder> next) {
			return app => {
				var middlewares = this.Setting.MiddlewareTypes;
				if (middlewares != null && middlewares.Length > 0) {
					foreach (var middleware in middlewares) {
						var type = middleware.Type;
						if (type != null) {
							app.UseMiddleware(type);
						}
					}
				}
				next(app);
				this.RegisterMiddleware(app);
			};
		}

		private void RegisterMiddleware(IApplicationBuilder app) {
			var factory = this.ObjectService.GetObject<IServiceFactory>();
			app.UseWhen(context => factory.IsHandleable(context), appBuilder => {
				appBuilder.Use((ctx, next) => {
					factory.ProcessRequest(ctx);
					return Task.CompletedTask;
				});
			});
		}
	}
}

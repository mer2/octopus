using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Octopus.Esb.Server;

namespace ConsoleApp1
{
	public class Startup
	{
		public Startup(IConfiguration configuration) {
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services) {
			//services.AddMvc();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
			if(env.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
			}
			app.UseEsb();

			/*app.UseWhen(context => {
				var matched = context.Request.Path.Value.StartsWith("/Services/", StringComparison.OrdinalIgnoreCase);
				return matched;
			}, appBuilder => {
				//var factory = ObjectService.GetObject<IServiceFactory>();
				//appBuilder.Use((ctx, next) => factory.InvokeAsync(ctx, null));
			});*/
			// app.UseMvc();
		}
	}
}

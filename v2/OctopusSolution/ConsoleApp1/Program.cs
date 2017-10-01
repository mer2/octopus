using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

//[assembly: HostingStartup(typeof(MyHostingStartup))]

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
			BuildWebHost(args).Run();
			/*var regexString = "/Services/(?<serviceName>\\w+)(/(?<methodName>\\w+))?";
	        var regexFind = new Regex(regexString, RegexOptions.Compiled | RegexOptions.Singleline);
			var match = regexFind.Match("/Services/PassportService/Hello?v=sss");*/
			Console.WriteLine("Hello World!");
			Console.ReadLine();
        }

	    public static IWebHost BuildWebHost(string[] args) {
		    return WebHost.CreateDefaultBuilder(args)
				.UseStartup<Startup>()
				.Build();
	    }
    }

	class MyHostingStartup : IHostingStartup
	{
		public void Configure(IWebHostBuilder builder) {
			Console.WriteLine("MyHostingStartup.Configure");
			builder.ConfigureServices((ctx, services) => {
				services.AddSingleton<IStartupFilter, A>();
				services.AddSingleton<IStartupFilter, B>();
			});
		}
	}

	public class A : IStartupFilter
	{
		public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) {
			Console.WriteLine("This is A1!");
			return app =>
			{
				Console.WriteLine("This is A2!");
				app.Use(next1 =>
				{
					Console.WriteLine("A");
					return async (context) =>
					{
						// 1. 对Request做一些处理
						// TODO

						// 2. 调用下一个中间件
						Console.WriteLine("A-BeginNext");
						await next1(context);
						Console.WriteLine("A-EndNext");

						// 3. 生成 Response
						//TODO
					};
				});

				app.Use(next1 =>
				{
					Console.WriteLine("B");
					return async (context) =>
					{
						// 1. 对Request做一些处理
						// TODO

						// 2. 调用下一个中间件
						Console.WriteLine("B-BeginNext");
						await next1(context);
						//await context.Response.WriteAsync("Hello ASP.NET Core!");
						Console.WriteLine("B-EndNext");

						// 3. 生成 Response
						//TODO
					};
				});
				next(app);
			};
		}
	}

	public class B : IStartupFilter
	{
		public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) {
			Console.WriteLine("This is B1!");
			return app =>
			{
				Console.WriteLine("This is B2!");
				next(app);
			};
		}
	}
}

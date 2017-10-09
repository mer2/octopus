using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
			BuildWebHost(args).Run();
			Console.WriteLine("Hello World!");
			Console.ReadLine();
        }

	    public static IWebHost BuildWebHost(string[] args) {
		    return WebHost.CreateDefaultBuilder(args)
				.UseStartup<Startup>()
				.Build();
	    }
    }
}

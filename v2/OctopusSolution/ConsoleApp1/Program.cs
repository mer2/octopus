﻿using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Octopus.Web;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
	        try {
				throw new NotImplementedException();
	        } catch { }
			BuildWebHost(args).Run();
			Console.WriteLine("Hello World!");
			Console.ReadLine();
        }

	    public static IWebHost BuildWebHost(string[] args) {
		    return WebHost.CreateDefaultBuilder(args)
				.UseStartup<Startup>()
				.UseEsb()
				.Build();
	    }
    }
}

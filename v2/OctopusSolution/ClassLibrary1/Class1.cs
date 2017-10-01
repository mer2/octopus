using System;
using Microsoft.AspNetCore.Hosting;

namespace ClassLibrary1
{
    public class Class1 : IHostingStartup
	{
		public void Configure(IWebHostBuilder builder) {
			Console.WriteLine("ClassLibrary1.Class1.Configure");
		}
	}
}

using ClassLibrary1;
using Octopus.Esb.Server;

namespace ConsoleApp1
{
	/*public interface ITestService
	{
		string Hello(string world);
	}*/
 
	internal class TestService : ITestService
	{
		public string Hello(string world) {
			var ctx = HttpContextHolder.Current;
			return ctx.TraceIdentifier + " hello world!" + world;
		}
	}
}

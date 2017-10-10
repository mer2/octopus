using ClassLibrary1;

namespace ConsoleApp1
{
	/*public interface ITestService
	{
		string Hello(string world);
	}*/
 
	internal class TestService : ITestService
	{
		public string Hello(string world) {
			return "hello world!" + world;
		}
	}
}

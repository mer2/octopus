using HTB.DevFx.Core;

namespace ClassLibrary1
{
	public interface ITestService
	{
		string Hello(string world);
		IAOPResult Welcome(string hello);
		IAOPResult<int> Test(int a);
	}
}
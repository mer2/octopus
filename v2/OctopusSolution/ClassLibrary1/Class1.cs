using HTB.DevFx.Core;

namespace ClassLibrary1
{
	public class Entity
	{
		public string Name { get; set; }
		public int Age { get; set; }
	}

	public interface ITestService
	{
		string Hello(string world);
		IAOPResult Welcome(string hello);
		IAOPResult<int> Test(int a);
		IAOPResult<Entity> GetEntity();
		Entity GetRawEntity();
		Entity SetRawEntity(Entity entity);
	}
}
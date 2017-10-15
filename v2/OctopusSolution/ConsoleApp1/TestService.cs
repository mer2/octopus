using ClassLibrary1;
using HTB.DevFx.Core;
using Octopus.Security;
using Octopus.Web;

namespace ConsoleApp1
{
	/*public interface ITestService
	{
		string Hello(string world);
	}*/
 
	internal class TestService : ITestService
	{
		[PostAuthorize]
		public string Hello(string world) {
			var ctx = HttpContextHolder.Current;
			return ctx.TraceIdentifier + " hello world!" + world;
		}

		public IAOPResult Welcome(string hello) {
			return AOPResult.Success("中文：" + hello);
		}

		public IAOPResult<int> Test(int a) {
			return AOPResult.Success(a + 1);
		}

		public IAOPResult<Entity> GetEntity() {
			return AOPResult.Success(new Entity { Name = "Abc", Age = 100 });
		}

		public Entity GetRawEntity() {
			return new Entity { Name = "Raw", Age = 101 };
		}

		public Entity SetRawEntity(Entity entity) {
			entity.Age++;
			return entity;
		}
	}
}

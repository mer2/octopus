using System.Collections;
using System.Web;
using HTB.DevFx.Core;

namespace Octopus.Admin.Server
{
	public interface IPassportServerService
	{
		IAOPResult Login(HttpContextBase ctx, string userName, string password, IDictionary options);
	}
}
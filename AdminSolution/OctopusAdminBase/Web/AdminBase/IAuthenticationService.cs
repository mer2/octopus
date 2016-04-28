using System.Web;
using HTB.DevFx.Core;

namespace Octopus.Web.AdminBase
{
	public interface IAuthenticationService
	{
		string GetAuthenticationUrl(HttpContextBase ctx, string returnUrl);
		IAOPResult<string> Validate(HttpContextBase ctx, string ticket);
	}
}

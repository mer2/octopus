using System.Collections;
using HTB.DevFx.Core;

namespace Octopus.Web.AdminBase
{
	public interface IPassportService
	{
		IAOPResult<PassportIdentity> ValidateSession(string appNo, string ticket, IDictionary options);
	}
}
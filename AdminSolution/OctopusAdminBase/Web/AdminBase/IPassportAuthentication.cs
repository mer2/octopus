using Octopus.Web.AdminBase.Config;

namespace Octopus.Web.AdminBase
{
	public interface IPassportAuthentication
	{
		IPassportAuthenticationSetting Setting { get; }
	}
}
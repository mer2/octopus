namespace Octopus.Web.AdminBase.Config
{
	public interface IPassportAuthenticationSetting
	{
		string LoginUrl { get; }
		string DefaultUrl { get; }
		string CookieName { get; }
		string MainDomain { get; }
	}
}

using HTB.DevFx;

namespace Octopus.Web.AdminBase
{
	public static class FormsAuthentication
	{
		public static string LoginUrl {
			get { return ObjectService.GetObject<PassportAuthentication>().CurrentSetting.LoginUrl; }
		}

		public static string DefaultUrl {
			get { return ObjectService.GetObject<PassportAuthentication>().CurrentSetting.DefaultUrl; }
		}

		public static string FormsCookieName {
			get { return ObjectService.GetObject<PassportAuthentication>().CurrentSetting.CookieName; }
		}

		public const int SessionTokenMaxLength = 32;
	}
}
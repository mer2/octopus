using System;
using System.Web.Security;
using HTB.DevFx.Config;

[assembly: ConfigResource("res://Octopus.Admin.Server.Config.Settings.config")]

namespace Octopus.Admin.Server.Config
{
	internal class PassportServerServiceSetting : ConfigSettingElement
	{
		protected override void OnConfigSettingChanged() {
			base.OnConfigSettingChanged();
			this.CookieName = this.GetSetting("cookieName", FormsAuthentication.FormsCookieName);
			this.CookiePath = this.GetSetting("cookiePath", FormsAuthentication.FormsCookiePath);
			this.CookieDomain = this.GetSetting("cookieDomain", FormsAuthentication.CookieDomain);
			this.CacheName = this.GetSetting("cacheName", "AdminPassportCache");
		}

		public string CookieName { get; private set; }
		public string CookiePath { get; private set; }
		public string CookieDomain { get; private set; }
		public TimeSpan CookieTimeout { get { return FormsAuthentication.Timeout; } }
		public string CacheName { get; private set; }
	}
}
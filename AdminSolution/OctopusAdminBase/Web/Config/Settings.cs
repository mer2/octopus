using System;
using System.Web.Security;
using HTB.DevFx.Config;
using Octopus.Web.AdminBase.Config;

[assembly: ConfigResource("res://Octopus.Web.Config.Settings.config")]
[assembly: ConfigDependency("Octopus.SecurityPermissions")]

namespace Octopus.Web.Config
{
	internal class PassportAuthenticationSetting : ConfigSettingElement, IPassportAuthenticationSetting
	{
		protected override void OnConfigSettingChanged() {
			base.OnConfigSettingChanged();
			this.CookieName = this.GetSetting("cookieName", FormsAuthentication.FormsCookieName);
			this.SerializerName = this.GetSetting("serializerName", "application/bson");
			this.LoginUrl = this.GetRequiredSetting("loginUrl");
			this.DefaultUrl = this.GetRequiredSetting("defaultUrl");
			this.RemoveInvalidTicket = this.GetSetting("removeInvalidTicket", true);
			this.ApplicationNo = this.GetSetting("appNo");
			var md = this.GetSetting("mainDomain");
			if (string.IsNullOrEmpty(md)) {
				var domain = FormsAuthentication.CookieDomain;
				if (domain.StartsWith("admin.", StringComparison.InvariantCultureIgnoreCase)) {
					md = domain.Substring(6);
				} else {
					md = domain;
				}
			}
			this.MainDomain = md;
		}

		public string CookieName { get; private set; }
		public string SerializerName { get; private set; }
		public string LoginUrl { get; private set; }
		public string DefaultUrl { get; private set; }
		public bool RemoveInvalidTicket { get; private set; }//是否自动清除无效的票据，默认True
		public string ApplicationNo { get; private set; }
		public string MainDomain { get; private set; }//主域名，如joyyang.com，而非admin.joyyang.com
	}
}

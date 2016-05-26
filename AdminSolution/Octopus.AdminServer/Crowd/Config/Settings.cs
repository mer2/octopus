using HTB.DevFx.Config;
[assembly: ConfigResource("res://Octopus.Admin.Crowd.Config.Settings.config")]

namespace Octopus.Admin.Crowd.Config
{
	public class CrowdServiceSetting : ConfigSettingElement
	{
		protected override void OnConfigSettingChanged() {
			base.OnConfigSettingChanged();
			this.CrowdAppNo = this.GetRequiredSetting("crowdAppNo");
			this.CrowdAppSecretKey = this.GetRequiredSetting("crowdAppSecretKey");
			this.CrowdServiceBaseUrl = this.GetRequiredSetting("crowdBaseUrl");

			this.CacheName = this.GetSetting("cacheName", "crowdCache");
			this.CacheTimeout = this.GetSetting("cacheTimeout", 5 * 60);
		}

		public string CrowdAppNo { get; private set; }
		public string CrowdAppSecretKey { get; private set; }
		public string CrowdServiceBaseUrl { get; private set; }

		public string CacheName { get; private set; }
		public int CacheTimeout { get; private set; }
	}
}

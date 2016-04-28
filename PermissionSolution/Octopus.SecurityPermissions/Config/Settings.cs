using HTB.DevFx.Config;
[assembly: ConfigResource("res://Octopus.SecurityPermissions.Config.Settings.config")]

namespace Octopus.SecurityPermissions.Config
{
	public class PermissionClientServiceSetting : ConfigSettingElement
	{
		protected override void OnConfigSettingChanged() {
			base.OnConfigSettingChanged();
			this.Cached = this.GetSetting("cached", true);
			this.CacheName = this.GetSetting("cacheName", "PermissionCache");
			this.BufferTimeout = this.GetSetting("bufferTimeout", 300);
			this.AppNo = this.GetSetting("appNo");
			this.ErrorUrl = this.GetSetting("errorUrl");
		}

		public bool Cached { get; private set; }
		public string CacheName { get; private set; }
		public int BufferTimeout { get; private set; }
		public string AppNo { get; private set; }
		public string ErrorUrl { get; private set; }
	}
}
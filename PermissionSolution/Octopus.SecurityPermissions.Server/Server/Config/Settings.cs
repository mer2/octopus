using HTB.DevFx.Config;
[assembly: ConfigResource("res://Octopus.SecurityPermissions.Server.Config.Settings.config")]

namespace Octopus.SecurityPermissions.Server.Config
{
	internal class PermissionServiceSetting : ConfigSettingElement
	{
		protected override void OnConfigSettingChanged() {
			base.OnConfigSettingChanged();
			this.CacheName = this.GetSetting("cacheName", "PermissionServerCache");
			this.BufferTimeout = this.GetSetting("bufferTimeout", 300);
			this.DefaultRole = this.GetSetting("defaultRole", "Admin.Administrators");
		}

		public string CacheName { get; private set; }
		public int BufferTimeout { get; private set; }
		public string DefaultRole { get; private set; }
	}
}

using HTB.DevFx.Config;

[assembly: ConfigResource("res://Joy.Runtime.Memcached.Config.cached.config")]

namespace Joy.Runtime.Memcached.Config
{
	internal class CacheSetting : ConfigSettingElement
	{
		protected override void OnConfigSettingChanged() {
			base.OnConfigSettingChanged();
			this.CacheInstanceName = this.GetSetting("name");
			this.KeyPrefix = this.GetSetting("keyPrefix");
			this.Servers = this.GetSetting("servers");
			this.SerializerName = this.GetSetting("serializer", "application/json; iso");
		}

		public string CacheInstanceName { get; private set; }
		public string KeyPrefix { get; private set; }
		public string Servers { get; private set; }
		public string SerializerName { get; private set; }
	}
}
using HTB.DevFx.Config;
[assembly: ConfigResource("res://Octopus.Performance.Config.performance.config")]

namespace Octopus.Performance.Config
{
	internal class PerformanceServiceSetting : ConfigSettingElement
	{
		protected override void OnConfigSettingChanged() {
			base.OnConfigSettingChanged();
			this.Monitors = this.GetSettings<MonitorSetting>("monitors", null).ToArray();
		}

		public MonitorSetting[] Monitors { get; private set; }
	}

	internal class MonitorSetting : ConfigSettingElement
	{
		protected override void OnConfigSettingChanged() {
			base.OnConfigSettingChanged();
			this.TypeName = this.GetRequiredSetting("type");
			this.Enabled = this.GetSetting("enabled", true);
		}

		public string TypeName { get; private set; }
		public bool Enabled { get; private set; }
	}
}

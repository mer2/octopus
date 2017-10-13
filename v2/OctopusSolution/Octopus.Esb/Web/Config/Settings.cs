using System;
using System.Linq;
using HTB.DevFx.Config;
[assembly: ConfigResource("res://Octopus.Web.Config.Settings.config")]

namespace Octopus.Web.Config
{
    internal class WebHostServiceSetting : ConfigSettingElement
	{
		protected override void OnConfigSettingChanged() {
			base.OnConfigSettingChanged();
			this.Address = this.GetSetting("address");
			this.Port = this.GetSetting<int>("port");
			this.StartupAssemblyNames = this.GetSettings<TypeNameSetting>("startupAssemblies", null).Select(x => x.Name).ToArray();
			this.StartupFilterTypes = this.GetSettings<TypeNameSetting>("startupFilters", null).ToArray();
			this.MiddlewareTypes = this.GetSettings<TypeNameSetting>("middlewares", null).ToArray();
		}

		public string Address { get; private set; }
		public int Port { get; internal set; }
		public string[] StartupAssemblyNames { get; private set; }
		public TypeNameSetting[] StartupFilterTypes { get; private set; }
		public TypeNameSetting[] MiddlewareTypes { get; private set; }
	}

	internal class TypeNameSetting : ConfigSettingElement
	{
		protected override void OnConfigSettingChanged() {
			base.OnConfigSettingChanged();
			this.Name = this.GetRequiredSetting("name");
			this.TypeName = this.GetSetting("type");
		}

		public string Name { get; private set; }
		public string TypeName { get; private set; }

		internal Type Type { get; set; }
	}
}
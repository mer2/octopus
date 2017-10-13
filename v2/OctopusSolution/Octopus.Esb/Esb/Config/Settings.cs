using System;
using HTB.DevFx.Config;
using Octopus.Esb.Server;
[assembly: ConfigResource("res://Octopus.Esb.Config.Settings.config")]

namespace Octopus.Esb.Config
{
	internal class ServiceFactorySetting : ConfigSettingElement
	{
		protected override void OnConfigSettingChanged() {
			base.OnConfigSettingChanged();
			this.Debug = this.GetSetting("debug", false);
			this.RouteUrl = this.GetRequiredSetting("routeUrl");
			this.PathRegex = this.GetRequiredSetting("pathRegex");
			this.Extenders = this.GetSettings<ServiceFactoryExtenderSetting>("extenders", null).ToArray();
			this.ServiceItems = this.GetSettings<ServiceItemSetting>("services", null).ToArray();
		}
		
		public bool Debug { get; private set; }
		public string RouteUrl { get; private set; }
		public string PathRegex { get; private set; }
		public ServiceFactoryExtenderSetting[] Extenders { get; private set; }
		public ServiceItemSetting[] ServiceItems { get; private set; }
	}

	public class ServiceItemSetting : ConfigSettingElement
	{
		protected override void OnConfigSettingChanged() {
			base.OnConfigSettingChanged();
			this.Name = this.GetRequiredSetting("name");
			this.AliasName = this.GetSetting("alias");
			this.Authorization = this.GetSetting("authorize");
			this.ContractTypeName = this.GetRequiredSetting("type");
			this.ServiceTypeName = this.GetSetting("serviceType");
			this.Inherits = this.GetSetting("inherits", false);
		}

		public string Name { get; private set; }
		public string AliasName { get; private set; }
		public string Authorization { get; private set; }

		internal string ContractTypeName { get; set; }
		internal string ServiceTypeName { get; set; }
		internal bool Inherits { get; private set; }
		internal Type ContractType { get; set; }
		internal ServiceHandler ServiceHandler { get; set; }
	}

	internal class ServiceFactoryExtenderSetting : ConfigSettingElement
	{
		protected override void OnConfigSettingChanged() {
			base.OnConfigSettingChanged();
			this.TypeName = this.GetSetting("type");
			this.Enabled = this.GetSetting("enabled", true);
		}

		public string TypeName { get; private set; }
		public bool Enabled { get; private set; }
	}

	internal class AuthorizationProviderFactorySetting : ConfigSettingElement
	{
		protected override void OnConfigSettingChanged() {
			base.OnConfigSettingChanged();
			this.DefaultFactory = this.GetSetting("defaultFactory");
			this.Providers = this.GetSettings<AuthorizationProviderSetting>("providers", null).ToArray();
		}

		public string DefaultFactory { get; private set; }
		public AuthorizationProviderSetting[] Providers { get; private set; }
	}

	internal class AuthorizationProviderSetting : ConfigSettingElement
	{
		protected override void OnConfigSettingChanged() {
			base.OnConfigSettingChanged();
			this.Category = this.GetRequiredSetting("name");
			this.ProviderTypeName = this.GetRequiredSetting("type");
		}

		public string Category { get; private set; }
		public string ProviderTypeName { get; private set; }
	}
}

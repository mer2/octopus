using System;
using HTB.DevFx.Config;
using Octopus.Esb.Server;

[assembly: ConfigResource("res://Octopus.Esb.Config.htb.devfx.esb.config", Index = 1)]

namespace Octopus.Esb.Config
{
	internal class ServiceFactorySetting : ConfigSettingElement
	{
		protected override void OnConfigSettingChanged() {
			base.OnConfigSettingChanged();
			this.RouteUrl = this.GetRequiredSetting("routeUrl");
			this.Extenders = this.GetSettings<ServiceFactoryExtenderSetting>("extenders", null).ToArray();
			this.ServiceItems = this.GetSettings<ServiceItemSetting>("services", null).ToArray();
		}
		
		public string RouteUrl { get; private set; }
		public ServiceFactoryExtenderSetting[] Extenders { get; private set; }
		public ServiceItemSetting[] ServiceItems { get; private set; }
	}

	internal class ServiceItemSetting : ConfigSettingElement
	{
		protected override void OnConfigSettingChanged() {
			base.OnConfigSettingChanged();
			this.Name = this.GetRequiredSetting("name");
			this.ContractTypeName = this.GetRequiredSetting("type");
			this.ServiceTypeName = this.GetSetting("serviceType");
		}

		public string Name { get; private set; }
		public string ContractTypeName { get; internal set; }
		public string ServiceTypeName { get; internal set; }

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

	internal class SerializerFactorySetting : ConfigSettingElement
	{
		protected override void OnConfigSettingChanged() {
			base.OnConfigSettingChanged();
			this.DefaultSerializerName = this.GetRequiredSetting("defaultSerializer");
			this.Debug = this.GetSetting("debug", false);
			this.SerializerItems = this.GetSettings<SerializerItemSetting>(null).ToArray();
		}

		public string DefaultSerializerName { get; private set; }
		public bool Debug { get; private set; }
		public SerializerItemSetting[] SerializerItems { get; private set; }
	}

	internal class SerializerItemSetting : ConfigSettingElement
	{
		protected override void OnConfigSettingChanged() {
			base.OnConfigSettingChanged();
			this.Name = this.GetRequiredSetting("name");
			this.ContentType = this.GetRequiredSetting("contentType");
			this.TypeName = this.GetRequiredSetting("type");
			this.Enabled = this.GetSetting("enabled", true);
		}

		public string Name { get; private set; }
		public string ContentType { get; private set; }
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

	internal class HttpRealProxySetting : ConfigSettingElement
	{
		protected override void OnConfigSettingChanged() {
			base.OnConfigSettingChanged();
			this.Extenders = this.GetSettings<HttpRealProxyExtenderSetting>("extenders", null).ToArray();
		}

		public HttpRealProxyExtenderSetting[] Extenders { get; private set; }
	}

	internal class HttpRealProxyExtenderSetting : ConfigSettingElement
	{
		protected override void OnConfigSettingChanged() {
			base.OnConfigSettingChanged();
			this.TypeName = this.GetSetting("type");
			this.Enabled = this.GetSetting("enabled", true);
		}

		public string TypeName { get; private set; }
		public bool Enabled { get; private set; }
	}
}

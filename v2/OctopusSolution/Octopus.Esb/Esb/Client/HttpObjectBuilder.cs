using System;
using HTB.DevFx;
using HTB.DevFx.Config;
using HTB.DevFx.Core.Config;
using HTB.DevFx.Remoting;

namespace Octopus.Esb.Client
{
	internal class HttpObjectBuilder : IRemotingObjectBuilder
	{
		public object CreateObject(IObjectSettingLite setting, Type objectType, string uri, params object[] parameters) {
			var contentType = setting.ConfigSetting.GetSetting("contentType") ?? string.Empty;
			var factory = ObjectService.GetObject<IHttpRealProxyFactory>();
			var instance = factory.GetProxyObject(objectType, uri, contentType);
			return instance;
		}
	}
}
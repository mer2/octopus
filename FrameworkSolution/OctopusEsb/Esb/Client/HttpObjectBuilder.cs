using System;
using HTB.DevFx;
using HTB.DevFx.Esb.Config;
using HTB.DevFx.Remoting;

namespace Octopus.Esb.Client
{
	internal class HttpObjectBuilder : IRemotingObjectBuilder
	{
		public object CreateObject(IObjectSetting setting, Type objectType, string uri, params object[] parameters) {
			var proxy = ObjectService.CreateObject<IHttpRealProxy>(objectType, uri);
			var contentType = setting.ConfigSetting.Property.TryGetPropertyValue("contentType");
			if(!string.IsNullOrEmpty(contentType)) {
				proxy.ContentType = contentType;
			}
			proxy.Init();
			return proxy.GetTransparentProxy();
		}
	}
}
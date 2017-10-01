using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HTB.DevFx.Core;
using Octopus.Esb.Config;
using HTB.DevFx;
using HTB.DevFx.Utils;

namespace Octopus.Esb.Client
{
	internal class HttpRealProxyFactory : ServiceBase<HttpRealProxyFactorySetting>, IHttpRealProxyFactory, IHttpRealProxyFactoryInternal
	{
		private readonly List<HttpRealProxyHandlerSetting> handlers = new List<HttpRealProxyHandlerSetting>();
		private readonly IDictionary<string, IHttpRealProxy> proxies = new Dictionary<string, IHttpRealProxy>();
		protected override void OnInit() {
			base.OnInit();
			var setting = this.Setting;
			if (setting?.Handlers != null) {
				this.handlers.AddRange(setting.Handlers);
			}
			if(setting?.Extenders != null) {
				foreach(var extenderSetting in setting.Extenders) {
					if(!extenderSetting.Enabled) {
						continue;
					}
					var extender = this.ObjectService.GetOrCreateObject<IObjectExtender<IHttpRealProxyFactory>>(extenderSetting.TypeName);
					extender?.Init(this);
				}
			}
		}

		public object GetProxyObject(Type objectType, string url, string contentType, IDictionary options = null) {
			var proxy = this.GetHttpRealProxy(objectType, url, contentType, options);
			return proxy?.GetTransparentProxy();
		}

		public T GetProxyObject<T>(string url, string contentType, IDictionary options = null) where T : class {
			var instance = this.GetProxyObject(typeof(T), url, contentType, options);
			return (T)instance;
		}

		public IHttpRealProxy GetHttpRealProxy(Type objectType, string url, string contentType, IDictionary options = null) {
			if(objectType == null || string.IsNullOrEmpty(url)) {
				return null;
			}
			var key = $"{objectType.AssemblyQualifiedName}_{url}";
			IHttpRealProxy proxy;
			lock(this.proxies) {
				if(!this.proxies.TryGetValue(key, out proxy)) {
					proxy = this.CreateHttpRealProxy(objectType, url, contentType, options);
					this.proxies.Add(key, proxy);
				}
			}
			return proxy;
		}

		protected IHttpRealProxy CreateHttpRealProxy(Type objectType, string url, string contentType, IDictionary options = null) {
			if (contentType == null) {
				contentType = string.Empty;
			}
			IHttpRealProxy proxy = null;
			var setting = this.handlers.FirstOrDefault(x => url.StartsWith(x.Name, StringComparison.InvariantCultureIgnoreCase));
			if (!string.IsNullOrEmpty(setting?.TypeName)) {
				var typeName = setting.TypeName;
				proxy = this.ObjectService.CreateObject(typeName, objectType, url, contentType) as IHttpRealProxy;
				if (proxy == null) {
					typeName = this.ObjectService.GetTypeName(typeName);
					proxy = TypeHelper.CreateObject(typeName, typeof(IHttpRealProxy), false, objectType, url, contentType) as IHttpRealProxy;
				}
			}
			if (proxy == null) {
				proxy = this.ObjectService.CreateObject<IHttpRealProxy>(objectType, url, contentType);
			}
			proxy.Init(this, options);
			return proxy;
		}

		public event Action<ProxyContext> Calling;
		public event Action<ProxyContext> Request;
		public event Action<ProxyContext, IAOPResult> ResultHandling;
		public event Action<ProxyContext> Called;

		public void OnCalling(ProxyContext ctx) {
			this.Calling?.Invoke(ctx);
		}
		public void OnRequest(ProxyContext ctx) {
			this.Request?.Invoke(ctx);
		}
		public void OnResultHandling(ProxyContext ctx, IAOPResult result) {
			this.ResultHandling?.Invoke(ctx, result);
		}
		public void OnCalled(ProxyContext ctx) {
			this.Called?.Invoke(ctx);
		}
	}
}

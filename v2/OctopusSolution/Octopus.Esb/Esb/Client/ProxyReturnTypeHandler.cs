using System;
using System.Collections.Generic;
using HTB.DevFx.Core;
using HTB.DevFx.Utils;
using Octopus.Esb.Config;

namespace Octopus.Esb.Client
{
	internal class ProxyReturnTypeHandler : ServiceBase<ProxyReturnTypeHandlerSetting>, IObjectExtender<IHttpRealProxyFactory>
	{
		private readonly IDictionary<string, Type> returnTypes = new Dictionary<string, Type>();
		public void Init(IHttpRealProxyFactory instance) {
			var setting = this.Setting;
			if(setting?.ReturnTypes != null) {
				foreach(var typeSetting in setting.ReturnTypes) {
					var type = TypeHelper.CreateType(typeSetting.TypeName, false);
					if(type != null) {
						this.returnTypes.Add(typeSetting.Name, type);
					}
				}
			}
			instance.Request += this.OnRequest;
		}

		private void OnRequest(ProxyContext ctx) {
			if (ctx.ExpectedReturnType != null) {
				return;
			}
			var name = $"{ctx.ProxyInstance.ProxyType}.{ctx.CallMethod.Name}";
			if(this.returnTypes.TryGetValue(name, out var type)) {
				ctx.ExpectedReturnType = type;
			}
		}
	}
}
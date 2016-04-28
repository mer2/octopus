using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HTB.DevFx;
using HTB.DevFx.Core;
using Octopus.Esb.Config;
using Octopus.Security;

namespace Octopus.Esb.Server
{
	internal class AuthorizationProviderFactory : ServiceBase<AuthorizationProviderFactorySetting>, IObjectExtender<IServiceFactory>, IAuthorizationProviderFactory
	{
		private Dictionary<string, IAuthorizationProvider> providers;
		private IAuthorizationProviderFactory providerFactory;
		protected IAuthorizationProviderFactory CurrentProviderFactory {
			get { return this.providerFactory ?? (this.providerFactory = this.ObjectService.GetObject<IAuthorizationProviderFactory>(this.Setting.DefaultFactory) ?? this); }
		}
		protected override void OnInit() {
			base.OnInit();
			this.providers = new Dictionary<string, IAuthorizationProvider>();
			var prders = this.Setting.Providers;
			if(prders != null && prders.Length > 0) {
				foreach (var prder in prders) {
					var instance = this.ObjectService.GetOrCreateObject<IAuthorizationProvider>(prder.ProviderTypeName);
					if(instance != null) {
						this.providers.Add(prder.Category, instance);
					}
				}
			}
		}

		public void Init(IServiceFactory instance) {
			instance.Calling += this.OnCalling;
		}

		public void Authorize(ServiceContext ctx, IAuthorizationProviderFactory defaultFactory, IAuthorizationIdentity[] identities) {
			if (identities == null) {
				return;
			}
			foreach(var identity in identities) {
				IAuthorizationProvider provider;
				if(!this.providers.TryGetValue(identity.Category, out provider)) {
					continue;
				}
				if(!provider.Authorize(ctx, identity)) {
					if(!ctx.ResultInitialized) {
						ctx.ResultInitialized = true;
						ctx.ResultValue = AOPResult.Failed(string.Format("Authorized Failed with {0}", identity.Category));
					}
					break;
				}
			}
		}

		private void OnCalling(ServiceContext ctx) {
			this.Authorize(ctx);
		}

		private static readonly Dictionary<MethodInfo, IAuthorizationIdentity[]> AuthorizeMetadata = new Dictionary<MethodInfo, IAuthorizationIdentity[]>();
		private void Authorize(ServiceContext ctx) {
			var method = ctx.CallMethod;
			IAuthorizationIdentity[] identities;
			if(!AuthorizeMetadata.TryGetValue(method, out identities)) {
				lock(method) {
					if(!AuthorizeMetadata.TryGetValue(method, out identities)) {
						var attributes = GetAuthorizationIdenties(ctx);
						if(attributes != null) {
							identities = attributes.ToArray();
						}
						lock (AuthorizeMetadata) {
							AuthorizeMetadata.Add(method, identities);
						}
					}
				}
			}
			this.CurrentProviderFactory.Authorize(ctx, this, identities);
		}

		private static List<IAuthorizationIdentity> GetAuthorizationIdenties(ServiceContext ctx) {
			var method = ctx.CallMethod;
			if(method == null) {
				return null;
			}
			var interfaceType = method.DeclaringType;
			var methodName = method.ToString();
			var index = methodName.IndexOf(' ') + 1;
			var methodInterfaceName = methodName.Substring(0, index) + interfaceType.FullName + "." + methodName.Substring(index);
			var list = new List<IAuthorizationIdentity>();
			var objectType = ctx.ObjectInstance.GetType();
			var interfaceMethod = objectType.GetInterfaceMap(interfaceType).TargetMethods.Where(m => { var n = m.ToString(); return n == methodInterfaceName || n == methodName; }).Single();
			var attributes = interfaceMethod.GetCustomAttributes(typeof(AuthorizeAttribute), true) as AuthorizeAttribute[];
			if(attributes != null && attributes.Length > 0) {
				list.AddRange(attributes.OrderBy(k => k.OrderIndex).ToArray());
			}
			attributes = objectType.GetCustomAttributes(typeof(AuthorizeAttribute), true) as AuthorizeAttribute[];
			if(attributes != null && attributes.Length > 0) {
				list.AddRange(attributes.OrderBy(k => k.OrderIndex).ToArray());
			}

			attributes = method.GetCustomAttributes(typeof(AuthorizeAttribute), true) as AuthorizeAttribute[];
			if(attributes != null && attributes.Length > 0) {
				list.AddRange(attributes.OrderBy(k => k.OrderIndex).ToArray());
			}
			attributes = interfaceType.GetCustomAttributes(typeof(AuthorizeAttribute), true) as AuthorizeAttribute[];
			if(attributes != null && attributes.Length > 0) {
				list.AddRange(attributes.OrderBy(k => k.OrderIndex).ToArray());
			}
			return list;
		}
	}
}
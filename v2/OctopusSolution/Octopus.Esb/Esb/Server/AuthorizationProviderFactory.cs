using System.Collections.Generic;
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
		protected IAuthorizationProviderFactory CurrentProviderFactory => this.providerFactory ?? (this.providerFactory = this.ObjectService.GetObject<IAuthorizationProviderFactory>(this.Setting.DefaultFactory) ?? this);

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

		private void Authorize(ServiceContext ctx) {
			var identities = ActionFilterAttribute.GetTypedFilters<IAuthorizationIdentity>(ctx);
			this.CurrentProviderFactory.Authorize(ctx, this, identities);
		}
	}
}
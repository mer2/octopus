using HTB.DevFx.Core;
using Octopus.Esb.Server;

namespace Octopus.Security
{
	public abstract class AuthorizationProviderBase : IAuthorizationProvider
	{
		public virtual bool Authorize(ServiceContext ctx, IAuthorizationIdentity identity) {
			var authorized = this.AuthorizeInternal(ctx, identity);
			if(!authorized) {
				ctx.ResultInitialized = true;
				ctx.ResultValue = AOPResult.Failed("授权失败");
			}
			return authorized;
		}

		protected abstract bool AuthorizeInternal(ServiceContext ctx, IAuthorizationIdentity identity);
	}
}

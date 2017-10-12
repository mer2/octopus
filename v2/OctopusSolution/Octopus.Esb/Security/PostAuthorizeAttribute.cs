using System;
using Octopus.Esb.Server;

namespace Octopus.Security
{
	[Serializable]
	public class PostAuthorizeAttribute : AuthorizeAttribute, IAuthorizationProvider
	{
		public const string AuthorizeCategory = "PostAuthenticationProvider";
		public PostAuthorizeAttribute() : this("?") {
		}

		public PostAuthorizeAttribute(string user) : base(user) {
			this.Category = AuthorizeCategory;
		}

		bool IAuthorizationProvider.Authorize(ServiceContext ctx, IAuthorizationIdentity identity) {
			return Authorize(ctx, identity);
		}

		public static bool Authorize(ServiceContext ctx, IAuthorizationIdentity identity) {
#if !DEBUG
			if(ctx.HttpContext.Request.Method != "POST") {
				ctx.ResultInitialized = true;
				ctx.ResultValue = HTB.DevFx.Core.AOPResult.Failed(-405, "非法请求");
				return false;
			}
#endif
			return true;
		}
	}
}

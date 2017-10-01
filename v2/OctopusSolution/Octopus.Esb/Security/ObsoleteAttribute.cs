using System;
using HTB.DevFx.Core;
using Octopus.Esb.Server;

namespace Octopus.Security
{
	[Serializable]
	public class ObsoleteAttribute : AuthorizeAttribute, IAuthorizationProvider
	{
		public const string AuthorizeCategory = "ObsoleteAuthenticationProvider";
		public ObsoleteAttribute() : base("?") {
			this.Category = AuthorizeCategory;
		}
		public ObsoleteAttribute(string message) : this() {
			this.Message = message;
		}

		public string Message { get; private set; }

		public bool Authorize(ServiceContext ctx, IAuthorizationIdentity identity) {
			string msg = null;
			var oa = (ObsoleteAttribute)identity;
			if(oa != null) {
				msg = oa.Message;
			}
			if(string.IsNullOrEmpty(msg)) {
				msg = "此方法已废弃，请更新客户端";
			}
			ctx.ResultInitialized = true;
			ctx.ResultValue = AOPResult.Failed(-304, msg);
			return false;
		}
	}
}

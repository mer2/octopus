using System;
using System.Web;
using HTB.DevFx;

namespace Octopus.SecurityPermissions.Web
{
	internal class PermissionHttpModule : IHttpModule
	{
		public virtual void Init(HttpApplication context) {
			this.InitInternal(context);
		}

		public virtual void Dispose() {
		}

		protected IPermissionClientService PermissionClientService;
		protected virtual void InitInternal(HttpApplication context) {
			this.PermissionClientService = ObjectService.GetObject<IPermissionClientService>();
			context.Error += this.OnError;
		}

		protected virtual void OnError(object sender, EventArgs e) {
			this.PermissionClientService.OnError(sender, e);
		}
	}
}

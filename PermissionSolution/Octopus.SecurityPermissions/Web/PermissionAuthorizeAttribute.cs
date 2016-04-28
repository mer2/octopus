using System;
using System.Collections.Generic;
using System.Web.Mvc;
using HTB.DevFx;

namespace Octopus.SecurityPermissions.Web
{
	[Serializable]
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
	public class PermissionAuthorizeAttribute : AuthorizeAttribute, IPermissionObject
	{
		public PermissionAuthorizeAttribute() {
			this.Enabled = true;
		}

		public PermissionAuthorizeAttribute(string permissionNo) : this() {
			this.PermissionNo = permissionNo;
		}

		protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext) {
			var authorized = base.AuthorizeCore(httpContext);
			if(authorized) {
				var filterContext = (AuthorizationContext)httpContext.Items[typeof(AuthorizationContext)];
				var actionDescriptor = filterContext.ActionDescriptor;
				var ps = actionDescriptor.GetCustomAttributes(typeof(PermissionAttribute), true) as PermissionAttribute[];
				var list = new List<IPermissionObject> { this };
				if(ps != null && ps.Length > 0) {
					list.AddRange(ps);
				}
				ps = actionDescriptor.ControllerDescriptor.GetCustomAttributes(typeof(PermissionAttribute), true) as PermissionAttribute[];
				if(ps != null && ps.Length > 0) {
					list.AddRange(ps);
				}
				var svr = ObjectService.GetObject<IPermissionClientService>();
				authorized = svr.Authorize(false, list.ToArray());
			}
			return authorized;
		}

		public override void OnAuthorization(AuthorizationContext filterContext) {
			filterContext.HttpContext.Items[typeof(AuthorizationContext)] = filterContext;
			base.OnAuthorization(filterContext);
		}

		protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext) {
			base.HandleUnauthorizedRequest(filterContext);
			if(filterContext.HttpContext.User.Identity.IsAuthenticated) {//用户验证了，那么是权限不够
				filterContext.Result = PermissionClientService.GetUnauthorizedResult();
			}
		}

		public string PermissionNo { get; set; }
		public string PermissionValue { get; set; }
		public string AppNo { get; set; }
		public bool Enabled { get; set; }
	}
}

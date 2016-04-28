using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using HTB.DevFx.Cache;
using HTB.DevFx.Core;
using HTB.DevFx.Utils;
using Octopus.SecurityPermissions.Config;

namespace Octopus.SecurityPermissions
{
	public class PermissionClientService : ServiceBase<PermissionClientServiceSetting>, IPermissionClientService
	{
		/// <summary>
		/// 转换用户，用于获取权限
		/// </summary>
		/// <param name="userName">原始用户名</param>
		/// <returns>转换后的用户名</returns>
		protected virtual string MapUser(string userName) {
			return userName;
		}

		public virtual bool Authorize(bool throwOnFailed, params IPermissionObject[] permissions) {
			var ctx = HttpContext.Current;
			var identity = ctx != null ? ctx.User.Identity : Thread.CurrentPrincipal.Identity;
			var authorized = identity.IsAuthenticated;
			if(authorized) {
				authorized = this.AuthorizeInternal(identity.Name, permissions);
				if(!authorized && throwOnFailed) {
					throw new AuthorizationException();
				}
			} else {
				if(throwOnFailed) {
					throw new AuthenticationException();
				}
			}
			return authorized;
		}

		PermissionObject[] IPermissionClientService.GetUserPermissions(string userName, string permissionNo, bool withPermissionResource) {
			var list = this.GetCachedUserPermissions(userName);
			if (list != null && list.Length > 0 && !string.IsNullOrEmpty(permissionNo)) {
				list = list.Where(x => string.Compare(x.PermissionNo, permissionNo, StringComparison.InvariantCultureIgnoreCase) == 0).ToArray();
			}
			if(list == null || list.Length <= 0) {
				return list;
			}
			if(withPermissionResource) {//需要获取权限实体
				var resources = this.GetCachedPermissionResources();
				foreach (var p in list) {
					if(p.PermissionResource == null) {
						p.PermissionResource = resources.SingleOrDefault(x => x.PermissionNo == p.PermissionNo);
					}
				}
			}
			return list;
		}

		PermissionResource[] IPermissionClientService.GetPermissionResources() {
			return this.GetCachedPermissionResources();
		}

		PermissionObject[] IPermissionClientService.GetPermissionsByPermissionNo(string permissionNo) {
			return this.GetCachedPermissionsByPermissionNo(permissionNo);
		}

		public void OnError(object sender, EventArgs e) {
			var app = (HttpApplication)sender;
			var error = app.Server.GetLastError();
			while(error != null) {
				if(error is AuthenticationException) {//没有认证，自动转向到登录页面
					app.Server.ClearError();
					app.Response.StatusCode = 401;
					break;
				}
				if(error is AuthorizationException) {//权限被拒绝
					app.Server.ClearError();
					var url = this.Setting.ErrorUrl;
					if(!string.IsNullOrEmpty(url)) {//如果配置了错误页面，则转向到此页面
						url = string.Format(url, 403, app.Server.UrlEncode(app.Request.Url.ToString()));
						app.Response.Redirect(url);
					} else {//否则简单的输出403
						app.Response.StatusCode = 403;
					}
					break;
				}
				error = error.InnerException;
			}
		}

		string IPermissionClientService.HandleUnauthorized(HttpContext ctx, bool redirect, bool endResponse) {
			if(ctx == null) {
				ctx = HttpContext.Current;
			}
			if(ctx == null) {//非Web环境
				throw new AuthorizationException();
			}
			var url = this.Setting.ErrorUrl;
			if(!string.IsNullOrEmpty(url)) {//如果配置了错误页面，则转向到此页面
				url = string.Format(url, 403, ctx.Server.UrlEncode(ctx.Request.Url.ToString()));
				if(redirect) {
					ctx.Response.Redirect(url);
				}
			} else {//否则简单的输出403
				ctx.Response.StatusCode = 403;
			}
			if(endResponse) {
				ctx.Response.End();
			}
			return url;
		}

		internal class PermissionObjectComparer : EqualityComparer<PermissionObject>
		{
			public override bool Equals(PermissionObject x, PermissionObject y) {
				return IsEquals(x, y);
			}

			public override int GetHashCode(PermissionObject obj) {
				return 0;
			}

			public static bool IsEquals(PermissionObject x, PermissionObject y) {
				if(x.Enabled && y.Enabled && x.PermissionNo == y.PermissionNo && x.AppNo == y.AppNo && x.Validating != y.Validating) {
					var validatePermission = x.Validating ? x : y;
					var ownerPermission = x.Validating ? y : x;
					if(validatePermission == ownerPermission) {
						return false;
					}
					if(!string.IsNullOrEmpty(validatePermission.PermissionValue)) {//需要判断权限范围
						if(validatePermission.PermissionValue != ownerPermission.PermissionValue) {
							return false;
						}
					}
					return ownerPermission.Granted;
				}
				return false;
			}
		}

		internal bool AuthorizeInternal(string userName, IPermissionObject[] permissions) {
			var validatingList = new List<PermissionObject>();
			if(permissions != null && permissions.Length > 0) {//查找所有完整的权限对象
				foreach(var per in permissions) {
					if(!string.IsNullOrEmpty(per.PermissionNo) && per.Enabled) {
						var appNo = string.IsNullOrEmpty(per.AppNo) ? this.Setting.AppNo : per.AppNo;
						var p = new PermissionObject { AppNo = appNo, PermissionNo = per.PermissionNo, PermissionValue = per.PermissionValue, Enabled = per.Enabled, Validating = true };
						validatingList.Add(p);
					}
				}
			}
			if(validatingList.Count <= 0) {//没找到，默认执行
				return true;
			}
			var list = this.GetCachedUserPermissions(userName);
			if(list == null || list.Length <= 0) {
				return false;
			}
			foreach (var item0 in validatingList) {
				var exists = false;
				foreach (var item1 in list) {
					if (PermissionObjectComparer.IsEquals(item0, item1)) {
						exists = true;
						break;
					}
				}
				if(!exists) {
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// 授权认证
		/// </summary>
		/// <param name="permissionNo">权限编码</param>
		/// <param name="permissionValue">权限值</param>
		/// <param name="appNo">应用编码</param>
		/// <param name="includeStack">是否从调用堆栈获取权限</param>
		/// <param name="throwOnFailed">认证失败时是否抛出异常</param>
		/// <returns>是否认证成功</returns>
		public static bool Authorize(string permissionNo = null, string permissionValue = null, string appNo = null, bool includeStack = true, bool throwOnFailed = false) {
			return Authorize(new PermissionObject { PermissionNo = permissionNo, PermissionValue = permissionValue, AppNo = appNo, Enabled = true }, includeStack, throwOnFailed);
		}

		public static bool Authorize(IPermissionObject permission, bool includeStack = true, bool throwOnFailed = false) {
			var list = includeStack ? GetPermissionsFromStack() : new List<IPermissionObject>();
			if (permission != null) {
				list.Insert(0, permission);
			}
			var svr = HTB.DevFx.ObjectService.GetObject<IPermissionClientService>();
			return svr == null || svr.Authorize(throwOnFailed, list.ToArray());
		}

		public static PermissionObject[] GetUserPermissions(string permissionNo = null, string userName = null, bool withPermissionResource = false) {
			userName = GetCurrentUserName(userName);
			if(string.IsNullOrEmpty(userName)) {
				return null;
			}
			var svr = HTB.DevFx.ObjectService.GetObject<IPermissionClientService>();
			var list = svr.GetUserPermissions(userName, permissionNo, withPermissionResource);
			return list;
		}

		public static PermissionResource[] GetViewablePermissionResources(string appNo = null, string userName = null) {
			if (string.IsNullOrEmpty(appNo)) {
				var svr = (PermissionClientService)HTB.DevFx.ObjectService.GetObject<IPermissionClientService>();
				appNo = svr.Setting.AppNo;
			}
			
			var permissions = GetUserPermissions(userName: userName, withPermissionResource: true);
			var list = permissions.Where(x => x.PermissionResource != null).Select(x => x.PermissionResource).Where(x => x.Viewable && x.Enabled);
			if (!string.IsNullOrEmpty(appNo)) {
				list = list.Where(x => x.AppNo == appNo);
			}
			list = list.Distinct().OrderBy(x => x.DispIndex);
			return list.ToArray();
		}

		public static PermissionResource[] GetPermissionResources() {
			var svr = HTB.DevFx.ObjectService.GetObject<IPermissionClientService>();
			return svr.GetPermissionResources();
		}

		public static PermissionObject[] GetPermissionsByPermissionNo(string permissionNo) {
			var svr = HTB.DevFx.ObjectService.GetObject<IPermissionClientService>();
			return svr.GetPermissionsByPermissionNo(permissionNo);
		}

		private static string GetCurrentUserName(string userName) {
			if(string.IsNullOrEmpty(userName)) {
				var ctx = HttpContext.Current;
				var identity = ctx != null ? ctx.User.Identity : Thread.CurrentPrincipal.Identity;
				if(identity.IsAuthenticated) {
					userName = identity.Name;
				}
			}
			return userName;
		}

		private static List<IPermissionObject> GetPermissionsFromStack() {
			var list = new List<IPermissionObject>();
			var ps = TypeHelper.GetAttributeFromRuntimeStack<PermissionAttribute>(true);
			if(ps != null && ps.Length > 0) {
				list.AddRange(ps);
			}
			return list;
		}

		public static string HandleUnauthorized(HttpContext ctx = null, bool redirect = true, bool endResponse = true) {
			var svr = HTB.DevFx.ObjectService.GetObject<IPermissionClientService>();
			return svr.HandleUnauthorized(ctx, redirect, endResponse);
		}

		internal static void SortPermissionResources(int index, int layer, List<PermissionResource> list) {
			var current = list[index];
			if(!string.IsNullOrEmpty(current.ParentNo)) {
				SortPermissionResources(index, layer, list);
			}
		}

		internal static void InsertParentBeforeCurrent(string parentNo, ref int index, ref int layer, List<PermissionResource> list) {
			if(string.IsNullOrEmpty(parentNo)) {
				return;
			}
			var parent = list.SingleOrDefault(x => x.PermissionNo.StringEquals(parentNo));
			if(parent == null) {
				return;
			}
			InsertParentBeforeCurrent(parent.ParentNo, ref index, ref layer, list);
			list.Remove(parent);
			list.Insert(index, parent);
		}

		public static ActionResult GetUnauthorizedResult(HttpContext ctx = null) {
			var url = HandleUnauthorized(ctx, false, false);
			return string.IsNullOrEmpty(url) ? (ActionResult) new HttpStatusCodeResult(403) : new RedirectResult(url);
		}

		protected virtual PermissionObject[] GetCachedUserPermissions(string userName) {
			var mappingName = this.MapUser(userName);
			if(string.IsNullOrEmpty(mappingName)) {
				return null;
			}
			if(this.Setting.Cached) {
				return this.PermissionCache.GetObjectFromCache("GetUserPermissions:" + mappingName, () => this.GetUserPermissionsFromServer(mappingName, userName), CacheDependency.Create(DateTime.Now.AddSeconds(this.Setting.BufferTimeout)));
			}
			return this.GetUserPermissionsFromServer(mappingName, userName);
		}

		protected virtual PermissionObject[] GetUserPermissionsFromServer(string mappingName, string userName) {
			return this.PermissionService.GetPermissions(mappingName);
		}

		private PermissionResource[] GetCachedPermissionResources() {
			if(this.Setting.Cached) {
				return this.PermissionCache.GetObjectFromCache("GetPermissionResources", () => this.PermissionService.GetPermissionResourcesByAppNo(this.Setting.AppNo), CacheDependency.Create(DateTime.Now.AddSeconds(this.Setting.BufferTimeout)));
			}
			return this.PermissionService.GetPermissionResourcesByAppNo(this.Setting.AppNo);
		}

		private PermissionObject[] GetCachedPermissionsByPermissionNo(string permissionNo) {
			if(this.Setting.Cached) {
				return this.PermissionCache.GetObjectFromCache("GetCachedPermissionsByPermissionNo:" + permissionNo, () => this.PermissionService.GetPermissionsByPermissionNo(permissionNo), CacheDependency.Create(DateTime.Now.AddSeconds(this.Setting.BufferTimeout)));
			}
			return this.PermissionService.GetPermissionsByPermissionNo(permissionNo);
		}

		private IPermissionService PermissionService;
		private ICache PermissionCache;
		protected override void OnInit() {
			base.OnInit();
			this.PermissionService = this.ObjectService.GetObject<IPermissionService>();
			this.PermissionCache = CacheService.GetCache(this.Setting.CacheName);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using HTB.DevFx;
using HTB.DevFx.Core;
using HTB.DevFx.Data.Entities;
using Octopus.SecurityPermissions;
using Octopus.SecurityPermissions.Server;

namespace Octopus.Admin.Web.Controllers
{
	[SecurityAuthorize]
	public class PermissionsController : Controller
	{
		internal IPermissionServerService PermissionServer {
			get { return ObjectService.GetObject<IPermissionServerService>(); }
		}

		[HttpGet, Permission("Admin.PermissionResourceList")]
		public ActionResult ResourceList(string permissionNo, string permissionTitle, string parentNo, int? startIndex) {
			const int pageSize = 20;
			var index = startIndex != null ? startIndex.Value : 0;
			this.ViewBag.StartIndex = index;
			this.ViewBag.PageSize = pageSize;
			this.ViewBag.PermissionNo = permissionNo;
			this.ViewBag.ParentNo = parentNo;
			this.ViewBag.PermissionTitle = permissionTitle;
			
			var permissions = this.PermissionServer.GetPermissionResources();
			IEnumerable<PermissionResource> list = permissions;
			if(!string.IsNullOrEmpty(permissionNo)) {
				list = list.Where(x => x.PermissionNo.Contains(permissionNo));
			}
			if(!string.IsNullOrEmpty(permissionTitle)) {
				list = list.Where(x => x.Title.Contains(permissionTitle));
			}
			if(!string.IsNullOrEmpty(parentNo)) {
				list = list.Where(x => x.ParentNo == parentNo);
			}
			var count = list.Count();
			list = list.Skip(index).Take(pageSize);
			var result = new PaginateResult<PermissionResource>{ Count = count, Items = list.ToArray() };
			return this.View(result);
		}

		[HttpGet, Permission("Admin.PermissionResourceEdit")]
		public ActionResult ResourceEdit(string id) {
			PermissionResource model;
			if(!string.IsNullOrEmpty(id)) {
				var permissions = this.PermissionServer.GetPermissionResources();
				model = permissions.SingleOrDefault(x => x.PermissionNo == id);
				if(model == null) {
					return this.Content("权限资源不存在");
				}
			} else {
				model = new PermissionResource { PermissionNo = id, Enabled = true };
			}
			return this.View(model);
		}

		[HttpPost, Permission("Admin.PermissionResourceEdit")]
		public ActionResult ResourceEdit(string id, PermissionResource resource) {
			this.ViewBag.Result = string.IsNullOrEmpty(id) ? this.PermissionServer.AddPermissionResource(resource) : this.PermissionServer.UpdatePermissionResource(resource);
			return this.ResourceEdit(id);
		}

		[HttpPost, Permission("Admin.PermissionResourceDelete")]
		public ActionResult ResourceDelete(string id) {
			var result = this.PermissionServer.DeletePermissionResource(id);
			return this.Json(new { result.ResultNo, result.ResultDescription });
		}

		[HttpGet, Permission("Admin.PermissionRoleList")]
		public ActionResult RoleList(string roleNo, string roleTitle, int? startIndex) {
			const int pageSize = 20;
			var index = startIndex != null ? startIndex.Value : 0;
			this.ViewBag.StartIndex = index;
			this.ViewBag.PageSize = pageSize;
			this.ViewBag.RoleNo = roleNo;
			this.ViewBag.RoleTitle = roleTitle;

			var roles = this.PermissionServer.GetPermissionRoles();
			IEnumerable<PermissionRole> list = roles;
			if(!string.IsNullOrEmpty(roleNo)) {
				list = list.Where(x => x.RoleNo.Contains(roleNo));
			}
			if(!string.IsNullOrEmpty(roleTitle)) {
				list = list.Where(x => x.Title.Contains(roleTitle));
			}
			var count = list.Count();
			list = list.Skip(index).Take(pageSize);
			var result = new PaginateResult<PermissionRole> { Count = count, Items = list.ToArray() };
			return this.View(result);
		}

		[HttpPost, Permission("Admin.PermissionRoleDelete")]
		public ActionResult RoleDelete(string id) {
			var result = this.PermissionServer.DeletePermissionRole(id);
			return this.Json(new { result.ResultNo, result.ResultDescription });
		}

		[HttpGet, Permission("Admin.PermissionRoleEdit")]
		public ActionResult RoleEdit(string id) {
			PermissionRole model;
			if(!string.IsNullOrEmpty(id)) {
				var permissions = this.PermissionServer.GetPermissionRoles();
				model = permissions.SingleOrDefault(x => x.RoleNo == id);
				if(model == null) {
					return this.Content("权限角色不存在");
				}
			} else {
				model = new PermissionRole { RoleNo = id, Enabled = true };
			}
			return this.View(model);
		}

		[HttpPost, Permission("Admin.PermissionRoleEdit")]
		public ActionResult RoleEdit(string id, PermissionRole model) {
			this.ViewBag.Result = string.IsNullOrEmpty(id) ? this.PermissionServer.AddPermissionRole(model) : this.PermissionServer.UpdatePermissionRole(model);
			return this.RoleEdit(id);
		}

		[HttpGet, Permission("Admin.PermissionUserRoleList")]
		public ActionResult UserRoleList(string userName, string roleNo, int? startIndex) {
			const int pageSize = 20;
			var index = startIndex != null ? startIndex.Value : 0;
			this.ViewBag.StartIndex = index;
			this.ViewBag.PageSize = pageSize;
			this.ViewBag.UserName = userName;
			this.ViewBag.RoleNo = roleNo;

			var items = this.PermissionServer.GetUserGroupRoles();
			var list = items.Where(x => x.TargetCategory == 2);
			if(!string.IsNullOrEmpty(userName)) {
				list = list.Where(x => string.Compare(x.UserName, userName, StringComparison.InvariantCultureIgnoreCase) == 0);
			}
			if(!string.IsNullOrEmpty(roleNo)) {
				list = list.Where(x => string.Compare(x.TargetValue, roleNo, StringComparison.InvariantCultureIgnoreCase) == 0);
			}
			var count = list.Count();
			list = list.OrderByDescending(x => x.ID) .Skip(index).Take(pageSize);
			var result = new PaginateResult<UserGroupRole> { Count = count, Items = list.ToArray() };
			return this.View(result);
		}

		[HttpPost, Permission("Admin.PermissionUserRoleDelete")]
		public ActionResult UserRoleDelete(int id) {
			IAOPResult result = null;
			if(!PermissionClientService.Authorize("Admin.PermissionUserRoleDeleteAll", includeStack: false)) { //没有对所有角色操作的权限，则需要限制为当前用户拥有的角色
				var userRoles = this.PermissionServer.GetUserGroupRoles();
				var roleNo = userRoles.Where(x => x.ID == id).Select(x => x.TargetValue).SingleOrDefault();
				//判断是否是当前用户可管理的角色
				var roles = PermissionClientService.GetUserPermissions("Admin.PermissionUserManageRole");
				var exists = roles.Any(x => x.PermissionValue.StringEquals(roleNo));
				if(!exists) {
					result = AOPResult.Failed("Access Denied");
				}
			}
			if(result == null) {
				result = this.PermissionServer.DeleteUserRole(id);
			}
			return this.Json(new { result.ResultNo, result.ResultDescription });
		}
		
		internal ActionResult UserRoleCreateInternal(UserGroupRole model = null) {
			this.ViewBag.Roles = this.PermissionServer.GetPermissionRoles();
			if(model == null) {
				model = new UserGroupRole { Enabled = true };
			}
			return this.View(model);
		}

		internal bool RoleAllow(string permissionAll, string roleNo) {
			if(!PermissionClientService.Authorize(permissionAll, includeStack: false)) {//没有对所有角色操作的权限，则需要限制为当前用户可管理的角色
				//判断是否是当前用户可管理的角色
				var roles = PermissionClientService.GetUserPermissions("Admin.PermissionUserManageRole");
				return roles.Any(x => x.PermissionValue.StringEquals(roleNo));
			}
			return true;
		}

		[HttpGet, Permission("Admin.PermissionUserRoleCreate")]
		public ActionResult UserRoleCreate() {
			return this.UserRoleCreateInternal();
		}

		[HttpPost, Permission("Admin.PermissionUserRoleCreate")]
		public ActionResult UserRoleCreate(UserGroupRole model) {
			model.TargetCategory = 2;
			this.ViewBag.Result = this.RoleAllow("Admin.PermissionUserRoleCreateAll", model.TargetValue) ? this.PermissionServer.AddUserRole(model) : AOPResult.Failed("Access Denied");
			return this.UserRoleCreateInternal(model);
		}

		[HttpGet, Permission("Admin.PermissionRelationList")]
		public ActionResult RelationList(int? targetObject, string targetValue, string permissionTitle, string permissionNo, string permissionValue, int? startIndex) {
			var items = this.PermissionServer.GetPermissionRelations(true);
			return this.RelationListInternal(items, targetObject, targetValue, permissionTitle, permissionNo, permissionValue, startIndex);
		}

		internal ActionResult RelationListInternal(PermissionRelation[] items, int? targetObject, string targetValue, string permissionTitle, string permissionNo, string permissionValue, int? startIndex) {
			const int pageSize = 30;
			var index = startIndex != null ? startIndex.Value : 0;
			this.ViewBag.StartIndex = index;
			this.ViewBag.PageSize = pageSize;
			this.ViewBag.TargetObject = targetObject;
			this.ViewBag.TargetValue = targetValue;
			this.ViewBag.PermissionTitle = permissionTitle;
			this.ViewBag.PermissionNo = permissionNo;
			this.ViewBag.PermissionValue = permissionValue;

			IEnumerable<PermissionRelation> list = items;
			if(targetObject != null) {
				list = list.Where(x => x.TargetObject == targetObject.Value);
			}
			if(!string.IsNullOrEmpty(targetValue)) {
				targetValue = targetValue.ToLowerInvariant();
				list = list.Where(x => x.TargetValue.ToLowerInvariant().Contains(targetValue));
			}
			if(!string.IsNullOrEmpty(permissionTitle)) {
				permissionTitle = permissionTitle.ToLowerInvariant();
				list = list.Where(x => x.PermissionResource != null && x.PermissionResource.Title.ToLowerInvariant().Contains(permissionTitle));
			}
			if(!string.IsNullOrEmpty(permissionNo)) {
				permissionNo = permissionNo.ToLowerInvariant();
				list = list.Where(x => x.PermissionNo.ToLowerInvariant().Contains(permissionNo));
			}
			if(!string.IsNullOrEmpty(permissionValue)) {
				list = list.Where(x => x.PermissionValue == permissionValue);
			}
			var count = list.Count();
			list = list.Skip(index).Take(pageSize);
			var result = new PaginateResult<PermissionRelation> { Count = count, Items = list.ToArray() };
			return this.View(result);
		}

		[HttpGet, Permission("Admin.PermissionUserRelationList")]
		public ActionResult UserRelationList(string userName, int? targetObject, string targetValue, string permissionTitle, string permissionNo, string permissionValue, int? startIndex) {
			if(string.IsNullOrEmpty(userName)) {
				userName = this.User.Identity.Name;
			}
			this.ViewBag.UserName = userName;
			var items = this.PermissionServer.GetUserPermissionRelations(userName, true);
			return this.RelationListInternal(items, targetObject, targetValue, permissionTitle, permissionNo, permissionValue, startIndex);
		}

		internal ActionResult RelationCreateInternal(PermissionRelation model = null) {
			this.ViewBag.PermissionResources = this.PermissionServer.GetPermissionResources();
			this.ViewBag.Roles = this.PermissionServer.GetPermissionRoles();
			if(model == null) {
				model = new PermissionRelation { Enabled = true, Granted = true };
			}
			return this.View(model);
		}

		[HttpGet, Permission("Admin.PermissionRelationCreate")]
		public ActionResult RelationCreate() {
			return this.RelationCreateInternal();
		}

		[HttpPost, Permission("Admin.PermissionRelationCreate")]
		public ActionResult RelationCreate(PermissionRelation model) {
			var allowed = true;
			var permission = PermissionClientService.GetUserPermissions(model.PermissionNo).FirstOrDefault();
			if (permission == null || !permission.Grantable) {//当前用户没有此权限或不可转授，需要进一步判断
				allowed = PermissionClientService.Authorize("Admin.PermissionRelationCreateAll", includeStack: false);
			}
			if(allowed) {
				//判断用户是否可对用户授权或对角色授权
				allowed = PermissionClientService.Authorize(model.TargetObject == 1 ? "Admin.PermissionRelationCreateForUser" : "Admin.PermissionRelationCreateForRole", includeStack: false);
			}

			this.ViewBag.Result = allowed ? this.PermissionServer.AddPermissionRelation(model) : AOPResult.Failed("Access Denied");
			return this.RelationCreateInternal(model);
		}

		[HttpPost, Permission("Admin.PermissionRelationDelete")]
		public ActionResult RelationDelete(int id) {
			var model = this.PermissionServer.GetPermissionRelation(id);
			var result = !this.PermissionAllow(model.PermissionNo, "Admin.PermissionRelationDeleteAll", false) ? AOPResult.Failed("Access Denied") : this.PermissionServer.DeletePermissionRelation(id);

			return this.Json(new { result.ResultNo, result.ResultDescription });
		}

		internal ActionResult RelationEditInternal(int id, PermissionRelation model = null) {
			if(model == null) {
				model = this.PermissionServer.GetPermissionRelation(id, true);
			}
			return this.View(model);
		}

		internal bool PermissionAllow(string permissionNo, string permissionAll, bool throwOnFailed) {
			var allowed = PermissionClientService.Authorize(permissionNo, includeStack: false);
			if(!allowed) {//不是自己拥有的权限，需要进一步判断是否可以操作
				allowed = PermissionClientService.Authorize(permissionAll, includeStack: false, throwOnFailed: throwOnFailed);
			}
			return allowed;
		}

		[HttpGet, Permission("Admin.PermissionRelationEdit")]
		public ActionResult RelationEdit(int id) {
			return this.RelationEditInternal(id);
		}

		[HttpPost, Permission("Admin.PermissionRelationEdit")]
		public ActionResult RelationEdit(int id, PermissionRelation model) {
			var model0 = this.PermissionServer.GetPermissionRelation(id, true);
			model.ID = id;
			model.PermissionResource = model0.PermissionResource;
	
			var result = this.PermissionAllow(model0.PermissionNo, "Admin.PermissionRelationEditAll", false) ? this.PermissionServer.UpdatePermissionRelation(model) : AOPResult.Failed("Access Denied");
			this.ViewBag.Result = result;
			return this.RelationEditInternal(id, model);
		}

		[HttpGet, Permission("Admin.PermissionLogList")]
		public ActionResult LogList(string userName, DateTime? startTime, DateTime? endTime, int? startIndex) {
			const int pageSize = 30;
			var index = startIndex != null ? startIndex.Value : 0;
			this.ViewBag.StartIndex = index;
			this.ViewBag.PageSize = pageSize;
			this.ViewBag.UserName = userName;
			this.ViewBag.StartTime = startTime != null ? startTime.Value.ToString("yyyy-MM-dd") : null;
			this.ViewBag.EndTime = endTime != null ? endTime.Value.ToString("yyyy-MM-dd") : null;

			var items = this.PermissionServer.GetPermissionLogs(userName, startTime, endTime, index, pageSize);
			return this.View(items);
		}
	}
}
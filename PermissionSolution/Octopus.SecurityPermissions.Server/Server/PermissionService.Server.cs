using System;
using System.Linq;
using System.Web;
using HTB.DevFx.Cache;
using HTB.DevFx.Core;
using HTB.DevFx.Data.Entities;
using Joy.Security;
using Newtonsoft.Json;

namespace Octopus.SecurityPermissions.Server
{
	partial class PermissionService : IPermissionServerService
	{
		public IAOPResult AddPermissionResource(PermissionResource resource) {
			this.PermissionDataService.AddPermissionResource(resource, this.Setting.DefaultRole);
			
			this.WritePermissionLog(string.Format("添加了权限资源：[{0}]{1}", resource.PermissionNo, resource.Title), resource);
	
			this.SetCachedPermissionResourcesExpired();
			this.SetCachedPermissionRelationsExpired();//增加权限会默认添加到管理员角色
			return AOPResult.Success();
		}

		public IAOPResult UpdatePermissionResource(PermissionResource resource) {
			this.PermissionDataService.UpdatePermissionResource(resource);
			
			this.WritePermissionLog(string.Format("更新了权限资源：[{0}]{1}", resource.PermissionNo, resource.Title), resource);
	
			this.SetCachedPermissionResourcesExpired();
			return AOPResult.Success();
		}

		public IAOPResult DeletePermissionResource(string id) {
			this.PermissionDataService.DeletePermissionResource(id);

			var items = this.GetCachedPermissionResources();
			var item = items.SingleOrDefault(x => string.Compare(x.PermissionNo, id, StringComparison.InvariantCultureIgnoreCase) == 0);
			this.WritePermissionLog(string.Format("删除了权限资源：[{0}]{1}", id, item != null ? item.Title : null), item);

			this.SetCachedPermissionResourcesExpired();
			this.SetCachedPermissionRelationsExpired();
			return AOPResult.Success();
		}

		public PermissionRole[] GetPermissionRoles() {
			return this.GetCachedPermissionRoles();
		}

		public IAOPResult DeletePermissionRole(string roleNo) {
			this.PermissionDataService.DeletePermissionRole(roleNo);

			var items = this.GetCachedPermissionRoles();
			var item = items.SingleOrDefault(x => string.Compare(x.RoleNo, roleNo, StringComparison.InvariantCultureIgnoreCase) == 0);
			this.WritePermissionLog(string.Format("删除了角色：[{0}]{1}", roleNo, item != null ? item.Title : null), item);

			this.SetCachedPermissionRolesExpired();
			//角色和用户的关系需要被更新
			this.SetCachedUserGroupRolesExpired();
			//角色和权限的关系需要被更新
			this.SetCachedPermissionRelationsExpired();
			//TODO:角色和用户组的关系需要被更新
			return AOPResult.Success();
		}

		public IAOPResult AddPermissionRole(PermissionRole role) {
			this.PermissionDataService.AddPermissionRole(role);

			this.WritePermissionLog(string.Format("添加了角色：[{0}]{1}", role.RoleNo, role.Title), role);

			this.SetCachedPermissionRolesExpired();
			return AOPResult.Success();
		}

		public IAOPResult UpdatePermissionRole(PermissionRole role) {
			this.PermissionDataService.UpdatePermissionRole(role);
	
			this.WritePermissionLog(string.Format("更新了角色：[{0}]{1}", role.RoleNo, role.Title), role);
	
			this.SetCachedPermissionRolesExpired();
			return AOPResult.Success();
		}

		public UserGroupRole[] GetUserGroupRoles() {
			return this.GetCachedUserGroupRoles();
		}

		public IAOPResult DeleteUserRole(int id) {
			this.PermissionDataService.DeleteUserGroupRole(id);

			var items = this.GetCachedUserGroupRoles();
			var item = items.SingleOrDefault(x => x.ID == id);
			this.WritePermissionLog(string.Format("删除了用户角色关系：[{0}]{1}/{2}", id, item != null ? item.UserName : null, item != null ? item.TargetValue : null), item);

			this.SetCachedUserGroupRolesExpired();
			return AOPResult.Success();
		}

		public IAOPResult AddUserRole(UserGroupRole item) {
			item.TargetCategory = 2;
			var items = this.GetCachedUserGroupRoles();
			var exists = items.Any(x => x.TargetCategory == item.TargetCategory && string.Compare(x.UserName, item.UserName, StringComparison.InvariantCultureIgnoreCase) == 0 && string.Compare(x.TargetValue, item.TargetValue, StringComparison.InvariantCultureIgnoreCase) == 0);
			if(exists) {
				return AOPResult.Failed("关系已存在");
			}
			this.PermissionDataService.AddUserGroupRole(item);
	
			this.WritePermissionLog(string.Format("添加了用户角色关系：[{0}]{1}/{2}", item.ID, item.UserName, item.TargetValue), item);

			this.SetCachedUserGroupRolesExpired();
			return AOPResult.Success();
		}

		public PermissionRelation[] GetPermissionRelations(bool withPermissionResource = false) {
			var list = this.GetCachedPermissionRelations();
			if(withPermissionResource) {//需要获取权限实体
				var resources = this.GetCachedPermissionResources();
				foreach(var p in list) {
					if(p.PermissionResource == null) {
						p.PermissionResource = resources.SingleOrDefault(x => x.PermissionNo == p.PermissionNo);
					}
				}
			}
			return list;
		}

		public IAOPResult AddPermissionRelation(PermissionRelation relation) {
			/*var resources = this.GetCachedPermissionResources();
			relation.AppNo = resources.Where(x => x.PermissionNo == relation.PermissionNo).Select(x => x.AppNo).FirstOrDefault();*/
			this.PermissionDataService.AddPermissionRelation(relation);

			this.WritePermissionLog(string.Format("添加了授权关系：[{0}]{1}/{2}/{3}", relation.ID, relation.PermissionNo, relation.TargetObject, relation.TargetValue), relation);

			this.SetCachedPermissionRelationsExpired();
			return AOPResult.Success();
		}

		public IAOPResult DeletePermissionRelation(int id) {
			this.PermissionDataService.DeletePermissionRelation(id);

			var items = this.GetCachedPermissionRelations();
			var item = items.SingleOrDefault(x => x.ID == id);
			this.WritePermissionLog(string.Format("删除了授权关系：[{0}]{1}/{2}/{3}", id, item != null ? item.PermissionNo : null, item != null ? item.TargetObject : -1, item != null ? item.TargetValue : null), item);

			this.SetCachedPermissionRelationsExpired();
			return AOPResult.Success();
		}

		public IAOPResult UpdatePermissionRelation(PermissionRelation relation) {
			this.PermissionDataService.UpdatePermissionRelation(relation);

			this.WritePermissionLog(string.Format("更新了授权关系：[{0}]{1}/{2}/{3}", relation.ID, relation.PermissionNo, relation.TargetObject, relation.TargetValue), relation);

			this.SetCachedPermissionRelationsExpired();
			return AOPResult.Success();
		}

		public PermissionRelation GetPermissionRelation(int id, bool withPermissionResource = false) {
			var relation = this.GetPermissionRelations().SingleOrDefault(x => x.ID == id);
			if(withPermissionResource && relation != null && relation.PermissionResource == null) {
				var resources = this.GetCachedPermissionResources();
				relation.PermissionResource = resources.SingleOrDefault(x => x.PermissionNo == relation.PermissionNo);
			}
			return relation;
		}

		public PermissionRelation[] GetUserPermissionRelations(string userName, bool withPermissionResource = false) {
			var list = this.FetchPermissionsByUserGroupRole(userName);
			if(withPermissionResource) {
				var resouces = this.GetCachedPermissionResources();
				return list.Select(x => {
					if(x.PermissionResource == null) {
						x.PermissionResource = resouces.SingleOrDefault(y => y.PermissionNo.StringEquals(x.PermissionNo));
					}
				    return x;
				}).ToArray();
			}
			return list.ToArray();
		}

		public IPaginateResult<PermissionLog> GetPermissionLogs(string userName, DateTime? startTime, DateTime? endTime, int startIndex, int length) {
			return this.PermissionDataService.GetPermissionLogs(userName, startTime, endTime, startIndex, length);
		}

		protected virtual PermissionRole[] GetCachedPermissionRoles() {
			return this.PermissionCache.GetObjectFromCache("GetPermissionRoles", () => this.PermissionDataService.GetPermissionRoles(), CacheDependency.Create(DateTime.Now.AddSeconds(this.Setting.BufferTimeout)));
		}

		protected virtual void SetCachedPermissionRolesExpired() {
			this.PermissionCache.Remove("GetPermissionRoles".ToLowerInvariant());
		}

		protected virtual void WritePermissionLog(string contents, object targetObject = null) {
			var log = new PermissionLog { Contents = contents };
			if(targetObject != null) {
				log.TargetObject = JsonConvert.SerializeObject(targetObject);
			}
			var ctx = HttpContext.Current;
			if(ctx != null) {
				log.ClientIP = SecurityHelper.GetClientIP(ctx, true);
				if(ctx.Request.IsAuthenticated) {
					log.UserName = ctx.User.Identity.Name;
				}
			}
			this.PermissionDataService.AddPermissionLog(log);
		}
	}
}
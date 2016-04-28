using System;
using System.Linq;
using System.Collections.Generic;
using HTB.DevFx.Cache;
using HTB.DevFx.Core;
using Octopus.SecurityPermissions.Server.Config;

namespace Octopus.SecurityPermissions.Server
{
	internal partial class PermissionService : ServiceBase<PermissionServiceSetting>
	{
		protected ICache PermissionCache;
		protected IPermissionDataService PermissionDataService;
		protected override void OnInit() {
			base.OnInit();
			this.PermissionCache = CacheService.GetCache(this.Setting.CacheName);
			this.PermissionDataService = this.ObjectService.GetObject<IPermissionDataService>();
		}

		public PermissionObject[] GetPermissions(string userName) {
			var relations = this.FetchPermissionsByUserGroupRole(userName);
			return relations.Select(x => new PermissionObject {
				AppNo = x.AppNo,
				PermissionNo = x.PermissionNo,
				PermissionValue = x.PermissionValue,
				Enabled = x.Enabled,
				Granted = x.Granted,
				Grantable = x.Grantable,
				Priority = x.Priority,
				Remark = x.Remark
			}).ToArray();
		}

		public PermissionResource[] GetPermissionResourcesByAppNo(string appNo) {
			var resources = this.GetCachedPermissionResources();
			if (string.IsNullOrEmpty(appNo)) {//需要限定应用
				resources = resources.Where(x => string.Compare(appNo, x.AppNo, StringComparison.CurrentCultureIgnoreCase) == 0).ToArray();
			}
			return resources;
		}

		public PermissionResource[] GetPermissionResources() {
			return this.GetCachedPermissionResources();
		}

		public PermissionObject[] GetPermissionsByPermissionNo(string permissionNo) {
			var relations = this.GetCachedPermissionRelations();
			return relations.Where(x => string.Compare(permissionNo, x.PermissionNo, StringComparison.CurrentCultureIgnoreCase) == 0).Select(x => new PermissionObject {
				AppNo = x.AppNo,
				PermissionNo = x.PermissionNo,
				PermissionValue = x.PermissionValue,
				Enabled = x.Enabled,
				Granted = x.Granted,
				Grantable = x.Grantable,
				Priority = x.Priority,
				Remark = x.Remark
			}).ToArray();
		}

		protected virtual List<PermissionRelation> FetchPermissionsByUserGroupRole(string userName) {
			var list = new List<PermissionRelation>();
			var ugrs = this.GetCachedUserGroupRoles();
			//获取用户所在的组
			var groups = ugrs.Where(x => string.Compare(x.UserName, userName, StringComparison.InvariantCultureIgnoreCase) == 0 && x.TargetCategory == 1).Select(x => x.TargetValue);
			//获取这些组所关联的角色
			var groupRoles = this.GetCachedGroupRoles();
			var roles = new List<string>();
			foreach(var g in groups) {
				var g1 = g;
				roles.AddRange(groupRoles.Where(x => string.Compare(x.GroupNo, g1, StringComparison.InvariantCultureIgnoreCase) == 0).Select(x => x.RoleNo));
			}
			//获取用户所属角色
			roles.AddRange(ugrs.Where(x => string.Compare(x.UserName, userName, StringComparison.InvariantCultureIgnoreCase) == 0 && x.TargetCategory == 2).Select(x => x.TargetValue));

			var relations = this.GetCachedPermissionRelations();
			//获取这些角色所对应的权限
			foreach(var rn in roles) {
				var roleNo = rn;
				list.AddRange(
					relations.Where(x => x.TargetObject == 2 && string.Compare(x.TargetValue, roleNo, StringComparison.InvariantCultureIgnoreCase) == 0)
				);
			}
			//获取用户直接的权限
			list.AddRange(
				relations.Where(x => x.TargetObject == 1 && string.Compare(x.TargetValue, userName, StringComparison.InvariantCultureIgnoreCase) == 0)
			);
			return list.OrderByDescending(x => x.Priority).ThenByDescending(x => x.ID).ToList();
		}

		protected virtual PermissionResource[] GetCachedPermissionResources() {
			return this.PermissionCache.GetObjectFromCache("GetPermissionResources", () => this.PermissionDataService.GetPermissionResources(), CacheDependency.Create(DateTime.Now.AddSeconds(this.Setting.BufferTimeout)));
		}

		protected virtual void SetCachedPermissionResourcesExpired() {
			this.PermissionCache.Remove("GetPermissionResources".ToLowerInvariant());
		}

		protected virtual PermissionRelation[] GetCachedPermissionRelations() {
			return this.PermissionCache.GetObjectFromCache("GetPermissionRelations", () => this.PermissionDataService.GetPermissionRelations(), CacheDependency.Create(DateTime.Now.AddSeconds(this.Setting.BufferTimeout)));
		}

		protected virtual void SetCachedPermissionRelationsExpired() {
			this.PermissionCache.Remove("GetPermissionRelations".ToLowerInvariant());
		}

		protected virtual GroupRole[] GetCachedGroupRoles() {
			return this.PermissionCache.GetObjectFromCache("GetGroupRoles", () => this.PermissionDataService.GetGroupRoles(), CacheDependency.Create(DateTime.Now.AddSeconds(this.Setting.BufferTimeout)));
		}

		protected virtual UserGroupRole[] GetCachedUserGroupRoles() {
			return this.PermissionCache.GetObjectFromCache("GetUserGroupRoles", () => this.PermissionDataService.GetUserGroupRoles(), CacheDependency.Create(DateTime.Now.AddSeconds(this.Setting.BufferTimeout)));
		}

		protected virtual void SetCachedUserGroupRolesExpired() {
			this.PermissionCache.Remove("GetUserGroupRoles".ToLowerInvariant());
		}
	}
}

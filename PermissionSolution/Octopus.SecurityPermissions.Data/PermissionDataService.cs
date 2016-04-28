using System;
using System.Collections.Generic;
using HTB.DevFx.Data;
using HTB.DevFx.Data.Entities;
using Octopus.SecurityPermissions.Server;

namespace Octopus.SecurityPermissions.Data
{
	internal class PermissionDataService : IPermissionDataService
	{
		public PermissionResource[] GetPermissionResources() {
			return DataService.Execute<PermissionResource[]>("GetPermissionResources", null);
		}

		public PermissionRelation[] GetPermissionRelations() {
			return DataService.Execute<PermissionRelation[]>("GetPermissionRelations", null);
		}

		public GroupRole[] GetGroupRoles() {
			return DataService.Execute<GroupRole[]>("GetGroupRoles", null);
		}

		public UserGroupRole[] GetUserGroupRoles() {
			return DataService.Execute<UserGroupRole[]>("GetUserGroupRoles", null);
		}

		public void AddPermissionResource(PermissionResource resource, string defaultRole) {
			var dict = resource.ToDictionary();
			dict.Add("DefaultRole", defaultRole);
			DataService.Execute("AddPermissionResource", dict);
		}

		public void UpdatePermissionResource(PermissionResource resource) {
			DataService.Execute("UpdatePermissionResource", resource);
		}

		public void DeletePermissionResource(string id) {
			DataService.Execute("DeletePermissionResource", new { PermissionNo = id });
		}

		public PermissionRole[] GetPermissionRoles() {
			return DataService.Execute<PermissionRole[]>("GetPermissionRoles", null);
		}

		public void DeletePermissionRole(string roleNo) {
			DataService.Execute("DeletePermissionRole", new { RoleNo = roleNo });
		}

		public void AddPermissionRole(PermissionRole role) {
			DataService.Execute("AddPermissionRole", role);
		}

		public void UpdatePermissionRole(PermissionRole role) {
			DataService.Execute("UpdatePermissionRole", role);
		}

		public void DeleteUserGroupRole(int id) {
			DataService.Execute("DeleteUserGroupRole", new { ID = id });
		}

		public void AddUserGroupRole(UserGroupRole item) {
			DataService.Execute("AddUserGroupRole", item);
		}

		public void AddPermissionRelation(PermissionRelation relation) {
			DataService.Execute("AddPermissionRelation", relation);
		}

		public void DeletePermissionRelation(int id) {
			DataService.Execute("DeletePermissionRelation", new { ID = id });
		}

		public void UpdatePermissionRelation(PermissionRelation relation) {
			DataService.Execute("UpdatePermissionRelation", relation);
		}

		public IPaginateResult<PermissionLog> GetPermissionLogs(string userName, DateTime? startTime, DateTime? endTime, int startIndex, int length) {
			var parameters = new Dictionary<string, object> {
				{ "StartIndex", startIndex },
				{ "PageSize", length },
			};
			if(!string.IsNullOrEmpty(userName)) {
				parameters.Add("UserName", userName);
			}
			if(startTime != null) {
				parameters.Add("StartTime", startTime.Value);
			}
			if(endTime != null) {
				parameters.Add("EndTime", endTime.Value);
			}
			return DataService.Execute<IPaginateResult<PermissionLog>>("GetPermissionLogs", parameters);
		}

		public void AddPermissionLog(PermissionLog log) {
			DataService.Execute("AddPermissionLog", log);
		}
	}
}

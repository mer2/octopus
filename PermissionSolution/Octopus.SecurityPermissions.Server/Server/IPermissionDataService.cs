using System;
using HTB.DevFx.Data.Entities;

namespace Octopus.SecurityPermissions.Server
{
	public interface IPermissionDataService
	{
		PermissionResource[] GetPermissionResources();
		PermissionRelation[] GetPermissionRelations();
		GroupRole[] GetGroupRoles();
		UserGroupRole[] GetUserGroupRoles();

		void AddPermissionResource(PermissionResource resource, string defaultRole);
		void UpdatePermissionResource(PermissionResource resource);
		void DeletePermissionResource(string id);
		PermissionRole[] GetPermissionRoles();
		void DeletePermissionRole(string roleNo);
		void AddPermissionRole(PermissionRole role);
		void UpdatePermissionRole(PermissionRole role);
		void DeleteUserGroupRole(int id);
		void AddUserGroupRole(UserGroupRole item);
		void AddPermissionRelation(PermissionRelation relation);
		void DeletePermissionRelation(int id);
		void UpdatePermissionRelation(PermissionRelation relation);

		IPaginateResult<PermissionLog> GetPermissionLogs(string userName, DateTime? startTime, DateTime? endTime, int startIndex, int length);
		void AddPermissionLog(PermissionLog log);
	}
}
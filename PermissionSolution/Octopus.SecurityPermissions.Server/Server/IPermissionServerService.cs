using System;
using HTB.DevFx.Core;
using HTB.DevFx.Data.Entities;

namespace Octopus.SecurityPermissions.Server
{
	public interface IPermissionServerService : IPermissionService
	{
		IAOPResult AddPermissionResource(PermissionResource resource);
		IAOPResult UpdatePermissionResource(PermissionResource resource);
		IAOPResult DeletePermissionResource(string id);
		PermissionRole[] GetPermissionRoles();
		IAOPResult DeletePermissionRole(string roleNo);
		IAOPResult AddPermissionRole(PermissionRole role);
		IAOPResult UpdatePermissionRole(PermissionRole role);
		UserGroupRole[] GetUserGroupRoles();
		IAOPResult DeleteUserRole(int id);
		IAOPResult AddUserRole(UserGroupRole item);
		PermissionRelation[] GetPermissionRelations(bool withPermissionResource = false);
		IAOPResult AddPermissionRelation(PermissionRelation relation);
		IAOPResult DeletePermissionRelation(int id);
		IAOPResult UpdatePermissionRelation(PermissionRelation relation);
		PermissionRelation GetPermissionRelation(int id, bool withPermissionResource = false);
		PermissionRelation[] GetUserPermissionRelations(string userName, bool withPermissionResource = false);
		IPaginateResult<PermissionLog> GetPermissionLogs(string userName, DateTime? startTime, DateTime? endTime, int startIndex = 0, int length = 30); 
	}
}
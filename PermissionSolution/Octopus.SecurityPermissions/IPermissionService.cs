namespace Octopus.SecurityPermissions
{
	/// <summary>
	/// 权限系统远端服务接口
	/// </summary>
	public interface IPermissionService
	{
		/// <summary>
		/// 获取指定用户的权限列表
		/// </summary>
		/// <param name="userName">用户名</param>
		/// <returns>权限列表</returns>
		PermissionObject[] GetPermissions(string userName);
		/// <summary>
		/// 获取所有权限资源（指定应用）
		/// </summary>
		/// <returns>资源列表</returns>
		PermissionResource[] GetPermissionResources();
		/// <summary>
		/// 获取指定应用的所有权限资源
		/// </summary>
		/// <param name="appNo">应用编码</param>
		/// <returns>资源列表</returns>
		PermissionResource[] GetPermissionResourcesByAppNo(string appNo);
		/// <summary>
		/// 获取指定编码的权限列表
		/// </summary>
		/// <param name="permissionNo">权限编码</param>
		/// <returns>权限列表</returns>
		PermissionObject[] GetPermissionsByPermissionNo(string permissionNo);
	}
}

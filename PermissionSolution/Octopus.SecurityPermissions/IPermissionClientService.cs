using System;
using System.Web;

namespace Octopus.SecurityPermissions
{
	/// <summary>
	/// 权限客户端服务接口
	/// </summary>
	public interface IPermissionClientService
	{
		/// <summary>
		/// 权限认证
		/// </summary>
		/// <param name="throwOnFailed">认证失败时是否抛出异常</param>
		/// <param name="permissions">被认证的权限列表</param>
		/// <returns>是否认证成功</returns>
		bool Authorize(bool throwOnFailed, params IPermissionObject[] permissions);
		PermissionObject[] GetUserPermissions(string userName, string permissionNo, bool withPermissionResource = false);
		PermissionResource[] GetPermissionResources();
		PermissionObject[] GetPermissionsByPermissionNo(string permissionNo);
		void OnError(object sender, EventArgs e);
		string HandleUnauthorized(HttpContext ctx = null, bool redirect = true, bool endResponse = true);
	}
}

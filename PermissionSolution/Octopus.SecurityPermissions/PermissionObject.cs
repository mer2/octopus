using System;

namespace Octopus.SecurityPermissions
{
	[Serializable]
	public class PermissionObject : IPermissionObject
	{
		public string AppNo { get; set; }
		public string PermissionNo { get; set; }
		public string PermissionValue { get; set; }
		public bool Enabled { get; set; }
		/// <summary>
		/// 此权限是否授予
		/// </summary>
		public bool Granted { get; set; }
		/// <summary>
		/// 此权限是否可以授权给别人
		/// </summary>
		public bool Grantable { get; set; }
		/// <summary>
		/// 此权限的优先级（数值越高优先级越高）
		/// </summary>
		public int Priority { get; set; }
		public string Remark { get; set; }
		/// <summary>
		/// 所对应的权限实体
		/// </summary>
		public PermissionResource PermissionResource { get; set; }
		/// <summary>
		/// 是需要被验证的权限，不是用户拥有的权限
		/// </summary>
		internal bool Validating { get; set; }
	}
}

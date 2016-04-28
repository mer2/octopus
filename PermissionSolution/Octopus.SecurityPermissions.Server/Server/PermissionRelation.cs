using System;

namespace Octopus.SecurityPermissions.Server
{
	[Serializable]
	public class PermissionRelation : PermissionObject
	{
		public int ID { get; set; }
		/// <summary>
		/// 授权对象，1：对用户授权，2：对角色授权
		/// </summary>
		public int TargetObject { get; set; }
		/// <summary>
		/// 授权对象值
		/// </summary>
		public string TargetValue { get; set; }
		/// <summary>
		/// 创建日期
		/// </summary>
		public DateTime CreateTime { get; set; }
	}
}

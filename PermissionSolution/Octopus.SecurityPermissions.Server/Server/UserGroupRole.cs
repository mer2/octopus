using System;

namespace Octopus.SecurityPermissions.Server
{
	[Serializable]
	public class UserGroupRole
	{
		public int ID { get; set; }
		public string UserName { get; set; }
		/// <summary>
		/// 1：用户和组的关系/2：用户和角色的关系
		/// </summary>
		public int TargetCategory { get; set; }
		public string TargetValue { get; set; }
		public bool Enabled { get; set; }
	}
}

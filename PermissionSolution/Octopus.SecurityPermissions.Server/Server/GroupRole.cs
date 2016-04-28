using System;

namespace Octopus.SecurityPermissions.Server
{
	[Serializable]
	public class GroupRole
	{
		public int ID { get; set; }
		public string GroupNo { get; set; }
		public string RoleNo { get; set; }
		public bool Enabled { get; set; }
	}
}

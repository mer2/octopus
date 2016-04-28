using System;

namespace Octopus.SecurityPermissions.Server
{
	[Serializable]
	public class PermissionRole
	{
		public string AppNo { get; set; }
		public string RoleNo { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public bool Enabled { get; set; }
	}
}
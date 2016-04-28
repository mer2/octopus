using System;

namespace Octopus.SecurityPermissions
{
	internal class PermissionSet
	{
		public string UserName { get; set; }
		public DateTime LastUpdated { get; set; }
		public PermissionObject[] Permissions { get; set; }
	}
}

using System;

namespace Octopus.SecurityPermissions
{
	[Serializable, AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
	public class PermissionAttribute : Attribute, IPermissionObject
	{
		public PermissionAttribute(string permissionNo) {
			this.PermissionNo = permissionNo;
			this.Enabled = true;
		}

		public string AppNo { get; set; }
		public string PermissionNo { get; set; }
		public string PermissionValue { get; set; }
		public bool Enabled { get; set; }
	}
}

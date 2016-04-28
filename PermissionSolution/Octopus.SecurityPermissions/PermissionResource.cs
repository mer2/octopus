using System;

namespace Octopus.SecurityPermissions
{
	[Serializable]
	public class PermissionResource
	{
		public string PermissionNo { get; set; }
		public string AppNo { get; set; }
		public string Category { get; set; }
		public string ParentNo { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public string Url { get; set; }
		public bool Viewable { get; set; }
		public bool Enabled { get; set; }
		public int DispIndex { get; set; }
	}
}

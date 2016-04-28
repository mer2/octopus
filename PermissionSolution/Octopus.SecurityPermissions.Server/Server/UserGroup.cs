using System;

namespace Octopus.SecurityPermissions.Server
{
	[Serializable]
	public class UserGroup
	{
		public string GroupNo { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public bool Enabled { get; set; }
	}
}

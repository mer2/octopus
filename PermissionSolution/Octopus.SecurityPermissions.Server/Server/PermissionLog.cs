using System;

namespace Octopus.SecurityPermissions.Server
{
	[Serializable]
	public class PermissionLog
	{
		public int ID { get; set; }
		public string UserName { get; set; }
		public string Contents { get; set; }
		public string TargetObject { get; set; }
		public DateTime CreatedTime { get; set; }
		public string ClientIP { get; set; }
	}
}

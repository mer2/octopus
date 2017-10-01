using System;

namespace Octopus.Security
{
	[Serializable]
	public class NoneAuthorizeAttribute : AuthorizeAttribute
	{
		public NoneAuthorizeAttribute() : base("*") {
			this.Category = "Application";
		}
	}
}
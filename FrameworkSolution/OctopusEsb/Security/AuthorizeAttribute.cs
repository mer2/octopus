using System;

namespace Octopus.Security
{
	[Serializable, AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Interface, Inherited = true, AllowMultiple = true)]
	public class AuthorizeAttribute : Attribute, IAuthorizationIdentity
	{
		public AuthorizeAttribute() : this("?", null) {}

		public AuthorizeAttribute(string users) : this(users, null) {
		}

		public AuthorizeAttribute(string users, string roles) : this(users, roles, "User") {
		}

		public AuthorizeAttribute(string users, string roles, string category) {
			this.Users = users;
			this.Roles = roles;
			this.Category = category;
		}
		
		public string Users { get; set; }
		public string Roles { get; set; }
		public string Category { get; set; }
		public int OrderIndex { get; set; }
	}
}

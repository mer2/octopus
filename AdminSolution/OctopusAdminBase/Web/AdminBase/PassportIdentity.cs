using System;
using System.Security.Principal;

namespace Octopus.Web.AdminBase
{
	[Serializable]
	public class PassportIdentity : IIdentity
	{
		public string SessionToken { get; set; }
		public string Name { get; set; }
		public string DisplayName { get; set; }
		public DateTime IssueDate { get; set; }
		public DateTime Expiration { get; set; }

		public bool Expired {
			get { return (this.Expiration < DateTime.Now); }
		}

		public string AuthenticationType {
			get { return "AdminPassport"; }
		}

		public bool IsAuthenticated {
			get { return !string.IsNullOrEmpty(this.Name); }
		}
	}
}
using System;
using System.Security;
using System.Security.Permissions;
using HTB.DevFx.Security;

namespace Octopus.SecurityPermissions
{
	[Serializable, AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class SecurityAuthorizeAttribute : InterceptorAttribute, IPermission, IPermissionObject
	{
		protected SecurityAuthorizeAttribute(SecurityAction action) : base(action) {
			this.Enabled = true;
		}
		public SecurityAuthorizeAttribute(InterceptorAction action = InterceptorAction.Demand) : base(action) {
			this.Enabled = true;
		}

		public string AppNo { get; set; }
		public string PermissionNo { get; set; }
		public string PermissionValue { get; set; }
		public bool Enabled { get; set; }

		protected override void Demand() {
			PermissionClientService.Authorize(this, true, true);
		}
	
		#region IPermission Members

		IPermission IPermission.Copy() {
			return (IPermission)this.MemberwiseClone();
		}

		IPermission IPermission.Intersect(IPermission target) {
			if (target != null && target.GetType() == this.GetType()) {
				return (IPermission)((SecurityAuthorizeAttribute)target).MemberwiseClone();
			}
			return null;
		}

		bool IPermission.IsSubsetOf(IPermission target) {
			return false;
		}

		IPermission IPermission.Union(IPermission target) {
			if(target != null && target.GetType() == this.GetType()) {
				return (IPermission)((SecurityAuthorizeAttribute)target).MemberwiseClone();
			}
			return null;
		}

		#endregion

		#region ISecurityEncodable Members

		void ISecurityEncodable.FromXml(SecurityElement e) {
			throw new NotImplementedException();
		}

		SecurityElement ISecurityEncodable.ToXml() {
			throw new NotImplementedException();
		}

		#endregion
	}
}

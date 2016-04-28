using System;

namespace Octopus.SecurityPermissions
{
	[AttributeUsage(AttributeTargets.Assembly,  AllowMultiple = false, Inherited = true)]
	public sealed class SecurityApplicationAttribute : Attribute
	{
		public SecurityApplicationAttribute(string application) {
			this.Application = application;
		}

		public string Application { get; set; }
	}
}

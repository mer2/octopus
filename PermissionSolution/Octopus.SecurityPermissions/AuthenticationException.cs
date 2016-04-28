using System;

namespace Octopus.SecurityPermissions
{
	/// <summary>
	/// 用户身份认证失败时的异常信息
	/// </summary>
	public class AuthenticationException : Exception
	{
		public AuthenticationException() {}

		public AuthenticationException(string message) : base(message) {}
	}
}

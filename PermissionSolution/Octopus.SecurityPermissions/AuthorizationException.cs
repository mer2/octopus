using System;

namespace Octopus.SecurityPermissions
{
	/// <summary>
	/// 用户授权认证失败时的异常信息
	/// </summary>
	public class AuthorizationException : Exception
	{
		public AuthorizationException() { }
		public AuthorizationException(string message) : base(message) { }
	}
}

using System;

namespace Octopus.Security
{
	[Serializable]
	public class UnauthorizedException : Exception
	{
		public UnauthorizedException() : base() {}

		public UnauthorizedException(string message) : base(message) { }
	}
}

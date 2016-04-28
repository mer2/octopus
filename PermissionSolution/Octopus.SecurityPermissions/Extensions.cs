using System;

namespace Octopus.SecurityPermissions
{
	public static class Extensions
	{
		public static bool StringEquals(this string x, string y) {
			return string.Compare(x, y, StringComparison.InvariantCultureIgnoreCase) == 0;
		}
	}
}
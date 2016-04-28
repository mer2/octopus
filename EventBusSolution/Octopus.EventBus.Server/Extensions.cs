using System;

namespace Octopus.EventBus.Server
{
	public static class Extensions
	{
		public static string ToGuidString(this byte[] bytes) {
			return new Guid(bytes).ToString("N");
		}
	}
}

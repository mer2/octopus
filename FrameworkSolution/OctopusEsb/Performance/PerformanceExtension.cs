using System;
using System.Collections.Generic;
using HTB.DevFx.Core;
using HTB.DevFx.Exceptions;

namespace Octopus.Performance
{
	public static class PerformanceExtension
	{
		public static IList<IAOPResult> Append(this IList<IAOPResult> results, string message, Action action) {
			if(results == null || action == null) {
				return results;
			}
			try {
				action();
			} catch(Exception e) {
				ExceptionService.Publish(e);
				results.Add(AOPResult.Failed(message + e.Message));
			}
			return results;
		}
	}
}

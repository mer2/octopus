using System;
using System.Collections;
using HTB.DevFx.Core;

namespace Octopus
{
	public static class Extensions
	{
		public static IDictionary Merge(this IDictionary dict, IDictionary options) {
			if(dict == null || options == null || options.Count <= 0) {
				return dict;
			}
			foreach(var key in options.Keys) {
				dict[key] = options[key];
			}
			return dict;
		}

		public static object ToLite<T>(this IAOPResult<T> aop, Func<T, object> resultHandler) {
			return new { aop.ResultNo, aop.ResultDescription, ResultAttachObject = resultHandler != null && aop.ResultAttachObject != null ? resultHandler(aop.ResultAttachObjectEx) : aop.ResultAttachObjectEx };
		}
	}
}
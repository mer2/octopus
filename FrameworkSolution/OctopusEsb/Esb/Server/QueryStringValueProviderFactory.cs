using System;
using System.Collections.Generic;
using System.Web.Routing;

namespace Octopus.Esb.Server
{
	internal class QueryStringValueProviderFactory : ValueProviderFactory
	{
		public override IValueProvider GetValueProvider(RequestContext requestContext) {
			if(requestContext == null) {
				throw new ArgumentNullException("requestContext");
			}
			var queryString = requestContext.HttpContext.Request.QueryString;
			IDictionary<string, object> dict = null;
			if (queryString.Count > 0) {
				dict = new Dictionary<string, object>();
				foreach (var key in queryString.AllKeys) {
					dict.Add(key, queryString[key]);
				}
				if (!dict.ContainsKey(KeyOfAllValues)) {
					dict.Add(KeyOfAllValues, dict);
				}
			}
			return dict != null ? new DictionaryValueProvider(dict, null) : null;
		}
	}
}

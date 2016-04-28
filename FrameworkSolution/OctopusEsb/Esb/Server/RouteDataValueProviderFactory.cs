using System;
using System.Collections.Generic;
using System.Web.Routing;

namespace Octopus.Esb.Server
{
	internal class RouteDataValueProviderFactory : ValueProviderFactory
	{
		public override IValueProvider GetValueProvider(RequestContext requestContext) {
			if(requestContext == null) {
				throw new ArgumentNullException("requestContext");
			}
			var values = requestContext.RouteData.Values;
			var dt = new Dictionary<string, object>(values);
			if(!dt.ContainsKey(KeyOfAllValues)) {
				dt.Add(KeyOfAllValues, dt);
			}
			return dt.Count > 0 ? new DictionaryValueProvider(dt, null) : null;
		}
	}
}

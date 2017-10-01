using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Octopus.Esb.Server
{
	internal class RouteDataValueProviderFactory : ValueProviderFactory
	{
		public override IValueProvider GetValueProvider(HttpContext httpContext) {
			if(httpContext == null) {
				throw new ArgumentNullException(nameof(httpContext));
			}
			var values = httpContext.GetRouteData()?.Values;
			if (values == null) {
				return null;
			}
			var dt = new Dictionary<string, object>(values);
			if(!dt.ContainsKey(KeyOfAllValues)) {
				dt.Add(KeyOfAllValues, dt);
			}
			return dt.Count > 0 ? new DictionaryValueProvider(dt, null) : null;
		}
	}
}

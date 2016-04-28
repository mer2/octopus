using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Routing;
using Newtonsoft.Json;

namespace Octopus.Esb.Server
{
	internal class JsonValueProviderFactory : ValueProviderFactory
	{
		public override IValueProvider GetValueProvider(RequestContext requestContext) {
			if(requestContext == null) {
				throw new ArgumentNullException("requestContext");
			}

			var jsonData = GetDeserializedObject(requestContext);
			if(jsonData == null) {
				return null;
			}
			var values = jsonData as IDictionary<string, object>;
			return values == null ? null : new DictionaryValueProvider(values);
		}

		private static object GetDeserializedObject(RequestContext requestContext) {
			if(!requestContext.HttpContext.Request.ContentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase)) {
				// not JSON request
				return null;
			}

			var reader = new StreamReader(requestContext.HttpContext.Request.InputStream);
			var bodyText = reader.ReadToEnd();
			if(string.IsNullOrEmpty(bodyText)) {
				// no JSON data
				return null;
			}

			var jsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(bodyText, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
			return jsonData;
		}
	}
}

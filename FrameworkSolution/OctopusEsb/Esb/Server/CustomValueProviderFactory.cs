using System;
using System.Collections.Generic;
using System.Web.Routing;

namespace Octopus.Esb.Server
{
	internal class CustomValueProviderFactory : ValueProviderFactory
	{
		public override IValueProvider GetValueProvider(RequestContext requestContext) {
			if(requestContext == null) {
				throw new ArgumentNullException("requestContext");
			}

			ISerializer serializer;
			var data = GetDeserializedObject(requestContext, out serializer);
			if(data == null) {
				return null;
			}
			var values = data as IDictionary<string, object>;
			if (values != null) {
				if(!values.ContainsKey(KeyOfAllValues)) {
					values.Add(KeyOfAllValues, values);
				}
			}
			return values == null ? null : new DictionaryValueProvider(values, serializer);
		}

		private static object GetDeserializedObject(RequestContext requestContext, out ISerializer serializer) {
			var ctx = requestContext.HttpContext;
			serializer = SerializerFactory.Current.GetSerializer(ctx.Request.ContentType);
			if(serializer == null) {
				return null;
			}

			var data = serializer.Deserialize(requestContext.HttpContext.Request.InputStream, typeof(Dictionary<string, object>), null);
			return data;
		}
	}
}

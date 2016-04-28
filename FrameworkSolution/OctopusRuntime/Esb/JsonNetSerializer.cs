using System;
using System.Collections;
using System.IO;
using HTB.DevFx.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Octopus.Esb
{
	public class JsonNetSerializer : SerializerBase
	{
		private readonly JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
		protected internal virtual JsonSerializer Serializer {
			get { return this.serializer; }
		}

		protected override void SerializeInternal(Stream stream, object instance, IDictionary options) {
			var sw = new StreamWriter(stream);
			this.Serializer.Serialize(new JsonTextWriter(sw), instance);
			sw.Flush();
		}

		protected override object DeserializeInternal(Stream stream, Type expectedType, IDictionary options) {
			var sr = new StreamReader(stream);
			return this.Serializer.Deserialize(new JsonTextReader(sr), expectedType);
		}

		protected override object ConvertInternal(object instance, Type expectedType, IDictionary options) {
			var token = instance as JToken;
			if (token != null) {
				var method = typeof (JToken).GetMethod("ToObject", new Type[] {}).MakeGenericMethod(expectedType);
				object result;
				if(TypeHelper.TryInvoke(token, method, out result, false)) {
					return result;
				}
			}
			return Convert.ChangeType(instance, expectedType);
		}
	}
}

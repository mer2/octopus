using System;
using System.Collections;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Octopus.Esb
{
	public class JsonNetSerializer : SerializerBase
	{
		private JsonSerializer serializer;
		protected internal virtual JsonSerializer GetSerializer(IDictionary options = null) {
			return this.serializer ?? (this.serializer = JsonSerializer.Create(this.GetJsonSerializerSettings(options)));
		}

		protected virtual JsonSerializerSettings GetJsonSerializerSettings(IDictionary options = null) {
			return new JsonSerializerSettings {
				Converters = { new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff" } }
			};
		}

		protected override void SerializeInternal(Stream stream, object instance, IDictionary options) {
			var sw = new StreamWriter(stream);
			this.GetSerializer(options).Serialize(new JsonTextWriter(sw), instance);
			sw.Flush();
		}

		protected override object DeserializeInternal(Stream stream, Type expectedType, IDictionary options) {
			var sr = new StreamReader(stream);
			return this.GetSerializer(options).Deserialize(new JsonTextReader(sr), expectedType);
		}

		protected override object ConvertInternal(object instance, Type expectedType, IDictionary options) {
			if(instance is JToken token) {
				var result = token.ToObject(expectedType, this.serializer);
				return result;
			}
			return Convert.ChangeType(instance, expectedType);
		}
	}
}

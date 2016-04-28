using System;
using System.Collections;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Converters;

namespace Octopus.Esb
{
	internal class BsonNetSerializer : JsonNetSerializer
	{
		private readonly JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All, Converters = { new IsoDateTimeConverter() } });
		protected internal override JsonSerializer Serializer {
			get { return this.serializer; }
		}

		protected override void SerializeInternal(Stream stream, object instance, IDictionary options) {
			var sw = new BsonWriter(stream);
			this.Serializer.Serialize(sw, instance);
			sw.Flush();
		}

		protected override object DeserializeInternal(Stream stream, Type expectedType, IDictionary options) {
			var sr = new BsonReader(stream);
			return this.Serializer.Deserialize(sr, expectedType);
		}
	}
}

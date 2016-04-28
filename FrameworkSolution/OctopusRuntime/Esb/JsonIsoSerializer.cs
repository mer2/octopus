using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Octopus.Esb
{
	internal class JsonIsoSerializer : JsonNetSerializer
	{
		private readonly JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All, Converters = { new IsoDateTimeConverter() }});
		protected internal override JsonSerializer Serializer {
			get { return this.serializer; }
		}
	}
}

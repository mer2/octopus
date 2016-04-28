using System;
using System.Collections;
using System.IO;
using System.Web.Script.Serialization;

namespace Octopus.Esb
{
	internal class JsonInternalSerializer : SerializerBase
	{
		protected override void SerializeInternal(Stream stream, object instance, IDictionary options) {
			var js = new JavaScriptSerializer();
			var jsonData = js.Serialize(instance);
			var sw = new StreamWriter(stream); 
			sw.Write(jsonData);
			sw.Flush();
		}

		protected override object DeserializeInternal(Stream stream, Type expectedType, IDictionary options) {
			var jsonData = (new StreamReader(stream)).ReadToEnd();
			var js = new JavaScriptSerializer();
			return js.Deserialize(jsonData, expectedType);
		}

		protected override object ConvertInternal(object instance, Type expectedType, IDictionary options) {
			if(expectedType.IsInstanceOfType(instance)) {
				return instance;
			}
			var js = new JavaScriptSerializer();
			return js.ConvertToType(instance, expectedType);
		}
	}
}

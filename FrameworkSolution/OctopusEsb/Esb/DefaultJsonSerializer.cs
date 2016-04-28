using System;
using System.Collections;
using System.IO;
using System.Web.Script.Serialization;
using HTB.DevFx.Utils;

namespace Octopus.Esb
{
	internal class DefaultJsonSerializer : SerializerBase
	{
		protected override void SerializeInternal(Stream stream, object instance, IDictionary options) {
			var jsonData = JsonHelper.ToJson(instance, true);
			var sw = new StreamWriter(stream);
			sw.Write(jsonData);
			sw.Flush();
		}

		protected override object DeserializeInternal(Stream stream, Type expectedType, IDictionary options) {
			var jsonData = (new StreamReader(stream)).ReadToEnd();
			var js = new JavaScriptSerializer();
			var method = typeof(JavaScriptSerializer).GetMethod("Deserialize", new[] { typeof(string) }).MakeGenericMethod(expectedType);
			object result;
			TypeHelper.TryInvoke(js, method, out result, true, jsonData);
			return result;
		}

		protected override object ConvertInternal(object instance, Type expectedType, IDictionary options) {
			if(expectedType.IsInstanceOfType(instance)) {
				return instance;
			}
			var js = new JavaScriptSerializer();
			var method = typeof(JavaScriptSerializer).GetMethod("ConvertToType", new[] { typeof(object) }).MakeGenericMethod(expectedType);
			object result;
			TypeHelper.TryInvoke(js, method, out result, true, instance);
			return result;
		}
	}
}

using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using HTB.DevFx;
using HTB.DevFx.Core;
using HTB.DevFx.Log;
using Octopus.Esb.Config;

namespace Octopus.Esb
{
	internal class SerializerFactoryInternal : ServiceBase<SerializerFactorySetting>, ISerializerFactory
	{
		protected override void OnInit() {
			base.OnInit();
			this.serializers = new Dictionary<string, ISerializer>();
			var setting = this.Setting;
			foreach(var item in setting.SerializerItems) {
				if(!item.Enabled) {
					continue;
				}
				var serializer = this.ObjectService.GetOrCreateObject<ISerializer>(item.TypeName);
				if(serializer != null) {
					serializer.ContentType = item.ContentType;
					if(setting.Debug) {
						serializer = new SerializerWrapper(serializer);
					}
					this.serializers.Add(item.Name, serializer);
				}
			}
			this.Default = this.serializers[setting.DefaultSerializerName];
		}

		private Dictionary<string, ISerializer> serializers;

		public ISerializer GetSerializer(string name) {
			if(string.IsNullOrEmpty(name)) {
				return null;
			}
			return this.serializers.Values.FirstOrDefault(s => name.StartsWith(s.ContentType, StringComparison.InvariantCultureIgnoreCase));
		}

		public ISerializer Default { get; private set; }

		private class SerializerWrapper : SerializerBase
		{
			public SerializerWrapper(ISerializer serializer) {
				this.serializer = serializer;
			}
			private readonly ISerializer serializer;

			protected override void SerializeInternal(Stream stream, object instance, IDictionary options) {
				using(var ms = new MemoryStream()) {
					this.serializer.Serialize(ms, instance, options);
					ms.WriteTo(stream);
					ms.Position = 0;
					var data = (new StreamReader(ms)).ReadToEnd();
					LogService.WriteLog(this, "Instance: {0}, Serialized {2}: \r\n{1}", instance, data, ms.Length);
				}
			}

			protected override object DeserializeInternal(Stream stream, Type expectedType, IDictionary options) {
				const int bufferSize = 1024;
				using(var ms = new MemoryStream()) {
					var br = new BinaryReader(stream);
					int length;
					do {
						var buffer = br.ReadBytes(bufferSize);
						length = buffer.Length;
						ms.Write(buffer, 0, length);
					} while (length >= bufferSize);
					ms.Position = 0;
					var data = new StreamReader(ms).ReadToEnd();
					LogService.WriteLog(this, "ContentType: {3}, Type: {0}, Deserialize {2}: \r\n{1}", expectedType?.FullName, data, ms.Length, this.serializer.ContentType);
					ms.Position = 0;
					return this.serializer.Deserialize(ms, expectedType, options);
				}
			}

			protected override object ConvertInternal(object instance, Type expectedType, IDictionary options) {
				return this.serializer.Convert(instance, expectedType, options);
			}

			protected override string ContentTypeInternal {
				get => this.serializer.ContentType;
				set => this.serializer.ContentType = value;
			}
		}
	}
}

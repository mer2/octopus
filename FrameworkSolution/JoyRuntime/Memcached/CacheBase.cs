using System.IO;
using HTB.DevFx;
using HTB.DevFx.Cache;
using HTB.DevFx.Cache.Config;
using HTB.DevFx.Config;
using HTB.DevFx.Core;
using Joy.Runtime.Memcached.Config;
using Octopus.Esb;

namespace Joy.Runtime.Memcached
{
	internal abstract class CacheBase : ICacheInternal
	{
		public void Add(string key, object value, ICacheDependency cacheDependency) {
			this.AddInternal(key, value, cacheDependency);
		}

		public void Add(string key, object value) {
			this.AddInternal(key, value, null);
		}

		public object Get(string key) {
			return this.GetInternal(key);
		}

		public bool TryGet(string key, out object value) {
			value = this.GetInternal(key);
			return value != null;
		}

		public void Remove(string key) {
			this.RemoveInternal(key);
		}

		public bool Contains(string key) {
			return this.ContainsInternal(key);
		}

		public void Clear() {
			this.ClearInternal();
		}

		object ICache.this[string key] {
			get { return this.Get(key); }
			set { this.SetInternal(key, value, null); }
		}

		object ICache.this[string key, ICacheDependency cacheDependency] {
			set { this.SetInternal(key, value, cacheDependency); }
		}

		public int Count {
			get { return -1; }
		}

		void IInitializable<ICacheSetting>.Init(ICacheSetting setting) {
			this.CacheName = setting.Name;
			string serializerName = null;
			var st = setting.ConfigSetting.ToSetting<CacheSetting>("memcached");
			if(st != null) {
				serializerName = st.SerializerName;
			}
			var serializerFactory = ObjectService.GetObject<ISerializerFactory>();
			if(!string.IsNullOrEmpty(serializerName)) {
				this.Serializer = serializerFactory.GetSerializer(serializerName);
			}
			if(this.Serializer == null) {
				this.Serializer = serializerFactory.Default;
			}
			this.InitInternal(st);
		}

		public string CacheName { get; private set; }
		public ISerializer Serializer { get; private set; }

		protected virtual void InitInternal(CacheSetting setting) {
		}

		protected virtual object GetInternal(string key) {
			var data = this.GetBytesInternal(key);
			return this.DeSerialize(data);
		}

		protected virtual byte[] Serialize(object value) {
			using(var outStream = new MemoryStream()) {
				this.Serializer.Serialize(outStream, value, null);
				return outStream.ToArray();
			}
		}

		protected virtual object DeSerialize(byte[] data) {
			object result = null;
			if(data != null) {
				using(var inStream = new MemoryStream(data)) {
					result = this.Serializer.Deserialize(inStream, null, null);
				}
			}
			return result;
		}

		protected abstract void RemoveInternal(string key);
		protected abstract bool ContainsInternal(string key);
		protected abstract void ClearInternal();
		protected abstract void AddInternal(string key, object value, ICacheDependency cacheDependency);
		protected abstract void SetInternal(string key, object value, ICacheDependency cacheDependency);
		protected abstract byte[] GetBytesInternal(string key);
	}
}

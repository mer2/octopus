using HTB.DevFx.Cache;
using BeIT.MemCached;
using Joy.Runtime.Memcached.Config;

namespace Joy.Runtime.Memcached
{
	/// <summary>
	/// 使用Memcached服务的缓存实现，需要在web.config/app.config配置以下内容：
	/// <code>
	///		<configSections>
	///			...
	///			<section name="beitmemcached" type="System.Configuration.NameValueSectionHandler"/>
	///			...
	///		</configSections>
	///		...
	///		<beitmemcached>
	///			<add key="Memcached名称" value="ip:port,ip:port,ip:port"/>
	///		</beitmemcached>
	/// </code>
	/// 
	/// HTB.DevFx的对象配置：
	/// <code>
	///		<cache>
	///			<add name="缓存名称" type="MemcachedCache">
	///				<memcached name="Memcached名称" keyPrefix="默认缓存前缀" servers="ip:port,ip:port,ip:port（如果没有配置，则会根据name尝试读取beitmemcached的配置）" serializer="序列化名称（可不填）" />
	///			</add>
	///		</cache>
	/// </code>
	/// </summary>
	internal class MemcachedCache : CacheBase
	{
		public MemcachedClient MemcachedClientInstance { get; private set; }

		protected override void InitInternal(CacheSetting setting) {
			base.InitInternal(setting);
			var memcachedName = this.CacheName;
			string keyPrefix = null;
			if(setting != null) {
				if(!string.IsNullOrEmpty(setting.CacheInstanceName)) {
					memcachedName = setting.CacheInstanceName;
				}
				keyPrefix = setting.KeyPrefix;
				if(!string.IsNullOrEmpty(setting.Servers)) {
					MemcachedClient.Setup(memcachedName, setting.Servers.Split(','));
				}
			}
			this.MemcachedClientInstance = MemcachedClient.GetInstance(memcachedName);
			this.MemcachedClientInstance.KeyPrefix = keyPrefix;
		}

		protected override void RemoveInternal(string key) {
			this.MemcachedClientInstance.Delete(key);
		}

		protected override bool ContainsInternal(string key) {
			return (this.MemcachedClientInstance.Get(key) != null);
		}

		protected override void ClearInternal() {
			this.MemcachedClientInstance.FlushAll();
		}

		protected override void AddInternal(string key, object value, ICacheDependency cacheDependency) {
			var data = this.Serialize(value);
			if (cacheDependency is ExpirationCacheDependency) {
				var expiration = (ExpirationCacheDependency)cacheDependency;
				if (expiration.Sliding) {
					this.MemcachedClientInstance.Add(key, data, expiration.SlidingExpiration);
				} else {
					this.MemcachedClientInstance.Add(key, data, expiration.AbsoluteExpiration);
				}
			} else {
				this.MemcachedClientInstance.Add(key, data);
			}
		}

		protected override void SetInternal(string key, object value, ICacheDependency cacheDependency) {
			var data = this.Serialize(value);
			if (cacheDependency is ExpirationCacheDependency) {
				var expiration = (ExpirationCacheDependency)cacheDependency;
				if (expiration.Sliding) {
					this.MemcachedClientInstance.Set(key, data, expiration.SlidingExpiration);
				} else {
					this.MemcachedClientInstance.Set(key, data, expiration.AbsoluteExpiration);
				}
			} else {
				this.MemcachedClientInstance.Set(key, data);
			}
		}

		protected override byte[] GetBytesInternal(string key) {
			return (byte[])this.MemcachedClientInstance.Get(key);
		}
	}
}
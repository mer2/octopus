using HTB.DevFx.Cache;
using Joy.Runtime.Memcached;
using Joy.Runtime.Memcached.Config;

namespace Joy.Runtime.Redis
{
	internal class RedisCache : CacheBase
	{
		private string redisServer;
		protected override void InitInternal(CacheSetting setting) {
			base.InitInternal(setting);
			var serverName = this.CacheName;
			if(setting != null) {
				if(!string.IsNullOrEmpty(setting.CacheInstanceName)) {
					serverName = setting.CacheInstanceName;
				}
			}
			this.redisServer = serverName;
		}

		protected override void RemoveInternal(string key) {
			using(var client = RedisClientFactory.GetRedisClient(this.redisServer)) {
				client.Del(key);
			}
		}

		protected override bool ContainsInternal(string key) {
			return this.GetBytesInternal(key) != null;
		}

		protected override void ClearInternal() {
			using(var client = RedisClientFactory.GetRedisClient(this.redisServer)) {
				client.FlushAll();
			}
		}

		protected override void AddInternal(string key, object value, ICacheDependency cacheDependency) {
			var data = this.Serialize(value);
			using(var client = RedisClientFactory.GetRedisClient(this.redisServer)) {
				if(cacheDependency is ExpirationCacheDependency) {
					var expiration = (ExpirationCacheDependency)cacheDependency;
					if(expiration.Sliding) {
						client.Add(key, data, expiration.SlidingExpiration);
					} else {
						client.Add(key, data, expiration.AbsoluteExpiration);
					}
				} else {
					client.Add(key, data);
				}
			}
		}

		protected override void SetInternal(string key, object value, ICacheDependency cacheDependency) {
			var data = this.Serialize(value);
			using(var client = RedisClientFactory.GetRedisClient(this.redisServer)) {
				if(cacheDependency is ExpirationCacheDependency) {
					var expiration = (ExpirationCacheDependency)cacheDependency;
					if(expiration.Sliding) {
						client.Set(key, data, expiration.SlidingExpiration);
					} else {
						client.Set(key, data, expiration.AbsoluteExpiration);
					}
				} else {
					client.Set(key, data);
				}
			}
		}

		protected override byte[] GetBytesInternal(string key) {
			using(var client = RedisClientFactory.GetRedisClient(this.redisServer)) {
				return client.Get(key);
			}
		}
	}
}

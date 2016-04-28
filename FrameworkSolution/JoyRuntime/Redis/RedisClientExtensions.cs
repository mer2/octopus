using Octopus.Esb;
using ServiceStack.Redis;
using ISerializer = Octopus.Esb.ISerializer;
using SerializerFactory = Octopus.Esb.SerializerFactory;

namespace Joy.Runtime.Redis
{
	public static class RedisClientExtensions
	{
		private static ISerializer GetSerializer(RedisClient redis) {
			var client = redis as RedisClientFactory.RedisClientPool.ServiceStatckRedisClient;
			var serializerName = "application/json; iso";
			if(client != null) {
				serializerName = client.SerializerName;
			}
			var serializer = SerializerFactory.GetSerializer(serializerName);
			return serializer ?? SerializerFactory.DefaultSerializer;
		}

		public static T GetItem<T>(this RedisClient redis, string key) {
			return redis.GetItem<T>(null, key);
		}

		public static void SetItem<T>(this RedisClient redis, string key, T value) {
			redis.SetItem(null, key, value);
		}

		public static T GetItem<T>(this RedisClient redis, ISerializer serializer, string key) {
			if(redis == null || string.IsNullOrEmpty(key)) {
				return default(T);
			}
			serializer = serializer ?? GetSerializer(redis);
			var bytes = redis.Get(key);
			return serializer.Deserialize<T>(bytes, null);
		}

		public static void SetItem<T>(this RedisClient redis, ISerializer serializer, string key, T value) {
			if(redis == null || string.IsNullOrEmpty(key)) {
				return;
			}
			serializer = serializer ?? GetSerializer(redis);
			redis.Set(key, serializer.Serialize(value, null));
		}
	}
}

using Joy.Runtime.Redis;
using Octopus.EventBus.Server;

namespace Octopus.EventBus.Data
{
	internal class RedisRepository : EventRepositoryBase
	{
		protected override IEventStorage GetStorage(string name) {
			var redis = RedisClientFactory.GetRedisClient(name);
			return new RedisEventStorage(redis, this.Encoding);
		}
	}
}

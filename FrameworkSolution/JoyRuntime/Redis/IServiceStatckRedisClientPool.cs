using ServiceStack.Redis;

namespace Joy.Runtime.Redis
{
	public interface IServiceStatckRedisClientPool
	{
		RedisClient GetRedisClient(string server);
	}
}

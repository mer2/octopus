using Sider;

namespace Octopus.EventBus.Repositories
{
	internal class SiderRedisClient : RedisClient<byte[]>
	{
		public SiderRedisClient(string host = "localhost", int port = 6379) : base(host, port) { }
	}
}

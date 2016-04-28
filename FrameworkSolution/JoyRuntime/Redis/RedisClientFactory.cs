using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using ServiceStack.Redis;

namespace Joy.Runtime.Redis
{
	public abstract class RedisClientFactory
	{
		private static readonly Dictionary<string, RedisClientPool> clientPools = new Dictionary<string, RedisClientPool>(StringComparer.InvariantCultureIgnoreCase);
		public static RedisClient GetRedisClient(string serverName) {
			if(string.IsNullOrEmpty(serverName)) {
				throw new ArgumentNullException("serverName");
			}
			var connectionStringSetting = ConfigurationManager.ConnectionStrings[serverName];
			if(connectionStringSetting != null) {
				var connectionString = connectionStringSetting.ConnectionString;
				var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
				serverName = connectionStringBuilder.DataSource;
			}
			return GetRedisClientInternal(serverName);
		}

		internal static RedisClient GetRedisClientInternal(string server) {
			if(string.IsNullOrEmpty(server)) {
				throw new ArgumentNullException("server");
			}
			RedisClientPool clientPool;
			if(!clientPools.TryGetValue(server, out clientPool)) {
				lock(clientPools) {
					if(!clientPools.TryGetValue(server, out clientPool)) {
						var host = server;
						var port = 6379;
						var s = server.Split(':');
						if(s.Length > 1) {
							host = s[0];
							if(!int.TryParse(s[1], out port)) {
								port = 6379;
							}
						}
						clientPool = new RedisClientPool(host, port);
						clientPools.Add(server, clientPool);
					}
				}
			}
			return clientPool.GetRedisClient();
		}

		internal class RedisClientPool
		{
			private readonly string host;
			private readonly int port;
			public RedisClientPool(string host, int port) {
				this.host = host;
				this.port = port;
			}

			private readonly Stack clients = Stack.Synchronized(new Stack());
			public RedisClient GetRedisClient() {
				if(this.clients.Count > 0) {
					lock(this.clients) {
						if(this.clients.Count > 0) {
							return (ServiceStatckRedisClient)this.clients.Pop();
						}
					}
				}
				return new ServiceStatckRedisClient(this);
			}

			internal class ServiceStatckRedisClient : RedisClient
			{
				private readonly RedisClientPool clientPool;
				public ServiceStatckRedisClient(RedisClientPool clientPool) : base(clientPool.host, clientPool.port) {
					this.clientPool = clientPool;
				}
	
				protected override void Dispose(bool disposing) {
					if(disposing) {
						if(this.HadExceptions) {//有错误，抛弃此对象
							base.Dispose(true);
						} else {
							this.clientPool.clients.Push(this);
						}
					} else {
						base.Dispose(false);
					}
				}

				public string SerializerName {
					get { return "application/json; iso"; }
				}
			}
		}
	}
}
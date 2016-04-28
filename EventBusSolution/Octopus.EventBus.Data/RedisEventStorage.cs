using System;
using System.Collections.Generic;
using System.Text;
using HTB.DevFx.Exceptions;
using Octopus.EventBus.Server;
using ServiceStack.Redis;

namespace Octopus.EventBus.Data
{
	internal class RedisEventStorage : RedisExecutor, IEventStorage
	{
		public RedisEventStorage(RedisClient redis, Encoding encoding) : base(redis, encoding) {
		}

		private IRedisSubscription subscription;
		private T ExecuteRedisCommandSafety<T>(Func<RedisClient, T> cmd, bool throwOnError = true) {
			try {
				return cmd(this.RedisClient);
			} catch(Exception e) {
				if(throwOnError) {
					throw;
				}
				ExceptionService.Publish(e);
			}
			return default(T);
		}

		private void ExecuteRedisCommandSafety(Action<RedisClient> cmd, bool throwOnError = true) {
			try {
				cmd(this.RedisClient);
			} catch(Exception e) {
				if(throwOnError) {
					throw;
				}
				ExceptionService.Publish(e);
			}
		}

		protected override T ExecuteRedisCommand<T>(Func<RedisClient, T> cmd) {
			return this.ExecuteRedisCommandSafety(cmd);
		}

		public void Dispose() {
			if(this.subscription != null) {
				this.subscription.UnSubscribeFromAllChannels();
				this.subscription.Dispose();
			}
			this.subscription = null;
			if(this.RedisClient != null) {
				this.RedisClient.Dispose();
			}
			this.RedisClient = null;
		}

		public void Subscribe(string queueName, Action<byte[]> handler) {
			this.ExecuteRedisCommandSafety(c => {
				var s = this.subscription = c.CreateSubscription();
				s.OnMessage = (ch, msg) => handler(this.Encoding.GetBytes(msg));
				s.SubscribeToChannels(queueName);
			});
		}

		public IEnumerable<object> Pipeline(Action<IEventStorageExecutor> handler) {
			using(var pl = this.RedisClient.CreatePipeline()) {
				var results = new List<object>();
				handler(new RedisPipelineExecutor(pl, results, this.Encoding));
				pl.Flush();
				return results;
			}
		}

		public void PersistToDatabase(EventMessage[] messages) {
			EventMessagePersister.Save(messages);
		}
	}
}
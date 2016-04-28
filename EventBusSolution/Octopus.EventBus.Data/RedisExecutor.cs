using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Octopus.EventBus.Server;
using ServiceStack.Redis;

namespace Octopus.EventBus.Data
{
	internal class RedisExecutor : IEventStorageExecutor
	{
		public RedisExecutor(RedisClient client, Encoding encoding) {
			this.RedisClient = client;
			this.Encoding = encoding;
		}
		protected virtual Encoding Encoding { get; private set; }
		protected RedisClient RedisClient;

		protected virtual T ExecuteRedisCommand<T>(Func<RedisClient, T> cmd) {
			return cmd(this.RedisClient);
		}

		protected virtual void ExecuteRedisCommand(Action<RedisClient> cmd) {
			cmd(this.RedisClient);
		}

		public virtual byte[] RightPop(string queueName) {
			return this.ExecuteRedisCommand(c => c.RPop(queueName));
		}

		public virtual byte[] RightPopLeftPush(string srcQueue, string destQueue) {
			return this.ExecuteRedisCommand(c => c.RPopLPush(srcQueue, destQueue));
		}

		public virtual void LeftPush(string queueName, byte[] value) {
			this.ExecuteRedisCommand(c => c.LPush(queueName, value));
		}

		public virtual int GetLength(string queueName) {
			return this.ExecuteRedisCommand(c => c.LLen(queueName));
		}

		public virtual byte[] Get(string key) {
			return this.ExecuteRedisCommand(c => c.Get(key));
		}

		public virtual byte[][] MultiGet(params string[] keys) {
			return this.ExecuteRedisCommand(c => c.MGet(keys));
		}

		public virtual string[] GetKeys(string pattern) {
			var bytes = this.ExecuteRedisCommand(c => c.Keys(pattern));
			return bytes.Select(bs => this.Encoding.GetString(bs)).ToArray();
		}

		public virtual void Set(string key, byte[] value) {
			this.ExecuteRedisCommand(c => c.Set(key, value));
		}

		public virtual void MultiSet(IEnumerable<KeyValuePair<string, byte[]>> values) {
			this.ExecuteRedisCommand(c => c.MSet(values.Select(y => this.Encoding.GetBytes(y.Key)).ToArray(), values.Select(y => y.Value).ToArray()));
		}

		public virtual long Increment(string key) {
			return this.ExecuteRedisCommand(c => c.Incr(key));
		}

		public virtual long Decrement(string key) {
			return this.ExecuteRedisCommand(c => c.Decr(key));
		}

		public virtual void Publish(string queueName, byte[] value) {
			this.ExecuteRedisCommand(c => c.Publish(queueName, value));
		}

		public virtual void Delete(params string[] keys) {
			this.ExecuteRedisCommand(c => c.Del(keys));
		}
	}
}

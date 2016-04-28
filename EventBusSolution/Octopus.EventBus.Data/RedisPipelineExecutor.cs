using System.Collections.Generic;
using System.Linq;
using System.Text;
using Octopus.EventBus.Server;
using ServiceStack.Redis;
using ServiceStack.Redis.Pipeline;

namespace Octopus.EventBus.Data
{
	internal class RedisPipelineExecutor : IEventStorageExecutor
	{
		public RedisPipelineExecutor(IRedisPipeline pipeline, List<object> results, Encoding encoding) {
			this.Pipeline = pipeline;
			this.Results = results;
			this.Encoding = encoding;
		}

		protected virtual Encoding Encoding { get; private set; }
		protected virtual IRedisPipeline Pipeline { get; private set; }
		protected virtual List<object> Results { get; private set; }

		public byte[] RightPop(string queueName) {
			this.Pipeline.QueueCommand(x => ((RedisClient)x).RPop(queueName), result => this.Results.Add(result));
			return null;
		}

		public byte[] RightPopLeftPush(string srcQueue, string destQueue) {
			this.Pipeline.QueueCommand(x => ((RedisClient)x).RPopLPush(srcQueue, destQueue), result => this.Results.Add(result));
			return null;
		}

		public void LeftPush(string queueName, byte[] value) {
			this.Pipeline.QueueCommand(x => ((RedisClient)x).LPush(queueName, value), result => this.Results.Add(result));
		}

		public int GetLength(string queueName) {
			this.Pipeline.QueueCommand(x => ((RedisClient)x).LLen(queueName), result => this.Results.Add(result));
			return -1;
		}

		public byte[] Get(string key) {
			this.Pipeline.QueueCommand(x => ((RedisClient)x).Get(key), result => this.Results.Add(result));
			return null;
		}

		public byte[][] MultiGet(params string[] keys) {
			this.Pipeline.QueueCommand(x => ((RedisClient)x).MGet(keys), result => this.Results.Add(result));
			return null;
		}

		public string[] GetKeys(string pattern) {
			this.Pipeline.QueueCommand(x => ((RedisClient)x).Keys(pattern), result => this.Results.Add(result.Select(x => this.Encoding.GetString(x)).ToArray()));
			return null;
		}

		public void Set(string key, byte[] value) {
			this.Pipeline.QueueCommand(x => ((RedisClient)x).Set(key, value), result => this.Results.Add(result));
		}

		public void MultiSet(IEnumerable<KeyValuePair<string, byte[]>> values) {
			this.Pipeline.QueueCommand(x => ((RedisClient)x).MSet(values.Select(y => this.Encoding.GetBytes(y.Key)).ToArray(), values.Select(y => y.Value).ToArray()), () => this.Results.Add(null));
		}

		public long Increment(string key) {
			this.Pipeline.QueueCommand(x => ((RedisClient)x).Incr(key), result => this.Results.Add(result));
			return -1;
		}

		public long Decrement(string key) {
			this.Pipeline.QueueCommand(x => ((RedisClient)x).Decr(key), result => this.Results.Add(result));
			return -1;
		}

		public void Delete(params string[] keys) {
			this.Pipeline.QueueCommand(x => ((RedisClient)x).Del(keys), result => this.Results.Add(result));
		}

		public void Publish(string queueName, byte[] value) {
			this.Pipeline.QueueCommand(x => ((RedisClient)x).Publish(queueName, value), result => this.Results.Add(result));
		}
	}
}

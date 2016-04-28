using System.Collections.Generic;

namespace Octopus.EventBus.Server
{
	public interface IEventStorageExecutor
	{
		byte[] RightPop(string queueName);
		byte[] RightPopLeftPush(string srcQueue, string destQueue);
		void LeftPush(string queueName, byte[] value);
		int GetLength(string queueName);

		byte[] Get(string key);
		byte[][] MultiGet(params string[] keys);
		string[] GetKeys(string pattern);

		void Set(string key, byte[] value);
		void MultiSet(IEnumerable<KeyValuePair<string, byte[]>> values);
		long Increment(string key);
		long Decrement(string key);
		void Delete(params string[] keys);

		void Publish(string queueName, byte[] value);
	}
}

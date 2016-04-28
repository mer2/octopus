using System;
using System.Collections.Generic;

namespace Octopus.EventBus.Server
{
	public interface IEventStorage : IEventStorageExecutor, IDisposable
	{
		void Subscribe(string queueName, Action<byte[]> handler);
		IEnumerable<object> Pipeline(Action<IEventStorageExecutor> handler);
		void PersistToDatabase(EventMessage[] messages);
	}
}

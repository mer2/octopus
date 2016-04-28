using Octopus.EventBus.Server;

namespace Octopus.EventBus.Data
{
	internal interface IEventStorageInternal : IEventStorage
	{
		string ConnectionStringName { set; }
	}
}
namespace Octopus.EventBus.Server
{
	public interface IEventDispatcher
	{
		void Init(IEventRepository repository, double interval);
		void OnMessageReceived(IEventRepository repository, object message);
	}
}

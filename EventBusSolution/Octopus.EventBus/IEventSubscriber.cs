namespace Octopus.EventBus
{
	public interface IEventSubscriber
	{
		void OnMessageReceived(IEventClientService service, EventMessage message);
	}
}
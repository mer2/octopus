namespace Octopus.EventBus.Server
{
	public interface IEventCallee
	{
		void SendMessage(IEventRepository repository, params EventMessage[] subscribers);
	}
}

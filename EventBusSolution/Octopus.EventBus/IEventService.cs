using HTB.DevFx.Core;

namespace Octopus.EventBus
{
	public interface IEventService
	{
		IAOPResult Publish(EventMessage message);
		IAOPResult RegisterSubscribers(params EventMessage[] messages);
		EventMessage Receive(params string[] channels);
		EventMessage[] Receives(int count, string channel);
		int GetMessageCount(params string[] channels);
	}
}

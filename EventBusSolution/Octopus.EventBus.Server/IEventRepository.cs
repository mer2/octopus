using HTB.DevFx.Core;

namespace Octopus.EventBus.Server
{
	public interface IEventRepository
	{
		IAOPResult PushToMainQueue(EventMessageHeader message);
		IAOPResult PushToSubscribeQueue(string queueName, EventMessageHeader message);
		IAOPResult PushToSubscribeQueues(EventMessageHeader message, string[] queueNames);
		EventMessageHeader PopFromMainQueue();
		EventMessageHeader PopFromSubscribeQueue(string queueName);
		EventMessageHeader[] PopMessagesFromSubscribeQueue(string queueName, int count);
		int GetSubscribeMessageCount(params string[] queueNames);

		IAOPResult RegisterSubscribers(EventMessage[] messages);
		EventMessage[] GetSubscribers();

		IAOPResult RegisterDispatcher(IEventDispatcher dispatcher);

		int Clear(int count);
		void RemoveUnavailableSubscribers();
	}
}

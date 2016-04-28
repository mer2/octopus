using System.Collections;
using HTB.DevFx.Core;

namespace Octopus.EventBus
{
	public interface IEventClientService
	{
		IAOPResult Publish(EventMessage message, bool sync = false);
		IAOPResult<EventMessage> PublishTo(string alias, object message, IDictionary data = null, bool sync = false, string eventName = null, int priority = 0, MessageFlags messageType = MessageFlags.Default, string messageId = null, params string[] tags);
		IAOPResult<EventMessage> Publish(object message, IDictionary data = null, bool sync = false, string category = null, string eventName = null, int priority = 0, MessageFlags messageType = MessageFlags.Default, string messageId = null, params string[] tags);
	}
}

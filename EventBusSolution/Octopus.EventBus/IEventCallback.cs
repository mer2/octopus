using HTB.DevFx.Core;

namespace Octopus.EventBus
{
	public interface IEventCallback
	{
		IAOPResult OnMessageReceived(EventMessage message);
	}
}

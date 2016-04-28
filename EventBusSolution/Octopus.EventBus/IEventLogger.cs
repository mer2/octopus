using System;
using System.Collections;

namespace Octopus.EventBus
{
	[Obsolete("该接口已经弃用，改为HTB.DevFx.Log.LogHelper来写日志")]
	public interface IEventLogger
	{
		void WriteLog(IEventClientService service, string message, IDictionary options);
	}
}
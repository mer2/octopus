using System;
using HTB.DevFx.Core;

namespace Octopus.Esb.Client
{
	public interface IHttpRealProxy
	{
		void Init();
		Type ProxyType { get; }
		string ProxyUrl { get; }
		string ContentType { get; set; }
		object GetTransparentProxy();
		event Action<ProxyContext> Calling;
		event Action<ProxyContext, IAOPResult> ResultHandling;
		event Action<ProxyContext> Called;
	}
}

using HTB.DevFx.Core;

namespace Octopus.Esb.Client
{
	public interface IHttpRealProxyFactoryInternal
	{
		void OnCalling(ProxyContext ctx);
		void OnRequest(ProxyContext ctx);
		void OnResultHandling(ProxyContext ctx, IAOPResult result);
		void OnCalled(ProxyContext ctx);
	}
}

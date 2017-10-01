using System;
using System.Collections;

namespace Octopus.Esb.Client
{
	public interface IHttpRealProxy
	{
		void Init(IHttpRealProxyFactoryInternal factory = null, IDictionary options = null);
		Type ProxyType { get; }
		string ProxyUrl { get; }
		string ContentType { get; set; }
		object GetTransparentProxy();
	}
}
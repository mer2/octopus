using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;

namespace Octopus.Esb.Client
{
	public class ProxyContext : HTB.DevFx.Reflection.CallContext
	{
		public IHttpRealProxy ProxyInstance { get; set; }
		public ISerializer Serializer { get; set; }
		public IDictionary<string, object> Parameters { get; set; }
		public IMethodCallMessage MethodCallMessage { get; set; }
	}
}
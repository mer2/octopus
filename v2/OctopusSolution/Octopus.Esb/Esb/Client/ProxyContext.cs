using System;
using System.Collections.Generic;
using System.Net;

namespace Octopus.Esb.Client
{
	public class ProxyContext : HTB.DevFx.Reflection.CallContext
	{
		public IHttpRealProxy ProxyInstance { get; set; }
		public ISerializer Serializer { get; set; }
		public IDictionary<string, object> Parameters { get; set; }
		public WebRequest WebRequest { get; set; }
		public string ProxyUrl { get; set; }//被代理的服务地址，可能是一个别名
		public Type ExpectedReturnType { get; set; }//期待返回值的类型
	}
}
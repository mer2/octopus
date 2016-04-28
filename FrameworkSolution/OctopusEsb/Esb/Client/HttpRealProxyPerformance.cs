using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Runtime.Remoting.Messaging;
using HTB.DevFx.Core;
using HTB.DevFx.Log;

namespace Octopus.Esb.Client
{
	internal class HttpRealProxyPerformance : HttpRealProxy
	{
		public HttpRealProxyPerformance(Type proxyType, string url) : base(proxyType, url) {}

		public override IMessage Invoke(IMessage msg) {
			return ElapseTime("Invoke", () => base.Invoke(msg));
		}

		protected override object Call(IDictionary<string, object> parameters, IMethodCallMessage methodMessage) {
			return ElapseTime("Call", () => base.Call(parameters, methodMessage));
		}

		protected override void PrepareRequest(HttpWebRequest request, ISerializer serializer, IDictionary<string, object> parameters, IMethodCallMessage methodMessage) {
			ElapseTime("PrepareRequest", () => base.PrepareRequest(request, serializer, parameters, methodMessage));
		}

		protected override object ResponseHandle(ProxyContext ctx, HttpWebResponse response) {
			return ElapseTime("ResponseHandle`2", () => base.ResponseHandle(ctx, response));
		}

		protected override object ResultHandle(IAOPResult aop, Type returnType, ISerializer serializer) {
			return ElapseTime("ResponseHandle`3", () => base.ResultHandle(aop, returnType, serializer));
		}

		private static T ElapseTime<T>(string message, Func<T> func) {
			var stopwatcher = new Stopwatch();
			stopwatcher.Start();
			var result = func();
			stopwatcher.Stop();
			LogService.WriteLog(typeof(HttpRealProxyPerformance), "Invoke {0}:{1}", message, stopwatcher.ElapsedMilliseconds);
			return result;
		}

		private static void ElapseTime(string message, Action func) {
			var stopwatcher = new Stopwatch();
			stopwatcher.Start();
			func();
			stopwatcher.Stop();
			LogService.WriteLog(typeof(HttpRealProxyPerformance), "Invoke {0}:{1}", message, stopwatcher.ElapsedMilliseconds);
		}
	}
}

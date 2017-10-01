using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using HTB.DevFx.Core;
using HTB.DevFx.Log;

namespace Octopus.Esb.Client
{
	internal class HttpRealProxyPerformance : HttpRealProxy
	{
		public HttpRealProxyPerformance(Type proxyType, string url, string contentType) : base(proxyType, url, contentType) {}

		protected override object Invoke(MethodInfo targetMethod, object[] args) {
			return ElapseTime("Invoke", () => base.Invoke(targetMethod, args));
		}

		protected override object Call(IDictionary<string, object> parameters, MethodInfo methodMessage) {
			return ElapseTime("Call", () => base.Call(parameters, methodMessage));
		}

		protected override void PrepareRequest(ProxyContext ctx) {
			ElapseTime("PrepareRequest", () => base.PrepareRequest(ctx));
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

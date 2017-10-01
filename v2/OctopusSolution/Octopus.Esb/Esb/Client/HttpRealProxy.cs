using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using HTB.DevFx;
using HTB.DevFx.Core;
using HTB.DevFx.Log;
using HTB.DevFx.Utils;
using RemotingException = HTB.DevFx.Remoting.RemotingException;

namespace Octopus.Esb.Client
{
	public class HttpRealProxy : DispatchProxy, IHttpRealProxy
	{
		public HttpRealProxy(Type proxyType, string url, string contentType) {
			this.ProxyType = proxyType;
			this.ProxyUrl = url;
			if (!string.IsNullOrEmpty(contentType)) {
				this.contentType = contentType;
			}
		}

		private string contentType = "application/json";
		private IHttpRealProxyFactoryInternal factory;

		public virtual string ContentType {
			get => this.contentType;
			set => this.contentType = value;
		}

		protected override object Invoke(MethodInfo targetMethod, object[] args) {
			object returnValue = null;
			if(targetMethod.DeclaringType == typeof(object)) {
				switch(targetMethod.Name) {
					case "GetType":
						returnValue = this.ProxyType;
						break;
					case "Equals":
						returnValue = this.Equals(args[0]);
						break;
					case "GetHashCode":
						returnValue = this.GetHashCode();
						break;
					case "ToString":
						returnValue = this.ProxyType.FullName;
						break;
				}
			} else {
				var parameters = this.BuildParameters(targetMethod, args);
				returnValue = this.Call(parameters, targetMethod);
			}
			return returnValue;
		}

		protected virtual IDictionary<string, object> BuildParameters(MethodInfo targetMethod, object[] args) {
			var parameters = new Dictionary<string, object>();
			var ps = targetMethod.GetParameters();
			for(var i = 0; i < ps.Length; i++) {
				parameters.Add(ps[i].Name, args[i]);
			}
			return parameters;
		}

		protected virtual ProxyContext PrepareProxyContext(IDictionary<string, object> parameters, MethodInfo targetMethod, string contentType) {
			var serializer = SerializerFactory.Current.GetSerializer(contentType) ?? SerializerFactory.Current.Default;
			var ctx = new ProxyContext {
				ProxyInstance = this,
				Serializer = serializer,
				CallMethod = targetMethod,
				Parameters = parameters,
				ProxyUrl = this.ProxyUrl,
			};
			return ctx;
		}

		protected virtual object Call(IDictionary<string, object> parameters, MethodInfo targetMethod) {
			var ctx = this.PrepareProxyContext(parameters, targetMethod, this.ContentType);
			this.Factory?.OnCalling(ctx);
			if(ctx.ResultInitialized) {
				return ctx.ResultValue;
			}
			var url = ctx.ProxyUrl;
			var request = WebRequest.Create(url);
			ctx.WebRequest = request;
			this.Factory?.OnRequest(ctx);
			this.PrepareRequest(ctx);
			if (ctx.WebRequest != null) {
				request = ctx.WebRequest;
			}
			object returnValue;
			using (var response = (HttpWebResponse) request.GetResponse()) {
				if(response.StatusCode == HttpStatusCode.OK) {
					returnValue = this.ResponseHandle(ctx, response);
				} else {
					throw new RemotingException(-1, $"Server Error: {response.StatusCode}/{response.StatusDescription}");
				}
			}
			ctx.WebRequest = null;
			this.Factory?.OnCalled(ctx);
			if(ctx.ResultInitialized) {
				returnValue = ctx.ResultValue;
			}
			return returnValue;
		}

		protected virtual void PrepareRequest(ProxyContext ctx) {
			var parameters = ctx.Parameters;
			var serializer = ctx.Serializer;
			var request = ctx.WebRequest;
			var methodInfo =ctx.CallMethod;
			parameters.Add("$method", methodInfo.ToString());
			var methodName = this.GetCallMethodName(ctx);
			parameters.Add("$action", methodName);
			request.Headers.Add("$action", methodName);
			request.ContentType = serializer.ContentType;
			request.Method = "POST";
			using(var ms = new MemoryStream()) {
				serializer.Serialize(ms, parameters, null);
				request.ContentLength = ms.Length;
				using(var requestStream = request.GetRequestStream()) {
					ms.WriteTo(requestStream);
				}
			}
		}

		protected virtual string GetCallMethodName(ProxyContext ctx) {
			return ctx.CallMethod.Name;
		}

		protected virtual Type GetExpectedReturnType(ProxyContext ctx) {
			var type = ctx.ExpectedReturnType;
			if (type == null) {
				var methodInfo = ctx.CallMethod;
				var returnType = methodInfo.ReturnType;
				if (returnType.IsInterface && returnType.IsGenericType && returnType.ToString().StartsWith("HTB.DevFx.Core.IAOPResult`1[")) {//是否为IAOPResult<>
					var esbType = typeof(AOPResult<>);
					type = esbType.MakeGenericType(returnType.GetGenericArguments());
				} else {
					type = typeof(AOPResult);
				}
			}
			return type;
		}

		protected virtual object ResponseHandle(ProxyContext ctx, HttpWebResponse response) {
			var serializer = ctx.Serializer;
			var expectedReturnType = this.GetExpectedReturnType(ctx);
			IAOPResult aop;
			using (var stream = response.GetResponseStream()) {
				if(stream == null) {
					throw new RemotingException(-1, "Server No Response");
				}
				aop = (IAOPResult)serializer.Deserialize(stream, expectedReturnType, null);
			}
			if(aop == null) {
				throw new RemotingException(-2, "Unexpected Response");
			}

			this.Factory?.OnResultHandling(ctx, aop);
			if(ctx.ResultInitialized) {
				return ctx.ResultValue;
			}
			var methodInfo = ctx.CallMethod;
			var returnType = methodInfo.ReturnType;
			return this.ResultHandle(aop, returnType, serializer);
		}

		protected virtual object ResultHandle(IAOPResult aop, Type returnType, ISerializer serializer) {
#if DEBUG
			LogService.WriteLog(this, LogLevel.DEBUG, aop.ToString());
#endif
			object returnValue = null;
			if(typeof(IAOPResult).IsAssignableFrom(returnType)) {
				returnValue = aop;
				if(returnType.IsGenericType && aop.ResultNo != 0) {//要求返回的是泛型，并且返回失败
					var aopType = aop.GetType();
					if(!aopType.IsGenericType) {//实际获取到的不是泛型，需要转换
						var rt = typeof(AOPResult<>).MakeGenericType(returnType.GetGenericArguments());
						var instance = (AOPResult)TypeHelper.CreateObject(rt, null, false);
						instance.ResultNo = aop.ResultNo;
						instance.ResultDescription = aop.ResultDescription;
						returnValue = instance;
					}
				}
			} else if(aop.ResultNo == 0) {
				if(returnType != typeof(void)) {
					if(aop.ResultAttachObject != null) {
						returnValue = returnType.IsInstanceOfType(aop.ResultAttachObject) ? aop.ResultAttachObject : serializer.Convert(aop.ResultAttachObject, returnType, null);
					}
				}
			} else {
				throw new RemotingException(aop.ResultNo, aop.ResultDescription);
			}
			return returnValue;
		}

		public virtual void Init(IHttpRealProxyFactoryInternal factory = null, IDictionary options = null) {
			this.Factory = factory;
		}

		//获取实际的代理实例
		public object GetTransparentProxy() {
			if (this.proxyInstance == null) {
				lock (this) {
					if (this.proxyInstance == null) {
						var creatorType = typeof(DispatchProxy).GetMethod("Create", BindingFlags.Static);
						var method = creatorType.MakeGenericMethod(this.ProxyType, this.GetType());
						this.proxyInstance = method.Invoke(null, new object[] { this.ProxyType, this });
					}
				}
			}
			return this.proxyInstance;
		}
		private object proxyInstance;

		public virtual IHttpRealProxyFactoryInternal Factory {
			get => this.factory ?? (this.factory = ObjectService.GetObject<IHttpRealProxyFactoryInternal>());
			private set => this.factory = value;
		}

		public virtual Type ProxyType { get; }
		public virtual string ProxyUrl { get; }
	}
}

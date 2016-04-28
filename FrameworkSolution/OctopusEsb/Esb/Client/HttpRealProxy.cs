using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using HTB.DevFx;
using HTB.DevFx.Core;
using HTB.DevFx.Log;
using HTB.DevFx.Utils;
using Octopus.Esb.Config;
using RemotingException = HTB.DevFx.Remoting.RemotingException;

namespace Octopus.Esb.Client
{
	public class HttpRealProxy : RealProxy, IHttpRealProxy, IRemotingTypeInfo
	{
		public HttpRealProxy(Type proxyType, string url) : base(proxyType) {
			this.proxyType = proxyType;
			this.url = url;
		}

		private readonly Type proxyType;
		private readonly string url;
		private string contentType = "application/bson";
		public virtual string ContentType {
			get { return this.contentType; }
			set { this.contentType = value; }
		}

		public override IMessage Invoke(IMessage msg) {
			object returnValue = null;
			IMethodCallMessage methodMessage = new MethodCallMessageWrapper((IMethodCallMessage)msg);
			if(methodMessage.MethodBase.DeclaringType == typeof(object)) {
				switch(methodMessage.MethodName) {
					case "GetType":
						returnValue = this.proxyType;
						break;
					case "Equals":
						returnValue = this.Equals(methodMessage.GetArg(0));
						break;
					case "GetHashCode":
						returnValue = this.GetHashCode();
						break;
					case "ToString":
						returnValue = this.proxyType.FullName;
						break;
				}
			} else {
				var parameters = this.BuildParameters(methodMessage);
				returnValue = this.Call(parameters, methodMessage);
			}
			return new ReturnMessage(returnValue, methodMessage.Args, methodMessage.ArgCount, methodMessage.LogicalCallContext, methodMessage);
		}

		protected virtual IDictionary<string, object> BuildParameters(IMethodCallMessage methodMessage) {
			var parameters = new Dictionary<string, object>();
			for(var i = 0; i < methodMessage.ArgCount; i++) {
				parameters.Add(methodMessage.GetArgName(i), methodMessage.GetArg(i));
			}
			return parameters;
		}

		protected virtual object Call(IDictionary<string, object> parameters, IMethodCallMessage methodMessage) {
			var serializer = SerializerFactory.Current.GetSerializer(this.ContentType) ?? SerializerFactory.Current.Default;
			var ctx = new ProxyContext {
				ProxyInstance = this,
				Serializer = serializer,
				MethodCallMessage = methodMessage,
				Parameters = parameters
			};
			if(this.Calling != null) {
				this.Calling(ctx);
			}
			if(ctx.ResultInitialized) {
				return ctx.ResultValue;
			}
			var url = this.url + "/" + methodMessage.MethodName;
			var request = (HttpWebRequest)WebRequest.Create(url);
			this.PrepareRequest(request, serializer, parameters, methodMessage);
			object returnValue;
			var response = (HttpWebResponse)request.GetResponse();
			if(response.StatusCode == HttpStatusCode.OK) {
				returnValue = this.ResponseHandle(ctx, response);
			} else {
				throw new RemotingException(-1, string.Format("Server Error: {0}/{1}", response.StatusCode, response.StatusDescription));
			}
			if(this.Called != null) {
				this.Called(ctx);
				if(ctx.ResultInitialized) {
					returnValue = ctx.ResultValue;
				}
			}
			return returnValue;
		}

		protected virtual void PrepareRequest(HttpWebRequest request, ISerializer serializer, IDictionary<string, object> parameters, IMethodCallMessage methodMessage) {
			var methodInfo = (MethodInfo)methodMessage.MethodBase;
			parameters.Add("$method", methodInfo.ToString());
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

		protected virtual object ResponseHandle(ProxyContext ctx, HttpWebResponse response) {
			var stream = response.GetResponseStream();
			if(stream == null) {
				throw new RemotingException(-1, "Server No Response");
			}
			var serializer = ctx.Serializer;
			var aop = (IAOPResult)serializer.Deserialize(stream, typeof(AOPResult), null);
			if(aop == null) {
				throw new RemotingException(-2, "Unexpected Response");
			}
			if(this.ResultHandling != null) {
				this.ResultHandling(ctx, aop);
			}
			if(ctx.ResultInitialized) {
				return ctx.ResultValue;
			}
			var methodInfo = (MethodInfo)ctx.MethodCallMessage.MethodBase;
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
				if(returnType.IsGenericType && aop.IsUnSuccess()) {//要求返回的是泛型，并且返回失败
					var aopType = aop.GetType();
					if(!aopType.IsGenericType) {//实际获取到的不是泛型，需要转换
						var rt = typeof(AOPResult<>).MakeGenericType(returnType.GetGenericArguments());
						var instance = (AOPResult)TypeHelper.CreateObject(rt, null, false);
						instance.ResultNo = aop.ResultNo;
						instance.ResultDescription = aop.ResultDescription;
						returnValue = instance;
					}
				}
			} else if(aop.IsSuccess()) {
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

		bool IRemotingTypeInfo.CanCastTo(Type fromType, object o) {
			return fromType == this.proxyType;
		}

		string IRemotingTypeInfo.TypeName {
			get { return proxyType.FullName; }
			set { }
		}

		public void Init() {
			var os = ObjectService.Current;
			var setting = os.GetObjectTypedSetting<HttpRealProxySetting>(typeof(IHttpRealProxy), false);
			if(setting != null && setting.Extenders != null) {
				foreach(var extenderSetting in setting.Extenders) {
					if(!extenderSetting.Enabled) {
						continue;
					}
					var extender = os.GetOrCreateObject<IObjectExtender<IHttpRealProxy>>(extenderSetting.TypeName);
					if(extender != null) {
						extender.Init(this);
					}
				}
			}
		}

		public Type ProxyType {
			get { return this.proxyType; }
		}

		public string ProxyUrl {
			get { return this.url; }
		}

		public event Action<ProxyContext> Calling;
		public event System.Action<ProxyContext, IAOPResult> ResultHandling;
		public event Action<ProxyContext> Called;
	}
}

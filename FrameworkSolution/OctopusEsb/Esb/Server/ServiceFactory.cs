using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using HTB.DevFx;
using HTB.DevFx.Core;
using HTB.DevFx.Exceptions;
using HTB.DevFx.Log;
using Octopus.Esb.Config;
using HTB.DevFx.Utils;

namespace Octopus.Esb.Server
{
	internal class ServiceFactory : ServiceBase<ServiceFactorySetting>, IRouteHandler, IHttpHandler, IServiceFactory
	{
		private ValueProviderFactoryCollection factories;
		private Dictionary<string, ServiceItemSetting> services;
		protected override void OnInit() {
			base.OnInit();
			this.factories = new ValueProviderFactoryCollection {
				new CustomValueProviderFactory(),
				new FormValueProviderFactory(),
				new RouteDataValueProviderFactory(),
				new QueryStringValueProviderFactory(),
			};
			var dictionary = this.services = new Dictionary<string, ServiceItemSetting>(StringComparer.OrdinalIgnoreCase);
			foreach(var item in this.Setting.ServiceItems) {
				dictionary.Add(item.Name, item);
			}
			var route = new Route(this.Setting.RouteUrl, this) {
				Defaults = new RouteValueDictionary(),
				Constraints = new RouteValueDictionary(),
				DataTokens = new RouteValueDictionary()
			};
			RouteTable.Routes.Insert(0, route);
			if (this.Setting.Extenders == null || this.Setting.Extenders.Length <= 0) {
				return;
			}
			foreach(var extenderSetting in this.Setting.Extenders) {
				if(!extenderSetting.Enabled) {
					continue;
				}
				var extender = this.ObjectService.GetOrCreateObject<IObjectExtender<IServiceFactory>>(extenderSetting.TypeName);
				if(extender != null) {
					extender.Init(this);
				}
			}
		}

		protected virtual IHttpHandler GetHttpHandler(RequestContext requestContext) {
			if(requestContext == null) {
				throw new ArgumentNullException("requestContext");
			}
			requestContext.HttpContext.Items[typeof(RequestContext)] = requestContext;
			return this;
		}

		protected internal virtual void ResultHandle(ServiceContext ctx) {
			var result = ctx.ResultValue;
			var context = ctx.HttpContext;
			var response = this.Response;
			if(response != null) {
				response(ctx);
			}
			if (ctx.Responsed) {
				return;
			}
			var serializer = SerializerFactory.Current.GetSerializer(ctx.RequestContext.HttpContext.Request.ContentType) ?? SerializerFactory.Current.Default;
			var aop = result as IAOPResult ?? AOPResult.Create(0, null, result, null);
			context.Response.Charset = context.Response.ContentEncoding != null ? context.Response.ContentEncoding.WebName : "utf-8";
			context.Response.ContentType = serializer.ContentType;
			serializer.Serialize(context.Response.OutputStream, aop, null);
		}

		protected internal virtual void ResultHandle(ServiceContext ctx, object result) {
			ctx.ResultInitialized = true;
			ctx.ResultValue = result;
			this.ResultHandle(ctx);
		}

		protected internal virtual void OnRequest(ServiceContext ctx) {
			var e = this.Request;
			if(e != null) {
				e(ctx);
			}
		}

		protected internal virtual void OnCalling(ServiceContext ctx) {
			var e = this.Calling;
			if(e != null) {
				e(ctx);
			}
		}

		protected internal virtual void OnCalled(ServiceContext ctx) {
			var e = this.Called;
			if(e != null) {
				e(ctx);
			}
		}

		protected internal virtual void OnError(ServiceContext ctx) {
			var e = ctx.Error;
			if (e == null) {
				return;
			}
			LogService.WriteLog(this, LogLevel.ERROR, "ServiceFactory Error: {0}", new[] { e });
			var errorHandler = this.Error;
			if(errorHandler != null) {
				errorHandler(ctx);
			} else {
				var errorNo = -500;
				var message = e.Message;
				var exceptionBase = FindSourceException<ExceptionBase>(e);
				if (exceptionBase != null) {
					errorNo = exceptionBase.ErrorNo;
					message = exceptionBase.Message;
				}
				this.ResultHandle(ctx, AOPResult.Failed(errorNo, message));
				ctx.Error = null;
			}
		}

		private static T FindSourceException<T>(Exception e) where T : Exception {
			var expectedExceptionType = typeof (T);
			while(e != null) {
				if(expectedExceptionType.IsInstanceOfType(e)) {
					return (T)e;
				}
				e = e.InnerException;
			}
			return null;
		}

		protected void ProcessRequest(HttpContext context) {
			var serviceContext = new ServiceContext {
				HttpContext = new HttpContextWrapper(context)
			};
			try {
				this.ProcessRequestInternal(serviceContext);
			} catch(Exception e) {
				serviceContext.Error = e;
				this.OnError(serviceContext);
				if(serviceContext.Error != null) {
					throw serviceContext.Error;
				}
			}
		}

		protected virtual void ProcessRequestInternal(ServiceContext ctx) {
			var context = ctx.HttpContext;
			var requestContext = ctx.RequestContext = context.Items[typeof(RequestContext)] as RequestContext;
			if(requestContext == null) {
				this.ResultHandle(ctx, AOPResult.Failed(-500, "requestContext missing"));
				return;
			}
			var serviceName = requestContext.RouteData.GetRequiredString("service");
			if(string.IsNullOrEmpty(serviceName)) {
				this.ResultHandle(ctx, AOPResult.Failed(-500, "service missing"));
				return;
			}
			if(!this.services.ContainsKey(serviceName)) {
				this.ResultHandle(ctx, AOPResult.Failed(-500, "service not found:" + serviceName));
				return;
			}
			var setting = ctx.Setting = this.services[serviceName];
			if(setting.ServiceHandler == null) {
				lock(setting) {
					if(setting.ServiceHandler == null) {
						setting.ServiceHandler = this.CreateHandler(setting);
					}
				}
			}
			var serviceHandler = setting.ServiceHandler;
			if(serviceHandler == null) {
				this.ResultHandle(ctx, AOPResult.Failed(-500, "serviceHandler not found"));
				return;
			}
			ctx.ValueProvider = this.factories.GetValueProvider(requestContext);
			serviceHandler.ProcessRequest(ctx);
		}

		protected virtual ServiceHandler CreateHandler(ServiceItemSetting setting) {
			var contractTypeName = ObjectService.GetTypeName(setting.ContractTypeName);
			var contractType = TypeHelper.CreateType(contractTypeName, false);
			if(contractType == null || !contractType.IsInterface) {
				throw new ArgumentException("ContractType can't be created.", contractTypeName);
			}
			setting.ContractType = contractType;
			return new ServiceHandler(contractType, this);
		}

		#region Events

		protected virtual event Action<ServiceContext> Request;
		protected virtual event Action<ServiceContext> Calling;
		protected virtual event Action<ServiceContext> Called;
		protected virtual event Action<ServiceContext> Response;
		protected virtual event Action<ServiceContext> Error;

		#endregion

		#region IHttpHandler Members

		void IHttpHandler.ProcessRequest(HttpContext context) {
			this.ProcessRequest(context);
		}

		bool IHttpHandler.IsReusable { get { return true; } }

		#endregion

		#region IRouteHandler Members

		IHttpHandler IRouteHandler.GetHttpHandler(RequestContext requestContext) {
			return this.GetHttpHandler(requestContext);
		}

		#endregion

		#region IServiceFactory Members

		event Action<ServiceContext> IServiceFactory.Request {
			add { this.Request += value; }
			remove { this.Request -= value; }
		}

		event Action<ServiceContext> IServiceFactory.Calling {
			add { this.Calling += value; }
			remove { this.Calling -= value; }
		}

		event Action<ServiceContext> IServiceFactory.Called {
			add { this.Called += value; }
			remove { this.Called -= value; }
		}

		event Action<ServiceContext> IServiceFactory.Response {
			add { this.Response += value; }
			remove { this.Response -= value; }
		}

		event Action<ServiceContext> IServiceFactory.Error {
			add { this.Error += value; }
			remove { this.Error -= value; }
		}

		#endregion
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HTB.DevFx;
using HTB.DevFx.Core;
using HTB.DevFx.Esb;
using HTB.DevFx.Exceptions;
using HTB.DevFx.Log;
using Octopus.Esb.Config;
using HTB.DevFx.Utils;
using Microsoft.AspNetCore.Http;

namespace Octopus.Esb.Server
{
	internal class ServiceFactory : ServiceBase<ServiceFactorySetting>, IServiceFactory
	{
		private ValueProviderFactoryCollection factories;
		private Dictionary<string, ServiceItemSetting> services;
		private Regex regexRoute;
		protected override void OnInit() {
			base.OnInit();
			this.factories = new ValueProviderFactoryCollection {
				new CustomValueProviderFactory(),
				new FormValueProviderFactory(),
				new QueryStringValueProviderFactory()
			};
			var dictionary = this.services = new Dictionary<string, ServiceItemSetting>(StringComparer.OrdinalIgnoreCase);
			foreach(var item in this.Setting.ServiceItems) {
				dictionary.Add(item.Name, item);
			}

			this.regexRoute = new Regex(this.Setting.PathRegex, RegexOptions.Compiled | RegexOptions.Singleline);
			if (this.Setting.Extenders == null || this.Setting.Extenders.Length <= 0) {
				return;
			}
			foreach(var extenderSetting in this.Setting.Extenders) {
				if(!extenderSetting.Enabled) {
					continue;
				}
				var extender = this.ObjectService.GetOrCreateObject<IObjectExtender<IServiceFactory>>(extenderSetting.TypeName);
				extender?.Init(this);
			}
		}

		protected internal virtual void ResultHandle(ServiceContext ctx) {
			var result = ctx.ResultValue;
			var context = ctx.HttpContext;
			this.Response?.Invoke(ctx);
			if (ctx.Responsed) {
				return;
			}
			var contentType = ctx.HttpContext.Request.ContentType;
			var serializer = SerializerFactory.Current.GetSerializer(contentType) ?? SerializerFactory.Current.Default;
			var aop = result as IAOPResult ?? AOPResult.Create(0, null, result, null);
			context.Response.ContentType = serializer.ContentType;
			serializer.Serialize(context.Response.Body, aop, new Hashtable {{ "ContentType", contentType } });
		}

		protected internal virtual void ResultHandle(ServiceContext ctx, object result) {
			ctx.ResultInitialized = true;
			ctx.ResultValue = result;
			this.ResultHandle(ctx);
		}

		protected internal virtual void OnRequest(ServiceContext ctx) {
			this.Request?.Invoke(ctx);
		}

		protected internal virtual void OnCalling(ServiceContext ctx) {
			this.Calling?.Invoke(ctx);
		}

		protected internal virtual void OnCalled(ServiceContext ctx) {
			this.Called?.Invoke(ctx);
		}

		protected internal virtual void OnError(ServiceContext ctx) {
			var e = ctx.Error;
			if (e == null) {
				return;
			}
			/*string debugFileName = null;
			if (this.Setting.Debug) {
				try {
					debugFileName = $"{DateTime.Now.Ticks}-{ctx.HttpContext.TraceIdentifier}.log";
					var filePath = FileHelper.GetPhysicalPath("~/", "../Logs/Error/Esb/", true);
					ctx.HttpContext.Request.Body.SaveAs(filePath + debugFileName, true);
				} catch (Exception ex) {
					ExceptionService.Publish(ex);
				}
			}
			LogService.WriteLog(this, LogLevel.ERROR, $"ServiceFactory Error: {debugFileName}\r\n{e}");*/

			LogService.WriteLog(this, LogLevel.ERROR, $"ServiceFactory Error:\r\n{e}");
			var errorHandler = this.Error;
			if(errorHandler != null) {
				errorHandler(ctx);
			} else {
				var errorNo = -500;
				var message = e.Message;
				var exceptionBase = FindSourceException<ExceptionBase>(e);
				if (exceptionBase != null) {
					if (exceptionBase.ErrorNo != 0) {
						errorNo = exceptionBase.ErrorNo;
					}
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

		public bool IsHandleable(HttpContext context) {
			var path = context?.Request.Path;
			if (path == null || !path.Value.HasValue) {
				return false;
			}
			var matched = path.Value.Value.StartsWith(this.Setting.RouteUrl, StringComparison.OrdinalIgnoreCase);
			return matched;
		}

		internal static readonly AsyncLocal<ServiceContext> ServiceContextCurrent = new AsyncLocal<ServiceContext>();
		public void ProcessRequest(HttpContext context) {
			var serviceContext = new ServiceContext {
				HttpContext = context
			};
			ServiceContextCurrent.Value = serviceContext;
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

		protected virtual IAOPResult<ServiceHandler> GetServiceHandler(ServiceContext ctx) {
			var httpContext = ctx.HttpContext;
			if (httpContext.Request.Path.HasValue) {
				var path = httpContext.Request.Path.Value;
				var match = this.regexRoute.Match(path);
				if (match != null) {
					ctx.ServiceName = match.Groups["serviceName"]?.Value;
					ctx.MethodName = match.Groups["methodName"]?.Value;
				}
			}
			var serviceName = ctx.ServiceName;
			if(string.IsNullOrEmpty(serviceName)) {
				return AOPResult.Failed<ServiceHandler>(-500, "service missing");
			}
			if(!this.services.ContainsKey(serviceName)) {
				return AOPResult.Failed<ServiceHandler>(-500, "service not found:" + serviceName);
			}
			var setting = ctx.Setting = this.services[serviceName];
			if(setting.ServiceHandler == null) {
				lock(setting) {
					if(setting.ServiceHandler == null) {
						setting.ServiceHandler = this.CreateHandler(setting);
					}
				}
			}
			return AOPResult.Success(setting.ServiceHandler);
		}

		protected virtual void ProcessRequestInternal(ServiceContext ctx) {
			var result = this.GetServiceHandler(ctx);
			if (result.ResultNo != 0) {
				this.ResultHandle(ctx, AOPResult.Failed(result.ResultNo, result.ResultDescription));
				return;
			}
			var serviceHandler = result.ResultAttachObject;
			if(serviceHandler == null) {
				this.ResultHandle(ctx, AOPResult.Failed(-500, "serviceHandler not found"));
				return;
			}
			ctx.ValueProvider = this.factories.GetValueProvider(ctx.HttpContext);
			serviceHandler.ProcessRequest(ctx);
		}

		protected virtual ServiceHandler CreateHandler(ServiceItemSetting setting) {
			var contractTypeName = this.ObjectService.GetTypeName(setting.ContractTypeName);
			var contractType = TypeHelper.CreateType(contractTypeName, false);
			if(contractType == null || !contractType.IsInterface) {
				throw new ArgumentException("ContractType can't be created.", contractTypeName);
			}
			setting.ContractType = contractType;
			return new ServiceHandler(setting, contractType, this);
		}

		protected virtual IDictionary<string, ServiceItemSetting> GetServiceItemSettings() {
			return this.services;
		}

		#region IMiddleware Members

		public Task InvokeAsync(HttpContext context, RequestDelegate next) {
			this.ProcessRequest(context);
			return Task.CompletedTask;
		}

		#endregion

		#region Events

		protected virtual event Action<ServiceContext> Request;
		protected virtual event Action<ServiceContext> Calling;
		protected virtual event Action<ServiceContext> Called;
		protected virtual event Action<ServiceContext> Response;
		protected virtual event Action<ServiceContext> Error;

		#endregion

		#region IServiceFactory Members

		IDictionary<string, ServiceItemSetting> IServiceFactory.GetServiceItemSettings() {
			return this.GetServiceItemSettings();
		}

		event Action<ServiceContext> IServiceFactory.Request {
			add => this.Request += value;
			remove => this.Request -= value;
		}

		event Action<ServiceContext> IServiceFactory.Calling {
			add => this.Calling += value;
			remove => this.Calling -= value;
		}

		event Action<ServiceContext> IServiceFactory.Called {
			add => this.Called += value;
			remove => this.Called -= value;
		}

		event Action<ServiceContext> IServiceFactory.Response {
			add => this.Response += value;
			remove => this.Response -= value;
		}

		event Action<ServiceContext> IServiceFactory.Error {
			add => this.Error += value;
			remove => this.Error -= value;
		}

		#endregion
	}
}

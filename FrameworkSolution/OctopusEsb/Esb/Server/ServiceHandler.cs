using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using HTB.DevFx;
using HTB.DevFx.Core;
using HTB.DevFx.Utils;

namespace Octopus.Esb.Server
{
	internal class ServiceHandler
	{
		public ServiceHandler(Type contractType, ServiceFactory serviceFactory) {
			this.ServiceFactory = serviceFactory;
			this.InitService(contractType);
		}
		protected Dictionary<string, MethodInfo> Methods { get; private set; }
		protected Dictionary<string, MethodInfo> AllMethods { get; private set; }
		protected ServiceFactory ServiceFactory { get; private set; }

		protected void InitService(Type contractType) {
			var dict = this.Methods = new Dictionary<string, MethodInfo>(StringComparer.OrdinalIgnoreCase);
			var allMethods = this.AllMethods = new Dictionary<string, MethodInfo>();
			var methods = contractType.GetMethods();
			foreach (var method in methods) {
				allMethods.Add(method.ToString(), method);
				var name = method.Name;
				if(method.IsDefined(typeof(MethodNameAttribute), true)) {
					var methodName = (method.GetCustomAttributes(typeof (MethodNameAttribute), true) as MethodNameAttribute[]).FirstOrDefault();
					if (methodName != null) {
						name = methodName.Name;
					}
				}
				if(!dict.ContainsKey(name)) {
					dict.Add(name, method);
				} else {
					dict.Remove(name);
				}
			}
		}

		protected virtual void ResultHandle(ServiceContext ctx) {
			this.ServiceFactory.ResultHandle(ctx);
		}

		protected virtual void ResultHandle(ServiceContext ctx, object result) {
			this.ServiceFactory.ResultHandle(ctx, result);
		}

		protected virtual object ConvertTo(ValueProviderResult vpResult, Type type) {
			if(vpResult == null) {
				vpResult = new ValueProviderResult(null, null);
			}
			return vpResult.ConvertTo(type);
		}

		public void ProcessRequest(ServiceContext ctx) {
			this.ServiceFactory.OnRequest(ctx);
			if(ctx.ResultInitialized) {
				this.ResultHandle(ctx);
				return;
			}
			var requestContext = ctx.RequestContext;
			var valueProvider = ctx.ValueProvider;
			var setting = ctx.Setting;

			var method = ctx.CallMethod;
			var methodName = requestContext.RouteData.GetRequiredString("method");
			if(method == null) {
				if(string.IsNullOrEmpty(methodName)) {
					this.ResultHandle(ctx, AOPResult.Failed(-500, "method missing"));
					return;
				}
				if(!this.Methods.ContainsKey(methodName)) {
					var vpResult = valueProvider.GetValue("$method");
					if(vpResult != null) {
						var methodFullName = vpResult.ConvertTo<string>();
						if(!string.IsNullOrEmpty(methodFullName) && this.AllMethods.ContainsKey(methodFullName)) {
							method = this.AllMethods[methodFullName];
						}
					}
				} else {
					method = this.Methods[methodName];
				}
				ctx.CallMethod = method;
			}
			if(method == null) {
				this.ResultHandle(ctx, AOPResult.Failed(-500, "method not found:" + methodName));
				return;
			}
			var serviceInstance = ctx.ObjectInstance;
			if(serviceInstance == null) {
				var serviceTypeName = setting.ServiceTypeName;
				ctx.ObjectInstance = serviceInstance = !string.IsNullOrEmpty(serviceTypeName) ? ObjectService.Current.GetOrCreateObject(serviceTypeName) : ObjectService.GetObject(setting.ContractType);
			}
			if(serviceInstance == null || !setting.ContractType.IsInstanceOfType(serviceInstance)) {
				this.ResultHandle(ctx, AOPResult.Failed(-500, "serviceInstance error"));
				return;
			}
			var parameterValues = ctx.CallArguments;
			if(parameterValues == null) {
				var parameters = method.GetParameters();
				ctx.CallArguments = parameterValues = parameters.Select(parameter => this.ConvertTo(valueProvider.GetValue(parameter.Name), parameter.ParameterType)).ToArray();
			}
			this.ServiceFactory.OnCalling(ctx);
			if(!ctx.ResultInitialized) {
				object returnValue;
				TypeHelper.TryInvoke(serviceInstance, method, out returnValue, true, parameterValues);
				ctx.ResultInitialized = true;
				ctx.ResultValue = returnValue;
			}
			this.ServiceFactory.OnCalled(ctx);
			this.ResultHandle(ctx);
		}
	}
}

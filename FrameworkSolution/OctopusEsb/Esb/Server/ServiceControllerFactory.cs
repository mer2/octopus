using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using HTB.DevFx;
using HTB.DevFx.Core;
using HTB.DevFx.Utils;
using Octopus.Esb.Config;
using Octopus.Esb.Server;

[assembly: PreApplicationStartMethod(typeof(ServiceControllerFactory), "PreApplicationStart")]

namespace Octopus.Esb.Server
{
	public class ServiceControllerFactory : DefaultControllerFactory, IInitializable<ServiceControllerFactorySetting>
	{
		private Dictionary<string, ServiceControllerSetting> services;
		private ModuleBuilder moduleBuilder;
		private AssemblyBuilder assemblyBuilder;

		public void Init(ServiceControllerFactorySetting setting) {
			var controllerTypeName = ObjectService.GetTypeName(setting.ControllerBaseTypeName);
			this.services = setting.ServiceControllers.ToDictionary(k => k.Name.ToLower(),
				v => {
					v.ControllerBaseTypeName = string.IsNullOrEmpty(v.ControllerBaseTypeName) ? controllerTypeName : ObjectService.GetTypeName(v.ControllerBaseTypeName);
					return v;
				});

			var domain = Thread.GetDomain();
			var name = new AssemblyName("Octopus.Esb.Server.Mvc.DynamicControllers");
			this.assemblyBuilder = domain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
			this.moduleBuilder = this.assemblyBuilder.DefineDynamicModule("DynamicModule");
			var routeName = this.GetType().Name;
			RouteTable.Routes.MapRoute(
				routeName, setting.RouteUrl
			).DataTokens[routeName] = true;
		}

		protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType) {
			if(controllerType != null) {
				var setting = requestContext.HttpContext.Items[controllerType] as ServiceControllerSetting;
				if(setting != null && setting.Initialized && setting.ContractType != null) {
					var instance = string.IsNullOrEmpty(setting.ServiceTypeName) ? ObjectService.GetObject(setting.ContractType) : ObjectService.GetObject(setting.ServiceTypeName);
					if(instance != null) {
						return (IController)Activator.CreateInstance(controllerType, new[] { instance });
					}
				}
			}
			return base.GetControllerInstance(requestContext, controllerType);
		}

		protected override Type GetControllerType(RequestContext requestContext, string controllerName) {
			var type = base.GetControllerType(requestContext, controllerName);
			var routeName = this.GetType().Name;
			var ctrlName = controllerName.ToLower();
			if(type == null && requestContext.RouteData.DataTokens.ContainsKey(routeName) && this.services.ContainsKey(ctrlName)) {
				var setting = this.services[ctrlName];
				if(!setting.Initialized) {
					lock (setting) {
						if(!setting.Initialized) {
							this.CreateControllerType(setting);
						}
					}
				}
				type = setting.ControllerType;
				if(type != null) {
					requestContext.HttpContext.Items[type] = setting;
				}
			}
			return type;
		}

		protected virtual void CreateControllerType(ServiceControllerSetting setting) {
			var contractTypeName = ObjectService.GetTypeName(setting.ContractTypeName);
			var contractType = TypeHelper.CreateType(contractTypeName, false);
			if(contractType == null || !contractType.IsInterface) {
				goto EXIT;
			}
			setting.ContractType = contractType;
			var baseTypeName = setting.ControllerBaseTypeName;
			var baseType = TypeHelper.CreateType(baseTypeName, typeof (ServiceControllerBase), false);
			if(baseType == null) {
				goto EXIT;
			}
			var typeBuilder = this.moduleBuilder.DefineType("Octopus.Esb.Server.Mvc.DynamicControllers." + contractType.FullName, TypeAttributes.Public | TypeAttributes.Class, baseType, new Type[] { });
			var ctor = typeBuilder.DefineMethod(".ctor", MethodAttributes.Public, CallingConventions.Standard, typeof(void), new[] { typeof(object) });
			var ctorBase = baseType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { }, null);
			var setServiceInstance = baseType.GetMethod("set_ServiceInstance", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(object) }, null);
			var getServiceInstance = baseType.GetMethod("get_ServiceInstance", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { }, null);
			var il = ctor.GetILGenerator();
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Call, ctorBase);
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Callvirt, setServiceInstance);
			il.Emit(OpCodes.Ret);
			var methods = contractType.GetMethods();
			foreach(var methodInfo in methods) {
				var parameters = methodInfo.GetParameters();
				var resultType = typeof(ActionResult);
				var resulHandle = baseType.GetMethod("ResultHandle", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(object) }, null);
				var methodBuilder = typeBuilder.DefineMethod(methodInfo.Name, MethodAttributes.Public | MethodAttributes.Virtual, CallingConventions.Standard, resultType, parameters.Select(parameterInfo => parameterInfo.ParameterType).ToArray());
				for(var i = 0; i < parameters.Length; i++) {
					var parameterInfo = parameters[i];
					methodBuilder.DefineParameter(i + 1, ParameterAttributes.None, parameterInfo.Name);
				}
				il = methodBuilder.GetILGenerator();
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Call, getServiceInstance);
				if(parameters.Length > 3) {
					il.Emit(OpCodes.Ldarg_1);
					il.Emit(OpCodes.Ldarg_2);
					il.Emit(OpCodes.Ldarg_3);
					for(var i = 4; i <= parameters.Length; i++) {
						il.Emit(OpCodes.Ldarg_S, i);
					}
				} else {
					switch(parameters.Length) {
						case 1:
							il.Emit(OpCodes.Ldarg_1);
							break;
						case 2:
							il.Emit(OpCodes.Ldarg_1);
							il.Emit(OpCodes.Ldarg_2);
							break;
						case 3:
							il.Emit(OpCodes.Ldarg_1);
							il.Emit(OpCodes.Ldarg_2);
							il.Emit(OpCodes.Ldarg_3);
							break;
					}
				}
				il.Emit(OpCodes.Callvirt, methodInfo);
				if(methodInfo.ReturnType == typeof(void)) {
					il.Emit(OpCodes.Ldnull);
				}
				il.Emit(OpCodes.Callvirt, resulHandle);
				il.Emit(OpCodes.Ret);
			}
			setting.ControllerType = typeBuilder.CreateType();

		EXIT:
			setting.Initialized = true;
		}
	
		public static void PreApplicationStart() {
			var factory = ObjectService.GetObject<ServiceControllerFactory>();
			if(factory != null) {
				ControllerBuilder.Current.SetControllerFactory(factory);
			}
		}
	}
}

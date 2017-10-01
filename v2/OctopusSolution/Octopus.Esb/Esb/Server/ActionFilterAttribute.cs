using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HTB.DevFx.Reflection;

namespace Octopus.Esb.Server
{
	[Serializable, AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Interface, AllowMultiple = true)]
	public class ActionFilterAttribute : Attribute
	{
		public ActionFilterAttribute() {}
		public ActionFilterAttribute(string action, string category) {
			this.Action = action;
			this.Category = category;
		}

		public string Action { get; set; }
		public string Category { get; set; }
		public string Data { get; set; }
		public int OrderIndex { get; set; }

		private static readonly Dictionary<MethodInfo, ActionFilterAttribute[]> FilterAttributes = new Dictionary<MethodInfo, ActionFilterAttribute[]>();
		public static ActionFilterAttribute[] GetFilterAttributes(CallContext ctx) {
			var method = ctx.CallMethod;
			ActionFilterAttribute[] filters;
			if(!FilterAttributes.TryGetValue(method, out filters)) {
				lock(method) {
					if(!FilterAttributes.TryGetValue(method, out filters)) {
						var attributes = GetFilterAttributesInternal(ctx);
						if(attributes != null) {
							filters = attributes.ToArray();
						}
						lock(FilterAttributes) {
							FilterAttributes.Add(method, filters);
						}
					}
				}
			}
			return filters;
		}

		public static T[] GetTypedFilters<T>(CallContext ctx) where T : class {
			var filters = GetFilterAttributes(ctx);
			var list = new List<T>();
			if (filters != null && filters.Length > 0) {
				foreach (var filter in filters) {
					var t = filter as T;
					if (t != null) {
						list.Add(t);
					}
				}
			}
			return list.ToArray();
		}

		private static List<ActionFilterAttribute> GetFilterAttributesInternal(CallContext ctx) {
			var method = ctx.CallMethod;
			if(method == null) {
				return null;
			}
			var interfaceType = method.DeclaringType;
			var methodName = method.ToString();
			var index = methodName.IndexOf(' ') + 1;
			var methodInterfaceName = methodName.Substring(0, index) + interfaceType.FullName + "." + methodName.Substring(index);
			var list = new List<ActionFilterAttribute>();
			var objectType = ctx.ObjectInstance.GetType();
			var interfaceMethod = objectType.GetInterfaceMap(interfaceType).TargetMethods.Where(m => { var n = m.ToString(); return n == methodInterfaceName || n == methodName; }).Single();
			var attributes = interfaceMethod.GetCustomAttributes(typeof(ActionFilterAttribute), true) as ActionFilterAttribute[];
			if(attributes != null && attributes.Length > 0) {
				list.AddRange(attributes.OrderBy(k => k.OrderIndex).ToArray());
			}
			attributes = objectType.GetCustomAttributes(typeof(ActionFilterAttribute), true) as ActionFilterAttribute[];
			if(attributes != null && attributes.Length > 0) {
				list.AddRange(attributes.OrderBy(k => k.OrderIndex).ToArray());
			}

			attributes = method.GetCustomAttributes(typeof(ActionFilterAttribute), true) as ActionFilterAttribute[];
			if(attributes != null && attributes.Length > 0) {
				list.AddRange(attributes.OrderBy(k => k.OrderIndex).ToArray());
			}
			attributes = interfaceType.GetCustomAttributes(typeof(ActionFilterAttribute), true) as ActionFilterAttribute[];
			if(attributes != null && attributes.Length > 0) {
				list.AddRange(attributes.OrderBy(k => k.OrderIndex).ToArray());
			}
			return list;
		}
	}
}

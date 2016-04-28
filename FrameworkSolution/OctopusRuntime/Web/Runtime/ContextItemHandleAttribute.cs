using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Octopus.Web.Runtime
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
	internal class ContextItemHandleAttribute : FilterAttribute, IAuthorizationFilter
	{
		internal static void Startup() {
			GlobalFilters.Filters.Add(new ContextItemHandleAttribute());
		}

		private static void StoreValues(ControllerContext filterContext, IEnumerable<object> attributes) {
			foreach(var attribute in attributes) {
				var itemArribute = attribute as ContextItemAttribute;
				if(itemArribute == null) {
					continue;
				}
				var options = filterContext.HttpContext.Items[itemArribute.ContextKey] as IDictionary;
				if(options == null) {
					options = new Hashtable();
					filterContext.HttpContext.Items[itemArribute.ContextKey] = options;
				}
				var valueName = itemArribute.ValueName;
				var itemValue = itemArribute.ItemValue;
				if(!string.IsNullOrEmpty(valueName)) {//需要从上下文中获取值
					var valueResult = filterContext.Controller.ValueProvider.GetValue(valueName);
					if(valueResult != null) {
						itemValue = valueResult.RawValue;
					}
				}
				options[itemArribute.ItemKey] = itemValue;
			}
		}

		public void OnAuthorization(AuthorizationContext filterContext) {
			if (filterContext == null) {
				return;
			}
			var attributes = filterContext.ActionDescriptor.ControllerDescriptor.GetCustomAttributes(typeof (ContextItemAttribute), true);
			StoreValues(filterContext, attributes);
			attributes = filterContext.ActionDescriptor.GetCustomAttributes(typeof(ContextItemAttribute), true);
			StoreValues(filterContext, attributes);
		}
	}
}
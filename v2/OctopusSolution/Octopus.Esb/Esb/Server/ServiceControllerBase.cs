using HTB.DevFx.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

namespace Octopus.Esb.Server
{
	public abstract class ServiceControllerBase : Controller
	{
		protected virtual object ServiceInstance { get; set; }

		protected virtual ActionResult ResultHandle(object result) {
			var aop = result as IAOPResult ?? AOPResult.Create(0, null, result, null);
			var json = JsonConvert.SerializeObject(aop, Formatting.None, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
			return this.Content(json, "application/json");
		}

		protected override void OnException(ExceptionContext filterContext) {
			filterContext.Result = this.ResultHandle(AOPResult.Failed(filterContext.Exception.Message));
			filterContext.ExceptionHandled = true;
			base.OnException(filterContext);
		}
	}
}
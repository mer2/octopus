using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Newtonsoft.Json;

namespace Octopus.Web.Controllers
{
	public static class ControllerExtensions
	{
		public static string GetReturnUrl(string returnUrl, bool returnDefaultUrl = true) {
			var domain = FormsAuthentication.CookieDomain;
			if(!string.IsNullOrEmpty(returnUrl)) {
				Uri uri;
				if(Uri.TryCreate(returnUrl, UriKind.RelativeOrAbsolute, out uri)) {
					if(uri.IsAbsoluteUri) {
						if(!uri.Host.EndsWith(domain)) {
							returnUrl = null;
						}
					}
				} else {
					returnUrl = null;
				}
			}
			if(string.IsNullOrEmpty(returnUrl) && returnDefaultUrl) {
				returnUrl = "http://www." + (domain.StartsWith(".") ? domain.Substring(0) : domain);
			}
			return returnUrl;
		}

		/// <summary>
		/// 判断是否为本域名访问，防止非法访问
		/// </summary>
		public static bool IsAjaxRequestAllowed(this HttpRequestBase request) {
			//判断是否为本域名访问，防止非法访问
			var referrer = request.UrlReferrer;
			return referrer != null && GetReturnUrl(referrer.ToString(), false) != null;
		}

		public static ActionResult Ajax(this Controller controller, string callback, Func<object> resultHandler, object failedResult = null) {
			failedResult = failedResult ?? new { ResultNo = -1, ResultDescription = "非法请求" };
			var result = controller.Request.IsAjaxRequestAllowed() ? resultHandler() : failedResult;
			var json = JsonConvert.SerializeObject(result);
			var contentType = "application/json";
			if(!string.IsNullOrEmpty(callback)) {
				json = string.Format("try {{ {0}({1}); }} catch(e) {{ }}", callback, json);
				contentType = "application/javascript";
			}
			return new ContentResult { Content = json, ContentType = contentType };
		}
	}
}

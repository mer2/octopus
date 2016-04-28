using System;
using System.Web;
using System.Web.Routing;
using System.Web.Security;
using HTB.DevFx.Core;
using HTB.DevFx.Utils;
using Octopus.Applications;
using Octopus.Web.Config;

namespace Octopus.Web.AdminBase
{
	internal class AuthenticationService : ServiceBase<AuthenticationServiceSetting>, IRouteHandler, IHttpHandler
	{
		protected override void OnInit() {
			base.OnInit();
			var route = new Route(this.Setting.RouteUrl, this) {
				Defaults = new RouteValueDictionary(),
				Constraints = new RouteValueDictionary(),
				DataTokens = new RouteValueDictionary()
			};
			RouteTable.Routes.Insert(0, route);
		}

		public virtual void ProcessRequest(HttpContext ctx) {
			var requestContext = ctx.Items[typeof(RequestContext)] as RequestContext;
			if(requestContext == null) {
				throw new ArgumentNullException("ctx");
			}
			var controllerName = requestContext.RouteData.GetRequiredString("controller");
			if(string.Compare(controllerName, "Account", StringComparison.OrdinalIgnoreCase) != 0) {
				ctx.Response.Write("Invalid Request.");
				return;
			}
			var actionName = requestContext.RouteData.GetRequiredString("action");
			if(string.Compare(actionName, "LogOn", StringComparison.OrdinalIgnoreCase) == 0) {
				//需要转到统一后台进行登录，ReturnUrl需要转成完整的URL包括http://
				var returnUrl = this.GetReturnUrl(ctx, UriKind.Absolute);
				returnUrl = string.Format(this.Setting.LoginUrl, ApplicationHelper.GetCurrentAppNo(), ctx.Server.UrlEncode(returnUrl));
				ctx.Response.Redirect(returnUrl);
			} else if(string.Compare(actionName, "SignOut", StringComparison.OrdinalIgnoreCase) == 0) {
				FormsAuthentication.SignOut();
				ctx.Response.Redirect("~/");
			} else {
				ctx.Response.Write("Invalid Request.");
			}
		}

		/// <summary>
		/// 获取基地址，包含http://
		/// </summary>
		protected string GetBaseUrl(HttpContext ctx) {
			var baseUrl = this.Setting.ApplicationBaseUrl;
			if(string.IsNullOrEmpty(baseUrl)) {
				var uri = ctx.Request.Url;
				if(uri.IsLoopback) {//获取真正的访问地址
					baseUrl = uri.GetLeftPart(UriPartial.Scheme) + ctx.Request.Headers["HOST"];
				} else {
					baseUrl = uri.GetLeftPart(UriPartial.Authority);
				}
			}
			if(baseUrl.EndsWith("/")) {
				baseUrl = baseUrl.Remove(baseUrl.Length - 1, 1);
			}
			return baseUrl;
		}

		protected string GetReturnUrl(HttpContext ctx, UriKind uriKind = UriKind.RelativeOrAbsolute) {
			var returnUrl = ctx.Request.QueryString["ReturnUrl"];
			var makeRelative = true;
			if(!string.IsNullOrEmpty(returnUrl)) {
				Uri returnUri;
				if(Uri.TryCreate(returnUrl, UriKind.RelativeOrAbsolute, out returnUri)) {
					if(returnUri.IsAbsoluteUri) {
						var isHostDomain = String.Compare(returnUri.Host, this.Setting.CookieDomain, StringComparison.OrdinalIgnoreCase) == 0;
						if(!isHostDomain && !returnUri.Host.EndsWith("." + this.Setting.CookieDomain, true, null)) {
							returnUrl = "";
						} else {
							makeRelative = false;
						}
					}
				}
			}
			if(makeRelative) {
				returnUrl = WebHelper.MakeUrlRelative(returnUrl, ctx.Request.ApplicationPath);
				if(uriKind == UriKind.Absolute) {
					returnUrl = this.GetBaseUrl(ctx) + WebHelper.UrlCombine(ctx.Request.ApplicationPath, returnUrl, true);
				}
			}
            return returnUrl;
		}

		IHttpHandler IRouteHandler.GetHttpHandler(RequestContext requestContext) {
			if(requestContext == null) {
				throw new ArgumentNullException("requestContext");
			}
			requestContext.HttpContext.Items[typeof(RequestContext)] = requestContext;
			return this;
		}

		bool IHttpHandler.IsReusable {
			get { return true; }
		}
	}
}

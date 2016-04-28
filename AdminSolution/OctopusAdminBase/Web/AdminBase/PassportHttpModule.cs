using System;
using System.Web;
using System.Web.Security;
using HTB.DevFx;

namespace Octopus.Web.AdminBase
{
	internal class PassportHttpModule : IHttpModule
	{
		void IHttpModule.Init(HttpApplication context) {
			this.formsModule = new FormsAuthenticationModule();
			this.formsModule.Init(context);

			context.BeginRequest += this.OnAuthenticateRequest;
			context.PostAuthenticateRequest += this.OnPostAuthenticateRequest;
			context.EndRequest += this.OnEndRequest;
		}

		void IHttpModule.Dispose() {
			if(this.formsModule != null) {
				this.formsModule.Dispose();
			}
			this.formsModule = null;
		}

		private FormsAuthenticationModule formsModule;
		internal PassportAuthenticationInternal PassportAuthentication {
			get { return ObjectService.GetObject<PassportAuthenticationInternal>(); }
		}

		private void OnAuthenticateRequest(object sender, EventArgs e) {
			//把认证Cookie保存起来，Form认证不通过的话会删除的
			var ctx = ((HttpApplication)sender).Context;
			var cookie = ctx.Request.Cookies[FormsAuthentication.FormsCookieName];
			if(cookie != null) {
				ctx.Items[this.GetType()] = cookie;
			} else {
				//尝试从POST请求参数中获取相应的Cookie（解决某些不支持Cookie的浏览器）
				var value = ctx.Request.Form[FormsAuthentication.FormsCookieName];
				if(!string.IsNullOrEmpty(value)) {
					ctx.Items[this.GetType()] = new HttpCookie(FormsAuthentication.FormsCookieName, value);
				}
			}
		}

		private void OnPostAuthenticateRequest(object sender, EventArgs e) {
			//恢复认证Cookie
			var ctx = ((HttpApplication)sender).Context;
			var cookie = ctx.Request.Cookies[FormsAuthentication.FormsCookieName];
			if(cookie == null) {
				cookie = ctx.Items[this.GetType()] as HttpCookie;
				if(cookie != null) {
					ctx.Request.Cookies.Add(cookie);
				}
			}
			this.PassportAuthentication.OnPostAuthenticateRequest(sender, e);
		}

		private void OnEndRequest(object sender, EventArgs e) {
			//this.PassportAuthentication.OnEndRequest(sender, e);
		}
	}
}

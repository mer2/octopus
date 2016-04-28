using System;
using System.Security.Principal;
using System.Web;
using System.Web.Caching;
using HTB.DevFx;
using HTB.DevFx.Core;
using Octopus.Esb;
using Octopus.Web.AdminBase.Config;
using Octopus.Web.Config;

namespace Octopus.Web.AdminBase
{
	public class PassportAuthentication
	{
		public static string DefaultUrl {
			get { return ObjectService.GetObject<IPassportAuthentication>().Setting.DefaultUrl; }
		}

		public static string MainDomain {
			get { return ObjectService.GetObject<IPassportAuthentication>().Setting.MainDomain; }
		}
	}

	internal class PassportAuthenticationInternal : ServiceBase<PassportAuthenticationSetting>, IPassportAuthentication
	{
		internal IPassportService PassportService {
			get { return this.ObjectService.GetObject<IPassportService>(); }
		}
		internal Cache SessionCache {
			get { return HttpRuntime.Cache; }
		}

		internal void OnAuthenticateRequest(HttpContext ctx) {
			if(ctx.Request.IsAuthenticated) {
				return;
			}

			var identityString = this.GetTicketString(ctx);
			if(string.IsNullOrEmpty(identityString)) {
				return;
			}
			var identity = this.GetIdentityFromIdentityString(ctx, identityString);
			if(identity == null || identity.Expired) {//不存在或已过期
				if (this.Setting.RemoveInvalidTicket) {
					System.Web.Security.FormsAuthentication.SignOut();
				}
				return;
			}
			var now = DateTime.Now;
			var spanPast = now - identity.IssueDate;
			var spanLeft = identity.Expiration - now;
			if(spanLeft > spanPast) {//时间未过半，认证通过
				ctx.User = new GenericPrincipal(identity, null);
				return;
			}
			//时间过半，需要到服务端验证（更新票据）
			var result = this.PassportService.ValidateSession(this.Setting.ApplicationNo, identityString, null);
			if (result.ResultNo != 0) {//验证失败
				return;
			}
			identity = result.ResultAttachObjectEx;
			this.CacheIdentity(identity, identityString);//更新本地缓存
			ctx.User = new GenericPrincipal(identity, null);
		}

		internal string GetTicketString(HttpContext ctx) {
			var authCookie = ctx.Request.Cookies[this.Setting.CookieName];
			return authCookie == null ? null : authCookie.Value;
		}

		internal void OnPostAuthenticateRequest(object sender, EventArgs e) {
			var ctx = ((HttpApplication)sender).Context;
			this.OnAuthenticateRequest(ctx);
		}

		internal void CacheIdentity(PassportIdentity identity, string identityString) {
			if(identity == null) {
				return;
			}
			//计算Session时间
			var timeout = identity.Expiration - identity.IssueDate;
			//序列化
			var bytes = this.Serializer.Serialize(identity, null);
			var value = Convert.ToBase64String(bytes);
			this.SessionCache.Insert("SessionToken:" + identity.SessionToken, value, null, Cache.NoAbsoluteExpiration, timeout);
		}

		internal PassportIdentity GetIdentityFromIdentityString(HttpContext ctx, string identityString) {
			//尝试从本地缓存中获取序列化后的字符串
			var ids = this.SessionCache.Get("SessionToken:" + identityString) as string;
			PassportIdentity identity;
			if(string.IsNullOrEmpty(ids)) {
				//本地缓存没有，从服务器拿
				var result = this.PassportService.ValidateSession(this.Setting.ApplicationNo, identityString, null);
				if(result.ResultNo != 0) { //服务器验证失败
					return null;
				}
				identity = result.ResultAttachObjectEx;
				this.CacheIdentity(identity, identityString);
			} else {
				//反序列化
				identity = this.Serializer.Deserialize<PassportIdentity>(Convert.FromBase64String(ids), null);
			}
			return identity;
		}

		internal ISerializer Serializer {
			get { return SerializerFactory.GetSerializer(this.Setting.SerializerName); }
		}

		IPassportAuthenticationSetting IPassportAuthentication.Setting {
			get { return this.Setting; }
		}
	}
}

using System;
using System.Collections;
using System.Security.Principal;
using System.Web;
using HTB.DevFx.Cache;
using HTB.DevFx.Core;
using Joy.Security;
using Octopus.Admin.Crowd;
using Octopus.Admin.Server.Config;
using Octopus.Web.AdminBase;

namespace Octopus.Admin.Server
{
	internal class PassportService : ServiceBase<PassportServerServiceSetting>, IPassportService, IPassportServerService
	{
		protected ICache Cache { get; private set; }
		protected override void OnInit() {
			base.OnInit();
			this.Cache = CacheService.GetCache(this.Setting.CacheName);
		}

		public IAOPResult<PassportIdentity> ValidateSession(string appNo, string ticket, IDictionary options) {
			if (string.IsNullOrEmpty(ticket)) {
				return AOPResult.Failed<PassportIdentity>("ticket empty");
			}
			var identity = this.GetSessionFromCache(ticket);
			if (identity == null) {
				return AOPResult.Failed<PassportIdentity>("ticket not found");
			}
			if (identity.Expired) {
				this.RemoveSessionFromCache(ticket);
				return AOPResult.Failed<PassportIdentity>("ticket expired");
			}
			return AOPResult.Success(identity);
		}

		public IAOPResult Login(HttpContextBase ctx, string userName, string password, IDictionary options) {
			//去安全服务验证
			var securityResult = SecurityHelper.SecurityValidate("SecurityCaptcha");
			if(securityResult.ResultNo != 0) {//验证码错误
				return securityResult;
			}
			//去Crowd认证
			var svr = CrowdService.Current;
			var result = svr.Authenticate(userName, password);
			if(result.ResultNo != 0) {
				return result;
			}
			var principal = result.ResultAttachObjectEx;
			var user = principal;
			var userResult = svr.GetUser(userName);
			if (userResult.IsSuccess() && userResult.ResultAttachObjectEx != null) {
				user = userResult.ResultAttachObjectEx;
			}
			var now = DateTime.Now;
			var identity = new PassportIdentity {
				SessionToken = Guid.NewGuid().ToString("N"),
				Name = user.Name, DisplayName = user.DisplayName,
				IssueDate = now, Expiration = now + this.Setting.CookieTimeout
			};
			//缓存起来
			this.SaveSessionToCache(identity);
			var cookie = new HttpCookie(this.Setting.CookieName, identity.SessionToken) {
				HttpOnly = true,
				Path = this.Setting.CookiePath,
				Domain = this.Setting.CookieDomain
			};
			ctx.Response.AppendCookie(cookie);
			ctx.User = new GenericPrincipal(identity, null);
			return AOPResult.Success();
		}

		protected void SaveSessionToCache(PassportIdentity identity) {
			var key = "SessionToken:" + identity.SessionToken;
			this.Cache[key, CacheDependency.Create(identity.Expiration)] = identity;
		}

		protected PassportIdentity GetSessionFromCache(string sessionToken) {
			var key = "SessionToken:" + sessionToken;
			return this.Cache[key] as PassportIdentity;
		}

		protected void RemoveSessionFromCache(string sessionToken) {
			var key = "SessionToken:" + sessionToken;
			this.Cache.Remove(key);
		}
	}
}
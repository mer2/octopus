using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using HTB.DevFx.Cache;
using HTB.DevFx.Core;
using HTB.DevFx.Exceptions;
using HTB.DevFx.Log;
using Newtonsoft.Json;
using Octopus.Admin.Crowd.Config;

namespace Octopus.Admin.Crowd
{
	public class CrowdService : ServiceBase<CrowdServiceSetting>
	{
		public static CrowdService Current {
			get { return HTB.DevFx.ObjectService.GetObject<CrowdService>(); }
		}

		public static bool UserInGroups(string userName, string[] groups) {
			if(groups == null || groups.Length <= 0) {
				return true;
			}
			var groupsResult = Current.GetUserGroups(userName);
			if(groupsResult.ResultNo != 0) {
				return false;
			}
			var allowGroups = groupsResult.ResultAttachObjectEx;
			if(!allowGroups.Intersect(groups, StringComparer.InvariantCultureIgnoreCase).Any()) {
				return false;
			}
			return true;
		}

		protected CrowdService() { }
		protected override void OnInit() {
			base.OnInit();
			this.cache = CacheService.GetCache(this.Setting.CacheName);
		}
		private ICache cache;
		private readonly object authenticateLockObject = new object();
		private readonly object validateLockObject = new object();
		private readonly object getUserGroupsLockObject = new object();
		private readonly CookieContainer cookieContainer = new CookieContainer();

		public virtual IAOPResult<CrowdUser> Authenticate(string userName, string password) {
			var ui = userName.IndexOf('@');//邮箱地址取@前面的
			if(ui > 0) {
				userName = userName.Substring(0, ui);
			}
			var key = "Authenticate:" + userName + "@" + password;
			return this.GetFromCache(key, this.authenticateLockObject, (out string url) => {
				url = string.Format("{0}/authentication?username={1}", this.Setting.CrowdServiceBaseUrl, userName);
				return this.GetResponse<CrowdUser>("POST", url, new { value = password });
			});
		}

		public virtual IAOPResult<CrowdPrincipal> Validate(string token) {
			var key = "Validate:" + token;
			return this.GetFromCache(key, this.validateLockObject, (out string url) => {
				url = string.Format("{0}/session/{1}", this.Setting.CrowdServiceBaseUrl, token);
				return this.GetResponse<CrowdPrincipal>("GET", url, null);
			});
		}

		public virtual IAOPResult ChangePassword(string userName, string password) {
			var url = string.Format("{0}/user/password?username={1}", this.Setting.CrowdServiceBaseUrl, userName);
			try {
				this.GetWebResponse("PUT", url, new { value = password });
			} catch(Exception e) {
				return this.ErrorHandle<object>(url, e);
			}
			return AOPResult.Success();
		}

		public virtual IAOPResult<string[]> GetUserGroups(string userName) {
			var key = "GetUserGroups:" + userName;
			return this.GetFromCache(key, this.getUserGroupsLockObject, (out string url) => {
				url = string.Format("{0}/user/group/direct?username={1}", this.Setting.CrowdServiceBaseUrl, userName);
				var result = this.GetResponse<CrowdGroupResult>("GET", url, null);
				var groups = new List<string>();
				if(result.Groups != null) {
					groups.AddRange(result.Groups.Select(g => g.Name));
				}
				return groups.ToArray();
			});
		}

		public virtual IAOPResult<CrowdPrincipal> Login(string userName, string password) {
			var url = string.Format("{0}/session", this.Setting.CrowdServiceBaseUrl);
			try {
				var result = this.GetResponse<CrowdPrincipal>("POST", url, new { username = userName, password/*, validationFactors = new[] {new {name = "remote_address", value = "127.0.0.1"}}*/});
				return AOPResult.Success(result);
			} catch(Exception e) {
				return this.ErrorHandle<CrowdPrincipal>(url, e);
			}
		}

		public virtual IAOPResult Logout(string token) {
			var url = string.Format("{0}/session/{1}", this.Setting.CrowdServiceBaseUrl, token);
			try {
				this.GetWebResponse("DELETE", url, null);
				return AOPResult.Success();
			} catch(Exception e) {
				return this.ErrorHandle<object>(url, e);
			}
		}

		public virtual IAOPResult<string[]> GetGroupUsers(string groupName) {
			var url = string.Format("{0}/group/user/direct?groupname={1}", this.Setting.CrowdServiceBaseUrl, groupName);
			try {
				var result = this.GetResponse<CrowdGroupUsers>("GET", url, null);
				return AOPResult.Success(result.users.Select(x => x.name).ToArray());
			} catch(Exception e) {
				return this.ErrorHandle<string[]>(url, e);
			}
		}

		public virtual IAOPResult<IDictionary<string, string>> GetUserAttributes(string userName) {
			var url = string.Format("{0}/user/attribute?username={1}", this.Setting.CrowdServiceBaseUrl, userName);
			try {
				var result = this.GetResponse<CrowdUserAttributes>("GET", url, null);
				return AOPResult.Success(result.attributes.ToDictionary(k => k.name, v => v.values[0]));
			} catch(Exception e) {
				return this.ErrorHandle<IDictionary<string, string>>(url, e);
			}
		}

		public virtual IAOPResult<CrowdUser> GetUser(string userName) {
			var url = string.Format("{0}/user?username={1}", this.Setting.CrowdServiceBaseUrl, userName);
			try {
				var result = this.GetResponse<CrowdUser>("GET", url, null);
				return AOPResult.Success(result);
			} catch(Exception e) {
				return this.ErrorHandle<CrowdUser>(url, e);
			}
		}

		protected delegate T GettingHandler<out T>(out string url);
		protected IAOPResult<T> GetFromCache<T>(string key, object lockObject, GettingHandler<T> missingHandler) {
			var timeout = this.Setting.CacheTimeout;
			if(timeout < 0) {//不缓存
				string url = null;
				try {
					var item = missingHandler(out url);
					return AOPResult.Success(item);
				} catch(Exception e) {
					return this.ErrorHandle<T>(url, e);
				}
			}
			if(!this.cache.Contains(key)) {
				lock(lockObject) {
					if(!this.cache.Contains(key)) {
						string url = null;
						try {
							var item = missingHandler(out url);
							if(timeout > 0) {
								this.cache[key, CacheDependency.Create(TimeSpan.FromSeconds(timeout))] = item;
							} else {
								this.cache[key] = item;
							}
						} catch(Exception e) {
							return this.ErrorHandle<T>(url, e);
						}
					}
				}
			}
			return AOPResult.Success((T)this.cache[key]);
		}

		protected HttpWebResponse GetWebResponse(string method, string url, object data) {
			var request = (HttpWebRequest)WebRequest.Create(url);
			request.CookieContainer = this.cookieContainer;
			request.ContentType = request.Accept = "application/json";
			request.Credentials = new NetworkCredential(this.Setting.CrowdAppNo, this.Setting.CrowdAppSecretKey);
			request.Method = method;
			if(data != null) {
				var dataString = JsonConvert.SerializeObject(data);
				var encoding = Encoding.UTF8;
				var length = encoding.GetByteCount(dataString);
				request.ContentLength = length;
				using(var sw = request.GetRequestStream()) {
					sw.Write(encoding.GetBytes(dataString), 0, length);
				}
			}
			return (HttpWebResponse)request.GetResponse();
		}

		protected string GetResponse(string method, string url, object data) {
			var response = this.GetWebResponse(method, url, data);
			var sr = new StreamReader(response.GetResponseStream(), true);
			return sr.ReadToEnd();
		}

		protected T GetResponse<T>(string method, string url, object data) {
			var response = this.GetResponse(method, url, data);
			return JsonConvert.DeserializeObject<T>(response);
		}

		protected IAOPResult<T> ErrorHandle<T>(string url, Exception e) {
			var we = e as WebException;
			CrowdFailedMessage failedMessage = null;
			if(we != null && we.Response != null) {
				var stream = we.Response.GetResponseStream();
				if(stream != null) {
					var sr = new StreamReader(we.Response.GetResponseStream());
					var errorResult = sr.ReadToEnd();
					LogService.WriteLog(this, LogLevel.ERROR, "{0}\r\n{1}", url, errorResult);
					try {
						failedMessage = JsonConvert.DeserializeObject<CrowdFailedMessage>(errorResult);
					} catch(Exception ex) {
						ExceptionService.Publish(ex);
					}
				}
			}
			ExceptionService.Publish(e);
			var result = new AOPResult<T>(-1, e.Message, default(T), null);
			result.ResultData[typeof(CrowdFailedMessage)] = failedMessage;
			return result;
		}
	}

	public class CrowdUser : IIdentity
	{
		string IIdentity.AuthenticationType {
			get { return "Crowd"; }
		}

		bool IIdentity.IsAuthenticated {
			get { return !string.IsNullOrEmpty(this.Name); }
		}

		public string Name { get; set; }
		[JsonProperty("first-name")]
		public string FirstName { get; set; }
		[JsonProperty("last-name")]
		public string LastName { get; set; }
		[JsonProperty("display-name")]
		public string DisplayName { get; set; }
		public string Email { get; set; }
		public bool Active { get; set; }
	}

	public class CrowdFailedMessage
	{
		[JsonProperty("status-code")]
		public string StatusCode { get; set; }
		public string Reason { get; set; }
		public string Message { get; set; }
	}

	public class CrowdPrincipal : IPrincipal
	{
		public string Token { get; set; }
		public CrowdUser User { get; set; }
		public string[] Roles { get; set; }

		IIdentity IPrincipal.Identity { get { return this.User; } }
		bool IPrincipal.IsInRole(string role) {
			if(role == null || this.Roles == null || this.User == null) {
				return false;
			}

			for(var i = 0; i < this.Roles.Length; ++i) {
				if(this.Roles[i] != null && string.Compare(this.Roles[i], role, StringComparison.OrdinalIgnoreCase) == 0)
					return true;
			}
			return false;
		}
	}

	internal class CrowdGroupResult
	{
		public CrowdGroup[] Groups;
	}

	internal class CrowdGroup
	{
		public string Name { get; set; }
	}

	internal class CrowdGroupUser
	{
		public string name { get; set; }
	}

	internal class CrowdGroupUsers
	{
		public CrowdGroupUser[] users { get; set; }
	}

	internal class CrowdUserAttribute
	{
		public string name { get; set; }
		public string[] values { get; set; }
	}

	internal class CrowdUserAttributes
	{
		public CrowdUserAttribute[] attributes { get; set; }
	}
}

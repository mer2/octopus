using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using HTB.DevFx;
using HTB.DevFx.Data;
using HTB.DevFx.Log;
using HTB.DevFx.Utils;

namespace Octopus.Utils
{
	public static class WebHelper
	{
		public static bool TryParseVideo(string url, out VideoItem video) {
			var helper = ObjectService.GetObject<VideoHelper>();
			video = null;
			if(helper == null) {
				return false;
			}
			return helper.TryGetVideoItem(url, out video);
		}

		/// <summary>
		/// 获取基地址，包含http://
		/// </summary>
		public static string GetBaseUrl(HttpContext ctx = null, string baseUrl = null) {
			if(ctx == null) {
				ctx = HttpContext.Current;
			}
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

		public static string GetReturnUrl(HttpContext ctx = null, string returnUrl = null, string domain = null, UriKind uriKind = UriKind.RelativeOrAbsolute) {
			if(ctx == null) {
				ctx = HttpContext.Current;
			}
			if(string.IsNullOrEmpty(returnUrl)) {
				returnUrl = ctx.Request.QueryString["ReturnUrl"];
			}
			if(string.IsNullOrEmpty(domain)) {
				domain = FormsAuthentication.CookieDomain;
			}
			var makeRelative = true;
			if(!string.IsNullOrEmpty(returnUrl)) {
				Uri returnUri;
				if(Uri.TryCreate(returnUrl, UriKind.RelativeOrAbsolute, out returnUri)) {
					if(returnUri.IsAbsoluteUri) {
						var isHostDomain = string.Compare(returnUri.Host, domain, StringComparison.OrdinalIgnoreCase) == 0;
						if(!isHostDomain && !returnUri.Host.EndsWith("." + domain, true, null)) {
							returnUrl = "";
						} else {
							makeRelative = false;
						}
					}
				}
			}
			if(makeRelative) {
				returnUrl = HTB.DevFx.Utils.WebHelper.MakeUrlRelative(returnUrl, ctx.Request.ApplicationPath);
				if(uriKind == UriKind.Absolute) {
					returnUrl = GetBaseUrl(ctx) + HTB.DevFx.Utils.WebHelper.UrlCombine(ctx.Request.ApplicationPath, returnUrl, true);
				}
			}
			return returnUrl;
		}

		public static string ObjectToUrl(object urlObject, Encoding encoding = null, string[] excludeFields = null) {
			if (encoding == null) {
				encoding = Encoding.UTF8;
			}
			var dict = GetSignObject(urlObject, excludeFields);
			var sb = new StringBuilder();
			foreach (var key in dict.Keys) {
				var value = dict[key];
				if (value == null) {
					continue;
				}
				sb.AppendFormat("{0}=", HttpUtility.UrlEncode(key, encoding));
				if(value is DateTime) {
					sb.AppendFormat("{0:yyyyMMddHHmmss}", value);
				} else if(value is decimal || value is double || value is float) {
					sb.AppendFormat("{0:0.00}", value);
				} else {
					var vt = value.GetType();
					if (vt.IsPrimitive || value is string) {
						sb.Append(HttpUtility.UrlEncode(value.ToString()));
					}
				}
				sb.Append("&");
			}
			if (sb.Length > 0) {
				sb.Remove(sb.Length - 1, 1);
			}
			return sb.ToString();
		}

		internal static StringBuilder BuildSignString(object tosignObject, StringBuilder sb = null, int deepIndex = 0, int index = -1, string[] excludeFields = null) {
			if (sb == null) {
				sb = new StringBuilder();
			}
			if (tosignObject == null) {
				return sb;
			}
			var parameters = GetSignObject(tosignObject, excludeFields);
			if (parameters == null || parameters.Count <= 0) {
				return sb;
			}
			var keys = parameters.Keys.OrderBy(x => x.ToLowerInvariant());
			foreach(var key in keys) {
				var value = parameters[key];
				var theKey = key.ToLowerInvariant();
				if(index >= 0) {
					theKey += ":" + index;
				}
				if(value != null) {
					var vt = value.GetType();
					if(value is DateTime) {
						sb.AppendFormat("{0}={1:yyyyMMddHHmmss}&", theKey, value);
					} else if(value is decimal || value is double || value is float) {
						sb.AppendFormat("{0}={1:0.00}&", theKey, value);
					} else if(value is Array) {//数组
						var et = vt.GetElementType();
						if (et == null) {
							continue;
						}
						var arr = value as Array;
						if (arr.Length <= 0) {
							continue;
						}
						if (et.IsPrimitive || et == typeof (string)) { //基本类型以,分隔
							sb.AppendFormat("{0}=", theKey);
							foreach (var v in arr) {
								sb.AppendFormat("{0},", v);
							}
							sb.Remove(sb.Length - 1, 1).Append("&");
						} else {//POCO类型
							if (deepIndex > 0) {//只允许第一层
								continue;
							}
							sb.AppendFormat("{0}=", theKey);
							for (var i = 0; i < arr.Length; i++) {
								var v = arr.GetValue(i);
								BuildSignString(v, sb, deepIndex + 1, i, excludeFields);
							}
						}
					} else if (value is string) {
						sb.AppendFormat("{0}={1}&", theKey, value);
					} else {
						if (vt.IsPrimitive) {
							sb.AppendFormat("{0}={1}&", theKey, value);
						}
					}
				}
			}
			return sb;
		}

		public static StringBuilder BuildSignString(object tosignObject, StringBuilder sb = null, string[] excludeFields = null) {
			return BuildSignString(tosignObject, sb, 0, -1, excludeFields);
		}

		public static IDictionary<string, object> GetSignObject(object parameters, string[] excludeFields = null) {
			var p = parameters.ToDictionary() as IDictionary<string, object>;
			if(p != null && excludeFields != null && excludeFields.Length > 0) {
				foreach(var f in excludeFields) {
					p.Remove(f);
				}
			}
			return p;
		}

		public static string SignObject(object parameters, string secretKey, string secretKeyName = "signkey", string encryptFormat = "md5", string[] excludeFields = null) {
			var sb = BuildSignString(parameters, null, excludeFields);
			sb.AppendFormat("{0}={1}", secretKeyName.ToLowerInvariant(), secretKey);
			var toSign = sb.ToString();
			var signed = Encryption.Encrypt(toSign, encryptFormat);
#if DEBUG
			LogService.WriteLog(string.Format("Sign:{0} = {1}", toSign, signed));
#endif
			return signed;
		}

		public static bool ValidateSign(object parameters, string secretKey, string signString, string secretKeyName = "signkey", string encryptFormat = "md5", string[] excludeFields = null) {
			var sign = SignObject(parameters, secretKey, secretKeyName, encryptFormat, excludeFields);
			return Compare(sign, signString);
		}

		internal static bool Compare(string sign, string signString) {
#if DEBUG
			/** 用于性能测试 **/
			var debugSignString = ConfigurationManager.AppSettings["Octopus.Utils.WebHelper:DebugSignString"];
			if(!string.IsNullOrEmpty(debugSignString)) {
				return string.Compare(debugSignString, signString, StringComparison.InvariantCultureIgnoreCase) == 0;
			}
			/*****************/
#endif
			return string.Compare(sign, signString, StringComparison.InvariantCultureIgnoreCase) == 0;
		}
	}
}

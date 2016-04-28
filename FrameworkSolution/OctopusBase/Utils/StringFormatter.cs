using System;
using System.Collections;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using HTB.DevFx.Data;

namespace Octopus.Utils
{
	public static class StringFormatter
	{
		private static readonly char[] Separator = { ':' };
		private static readonly Regex FindParameters = new Regex(
			"\\{(?<param>.*?)\\}",
			RegexOptions.Compiled | RegexOptions.Singleline);

		public static string NamedStringFormat(this string format, object args) {
			return format.NamedStringFormat(args.ToDictionary());
		}

		public static string NamedStringFormat(this string format, IDictionary args, Func<string[], object, string> matchedHandle = null) {
			return FindParameters.Replace(format, delegate(Match match) {
				var param0 = match.Groups["param"].Value;
				var param = param0.Split(Separator);
				var value = args.Contains(param[0]) ? args[param[0]] : null;
				if (matchedHandle != null) {
					return matchedHandle(param, value);
				}
				if(param.Length <= 1 || string.IsNullOrEmpty(param[1])) {//只有一个参数或第二个参数为空，直接返回
					return Convert.ToString(value);
				}
				//Url编码
				if (string.Compare(param[1], "urlencode", StringComparison.InvariantCultureIgnoreCase) == 0) {
					var str = Convert.ToString(value);
					if (string.IsNullOrEmpty(str)) {
						return str;
					}
					string encodingName = null;
					if (param.Length > 2) {
						encodingName = param[2];
					}
					if (string.IsNullOrEmpty(encodingName)) {
						encodingName = "utf-8";
					}
					var encoding = Encoding.GetEncoding(encodingName);
					return HttpUtility.UrlEncode(str, encoding);
				}
				//有多个参数，把第一个参数换为0
				param[0] = "0";
				return string.Format(CultureInfo.CurrentCulture, "{" + string.Join(":", param) + "}", value);
			});
		}
	}
}
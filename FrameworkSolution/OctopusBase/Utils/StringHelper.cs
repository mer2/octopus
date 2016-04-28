using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HTB.DevFx;
using Microsoft.VisualBasic;

namespace Octopus.Utils
{
	public static class StringHelper
	{
		public sealed class Keywords
		{
			public IList<string> Words { get; set; }
			public int MaxLength { get; set; }
		}

		public const string SpaceString = " ~!@#$%^&*()_+`1234567890-={}|[]\\:\";'<>?,./\r\n";

		public static IDictionary<char, Keywords> GetKeywords(string keys, char spliter = '|') {
			return string.IsNullOrEmpty(keys) ? new Dictionary<char, Keywords>() : GetKeywords(keys.Split(spliter));
		}

		public static IDictionary<char, Keywords> GetKeywords(string[] list) {
			var dict = new Dictionary<char, Keywords>();
			if(list == null || list.Length <= 0) {
				return dict;
			}
			foreach(var str0 in list) {
				if(string.IsNullOrEmpty(str0)) {
					continue;
				}
				var str = str0.ToLowerInvariant();//转换成小写
				var ch = str[0];
				Keywords kw;
				if(!dict.TryGetValue(ch, out kw)) {
					kw = new Keywords { Words = new List<string>() };
					dict.Add(ch, kw);
				}
				kw.Words.Add(str);
				if(kw.MaxLength < str.Length) {
					kw.MaxLength = str.Length;
				}
			}
			foreach(var v in dict.Values) {
				v.Words = v.Words.OrderByDescending(x => x).ToList();
			}
			return dict;
		}

		public static bool KeywordsContains(string content, IDictionary<char, Keywords> keys) {
			var exists = false;
			FilterKeywords(content, keys, (sb, stt, w) => { exists = true; return false; });
			return exists;
		}

		public static string FilterKeywords(string content, IDictionary<char, Keywords> keys, System.Func<StringBuilder, string, string, bool> filter = null, string space = SpaceString) {
			if(string.IsNullOrEmpty(content)) {
				return content;
			}
			var ct = Strings.StrConv(content, VbStrConv.SimplifiedChinese);//转为简体
			ct = Strings.StrConv(ct, VbStrConv.Narrow);//全角转为半角
			ct = ct.ToLowerInvariant();//转换成小写
			var length = content.Length;
			var sb = new StringBuilder(length);
			var st = new StringBuilder(20);
			for(var i = 0; i < length; i++) {
				var ch0 = content[i];
				var ch = ct[i];
				if(!keys.ContainsKey(ch)) {
					sb.Append(ch0);
					continue;
				}
				var kw = keys[ch];
				var count = kw.MaxLength;
				//获取count个数字，去除空格符号等
				var p = i;
				st.Length = 0;
				while(count > 0 && p < length) {
					var c = ct[p];
					if(space.IndexOf(c) < 0) {//不是空格等
						st.Append(c);
						count--;
					}//跳过
					p++;
				}
				var found = false;
				if(st.Length > 0) {
					var stt = st.ToString();
					foreach(var w in kw.Words) {
						if(stt.StartsWith(w, StringComparison.InvariantCultureIgnoreCase)) {//含有屏蔽字
							if(filter != null) {
								if(!filter(sb, stt, w)) {//强制退出
									return null;
								}
							} else {
								sb.Append("**");
							}
							i = p - 1 + w.Length - st.Length;
							found = true;
							break;
						}
					}
				}
				if(!found) {
					sb.Append(ch0);
				}
			}
			return sb.ToString();
		}

		private static readonly string[,] ubbRegexes = {
			{@"\[p\]([^\[]*?)\[\/p\]", "$1<br />"},
			{@"\[b\]([^\[]*?)\[\/b\]", "<b>$1</b>"},
			{@"\[i\]([^\[]*?)\[\/i\]", "<i>$1</i>"},
			{@"\[u\]([^\[]*?)\[\/u\]", "<u>$1</u>"},
			{@"\[ol\]([^\[]*?)\[\/ol\]", "<ol>$1</ol>"},
			{@"\[ul\]([^\[]*?)\[\/ul\]", "<ul>$1</ul>"},
			{@"\[li\]([^\[]*?)\[\/li\]", "<li>$1</li>"},
			{@"\[code\]([^\[]*?)\[\/code\]", "<div class=\"ubb_code\">$1</div>"},
			{@"\[quote\]([^\[]*?)\[\/quote\]", "<div class=\"ubb_quote\">$1</div>"},
			{@"\[color=([^\]]*)\]([^\[]*?)\[\/color\]", "<font style=\"color: $1\">$2</font>"},
			{@"\[hilitecolor=([^\]]*)\]([^\[]*?)\[\/hilitecolor\]", "<font style=\"background-color: $1\">$2</font>"},
			{@"\[align=([^\]]*)\]([^\[]*?)\[\/align\]", "<div style=\"text-align: $1\">$2</div>"},
			{@"\[url=([^\]]*)\]([^\[]*?)\[\/url\]", "<a href=\"$1\">$2</a>"},
			{@"\[img\]([^\[]*?)\[\/img\]", "<img src=\"$1\" />"}
		};

		public static string UBB2Html(string text) {
			if(string.IsNullOrEmpty(text)) {
				return text;
			}
			var sb = new StringBuilder(text);
			sb = sb.Replace("&", "&amp;");
			sb = sb.Replace(">", "&gt;");
			sb = sb.Replace("<", "&lt;");
			sb = sb.Replace("\"", "&quot;");
			sb = sb.Replace("&amp;#91;", "&#91;");
			sb = sb.Replace("&amp;#93;", "&#93;");
			sb = sb.Replace("\r", "");
			sb = sb.Replace("\n", "<br />");
			text = Regex.Replace(sb.ToString(), @"\[br\]", "<br />", RegexOptions.IgnoreCase);
			var found = true;
			while(found) {
				found = false;
				for(var ti = 0; ti < ubbRegexes.GetLength(0); ti++) {
					var regex = new Regex(ubbRegexes[ti, 0], RegexOptions.IgnoreCase);
					if(regex.Match(text).Success) {
						found = true;
						text = Regex.Replace(text, ubbRegexes[ti, 0], ubbRegexes[ti, 1], RegexOptions.IgnoreCase);
					}
				}
			}
			return text;
		}

		public static string SafeHtml(string html, bool allowMedia = false) {
			if(string.IsNullOrEmpty(html)) {
				return html;
			}
			var helper = ObjectService.GetObject<HtmlFilter>();
			return helper.Filter(html, allowMedia);
		}

		public static string Html2Text(string html, string replace = " ") {
			if(string.IsNullOrEmpty(html)) {
				return html;
			}
			html = Regex.Replace(html, @"<(.[^>]*)>", replace, RegexOptions.IgnoreCase);
			return string.IsNullOrEmpty(replace) ? html : html.Trim(replace.ToCharArray());
		}

		/// <summary>
		/// 获取子字符串（去除HTML标签）
		/// </summary>
		/// <param name="html">原始字符串</param>
		/// <param name="length">需被截取的长度（汉字等宽字体长度算2，如果<paramref name="widechar"/>为true的话）</param>
		/// <param name="startIndex">起始位置</param>
		/// <param name="widechar">是否把汉字等宽字体长度算成2</param>
		/// <param name="ellipsis">如果文字太多，截取后添加此字符串</param>
		/// <param name="replace">HTML标签被替换后的字符串</param>
		/// <returns>字符串</returns>
		public static string HtmlSubstring(this string html, int length, int startIndex = 0, bool widechar = true, string ellipsis = " ...", string replace = " ") {
			if(string.IsNullOrEmpty(html)) {
				return html;
			}
			html = Html2Text(html, replace);
			if (string.IsNullOrEmpty(html)) {
				return html;
			}
			if (startIndex < 0) {
				startIndex = 0;
			}
			int i, count = 0;
			var sb = new StringBuilder();
			for (i = startIndex; i < html.Length; i++) {
				var ch = html[i];
				sb.Append(ch);
				if (widechar) {
					if (ch < '\u00ff') { //ASCII码？算一个位置
						count++;
					} else {
						count += 2;
					}
				} else {
					count++;
				}
				if (count >= length) {
					break;
				}
			}
			if (i > startIndex && i < html.Length) {
				sb.Append(ellipsis);
			}
			return sb.ToString();
		}
	}
}
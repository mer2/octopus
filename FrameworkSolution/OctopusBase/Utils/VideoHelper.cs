using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using HTB.DevFx.Core;
using Octopus.Utils.Config;

namespace Octopus.Utils
{
	[Serializable]
	public class VideoItem
	{
		public string Url { get; set; }
		public string Host { get; set; }
		public string VideoID { get; set; }
		public string IconUrl { get; set; }
		public string VideoUrl { get; set; }
		public string UrlHtml { get; set; }
	}
	
	internal class VideoHelper : ServiceBase<VideoHelperSetting>
	{
		protected VideoSiteSetting[] SiteSettings { get; set; }  
		protected override void OnInit() {
			base.OnInit();
			var sites = this.Setting.VideoSites;
			if(sites != null && sites.Length > 0) {
				foreach (var vss in sites) {
					vss.RegexID = new Regex(vss.IDRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase);
					if(vss.DataFormat == "regex") {
						vss.RegexIcon = new Regex(vss.IconRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase);
					}
				}
			}
			this.SiteSettings = sites;
		}

		internal bool TryGetVideoItem(string url, out VideoItem video) {
			video = new VideoItem { Url = url };
			if(string.IsNullOrEmpty(url)) {
				return false;
			}
			Uri uri;
			if(!Uri.TryCreate(url, UriKind.Absolute, out uri)) {
				return false;
			}
			var host  = uri.Host;
			VideoSiteSetting site = null;
			foreach (var st in this.SiteSettings) {
				if(string.Compare(st.Host, host, StringComparison.InvariantCultureIgnoreCase) == 0) {
					site = st;
					break;
				}
				if(host.EndsWith("." + st.Host, StringComparison.InvariantCultureIgnoreCase)) {
					site = st;
					break;
				}
			}
			if(site == null) {
				return false;
			}
			video.Host = site.Host;
			return TryGetVideoItem(url, site, video);
		}

		internal static bool TryGetVideoItem(string url, VideoSiteSetting site, VideoItem video) {
			var matches = site.RegexID.Matches(url);
			if(matches.Count <= 0) {
				return false;
			}
			var id = video.VideoID = matches[matches.Count - 1].Groups[1].Value;
			video.VideoUrl = string.Format(site.VideoUrl, id);
			var contentUrl = string.IsNullOrEmpty(site.ContentUrl) ? url : string.Format(site.ContentUrl, id);
			var html = video.UrlHtml = GetContent(contentUrl);
			if(string.IsNullOrEmpty(html)) {
				return false;
			}
			if(site.DataFormat == "json") {
				var js = new JavaScriptSerializer();
				var data = js.DeserializeObject(html) as IDictionary<string, object>;
				if(data != null) {
					var names = site.IconRegex.Split('/');
					object value = data;
					foreach (var name in names) {
						if(value is IDictionary<string, object>) {
							var dict = value as IDictionary<string, object>;
							if(dict.ContainsKey(name)) {
								value = dict[name];
							}
						} else if(value is Array) {
							var array = value as Array;
							int index;
							if(!int.TryParse(name, out index)) {
								break;
							}
							value = array.GetValue(index);
						} else {
							break;
						}
					}
					if(value != null) {
						video.IconUrl = Convert.ToString(value);
					}
				}
			} else {
				var match = site.RegexIcon.Match(html);
				if(match.Success) {
					video.IconUrl = match.Groups[1].Value;
				}
			}
			return true;
		}

		internal static string GetContent(string url) {
			var myReq = (HttpWebRequest)WebRequest.Create(url);
			myReq.UserAgent = "User-Agent:Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705";
			myReq.Accept = "*/*";
			myReq.KeepAlive = true;
			myReq.Headers.Add("Accept-Language", "zh-cn,en-us;q=0.5");
			var result = (HttpWebResponse)myReq.GetResponse();
			using(var receviceStream = result.GetResponseStream()) {
				using(var readerOfStream = new StreamReader(receviceStream, Encoding.UTF8)) {
					return readerOfStream.ReadToEnd();
				}
			}
		}
	}
}

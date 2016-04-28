using System.Text.RegularExpressions;
using HTB.DevFx.Config;
[assembly: ConfigResource("res://Octopus.Utils.Config.Settings.config")]

namespace Octopus.Utils.Config
{
	internal class VideoHelperSetting : ConfigSettingElement
	{
		protected override void OnConfigSettingChanged() {
			base.OnConfigSettingChanged();
			this.VideoSites = this.GetSettings<VideoSiteSetting>("sites", null).ToArray();
		}

		public VideoSiteSetting[] VideoSites { get; private set; }
	}

	internal class VideoSiteSetting : ConfigSettingElement
	{
		protected override void OnConfigSettingChanged() {
			base.OnConfigSettingChanged();
			this.Host = this.GetRequiredSetting("name");
			this.IDRegex = this.GetRequiredSetting("regId");
			this.ContentUrl = this.GetSetting("contentUrl");
			this.DataFormat = this.GetSetting("dataFormat", "json");
			this.IconRegex = this.GetRequiredSetting("regIcon");
			this.VideoUrl = this.GetRequiredSetting("videoUrl");
		}

		public string Host { get; private set; }
		public string IDRegex { get; private set; }
		public string ContentUrl { get; private set; }
		public string DataFormat { get; private set; }
		public string IconRegex { get; private set; }
		public string VideoUrl { get; private set; }

		internal Regex RegexID { get; set; }
		internal Regex RegexIcon { get; set; }
	}

	internal class HtmlFilterSetting : ConfigSettingElement
	{
		protected override void OnConfigSettingChanged() {
			base.OnConfigSettingChanged();
			this.TagWhiteString = this.GetRequiredSetting("tags");
			this.ProtocolWhiteString = this.GetRequiredSetting("protocols");
			this.UrlAttributeWhiteString = this.GetRequiredSetting("linkTags");
			this.CssBlockString = this.GetRequiredSetting("blockCss");
			this.AttributeBlockString = this.GetRequiredSetting("blockAttributes");
			this.AllowMedia = this.GetSetting("allowMedia", false);
		}

		public string TagWhiteString { get; private set; }//HTML标签白名单
		public string ProtocolWhiteString { get; private set; }//URL前缀
		public string UrlAttributeWhiteString { get; private set; }//可以使用URL的属性
		public string CssBlockString { get; private set; }//CSS不被允许的属性
		public string AttributeBlockString { get; private set; }//不被允许的属性

		public bool AllowMedia { get; private set; }//是否允许嵌入媒体播放器
	}
}

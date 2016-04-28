using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HTB.DevFx.Core;
using HtmlAgilityPack;
using Octopus.Utils.Config;

namespace Octopus.Utils
{
	internal class HtmlFilter : ServiceBase<HtmlFilterSetting>
	{
		protected string[] TagWhiteList;
		protected string[] AttributeBlockList;
		protected string[] CssBlockList;
		protected string[] UrlAttributeWhiteList;
		protected string[] ProtocolWhiteList;

		protected override void OnInit() {
			base.OnInit();
			this.TagWhiteList = this.Setting.TagWhiteString.Split(',');
			this.AttributeBlockList = this.Setting.AttributeBlockString.Split(',');
			this.CssBlockList = this.Setting.CssBlockString.Split(',');
			this.UrlAttributeWhiteList = this.Setting.UrlAttributeWhiteString.Split(',');
			this.ProtocolWhiteList = this.Setting.ProtocolWhiteString.Split(',');
		}

		public string Filter(string html) {
			return this.Filter(html, this.Setting.AllowMedia);
		}

		public string Filter(string html, bool allowMedia) {
			var hd = new HtmlDocument {
				OptionAutoCloseOnEnd = true,
				OptionFixNestedTags = true,
				OptionOutputOptimizeAttributeValues = true,
				OptionOutputOriginalCase = false,
				OptionWriteEmptyNodes = true,
				OptionDefaultStreamEncoding = Encoding.UTF8
			};
			hd.LoadHtml(html);
			var bodyNode = hd.DocumentNode.SelectSingleNode("/html/body") ?? hd.DocumentNode.SelectSingleNode("/body");
			if(bodyNode != null) {
				hd.DocumentNode.RemoveAll();
				hd.DocumentNode.AppendChildren(bodyNode.ChildNodes);
			}
			this.FilterInternal(hd.DocumentNode.ChildNodes, allowMedia);
			using(var ms = new MemoryStream()) {
				hd.Save(ms);
				ms.Position = 0;
				using(var sr = new StreamReader(ms, Encoding.UTF8)) {
					var s = sr.ReadToEnd();
					return s;
				}
			}
		}

		protected void FilterInternal(IList<HtmlNode> nodes, bool allowMedia) {
			if(nodes == null || nodes.Count <= 0) {
				return;
			}
			for(var i = 0; i < nodes.Count; i++) {
				var node = nodes[i];
				var removeNode = false;
				switch(node.NodeType) {
					case HtmlNodeType.Comment: //删除无用的注释
						removeNode = true;
						break;
					case HtmlNodeType.Element:
						var nodeName = node.Name.ToLowerInvariant();
						if (nodeName == "embed") { //媒体
							removeNode = !allowMedia;
						} else if (!TagWhiteList.Contains(nodeName)) { //节点不在白名单内
							removeNode = true;
						}
						if (removeNode) {
							break;
						}
						if (node.HasAttributes) { //判断属性是否被允许
							for (var k = 0; k < node.Attributes.Count; k++) {
								var removeAttribute = false;
								var attr = node.Attributes[k];
								var attrName = attr.Name.ToLowerInvariant();
								var attrValue = attr.Value;
								if (attrName.StartsWith("on")) { //所有以on开头的事件全部移除
									removeAttribute = true;
								} else if (AttributeBlockList.Contains(attrName)) { //在黑名单里
									removeAttribute = true;
								} else if (attrName == "style") { //如果是样式表，且含有不被允许的单词，则移除
									if (!string.IsNullOrEmpty(attrValue)) {
										attrValue = attrValue.ToLowerInvariant();
										foreach (var css in CssBlockList) {
											if (attrValue.Contains(css)) {
												removeAttribute = true;
												break;
											}
										}
									}
								} else {
									//判断含有链接的属性里的链接是否安全
									if (UrlAttributeWhiteList.Contains(attrName)) {
										if (!string.IsNullOrEmpty(attrValue)) {
											attrValue = attrValue.ToLowerInvariant();
											if (!ProtocolWhiteList.Any(x => attrValue.StartsWith(x))) { //不在安全链接里，则删除
												removeAttribute = true;
											}
										}
									}
								}
								if (removeAttribute) {
									attr.Remove();
									k--;
								}
							}
						}
						//处理子节点
						FilterInternal(node.ChildNodes, allowMedia);
						break;
				}
				if (removeNode) {
					node.Remove();
					i--;
				}
			}
		}
	}
}
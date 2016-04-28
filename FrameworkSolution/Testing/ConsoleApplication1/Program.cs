using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using HtmlAgilityPack;
using Octopus.Utils;

namespace ConsoleApplication1
{
	class Program
	{
		private const string TagWhiteString = "a,abbr,acronym,address,area,b,bdo,big,blockquote,br,button,caption,center,cite,col,colgroup,dd,del,div,dl,dt,em,font,h1,h2,h3,h4,h5,h6,hr,i,img,ins,label,li,map,ol,p,param,pre,q,small,span,strong,sub,sup,table,tbody,td,tfoot,th,thead,tr,tt,u,ul";//HTML标签白名单
		private const string ProtocolWhiteString = "http://,https://,mailto:,/,./,../";//url前缀
		private const string UrlAttributeWhiteString = "background,codebase,href,lowsrc,src";//可以使用url的属性
		private const string CssBlockString = "absolute,behavior,behaviour,content,expression,fixed,include-source,moz-binding";//CSS不被允许的属性
		private const string AttributeBlockString = "dynsrc,id,name";//不被允许的属性
		private static string[] TagWhiteList;
		private static string[] AttributeBlockList;
		private static string[] CssBlockList;
		private static string[] UrlAttributeWhiteList;
		private static string[] ProtocolWhiteList;
		static void Main() {
			//TODO:embed
			var w = new HtmlWeb();
			var hw = w.Load("http://infos.joyyang.com/News/viewnews/11659.html");
			hw.OptionAutoCloseOnEnd = true;
			hw.OptionFixNestedTags = true;
			hw.OptionOutputOptimizeAttributeValues = true;
			hw.OptionOutputOriginalCase = false;
			hw.OptionWriteEmptyNodes = true;
			hw.LoadHtml("<body><div onclick=\"alert(1)\" style=\"color:red; behavior:123\">123<span>1233</div></span><script>alert(1)</script></body>");
			var bodyNode = hw.DocumentNode.SelectSingleNode("/html/body") ?? hw.DocumentNode.SelectSingleNode("/body");
			if (bodyNode != null) {
				hw.DocumentNode.RemoveAll();
				hw.DocumentNode.AppendChildren(bodyNode.ChildNodes);
			}
			TagWhiteList = TagWhiteString.Split(',');
			AttributeBlockList = AttributeBlockString.Split(',');
			CssBlockList = CssBlockString.Split(',');
			UrlAttributeWhiteList = UrlAttributeWhiteString.Split(',');
			ProtocolWhiteList = ProtocolWhiteString.Split(',');
			HtmlFilter(hw.DocumentNode.ChildNodes);
			using (var ms = new MemoryStream()) {
				hw.Save(ms);
				ms.Position = 0;
				using (var sr = new StreamReader(ms)) {
					var s = sr.ReadToEnd();
					Console.Write(s);
				}
			}
			//hw.Save(@"d:\news.html");
			Console.ReadLine();
		}

		static void HtmlFilter(IList<HtmlNode> nodes) {
			if (nodes == null || nodes.Count <= 0) {
				return;
			}
			for (var i = 0; i < nodes.Count; i++) {
				var node = nodes[i];
				switch (node.NodeType) {
					case HtmlNodeType.Comment://删除无用的注释
						node.Remove();i--;
						break;
					case HtmlNodeType.Element:
						var nodeName = node.Name.ToLowerInvariant();
						if(!TagWhiteList.Contains(nodeName)) { //节点不在白名单内
							node.Remove(); i--;
						} else {
							if(node.HasAttributes) {//判断属性是否被允许
								for (var k = 0; k < node.Attributes.Count; k++) {
									var attr = node.Attributes[k];
									var attrName = attr.Name.ToLowerInvariant();
									var attrValue = attr.Value;
									if(attrName.StartsWith("on")) { //所有以on开头的事件全部移除
										attr.Remove();k--;
									} else if(AttributeBlockList.Contains(attrName)) {//在黑名单里
										attr.Remove(); k--;
									} else if(attrName == "style") { //如果是样式表，且含有不被允许的单词，则移除
										if(!string.IsNullOrEmpty(attrValue)) {
											attrValue = attrValue.ToLowerInvariant();
											foreach (var css in CssBlockList) {
												if (attrValue.Contains(css)) {
													attr.Remove(); k--;
													break;
												}
											}
										}
									} else {
										//判断含有链接的属性里的链接是否安全
										if(UrlAttributeWhiteList.Contains(attrName)) {
											if(!string.IsNullOrEmpty(attrValue)) {
												attrValue = attrValue.ToLowerInvariant();
												if(!ProtocolWhiteList.Any(x => attrValue.StartsWith(x))) {//不在安全链接里，则删除
													attr.Remove();k--;
												}
											}
										}
									}
								}
							}
							//处理子节点
							HtmlFilter(node.ChildNodes);
						}
						break;
				}
			}
		}

		static void Main0(string[] args) {
			var keys = StringHelper.GetKeywords("法轮功|法国|TMD|fuck");
			Console.WriteLine(StringHelper.FilterKeywords("ｆｕｃk，中国，法國。。。。哈哈。法\r\n轮功。。。。。。", keys));
			Console.ReadLine();
		}

		static void Main1() {
			do {
				var url = Console.ReadLine();
				if(string.IsNullOrEmpty(url)) {
					break;
				}
				VideoItem video;
				var result = WebHelper.TryParseVideo(url, out video);
				Console.WriteLine(result);
				Console.WriteLine(video.Url);
				Console.WriteLine(video.Host);
				Console.WriteLine(video.VideoID);
				Console.WriteLine(video.IconUrl);
				Console.WriteLine(video.VideoUrl);
			} while(true);
		}
	}
}

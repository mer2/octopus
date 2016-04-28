using Microsoft.VisualStudio.TestTools.UnitTesting;
using Octopus.Utils;

namespace TestProject1
{
	[TestClass]
	public class HtmlFilterTest
	{
		[TestMethod]
		public void TestMethod() {
			var result = StringHelper.SafeHtml("<html><head><script type=\"javascript\"></script></head><body><style>a { }</style><div onclick=\"alert(1)\" style=\"color:red; behavior:123\">123<span>1233</div></span><script>alert(1)</script></body></html>");
			Assert.AreEqual("<div>123<span>1233</span></div>", result);
			const string html = "<p><embed src=\"http://player.youku.com/player.php/sid/XNTQ5NDE3MDc2/v.swf\" allowfullscreen=\"true\" quality=\"high\" width=\"480\" height=\"400\" align=\"middle\" allowscriptaccess=\"always\" type=\"application/x-shockwave-flash\" data-pinit=\"registered\" /></p>";
			result = StringHelper.SafeHtml(html);
			Assert.AreEqual("<p />", result);
			result = StringHelper.SafeHtml(html, true);
			Assert.AreEqual("<p><embed src=http://player.youku.com/player.php/sid/XNTQ5NDE3MDc2/v.swf allowfullscreen=true quality=high width=480 height=400 align=middle allowscriptaccess=always type=application/x-shockwave-flash data-pinit=registered /></p>", result);
		}

		[TestMethod]
		public void TestHtml2Text() {
			const string html = "<p><embed src=\"http://player.youku.com/player.php/sid/XNTQ5NDE3MDc2/v.swf\" allowfullscreen=\"true\" quality=\"high\" width=\"480\" height=\"400\" align=\"middle\" allowscriptaccess=\"always\" type=\"application/x-shockwave-flash\" data-pinit=\"registered\" /></p>";
			var result = StringHelper.Html2Text(html, "");
			Assert.AreEqual("", result);
			const string html1 = "<p>123<div>1244</div><embed src=\"http://player.youku.com/player.php/sid/XNTQ5NDE3MDc2/v.swf\" allowfullscreen=\"true\" quality=\"high\" width=\"480\" height=\"400\" align=\"middle\" allowscriptaccess=\"always\" type=\"application/x-shockwave-flash\" data-pinit=\"registered\" /></p>";
			result = StringHelper.Html2Text(html1, "");
			Assert.AreEqual("1231244", result);
			result = StringHelper.Html2Text(html1);
			Assert.AreEqual("123 1244", result);
			const string html2 = "<p>12哈哈哈3<div>1244</div><embed src=\"http://player.youku.com/player.php/sid/XNTQ5NDE3MDc2/v.swf\" allowfullscreen=\"true\" quality=\"high\" width=\"480\" height=\"400\" align=\"middle\" allowscriptaccess=\"always\" type=\"application/x-shockwave-flash\" data-pinit=\"registered\" /></p>";
			result = html2.HtmlSubstring(8);
			Assert.AreEqual("12哈哈哈 ...", result);
			result = html2.HtmlSubstring(4, 0, false);
			Assert.AreEqual("12哈哈 ...", result);
			result = html2.HtmlSubstring(5, 0, false);
			Assert.AreEqual("12哈哈哈 ...", result);
			result = html2.HtmlSubstring(7);
			Assert.AreEqual("12哈哈哈 ...", result);
			result = html2.HtmlSubstring(12);
			Assert.AreEqual("12哈哈哈3 12 ...", result);
			result = html2.HtmlSubstring(12, ellipsis:null);
			Assert.AreEqual("12哈哈哈3 12", result);
			result = html2.HtmlSubstring(12, -1, ellipsis: null);
			Assert.AreEqual("12哈哈哈3 12", result);
			result = html2.HtmlSubstring(12, 100, ellipsis: null);
			Assert.AreEqual("", result);
			result = html2.HtmlSubstring(100);
			Assert.AreEqual("12哈哈哈3 1244", result);
		}
	}
}
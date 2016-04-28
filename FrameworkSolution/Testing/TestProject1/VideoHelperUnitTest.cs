using Microsoft.VisualStudio.TestTools.UnitTesting;
using Octopus.Utils;

namespace TestProject1
{
	[TestClass]
	public class VideoHelperUnitTest
	{
		[TestMethod]
		public void TestMethod() {
			//youku.com
			VideoItem video;
			var result = WebHelper.TryParseVideo("http://v.youku.com/v_show/id_XNDk3MjcxNDg0.html", out video);
			Assert.IsTrue(result);
			Assert.AreEqual("XNDk3MjcxNDg0", video.VideoID);
			Assert.AreEqual("http://g2.ykimg.com/1100641F4650E689B1431802EFD18BD91D4F10-1868-3012-E851-F8D7F76B90CC", video.IconUrl);
			Assert.AreEqual("http://player.youku.com/player.php/sid/XNDk3MjcxNDg0/v.swf", video.VideoUrl);

			//tudou.com
			result = WebHelper.TryParseVideo("http://www.tudou.com/programs/view/2Ml4NgvjR1s/", out video);
			Assert.IsTrue(result);
			Assert.AreEqual("2Ml4NgvjR1s", video.VideoID);
			Assert.AreEqual("http://i3.tdimg.com/160/659/636/w.jpg", video.IconUrl);
			Assert.AreEqual("http://www.tudou.com/v/2Ml4NgvjR1s/v.swf", video.VideoUrl);

			//tudou.com
			result = WebHelper.TryParseVideo("http://www.tudou.com/albumplay/QQnO7F77NlI/QwL2LJHSHww.html", out video);
			Assert.IsTrue(result);
			Assert.AreEqual("QwL2LJHSHww", video.VideoID);
			Assert.AreEqual("http://g4.ykimg.com/w.com/01270F1F4650DBE60E5A5A0123193C70659795-BA6B-B336-8A15-FBB650B8069E", video.IconUrl);
			Assert.AreEqual("http://www.tudou.com/v/QwL2LJHSHww/v.swf", video.VideoUrl);

			//ku6.com
			result = WebHelper.TryParseVideo("http://v.ku6.com/special/show_6591050/_RlOtbjW1h0A5x22hRscqg...html", out video);
			Assert.IsTrue(result);
			Assert.AreEqual("_RlOtbjW1h0A5x22hRscqg..", video.VideoID);
			Assert.AreEqual("http://vi0.ku6img.com/data1/p2/ku6video/2013/1/4/19/1362509900137_38724196_38724196/105.jpg", video.IconUrl);
			Assert.AreEqual("http://player.ku6.com/refer/_RlOtbjW1h0A5x22hRscqg../v.swf", video.VideoUrl);
		}
	}
}

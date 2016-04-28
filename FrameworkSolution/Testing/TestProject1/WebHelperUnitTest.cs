using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Octopus.Utils;

namespace TestProject1
{
	[TestClass]
	public class WebHelperUnitTest
	{
		[TestMethod]
		public void SignObjectTestMethod() {
			var signString = WebHelper.BuildSignString(new { UserID = 120L, Amount = (decimal)12.00, Tips = "你好", ReturnUrl = "http://www.joyyang.com", CreateTime = new DateTime(2013, 11, 1, 10, 45, 00) });
			Assert.AreEqual("amount=12.00&createtime=20131101104500&returnurl=http://www.joyyang.com&tips=你好&userid=120&", signString.ToString());

			signString = WebHelper.BuildSignString(new {
				UserID = 120L, Amount = (decimal)12.00, Tips = "你好", ReturnUrl = "http://www.joyyang.com", CreateTime = new DateTime(2013, 11, 1, 10, 45, 00),
				List = new[] { new {
					A = 1, B = 2
				}, new {
					A = 3, B = 4
				} }
			});
			Assert.AreEqual("amount=12.00&createtime=20131101104500&list=a:0=1&b:0=2&a:1=3&b:1=4&returnurl=http://www.joyyang.com&tips=你好&userid=120&", signString.ToString());

			signString = WebHelper.BuildSignString(new {
				UserID = 120L, Amount = (decimal)12.00, Tips = "你好", ReturnUrl = "http://www.joyyang.com", CreateTime = new DateTime(2013, 11, 1, 10, 45, 00),
				List = new[] { 1,2,3,4,5 }
			});
			Assert.AreEqual("amount=12.00&createtime=20131101104500&list=1,2,3,4,5&returnurl=http://www.joyyang.com&tips=你好&userid=120&", signString.ToString());

			var signedString = WebHelper.SignObject(new { UserID = 120L, Amount = (decimal)12.00, Tips = "你好", ReturnUrl = "http://www.joyyang.com", CreateTime = new DateTime(2013, 11, 1, 10, 45, 00) }, "123456");
			Assert.AreEqual("D23ADD76C11A3A5CAB80F1E7126FAC5E", signedString);

			var signedResult = WebHelper.ValidateSign(new { UserID = 120L, Amount = (decimal)12.00, Tips = "你好", ReturnUrl = "http://www.joyyang.com", CreateTime = new DateTime(2013, 11, 1, 10, 45, 00) }, "123456", "D23ADD76C11A3A5CAB80F1E7126FAC5E");
			Assert.IsTrue(signedResult);

			var url = WebHelper.ObjectToUrl(new { UserID = 120L, Amount = (decimal)12.00, Tips = "你好", ReturnUrl = "http://www.joyyang.com", CreateTime = new DateTime(2013, 11, 1, 10, 45, 00) });
			Assert.AreEqual("UserID=120&Amount=12.00&Tips=%e4%bd%a0%e5%a5%bd&ReturnUrl=http%3a%2f%2fwww.joyyang.com&CreateTime=20131101104500", url);
		}
	}
}

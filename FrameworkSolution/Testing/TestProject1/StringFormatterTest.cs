using System;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Octopus.Utils;

namespace TestProject1
{
	[TestClass]
	public class StringFormatterTest
	{
		[TestMethod]
		public void StringFormatterTestMethod() {
			var result = "{BusinessNo},{CreateTime:yyyy-MM-dd HH:mm:ss},{BusinessName},￥{ToAmount:0.00},{UserName},{ToUserName},{StatusTitle}".NamedStringFormat(
			new { BusinessNo = "1234567890", CreateTime = DateTime.Parse("2013-11-29 10:43:55"), BusinessName = "你好", ToAmount = 10.89m, UserName = "大家好", ToUserName = 12 });
			Assert.AreEqual("1234567890,2013-11-29 10:43:55,你好,￥10.89,大家好,12,", result);
		}
	}
}

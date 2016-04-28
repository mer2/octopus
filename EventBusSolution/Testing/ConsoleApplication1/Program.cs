using System;
using System.Collections;
using Octopus.EventBus;

namespace ConsoleApplication1
{
	class Program
	{
		static void Main(string[] args) {
			do {
				PublishMessageDirect();
				Console.ReadLine();
			} while (true); 
		}

		/// <summary>
		/// 直接发送消息
		/// </summary>
		static void PublishMessageDirect() {
			var result = EventClientService.Publish("通行证登录", new Hashtable { { "UserID", "xxx" }, { "UserName", "yyy" }, { "NickName", "这是中文" } }, category: "Joy1.Passport.All", eventName: "Login");
			if(result.ResultNo == 0) {
				Console.WriteLine("消息发送成功");
			} else {
				Console.WriteLine("消息发送失败：" + result.ResultDescription);
			}
		}
	}

	class MessageHandler : IEventSubscriber
	{
		public void OnMessageReceived(IEventClientService service, EventMessage message) {
			Console.WriteLine("{0}, {1}", message.Category, message.Message);
		}
	}
}

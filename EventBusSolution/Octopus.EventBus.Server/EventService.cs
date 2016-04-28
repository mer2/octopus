using System.Linq;
using System.Diagnostics;
using HTB.DevFx.Core;
using HTB.DevFx.Log;
using Octopus.EventBus.Server.Config;

namespace Octopus.EventBus.Server
{
	public class EventService : ServiceBase<EventServiceSetting>, IEventService
	{
		[Conditional("DEBUG")]
		private void WriteLog(string message, params object[] args) {
			LogService.WriteLog(this, message, args);
		}
		private IEventRepository Repository;
		private IEventDispatcher Dispatcher;
		private EventCleaner Cleaner;
		protected override void OnInit() {
			base.OnInit();
			this.Repository = this.ObjectService.GetObject<IEventRepository>();
			this.Dispatcher = new EventDispatcher();//消息分发器
			this.Dispatcher.Init(this.Repository, this.Setting.DispatchInterval);
			var cleanInterval = this.Setting.CleanInterval;
			if (cleanInterval > 0) {
				this.Cleaner = new EventCleaner();//清理没有被订阅的消息
				this.Cleaner.Init(this.Repository, this.Setting.CleanBatchSize, this.Setting.CleanInterval);
			}
		}

		public IAOPResult Publish(EventMessage message) {//发布消息
			this.WriteLog("Publish {0}", message);
			//插入到主消息队列中
			return this.Repository.PushToMainQueue(new EventMessageHeader(message));
		}

		private EventMessage PopMessage(string queueName) {
			var header = this.Repository.PopFromSubscribeQueue(queueName);
			return header == null ? null : header.Message;
		}

		public IAOPResult RegisterSubscribers(params EventMessage[] messages) {//注册订阅器
			if(messages == null || messages.Length <= 0) {
				return AOPResult.Failed("无订阅");
			}
			this.WriteLog("RegisterSubscribers {0}", messages.Length);
			return this.Repository.RegisterSubscribers(messages);
		}

		public EventMessage Receive(params string[] channels) {//查询并接收消息，channels是订阅器ID列表
			if(channels == null || channels.Length <= 0) {
				return null;
			}
			foreach(var channel in channels) {
				var message = this.PopMessage(channel);
				if(message == null) {
					continue;
				}
				message.ChannelID = channel;
				return message;
			}
			return null;
		}

		public EventMessage[] Receives(int count, string channel) {//批量查询并接收消息
			var headers = this.Repository.PopMessagesFromSubscribeQueue(channel, count);
			if(headers != null && headers.Length > 0) {
				return headers.Select(h => h.Message).Where(m => m != null).ToArray();
			}
			return null;
		}

		public int GetMessageCount(params string[] channels) {//查询订阅消息数
			return this.Repository.GetSubscribeMessageCount(channels);
		}
	}
}

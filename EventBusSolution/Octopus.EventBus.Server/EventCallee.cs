using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using HTB.DevFx;
using HTB.DevFx.Log;

namespace Octopus.EventBus.Server
{
	public class EventCallee : IEventCallee
	{
		[Conditional("DEBUG")]
		private void WriteLog(string message, params object[] args) {
			LogService.WriteLog(this, message, args);
		}
		private readonly Dictionary<EventMessage, IEventCallback> callbacks = new Dictionary<EventMessage, IEventCallback>();
		protected virtual void SendMessageInternal(IEventRepository repository, EventMessage subscriber) {//主动发送消息到订阅方
			if (subscriber == null || string.IsNullOrEmpty(subscriber.MessageID)) {
				return;
			}
			IEventCallback callback;
			if (!this.callbacks.TryGetValue(subscriber, out callback)) {
				lock (this.callbacks) {
					if (!this.callbacks.TryGetValue(subscriber, out callback)) {
						callback = ObjectService.GetObject<IEventCallback>(subscriber.MessageID);
						if (callback != null) {
							this.callbacks.Add(subscriber, callback);
						}
					}
				}
			}
			if (callback == null) {
				this.WriteLog("无法创建回调器");
				return;
			}
			var channelId = subscriber.ChannelID;
			do {
				var header = repository.PopFromSubscribeQueue(channelId);
				if (header == null || header.Message == null) {
					break;
				}
				var message = header.Message;
				message.ChannelID = channelId;
				header.SentCount++;
				bool sentFailed;
				try {
					var result = callback.OnMessageReceived(message);
					this.WriteLog("回调结果：id={0}, channel={1}, result={2}", header.MessageGuid, channelId, result);
					sentFailed = result.ResultNo != 0;
				} catch (Exception e) {
					this.WriteLog("回调异常：id={0}, channel={1}\r\n{2}", header.MessageGuid, channelId, e);
					sentFailed = true;
				}
				if(sentFailed) {
					this.WriteLog("回调失败 {0} 次，放弃", 1);
				}
			} while(true);
		}

		protected void SendMessageInternal(IEventRepository repository, EventMessage[] subscribers) {
			if(subscribers == null || subscribers.Length <= 0) {
				return;
			}
			if(subscribers.Length == 1) {
				this.SendMessageInternal(repository, subscribers[0]);
				return;
			}
			Parallel.ForEach(subscribers, m => this.SendMessageInternal(repository, m));
		}

		void IEventCallee.SendMessage(IEventRepository repository, params EventMessage[] subscribers) {
			this.SendMessageInternal(repository, subscribers);
		}
	}
}

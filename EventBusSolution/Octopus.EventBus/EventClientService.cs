using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using HTB.DevFx.Core;
using HTB.DevFx.Esb;
using HTB.DevFx.Exceptions;
using HTB.DevFx.Log;
using Octopus.EventBus.Config;
using HTB.DevFx.Utils;
using Timer = System.Timers.Timer;

namespace Octopus.EventBus
{
	public class EventClientService : TimerBase, IInitializable<EventClientServiceSetting>, IEventClientService, IEventCallback
	{
		protected internal EventClientService() { }

		protected virtual IAOPResult PublishToServer(EventMessage message) {
			try {
				return this.EventService.Publish(message);
			} catch(Exception e) {
				ExceptionHelper.Publish(e);
				return AOPResult.Failed(e.Message);
			}
		}

		protected virtual IAOPResult PublishToLocal(EventMessage message) {
			if (this.LocalSubscriberMessages.Count <= 0) {
				return AOPResult.Success();
			}
			var subs = GetDispatchedSubscribers(message, this.LocalSubscriberMessages);
			if (subs.Count <= 0) {
				return AOPResult.Success();
			}
			foreach (var sub in subs) {
				var subscriber = sub.Message as IEventSubscriber;
				if (subscriber == null) {
					continue;
				}
				try {
					subscriber.OnMessageReceived(this, message);
				} catch (Exception e) {
					ExceptionHelper.Publish(e);
				}
			}
			return AOPResult.Success();
		}

		protected virtual IAOPResult PublishMessage(EventMessage message) {
			IAOPResult result = null;
			if (string.IsNullOrEmpty(message.MessageGuid)) {
				message.MessageGuid = Guid.NewGuid().ToString("N");
			}
			if (this.Setting.PublishToServer) {
				result = this.PublishToServer(message);
			}
			if (this.Setting.PublishToLocal) {
				result = this.PublishToLocal(message);
			}
			if (result == null) {
				result = AOPResult.Success();
			}
			return result;
		}

		protected virtual IAOPResult PublishInternal(EventMessage message, bool sync) {
			if (sync || this.MessageQueue.Count >= this.PublishMaxBufferSize) {
				this.StartTimer();
				return this.PublishMessage(message);
			}
			lock(this.MessageQueue) {
				this.MessageQueue.Enqueue(message);
			}
			return AOPResult.Success();
		}

		protected virtual IAOPResult<EventMessage> PublishInternal(object message, IDictionary data, bool sync, string category, string eventName, int priority, MessageFlags messageType, string messageId, params string[] tags) {
			if(string.IsNullOrEmpty(category)) {
				return this.PublishInternal(category, message, data, sync, eventName, priority, messageType, messageId, tags);
			}
			var msg = new EventMessage {
				Category = category,
				EventName = eventName,
				CreatedTime = DateTime.Now,
				Message = message,
				MessageID = messageId,
				Priority = priority,
				MessageType = messageType,
				Tags = tags,
				Data = data
			};
			var result = this.PublishInternal(msg, sync);
			return AOPResult.Create(result.ResultNo, result.ResultDescription, msg, result);
		}

		protected virtual IAOPResult<EventMessage> PublishInternal(string alias, object message, IDictionary data, bool sync, string eventName, int priority, MessageFlags messageType, string messageId, params string[] tags) {
			if(string.IsNullOrEmpty(alias) && this.Setting != null && this.Setting.PublisherSetting != null) {
				alias = this.Setting.PublisherSetting.DefaultPublisher;
			}
			if (string.IsNullOrEmpty(alias)) {
				return AOPResult.Failed<EventMessage>("alias 参数为空");
			}
			if (!this.Publishers.ContainsKey(alias)) {
				return AOPResult.Failed<EventMessage>(string.Format("未找到别名为 {0} 的发布配置", alias));
			}
			var setting = this.Publishers[alias];
			var category = setting.Category;
			if(string.IsNullOrEmpty(category)) {
				return AOPResult.Failed<EventMessage>("发布配置中未配置Category属性");
			}
			if(eventName == null) {
				eventName = setting.EventName;
			}
			if(priority == 0) {
				priority = setting.Priority;
			}
			if(tags == null) {
				tags = setting.Tags;
			}
			return this.PublishInternal(message, data, sync, category, eventName, priority, messageType, messageId, tags);
		}

		protected virtual void WriteLog(string message, params object[] args) {
			if(this.Setting == null || !this.Setting.Debug) {
				return;
			}
			LogHelper.WriteLog(this, LogLevel.INFO, message, args);
		}

		private Dictionary<string, PublisherSetting> Publishers;
		private Dictionary<string, IEventSubscriber> Subscribers;
		private List<EventMessage> LocalSubscriberMessages;
		private string[] Channels;
		private EventClientServiceSetting Setting;
		private Queue<EventMessage> MessageQueue;
		private bool FetchMessageOnTimer = true;
		private bool SubscribersRegistered;
		private bool SubscribersRegistering;
		private List<EventMessage> SubscriberMessages;
		private Timer PublishTimer;
		private Timer ReceiveTimer;
		private double PublishInterval = 100;
		private double ReceiveInterval = 100;
		private int PublishMaxBufferSize = 1000;
		private DateTime LastRegisterSubscriberTime;
		protected virtual void Init(EventClientServiceSetting setting) {
			this.Setting = setting;

			this.WriteLog("EventClientService Begin Init...");
			this.Publishers = new Dictionary<string, PublisherSetting>();
			this.Subscribers = new Dictionary<string, IEventSubscriber>();
			this.LocalSubscriberMessages = new List<EventMessage>();
			this.SubscriberMessages = new List<EventMessage>();
			if(setting != null) {
				this.PublishInterval = setting.PublisherSetting.Interval;
				this.PublishMaxBufferSize = setting.PublisherSetting.MaxBufferSize;
				this.ReceiveInterval = setting.SubscriberSetting.Interval;

				foreach(var ps in setting.PublisherSetting.Publishers) {
					this.Publishers.Add(ps.Name, ps);
				}
				this.WriteLog("Find {0} Pulishers.", this.Publishers.Count);
				var subscribers = this.SubscriberMessages;
				var channels = new List<string>();
				var callback = setting.SubscriberSetting.SubscriberCallback;
				foreach (var ss in setting.SubscriberSetting.Subscribers) {
					var subscriber = ServiceLocator.GetService<IEventSubscriber>(ss.HandlerName);
					if (subscriber == null) {
						continue;
					}
					if (ss.IsLocal) {//本地消费者
						this.LocalSubscriberMessages.Add(new EventMessage { MessageType = MessageFlags.Subscriber, Priority = ss.Timeout, Category = ss.Category, CreatedTime = DateTime.Now, EventName = ss.EventName, ChannelID = ss.Name, Message = subscriber, Tags = ss.Tags, MessageID = callback });
						continue;
					}
					this.Subscribers.Add(ss.Name, subscriber);
					channels.Add(ss.Name);
					if(ss.Register) {
						subscribers.Add(new EventMessage { MessageType = MessageFlags.Subscriber, Priority = ss.Timeout, Category = ss.Category, CreatedTime = DateTime.Now, EventName = ss.EventName, ChannelID = ss.Name, Message = ss.Title, Tags = ss.Tags, MessageID = callback });
					}
				}
				this.WriteLog("Find {0} Subscribers.", this.Subscribers.Count);
				this.WriteLog("Find {0} Local Subscribers.", this.LocalSubscriberMessages.Count);
				this.Channels = channels.ToArray();
				if (setting.Interval > 0) {
					this.Interval = setting.Interval;
				}
				this.FetchMessageOnTimer = this.Setting.SubscriberSetting.IntervalEnabled;
			}
			this.MessageQueue = new Queue<EventMessage>();
			this.OnTimer();
			this.WriteLog("EventClientService Init Done.");
		}

		protected override void OnTimer() {
			this.InitPulisher();
			//是否需要再次注册订阅器
			var now = DateTime.Now;
			if (this.LastRegisterSubscriberTime.AddMinutes(this.Setting.SubscriberSetting.RegisterInterval) < now && !this.SubscribersRegistering) {
				this.LastRegisterSubscriberTime = now;
				this.SubscribersRegistered = false;
				this.RegisterSubscribers();
			}
			this.InitReceiver();
			this.StartTimer();
		}

		protected virtual void InitPulisher() {
			var timer = this.PublishTimer;
			if (timer == null) {
				timer = this.PublishTimer = new Timer(this.PublishInterval) { AutoReset = false };
				timer.Disposed += delegate { this.PublishTimer = null; };
				timer.Elapsed += delegate {
					this.PublishMessages();
					if (this.PublishTimer != null) {
						try {
							this.PublishTimer.Start();
						} catch (ObjectDisposedException) {
							this.PublishTimer = null;
						}
					}
				};
			}
			try {
				this.PublishTimer.Start();
			} catch (ObjectDisposedException) {
				this.PublishTimer = null;
			}
		}

		protected virtual void InitReceiver() {
			if (!this.FetchMessageOnTimer || this.Channels == null || this.Channels.Length <= 0) {
				return;
			}
			var timer = this.ReceiveTimer;
			if (timer == null) {
				timer = this.ReceiveTimer = new Timer(this.ReceiveInterval) { AutoReset = false };
				timer.Disposed += delegate { this.ReceiveTimer = null; };
				timer.Elapsed += delegate {
					this.SubscribeMessages();
					if (this.ReceiveTimer != null) {
						try {
							this.ReceiveTimer.Start();
						} catch (ObjectDisposedException) {
							this.ReceiveTimer = null;
						}
					}
				};
			}
			try {
				this.ReceiveTimer.Start();
			} catch (ObjectDisposedException) {
				this.ReceiveTimer = null;
			}
		}

		protected virtual void RegisterSubscribers() {
			var subscribers = this.SubscriberMessages;
			if(this.SubscribersRegistered || this.SubscribersRegistering || subscribers.Count <= 0) {
				return;
			}
			this.SubscribersRegistering = true;
			if(!this.SubscribersRegistered) {
				lock (this.SubscriberMessages) {
					if(!this.SubscribersRegistered) {
						try {
							var result = this.EventService.RegisterSubscribers(subscribers.ToArray());
							this.SubscribersRegistered = true;
							this.WriteLog("Registered {0} Subscribers: {1}; result: {2}", subscribers.Count, string.Join(",", this.Channels), result);
						} catch(Exception e) {
							ExceptionHelper.Publish(e);
						}
					}
				}
			}
			this.SubscribersRegistering = false;
		}

		protected virtual void SubscribeMessages() {
			if(!this.FetchMessageOnTimer || this.Channels == null || this.Channels.Length <= 0) {
				return;
			}
			try {
				var count = this.EventService.GetMessageCount(this.Channels);
				if(count <= 0) {
					return;
				}
				while(--count >= 0) {
					var message = this.EventService.Receive(this.Channels);
					if(message == null) {
						break;
					}
					this.WriteLog("receive: id={0}, channel={1}, category={2}, eventName={3}", message.MessageGuid, message.ChannelID, message.Category, message.EventName);
					ThreadPool.QueueUserWorkItem(this.RaiseSubscribeEventSafety, message);
				}
			} catch(Exception e) {
				ExceptionHelper.Publish(e);
			}
		}

		protected virtual void RaiseSubscribeEventSafety(object state) {
			try {
				this.RaiseSubscribeEvent(state);
			} catch(Exception e) {
				ExceptionHelper.Publish(e);
			}
	}

		protected virtual void RaiseSubscribeEvent(object state) {
			var message = state as EventMessage;
			if(message != null) {
				this.WriteLog("callback: id={0}, channel={1}, category={2}, eventName={3}", message.MessageGuid, message.ChannelID, message.Category, message.EventName);
				var channelId = message.ChannelID;
				IEventSubscriber subscriber;
				if(string.IsNullOrEmpty(channelId) || !this.Subscribers.TryGetValue(channelId, out subscriber)) {
					this.WriteLog("channelId is null or not found. drop it ({0})...", message.MessageGuid);
					return;
				}
				subscriber.OnMessageReceived(this, message);
			}
		}

		protected virtual void PublishMessages() {
			if(this.MessageQueue.Count <= 0) {
				return;
			}
			lock(this.MessageQueue) {
				while(this.MessageQueue.Count > 0) {
					var message = this.MessageQueue.Dequeue();
					if (message != null) {
						ThreadPool.QueueUserWorkItem(this.PublishMessage, message);
					}
				}
			}
		}

		protected virtual void PublishMessage(object state) {
			var message = state as EventMessage;
			if (message != null) {
				this.PublishMessage(message);
			}
		}

		protected virtual IAOPResult OnMessageReceived(EventMessage message) {
			if (message != null) {
				this.WriteLog("callback receive: id={0}, channel={1}, category={2}, eventName={3}", message.MessageGuid, message.ChannelID, message.Category, message.EventName);
				ThreadPool.QueueUserWorkItem(this.RaiseSubscribeEventSafety, message);
			}
			return AOPResult.Success();
		}

		protected virtual IEventService EventService {
			get { return ServiceLocator.GetService<IEventService>(); }
		}

		void IInitializable<EventClientServiceSetting>.Init(EventClientServiceSetting setting) {
			this.Init(setting);
		}

		IAOPResult IEventClientService.Publish(EventMessage message, bool sync) {
			return this.PublishInternal(message, sync);
		}

		IAOPResult<EventMessage> IEventClientService.PublishTo(string alias, object message, IDictionary data, bool sync, string eventName, int priority, MessageFlags messageType, string messageId, params string[] tags) {
			return this.PublishInternal(alias, message, data, sync, eventName, priority, messageType, messageId, tags);
		}

		IAOPResult<EventMessage> IEventClientService.Publish(object message, IDictionary data, bool sync, string category, string eventName, int priority, MessageFlags messageType, string messageId, params string[] tags) {
			return this.PublishInternal(message, data, sync, category, eventName, priority, messageType, messageId, tags);
		}

		IAOPResult IEventCallback.OnMessageReceived(EventMessage message) {
			return this.OnMessageReceived(message);
		}

		public static List<EventMessage> GetDispatchedSubscribers(EventMessage message, List<EventMessage> list) {
			var subs = new List<EventMessage>();//消息被分发到的订阅队列
			foreach(var subscriber in list) {
				if(message.Category.StartsWith(subscriber.Category, true, null)) {
					if(!string.IsNullOrEmpty(subscriber.EventName)) {
						if(string.IsNullOrEmpty(message.EventName) || !subscriber.EventName.StartsWith(message.EventName, true, null)) {
							continue;//此消息不符合订阅器要求
						}
					}
					//此消息符合订阅要求
					subs.Add(subscriber);
				}
			}
			return subs;
		}

		public static IEventClientService Current {
			get { return ServiceLocator.GetService<IEventClientService>(); }
		}

		/// <summary>
		/// 消息系统客户端初始化，视情况需要在应用启动时调用，比如放在global.asax中调用
		/// </summary>
		public static void Initialize() {
			Current.ToString();
		}

		/// <summary>
		/// 发布消息
		/// </summary>
		/// <param name="message">自定义的消息</param>
		/// <param name="sync">是否同步发送（默认为异步发送）</param>
		/// <returns>是否成功提交到服务端</returns>
		public static IAOPResult Publish(EventMessage message, bool sync = false) {
			return Current.Publish(message, sync);
		}

		/// <summary>
		/// 发布消息
		/// </summary>
		/// <param name="alias">此类消息的别名（定义在配置中，null表示使用默认配置）</param>
		/// <param name="message">消息体</param>
		/// <param name="data">消息键值对</param>
		/// <param name="sync">是否同步发送（默认为异步发送）</param>
		/// <param name="eventName">消息事件名</param>
		/// <param name="priority">消息优先级</param>
		/// <param name="messageType">消息类型</param>
		/// <param name="messageId">消息自定义的ID（用于消息之间的关联）</param>
		/// <param name="tags">消息标签（可用来匹配消息）</param>
		/// <returns>是否成功提交到服务端</returns>
		public static IAOPResult<EventMessage> PublishTo(string alias, object message, IDictionary data = null, bool sync = false, string eventName = null, int priority = 0, MessageFlags messageType = MessageFlags.Default, string messageId = null, params string[] tags) {
			return Current.PublishTo(alias, message, data, sync, eventName, priority, messageType, messageId, tags);
		}

		/// <summary>
		/// 发布消息
		/// </summary>
		/// <param name="message">消息体</param>
		/// <param name="data">消息键值对</param>
		/// <param name="sync">是否同步发送（默认为异步发送）</param>
		/// <param name="category">消息主题</param>
		/// <param name="eventName">消息事件名</param>
		/// <param name="priority">消息优先级</param>
		/// <param name="messageType">消息类型</param>
		/// <param name="messageId">消息自定义的ID（用于消息之间的关联）</param>
		/// <param name="tags">消息标签（可用来匹配消息）</param>
		/// <returns>是否成功提交到服务端</returns>
		public static IAOPResult<EventMessage> Publish(object message, IDictionary data = null, bool sync = false, string category = null, string eventName = null, int priority = 0, MessageFlags messageType = MessageFlags.Default, string messageId = null, params string[] tags) {
			return Current.Publish(message, data, sync, category, eventName, priority, messageType, messageId, tags);
		}
	}
}

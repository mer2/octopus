using System;
using System.Collections.Generic;
using System.Text;
using HTB.DevFx.Core;
using Octopus.EventBus.Server.Config;
using ServiceStack.Redis;

namespace Octopus.EventBus.Server
{
	public class EventRepository : ServiceBase<EventRepositorySetting>, IEventRepository
	{
		private string PublishQueueName;
		private string NotifyQueueName;
		private string MessagePrefix;
		private string MessageRefPrefix;
		private string MessageHeaderPrefix;
		private string SubscriberQueuePrefix;
		private string SubscriberKeyPrefix;
		private string SubscriberRefKeyPrefix;
		protected override void OnInit() {
			base.OnInit();
			this.MessagePrefix = "msg:body:";
			this.MessageRefPrefix = "msg:ref:";
			this.MessageHeaderPrefix = "msg:header:";
			this.SubscriberQueuePrefix = "mq:subs:";
			this.PublishQueueName = "mq:main:" + this.Setting.PublishQueueKey;
			this.NotifyQueueName = "mq:notify:" + this.Setting.PublishQueueKey;
			this.SubscriberKeyPrefix = "subs:main:";
			this.SubscriberRefKeyPrefix = "subs:ref:";
		}

		protected RedisClient GetRedisClient() {
			return Joy.Runtime.Redis.RedisClientFactory.GetRedisClient(this.Setting.CacheName);
		}

		protected EventMessageHeader PopMessage(string queueName, bool withBody = false) {
			var msgKey = this.Redis.RightPop(this.RedisDb, queueName, true).GetResult(this.Redis);
			if (msgKey == null) {
				return null;
			}
			var messageId = msgKey.ToGuidString();
			var messageBytes = this.Redis.Get(this.RedisDb, this.MessageHeaderPrefix + messageId, true).GetResult(this.Redis);
			if (messageBytes == null) {
				return null;
			}
			var header = messageBytes.DeserializeTo<EventMessageHeader>();
			if (withBody) {
				var bodyBytes = this.Redis.Get(this.RedisDb, this.MessagePrefix + messageId, true).GetResult(this.Redis);
				if(bodyBytes != null) {
					header.Message = bodyBytes.DeserializeTo<EventMessage>();
				}
			}
			return header;
		}

		public IAOPResult PushToMainQueue(EventMessageHeader header) {
			var message = header.Message; header.Message = null;
			this.Redis.Set(this.RedisDb, this.MessagePrefix + message.MessageGuid, message.SerializeToBytes());
			this.Redis.Set(this.RedisDb, this.MessageHeaderPrefix + message.MessageGuid, header.SerializeToBytes());
			var messageKey = header.GetMessageKey();
			this.Redis.LeftPush(this.RedisDb, this.PublishQueueName, header.GetMessageKey());
			this.Redis.Publish(this.NotifyQueueName, messageKey);
			return AOPResult.Success();
		}

		public IAOPResult PushToSubscribeQueue(string queueName, EventMessageHeader header) {
			this.Redis.LeftPush(this.RedisDb, this.SubscriberQueuePrefix + queueName, header.GetMessageKey());
			this.Redis.Increment(this.RedisDb, this.MessageRefPrefix + header.MessageGuid);
			return AOPResult.Success();
		}

		public EventMessageHeader PopFromMainQueue() {
			return this.PopMessage(this.PublishQueueName);
		}

		public EventMessageHeader PopFromSubscribeQueue(string queueName) {
			this.Redis.Set(this.RedisDb, this.SubscriberRefKeyPrefix + queueName, DateTime.Now.Ticks.ToString());
			var msg = this.PopMessage(this.SubscriberQueuePrefix + queueName, true);
			if(msg != null) {
				this.Redis.Decrement(this.RedisDb, this.MessageRefPrefix + msg.MessageGuid);
			}
			return msg;
		}

		public EventMessageHeader[] PopMessagesFromSubscribeQueue(string queueName, int count) {
			throw new NotImplementedException();
		}

		public IAOPResult RegisterSubscribers(EventMessage[] messages) {
			if(messages != null && messages.Length > 0) {
				foreach(var message in messages) {
					this.Redis.Set(this.RedisDb, this.SubscriberKeyPrefix + message.ChannelID, message.SerializeToBytes());
				}
			}
			this.Redis.Publish(this.NotifyQueueName, Encoding.ASCII.GetBytes("SubscribersChanged"));
			return AOPResult.Success();
		}

		public EventMessage[] GetSubscribers() {
			var keys = this.Redis.GetKeysSync(this.RedisDb, this.SubscriberKeyPrefix + "*", true).GetResult(this.Redis);
			if(keys == null || keys.Length <= 0) {
				return null;
			}
			var list = new List<EventMessage>();
			foreach(var key in keys) {
				var bytes = this.Redis.Get(this.RedisDb, key, true).GetResult(this.Redis);
				if(bytes != null && bytes.Length > 0) {
					var message = bytes.DeserializeTo<EventMessage>();
					if(message != null) {
						list.Add(message);
					}
				}
			}
			return list.ToArray();
		}

		private readonly Dictionary<IEventDispatcher, RedisSubscriberConnection> dispatchers = new Dictionary<IEventDispatcher, RedisSubscriberConnection>();
		public IAOPResult RegisterDispatcher(IEventDispatcher dispatcher) {
			if(!this.dispatchers.ContainsKey(dispatcher)) {
				lock(this.dispatchers) {
					if(!this.dispatchers.ContainsKey(dispatcher)) {
						var redis = new RedisSubscriberConnection(this.Redis.Host, this.Redis.Port);
						redis.Open();
						redis.Subscribe(this.NotifyQueueName, (k, r) => dispatcher.OnMessageReceived(this, r));
						dispatchers.Add(dispatcher, redis);
					}
				}
			}
			return AOPResult.Success();
		}
	}
}

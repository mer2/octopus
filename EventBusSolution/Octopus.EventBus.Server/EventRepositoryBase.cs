using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HTB.DevFx.Core;
using HTB.DevFx.Exceptions;
using Octopus.Esb;
using Octopus.EventBus.Server.Config;

namespace Octopus.EventBus.Server
{
	public abstract class EventRepositoryBase : ServiceBase<EventRepositorySetting>, IEventRepository
	{
		public const string SubscribersChangedName = "SubscribersChanged";//订阅器变化的通知名称
		public static readonly byte[] SubscribersChangedBytes = Encoding.ASCII.GetBytes(SubscribersChangedName);

		private string PublishQueueName;
		private string NotifyQueueName;
		/// <summary>
		/// 完整消息的前缀
		/// </summary>
		private string MessagePrefix;
		/// <summary>
		/// 消息被引用次数的前缀
		/// </summary>
		private string MessageRefPrefix;
		/// <summary>
		/// 消息被分发到哪些订阅队列的前缀
		/// </summary>
		private string MessageDispatchPrefix;
		/// <summary>
		/// 消息头前缀
		/// </summary>
		private string MessageHeaderPrefix;
		/// <summary>
		/// 订阅队列前缀
		/// </summary>
		private string SubscriberQueuePrefix;
		/// <summary>
		/// 订阅信息前缀
		/// </summary>
		private string SubscriberKeyPrefix;
		/// <summary>
		/// 订阅者的生命周期前缀
		/// </summary>
		private string SubscriberRefKeyPrefix;
		/// <summary>
		/// 主存储名（可读写）
		/// </summary>
		private string MainStorageName;
		/// <summary>
		/// 只读存储名
		/// </summary>
		private string ReadOnlyStorageName;
		/// <summary>
		/// 备份队列的后缀
		/// </summary>
		private string BackupQueueSuffix;
		protected override void OnInit() {
			base.OnInit();
			this.Serializer = SerializerFactory.GetSerializer(this.Setting.SerializerName);
			this.Encoding = Encoding.GetEncoding(this.Setting.EncodingName);
			this.MessagePrefix = "msg:body:";//消息体前缀
			this.MessageRefPrefix = "msg:ref:";
			this.MessageDispatchPrefix = "msg:dspt:";
			this.MessageHeaderPrefix = "msg:header:";//消息头前缀
			this.SubscriberQueuePrefix = "mq:subs:";
			this.PublishQueueName = "mq:main:" + this.Setting.PublishQueueKey;
			this.NotifyQueueName = "mq:notify:" + this.Setting.PublishQueueKey;
			this.SubscriberKeyPrefix = "subs:main:";
			this.SubscriberRefKeyPrefix = "subs:ref:";
			this.MainStorageName = this.Setting.MainStorageName;
			this.ReadOnlyStorageName = this.Setting.ReadOnlyStorageName;
			this.BackupQueueSuffix = ":bk";
		}

		/// <summary>
		/// 获取Redis实例（可能是主服务或者从服务）
		/// </summary>
		protected abstract IEventStorage GetStorage(string name);
		protected ISerializer Serializer { get; private set; }
		protected Encoding Encoding { get; private set; }

		protected EventMessageHeader PopMessage(string queueName, bool withBody = false, bool backup = false) {
			using(var storage = this.GetStorage(this.ReadOnlyStorageName)) {
				return this.PopMessage(storage, queueName, withBody, backup);
			}
		}

		protected EventMessageHeader PopMessage(IEventStorage storage, string queueName, bool withBody = false, bool backup = false) {
			var msgKey = backup ? storage.RightPopLeftPush(queueName, queueName + this.BackupQueueSuffix) : storage.RightPop(queueName);
			if(msgKey == null) {
				return null;
			}
			var messageId = msgKey.ToGuidString();
			EventMessageHeader header;
			if(withBody) {
				var result = storage.MultiGet(this.MessageHeaderPrefix + messageId, this.MessagePrefix + messageId);
				if(result == null || result.Length < 2) {
					return null;
				}
				header = this.Serializer.Deserialize<EventMessageHeader>(result[0], null);
				header.Message = this.Serializer.Deserialize<EventMessage>(result[1], null);
			} else {
				var result = storage.Get(this.MessageHeaderPrefix + messageId);
				if(result == null || result.Length <= 0) {
					return null;
				}
				header = this.Serializer.Deserialize<EventMessageHeader>(result, null);
			}
			return header;
		}

		public IAOPResult PushToMainQueue(EventMessageHeader header) {//新消息发布到主队列
			var message = header.Message; header.Message = null;
			using(var storage = this.GetStorage(this.MainStorageName)) {
				storage.Pipeline(s => {
					//保存消息主体和消息头
					s.MultiSet(new Dictionary<string, byte[]> {
						{ this.MessagePrefix + message.MessageGuid, this.Serializer.Serialize(message, null) },
						{ this.MessageHeaderPrefix + message.MessageGuid, this.Serializer.Serialize(header, null) },
					});
					var messageKey = header.GetMessageKey();
					s.LeftPush(this.PublishQueueName, messageKey);//把消息ID入列到消息队列里
					s.Publish(this.NotifyQueueName, messageKey);//通知分发器有新消息过来了
				});
			}
			return AOPResult.Success();
		}

		public IAOPResult PushToSubscribeQueue(string queueName, EventMessageHeader header) {//新消息分发到各个订阅队列
			using(var storage = this.GetStorage(this.MainStorageName)) {
				storage.Pipeline(s => {
					s.LeftPush(this.SubscriberQueuePrefix + queueName, header.GetMessageKey());//把消息ID入列
					s.Increment(this.MessageRefPrefix + header.MessageGuid);//增加消息的引用数
				});
			}
			return AOPResult.Success();
		}

		//新消息分发到各个订阅队列
		public IAOPResult PushToSubscribeQueues(EventMessageHeader header, string[] queueNames) {
			if (queueNames != null && queueNames.Length > 0) {
				using(var storage = this.GetStorage(this.MainStorageName)) {
					storage.Pipeline(s => {
						foreach (var queueName in queueNames) {
							s.LeftPush(this.SubscriberQueuePrefix + queueName, header.GetMessageKey()); //把消息ID入列
							s.Increment(this.MessageRefPrefix + header.MessageGuid); //增加消息的引用数
						}
						//保存此消息分发到哪些订阅队列，以供排查分析
						s.Set(this.MessageDispatchPrefix + header.MessageGuid, this.Encoding.GetBytes(string.Join(",", queueNames)));
					});
				}
			}
			return AOPResult.Success();
		}

		public EventMessageHeader PopFromMainQueue() {//从主队列获取消息
			return this.PopMessage(this.PublishQueueName, backup: true);
		}

		private void SetSubscribeAlive(IEventStorageExecutor executor, string queueName) {
			executor.Set(this.SubscriberRefKeyPrefix + queueName, BitConverter.GetBytes(DateTime.Now.Ticks));//置订阅队列为当前时间，以表示此订阅队列还在活动（避免被清除）			
		}

		public EventMessageHeader PopFromSubscribeQueue(string queueName) {//从订阅队列获取消息
			using(var storage = this.GetStorage(this.MainStorageName)) {
				var msg = this.PopMessage(storage, this.SubscriberQueuePrefix + queueName, true);
				if(msg != null) {
					storage.Pipeline(s => {
						SetSubscribeAlive(s, queueName);
						s.Decrement(this.MessageRefPrefix + msg.MessageGuid);//减少消息的引用数
					});
				} else {
					SetSubscribeAlive(storage, queueName);
				}
				return msg;
			}
		}

		public EventMessageHeader[] PopMessagesFromSubscribeQueue(string queueName, int count) {//从订阅队列获取多个消息
			using(var storage = this.GetStorage(this.MainStorageName)) {
				var queueKey = queueName;
				var result = storage.Pipeline(s => {
					SetSubscribeAlive(s, queueKey);
					queueKey = this.SubscriberQueuePrefix + queueKey;
					s.GetLength(queueKey);
				});
				if (result == null) {
					return null;
				}
				var objects = result.ToArray();
				if(objects.Length < 2) {
					return null;
				}
				var length = (int)objects[1];
				if(length <= 0) {//没有新消息？
					return null;
				}
				queueName = queueKey;
				if(length < 10 || count < 10) {
					return this.PopMessagesFromSubscribeQueue(storage, queueName, count);
				}
				if(count > length) {
					count = length;
				}
				return this.PopMessagesFromSubscribeQueueBatch(storage, queueName, count);
			}
		}

		public int GetSubscribeMessageCount(params string[] queueNames) {
			if(queueNames == null || queueNames.Length <= 0) {
				return -1;
			}
			using(var storage = this.GetStorage(this.MainStorageName)) {
				var count = storage.Pipeline(s => {
					foreach(var queueName in queueNames) {
						s.GetLength(this.SubscriberQueuePrefix + queueName);
					}
				}).Sum(x => (int)x);
				//设置订阅器的最后时间
				storage.Pipeline(s => {
					foreach (var queueName in queueNames) {
						this.SetSubscribeAlive(s, queueName);
					}
				});
				return count;
			}
		}

		protected EventMessageHeader[] PopMessagesFromSubscribeQueue(IEventStorage storage, string queueName, int count) {
			var list = new List<EventMessageHeader>();
			while(--count >= 0) {
				var msg = this.PopMessage(storage, queueName, true);
				if(msg == null) {
					break;
				}
				list.Add(msg);
			}
			storage.Pipeline(s => list.ForEach(m => s.Decrement(this.MessageRefPrefix + m.MessageGuid)));
			return list.ToArray();
		}

		protected EventMessageHeader[] PopMessagesFromSubscribeQueueBatch(IEventStorage storage, string queueName, int count) {
			var length = count;
			var result = storage.Pipeline(s => {
				while(--length >= 0) {
					s.RightPop(queueName);
				}
			});
			var keys = new List<string>();
			var headerKeys = new List<string>();
			var bodyKeys = new List<string>();
			foreach(var bytes in result) {
				if(!(bytes is byte[])) {
					continue;
				}
				var msgKey = (byte[])bytes;
				var key = msgKey.ToGuidString();
				keys.Add(key);
				headerKeys.Add(this.MessageHeaderPrefix + key);
				bodyKeys.Add(this.MessagePrefix + key);
			}
			if(keys.Count <= 0) {
				return null;
			}
			storage.Pipeline(s => keys.ForEach(k => s.Decrement(this.MessageRefPrefix + k)));

			result = storage.Pipeline(s => {
				s.MultiGet(headerKeys.ToArray());
				s.MultiGet(bodyKeys.ToArray());
			});
			storage.Dispose();

			if (result == null) {
				return null;
			}
			var objects = result.ToArray();
			if(objects.Length < 2) {
				return null;
			}

			var headerBytes = (byte[][])objects[0];
			var bodyBytes = (byte[][])objects[1];
			if(headerBytes == null || headerBytes.Length <= 0) {
				return null;
			}
			if(bodyBytes == null || bodyBytes.Length != headerBytes.Length) {
				return null;
			}

			var list = new List<EventMessageHeader>();
			for(var i = 0; i < headerBytes.Length; i++) {
				var bytes = headerBytes[i];
				var header = this.Serializer.Deserialize<EventMessageHeader>(bytes, null);
				if(header == null) {
					continue;
				}
				bytes = bodyBytes[i];
				var body = this.Serializer.Deserialize<EventMessage>(bytes, null);
				if(body.MessageGuid != header.MessageGuid) {
					continue;
				}
				header.Message = body;
				list.Add(header);
			}
			return list.ToArray();
		}

		public IAOPResult RegisterSubscribers(EventMessage[] messages) {//注册订阅器
			Dictionary<string, byte[]> values = null;
			if(messages != null && messages.Length > 0) {
				values = new Dictionary<string, byte[]>();//组装成Redis可批量操作的格式
				foreach(var message in messages) {
					message.MessageGuid = Guid.NewGuid().ToString("N");
					message.MessageType = MessageFlags.Subscriber;
					if (message.Category == null) {
						message.Category = string.Empty;
					}
					if (message.EventName == null) {
						message.EventName = string.Empty;
					}
					values.Add(this.SubscriberKeyPrefix + message.ChannelID, this.Serializer.Serialize(message, null));
				}
			}
			using(var storage = this.GetStorage(this.MainStorageName)) {
				storage.PersistToDatabase(messages);//持久化后备查
				if(values != null && values.Count > 0) {
					storage.Pipeline(s => {
						s.MultiSet(values);//保存订阅器
						s.Publish(this.NotifyQueueName, SubscribersChangedBytes);//发出订阅器列表变更的消息
					});
				} else {
					storage.Publish(this.NotifyQueueName, SubscribersChangedBytes);
				}
			}
			return AOPResult.Success();
		}

		protected EventMessage[] GetSubscribersInternal(IEventStorage storage) {
			var keys = storage.GetKeys(this.SubscriberKeyPrefix + "*");
			if(keys == null || keys.Length <= 0) {
				return null;
			}
			var result = storage.MultiGet(keys);
			if(result == null || result.Length <= 0) {
				return null;
			}
			var list = new List<EventMessage>(result.Length);
			foreach(var bytes in result) {
				if(bytes != null && bytes.Length > 0) {
					var message = this.Serializer.Deserialize<EventMessage>(bytes, null);
					if(message != null) {
						list.Add(message);
					}
				}
			}
			return list.ToArray();
		}

		public EventMessage[] GetSubscribers() {
			using(var storage = this.GetStorage(this.ReadOnlyStorageName)) {
				return this.GetSubscribersInternal(storage);
			}
		}

		private readonly Dictionary<IEventDispatcher, IEventStorage> dispatchers = new Dictionary<IEventDispatcher, IEventStorage>();
		public IAOPResult RegisterDispatcher(IEventDispatcher dispatcher) {//注册消息分发器
			if(!this.dispatchers.ContainsKey(dispatcher)) {
				lock(this.dispatchers) {
					if(!this.dispatchers.ContainsKey(dispatcher)) {
						var storage = this.GetStorage(this.MainStorageName);
						storage.Subscribe(this.NotifyQueueName, r => dispatcher.OnMessageReceived(this, r));
						dispatchers.Add(dispatcher, storage);
					}
				}
			}
			return AOPResult.Success();
		}

		//清理已处理的消息
		public int Clear(int count) {
			if(count <= 0) {
				return count;
			}
			using(var storage = this.GetStorage(this.MainStorageName)) {
				var queueName = this.PublishQueueName + this.BackupQueueSuffix;//备用List
				var length = storage.GetLength(queueName);//查看队列长度
				if(length <= 0) {
					return 0;
				}
				if(length < count) {
					count = length;
				}
				length = count;
				//批量获取length个消息Key
				var result = storage.Pipeline(s => {
					while(--length >= 0) {
						s.RightPop(queueName);
					}
				});
				if (result == null) {
					return 0;
				}
				var objects = result.ToArray();
				count = objects.Length;
				if(count <= 0) {
					return 0;
				}
				var keys = new List<string>();
				var refKeys = new List<string>();
				var remainKeys = new Dictionary<string, byte[]>();
				foreach(var bytes in objects) {
					if(bytes == null || !(bytes is byte[])) {
						continue;
					}
					var keyBytes = (byte[])bytes;
					var key = keyBytes.ToGuidString();
					remainKeys.Add(key, keyBytes);
					keys.Add(key);
					refKeys.Add(this.MessageRefPrefix + key);
				}
				if(keys.Count <= 0) {
					return 0;
				}
				var refCounts = storage.MultiGet(refKeys.ToArray());
				var removingKeys = new List<string>();
				var removingMessageKeys = new List<string>();
				if(refCounts != null && refCounts.Length > 0) {
					for(var i = 0; i < refCounts.Length; i++) {
						var bytes = refCounts[i];
						if(bytes != null && bytes.Length > 0) {//有引用值
							int refCount;
							if(int.TryParse(Encoding.ASCII.GetString(bytes), out refCount) && refCount > 0) {
								continue;
							}
						}//无引用值，可以删除
						var key = keys[i];
						removingMessageKeys.Add(this.MessagePrefix + key);
						removingKeys.Add(this.MessagePrefix + key);//消息主体
						removingKeys.Add(this.MessageHeaderPrefix + key);//消息头
						removingKeys.Add(this.MessageRefPrefix + key);//消息引用数
						removingKeys.Add(this.MessageDispatchPrefix + key);//消息被分发的通道
						remainKeys.Remove(key);
					}
				}
				//即将被删除的消息转移到数据库备查
				if(removingMessageKeys.Count > 0 && this.Setting.Persist) {
					this.PersistMessages(storage, removingMessageKeys.ToArray());
				}
				//删除未被引用的消息
				count = removingKeys.Count;
				if(count > 0) {
					storage.Delete(removingKeys.ToArray());
				}
				//把还有引用关系的消息重新入列
				if(remainKeys.Count > 0) {
					storage.Pipeline(s => {
						foreach(var value in remainKeys.Values) {
							s.LeftPush(queueName, value);
						}
					});
				}
				return count;
			}
		}

		protected void PersistMessages(IEventStorage storage, string[] keys) {//消息持久化（备查）
			if(keys == null || keys.Length <= 0) {
				return;
			}
			var result = storage.MultiGet(keys);
			if(result == null || result.Length <= 0) {
				return;
			}
			var list = new List<EventMessage>();
			foreach(var bytes in result) {
				if(bytes == null || bytes.Length <= 0) {
					continue;
				}
				var message = this.Serializer.Deserialize<EventMessage>(bytes, null);
				if(message != null) {
					//把被分发到的订阅者也一并保存
					var dsptBytes = storage.Get(this.MessageDispatchPrefix + message.MessageGuid);
					string channels = null;
					if(dsptBytes != null && dsptBytes.Length > 0) {
						channels = this.Encoding.GetString(dsptBytes);
					}
					if (!string.IsNullOrEmpty(channels)) {
						message.Data["__Channels__"] = channels;
					}
					list.Add(message);
				}
			}
			if (list.Count > 0) {
				storage.PersistToDatabase(list.ToArray());
			}
		}

		//移除不活跃的订阅（默认24小时前）
		public void RemoveUnavailableSubscribers() {
			using(var storage = this.GetStorage(this.MainStorageName)) {
				var subscribers = this.GetSubscribersInternal(storage);
				if(subscribers == null || subscribers.Length <= 0) {
					return;
				}
				foreach(var subscriber in subscribers) {
					this.RemoveSubscriber(storage, subscriber);
				}
			}
		}

		//移除不活跃的订阅（默认24小时前）
		protected void RemoveSubscriber(IEventStorage storage, EventMessage subscriber) {
			//判断队列是否有消息
			var queueName = subscriber.ChannelID;
			var qkey = this.SubscriberQueuePrefix + queueName;
			var qlength = storage.GetLength(qkey);
			if(qlength <= 0) {//没有任何消息，忽略（实际上此队列在Redis中是不存在的）
				return;
			}
			//24小时前
			var now = DateTime.Now;
			var minTicks = subscriber.Priority > 0 ? now.AddSeconds(-subscriber.Priority).Ticks : now.AddHours(-24).Ticks;
			//获取最后一次活跃时间
			var qfkey = this.SubscriberRefKeyPrefix + queueName;
			var bytes = storage.Get(qfkey);
			var remove = true;
			if(bytes != null && bytes.Length > 0) {
				var ticks = BitConverter.ToInt64(bytes, 0);
				if(ticks > minTicks) {//还是活跃的
					remove = false;
				}
			}
			if(!remove) {//此订阅还是活跃的，忽略
				return;
			}
			//把订阅信息移除
			var sk = this.SubscriberKeyPrefix + queueName;
			storage.Delete(sk);
			//把队列里的消息引用数-1
			var mlist = new List<string>();
			do {
				bytes = storage.RightPop(qkey);
				if(bytes != null && bytes.Length > 0) {
					var messageId = bytes.ToGuidString();
					mlist.Add(this.MessageRefPrefix + messageId);
				}
			} while(bytes != null);
			//把订阅队列移除
			storage.Pipeline(s => {
				foreach(var msgid in mlist) {
					s.Decrement(msgid);
				}
				s.Delete(qkey);
			});
		}

		protected virtual T InvokeMethodSafety<T>(Func<T> method, bool throwOnError = false) {
			try {
				return method();
			} catch(Exception e) {
				ExceptionService.Publish(e);
				if(throwOnError) {
					throw;
				}
			}
			return default(T);
		}

		protected virtual void InvokeMethodSafety(Action method, bool throwOnError = false) {
			try {
				method();
			} catch(Exception e) {
				ExceptionService.Publish(e);
				if(throwOnError) {
					throw;
				}
			}
		}

		protected virtual IAOPResult InvokeMethodSafety(Func<IAOPResult> method) {
			try {
				return this.InvokeMethodSafety(method, true);
			} catch(Exception e) {
				return AOPResult.Failed(e.Message);
			}
		}

		#region IEventRepository Members

		IAOPResult IEventRepository.PushToMainQueue(EventMessageHeader message) {
			return this.InvokeMethodSafety(() => this.PushToMainQueue(message));
		}

		IAOPResult IEventRepository.PushToSubscribeQueue(string queueName, EventMessageHeader message) {
			return this.InvokeMethodSafety(() => this.PushToSubscribeQueue(queueName, message));
		}

		IAOPResult IEventRepository.PushToSubscribeQueues(EventMessageHeader message, string[] queueNames) {
			return this.InvokeMethodSafety(() => this.PushToSubscribeQueues(message, queueNames));
		}

		EventMessageHeader IEventRepository.PopFromMainQueue() {
			return this.InvokeMethodSafety(() => this.PopFromMainQueue());
		}

		EventMessageHeader IEventRepository.PopFromSubscribeQueue(string queueName) {
			return this.InvokeMethodSafety(() => this.PopFromSubscribeQueue(queueName));
		}

		EventMessageHeader[] IEventRepository.PopMessagesFromSubscribeQueue(string queueName, int count) {
			return this.InvokeMethodSafety(() => this.PopMessagesFromSubscribeQueue(queueName, count));
		}

		int IEventRepository.GetSubscribeMessageCount(params string[] queueNames) {
			return this.InvokeMethodSafety(() => this.GetSubscribeMessageCount(queueNames));
		}

		IAOPResult IEventRepository.RegisterSubscribers(EventMessage[] messages) {
			return this.InvokeMethodSafety(() => this.RegisterSubscribers(messages));
		}

		EventMessage[] IEventRepository.GetSubscribers() {
			return this.InvokeMethodSafety(() => this.GetSubscribers());
		}

		IAOPResult IEventRepository.RegisterDispatcher(IEventDispatcher dispatcher) {
			return this.InvokeMethodSafety(() => this.RegisterDispatcher(dispatcher));
		}

		int IEventRepository.Clear(int count) {
			return this.InvokeMethodSafety(() => this.Clear(count));
		}

		void IEventRepository.RemoveUnavailableSubscribers() {
			this.InvokeMethodSafety(() => this.RemoveUnavailableSubscribers());
		}

		#endregion
	}
}

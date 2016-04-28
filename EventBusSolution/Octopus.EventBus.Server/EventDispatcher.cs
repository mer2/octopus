using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HTB.DevFx.Utils;

namespace Octopus.EventBus.Server
{
	public class EventDispatcher : TimerBase, IEventDispatcher
	{
		private readonly Dictionary<string, EventMessage> subscribers = new Dictionary<string, EventMessage>();
		protected void InitSubscribers(IEventRepository repository) {//初始化订阅器
			var subs = repository.GetSubscribers();
			lock(this.subscribers) {
				if(subs == null || subs.Length <= 0) {
					this.subscribers.Clear();
					return;
				}
				var list = new List<EventMessage>();
				foreach (var subscriber in subs) {
					if (string.IsNullOrEmpty(subscriber.Category) || string.IsNullOrEmpty(subscriber.ChannelID)) {
						continue;
					}
					this.subscribers[subscriber.ChannelID] = subscriber;
					list.Add(subscriber);
				}
				if(list.Count > 0) {
					this.Callee.SendMessage(repository, list.ToArray());//主动发送消息给调用方？
				}
			}
		}

		//分发一条消息到各个订阅队列
		protected void DispatchMessage(IEventRepository repository, List<EventMessage> list, EventMessageHeader message) {
			var subs = EventClientService.GetDispatchedSubscribers(message.Message, list);//消息被分发到的订阅队列
			if (subs.Count > 0) {
				var names = subs.Select(x => x.ChannelID).ToArray();
				repository.PushToSubscribeQueues(message, names);//分发到订阅队列
				foreach (var subscriber in subs) {
					var sub = subscriber;
					Task.Factory.StartNew(() => this.Callee.SendMessage(repository, sub));//异步主动推送消息给调用方
				}
			}
		}

		protected override void OnTimer() {//定时获取消息，然后分发
			var repository = this.Repository;
			this.OnMessageReceived(repository, EventRepositoryBase.SubscribersChangedBytes);
			repository.RegisterDispatcher(this);//为以防意外，每次都要在下层的存储中注册自己，以期利用Redis的订阅模式快速处理消息
			this.StartTimer();
		}

		private IEventCallee Callee;
		private IEventRepository Repository;
		public void Init(IEventRepository repository, double interval) {
			this.Repository = repository;
			this.Callee = new EventCallee();
			this.Interval = interval;
			this.StartTimer();
		}

		public void OnMessageReceived(IEventRepository repository, object state) {//接收到新消息需要分发
			var bytes = state as byte[];
			if (bytes != null && bytes.Length > 0) {
				var msg = Encoding.ASCII.GetString(bytes);
				if (msg == EventRepositoryBase.SubscribersChangedName) {//这是订阅器注册的消息
					this.InitSubscribers(repository);
				}
			}
			if (this.subscribers.Count <= 0) {//没有订阅器？
				return;
			}
			List<EventMessage> list;
			lock (this.subscribers) {
				list = new List<EventMessage>(this.subscribers.Count);
				list.AddRange(this.subscribers.Values);
			}
			do {
				var message = repository.PopFromMainQueue();//从主队列获取消息
				if(message == null) {
					break;
				}
				this.DispatchMessage(repository, list, message);//分发
			} while(true);
		}
	}
}

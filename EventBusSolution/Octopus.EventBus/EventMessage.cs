using System;
using System.Collections;

namespace Octopus.EventBus
{
	/// <summary>
	/// 事件消息
	/// </summary>
	[Serializable]
	public class EventMessage
	{
		/// <summary>
		/// 消息唯一标识（系统内部使用）
		/// </summary>
		public string MessageGuid { get; set; }
		/// <summary>
		/// 消息分类
		/// </summary>
		public string Category { get; set; }
		/// <summary>
		/// 消息事件名
		/// </summary>
		public string EventName { get; set; }
		/// <summary>
		/// 消息优先级（在订阅器注册里复用为订阅器过期秒数）
		/// </summary>
		public int Priority { get; set; }
		/// <summary>
		/// 消息类型
		/// </summary>
		public MessageFlags MessageType { get; set; }
		/// <summary>
		/// 消息自定义ID
		/// </summary>
		public string MessageID { get; set; }
		/// <summary>
		/// 消息通道ID（系统内部使用）
		/// </summary>
		public string ChannelID { get; set; }
		/// <summary>
		/// 消息标签
		/// </summary>
		public string[] Tags { get; set; }
		/// <summary>
		/// 创建时间
		/// </summary>
		public DateTime CreatedTime { get; set; }
		/// <summary>
		/// 消息体
		/// </summary>
		public object Message { get; set; }

		private IDictionary data;
		/// <summary>
		/// 消息其他数据
		/// </summary>
		public IDictionary Data {
			get { return this.data ?? (this.data = new Hashtable()); }
			set { this.data = value; }
		}

		public override string ToString() {
			return string.Format("id={0};c={1};e={2}", this.MessageGuid, this.Category, this.EventName);
		}
	}
}

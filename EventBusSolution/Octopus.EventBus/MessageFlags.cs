using System;

namespace Octopus.EventBus
{
	[Flags]
	public enum MessageFlags
	{
		/// <summary>
		/// 缺省的消息
		/// </summary>
		Default = 0,

		/// <summary>
		/// 这是一个应答消息
		/// </summary>
		Ack = 1,

		/// <summary>
		/// 消息需要被处理（才能丢弃）
		/// </summary>
		Handle = 2,

		/// <summary>
		/// 这是一个直链消息（直接到达指定的通道）
		/// </summary>
		Direct = 4,

		/// <summary>
		/// 这是订阅通道注册信息（内部使用）
		/// </summary>
		Subscriber = 8,
	}
}

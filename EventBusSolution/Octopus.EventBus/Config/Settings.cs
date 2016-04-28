using HTB.DevFx.Config;
[assembly: ConfigResource("res://Octopus.EventBus.Config.Settings.config")]

namespace Octopus.EventBus.Config
{
	public class EventClientServiceSetting : ConfigSettingElement
	{
		protected override void OnConfigSettingChanged() {
			base.OnConfigSettingChanged();
			this.Debug = this.GetSetting("debug", false);
			this.PublishToServer = this.GetSetting("publishToServer", true);
			this.PublishToLocal = this.GetSetting("publishToLocal", true);
			this.Interval = this.GetSetting("interval", 1000D);
			this.PublisherSetting = this.GetTypedSetting<PublisherContext>("publishers");
			this.SubscriberSetting = this.GetTypedSetting<SubscriberContext>("subscribers");
		}

		/// <summary>
		/// 是否发布到中央服务器处理
		/// </summary>
		public bool PublishToServer { get; private set; }
		/// <summary>
		/// 是否通知本地消费者
		/// </summary>
		public bool PublishToLocal { get; private set; }
		public bool Debug { get; private set; }
		/// <summary>
		/// 监控器时间间隔
		/// </summary>
		public double Interval { get; private set; }
		public PublisherContext PublisherSetting { get; private set; }
		public SubscriberContext SubscriberSetting { get; private set; }
	}

	public class PublisherContext : ConfigSettingElement
	{
		protected override void OnConfigSettingChanged() {
			base.OnConfigSettingChanged();
			this.Interval = this.GetSetting("interval", 100D);
			this.MaxBufferSize = this.GetSetting("maxBufferSize", 1000);
			this.DefaultPublisher = this.GetSetting("defaultPublisher");
			this.Publishers = this.GetSettings<PublisherSetting>(null).ToArray();
		}

		/// <summary>
		/// 发送器时间间隔
		/// </summary>
		public double Interval { get; private set; }
		/// <summary>
		/// 最大本地缓存消息数
		/// </summary>
		public int MaxBufferSize { get; private set; }
		public string DefaultPublisher { get; private set; }
		public PublisherSetting[] Publishers { get; private set; }
	}

	public class PublisherSetting : ConfigSettingElement
	{
		protected override void OnConfigSettingChanged() {
			base.OnConfigSettingChanged();
			this.Name = this.GetSetting("name");
			this.Category = this.GetSetting("category");
			this.EventName = this.GetSetting("eventName");
			this.Priority = this.GetSetting("priority", 0);
			var tags = this.GetSetting("tags");
			if(!string.IsNullOrEmpty(tags)) {
				this.Tags = tags.Split('\t');
			}
		}

		public string Name { get; private set; }
		public string Category { get; private set; }
		public string EventName { get; private set; }
		public int Priority { get; private set; }
		public string[] Tags { get; private set; }
	}

	public class SubscriberContext : ConfigSettingElement
	{
		protected override void OnConfigSettingChanged() {
			base.OnConfigSettingChanged();
			this.Interval = this.GetSetting("interval", 100D);
			this.IntervalEnabled = this.GetSetting("intervalEnabled", true);
			this.RegisterInterval = this.GetSetting("registerInterval", 60);//一小时
			this.SubscriberCallback = this.GetSetting("callback");
			this.Subscribers = this.GetSettings<SubscriberSetting>(null).ToArray();
		}

		/// <summary>
		/// 接收器时间间隔
		/// </summary>
		public double Interval { get; private set; }
		public bool IntervalEnabled { get; private set; }
		public int RegisterInterval { get; private set; }//再次到服务器上注册订阅器的时间间隔，单位分钟
		public string SubscriberCallback { get; private set; }
		public SubscriberSetting[] Subscribers { get; private set; }
	}

	public class SubscriberSetting : PublisherSetting
	{
		protected override void OnConfigSettingChanged() {
			base.OnConfigSettingChanged();
			this.Register = this.GetSetting("register", true);
			this.Title = this.GetSetting("title");
			this.HandlerName = this.GetSetting("type");
			this.Timeout = this.GetSetting("timeout", 0);
			this.IsLocal = this.GetSetting("isLocal", false);
		}

		public bool Register { get; private set; }
		public string Title { get; private set; }
		public string HandlerName { get; private set; }
		/// <summary>
		/// 订阅器在服务器上超时秒数（不活跃秒数超过后此订阅器被清除）
		/// </summary>
		public int Timeout { get; private set; }
		/// <summary>
		/// 是否为本地消息（同一宿主内自己产生内容自己消费）
		/// </summary>
		public bool IsLocal { get; private set; }
	}
}

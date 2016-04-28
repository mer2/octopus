using HTB.DevFx.Config;
[assembly: ConfigResource("res://Octopus.EventBus.Server.Config.Settings.config")]

namespace Octopus.EventBus.Server.Config
{
	public class EventServiceSetting : ConfigSettingElement
	{
		protected override void OnConfigSettingChanged() {
			base.OnConfigSettingChanged();
			this.DispatchInterval = this.GetSetting("dispatchInterval", 100D);
			this.CleanInterval = this.GetSetting("cleanInterval", 100D);
			this.CleanBatchSize = this.GetSetting("cleanBatchSize", 10);
		}

		/// <summary>
		/// 分发间隔（默认100毫秒）
		/// </summary>
		public double DispatchInterval { get; private set; }
		/// <summary>
		/// 清除无效消息的间隔，小于等于0表示无需清除（默认100毫秒）
		/// </summary>
		public double CleanInterval { get; private set; }
		/// <summary>
		/// 一次清除消息的个数（默认10个）
		/// </summary>
		public int CleanBatchSize { get; private set; }
	}

	public class EventRepositorySetting : ConfigSettingElement
	{
		protected override void OnConfigSettingChanged() {
			base.OnConfigSettingChanged();
			this.SerializerName = this.GetSetting("serializerName", "application/bson");
			this.EncodingName = this.GetSetting("encoding", "utf-8");
			this.Persist = this.GetSetting("persist", false);
			this.PublishQueueKey = this.GetSetting("queue", "51541b9a7c824eab8ab2e512b6f08203");
			this.MainStorageName = this.GetSetting("mainStorage", "EventStorage");
			this.ReadOnlyStorageName = this.GetSetting("readOnlyStorage", "EventStorage");
		}

		public string SerializerName { get; private set; }
		public string EncodingName { get; private set; }
		public bool Persist { get; private set; }//是否持久化到数据库备查
		public string PublishQueueKey { get; private set; }
		public string MainStorageName { get; private set; }
		public string ReadOnlyStorageName { get; private set; }
	}
}
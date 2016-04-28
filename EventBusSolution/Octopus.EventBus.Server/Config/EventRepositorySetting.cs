using HTB.DevFx.Config;

namespace Octopus.EventBus.Server.Config
{
	public class EventRepositorySetting : ConfigSettingElement
	{
		protected override void OnConfigSettingChanged() {
			base.OnConfigSettingChanged();
			this.PublishQueueKey = this.GetSetting("queue", "51541b9a7c824eab8ab2e512b6f08203");
			this.MainStorageName = this.GetSetting("mainStorage", "EventStorage");
			this.ReadOnlyStorageName = this.GetSetting("readOnlyStorage", "EventStorage");
		}

		public string PublishQueueKey { get; private set; }
		public string MainStorageName { get; private set; }
		public string ReadOnlyStorageName { get; private set; }
	}
}
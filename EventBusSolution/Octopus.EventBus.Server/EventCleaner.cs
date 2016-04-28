using HTB.DevFx.Utils;

namespace Octopus.EventBus.Server
{
	internal class EventCleaner : TimerBase
	{
		private IEventRepository Repository;
		private int BatchCount;

		protected override void OnTimer() {
			this.Repository.RemoveUnavailableSubscribers();//移除不活跃的订阅
			while (this.Repository.Clear(this.BatchCount) > 0) {}//移除已处理过的消息
			this.StartTimer();
		}

		public void Init(IEventRepository repository, int count, double interval) {
			this.Repository = repository;
			this.BatchCount = count;
			this.Interval = interval;
			this.StartTimer();
		}
	}
}

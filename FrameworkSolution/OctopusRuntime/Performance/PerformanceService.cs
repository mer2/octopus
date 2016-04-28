using System.Collections.Generic;
using HTB.DevFx;
using HTB.DevFx.Core;
using Octopus.Performance.Config;

namespace Octopus.Performance
{
	internal class PerformanceService : ServiceBase<PerformanceServiceSetting>
	{
		protected override void OnInit() {
			base.OnInit();
			var ms = new List<IMonitorable>();
			if(this.Setting.Monitors != null) {
				foreach (var mts in this.Setting.Monitors) {
					if(!string.IsNullOrEmpty(mts.TypeName)) {
						var m = this.ObjectService.GetOrCreateObject<IMonitorable>(mts.TypeName);
						if(m != null) {
							ms.Add(m);
						}
					}
				}
			}
			this.monitors = ms.ToArray();
		}

		private IMonitorable[] monitors;

		public IAOPResult GetPerformanceResult() {
			var results = new List<IAOPResult>();
			foreach (var monitor in this.monitors) {
				monitor.Monitor(results);
			}
			results.Add(AOPResult.Success());
			return results[0];
		}
	}
}

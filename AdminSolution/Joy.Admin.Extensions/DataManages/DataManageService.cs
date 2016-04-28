using System.Linq;
using System.Collections.Generic;
using HTB.DevFx.Core;
using Joy.Admin.Extensions.DataManages.Config;

namespace Joy.Admin.Extensions.DataManages
{
	internal class DataManageService : ServiceBase<DataManageServiceSetting>, IDataManageService
	{
		protected override void OnInit() {
			base.OnInit();
			this.DataItems = this.Setting.Items.ToDictionary(k => k.Name, v => new DataItem(this, v));
		}
		protected IDictionary<string, DataItem>  DataItems { get; private set; }
		public DataItem GetDataItem(string itemName) {
			DataItem dataItem;
			this.DataItems.TryGetValue(itemName, out dataItem);
			return dataItem;
		}
	}
}
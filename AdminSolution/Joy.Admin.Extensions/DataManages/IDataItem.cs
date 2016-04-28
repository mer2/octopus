using System.Collections;
using System.Collections.Generic;
using HTB.DevFx.Core;
using HTB.DevFx.Data.Entities;

namespace Joy.Admin.Extensions.DataManages
{
	public interface IDataItem
	{
		string Name { get; }
		string Title { get; }

		IPaginateResult GetList(IDictionary query, int startIndex, int length);
		IAOPResult Insert(IDictionary<string, string> data);
		IAOPResult Update(IDictionary<string, string> data);
		IAOPResult Delete(IDictionary<string, string> data);
		ItemColumn[] GetListColumns();
		ItemColumn[] GetQueryColumns();
		ItemColumn[] GetEditColumns();
		ItemColumn[] GetInsertColumns();
	}
}
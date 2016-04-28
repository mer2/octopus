using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Joy.Admin.Extensions.DataManages.Data.Server
{
	public interface IManageData
	{
	    DataTable GetData(string sql, Dictionary<string, string> options);

        int ExecuteSql(string sql, Dictionary<string, string> options);
	}
}

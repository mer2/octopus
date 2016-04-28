using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;
using HTB.DevFx.Data;
using Joy.Admin.Extensions.DataManages.Data.Server;

namespace Joy.Admin.Extensions.DataManages.Data.Data
{
	public class ManageData : IManageData
    {
        public DataTable GetData(string sql,Dictionary<string,string> options ) {

            return DataService.Execute<DataTable>("ManageSql", new { Sql = sql, Parameters = options });
		}

        public int ExecuteSql(string sql, Dictionary<string, string> options) {

            return DataService.Execute<int>("ManageSql", new { Sql = sql, Parameters = options });
        }
    }
}

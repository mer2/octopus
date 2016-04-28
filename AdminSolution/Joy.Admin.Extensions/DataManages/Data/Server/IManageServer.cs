using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Data;
using Joy.Admin.Extensions.DataManages.Data.Model;

namespace Joy.Admin.Extensions.DataManages.Data.Server
{
    public interface IManageServer
    {
        string TableName { get; set; }
        string KeyField { get;}

        DataTable GetDataList(Dictionary<string, string> search, Pager pager);

        bool InsertData(Dictionary<string, string> options);

        bool UpdateData(Dictionary<string, string> options);

        ManageColumn[] GetListColumns();

        ManageColumn[] GetSearchColumns();

        ManageColumn[] GetEditColumns();

        ManageColumn[] GetAddColumns();
    }
}

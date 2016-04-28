using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using HTB.DevFx.Config;
using HTB.DevFx.Core;
using HTB.DevFx.Data;
using Joy.Admin.Extensions.DataManages.Config;
using Joy.Admin.Extensions.DataManages.Data.Model;
using RazorEngine;

namespace Joy.Admin.Extensions.DataManages.Data.Server
{
	internal class ManageServer : ServiceBase<CreateEntitiesSetting>, IManageServer
	{
		private string ConnectionString { get; set; }

		private ManageEntitySetting ManageEntity {
			get { return this.Setting.ManageEntities.Single(m => m.Name == TableName); }
		}

		protected override void OnInit() {
		    base.OnInit();
			this.ConnectionString = this.Setting.ConnectionString;
		}

		public string TableName { get; set; }

	    public string KeyField {
            get { return ManageEntity.Columns.Single(c => c.IsPrimaryKey).Name; }
	    }

	    private string GetListSql(Dictionary<string, string> search,Pager pager,out Dictionary<string,string>param ) {
		    string strSql = string.Empty;
	        string whereSql = string.Empty;
	        string declareSql =  string.Empty;

            whereSql = GetWhereSql(search, out param);
		    var sortSql = this.GetSortSql();
		    if (ManageEntity.NeedPager && pager!=null) {
		        strSql = string.Format("{0} select * from(select *,row_number()over({1}) as RowIndex from {2} {3}) t where t.RowIndex between {4} and {5} order by t.RowIndex", 
                    declareSql,sortSql, TableName, whereSql,pager.StartIndex,pager.EndIndex);
		    } else {
                strSql =string.Format("{0} select * from {1} {2}",declareSql, TableName,whereSql);
            }

		    return strSql;
		}
        private string GetSortSql() {
            string sortSql = string.Empty;
            
            var sorts = ManageEntity.Columns.Where(c => !string.IsNullOrEmpty(c.Sort)).Select(c => c.Name + " " + c.Sort);
            sortSql = string.Join(",", sorts);
            if (sortSql.Length > 0) {
                sortSql = "order by " + sortSql;
            }

            return sortSql;
        }
        private string GetWhereSql(Dictionary<string, string> search, out Dictionary<string, string> param) {
            param = new Dictionary<string, string>();
            StringBuilder whereSql = new StringBuilder();

            if (search != null) {
                var searchColumns = from mc in ManageEntity.Columns join s in search on mc.Name equals s.Key select new {mc.Name, mc.SearchType, s.Value};
                foreach (var column in searchColumns) {
                    var value = column.Value.Replace("'", "''");
                    param.Add(column.Name, value);

                    whereSql.Append("and ");
                    switch (column.SearchType) {
                        case SearchTypes.Equals:
                            whereSql.AppendFormat(" {0}=@{0} ", column.Name);
                            break;
                        case SearchTypes.Like:
                            whereSql.AppendFormat(" {0} like '%'+@{0}+'%' ", column.Name);
                            break;
                        case SearchTypes.GreaterThanOrEquals:
                            whereSql.AppendFormat(" {0}>=@{0} ", column.Name);
                            break;
                        case SearchTypes.GreaterThan:
                            whereSql.AppendFormat(" {0}>@{0} ", column.Name);
                            break;
                        case SearchTypes.LessThanOrEquals:
                            whereSql.AppendFormat(" {0}<=@{0} ", column.Name);
                            break;
                        case SearchTypes.LessThan:
                            whereSql.AppendFormat(" {0}<@{0} ", column.Name);
                            break;
                        default:
                            whereSql.AppendFormat(" {0}=@{0} ", column.Name);
                            break;
                    }
                }
                if (whereSql.Length > 0) {
                    whereSql.Remove(0, 4).Insert(0, "where ");
                }
            }

            return whereSql.ToString();
        }
        private string GetInsertSql(Dictionary<string, string> options, out Dictionary<string, string> param) {
            param = new Dictionary<string, string>();
	        string strSql = string.Empty;

	        StringBuilder start = new StringBuilder();
            StringBuilder end = new StringBuilder();
	        foreach (var option in options) {
	            var field = option.Key;
	            var value = option.Value;
	            param.Add(field, value);

	            start.AppendFormat(",{0}", field);
                end.AppendFormat(",@{0}", field);
	        }
	        if (start.Length > 0) {
	            start.Remove(0, 1);
	            end.Remove(0, 1);

	            strSql = string.Format("insert into {0}({1})values({2})",TableName, start.ToString(), end.ToString());
	        }

	        return strSql;
	    }
        private string GetUpdateSql(Dictionary<string, string> options,out Dictionary<string,string> param ) {
            param = new Dictionary<string, string>();
            string strSql = string.Empty;

            string whereSql = string.Empty;
            StringBuilder updateSql = new StringBuilder();
            foreach(var option in options) {
                var field = option.Key;
                var value = option.Value.Replace("'", "''");
                param.Add(field, value);

                if (field != KeyField) {
                    updateSql.AppendFormat(",{0}=@{0}", field);
                } else {
                    whereSql = string.Format(" where {0}=@{0}", field);
                }
            }
            if(updateSql.Length > 0) {
                //declareSql.Remove(0, 1).Insert(0, "declare ").Append(";");
                updateSql.Remove(0, 1);

                strSql = string.Format("update {0} set {1} {2}", TableName, updateSql, whereSql);
            }

            return strSql;
        }

	    public ManageColumn[] GetSearchColumns() {
            var columns = ManageEntity.Columns.Where(c =>c.ControlType!= Model.ControlTypes.None)
                .Select(c => c.ToManageColumn()).ToArray();

            return columns;
        }

        public ManageColumn[] GetListColumns() {
            var columns = ManageEntity.Columns.Where(c => !string.IsNullOrEmpty(c.Label)).ToArray()
                .Select(c => c.ToManageColumn()).ToArray();

            return columns;
        }

        public ManageColumn[] GetEditColumns() {
            var columns = ManageEntity.Columns.Where(c => c.CanEdit||c.IsPrimaryKey)
                .Select(c => c.ToManageColumn()).ToArray();

            return columns;
        }

        public ManageColumn[] GetAddColumns() {
            var columns = ManageEntity.Columns.Where(c => c.CanAdd)
               .Select(c => c.ToManageColumn()).ToArray();

            return columns;
        }

		public DataTable GetDataList(Dictionary<string, string> search, Pager pager) {
            Dictionary<string, string> param;
			string sql = this.GetListSql(search,pager,out param);

			var svr = ObjectService.GetObject<IManageData>();
            return svr.GetData(sql, param);
		}

	    public bool InsertData(Dictionary<string, string> options) {
            Dictionary<string, string> param;
	        string sql = this.GetInsertSql(options,out param);

            var svr = ObjectService.GetObject<IManageData>();
            var result= svr.ExecuteSql(sql, param);

	        return result > 0;
	    }

	    public bool UpdateData(Dictionary<string, string> options) {
	        Dictionary<string, string> param;
            string sql = this.GetUpdateSql(options,out param);

            var svr = ObjectService.GetObject<IManageData>();
            var result= svr.ExecuteSql(sql,param);

	        return result > 0;
	    }
	}
}

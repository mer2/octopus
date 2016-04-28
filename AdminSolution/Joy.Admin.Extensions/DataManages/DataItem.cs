using System.Linq;
using System.Collections;
using System.Collections.Generic;
using HTB.DevFx.Core;
using HTB.DevFx.Data;
using HTB.DevFx.Data.Entities;
using Joy.Admin.Extensions.DataManages.Config;

namespace Joy.Admin.Extensions.DataManages
{
	public class DataItem
	{
		internal DataItem(DataManageService dataManage, DataItemSetting setting) {
			this.DataManageService = dataManage;
			this.Setting = setting;
			this.Name = setting.Name;
			this.Title = setting.Title;
			this.Columns = this.Setting.Columns.Select(x => new ItemColumn(x)).ToArray();
			var orderBy = this.Setting.Columns.Where(x => x.Sort != SortTypes.None).ToDictionary(k => k.Name, v => v.Sort.ToString());
			if (orderBy.Count <= 0) {//默认使用主键排序
				orderBy = this.Setting.Columns.Where(x => x.IsPrimaryKey).ToDictionary(k => k.Name, v => "DESC");
			}
			this.DefaultOrderBy = orderBy;
			this.ListColumns = this.Columns.Where(x => x.Listable).ToArray();
			this.EditColumns = this.Columns.Where(x => x.Editable).ToArray();
			this.QueryColumns = this.Columns.Where(x => x.Queryable).ToArray();
		}
		internal DataManageService DataManageService { get; private set; }
		internal DataItemSetting Setting { get; private set; }
		protected ItemColumn[] Columns { get; private set; }//所有的列定义
		protected ItemColumn[] ListColumns { get; private set; }//列表的列定义
		protected ItemColumn[] EditColumns { get; private set; }//编辑模式的列定义
		protected ItemColumn[] QueryColumns { get; private set; }//查询模式的列定义
		protected IDictionary DefaultOrderBy { get; private set; }
		public string Name { get; private set; }
		public string Title { get; private set; }

		public IPaginateResult GetList(IDictionary query, int startIndex, int length) {
			//构建查询操作
			var queryableItems = new List<QueryableItem>();
			var keys = new object[query.Count];
			query.Keys.CopyTo(keys, 0);
			foreach(string key in keys) {
				var item = new QueryableItem { Name = key, Value = query[key] };
				var column = this.QueryColumns.FirstOrDefault(x => x.Name == key);
				if (column != null) {
					item.QueryOperator = column.QueryOperator;
					queryableItems.Add(item);
				} else {
					query.Remove(key);
				}
			}
			return DataService.Execute<IPaginateResult>("DataManages.Query", new {
				this.Setting.TableName, StartIndex = startIndex, Length = length, Query = queryableItems.ToArray(), Parameters = query, OrderBy = this.DefaultOrderBy
			});
		}

		public IAOPResult Insert(IDictionary<string, string> data) {
			throw new System.NotImplementedException();
		}

		public IAOPResult Update(IDictionary<string, string> data) {
			throw new System.NotImplementedException();
		}

		public IAOPResult Delete(IDictionary<string, string> data) {
			throw new System.NotImplementedException();
		}

		public ItemColumn[] GetListColumns() {
			return this.ListColumns;
		}

		public ItemColumn[] GetQueryColumns() {
			return this.QueryColumns;
		}

		public ItemColumn[] GetEditColumns() {
			return this.EditColumns;
		}

		public ItemColumn[] GetInsertColumns() {
			//TODO：？
			return this.EditColumns;
		}
	}
}

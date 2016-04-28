using System;
using Joy.Admin.Extensions.DataManages.Config;

namespace Joy.Admin.Extensions.DataManages
{
	[Serializable]
	public class ItemColumn
	{
		internal ItemColumn(ItemColumnSetting setting) {
			this.Title = setting.Title;
			this.Name = setting.Name;
			this.IsPrimaryKey = setting.IsPrimaryKey;
			this.EditRenderType = setting.EditRenderType;
			this.QueryRenderType = setting.QueryRenderType;
			this.Queryable = setting.Queryable;
			this.Sort = setting.Sort;
			this.Editable = setting.Editable;
			this.QueryOperator = setting.QueryOperator;
			this.ValueFormat = setting.ValueFormat;
			this.Listable = setting.Listable;
		}
		/// <summary>
		/// 显示的名称（标题）
		/// </summary>
		public string Title { get; set; }
		/// <summary>
		/// 字段名（可选）
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// 显示的格式（NamedFormat形式，如{CreateTime:yyyyMMdd}
		/// </summary>
		public string ValueFormat { get; set; }
		/// <summary>
		/// 是否是主键
		/// </summary>
		public bool IsPrimaryKey { get; set; }
		/// <summary>
		/// 可显示在列表中
		/// </summary>
		public bool Listable { get; set; }
		public RenderTypes EditRenderType { get; set; }
		public RenderTypes QueryRenderType { get; set; }
		/// <summary>
		/// 可被查询（显示在查询中）
		/// </summary>
		public bool Queryable { get; set; }
		public OperatorTypes QueryOperator { get; set; }
		/// <summary>
		/// 是否可排序，排序方向
		/// </summary>
		public SortTypes Sort { get; set; }
		/// <summary>
		/// 可被编辑（显示在更新界面中）
		/// </summary>
		public bool Editable { get; set; }
	}

	[Serializable]
	public class QueryableItem
	{
		public string Name { get; set; }
		public OperatorTypes QueryOperator { get; set; }
		public string Operator {
			get {
				switch (this.QueryOperator) {
					case OperatorTypes.Equals:
						return "=";
					case OperatorTypes.Like:
						return "LIKE";
					case OperatorTypes.GreaterThanOrEquals:
						return ">=";
					case OperatorTypes.GreaterThan:
						return ">";
					case OperatorTypes.LessThanOrEquals:
						return "<=";
					case OperatorTypes.LessThan:
						return "<";
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}
		public object Value { get; set; }
	}
}
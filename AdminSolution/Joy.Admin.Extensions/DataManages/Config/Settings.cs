using System.Collections.Generic;
using HTB.DevFx.Config;
using Joy.Admin.Extensions.DataManages.Data.Model;

[assembly: ConfigResource("res://Joy.Admin.Extensions.DataManages.Config.Settings.config")]

namespace Joy.Admin.Extensions.DataManages.Config
{
	internal class DataManageServiceSetting : ConfigSettingElement
	{
		protected override void OnConfigSettingChanged() {
			base.OnConfigSettingChanged();
			this.Items = this.GetSettings<DataItemSetting>("items", null).ToArray();
		}
		public DataItemSetting[] Items { get; private set; }
	}

	internal class DataItemSetting : ConfigSettingElement
	{
		protected override void OnConfigSettingChanged() {
			base.OnConfigSettingChanged();
			this.Name = this.GetRequiredSetting("name");
			this.Title = this.GetRequiredSetting("title");
			this.TableName = this.GetRequiredSetting("tableName");
			this.Columns = this.GetSettings<ItemColumnSetting>(null).ToArray();
		}
		public string Name { get; private set; }
		public string Title { get; private set; }
		public string TableName { get; private set; }
		public ItemColumnSetting[] Columns { get; private set; }
	}

	internal class ItemColumnSetting : ConfigSettingElement
	{
		protected override void OnConfigSettingChanged() {
			base.OnConfigSettingChanged();
			this.Title = this.GetSetting("title");
			this.Name = this.GetSetting("columnNme");
			if (string.IsNullOrEmpty(this.Name)) {
				this.Name = this.GetSetting("name");
			}
			this.IsPrimaryKey = this.GetSetting("primaryKey", false);
			this.EditRenderType = this.GetSetting<RenderTypes>("editRenderType");
			this.QueryRenderType = this.GetSetting<RenderTypes>("queryRenderType");
			this.Queryable = this.GetSetting("queryable", false);
			this.Sort = this.GetSetting<SortTypes>("sort");
			this.Editable = this.GetSetting("editable", false);
			this.QueryOperator = this.GetSetting<OperatorTypes>("queryOperator");
			this.ValueFormat = this.GetSetting("valueFormat");
			this.Listable = this.GetSetting("listable", true);
		}
		public string Title { get; private set; }
		public string Name { get; private set; }
		public bool IsPrimaryKey { get; private set; }
		public RenderTypes EditRenderType { get; private set; }
		public bool Queryable { get; private set; }
		public OperatorTypes QueryOperator { get; private set; }
		public RenderTypes QueryRenderType { get; private set; }
		public SortTypes Sort { get; private set; }
		public bool Editable { get; private set; }
		public string ValueFormat { get; private set; }
		public bool Listable { get; private set; }
	}

	internal class CreateEntitiesSetting : ConfigSettingElement
	{
		protected override void OnConfigSettingChanged() {
			this.ManageEntities = this.GetSettings<ManageEntitySetting>("entity");
			this.ConnectionString = GetSetting("connection", "");
		}

		public List<ManageEntitySetting> ManageEntities { get; private set; }
		public string ConnectionString { get; private set; }
	}

	public class ManageEntitySetting : ConfigSettingElement
	{
		protected override void OnConfigSettingChanged() {
			base.OnConfigSettingChanged();
			this.Columns = this.GetSettings<EntityColumnSetting>("columns",null).ToArray();
			this.Name = GetSetting("name");
		    this.NeedPager = this.GetSetting("needpager", false);
		}
		public string Name { get; private set; }
        public bool NeedPager{get; private set; }
		public EntityColumnSetting[] Columns { get; private set; }
	}

    public class EntityColumnSetting : ConfigSettingElement
    {
        protected override void OnConfigSettingChanged() {
            base.OnConfigSettingChanged();
            this.IsPrimaryKey = this.GetSetting("primkey", false);
            this.Name = GetSetting("name", "");
            this.Label = GetSetting("label", "");
            this.Sort = GetSetting("sort", "");
            this.Formatter = this.GetSetting("formatter", "");
            this.CanEdit = this.GetSetting("canedit", false);
            this.CanAdd = this.GetSetting("canadd", false);
            this.ControlType = this.GetSetting("control", Data.Model.ControlTypes.None);
            this.Relation = this.GetSetting("relation", "");
            this.RelationFields = this.GetSetting("relationfields", "");
        }
        public bool IsPrimaryKey { get; private set; }
        public string Name { get; private set; }
        public string Label { get; private set; }
        public string Sort { get; private set; }
        public string Formatter { get; private set; }
        public bool CanEdit { get; private set; }
        public bool CanAdd { get; private set; }
        public SearchTypes SearchType { get; private set; }
        public Data.Model.ControlTypes ControlType { get; private set; }
        public string Relation { get; private set; }
        public string RelationFields { get; private set; }


        public ManageColumn ToManageColumn() {
            return new ManageColumn() {
                IsPrimaryKey = IsPrimaryKey,
                Field = this.Name, 
                Label = this.Label, 
                Formatter = this.Formatter,
                ControlType=this.ControlType,
                RelationName = Relation,
                RelationFields = RelationFields
            };
        }
    }

}

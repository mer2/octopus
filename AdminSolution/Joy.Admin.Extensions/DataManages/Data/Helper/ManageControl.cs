using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using HTB.DevFx;
using Joy.Admin.Extensions.DataManages.Data.Model;
using Joy.Admin.Extensions.DataManages.Data.Server;

namespace Joy.Admin.Extensions.DataManages.Data.Helper
{
    public class ManageControl
    {
        public Model.ControlTypes ControlType { get; set; }
        public string RelationName { get; set; }
        public string RelationField { get; set; }

        public string GetControl(Model.ControlTypes controlType, string relationName, string relationFields, string name, string value) {
            string html = string.Empty;

            switch(controlType) {
                case Model.ControlTypes.Text:
                    html = string.Format("<input type='text' name='{0}' value='{1}' />", name, value);
                    break;
                case Model.ControlTypes.Select:
                    var svr = ObjectService.GetObject<IManageServer>();
                    svr.TableName = relationName;
                    var dt = svr.GetDataList(null, null);
                    var fields = relationFields.Split(',');
                    StringBuilder select = new StringBuilder();
                    select.AppendFormat("<select name='{0}'>", name);
                    foreach(DataRow dr in dt.Rows) {
                        var selValue = dr[fields[0]].ToString();
                        var selText = dr[fields[1]].ToString();
                        select.AppendFormat("<option {2} value='{0}'>{1}</option>", selValue, selText, selValue == value ? "selected='selected'" : string.Empty);
                    }
                    select.Append("</select>");
                    html = select.ToString();
                    break;
                default:
                    html = string.Format("<input type='hidden' name='{0}' value='{1}' />", name, value);
                    break;
            }

            return html;
        }
    }
}

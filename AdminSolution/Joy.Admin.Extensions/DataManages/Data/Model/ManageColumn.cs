using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Joy.Admin.Extensions.DataManages.Data.Model
{
    public class ManageColumn
    {
        public bool IsPrimaryKey { get; set; }
        public string Field { get; set; }
        public string Label { get; set; }
        public string Formatter { get; set; }
        public ControlTypes ControlType { get; set; }
        public string RelationName { get; set; }
        public string RelationFields { get; set; }
    }
}

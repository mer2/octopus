using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Joy.Admin.Extensions.DataManages.Data.Model
{
    public class Pager
    {
        public int StartIndex { get; set; }
        public int EndIndex {
            get { return StartIndex + Length - 1; }
        }
        public int Length { get; set; }
        public int Count { get; set; }
    }
}

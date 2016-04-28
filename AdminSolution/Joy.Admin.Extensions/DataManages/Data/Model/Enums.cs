using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Joy.Admin.Extensions.DataManages.Data.Model
{
    public enum SearchTypes
    {
        Equals,
        Like,
        GreaterThanOrEquals,
        GreaterThan,
        LessThanOrEquals,
        LessThan
    }

    public enum ControlTypes
    {
        None,
        Text,
        Select
    }
}

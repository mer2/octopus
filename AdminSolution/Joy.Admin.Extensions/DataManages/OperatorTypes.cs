using System;

namespace Joy.Admin.Extensions.DataManages
{
	[Serializable]
	public enum OperatorTypes
	{
		Equals,
		Like,
		GreaterThanOrEquals,
		GreaterThan,
		LessThanOrEquals,
		LessThan
	}
}
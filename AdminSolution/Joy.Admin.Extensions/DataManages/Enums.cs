using System;

namespace Joy.Admin.Extensions.DataManages
{
	[Serializable, Flags]
	public enum OperatorTypes
	{
		Equals = 0,
		Like = 1,
		GreaterThanOrEquals = 2,
		GreaterThan = 4,
		LessThanOrEquals = 8,
		LessThan = 16
	}
	
	[Serializable]
	public enum RenderTypes
	{
		None,
		Text,
		Select,
		Hidden
	}

	[Serializable]
	public enum SortTypes
	{
		None,
		Asc,
		Desc
	}
}
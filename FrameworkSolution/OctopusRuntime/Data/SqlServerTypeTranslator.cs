using System;
using System.Collections;
using HTB.DevFx.Data.Entities;
using HTB.DevFx.Utils;

namespace Octopus.Data
{
	public class SqlServerTypeTranslator : ITypeTranslator
	{
		public object Translate(object value, Type expectedType, IDictionary options) {
			if (value == null || value == DBNull.Value) {
				if(expectedType.IsValueType) {
					return TypeHelper.CreateObject(expectedType, null, true);
				}
				return null;
			}

			var convertingValue = value;
			if (convertingValue is DateTime && expectedType == typeof(DateTime?)) {
				return convertingValue;
			}
			if (expectedType == null) {
				return convertingValue;
			}
			return expectedType.IsEnum ? Enum.ToObject(expectedType, Convert.ToInt32(value)) : Convert.ChangeType(convertingValue, expectedType);
		}
	}
}

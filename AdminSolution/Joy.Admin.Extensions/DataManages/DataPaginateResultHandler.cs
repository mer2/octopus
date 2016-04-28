using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using HTB.DevFx.Data;
using HTB.DevFx.Data.Entities;

namespace Joy.Admin.Extensions.DataManages
{
	internal class DataPaginateResultHandler : PaginateResultHandler
	{
		public DataPaginateResultHandler(IObjectMapper objectMapper) : base(objectMapper) {}

		protected override IPaginateResult ExecuteResultInternal(IDbExecuteContext ctx, DbCommand command, Type elementType) {
			var result = new PaginateResult();
			using(var reader = command.ExecuteReader(CommandBehavior.Default)) {
				if(reader.Read()) {
					result.Count = Convert.ToInt32(reader[0]);
				}
				var list = new ArrayList();
				if(reader.NextResult()) {
					while(reader.Read()) {
						list.Add(reader.ToDictionary());
					}
				}
				result.Items = list.ToArray();
			}
			return result;
		}
	}
}

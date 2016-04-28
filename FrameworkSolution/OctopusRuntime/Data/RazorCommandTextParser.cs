using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using HTB.DevFx.Data;
using HTB.DevFx.Data.Config;
using HTB.DevFx.Log;
using RazorEngine;

namespace Octopus.Data
{
	internal class RazorCommandTextParser : CommandTextParser
	{
		private readonly List<IStatementSetting> templates = new List<IStatementSetting>();
		protected override string GetCommandText(IStatementSetting statement, IDictionary parameters) {
			if(!this.templates.Contains(statement)) {
				lock(this.templates) {
					if(!this.templates.Contains(statement)) {
						Razor.Compile(statement.StatementText.ConfigSetting.Value.Value.Trim(' ', '\t', '\n'), typeof(DynamicParameter), statement.Name);
						if(!this.templates.Contains(statement)) {
							this.templates.Add(statement);
						}
					}
				}
			}
			var commandText = Razor.Run(statement.Name, new DynamicParameter(parameters));
			this.Trace(statement, parameters, commandText);
			return commandText;
		}

		private void Trace(IStatementSetting statement, IDictionary parameters, string commandText) {
			var sb = new StringBuilder();
			sb.AppendFormat("{0}\r\n{1}\r\n", statement.Name, commandText);
			sb.Append("{");
			foreach(var key in parameters.Keys) {
				sb.AppendFormat("{0}={1},", key, parameters[key]);
			}
			sb.Append("}");
			LogService.WriteLog(this, LogLevel.DEBUG, sb.ToString());
		}
	
		internal class DynamicParameter : DynamicObject
		{
			public DynamicParameter(IDictionary parameters) {
				this.parameters = parameters;
			}
			private readonly IDictionary parameters;

			public void Add(string key, object value) {
				this.parameters.Add(key, value);
			}

			public bool Contains(string key) {
				return this.parameters.Contains(key);
			}

			public bool Contains(string key, bool notNull) {
				if(this.parameters.Contains(key)) {
					if(notNull) {
						return this.parameters[key] != null;
					}
					return true;
				}
				return false;
			}

			public override bool TryGetMember(GetMemberBinder binder, out object result) {
				result = this.parameters[binder.Name];
				return true;
			}

			public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result) {
				var name = binder.Name;
				if (name == "Contains") {
					if(args.Length >= 2) {
						result = this.Contains((string)args[0], (bool)args[1]);
					} else if(args.Length >= 1) {
						result = this.Contains((string)args[0]);
					} else {
						result = null;
					}
				} else {
					result = null;
				}
				return true;
			}

			public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result) {
				result = this.parameters[indexes[0]];
				return true;
			}
		}
	}
}

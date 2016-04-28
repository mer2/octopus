using System;
using System.Configuration;
using System.Data.Common;

namespace Octopus.EventBus.Repositories
{
	internal class RedisConnectionStringBuilder : DbConnectionStringBuilder
	{
		public RedisConnectionStringBuilder(string connectionStringName) {
			var connectionSetting = ConfigurationManager.ConnectionStrings[connectionStringName];
			if (connectionSetting != null && !string.IsNullOrEmpty(connectionSetting.ConnectionString)) {
				this.Init(connectionSetting.ConnectionString);
			}
		}

		private void Init(string connectionString) {
			this.ConnectionString = connectionString;
			object value;
			if (this.TryGetValue("Server", out value)) {
				this.server = Convert.ToString(value);
			}
			if (this.TryGetValue("Port", out value)) {
				this.port = Convert.ToInt32(value);
			}
			if (this.TryGetValue("Password", out value)) {
				this.password = Convert.ToString(value);
			}
		}

		private string server = "localhost";
		private int port = 6379;
		private string password;

		public string Server {
			get { return this.server; }
		}

		public int Port {
			get { return this.port; }
		}

		public string Password {
			get { return this.password; }
		}
	}
}

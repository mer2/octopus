using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace Octopus.Cryptography
{
	[Serializable]
	public abstract class TicketBase
	{
		/// <summary>
		/// 票据版本
		/// </summary>
		public int Version { get; set; }
		/// <summary>
		/// 票据创建时间
		/// </summary>
		public DateTime CreateDate { get; set; }
		/// <summary>
		/// 票据发行时间
		/// </summary>
		public DateTime IssueDate { get; set; }
		/// <summary>
		/// 票据过期时间
		/// </summary>
		public DateTime Expiration { get; set; }
		/// <summary>
		/// 票据标识（唯一性）
		/// </summary>
		public string Token { get; set; }

		private IDictionary td;
		/// <summary>
		/// 信息存储
		/// </summary>
		public virtual IDictionary TicketData {
			get { return this.td ?? (this.td = new HybridDictionary()); }
			set { this.td = value; }
		}

		public virtual void Initialiaze() {
			var data = this.TicketData;
			this.Version = this.GetValue<int>(data, "v");
			this.CreateDate = this.GetValue<DateTime>(data, "c");
			this.IssueDate = this.GetValue<DateTime>(data, "i");
			this.Expiration = this.GetValue<DateTime>(data, "e");
			this.Token = this.GetValue<string>(data, "t");
		}

		public override string ToString() {
			var sb = new StringBuilder();
			this.Serialize(sb);
			return sb.ToString();
		}

		protected virtual Encoding Encoding {
			get { return Encoding.UTF8; }
		}

		protected virtual void Serialize(StringBuilder sb) {
			this.Serialize(sb, "v", this.Version, "&");
			this.Serialize(sb, "c", this.CreateDate, "&");
			this.Serialize(sb, "i", this.IssueDate, "&");
			this.Serialize(sb, "e", this.Expiration, "&");
			this.Serialize(sb, "t", this.Token, "");
		}

		protected virtual void Serialize(StringBuilder sb, string name, object value, string link) {
			sb.AppendFormat("{0}={1}{2}", name, HttpUtility.UrlEncode(Convert.ToString(value), this.Encoding), link);
		}

		protected virtual void Serialize(StringBuilder sb, string name, int value, string link) {
			sb.AppendFormat("{0}={1}{2}", name, value, link);
		}

		protected virtual void Serialize(StringBuilder sb, string name, DateTime value, string link) {
			sb.AppendFormat("{0}={1}{2}", name, value.Ticks, link);
		}

		protected virtual T GetValue<T>(IDictionary data, string name) {
			var value = data[name];
			if(value == null) {
				return default(T);
			}
			var type = typeof (T);
			if(type == typeof(DateTime)) {
				value = new DateTime((long)Convert.ChangeType(value, typeof (long)));
				return (T)value;
			}
			return (T)Convert.ChangeType(value, type);
		}
	}
}

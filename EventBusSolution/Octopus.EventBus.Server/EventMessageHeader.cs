using System;

namespace Octopus.EventBus.Server
{
	[Serializable]
	public class EventMessageHeader
	{
		public EventMessageHeader() { }

		public EventMessageHeader(EventMessage message) {
			if(string.IsNullOrEmpty(message.MessageGuid)) {
				message.MessageGuid = Guid.NewGuid().ToString("N");
			}
			this.MessageGuid = message.MessageGuid;
			this.Category = message.Category;
			this.EventName = message.EventName;
			this.Priority = message.Priority;
			this.Message = message;
		}

		public string MessageGuid { get; set; }
		public string Category { get; set; }
		public string EventName { get; set; }
		public int Priority { get; set; }
		public int SentCount { get; set; }
		public EventMessage Message { get; set; }

		public byte[] GetMessageKey() {
			return Guid.ParseExact(this.MessageGuid, "N").ToByteArray();
		}
	}
}

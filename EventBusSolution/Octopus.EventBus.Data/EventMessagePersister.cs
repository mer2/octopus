using System;
using System.Collections;
using HTB.DevFx.Data;
using HTB.DevFx.Exceptions;

namespace Octopus.EventBus.Data
{
	internal class EventMessagePersister
	{
		public static void Save(EventMessage[] messages) {
			if(messages == null || messages.Length <= 0) {
				return;
			}
			using(var session = DataService.BeginSession()) {
				foreach(var message in messages) {
					if(message == null) {
						continue;
					}
					try {
						session.Execute("SaveEventMessage", EventMessageToDictionary(message));
					} catch(Exception e) {
						ExceptionService.Publish(e);
					}
				}
			}
		}

		public static IDictionary EventMessageToDictionary(EventMessage message) {
			var dict = message.ToDictionary();
			if(message.Tags != null) {
				dict["Tags"] = string.Join(",", message.Tags);
			}
			var body = message.Message;
			if(body != null && !(body is string)) {
				var type = body.GetType();
				if(!type.IsPrimitive && !type.IsEnum) {
					dict["Message"] = Newtonsoft.Json.JsonConvert.SerializeObject(body);
				}
			}
			var data = message.Data;
			if(data != null && data.Count > 0) {
				dict["Data"] = Newtonsoft.Json.JsonConvert.SerializeObject(data);
			} else {
				dict["Data"] = null;
			}
			return dict;
		}
	}
}

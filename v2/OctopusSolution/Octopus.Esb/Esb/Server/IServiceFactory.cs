using System;
using System.Collections.Generic;
using Octopus.Esb.Config;

namespace Octopus.Esb.Server
{
	public interface IServiceFactory : IHttpHandler
	{
		IDictionary<string, ServiceItemSetting> GetServiceItemSettings();
		event Action<ServiceContext> Request;
		event Action<ServiceContext> Calling;
		event Action<ServiceContext> Called;
		event Action<ServiceContext> Response;
		event Action<ServiceContext> Error;
	}
}
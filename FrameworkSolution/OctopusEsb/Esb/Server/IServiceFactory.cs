using System;

namespace Octopus.Esb.Server
{
	public interface IServiceFactory
	{
		event Action<ServiceContext> Request;
		event Action<ServiceContext> Calling;
		event Action<ServiceContext> Called;
		event Action<ServiceContext> Response;
		event Action<ServiceContext> Error;
	}
}
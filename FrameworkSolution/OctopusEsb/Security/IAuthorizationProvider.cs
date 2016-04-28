using Octopus.Esb.Server;

namespace Octopus.Security
{
	public interface IAuthorizationProvider
	{
		bool Authorize(ServiceContext ctx, IAuthorizationIdentity identity);
	}
}
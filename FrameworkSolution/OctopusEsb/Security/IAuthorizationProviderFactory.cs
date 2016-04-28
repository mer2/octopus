using Octopus.Esb.Server;

namespace Octopus.Security
{
	public interface IAuthorizationProviderFactory
	{
		void Authorize(ServiceContext ctx, IAuthorizationProviderFactory defaultFactory, IAuthorizationIdentity[] identities);
	}
}

using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Routing;

namespace Octopus.Esb.Server
{
	internal class ValueProviderFactoryCollection : Collection<ValueProviderFactory>
	{
		public IValueProvider GetValueProvider(RequestContext requestContext) {
			var valueProviders = from factory in this
								 let valueProvider = factory.GetValueProvider(requestContext)
								 where valueProvider != null
								 select valueProvider;

			return new ValueProviderCollection(valueProviders.ToList());
		}
	}
}

namespace Octopus.Esb.Server
{
	public  interface IValueProvider
	{
		ValueProviderResult GetValue(string name);
	}
}

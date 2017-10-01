namespace Octopus.Esb
{
	public interface ISerializerFactory
	{
		ISerializer GetSerializer(string name);
		ISerializer Default { get; }
	}
}

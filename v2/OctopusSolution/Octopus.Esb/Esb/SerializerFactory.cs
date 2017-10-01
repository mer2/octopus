using HTB.DevFx;

namespace Octopus.Esb
{
	public abstract class SerializerFactory
	{
		public static ISerializerFactory Current => ObjectService.GetObject<ISerializerFactory>();
		public static ISerializer DefaultSerializer => Current.Default;

		public static ISerializer GetSerializer(string name) {
			return Current.GetSerializer(name);
		}
	}
}

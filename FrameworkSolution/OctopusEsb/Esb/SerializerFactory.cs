using HTB.DevFx;

namespace Octopus.Esb
{
	public abstract class SerializerFactory
	{
		public static ISerializerFactory Current {
			get { return ObjectService.GetObject<ISerializerFactory>(); }
		}

		public static ISerializer DefaultSerializer {
			get { return Current.Default; }
		}

		public static ISerializer GetSerializer(string name) {
			return Current.GetSerializer(name);
		}
	}
}

using System;
using HTB.DevFx;
using HTB.DevFx.Esb;
using HTB.DevFx.Esb.Config;
using HTB.DevFx.Utils;

namespace Octopus.EventBus.Repositories
{
	internal class RedisEventStorageBuilder : IObjectBuilder
	{
		public object CreateObject(IObjectSetting setting, params object[] parameters) {
			if (setting == null) {
				return null;
			}
			var typeName = ObjectService.GetTypeName(setting.TypeName);
			var objectType = TypeHelper.CreateType(typeName, false);
			if (objectType == null) {
				return null;
			}
			if(!typeof(IEventStorageInternal).IsAssignableFrom(objectType)) {
				throw new Exception("Create Redis Event Storage Only.");
			}
			var storage = (IEventStorageInternal)ObjectService.Current.ObjectBuilder.CreateObject((HTB.DevFx.Core.Config.IObjectSetting)setting, parameters);
			if(storage == null) {
				return null;
			}
			storage.ConnectionStringName = setting.Name;
			return storage;
		}
	}
}

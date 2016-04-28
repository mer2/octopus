using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Web.Script.Serialization;
using HTB.DevFx.Utils;

namespace Octopus.Esb
{
	internal class JsonSerializer : ISerializer
	{
		protected override void SerializeInternal(TextWriter sw, object instance) {
			var ser = this.GetSerializer();
			ser.Serialize(sw, instance);
			sw.Flush();
		}

		protected override object DeserializeInternal(TextReader sr, Type expectedType) {
			var ser = this.GetSerializer();
			return ser.Deserialize(sr, expectedType);
		}

		protected override object ConvertInternal(object instance, Type expectedType) {
			var ser = this.GetSerializer();
			return ser.Convert(instance, expectedType);
		}

		protected virtual ISerializer GetSerializer() {
			if(serializer == null) {
				lock(typeof(JsonSerializerFactory)) {
					serializer = CreateSerializer();
				}
			}
			return serializer;
		}

		private static ISerializer serializer;
		private static ISerializer CreateSerializer() {
			var newtonsoftJsonSerializerType = TypeHelper.CreateType("Newtonsoft.Json.JsonSerializer, Newtonsoft.Json", false);
			if(newtonsoftJsonSerializerType != null) {
				var type = CreateDynamicType(newtonsoftJsonSerializerType);
				return (ISerializer)TypeHelper.CreateObject(type, typeof(ISerializer), false);
			}
			return new DefaultJsonSerializer();
		}
		private static Type CreateDynamicType(Type baseType) {
			const string spaceName = "Octopus.Esb.Reflection.Dynamics";
			var domain = Thread.GetDomain();
			var name = new AssemblyName(spaceName);
			var builder = domain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
			var moduleBuilder = builder.DefineDynamicModule("DynamicModule");
			var typeBuilder = moduleBuilder.DefineType(spaceName + ".NewtonsoftJsonSerializer", TypeAttributes.Class, baseType, new[] { typeof(ISerializer) });
			
			var ctorBuilder = typeBuilder.DefineMethod(".ctor", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, typeof(void), new Type[] { });
			var ctor = baseType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { }, null);
			var setTypeNameHandling = baseType.GetMethod("set_TypeNameHandling", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { TypeHelper.CreateType("Newtonsoft.Json.TypeNameHandling, Newtonsoft.Json", true) }, null);
			var gen = ctorBuilder.GetILGenerator();
			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Call, ctor);
			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Ldc_I4_3);
			gen.Emit(OpCodes.Callvirt, setTypeNameHandling);
			gen.Emit(OpCodes.Ret);

			var serializeBuilder = typeBuilder.DefineMethod("ISerializer.Serialize", MethodAttributes.Private | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.SpecialName, typeof(void), new[] { typeof(TextWriter), typeof(object) });
			var serialize = baseType.GetMethod("Serialize", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[]{ typeof(TextWriter), typeof(object) }, null);
			serializeBuilder.DefineParameter(1, ParameterAttributes.None, "");
			serializeBuilder.DefineParameter(2, ParameterAttributes.None, "");
			gen = serializeBuilder.GetILGenerator();
			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Ldarg_1);
			gen.Emit(OpCodes.Ldarg_2);
			gen.Emit(OpCodes.Call, serialize);
			gen.Emit(OpCodes.Ret);
			typeBuilder.DefineMethodOverride(serializeBuilder, typeof(ISerializer).GetMethod("Serialize"));

			var deserializeBuilder = typeBuilder.DefineMethod("ISerializer.Deserialize", MethodAttributes.Private | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.SpecialName, typeof(object), new[] { typeof(TextReader), typeof(Type) });
			var deserialize = baseType.GetMethod("Deserialize", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(TextReader), typeof(Type) }, null);
			deserializeBuilder.DefineParameter(1, ParameterAttributes.None, "");
			deserializeBuilder.DefineParameter(2, ParameterAttributes.None, "");
			gen = deserializeBuilder.GetILGenerator();
			gen.DeclareLocal(typeof(object));
			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Ldarg_1);
			gen.Emit(OpCodes.Ldarg_2);
			gen.Emit(OpCodes.Call, deserialize);
			gen.Emit(OpCodes.Ret);
			typeBuilder.DefineMethodOverride(deserializeBuilder, typeof(ISerializer).GetMethod("Deserialize"));

			var convertBuilder = typeBuilder.DefineMethod("ISerializer.Convert", MethodAttributes.Private | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.SpecialName, typeof(object), new[] { typeof(object), typeof(Type) });
			convertBuilder.DefineParameter(1, ParameterAttributes.None, "");
			convertBuilder.DefineParameter(2, ParameterAttributes.None, "");
			gen = convertBuilder.GetILGenerator();
			gen.Emit(OpCodes.Ldarg_1);
			gen.Emit(OpCodes.Ret);
			typeBuilder.DefineMethodOverride(convertBuilder, typeof(ISerializer).GetMethod("Convert"));

			var getContentTypeBuilder = typeBuilder.DefineMethod("get_ContentType", MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot, typeof(string), new Type[] { });
			gen = getContentTypeBuilder.GetILGenerator();
			gen.Emit(OpCodes.Ldstr, "application/json");
			gen.Emit(OpCodes.Ret);

			var returnType = typeBuilder.CreateType();
			return returnType;
		}

		public void Serialize(TextWriter sw, object instance) {
			throw new NotImplementedException();
		}

		public object Deserialize(TextReader sr, Type expectedType) {
			throw new NotImplementedException();
		}

		public object Convert(object instance, Type expectedType) {
			throw new NotImplementedException();
		}

		string ISerializer.ContentType {
			get { return ContentType; }
		}

		protected override string ContentType {
			get { return "application/json"; }
		}

		private class DefaultJsonSerializer : SerializerFactory
		{
			protected override void SerializeInternal(TextWriter sw, object instance) {
				var jsonData = JsonHelper.ToJson(instance, true);
				sw.Write(jsonData);
			}

			protected override object DeserializeInternal(TextReader sr, Type expectedType) {
				var jsonData = sr.ReadToEnd();
				var js = new JavaScriptSerializer();
				var method = typeof (JavaScriptSerializer).GetMethod("Deserialize", new[] {typeof (string)}).MakeGenericMethod(expectedType);
				object result;
				TypeHelper.TryInvoke(js, method, out result, true, jsonData);
				return result;
			}

			protected override object ConvertInternal(object instance, Type expectedType) {
				if(expectedType.IsInstanceOfType(instance)) {
					return instance;
				}
				var js = new JavaScriptSerializer();
				var method = typeof (JavaScriptSerializer).GetMethod("ConvertToType", new[] { typeof(object) }).MakeGenericMethod(expectedType);
				object result;
				TypeHelper.TryInvoke(js, method, out result, true, instance);
				return result;
			}

			protected override string ContentType {
				get { return "application/json"; }
			}
		}
	}
}

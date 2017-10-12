using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using HTB.DevFx.Cryptography;
using HTB.DevFx.Esb;
using HTB.DevFx.Exceptions;

namespace Octopus.Cryptography
{
	/// <summary>
	/// 对对象进行加解密
	/// </summary>
	public static class CryptoHelper
	{
		#region RSA

		#region RsaEncrypt

		public static string RsaEncrypt(object data, string signKey, string encryptKey, Encoding encoding, bool urlEncoding, ISerializer serializer = null) {
			using(var stream = new MemoryStream()) {
				serializer = serializer ?? SerializerFactory.Current.Default;
				serializer.Serialize(stream, data, null);
				return RsaEncryptBase(stream, signKey, encryptKey, encoding, urlEncoding);
			}
		}

		/// <summary>
		/// 加密（使用RSA签名）
		/// </summary>
		/// <param name="data">被加密的字符串</param>
		/// <param name="signKey">验证密钥</param>
		/// <param name="encryptKey">加密密钥</param>
		/// <param name="encoding">编码</param>
		/// <param name="urlEncoding">是否进行Url Encode</param>
		/// <returns>加密后的字符串（Base64）</returns>
		public static string RsaEncrypt(string data, string signKey, string encryptKey, Encoding encoding, bool urlEncoding) {
			if (encoding == null) {
				encoding = Encoding.UTF8;
			}
			using (var stream = new MemoryStream()) {
				var bytes = encoding.GetBytes(data);
				stream.Write(bytes, 0, bytes.Length);
				return RsaEncryptBase(stream, signKey, encryptKey, encoding, urlEncoding);
			}
		}

		/// <summary>
		/// 加密（使用RSA签名）
		/// </summary>
		/// <param name="data">被加密的字节流</param>
		/// <param name="signKey">验证密钥</param>
		/// <param name="encryptKey">加密密钥</param>
		/// <param name="encoding">编码</param>
		/// <param name="urlEncoding">是否进行Url Encode</param>
		/// <returns>加密后的字符串（Base64）</returns>
		public static string RsaEncrypt(byte[] data, string signKey, string encryptKey, Encoding encoding, bool urlEncoding) {
			if (encoding == null) {
				encoding = Encoding.UTF8;
			}
			using (var stream = new MemoryStream()) {
				stream.Write(data, 0, data.Length);
				return RsaEncryptBase(stream, signKey, encryptKey, encoding, urlEncoding);
			}
		}

		internal static byte[] RsaEncryptBase(MemoryStream stream, RSACryptoServiceProvider rsa, byte[] encryptKey, HashAlgorithm hashAlgorithm) {
			var signData = rsa.SignData(stream.ToArray(), hashAlgorithm);
			stream.Write(signData, 0, signData.Length);
			stream.WriteByte(Convert.ToByte(signData.Length));
			return XXTEA.Encrypt(stream.ToArray(), encryptKey);
		}

		internal static byte[] RsaEncryptBase(MemoryStream stream, string signKey, byte[] encryptKey, HashAlgorithm hashAlgorithm) {
			RSACryptoServiceProvider rsa;
			if (signKey.StartsWith("<RSAKeyValue>", true, null)) {
				rsa = new RSACryptoServiceProvider();
				rsa.FromXmlString(signKey);
			} else {
				rsa = new RSACryptoServiceProvider(new CspParameters { KeyContainerName = signKey, Flags = CspProviderFlags.UseMachineKeyStore });
			}
			using (rsa) {
				return RsaEncryptBase(stream, rsa, encryptKey, hashAlgorithm);
			}
		}

		internal static string RsaEncryptBase(MemoryStream stream, string signKey, string encryptKey, Encoding encoding, bool urlEncoding) {
			if (encoding == null) {
				encoding = Encoding.UTF8;
			}
			var hashAlgorithm = MD5.Create();
			var encryptKeyBytes = hashAlgorithm.ComputeHash(encoding.GetBytes(encryptKey));
			var encryptedBytes = RsaEncryptBase(stream, signKey, encryptKeyBytes, hashAlgorithm);
			var result = ToBase64String(encryptedBytes);
			return urlEncoding ? HttpUtility.UrlEncode(result, encoding) : result;
		}

		#endregion

		#region RsaDecrypt

		internal static byte[] RsaDecryptBase(byte[] data, RSACryptoServiceProvider rsa, byte[] encryptKey, HashAlgorithm hashAlgorithm) {
			var input = XXTEA.Decrypt(data, encryptKey);
			var hashLength = input[input.Length - 1];
			var hashData = new byte[hashLength];
			var signData = new byte[input.Length - hashLength - 1];
			Buffer.BlockCopy(input, input.Length - hashLength - 1, hashData, 0, hashLength);
			Buffer.BlockCopy(input, 0, signData, 0, signData.Length);
			return rsa.VerifyData(signData, hashAlgorithm, hashData) ? signData : null;
		}

		internal static byte[] RsaDecryptBase(byte[] data, string signKey, byte[] encryptKey, HashAlgorithm hashAlgorithm) {
			RSACryptoServiceProvider rsa;
			if (signKey.StartsWith("<RSAKeyValue>", true, null)) {
				rsa = new RSACryptoServiceProvider();
				rsa.FromXmlString(signKey);
			} else {
				rsa = new RSACryptoServiceProvider(new CspParameters { KeyContainerName = signKey, Flags = CspProviderFlags.UseMachineKeyStore });
			}
			using (rsa) {
				return RsaDecryptBase(data, rsa, encryptKey, hashAlgorithm);
			}
		}

		/// <summary>
		/// 解密（使用RSA签名）
		/// </summary>
		/// <param name="data">被解密的字符串（Base64）</param>
		/// <param name="signKey">验证密钥</param>
		/// <param name="encryptKey">解密密钥</param>
		/// <param name="encoding">编码</param>
		/// <param name="urlEncoding">被解密的字符串需要进行Url Decode</param>
		/// <returns>如果成功，则返回字节数组，否则为空</returns>
		public static byte[] RsaDecryptBase(string data, string signKey, string encryptKey, Encoding encoding, bool urlEncoding) {
			if (encoding == null) {
				encoding = Encoding.UTF8;
			}
			data = urlEncoding ? HttpUtility.UrlDecode(data, encoding) : data;
			var input = FromBase64String(data);
			var hashAlgorithm = MD5.Create();
			var encryptKeyBytes = hashAlgorithm.ComputeHash(encoding.GetBytes(encryptKey));
			return RsaDecryptBase(input, signKey, encryptKeyBytes, hashAlgorithm);
		}

		/// <summary>
		/// 解密（使用RSA签名）
		/// </summary>
		/// <param name="data">被解密的字符串（Base64）</param>
		/// <param name="signKey">验证密钥</param>
		/// <param name="encryptKey">解密密钥</param>
		/// <param name="encoding">编码</param>
		/// <param name="urlEncoding">被解密的字符串需要进行Url Decode</param>
		/// <returns>如果成功，则返回对象，否则为空</returns>
		public static T RsaDecrypt<T>(string data, string signKey, string encryptKey, Encoding encoding, bool urlEncoding) {
			return RsaDecrypt<T>(data, signKey, encryptKey, encoding, urlEncoding, null);
		}

		public static T RsaDecrypt<T>(string data, string signKey, string encryptKey, Encoding encoding, bool urlEncoding, ISerializer serializer) {
			var bytes = RsaDecryptBase(data, signKey, encryptKey, encoding, urlEncoding);
			if(bytes != null) {
				using(var stream = new MemoryStream(bytes)) {
					serializer = serializer ?? SerializerFactory.Current.Default;
					return (T)serializer.Deserialize(stream, typeof(T), null);
				}
			}
			return default(T);
		}

		/// <summary>
		/// 解密（使用RSA签名，返回原始的字符串）
		/// </summary>
		/// <param name="data">被解密的字符串（Base64）</param>
		/// <param name="signKey">验证密钥</param>
		/// <param name="encryptKey">解密密钥</param>
		/// <param name="encoding">编码</param>
		/// <param name="urlEncoding">被解密的字符串需要进行Url Decode</param>
		/// <returns>如果成功，则返回字符串，否则为空</returns>
		public static string RsaDecryptToString(string data, string signKey, string encryptKey, Encoding encoding, bool urlEncoding) {
			var bytes = RsaDecryptBase(data, signKey, encryptKey, encoding, urlEncoding);
			if (bytes != null) {
				if(encoding == null) {
					encoding = Encoding.UTF8;
				}
				return encoding.GetString(bytes);
			}
			return null;
		}

		#endregion
	
		#endregion

		public static string EncryptObjectToString(object data, string validateKey, string encryptKey, Encoding encoding, bool urlEncoding, ISerializer serializer = null) {
			if(encoding == null) {
				encoding = Encoding.UTF8;
			}
			var encryptedBytes = EncryptObjectToBytes(data, validateKey, encryptKey, encoding, serializer);
			var result = ToBase64String(encryptedBytes);
			return urlEncoding ? HttpUtility.UrlEncode(result, encoding) : result;
		}

		public static byte[] EncryptObjectToBytes(object data, string validateKey, string encryptKey, Encoding encoding, ISerializer serializer = null) {
			if(encoding == null) {
				encoding = Encoding.UTF8;
			}
			var hashAlgorithm = MD5.Create();
			var validateKeyBytes = hashAlgorithm.ComputeHash(encoding.GetBytes(validateKey));
			var encryptKeyBytes = hashAlgorithm.ComputeHash(encoding.GetBytes(encryptKey));
			return Encrypt(data, validateKeyBytes, encryptKeyBytes, hashAlgorithm, serializer);
		}

		public static byte[] Encrypt(byte[] data, string validateKey, string encryptKey, Encoding encoding) {
			if(encoding == null) {
				encoding = Encoding.UTF8;
			}
			var hashAlgorithm = MD5.Create();
			var validateKeyBytes = hashAlgorithm.ComputeHash(encoding.GetBytes(validateKey));
			var encryptKeyBytes = hashAlgorithm.ComputeHash(encoding.GetBytes(encryptKey));
			return Encrypt(data, validateKeyBytes, encryptKeyBytes, hashAlgorithm);
		}

		internal static byte[] Encrypt(object data, byte[] validateKey, byte[] encryptKey, HashAlgorithm hashAlgorithm, ISerializer serializer = null) {
			using(var stream = new MemoryStream()) {
				serializer = serializer ?? SerializerFactory.Current.Default;
				serializer.Serialize(stream, data, null);
				return Encrypt(stream, validateKey, encryptKey, hashAlgorithm);
			}
		}

		internal static byte[] Encrypt(byte[] data, byte[] validateKey, byte[] encryptKey, HashAlgorithm hashAlgorithm) {
			using(var stream = new MemoryStream()) {
				stream.Write(data, 0, data.Length);
				return Encrypt(stream, validateKey, encryptKey, hashAlgorithm);
			}
		}

		internal static byte[] Encrypt(MemoryStream stream, byte[] validateKey, byte[] encryptKey, HashAlgorithm hashAlgorithm) {
			var length = stream.Length;
			stream.Write(validateKey, 0, validateKey.Length);
			var hash = hashAlgorithm.ComputeHash(stream.ToArray());
			stream.SetLength(length);
			stream.Write(hash, 0, hash.Length);
			return XXTEA.Encrypt(stream.ToArray(), encryptKey);
		}

		public static object DecryptToObject(string data, string validateKey, string encryptKey, Encoding encoding, bool urlEncoding, ISerializer serializer = null) {
			if(string.IsNullOrEmpty(data)) {
				return null;
			}
			if(encoding == null) {
				encoding = Encoding.UTF8;
			}
			data = urlEncoding ? HttpUtility.UrlDecode(data, encoding) : data;
			var input = FromBase64String(data);
			return DecryptToObject(input, validateKey, encryptKey, encoding, serializer);
		}

		public static object DecryptToObject(byte[] data, string validateKey, string encryptKey, Encoding encoding, ISerializer serializer = null) {
			if(data == null || data.Length <= 0) {
				return null;
			}
			if(encoding == null) {
				encoding = Encoding.UTF8;
			}
			var hashAlgorithm = MD5.Create();
			var validateKeyBytes = hashAlgorithm.ComputeHash(encoding.GetBytes(validateKey));
			var encryptKeyBytes = hashAlgorithm.ComputeHash(encoding.GetBytes(encryptKey));
			return DecryptToObject(data, validateKeyBytes, encryptKeyBytes, hashAlgorithm, 16, serializer);
		}

		public static byte[] Decrypt(byte[] data, string validateKey, string encryptKey, Encoding encoding) {
			if(data == null || data.Length <= 0) {
				return null;
			}
			if(encoding == null) {
				encoding = Encoding.UTF8;
			}
			var hashAlgorithm = MD5.Create();
			var validateKeyBytes = hashAlgorithm.ComputeHash(encoding.GetBytes(validateKey));
			var encryptKeyBytes = hashAlgorithm.ComputeHash(encoding.GetBytes(encryptKey));
			var result = Decrypt(data, validateKeyBytes, encryptKeyBytes, hashAlgorithm, 16);
			if(result == null) {
				return null;
			}
			using(result) {
				return result.ToArray();
			}
		}

		public static T DecryptTo<T>(string data, string validateKey, string encryptKey, Encoding encoding, bool urlEncoding, ISerializer serializer = null) {
			if(encoding == null) {
				encoding = Encoding.UTF8;
			}
			if(string.IsNullOrEmpty(data)) {
				return default(T);
			}
			data = urlEncoding ? HttpUtility.UrlDecode(data, encoding) : data;
			var input = FromBase64String(data);
			var hashAlgorithm = MD5.Create();
			var validateKeyBytes = hashAlgorithm.ComputeHash(encoding.GetBytes(validateKey));
			var encryptKeyBytes = hashAlgorithm.ComputeHash(encoding.GetBytes(encryptKey));
			return (T)DecryptToObject(input, validateKeyBytes, encryptKeyBytes, hashAlgorithm, 16, serializer);
		}

		internal static MemoryStream Decrypt(byte[] data, byte[] validateKey, byte[] encryptKey, HashAlgorithm hashAlgorithm, int hashReultLength) {
			try {
				var input = XXTEA.Decrypt(data, encryptKey);
				var hash = new byte[hashReultLength];
				Buffer.BlockCopy(input, input.Length - hashReultLength, hash, 0, hashReultLength);
				var stream = new MemoryStream();
				stream.Write(input, 0, input.Length - hashReultLength);
				var length = stream.Length;
				stream.Write(validateKey, 0, validateKey.Length);
				var validating = hashAlgorithm.ComputeHash(stream.ToArray());
				if(BitEquals(hash, validating)) {
					stream.Position = 0;
					stream.SetLength(length);
					return stream;
				}
			} catch(Exception e) {
				ExceptionService.Publish(e);
			}
			return null;
		}

		internal static object DecryptToObject(byte[] data, byte[] validateKey, byte[] encryptKey, HashAlgorithm hashAlgorithm, int hashReultLength, ISerializer serializer = null) {
			var stream = Decrypt(data, validateKey, encryptKey, hashAlgorithm, hashReultLength);
			if(stream == null) {
				return null;
			}
			using(stream) {
				serializer = serializer ?? SerializerFactory.Current.Default;
				return serializer.Deserialize(stream, null, null);
			}
		}

		internal static byte[] FromBase64String(string data) {
			var length = data.Length;
			var left = length % 4;
			if(left != 0) {
				length += 4 - left;
				data = data.PadRight(length, '=');
			}
			return Convert.FromBase64String(data);
		}

		internal static string ToBase64String(byte[] bytes) {
			return Convert.ToBase64String(bytes).TrimEnd('=');
		}

		internal static bool BitEquals(byte[] data, byte[] input) {
			var len1 = data == null ? 0 : data.Length;
			var len2 = input == null ? 0 : input.Length;
			if(len1 != len2) {
				return false;
			}
			if(data != null && input != null) {
				if(data.Length != input.Length) {
					return false;
				}
				for(var i = 0; i < data.Length; i++) {
					if(data[i] != input[i]) {
						return false;
					}
				}
			}
			return true;
		}
	}
}
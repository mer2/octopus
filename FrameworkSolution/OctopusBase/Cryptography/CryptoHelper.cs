using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using HTB.DevFx.Cryptography;

namespace Octopus.Cryptography
{
	/// <summary>
	/// 对对象进行加解密
	/// </summary>
	public static class CryptoHelper
	{
		#region RSA

		#region RsaEncrypt

		/// <summary>
		/// 加密（使用RSA签名）
		/// </summary>
		/// <param name="data">被加密的字节流</param>
		/// <param name="signKey">验证密钥</param>
		/// <param name="encryptKey">加密密钥</param>
		/// <param name="encoding">编码</param>
		/// <returns>加密后的字节流</returns>
		public static byte[] RsaEncrypt(byte[] data, string signKey, string encryptKey, Encoding encoding) {
			if (encoding == null) {
				encoding = Encoding.UTF8;
			}
			using (var stream = new MemoryStream()) {
				stream.Write(data, 0, data.Length);
				return RsaEncryptBase(stream, signKey, encryptKey, encoding);
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

		internal static byte[] RsaEncryptBase(MemoryStream stream, string signKey, string encryptKey, Encoding encoding) {
			if (encoding == null) {
				encoding = Encoding.UTF8;
			}
			var hashAlgorithm = MD5.Create();
			var encryptKeyBytes = hashAlgorithm.ComputeHash(encoding.GetBytes(encryptKey));
			return RsaEncryptBase(stream, signKey, encryptKeyBytes, hashAlgorithm);
		}

		#endregion

		#region RsaDecrypt

		public static byte[] RsaDecrypt(byte[] data, string signKey, string encryptKey, Encoding encoding) {
			if(encoding == null) {
				encoding = Encoding.UTF8;
			}
			var hashAlgorithm = MD5.Create();
			var encryptKeyBytes = hashAlgorithm.ComputeHash(encoding.GetBytes(encryptKey));
			return RsaDecryptBase(data, signKey, encryptKeyBytes, hashAlgorithm);
		}

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

		#endregion
	
		#endregion

		/// <summary>
		/// 加密
		/// </summary>
		/// <param name="data">被加密的数据</param>
		/// <param name="validateKey">验证密钥</param>
		/// <param name="encryptKey">加密密钥</param>
		/// <param name="encoding">编码</param>
		/// <returns>加密后的字节数组</returns>
		public static byte[] Encrypt(byte[] data, string validateKey, string encryptKey, Encoding encoding) {
			if (encoding == null) {
				encoding = Encoding.UTF8;
			}
			var hashAlgorithm = MD5.Create();
			var validateKeyBytes = hashAlgorithm.ComputeHash(encoding.GetBytes(validateKey));
			var encryptKeyBytes = hashAlgorithm.ComputeHash(encoding.GetBytes(encryptKey));
			return Encrypt(data, validateKeyBytes, encryptKeyBytes, hashAlgorithm);
		}

		internal static byte[] Encrypt(byte[] data, byte[] validateKey, byte[] encryptKey, HashAlgorithm hashAlgorithm) {
			using(var stream = new MemoryStream()) {
				var length = data.Length;
				stream.Write(data, 0, length);
				stream.Write(validateKey, 0, validateKey.Length);
				var hash = hashAlgorithm.ComputeHash(stream.ToArray());
				stream.SetLength(length);
				stream.Write(hash, 0, hash.Length);
				return XXTEA.Encrypt(stream.ToArray(), encryptKey);
			}
		}

		/// <summary>
		/// 解密
		/// </summary>
		/// <param name="data">被解密的字节数组</param>
		/// <param name="validateKey">验证密钥</param>
		/// <param name="encryptKey">解密密钥</param>
		/// <param name="encoding">编码</param>
		/// <returns>解密的字节数组</returns>
		public static byte[] Decrypt(byte[] data, string validateKey, string encryptKey, Encoding encoding) {
			if(data == null || data.Length <= 0) {
				return null;
			}
			if (encoding == null) {
				encoding = Encoding.UTF8;
			}
			var hashAlgorithm = MD5.Create();
			var validateKeyBytes = hashAlgorithm.ComputeHash(encoding.GetBytes(validateKey));
			var encryptKeyBytes = hashAlgorithm.ComputeHash(encoding.GetBytes(encryptKey));
			return Decrypt(data, validateKeyBytes, encryptKeyBytes, hashAlgorithm, 16);
		}

		internal static byte[] Decrypt(byte[] data, byte[] validateKey, byte[] encryptKey, HashAlgorithm hashAlgorithm, int hashReultLength) {
			try {
				var input = XXTEA.Decrypt(data, encryptKey);
				var hash = new byte[hashReultLength];
				Buffer.BlockCopy(input, input.Length - hashReultLength, hash, 0, hashReultLength);
				using(var stream = new MemoryStream()) {
					stream.Write(input, 0, input.Length - hashReultLength);
					var length = stream.Length;
					stream.Write(validateKey, 0, validateKey.Length);
					var validating = hashAlgorithm.ComputeHash(stream.ToArray());
					if(BitEquals(hash, validating)) {
						stream.Position = 0;
						stream.SetLength(length);
						return stream.ToArray();
					}
				}
			} catch { }
			return null;
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
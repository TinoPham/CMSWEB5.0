using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Cryptography
{
	public class Rijndael
	{
		

		private static void SetupRijndael(RijndaelManaged Rijndael, int keysize = Utils.KEY_SIZE_256, int blocksize = Utils.BLOCK_SIZE_256)
		{
			Rijndael.KeySize = keysize;
			Rijndael.BlockSize = blocksize;
		}
		public static string DefaultEncryptStringToBase64(string plainText)
		{
			byte[] key = DefaultKey();
			byte [] iv = DefaultIV();
			return EncryptStringToBase64( plainText, key, iv);
		}

		public static string EncryptStringToBase64(string plainText, byte [] Key, byte [] IV)
		{
			if( string.IsNullOrEmpty(plainText))
				return plainText;
			byte[]buff = EncryptStringToBytes( plainText, Key, IV);
			if( buff == null || buff.Length == 0)
				return null;
			return Convert.ToBase64String(buff);

		}

		public static string DefaultDecryptStringFromBase64(string base64)
		{
			byte [] key = DefaultKey();
			byte [] iv = DefaultIV();
			return DecryptStringFromBase64(base64, key, iv);
		}

		public static string DecryptStringFromBase64(string base64, byte [] Key, byte [] IV)
		{
			if( string.IsNullOrEmpty( base64))
				return null;
			try
			{
				byte [] encryptedBytes = Convert.FromBase64String(base64);
				return DecryptStringFromBytes(encryptedBytes, Key, IV); 
			}
			catch(Exception )
			{
				return null;
			}

		}
		
		public static byte [] DefaultEncryptStringToBytes(string plainText)
		{
			byte [] key = DefaultKey();
			byte [] iv = DefaultIV();
			return EncryptStringToBytes(plainText, key, iv);
		}

		public static byte [] EncryptStringToBytes(string plainText, byte [] Key, byte [] IV)
		{
			// Check arguments.
			if (plainText == null || plainText.Length <= 0)
				throw new ArgumentNullException("plainText");
			if (Key == null || Key.Length <= 0)
				throw new ArgumentNullException("Key");
			if (IV == null || IV.Length <= 0)
				throw new ArgumentNullException("Key");
			byte [] encrypted;
			// Create an RijndaelManaged object
			// with the specified key and IV.
			using (RijndaelManaged rijAlg = new RijndaelManaged())
			{
				 SetupRijndael(rijAlg);
				rijAlg.Key = Key;
				rijAlg.IV = IV;

				// Create a decrytor to perform the stream transform.
				ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

				// Create the streams used for encryption.
				using (MemoryStream msEncrypt = new MemoryStream())
				{
					using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
					{
						using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
						{

							//Write all data to the stream.
							swEncrypt.Write(plainText);
						}
						encrypted = msEncrypt.ToArray();
					}
				}
			}


			// Return the encrypted bytes from the memory stream.
			return encrypted;

		}

		public static string DefaultDecryptStringFromBytes(byte [] cipherText)
		{
			byte [] key = DefaultKey();
			byte [] iv = DefaultIV();
			return DecryptStringFromBytes( cipherText, key, iv);
		}
		public static string DecryptStringFromBytes(byte [] cipherText, byte [] Key, byte [] IV)
		{
			// Check arguments.
			if (cipherText == null || cipherText.Length <= 0)
				throw new ArgumentNullException("cipherText");
			if (Key == null || Key.Length <= 0)
				throw new ArgumentNullException("Key");
			if (IV == null || IV.Length <= 0)
				throw new ArgumentNullException("Key");

			// Declare the string used to hold
			// the decrypted text.
			string plaintext = null;

			// Create an RijndaelManaged object
			// with the specified key and IV.
			using (RijndaelManaged rijAlg = new RijndaelManaged())
			{
				SetupRijndael(rijAlg);
				rijAlg.Key = Key;
				rijAlg.IV = IV;

				// Create a decrytor to perform the stream transform.
				ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

				// Create the streams used for decryption.
				using (MemoryStream msDecrypt = new MemoryStream(cipherText))
				{
					using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
					{
						using (StreamReader srDecrypt = new StreamReader(csDecrypt))
						{

							// Read the decrypted bytes from the decrypting stream
							// and place them in a string.
							plaintext = srDecrypt.ReadToEnd();
						}
					}
				}

			}

			return plaintext;

		}

		private static byte[]DefaultKey()
		{
			string skey = Utils.EmbededResourceString(Utils.Rijndael_KEY_DEFAULT);
			return Convert.FromBase64String(skey);
			
		}
		private static byte [] DefaultIV()
		{
			string skey = Utils.EmbededResourceString(Utils.Rijndael_IV_DEFAULT);
			return Convert.FromBase64String(skey);

		}
	}
}

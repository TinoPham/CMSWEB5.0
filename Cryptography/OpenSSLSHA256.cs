using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Cryptography
{
	public class OpenSSLSHA256
	{

		public static string OpenSSLEncrypt(string plainText, string passphrase, string str_salt = null)
		{
			try
			{
				// generate salt
				byte [] key, iv;
				byte [] salt = new byte [Utils.SALT_LEN_8_BYTES];
				RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
				rng.GetNonZeroBytes(salt);
				DeriveKeyAndIV(passphrase, salt, out key, out iv);
				// encrypt bytes
				byte [] encryptedBytes = EncryptStringToBytesAes(plainText, key, iv);
				// add salt as first 8 bytes
				bool is_default_salt = string.IsNullOrEmpty(str_salt);
				int str_salt_len = is_default_salt ? Utils.SALT_LEN_8_BYTES : str_salt.Length;

				byte [] encryptedBytesWithSalt = new byte [str_salt_len  + salt.Length + encryptedBytes.Length];
				Buffer.BlockCopy(Encoding.ASCII.GetBytes(is_default_salt ? "Salted__" : str_salt), 0, encryptedBytesWithSalt, 0, str_salt_len);
				Buffer.BlockCopy(salt, 0, encryptedBytesWithSalt, str_salt_len, salt.Length);
				Buffer.BlockCopy(encryptedBytes, 0, encryptedBytesWithSalt, salt.Length + str_salt_len, encryptedBytes.Length);
				// base64 encode
				return Convert.ToBase64String(encryptedBytesWithSalt);
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static string OpenSSLDecrypt(string encrypted, string passphrase, string str_salt = null)
		{
			try
			{
				// base 64 decode
				bool is_default_salt = string.IsNullOrEmpty(str_salt);
				int str_salt_len = is_default_salt ? Utils.SALT_LEN_8_BYTES : str_salt.Length;

				byte [] encryptedBytesWithSalt = Convert.FromBase64String(encrypted);
				// extract salt (first 8 bytes of encrypted)
				byte [] salt = new byte [Utils.SALT_LEN_8_BYTES];
				byte [] encryptedBytes = new byte [encryptedBytesWithSalt.Length - salt.Length - str_salt_len];
				Buffer.BlockCopy(encryptedBytesWithSalt, str_salt_len, salt, 0, salt.Length);
				Buffer.BlockCopy(encryptedBytesWithSalt, salt.Length + str_salt_len, encryptedBytes, 0, encryptedBytes.Length);
				// get key and iv
				byte [] key, iv;
				DeriveKeyAndIV(passphrase, salt, out key, out iv);
				return DecryptStringFromBytesAes(encryptedBytes, key, iv);
			}
			catch (Exception)
			{
				return null;
			}
		}

		#region private
		static void DeriveKeyAndIV(string passphrase, byte [] salt, out byte [] key, out byte [] iv)
		{
			// generate key and iv
			int key_len_byte = Utils.KEY_SIZE_256/8;
			int iv_len_byte = Utils.IV_LEN_128/8;
			int key_iv_len = key_len_byte + iv_len_byte; //KEY_LEN + IV_LEN;
			List<byte> concatenatedHashes = new List<byte>(key_iv_len);

			byte [] password = Encoding.UTF8.GetBytes(passphrase);
			byte [] currentHash = new byte [0];
			MD5 md5 = MD5.Create();
			bool enoughBytesForKey = false;
			// See http://www.openssl.org/docs/crypto/EVP_BytesToKey.html#KEY_DERIVATION_ALGORITHM
			while (!enoughBytesForKey)
			{
				int preHashLength = currentHash.Length + password.Length + salt.Length;
				byte [] preHash = new byte [preHashLength];

				Buffer.BlockCopy(currentHash, 0, preHash, 0, currentHash.Length);
				Buffer.BlockCopy(password, 0, preHash, currentHash.Length, password.Length);
				Buffer.BlockCopy(salt, 0, preHash, currentHash.Length + password.Length, salt.Length);

				currentHash = md5.ComputeHash(preHash);
				concatenatedHashes.AddRange(currentHash);

				if (concatenatedHashes.Count >= key_iv_len)
					enoughBytesForKey = true;
			}

			key = new byte [key_len_byte];
			iv = new byte [iv_len_byte];
			//concatenatedHashes.CopyTo(0, key, 0, 32);
			concatenatedHashes.CopyTo(0, key, 0, key_len_byte);
			//concatenatedHashes.CopyTo(32, iv, 0, 16);
			concatenatedHashes.CopyTo(key_len_byte, iv, 0, iv_len_byte);

			md5.Clear();
			md5 = null;
		}

		public static byte [] EncryptStringToBytesAes(string plainText, byte [] key, byte [] iv)
		{
			// Check arguments.
			if (plainText == null || plainText.Length <= 0)
				throw new ArgumentNullException("plainText");
			if (key == null || key.Length <= 0)
				throw new ArgumentNullException("key");
			if (iv == null || iv.Length <= 0)
				throw new ArgumentNullException("iv");

			// Declare the stream used to encrypt to an in memory
			// array of bytes.
			MemoryStream msEncrypt;

			// Declare the RijndaelManaged object
			// used to encrypt the data.
			RijndaelManaged aesAlg = null;

			try
			{
				// Create a RijndaelManaged object
				// with the specified key and IV.
				//aesAlg = new RijndaelManaged { Mode = CipherMode.CBC, KeySize = 256, BlockSize = 128, Key = key, IV = iv };
				aesAlg = new RijndaelManaged { Mode = CipherMode.CBC, KeySize = key.Length * 8, BlockSize = iv.Length * 8, Key = key, IV = iv };

				// Create an encryptor to perform the stream transform.
				ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

				// Create the streams used for encryption.
				msEncrypt = new MemoryStream();
				using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
				{
					using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
					{

						//Write all data to the stream.
						swEncrypt.Write(plainText);
						swEncrypt.Flush();
						swEncrypt.Close();
					}
				}
			}
			finally
			{
				// Clear the RijndaelManaged object.
				if (aesAlg != null)
					aesAlg.Clear();
			}

			// Return the encrypted bytes from the memory stream.
			return msEncrypt.ToArray();
		}

		public static string DecryptStringFromBytesAes(byte [] cipherText, byte [] key, byte [] iv)
		{
			// Check arguments.
			if (cipherText == null || cipherText.Length <= 0)
				throw new ArgumentNullException("cipherText");
			if (key == null || key.Length <= 0)
				throw new ArgumentNullException("key");
			if (iv == null || iv.Length <= 0)
				throw new ArgumentNullException("iv");

			// Declare the RijndaelManaged object
			// used to decrypt the data.
			RijndaelManaged aesAlg = null;

			// Declare the string used to hold
			// the decrypted text.
			string plaintext = null;

			try
			{
				// Create a RijndaelManaged object
				// with the specified key and IV.
				//aesAlg = new RijndaelManaged { Mode = CipherMode.CBC, KeySize = 256, BlockSize = 128, Key = key, IV = iv };
				aesAlg = new RijndaelManaged { Mode = CipherMode.CBC,  KeySize = key.Length * 8, BlockSize = iv.Length * 8, Key = key, IV = iv };

				// Create a decrytor to perform the stream transform.
				ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
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
							srDecrypt.Close();
						}
					}
				}
			}
			
			finally
			{
				// Clear the RijndaelManaged object.
				if (aesAlg != null)
					aesAlg.Clear();
			}

			return plaintext;
		}
		#endregion

	}
}

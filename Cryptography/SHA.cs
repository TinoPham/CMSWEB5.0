using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Cryptography
{
	public sealed class SHA
	{

		public static byte [] ComputeHash(byte [] plainTextBytes, byte [] saltBytes, Utils.SHA_SIZE hashsize = Utils.SHA_SIZE.SHA_512)
		{
			if(plainTextBytes == null)
				return null;

			// If salt is not specified, generate it.
			if (saltBytes == null)
				saltBytes = GetRamdomSaltBytes();

			// Allocate array, which will hold plain text and salt.
			byte [] plainTextWithSaltBytes = new byte [plainTextBytes.Length + saltBytes.Length];
			//copy plain bytes to last buffer
			Buffer.BlockCopy(plainTextBytes, 0, plainTextWithSaltBytes, 0, plainTextBytes.Length);
			Buffer.BlockCopy(saltBytes, 0, plainTextWithSaltBytes, plainTextBytes.Length, saltBytes.Length);

			HashAlgorithm hash = GetHashAlg(hashsize);

			// Compute hash value of our plain text with appended salt.
			byte [] hashBytes = hash.ComputeHash(plainTextWithSaltBytes);

			return hashBytes;

		}

		public static bool VerifyHash(byte[] plainText, byte[] salttext, byte[] hashValue, Utils.SHA_SIZE hashsize = Utils.SHA_SIZE.SHA_512)
		{

			// Convert base64-encoded hash value into a byte array.
			// We must know size of hash (without salt).
			int hashSizeInBits, hashSizeInBytes;
			hashSizeInBits = HashSizeInBits(hashsize);
			// Convert size of hash from bits to bytes.
			hashSizeInBytes = hashSizeInBits / 8;

			// Make sure that the specified hash value is long enough.
			if (hashValue.Length < hashSizeInBytes)
				return false;


			// Compute a new hash string.
			
			byte[] expectedHashbytes = ComputeHash(plainText, salttext, hashsize);

			// If the computed hash matches the specified hash,
			// the plain text value must be correct.
			return expectedHashbytes.SequenceEqual(hashValue);
		}

		public static string ComputeHash(string plainText, string saltbase64, Utils.SHA_SIZE hashsize = Utils.SHA_SIZE.SHA_512)
		{
			// Convert plain text into a byte array.
			byte [] plainTextBytes = Encoding.UTF8.GetBytes(plainText == null? string.Empty : plainText);
			byte[] saltBytes = string.IsNullOrEmpty(saltbase64) ? null : Convert.FromBase64String(saltbase64) ;
			byte [] hashBytes = ComputeHash(plainTextBytes, saltBytes, hashsize);
			string hashValue = Convert.ToBase64String(hashBytes);
			return hashValue;
		}

		public static bool VerifyHash(string plainText, string saltbase64, string hashValue, Utils.SHA_SIZE hashsize = Utils.SHA_SIZE.SHA_512)
		{

			// Convert base64-encoded hash value into a byte array.
			byte [] hashWithSaltBytes = Convert.FromBase64String(hashValue);
			byte [] plainTextBytes = Encoding.UTF8.GetBytes(plainText == null? string.Empty : plainText);
			byte[] saltBytes = string.IsNullOrEmpty(saltbase64) ? null : Convert.FromBase64String(saltbase64);
			return VerifyHash(plainTextBytes, saltBytes, hashWithSaltBytes, hashsize);
		}

		private static byte[] GetRamdomSaltBytes()
		{
			byte[] saltBytes = null;
			// Define min and max salt sizes.
			const int minSaltSize = 4;
			const int maxSaltSize = 8;

			// Generate a random number for the size of the salt.
			Random random = new Random();
			int saltSize = random.Next(minSaltSize, maxSaltSize);

			// Allocate a byte array, which will hold the salt.
			saltBytes = new byte [saltSize];

			// Initialize a random number generator.
			RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

			// Fill the salt with cryptographically strong byte values.
			rng.GetNonZeroBytes(saltBytes);

			return saltBytes;
		}

		private static int HashSizeInBits(Utils.SHA_SIZE shasize = Utils.SHA_SIZE.SHA_512)
		{
			int hashsizeinbits;
			switch (shasize)
			{
				case Utils.SHA_SIZE.SHA_256:
					hashsizeinbits = 256;
					break;
				case Utils.SHA_SIZE.SHA_384:
					hashsizeinbits = 384;
					break;
				case Utils.SHA_SIZE.SHA_512:
					hashsizeinbits = 512;
					break;
				default:
					hashsizeinbits = 128;
					break;
			}
			return hashsizeinbits;
		}

		private static HashAlgorithm GetHashAlg( Utils.SHA_SIZE shasize = Utils.SHA_SIZE.SHA_512)
		{
			HashAlgorithm sha = null;
			switch( shasize)
			{
				case Utils.SHA_SIZE.SHA_256:
					sha = new SHA256Managed() ;
				break;
				case Utils.SHA_SIZE.SHA_384:
					sha = new SHA384Managed();
				break;
				case Utils.SHA_SIZE.SHA_512:
					sha = new SHA512Managed();
				break;
				default:
					sha = new MD5CryptoServiceProvider();
				break;
			}
			return sha;
		}
	}
}

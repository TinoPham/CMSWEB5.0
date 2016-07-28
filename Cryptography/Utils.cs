using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Security;

namespace Cryptography
{
	public static class Utils
	{
		internal const int KEY_SIZE_256 = 256;
		internal const int BLOCK_SIZE_256 = 256;
		internal const int SALT_LEN_8_BYTES = 8;
		//const int KEY_LEN = 32;// 32 * 8 = 256 bits
		internal const int IV_LEN_128 = 128;//16 *8= 128 bits
		internal const int RSA_KEY_LEN = 2048;
		internal const string RSA_KEY_DEFAULT_PRIVATE = "Cryptography.Keys.RSAPrivate.txt";
		internal const string RSA_KEY_DEFAULT_PUBLIC = "Cryptography.Keys.RSAPublic.txt";
		internal const string Rijndael_KEY_DEFAULT = "Cryptography.Keys.Rijndael.key";
		internal const string Rijndael_IV_DEFAULT = "Cryptography.Keys.Rijndael.iv";

		public enum SHA_SIZE : int
		{
			SHA_256,
			SHA_384,
			SHA_512
		}

		public static string GeneratePassword( int length, int numberOfNonAlphanumericCharacters)
		{
			return Membership.GeneratePassword( length, numberOfNonAlphanumericCharacters);
		}
	
		public static byte[] GetRandomBytes(int len)
		{
			if( len <= 0)
				return null;
		   using(RNGCryptoServiceProvider _generator = new RNGCryptoServiceProvider())
		   {
				byte[]ret = new byte[len];
				_generator.GetNonZeroBytes( ret);
				return ret;
		   }
		}
		
		public static void GenerateKeyIV(out byte [] Key, out byte [] IV, int keysize = Utils.KEY_SIZE_256, int blocksize = Utils.BLOCK_SIZE_256)
		{
			using (RijndaelManaged Rijndael = new RijndaelManaged())
			{
				Rijndael.KeySize = CheckValidSize(Rijndael.LegalKeySizes.First(), keysize); //keysize;
				Rijndael.BlockSize = CheckValidSize(Rijndael.LegalBlockSizes.First(), blocksize); //blocksize;

				Rijndael.GenerateKey();
				Rijndael.GenerateIV();
				Key = Rijndael.Key;
				IV = Rijndael.IV;
			}
		}

		public static int CheckValidSize( KeySizes size, int value )
		{
			if( size == null)
				return 0;
			int ret = 0;
			ret = ValidSize( size.MinSize, size.MaxSize, value);
			if( ret > 0)
				return ret;

			while( value < size.MaxSize && ret == 0)
			{
				value += size.SkipSize;
				ret = ValidSize(size.MinSize, size.MaxSize, value);
			}

			return ret;
			
		}
	
		private static int ValidSize( int val_min, int val_max, int value )
		{
			if (value <= val_min)
				return val_min;

			if (value >= val_max)
				return val_max;

			return 0;
		}

		public static int Between(int minimumValue, int maximumValue)
        {
            byte[] randomNumber = new byte[1];
 
			using(RNGCryptoServiceProvider _generator = new RNGCryptoServiceProvider())
			{
				_generator.GetBytes(randomNumber);
 
				double asciiValueOfRandomCharacter = Convert.ToDouble(randomNumber[0]);
 
            // We are using Math.Max, and substracting 0.00000000001, 
            // to ensure "multiplier" will always be between 0.0 and .99999999999
            // Otherwise, it's possible for it to be "1", which causes problems in our rounding.
				double multiplier = Math.Max(0, (asciiValueOfRandomCharacter / 255d) - 0.00000000001d);
 
            // We need to add one to the range, to allow for the rounding done with Math.Floor
				int range = maximumValue - minimumValue + 1;
 
				double randomValueInRange = Math.Floor(multiplier * range);
	 
		        return (int)(minimumValue + randomValueInRange);
			}
    }

		internal static Stream EmbededResourceStream( string ResourceName)
		{
			try
			{
				Stream resourceStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName);
				return resourceStream;
			}
			catch(Exception)
			{ return null;}
		}

		internal static string EmbededResourceString(string ResourceName)
		{
			byte[]buff = EmbededResourceByte(ResourceName);
			if( buff == null || buff.Length == 0)
				return null;
			return Encoding.UTF8.GetString( buff);
		}

		internal static byte [] EmbededResourceByte(string ResourceName)
		{
			Stream stream = EmbededResourceStream(ResourceName); 
			if( stream == null || !stream.CanRead)
				return null;
			if( stream.CanSeek)
				stream.Seek( 0, SeekOrigin.Begin);
			byte[]buff = new byte[stream.Length];
			stream.Read( buff, 0, buff.Length);
			stream.Close();
			stream.Dispose();
			stream = null;
			return buff;
		}
	}
}

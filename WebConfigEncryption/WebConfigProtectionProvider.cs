using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WebConfigEncryption
{
	public class WebConfigProtectionProvider : ProtectedConfigurationProvider, IDisposable
	{
		private const string EncryptedData_Begin_Tag = "<EncryptedData>";
		private const string EncryptedData_End_Tag = "</EncryptedData>";
		private const string Default_Ecrypt_Key_Path = "WebConfigEncryption.DefaultEncryptionKey.txt";
		private const string PC_Encrypt_Key_FileName = "EnctKey.txt";
		private const string Key_Path = "KeyPath";

		#region Private variables
		/// <summary>
		/// algorithm provider 
		/// </summary>
		private SymmetricAlgorithm provider = new RijndaelManaged(); //new TripleDESCryptoServiceProvider();
		//private SymmetricAlgorithm provider = new TripleDESCryptoServiceProvider();

		/// <summary>
		/// variable that gets the provider name
		/// </summary>
		private string providerName;
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the WebConfigProtectionProvider class
		/// TripleDESCryptoServiceProvider will use to generate key
		/// </summary>
		public WebConfigProtectionProvider()
		{
		}

		/// <summary>
		/// Initializes a new instance of the WebConfigProtectionProvider class with symmetric algorithm
		/// </summary>
		/// <param name="symmetricAlgorithm">symmetric algorithm</param>
		public WebConfigProtectionProvider(SymmetricAlgorithm symmetricAlgorithm)
		{
			this.provider = symmetricAlgorithm;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Gets the provider name
		/// </summary>
		public override string Name
		{
			get { return this.providerName; }
		}

		public string KeyPath{ get ;set;}
		#endregion

		#region Public methods
		/// <summary>
		/// Performs provider initialization.
		/// </summary>
		/// <param name="name">name of the provider</param>
		public override void Initialize(string name, NameValueCollection config)
		{
			if( config.HasKeys())
			{
				string keypath = config.AllKeys.FirstOrDefault( item =>  string.Compare( item, Key_Path, true) == 0);
				this.KeyPath = string.IsNullOrEmpty( keypath)? null : config.Get(keypath);
			}
			this.providerName = name;
			this.ReadKey( this.KeyPath);
		}

		/// <summary>
		/// Performs encryption. 
		/// </summary>
		/// <param name="node">xml node to be encrypted</param>
		/// <returns>encrypted xml node</returns>
		public override XmlNode Encrypt(XmlNode node)
		{
			string encryptedData = this.EncryptString(node.OuterXml);
			XmlDocument xmlDoc = new XmlDocument();
			
			xmlDoc.PreserveWhitespace = true;
			xmlDoc.LoadXml( EncryptedData_Begin_Tag + encryptedData + EncryptedData_End_Tag);
			return xmlDoc.DocumentElement;
		}

		/// <summary>
		/// Performs decryption.
		/// </summary>
		/// <param name="encryptedNode">encrypted xml node</param>
		/// <returns>decrypted xml node</returns>
		public override XmlNode Decrypt(XmlNode encryptedNode)
		{
			string decryptedData = this.DecryptString(encryptedNode.InnerText);

			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.PreserveWhitespace = true;
			xmlDoc.LoadXml(decryptedData);

			return xmlDoc.DocumentElement;
		}

		/// <summary>
		/// Generates a new TripleDES key and vector and writes them to the supplied file path. 
		/// </summary>
		/// <param name="filePath">file path of the encryption key</param>
		public void CreateKey(string filePath)
		{
			this.provider.GenerateKey();
			this.provider.GenerateIV();

			string encodeKey = Encode(this.provider.Key);
			string encodeIV = Encode(this.provider.IV);

			using (StreamWriter sw = new StreamWriter(filePath, false))
			{
				sw.WriteLine(encodeKey);
				sw.WriteLine(encodeIV);
			}
		}
		#endregion

		#region Private methods
		private void ResourceKey()
		{
			string resourceName = Default_Ecrypt_Key_Path;//"Cryptography.WebConfigEncryption.WebConfigEncryptionKey.txt";
			using (var resourceStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
			{
				if (resourceStream != null)
				{
					using (var textStreamReader = new StreamReader(resourceStream))
					{
						UpdateKey(textStreamReader, this.provider);
					}
				}
			}
		}

		///// <summary>
		///// Reads in the TripleDES key and vector from  the supplied key file path and sets the Key and IV properties of the TripleDESCryptoServiceProvider.
		///// </summary>  
		//private void ReadKey()
		//{
		//	string resourceName = Default_Ecrypt_Key_Path;//"Cryptography.WebConfigEncryption.WebConfigEncryptionKey.txt";
		//	using (var resourceStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
		//	{
		//		if (resourceStream != null)
		//		{
		//			using (var textStreamReader = new StreamReader(resourceStream))
		//			{
		//				string key = textStreamReader.ReadLine();
		//				string iv = textStreamReader.ReadLine();
		//				byte [] bkey = Decode(key);
		//				byte [] biv = Decode(iv);
		//				provider.BlockSize = biv.Length * 8;
		//				provider.KeySize = bkey.Length * 8;
		//				this.provider.Key = Decode(key);
		//				this.provider.IV = Decode(iv);
		//			}
		//		}
		//		else
		//		{
		//			CreateKey(PC_Encrypt_Key_FileName);
		//			ReadKey(PC_Encrypt_Key_FileName);
		//		}
		//	}
		//}

		/// <summary>
		/// Reads in the TripleDES key and vector from  the supplied key file path and sets the Key and IV properties of the TripleDESCryptoServiceProvider.
		/// </summary>
		/// <param name="filepath">key file path</param>
		private void ReadKey(string filepath)
		{
			if( string.IsNullOrEmpty( filepath) || File.Exists(filepath))
				 ResourceKey();
			else {
				using (var textStreamReader = new StreamReader(filepath))
				{
					UpdateKey(textStreamReader, this.provider);
				}
			}
		}
		private void UpdateKey( StreamReader reader, SymmetricAlgorithm algorithm)
		{
			int bsize = 0, ksize = 0;
			string key = reader.ReadLine();
			string iv = reader.ReadLine();
			byte [] bkey = Decode(key);
			byte [] biv = Decode(iv);
			bsize = CheckValidSize(provider.LegalBlockSizes.First(), bsize);
			ksize = CheckValidSize(provider.LegalKeySizes.First(), ksize);
			algorithm.BlockSize = bsize;
			algorithm.KeySize = ksize;
			algorithm.Key = bkey.Take(ksize / 8).ToArray(); //Decode(key);
			algorithm.IV = biv.Take(bsize / 8).ToArray(); //Decode(iv);

		}
		/// <summary>
		/// Encodes byte array input to string 
		/// </summary>
		/// <param name="input">byte array</param>
		/// <returns>base 64 string</returns>
		private string Encode(byte [] input)
		{
			return Convert.ToBase64String(input);
		}

		/// <summary>
		/// Decodes string input to byte array
		/// </summary>
		/// <param name="input">base 64 string input</param>
		/// <returns>byte array</returns>
		private byte [] Decode(string input)
		{
			return Convert.FromBase64String(input);
		}

		/// <summary>
		/// Encrypts a configuration section and returns the encrypted XML as a string. 
		/// </summary>
		/// <param name="encryptValue">encrypted value</param>
		/// <returns>encrypted string</returns>
		private string EncryptString(string encryptValue)
		{
			byte [] valBytes = Encoding.Unicode.GetBytes(encryptValue);

			ICryptoTransform transform = this.provider.CreateEncryptor();

			byte [] returnBytes;

			using (MemoryStream ms = new MemoryStream())
			{
				using (CryptoStream cs = new CryptoStream(ms, transform, CryptoStreamMode.Write))
				{
					cs.Write(valBytes, 0, valBytes.Length);
					cs.FlushFinalBlock();
					returnBytes = ms.ToArray();
				}
			}

			return Convert.ToBase64String(returnBytes);
		}

		/// <summary>
		/// Decrypts an encrypted configuration section and returns the unencrypted XML as a string. 
		/// </summary>
		/// <param name="encryptedValue">encrypted value</param>
		/// <returns>decrypted string</returns>
		private string DecryptString(string encryptedValue)
		{
			byte [] valBytes = Convert.FromBase64String(encryptedValue);

			ICryptoTransform transform = this.provider.CreateDecryptor();

			byte [] returnBytes;

			using (MemoryStream ms = new MemoryStream())
			{
				using (CryptoStream cs = new CryptoStream(ms, transform, CryptoStreamMode.Write))
				{
					cs.Write(valBytes, 0, valBytes.Length);
					cs.FlushFinalBlock();
					returnBytes = ms.ToArray();
				}
			}

			return Encoding.Unicode.GetString(returnBytes);
		}


		private int CheckValidSize(KeySizes size, int value)
		{
			if (size == null)
				return 0;
			int ret = 0;
			ret = ValidSize(size.MinSize, size.MaxSize, value);
			if (ret > 0)
				return ret;

			while (value < size.MaxSize && ret == 0)
			{
				value += size.SkipSize;
				ret = ValidSize(size.MinSize, size.MaxSize, value);
			}

			return ret;

		}
		private int ValidSize(int val_min, int val_max, int value)
		{
			if (value <= val_min)
				return val_min;

			if (value >= val_max)
				return val_max;

			return 0;
		}
		#endregion

		#region Dispose
		/// <summary>
		/// dispose the unused object from the class
		/// </summary>
		/// <param name="disposing">boolean value</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (this.provider != null)
				{
					this.provider.Dispose();
				}
			}
			// free native resources
		}

		/// <summary>
		/// implements the dispose of the IDisposable interface
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}

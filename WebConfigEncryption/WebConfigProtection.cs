using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.IO;

namespace WebConfigEncryption
{
	public class WebConfigProtection : IDisposable
	{
		#region Members
		/// <summary>
		/// configuration provider private variable
		/// </summary>
		private System.Configuration.Provider.ProviderBase configurationProvider;

		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the WebConfigProtection class
		/// </summary>
		public WebConfigProtection()
		{
			this.configurationProvider = new WebConfigProtectionProvider();
		}

		/// <summary>
		/// Initializes a new instance of the WebConfigProtection class
		/// </summary>
		/// <param name="configurationProvider">configuration Provider</param>
		public WebConfigProtection(ProtectedConfigurationProvider configurationProvider)
		{
			this.configurationProvider = configurationProvider;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Gets or sets the Configuration Provider object
		/// </summary>
		public ProtectedConfigurationProvider ConfigurationProvider { get; set; }

		#endregion

		#region Public methods

		/// <summary>
		/// creates symmetric key at the specified path
		/// </summary>
		/// <param name="symmetricAlgorithm">algorithm used for encryption</param>
		/// <param name="keyFilePath">path where the key needs to be stored</param>
		public virtual void CreateSymmetricKey(SymmetricAlgorithm symmetricAlgorithm, string keyFilePath)
		{
			keyFilePath = string.IsNullOrEmpty(keyFilePath) ? @"pckey.txt" : keyFilePath;
			new WebConfigProtectionProvider(symmetricAlgorithm).CreateKey(keyFilePath);
		}

		/// <summary>
		/// creates key for Triple DES provider algorithm
		/// </summary>
		/// <param name="keyFilePath">path where the key needs to be stored</param>
		public virtual void CreateSymmetricKey(string keyFilePath)
		{
			keyFilePath = string.IsNullOrEmpty(keyFilePath) ? @"pckey.txt" : keyFilePath;
			new WebConfigProtectionProvider().CreateKey(keyFilePath);
		}

		/// <summary>
		/// Protect the web config file by mentioning the provider name and web config file relative path
		/// </summary>
		/// <param name="sections">sections to be encrypted</param>
		/// <param name="providerName">provider that is responsible for encryption</param>
		/// <param name="webConfigPath">path of the web.config file</param>
		public void EncryptConfigFile(string [] sections, string providerName, string webConfigPath, bool isAppConfig = false)
		{
			try
			{
				// Get the application configuration file.
				System.Configuration.Configuration config;
				if (webConfigPath == "/")
				{
					config = WebConfigurationManager.OpenWebConfiguration(webConfigPath);
				}

				else if (isAppConfig == true)
				{
					config = ConfigurationManager.OpenExeConfiguration(webConfigPath);
				}

				else
				{
					VirtualDirectoryMapping vdm = new VirtualDirectoryMapping(webConfigPath, true);
					WebConfigurationFileMap wcfm = new WebConfigurationFileMap();
					wcfm.VirtualDirectories.Add("/", vdm);
					config = WebConfigurationManager.OpenMappedWebConfiguration(wcfm, "/");
				}

				// Define the provider name. 
				foreach (string section in sections)
				{
					ConfigurationSection objAppsettings = (ConfigurationSection)config.GetSection(section);
					if (objAppsettings != null)
					{
						if (!objAppsettings.SectionInformation.IsProtected)
						{
							if (!objAppsettings.ElementInformation.IsLocked)
							{
								objAppsettings.SectionInformation.ProtectSection(providerName);
								objAppsettings.SectionInformation.ForceSave = true;
								config.Save(ConfigurationSaveMode.Full);
							}
						}
					}
				}
			}
			catch (ConfigurationException ex)
			{
				WriteLog("Exception occurred while encryption using provider name " + ex.Message);
			}
			catch (Exception ex)
			{
				WriteLog("General Exception occurred " + ex.Message);
			}
		}

		/// <summary>
		/// Protects the web config file using the custom configuration provider
		/// </summary>
		/// <param name="sections">sections to be encrypted</param>
		/// <param name="configurationProvider">custom configuration provider</param>
		/// <param name="webConfigPath">path of the web config</param>
		public void EncryptConfigFile(string [] sections, System.Configuration.Provider.ProviderBase configurationProvider, string webConfigPath, bool isAppConfig = false)
		{
			try
			{
				webConfigPath = string.IsNullOrEmpty(webConfigPath) ? "/" : webConfigPath;
				System.Configuration.Configuration config;
				if (webConfigPath == "/")
				{
					config = WebConfigurationManager.OpenWebConfiguration(webConfigPath);
				}

				else if (isAppConfig == true)
				{
					config = ConfigurationManager.OpenExeConfiguration(webConfigPath);
				}

				else
				{
					VirtualDirectoryMapping vdm = new VirtualDirectoryMapping(webConfigPath, true);
					WebConfigurationFileMap wcfm = new WebConfigurationFileMap();
					wcfm.VirtualDirectories.Add("/", vdm);
					config = WebConfigurationManager.OpenMappedWebConfiguration(wcfm, "/");
				}

				foreach (string section in sections)
				{
					//// Define the provider name. 
					ConfigurationSection objAppsettings = (ConfigurationSection)config.GetSection(section);
					if (objAppsettings != null)
					{
						if (!objAppsettings.SectionInformation.IsProtected)
						{
							if (!objAppsettings.ElementInformation.IsLocked)
							{
								objAppsettings.SectionInformation.ProtectSection(configurationProvider.Name);
								objAppsettings.SectionInformation.ForceSave = true;
								config.Save(ConfigurationSaveMode.Full);
							}
						}
					}
				}
			}
			catch (ConfigurationException ex)
			{
				WriteLog("Exception occurred while encrypting using ProtectConfigurationProvider " + ex.Message);
			}
			catch (Exception ex)
			{
				WriteLog("General Exception occurred " + ex.Message);
			}
		}

		/// <summary>
		/// Unprotect the connectionStrings section.  
		/// </summary>
		/// <param name="sections">sections of the web config to be decrypted</param>
		/// <param name="webConfigPath">path of the web config</param>
		public void DecryptConfigFile(string [] sections, string webConfigPath, bool isAppConfig = false)
		{
			try
			{
				// Get the application configuration file.
				System.Configuration.Configuration config;
				if (webConfigPath == "/")
				{
					config = WebConfigurationManager.OpenWebConfiguration(webConfigPath);
				}

				else if (isAppConfig == true)
				{
					config = ConfigurationManager.OpenExeConfiguration(webConfigPath);
				}

				else
				{
					VirtualDirectoryMapping vdm = new VirtualDirectoryMapping(webConfigPath, true);
					WebConfigurationFileMap wcfm = new WebConfigurationFileMap();
					wcfm.VirtualDirectories.Add("/", vdm);
					config = WebConfigurationManager.OpenMappedWebConfiguration(wcfm, "/");
				}

				foreach (string section in sections)
				{
					// Get the section to unprotect.
					ConfigurationSection objAppsettings = (ConfigurationSection)config.GetSection(section);
					if (objAppsettings != null)
					{
						if (objAppsettings.SectionInformation.IsProtected)
						{
							if (!objAppsettings.ElementInformation.IsLocked)
							{
								// Unprotect the section.
								objAppsettings.SectionInformation.UnprotectSection();
								objAppsettings.SectionInformation.ForceSave = true;
								config.Save(ConfigurationSaveMode.Full);
							}
						}
					}
				}
			}
			catch (ConfigurationException ex)
			{
				WriteLog("Exception occurred while decrypting config file section " + ex.Message);
			}
			catch (Exception ex)
			{
				WriteLog("General Exception occurred " + ex.Message);
			}
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
				if (this.configurationProvider != null)
				{
					this.configurationProvider = null;
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
		private string LogFileName()
		{
			return Path.Combine("Logs", DateTime.Now.ToString("yyyy_MM_dd.txt"));
		}
		private void WriteLog( string  log)
		{
			//string fileName = LogFileName();
			//using (FileStream fs = new FileStream(fileName,FileMode.OpenOrCreate, FileAccess.Write))
			//using (StreamWriter sw = new StreamWriter(fs))
			//{
			//	fs.Seek(0, SeekOrigin.End);
			//	sw.WriteLine(log);
			//}
			
		}
		#endregion
	}
}

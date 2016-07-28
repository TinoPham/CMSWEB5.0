using System;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using Microsoft.Win32;
using System.Configuration.Install;
using System.Runtime.InteropServices;
using System.IO;
using System.Security.Principal;
using System.Windows;

namespace ServiceManager
{
	/// <summary>
	/// Summary description for Service.
	/// </summary>
	[RunInstaller(true)]
	public class Service : System.Configuration.Install.Installer
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		public Service()
		{
			///
			/// Required for Windows.Forms Class Composition Designer support
			///
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}
		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
		}
		#endregion
		//const string NT_USER_GROUP = "Users";
		//const string REG_PAC_PATH = @"SOFTWARE\PAC";
		//const string installDir = "D:\\PAC\\WebPAC";
		const string Convert_Service_Filename = "PACDMConverter.exe";
		const string Converter_Service_Name = PACDMConverter.Consts.STR_CONVERTER;// "PACDMConverter";
		const string STR_PACDMConverter = "PACDMConverter";
		//private string[] CondsFile={"msa2pg_srvc.exe","AKSHASP.DLL","Mono.Security.dll","Mono.Security.Protocol.Tls.dll","msa2pg.dll","Npgsql.DLL"};
		
		protected override void OnBeforeInstall(IDictionary savedState)
		{
			base.OnBeforeInstall (savedState);
			//check existed and Stop service 
			//StopService(Converter_Service_Name);
			APIService.StopService(Converter_Service_Name);
		}
		public override void Install(IDictionary stateSaver)
		{
			base.Install (stateSaver);
			bool is_silent = false;
			if( Context.Parameters.ContainsKey("silent"))
			{
				string str_silent = Context.Parameters["silent"];
				is_silent = (string.IsNullOrEmpty( str_silent) || str_silent == "0")? false : true;
				RegistryValue(Commons.ConstEnums.Registry_FirstRun_Key, is_silent == false? null : "1");
			}
			else
				RegistryValue(Commons.ConstEnums.Registry_FirstRun_Key, null);
			string path = Path.GetFullPath(Context.Parameters["path"]);
			//Set permission for  users
			FolderPermissions.SetFullControlFolder(path, WellKnownSidType.WorldSid);
			//FolderPermissions.SetFullControlReg(REG_PAC_PATH, WellKnownSidType.WorldSid);
			//Upgrade DB for POS Exception support 64 channel
			//UpgradeDatabase.CheckExcpetionChannelColumn(path);
			//Upgrade DB for POS Exception with contend any
			//UpgradeDatabase.UpgratePOSExceptionStructure(path);

			#region Install and set security for service
			string svr_path = Path.Combine(path, Convert_Service_Filename);
			int error_code = 0;
			APIService.UnInstallService(Converter_Service_Name);

			if ( (error_code = APIService.InstallService(svr_path, Converter_Service_Name)) == 0 )
			{
				//set sercurity for service
				if ((error_code = APIService.ChageServiceSecurity(Converter_Service_Name, WellKnownSidType.WorldSid, APIService.SERVICE_ACCESS.SERVICE_ALL_ACCESS)) == 0)
				{
					APIService.StartService(Converter_Service_Name);
				}
				else
				{
					if( !is_silent)
					MessageBox.Show(APIErrorCodes.GetAPIErrorCode(error_code) + " Please install software with administrator privilege.");
					//base.Rollback(stateSaver);
				}

			}
			else
			{
				if (!is_silent)
					MessageBox.Show("The SQL converter service cannot be installed.\n\r" + APIErrorCodes.GetAPIErrorCode( error_code));
				//base.Rollback(stateSaver);
			}
			#endregion
		}
		
		protected override void OnAfterInstall(IDictionary savedState)
		{
			base.OnAfterInstall(savedState);
		}

		protected override void OnBeforeUninstall(IDictionary savedState)
		{
			//System.ServiceProcess.ServiceController service = GetService(service_Name);
			//if(service != null)
			//    this.Stop(service);
			APIService.StopService(Converter_Service_Name);
			base.OnBeforeUninstall(savedState);
		}
		
		public override void Uninstall(IDictionary savedState)
		{
			APIService.StopService(Converter_Service_Name);
			APIService.UnInstallService(Converter_Service_Name);
			RegistryValue( Commons.ConstEnums.Registry_PACConverterVerOld, null);
			RegistryValue(Commons.ConstEnums.Registry_FirstRun_Key, null);
			RegistryValue("DomainSid", null);
			
			base.Uninstall(savedState);
		}

		private void RegistryValue(string valname, string val)
		{
			if (string.IsNullOrEmpty(val))
				PACDMConverter.Utils.Instance.DeleteRegValue(Microsoft.Win32.RegistryHive.LocalMachine, Commons.ConstEnums.Registry_PAC_Key, valname);
				//Commons.RegistryUtils.DeleteRegValue(Microsoft.Win32.RegistryHive.LocalMachine, Microsoft.Win32.RegistryView.Default, Commons.ConstEnums.Registry_PAC_Key, valname);
			else
				PACDMConverter.Utils.Instance.SetRegValue(Microsoft.Win32.RegistryHive.LocalMachine, Commons.ConstEnums.Registry_PAC_Key, valname, val);
				//Commons.RegistryUtils.SetRegValue(Microsoft.Win32.RegistryHive.LocalMachine, Microsoft.Win32.RegistryView.Default, Commons.ConstEnums.Registry_PAC_Key, valname, val);
		}

		public override void Rollback(IDictionary savedState)
		{
			base.Rollback(savedState);
			RegistryValue(Commons.ConstEnums.Registry_FirstRun_Key, null);
		}
	}
}

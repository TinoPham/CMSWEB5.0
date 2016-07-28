using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConvertMessage;
using System.Xml;
using System.IO;
using System.Runtime.InteropServices;

namespace PACDMConverter
{
	internal class CMSAgreement: IDisposable
	{
		
		const string FileName= "CMSAgreement.xml";
		const string str_agreement_state = "agreement_state";
		const string str_domain_name = "domain_name";
		const string str_company_name = "company_name";
		const string str_email_address = "email_address";
		const string str_pac_converter_port = "pac_converter_port";
		const string str_allow_connection = "allow_connection";
		
		public delegate void FileChangeEvent( Customer oldinfo, Customer newinfo, WatcherChangeTypes changetype);
		public event FileChangeEvent OnFileChangeEvent;
		private readonly string Dir;
		
		FileSystemWatcher FileWatcher;
		FileSystemWatcher DirWatcher;
		volatile byte Change_Count = 0;
		public Customer Info{ get ;private set;}
		
		public CMSAgreement( string dir)
		{
			Dir = dir;
			Info = Parser( Path.Combine(Dir, FileName));
		}
		public void Dispose()
		{
			StopWatcher( ref FileWatcher);
			StopWatcher(ref DirWatcher);
		}
		private void StopWatcher(ref FileSystemWatcher Watcher)
		{
			if (Watcher != null)
			{
				Watcher.EnableRaisingEvents = false;
				Watcher.Deleted -= FileWatcher_Deleted;
				Watcher.Renamed -= FileWatcher_Renamed;
				Watcher.Created -= FileWatcher_Created;
				Watcher.Dispose();
				Watcher = null;
			}
		}
		public void ChangeDomain( string newurl)
		{
			try
			{
				string fpath = Path.Combine( Dir, FileName);
				XmlDocument doc = Commons.XMLUtils.LoadXMLDocument(Path.Combine(Dir, FileName) );
				if( doc != null)
				{
					XmlNode root = doc.DocumentElement as XmlNode;
					if( root == null)
						return;
					XmlNode agreementnode = Commons.XMLUtils.SelectNode(root, str_agreement_state);
					if( agreementnode == null)
						return;
					XmlNode domain_node = Commons.XMLUtils.SelectNode(agreementnode, str_domain_name);
					if (domain_node != null)
					{
						domain_node.InnerText = newurl;
						doc.Save( fpath);
					}
				}
			} catch(Exception){}
		}

		public void InitWatcher()
		{
			DirWatcher = WatchConfigurationDir(Dir);
			InitFileWatcher();
		}
		private void InitFileWatcher()
		{
			if (Directory.Exists(Dir) && FileWatcher == null)
				FileWatcher = InitFileverionsWatcher(Dir, FileName);
		}
		private FileSystemWatcher WatchConfigurationDir(string path)
		{
			DirectoryInfo dinfo = new DirectoryInfo(path);
			string dirname = dinfo.Name;
			string dirpath = dinfo.Parent.FullName;
			FileSystemWatcher DirWatcher = new FileSystemWatcher(dirpath, dirname);
			DirWatcher.Deleted += FileWatcher_Deleted;
			DirWatcher.Renamed += FileWatcher_Renamed;
			DirWatcher.Created += FileWatcher_Created;
			DirWatcher.Changed += FileWatcher_Changed;
			DirWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size | NotifyFilters.LastWrite | NotifyFilters.DirectoryName;
			DirWatcher.IncludeSubdirectories = false;
			DirWatcher.EnableRaisingEvents = true;
			return DirWatcher;
			 
		}
		private FileSystemWatcher InitFileverionsWatcher(string path, string filename)
		{
			FileSystemWatcher FileWatcher = new FileSystemWatcher(path, filename);
			FileWatcher.Deleted += FileWatcher_Deleted;
			FileWatcher.Renamed += FileWatcher_Renamed;
			FileWatcher.Created += FileWatcher_Created;
			FileWatcher.Changed += FileWatcher_Changed;
			FileWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size | NotifyFilters.LastWrite | NotifyFilters.FileName;
			FileWatcher.IncludeSubdirectories = false;
			FileWatcher.EnableRaisingEvents = true;
			return FileWatcher;
		}
		
		private void RaiseChangeEvent(Customer newinfo, WatcherChangeTypes changetype)
		{
			if(OnFileChangeEvent != null)
			{
				if(Info != null && Info.CompareTo(newinfo) == 0)
					return;

				Customer oldinfo = Info == null? new Customer(): new Customer{ Domain = Info.Domain, Email = Info.Email, Name = Info.Name, Phone = Info.Phone, ConverterPort = Info.ConverterPort, AllowConnect = Info.AllowConnect};
				Info = newinfo;
				OnFileChangeEvent(oldinfo, newinfo, changetype);
			}
		}

		void FileWatcher_Changed(object sender, FileSystemEventArgs e)
		{
			if (string.Compare(e.Name, FileName, true) != 0 || e.ChangeType != WatcherChangeTypes.Changed)
				return;

			if (Change_Count % 2 == 1)
			{
				Change_Count = 0;
				Customer newinfo = Parser(Path.Combine(Dir, FileName));
				RaiseChangeEvent(newinfo, e.ChangeType);

			}
			else
			{
				if (Change_Count == byte.MaxValue || Change_Count == byte.MinValue)
					Change_Count = 0;

				Change_Count++;
			}
		}

		void FileWatcher_Created(object sender, FileSystemEventArgs e)
		{
			Customer newinfo= null;
			if (string.Compare(e.FullPath, Dir, true) == 0 && FileWatcher == null)
			{
				InitFileWatcher();
				if( File.Exists( Path.Combine(Dir, CMSAgreement.FileName) ) )
				{
					newinfo = Parser(Path.Combine(Dir, FileName));
					RaiseChangeEvent(newinfo, e.ChangeType);
				}
				return;
			}

			if (string.Compare(e.Name, FileName, true) != 0 || e.ChangeType != WatcherChangeTypes.Created)
				return;
			newinfo = Parser(Path.Combine(Dir, FileName));
			RaiseChangeEvent(newinfo, e.ChangeType);
		}

		void FileWatcher_Renamed(object sender, RenamedEventArgs e)
		{
			if( e.ChangeType != WatcherChangeTypes.Renamed)
				return;
			//init file watcher if folder already existed
			Customer newinfo = null;
			//check dir
			if (string.Compare(e.FullPath, Dir, true) == 0 || string.Compare(e.OldFullPath, Dir, true) == 0)
			{
				InitFileWatcher();
				newinfo = Parser(Path.Combine(Dir, FileName));
				RaiseChangeEvent(newinfo, e.ChangeType);
				return;
			}


			if (string.Compare(e.Name, FileName, true) == 0 || string.Compare(e.OldName, FileName, true) == 0)
			{
				newinfo = Parser(Path.Combine(Dir, FileName));
				RaiseChangeEvent(newinfo, e.ChangeType);
			}
		}

		void FileWatcher_Deleted(object sender, FileSystemEventArgs e)
		{
			//if( e.ChangeType != WatcherChangeTypes.Deleted || string.Compare( e.Name, FileName, true) != 0)
			//    return;
			if (e.ChangeType != WatcherChangeTypes.Deleted)
				return;
			if( string.Compare(e.FullPath,this.Dir, true ) == 0)
			{
				StopWatcher(ref this.FileWatcher);

				RaiseChangeEvent(new Customer{ AllowConnect = false, Domain = null}, e.ChangeType);
				return;
			}

			if(string.Compare( e.Name, FileName, true) == 0)
			{
				Customer newinfo = Parser(Path.Combine(Dir, FileName));
				RaiseChangeEvent(newinfo, e.ChangeType);
			}
		}

		private bool isDirectory(string path)
		{
			if(string.IsNullOrEmpty(path))
				return true;
			FileAttributes attr = File.GetAttributes(path);

			//detect whether its a directory or file
			if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
				return true;
			else
				return false;

		}
		private Customer Parser( string filepath)
		{
			if( string.IsNullOrEmpty(filepath) || !File.Exists(filepath))
				return new Customer();
			if( IsFileLocked(filepath, FileMode.Open, 3 ))
			{
				System.Threading.Thread.Sleep(1000);
			}

			XmlDocument doc = Commons.XMLUtils.LoadXMLDocument(filepath);
			if( doc == null || doc.DocumentElement == null)
				return new Customer();
			XmlNode root =  doc.DocumentElement as XmlNode;
			XmlNode agreementnode = Commons.XMLUtils.SelectNode( root, str_agreement_state);
			if (agreementnode == null)
				return new Customer();
			Customer info = new Customer();
			info.Domain = GetNode(agreementnode, str_domain_name);
			info.Name = GetNode(agreementnode, str_company_name);
			info.Email = GetNode(agreementnode, str_email_address);
			string dvrport = GetNode(agreementnode, str_pac_converter_port);
			UInt16 port = (UInt16)Consts.DEFAULT_CMS_TCP_PORT;
			UInt16.TryParse(dvrport, out port);
			info.ConverterPort = port == 0 ? (UInt16)Consts.DEFAULT_CMS_TCP_PORT : port;

			dvrport = GetNode(agreementnode, str_allow_connection);
			port = 0;
			UInt16.TryParse( dvrport, out port);
			info.AllowConnect = port == 1? true : false;
			return info;
		}

		private string GetNode(XmlNode parent, string name )
		{
			XmlNode node = Commons.XMLUtils.SelectNode(parent, name);
			return GetxmlValue(node) ;

		}
		
		private string GetxmlValue(XmlNode node)
		{
			return  node == null ? null : node.InnerText;
		}

		private bool IsFileLocked(string filePath,FileMode mode, int secondsToWait)
		{
			bool isLocked = true;
			int i = 0;
			const int wait_check_file = 500;
			int total_wait = secondsToWait * 1000;
			while (isLocked && ((i < total_wait) || (total_wait == 0)))
			{
				try
				{
					using (File.Open(filePath, mode)) { }
					return false;
				}
				catch (IOException e)
				{
					var errorCode = Marshal.GetHRForException(e) & ((1 << 16) - 1);
					isLocked = errorCode == 32 || errorCode == 33;

					if (secondsToWait != 0)
						new System.Threading.ManualResetEvent(false).WaitOne(wait_check_file);

					i += wait_check_file;

				}
			}

			return isLocked;
		}
	}
}

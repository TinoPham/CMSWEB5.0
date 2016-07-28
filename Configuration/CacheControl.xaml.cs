using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ConverterDB;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceConfig
{
	/// <summary>
	/// Interaction logic for CacheControl.xaml
	/// </summary>
	public partial class CacheControl : UserControl
	{
		public event ServiceConfig.Events.NotifySettingChange<UserControl, DataTransferEventArgs> OnSettingChange;
		public static DependencyProperty DataProperty = DependencyProperty.Register("Data", typeof(ConvertDB), typeof(CacheControl));
		const string CacheResourceFileName = "CacheTable.txt";

		private bool _changeConfig = false;
		public bool ConfigChange
		{
			get{ return _changeConfig;}
			set{ 
					_changeConfig = value;
					if (OnSettingChange != null)
						OnSettingChange(this, DataTransferEventArgs.Empty as DataTransferEventArgs, value);
			}
		}
		public ConvertDB Data
		{
			get
			{
				return (ConvertDB)base.GetValue(DataProperty);
			}
			set
			{
				base.SetValue(DataProperty, value);
			}
		}

		public CacheControl()
		{
			InitializeComponent();

		}
		
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			List<string> table = Loadtable(CacheResourceFileName);
			table.ForEach( item => DeleteCache(Data, item));
			ConfigChange = true;

		}

		List<string> Loadtable( string filename)
		{
			List<string>ret = new List<string>();

			string filepath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), filename);
			StreamReader reader = null;
			if (File.Exists(filepath))
				reader = new StreamReader(filepath, Encoding.UTF8, true);
			else
			{
				string full_resName = string.Format("{0}.{1}", this.GetType().Namespace, filename);
				reader = new StreamReader( this.GetType().Assembly.GetManifestResourceStream(full_resName));
			}
			if( reader != null)
			{
				ret = LoadTable(reader);
				reader.Close();

				reader.Dispose();
			}
			
			return ret;
		}

		List<string>LoadTable(StreamReader reader)
		{
			if( reader == null)
				return new List<string>();

			List<String> ret = new List<string>();
			string line = string.Empty;
			while( !string.IsNullOrEmpty(line = reader.ReadLine()))
					ret.Add(line);
			return ret;
		}

		private void DeleteCache(ConvertDB database, string tablename)
		{
			database.DeleteData(tablename);
		}
	}
}

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using ConverterDB.Model;
using System.Diagnostics;

namespace ServiceConfig
{
	/// <summary>
	/// Interaction logic for ConverterConfig.xaml
	/// </summary>
	public partial class ConverterConfig : UserControl, INotifyPropertyChanged
	{
		public event ServiceConfig.ServiceConfigure.ButtonClick OnButtonClick;
		public event ServiceConfig.Events.NotifySettingChange<UserControl, DataTransferEventArgs> OnSettingChange;
		public event PropertyChangedEventHandler PropertyChanged;

		//public static DependencyProperty DVRSettingProperty = DependencyProperty.Register("DVRSetting", typeof(ConverterDB.Model.DVRConverter), typeof(ConverterConfig));
		//public static DependencyProperty ConvertInfoProperty = DependencyProperty.Register("ConvertInfo", typeof(ObservableCollection<ConvertInfo>), typeof(ConverterConfig));
		public bool DataLoaded{ get ;private set;}
		private bool _changeConfig = false;
		public bool ConfigChange
		{
			get { return _changeConfig; }
			set
			{
				_changeConfig = value;
				if (OnSettingChange != null)
					OnSettingChange(this, DataTransferEventArgs.Empty as DataTransferEventArgs, value);
			}
		}

		DVRConverter _DVRSetting;
		public ConverterDB.Model.DVRConverter DVRSetting
		{
			get
			{
				return _DVRSetting;
			}
			set
			{
				_DVRSetting = value;
				OnPropertyChanged("DVRSetting");
			}
		}
		ConverterDB.Model.ServiceConfig _ServiceConfig;
		public ConverterDB.Model.ServiceConfig ServiceConfig
		{
			get { return _ServiceConfig;}
			set {
				_ServiceConfig = value;
				OnPropertyChanged("ServiceConfig");
			}
		}

		ObservableCollection<ConvertInfo>  _ConvertInfo;
		public ObservableCollection<ConvertInfo>  ConvertInfo
		{
			get
			{
				return _ConvertInfo;
			}
			set
			{
				_ConvertInfo = value;
				OnPropertyChanged("ConvertInfo");
			}
		}


		//private DVRConverter _dvrconvert;
		//private ObservableCollection<ConvertInfo> _Converters;
		//public ObservableCollection<ConvertInfo> ConvertInfo
		//{
		//    get{ return _Converters;}
		//    set {

		//        _Converters = value;
		//        OnPropertyChanged("ConvertInfo");
		//        UpdateOrderButtons();
		//    }
		//}

		//public DVRConverter DVRConvert
		//{
		//    get{ return _dvrconvert;}
		//    set 
		//    {
		//        _dvrconvert = value;
		//        OnPropertyChanged("DVRConvert");
				
		//    }
		//}

		public ConverterConfig()
		{
			InitializeComponent();
			//_Converters = new IEnumerable<ConvertInfo>();
			//_dvrconvert = new DVRConverter();

			//Anh Huynh, Load and Init DB after show main form, Feb 02, 2015
			//this.DataContext = this;
			//UpdateOrderButtons();
		}

		//Anh Huynh, Load and Init DB after show main form, Feb 02, 2015, begin
		public void LoadFormData()
		{
			this.DataContext = this;
			UpdateOrderButtons();
			DataLoaded = true;
		}
		//Anh Huynh, end

		public void OnPropertyChanged(string PropertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
		}

		private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			UpdateOrderButtons();
		}

		private void UpdateOrderButtons()
		{
			  int index = dgConverter.SelectedIndex;
			  dgConverter.CurrentItem = dgConverter.SelectedItem;
			  dgConverter.Items.MoveCurrentTo( dgConverter.CurrentItem);
			  btnUp.IsEnabled = index > 0;
			  btnDown.IsEnabled = index >= 0 && ConvertInfo != null && ConvertInfo.Count() -1 != index;
			  btnFirst.IsEnabled = index > 0;
			  btnLast.IsEnabled = ConvertInfo != null &&  index < ConvertInfo.Count() -1;
		}
		
		private void Button_Click(object sender, RoutedEventArgs e)
		{
            bool iscontinue = true;
            if( sender.Equals(this.btnSave) )
            {
                iscontinue = CheckValidPort( true);
            }
            if( iscontinue)
			    OnButtonClickHandler(sender, e);
		}
        private bool CheckValidPort( bool message)
        {
            string strport = txtPort.Text;
            string msg = "";
            if (string.IsNullOrEmpty(strport))
            {
                if (message)
                {
                    msg = lblTcpPort.Content.ToString() + " value must be in range 1 - " + ushort.MaxValue.ToString();
                    MainWindow.ShowMessageBox(msg);
                }
                return false;
            }
            ushort port = 0;
            ushort.TryParse(strport, out port);
            if (port <= 0 || port > ushort.MaxValue)
            {
                if (message)
                {
                    msg = lblTcpPort.Content.ToString() + " value must be in range 1 - " + ushort.MaxValue.ToString();
                    MainWindow.ShowMessageBox(msg);
                }
                return false;
            }
            Process p = CheckValidPort(port);
            if (p == null)
                return true;
            if (string.Compare(p.ProcessName, PACDMConverterConfig.PACDMConverter_ProcessName, true) != 0)
            {
                if(message)
                {
                    msg = string.Format("{0} {1} is using by {2}", lblTcpPort.Content.ToString(), port, p.ProcessName);
                    MainWindow.ShowMessageBox(msg);
                }
                return false;
            }
            return true;
        }
        Process CheckValidPort(ushort tcpport)
        {
            if (tcpport <= 0)
                return null;

            Win32API.MIB_TCPROW_OWNER_PID[] Ports = Win32API.Instance.GetAllTcpListener();
            if (Ports == null || Ports.Length == 0)
                return null;
            Win32API.MIB_TCPROW_OWNER_PID port = Ports.FirstOrDefault(it => it.LocalPort == tcpport);
            if (port.LocalPort == 0 || port.owningPid == 0)
                return null;
            Process p = Process.GetProcessById((int)port.owningPid);
            return p;
        }

		private void OnButtonClickHandler( object sender, RoutedEventArgs e)
		{
			if (OnButtonClick != null)
				OnButtonClick(sender, e);
		}

		private void btnFirst_Click(object sender, RoutedEventArgs e)
		{
			int index = ConvertInfo.IndexOf( dgConverter.SelectedItem as ConvertInfo);
			if(index == -1)
				return;

			if( sender.Equals(btnFirst))
			{
				ConvertInfo.Move(index, 0);
				dgConverter.Items.MoveCurrentToFirst();
				dgConverter.SelectedIndex = 0;
				 UpdateOrderButtons();
				 return;
			}
			if( sender.Equals(btnLast))
			{
				ConvertInfo.Move(index, ConvertInfo.Count -1);
				dgConverter.Items.MoveCurrentToLast();
				dgConverter.SelectedIndex = ConvertInfo.Count - 1;
				UpdateOrderButtons();
				return;
			}
			if( sender.Equals(btnUp))
			{
				ConvertInfo.Move(index, index -1);
				dgConverter.Items.MoveCurrentToPrevious();
				dgConverter.SelectedIndex = index -1;
				UpdateOrderButtons();
				return;
			}
			if( sender.Equals(btnDown))
			{
				ConvertInfo.Move(index, index + 1);
				dgConverter.Items.MoveCurrentToNext();
				dgConverter.SelectedIndex = index + 1;
				UpdateOrderButtons();
				return;
			}
		}

		private static bool IsNumberOnly(string text)
		{
			Regex regex = new Regex("[^0-9]+"); //regex that matches disallowed text
			return !regex.IsMatch(text);
		}

		private void TextBox_Pasting(object sender, DataObjectPastingEventArgs e)
		{
			if (e.DataObject.GetDataPresent(typeof(String)))
			{
				String Text1 = (String)e.DataObject.GetData(typeof(String));
				if (!IsNumberOnly(Text1))
					e.CancelCommand();
			}
			else
				e.CancelCommand();
		}

		private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			string text = e.Text;
			if (!IsNumberOnly(text))
			{
				e.Handled = true;
				return;
			}
			int interval;
			Int32.TryParse(text, out interval);
		}

		private void TextBox_SourceUpdated(object sender, DataTransferEventArgs e)
		{
            if (CheckValidPort(false))
            {
                Process p = CheckValidPort( ushort.Parse(txtPort.Text));
                if( p== null || string.Compare(p.ProcessName, PACDMConverterConfig.PACDMConverter_ProcessName, true) != 0)
                    ConfigChange = true;
            }
		}

		private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
		   TextBox port = sender as TextBox;
		   int value;
		   Int32.TryParse(port.Text, out value);
		   if (value > ushort.MaxValue)
		   {
			   txtPort.Text = ushort.MaxValue.ToString();
		   }
		}

		private void txtLastKey_MouseLeave(object sender, MouseEventArgs e)
		{
			TextBox port = sender as TextBox;
			if (port.Text.Trim().Length == 0)
			{
				(sender as TextBox).Text = "0";
				(sender as TextBox).SelectionStart = port.Text.Length;
			}
			else
			{
				//(sender as TextBox).Text = Int64.Parse(port.Text).ToString();

			}
		}

		private void TextBox_TextInput(object sender, TextCompositionEventArgs e)
		{
			string text = e.Text;
			if (!IsNumberOnly(text))
			{
				e.Handled = true;
				return;
			}
			int interval;
			Int32.TryParse(text, out interval);
		}

		private void txtLastKey_KeyDown(object sender, KeyEventArgs e)
		{
			if(e.Key == Key.Space)
			{
				e.Handled = true;
				return;
			}
			if (e.Key == Key.Tab)
			{
				TextBox port = sender as TextBox;
				if (port.Text.Trim().Length == 0)
				{
					(sender as TextBox).Text = "0";
					(sender as TextBox).SelectionStart = port.Text.Length;
				}
			}
		}

		private void txtLastKey_LostFocus(object sender, RoutedEventArgs e)
		{
			TextBox port = sender as TextBox;
			if (port.Text.Trim().Length == 0)
			{
				(sender as TextBox).Text = "0";
				(sender as TextBox).SelectionStart = port.Text.Length;
			}
		}

	}
}

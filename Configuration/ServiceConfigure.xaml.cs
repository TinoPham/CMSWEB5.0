using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using localdb = ConverterDB.Model;

namespace ServiceConfig
{
	/// <summary>
	/// Interaction logic for ServiceConfigure.xaml
	/// </summary>
	public partial class ServiceConfigure : UserControl, INotifyPropertyChanged
	{

		public event ServiceConfig.Events.NotifySettingChange<UserControl, DataTransferEventArgs> OnSettingChange;
		public delegate void ButtonClick(object sender, RoutedEventArgs e);

		public event ButtonClick OnButtonClick;

		
		public event PropertyChangedEventHandler PropertyChanged;

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

		//public static DependencyProperty DataProperty = DependencyProperty.Register("Data", typeof(ConverterDB.Model.ServiceConfig), typeof(ServiceConfigure));

		private localdb.ServiceConfig _data;

		public localdb.ServiceConfig Data
		{
			get{
				return _data; //(ConverterDB.Model.ServiceConfig)base.GetValue(DataProperty);
				}
			set{

				//base.SetValue(DataProperty, value);
				_data = value;
				if( _data == null)
					_data = new localdb.ServiceConfig();
				OnPropertyChanged("Data");
				//BindingOperations.GetBindingExpressionBase(txtInterval, TextBox.TextProperty).UpdateTarget();
				//BindingOperations.GetBindingExpressionBase(txtUrl, TextBox.TextProperty).UpdateTarget();
				
			}
		}

		public ServiceConfigure()
		{
			InitializeComponent();
			_data = new localdb.ServiceConfig();
			this.Loaded += (s, e) =>
			{
				this.DataContext = this;
			};
			
		}

		public void OnPropertyChanged(string PropertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
		}

		private void OnButtonClickHandler(object sender, RoutedEventArgs e)
		{
			if( OnButtonClick != null)
				OnButtonClick(sender, e);
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
			if(!IsNumberOnly( text))
			{
				e.Handled = true;
				return;
			}
			int interval;
			Int32.TryParse(text, out interval);


		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			OnButtonClickHandler(sender, e);
		}

		private void TextBox_SourceUpdated(object sender, DataTransferEventArgs e)
		{
			ConfigChange = true;
		}

	}

	
}

using System;
using System.Windows;
using System.Windows.Controls;
using Commons;
using ConvertMessage;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;

namespace ServiceConfig
{
	/// <summary>
	/// Interaction logic for ServiceControl.xaml
	/// </summary>
	public partial class ServiceControl : UserControl
	{
		//public event ServiceConfig.ServiceConfigure.ButtonClick OnButtonClick;
		public event Events.ServiceControlButtonClick OnCompletedButtonClick;
		public event Events.ServiceControlButtonClick OnControlButtonClick;

		string _ServiceName;
		public string ServiceName { get { return _ServiceName; }set{ _ServiceName = value; UpdateButtonStatus(value);}}
		public ConverterDB.Model.ServiceConfig ServiceConfig { get; set; }
		public bool RequestStart { get; set; }

		public ServiceControl()
		{
			InitializeComponent();
		}

		private MessageResult GetInfoServiceForControl(ConverterDB.Model.ServiceConfig service)
		{
			ValidateClientConfig vlc = new ValidateClientConfig(service);
			return vlc.GetInfoServices();
		}

		public string validateService(bool requestStart, ConverterDB.Model.ServiceConfig service)
		{

			if (requestStart)
			{
				return Commons.Resources.ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.MSG_VAID_SAVE);
			}

			if (service == null)
			{
				return Commons.Resources.ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.SERVICE_CANNOT_BE_NULL);
			}

			if (String.IsNullOrEmpty(service.Url))// || !Utils.ValidateURL(service.Url))//Anh Huynh, Don't need to check valid URL here
			{
				return Commons.Resources.ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.MSG_VAID_URL);
			}

			MessageResult result = GetInfoServiceForControl(service);

			switch (result.ErrorID)
			{
				case ERROR_CODE.OK:
				{
					return null;
				}
				case ERROR_CODE.DVR_LOCKED_BY_ADMIN:
				{
					return Commons.Resources.ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.MSG_VAID_LOCKBYID);
				}
				default:
				{
					return string.Format(Commons.Resources.ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.MSG_VAID_FAILED), result.Data);
				}
			}
		}

		private void OnButtonClickHandler(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(_ServiceName))
				return;

			int svrState = 0;
			string Msg = string.Empty;

			if (OnControlButtonClick != null)
				OnControlButtonClick(sender, svrState);

			if (sender.Equals(btnStop))
			{
				svrState = APIService.StopService(_ServiceName);
				if (svrState != 0)
					Msg = string.Format("Cannot stop {0} Service", _ServiceName);
			}
			else if (sender.Equals(btnStart))
			{
				svrState = APIService.StartService(_ServiceName);

				if (svrState != 0)
					Msg = string.Format("Cannot start {0} Service", _ServiceName);
			}

			else if (sender.Equals(btnRestart))
			{
				svrState = svrState = APIService.StopService(_ServiceName);
				if (svrState != 0)
					Msg = string.Format("Cannot stop {0} Service", _ServiceName);
				if (svrState == 0)
				{
					svrState = svrState = APIService.StartService(_ServiceName);
					if (svrState != 0)
						Msg = string.Format("Cannot start {0} Service", _ServiceName);
				}
			}

			if (svrState != 0)
			{
				//MessageBox.Show(Msg, Window.GetWindow(this).Title, MessageBoxButton.OK);
				MainWindow.ShowMessageBox(Msg, Window.GetWindow(this).Title);

				if (OnCompletedButtonClick != null)
					OnCompletedButtonClick(sender, svrState);
				return;
			}

			UpdateButtonStatus(_ServiceName);
			if (OnCompletedButtonClick != null)
				OnCompletedButtonClick(sender, svrState);
		}

		private void UpdateButtonStatus( string svrName)
		{
			if( string.IsNullOrEmpty( svrName))
			{
				btnRestart.IsEnabled =false;
				btnStart.IsEnabled = false;
				btnStop.IsEnabled = false;
				return;
			}
			APIService.SERVICE_STATE svrState = APIService.GetServiceState(svrName);

			btnStart.IsEnabled = svrState == APIService.SERVICE_STATE.SERVICE_STOPPED || svrState == APIService.SERVICE_STATE.SERVICE_STOP_PENDING;
			btnRestart.IsEnabled = svrState == APIService.SERVICE_STATE.SERVICE_RUNNING || svrState == APIService.SERVICE_STATE.SERVICE_START_PENDING;;
			btnStop.IsEnabled = svrState == APIService.SERVICE_STATE.SERVICE_RUNNING || svrState == APIService.SERVICE_STATE.SERVICE_START_PENDING;

		}

	}
}

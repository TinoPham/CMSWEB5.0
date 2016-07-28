using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Commons;
using ConverterDB;
using ConverterDB.Model;
using ConvertMessage;
using Microsoft.Windows.Shell;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.ComponentModel;
using System.Collections.ObjectModel;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Pipes.Server;
using Pipes.Interfaces;
using System.Windows.Threading;
using System.Diagnostics;
using System.IO;
namespace ServiceConfig
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		private bool isAsking = false;
		PipeServer _pipeserver;
		delegate void VersionMessageDelegate(string oldVersion, string newVersion);
		delegate void AppLeaveToExitDelegate(int sec);
		delegate void AppExitDelegate();

		public event PropertyChangedEventHandler PropertyChanged;
		public const int WM_NCLBUTTONDOWN = 0xA1;
		public const int HT_CAPTION = 0x2;
		[DllImportAttribute("user32.dll")]
		public static extern bool ReleaseCapture();
		[DllImportAttribute("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

		const string msg_Cannot_LoadConnection = "The connection to database has been failed.";

		const string msg_Timer_Upgrade_app = "The new package was downloaded. Application will close in {0} second(s).";

		public const string STR_UPGRADE = "upgrade";
		public const string APP_Upgrade = "AppUpgrade.exe";
		const string Cmd_Old_Version = "-o";
		const string Cmd_New_Version = "-n";
		const string Cmd_Start = "-s";
		const string Cmd_PID = "-P";

		const int Timer_Upgrade_Interval = 30;

		DispatcherTimer dispatcherTimer;

		private string NewVersion{ get ;set;}

		private volatile int UpgradeInterval;

		public bool _WaitingUpgrade;
		public bool WaitingUpgrade { get { return _WaitingUpgrade; } set { _WaitingUpgrade = value; OnPropertyChanged("WaitingUpgrade"); } }

		private string _msg_WaitingUpgrade{ get;set;}
		public string msg_WaitingUpgrade { get { return _msg_WaitingUpgrade; } set { _msg_WaitingUpgrade = value; OnPropertyChanged("msg_WaitingUpgrade"); } }

		ConvertDB convertDb;
		private bool _changeSetting = false;
		private bool _RestartRequest = false;
		private bool _shownForm = false;
		private static MainWindow _mainForm = null;

		public bool RestartRequest
		{
			get{ return _RestartRequest;}
			set{ 
				_RestartRequest = value;
				OnPropertyChanged("RestartRequest");
			}
		}

		//public static DependencyProperty DBProperty = DependencyProperty.Register("DB", typeof(ConvertDB), typeof(MainWindow));
		//public static DependencyProperty ServiceSettingProperty = DependencyProperty.Register("ServiceSetting", typeof(ConverterDB.Model.ServiceConfig), typeof(MainWindow));
		//public static DependencyProperty DVRSettingProperty = DependencyProperty.Register("DVRSetting", typeof(ConverterDB.Model.DVRConverter), typeof(MainWindow));
		//public static DependencyProperty ConverterSettingProperty = DependencyProperty.Register("ConverterSetting", typeof(ObservableCollection<ConverterDB.Model.ConvertInfo>), typeof(MainWindow));

		public ConvertDB DB
		{
			get {
				return convertDb; 
				}

			private set {
				convertDb = value;
			}

		 }

		public ConverterDB.Model.ServiceConfig ServiceSetting
		{
			get
			{
				return DB == null? null : DB.ServiceConfig;
			}
			
		}

		public ConverterDB.Model.DVRConverter DVRSetting
		{
			get
			{
				return DB== null? null : DB.DvrConverter; //(ConverterDB.Model.DVRConverter)base.GetValue(DVRSettingProperty);
			}
			//private set
			//{
			//    base.SetValue(DVRSettingProperty, value);
			//}
		}

		ObservableCollection<ConverterDB.Model.ConvertInfo> _ConverterSetting;
		public ObservableCollection<ConverterDB.Model.ConvertInfo> ConverterSetting
		{
			get
			{
				if( _ConverterSetting == null)
					_ConverterSetting = new ObservableCollection<ConvertInfo> (DB == null ? new List<ConvertInfo>() : DB.ConvertInfo);
				return _ConverterSetting;
			}
			//private set
			//{
			//    base.SetValue(ConverterSettingProperty, value);
			//}
		}


		public MainWindow()
		{
			InitializeComponent();
			bntAbout.IsEnabled = false;
			WaitingUpgrade = false;
			this.CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, this.OnCloseWindow));
			this.CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, this.OnMaximizeWindow, this.OnCanResizeWindow));
			this.CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, this.OnMinimizeWindow, this.OnCanMinimizeWindow));
			this.CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, this.OnRestoreWindow, this.OnCanResizeWindow));

			//Anh Huynh, Load and Init DB after show main form, Feb 02, 2015, begin
			/*
			if(!InitDefaultDB())
			{
				Application.Current.Shutdown();
			}
			else
			{
				PACDMConverterConfig.Instance.CheckData(convertDb);
				btnsvrConfig.IsChecked = false;
				btncvtConfig.IsChecked = false;
				this.btncvtConfig.Checked +=ToggleButton_Checked;
				this.btnsvrConfig.Checked += ToggleButton_Checked;
				this.btnConverter.Checked += ToggleButton_Checked;
				btnsvrConfig.IsChecked = true;
			}

			this.DataContext = this;
			DB = convertDb;

			ServiceConfig.Loaded += (s, e) =>
			{
				ServiceConfig.Data = DB.ServiceConfig;
			};

			ConverterConfig.Loaded+=(s,e) =>
			{
				ConverterConfig.DVRSetting = this.DVRSetting;
				ConverterConfig.ConvertInfo = this.ConverterSetting;
			};

			Converter.OnCompletedButtonClick += Converter_OnCompletedButtonClick;

			ViewLogs.Loaded +=(s,e) =>
								{
								 ViewLogs.Data = this.DB; ViewLogs.Date = DateTime.Now;
								};

			Cachecontrol.Loaded +=(s,e)=>
				{ 
				Cachecontrol.Data = DB; 
				};
			*/
			//LoadFormData();
			_mainForm = this;
			//Anh Huynh, Load and Init DB after show main form, Feb 02, 2015, end
		}

		//Anh Huynh, Load and Init DB after show main form, Feb 02, 2015, begin
		protected override void OnContentRendered(EventArgs e)
		{
			base.OnContentRendered(e);
			if (_shownForm)
				return;
			_shownForm = true;

			this.BusyIndicator.IsBusy = true;
			ShowBusy(this.BusyIndicator.IsBusy);
			ShowMessageUpgradeComplete();
			LoadFormData();

			this.BusyIndicator.IsBusy = false;
			ShowBusy(this.BusyIndicator.IsBusy);
			bntAbout.IsEnabled = true;
		}
	
		void LoadFormData()
		{
			if (!InitDefaultDB())
			{
				Application.Current.Shutdown();
			}
			else
			{
				PACDMConverterConfig.Instance.CheckData(convertDb);
				btnsvrConfig.IsChecked = false;
				btncvtConfig.IsChecked = false;
				this.btncvtConfig.Checked += ToggleButton_Checked;
				this.btnsvrConfig.Checked += ToggleButton_Checked;
				this.btnConverter.Checked += ToggleButton_Checked;
				btnsvrConfig.IsChecked = true;
			}

			this.DataContext = this;
			DB = convertDb;

			ServiceConfig.Data = DB.ServiceConfig;

			//ConverterConfig.DVRSetting = this.DVRSetting;
			//ConverterConfig.ConvertInfo = this.ConverterSetting;

			//ConverterConfig.LoadFormData();

			Converter.OnCompletedButtonClick += Converter_OnCompletedButtonClick;

			ViewLogs.Data = this.DB; 
			ViewLogs.Date = DateTime.Now;

			Cachecontrol.Data = DB;
			InitPipe();
		}
		//Anh Huynh, Load and Init DB after show main form, Feb 02, 2015, end

		void Converter_OnCompletedButtonClick(object sender, int svrStat)
		{
			this.BusyIndicator.IsBusy = false;
			this.RestartRequest = false;
			_changeSetting = false;
		}
		
		private void ShowMessageUpgradeComplete()
		{
			bool warn = (App.Current as ServiceConfig.App).IsWarningVersion;
			if( AppInfo.Instance.IsNewUpgrade && warn)
			{
				ServiceConfig.NewVersion window = new ServiceConfig.NewVersion( AppInfo.Instance.AppVersion)
				{
					ShowInTaskbar = false,               // don't show the dialog on the taskbar
					Topmost = false,                      // ensure we're Always On Top
					ResizeMode = ResizeMode.NoResize,    // remove excess caption bar buttons
					WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner,
					Owner = Application.Current.MainWindow,
					 Width=300, Height=110
				};

				window.Show();
				window.Activate();
				window.Focus();
			}
		}
		public void OnPropertyChanged(string PropertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
		}
		
		private void OnCanResizeWindow(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = this.ResizeMode == ResizeMode.CanResize || this.ResizeMode == ResizeMode.CanResizeWithGrip;
		}

		private void OnCanMinimizeWindow(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = this.ResizeMode != ResizeMode.NoResize;
		}

		private void OnCloseWindow(object target, ExecutedRoutedEventArgs e)
		{
			SystemCommands.CloseWindow(this);
		}

		private void OnMaximizeWindow(object target, ExecutedRoutedEventArgs e)
		{
			SystemCommands.MaximizeWindow(this);
		}

		private void OnMinimizeWindow(object target, ExecutedRoutedEventArgs e)
		{
			SystemCommands.MinimizeWindow(this);
		}

		private void OnRestoreWindow(object target, ExecutedRoutedEventArgs e)
		{
			SystemCommands.RestoreWindow(this);
		}
	
		public void move_window(object sender, MouseButtonEventArgs e)
		{
			ReleaseCapture();
			SendMessage(new WindowInteropHelper(this).Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
		}

		private void btnClose_Click(object sender, RoutedEventArgs e)
		{
			isAsking = true;
			SystemCommands.CloseWindow(this);
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if( isAsking)
			{
				MessageBoxResult ret = MessageBox.Show("Do you want to exit PAC Converter Configuration?", this.Title, MessageBoxButton.YesNo);
				if( ret == MessageBoxResult.No)
					e.Cancel = true;
			}
			StopPipe();
			isAsking = false;
			base.OnClosing(e);

		}

		private bool InitDefaultDB()
		{
			try
			{
				string config = PACDMConverterConfig.Instance.ConvertDBConnection;
				if( string.IsNullOrEmpty( config))
				{
					//MessageBox.Show(msg_Cannot_LoadConnection, this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
					ShowMessageBox(msg_Cannot_LoadConnection, this.Title);
					return false;
				}

				convertDb = new ConvertDB(config);
				return true;
				
			}
			catch(Exception ex)
			{
				//MessageBox.Show( ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
				ShowMessageBox(ex.Message, this.Title);
				return false;
			}

		}

		private void ToggleButton_Checked(object sender, RoutedEventArgs e)
		{
			if( sender.Equals(btnsvrConfig))
			{
				//SetSVRConfig(convertDb.ServiceConfig);
				ServiceConfig.Data = this.ServiceSetting;
				return;
			}
			if( sender.Equals(btncvtConfig))
			{
				//LoadConverterConfig();
				if (!ConverterConfig.DataLoaded)
				{
					ConverterConfig.DVRSetting = this.DVRSetting;
					ConverterConfig.ConvertInfo = this.ConverterSetting;
					ConverterConfig.ServiceConfig = this.ServiceSetting;
					ConverterConfig.LoadFormData();
				}
				else
				{
					DB.Refresh<ConvertInfo>();
					DB.Refresh<ConverterDB.Model.ServiceConfig>();
					_ConverterSetting = null;
					ConverterConfig.ConvertInfo = this.ConverterSetting;
					ConverterConfig.ServiceConfig = this.ServiceSetting;
				}
				
				return;
			}

			if( sender.Equals(btnConverter))
			{
				this.Converter.ServiceName = PACDMConverterConfig.Instance.ConverterServiceName;
				this.Converter.ServiceConfig = convertDb.ServiceConfig;
			}
		}

		private void bntAbout_Click(object sender, RoutedEventArgs e)
		{
			About window = new About()
			{
				ShowInTaskbar = false,               // don't show the dialog on the taskbar
				Topmost = false,                      // ensure we're Always On Top
				ResizeMode = ResizeMode.NoResize,    // remove excess caption bar buttons
				WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner,
				Owner = Application.Current.MainWindow,
			};

			window.ShowDialog();
		}

		private MessageResult GetInfoServices()
		{
			ValidateClientConfig vlc = new ValidateClientConfig(ServiceConfig.Data);
				return vlc.GetInfoServices();
		}

		private void ShowBusy(bool isBusy)
		{
			if (isBusy)
			{
				this.Cachecontrol.IsEnabled = false;
				this.ConverterConfig.IsEnabled = false;
				this.ViewLogs.IsEnabled = false;
				this.ServiceConfig.IsEnabled = false;
				this.Converter.IsEnabled = false;
				//this.frmMain.IsEnabled = false;
			}
			else
			{
				//this.frmMain.IsEnabled = true;
				this.Cachecontrol.IsEnabled = true;
				this.ConverterConfig.IsEnabled = true;
				this.ViewLogs.IsEnabled = true;
				this.ServiceConfig.IsEnabled = true;
				this.Converter.IsEnabled = true;
			}
		}

		private void ValidateClientConfig(bool isCheck = true)
		{
			this.BusyIndicator.IsBusy = true;
			ShowBusy(this.BusyIndicator.IsBusy);
			if (String.IsNullOrEmpty(ServiceConfig.Data.Url))// || !Utils.ValidateURL(ServiceConfig.Data.Url)) //Anh Huynh, Don't need to check valid URL here
			{
				this.BusyIndicator.IsBusy = false;
				ShowBusy(this.BusyIndicator.IsBusy);
				//MessageBox.Show(
				//	Commons.Resources.ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.MSG_VAID_URL),
				//	Window.GetWindow(this).Title, MessageBoxButton.OK, MessageBoxImage.Warning);
				ShowMessageBox(Commons.Resources.ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.MSG_VAID_URL), Window.GetWindow(this).Title, MessageBoxImage.Warning);
				return;
			}

			MessageResult result = GetInfoServices();

			switch (result.ErrorID)
			{
				case ERROR_CODE.OK:
					{
						this.BusyIndicator.IsBusy = false;
						ShowBusy(this.BusyIndicator.IsBusy);
						if (isCheck)
						{
							//MessageBox.Show(Commons.Resources.ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.MSG_TEST_OK), Window.GetWindow(this).Title, MessageBoxButton.OK,
							//	MessageBoxImage.Information);
							ShowMessageBox(Commons.Resources.ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.MSG_TEST_OK), Window.GetWindow(this).Title);
						}
						break;
					}
				case ERROR_CODE.DVR_LOCKED_BY_ADMIN:
					{
						this.BusyIndicator.IsBusy = false;
						ShowBusy(this.BusyIndicator.IsBusy);

						//MessageBox.Show(Commons.Resources.ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.MSG_VAID_LOCKBYID),
						//	Window.GetWindow(this).Title, MessageBoxButton.OK, MessageBoxImage.Warning);
						ShowMessageBox(Commons.Resources.ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.MSG_VAID_LOCKBYID),Window.GetWindow(this).Title, MessageBoxImage.Warning);
						break;
					}
				default:
					{
						this.BusyIndicator.IsBusy = false;
						ShowBusy(this.BusyIndicator.IsBusy);

						//MessageBox.Show(
						//	Commons.Resources.ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.MSG_TEST_FAILED) + result.Data,
						//	Window.GetWindow(this).Title, MessageBoxButton.OK, MessageBoxImage.Error);
						ShowMessageBox(Commons.Resources.ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.MSG_TEST_FAILED) + result.Data,Window.GetWindow(this).Title,MessageBoxImage.Error);
						break;
					}
			}
		}

		private void ServiceConfig_OnButtonClick(object sender, RoutedEventArgs e)
		{

			if ((sender as Button).IsDefault)
			{
				ValidateClientConfig();
				return;
			}

			if ((sender as Button).IsCancel)
			{
				this.convertDb.CancelChanges();
				//SetSVRConfig(convertDb.ServiceConfig);
				ServiceConfig.ConfigChange = false;
				ServiceConfig.Data = this.ServiceSetting;
			}
			else
			{
				if (ServiceConfig.Data == null)
					return;

				if (String.IsNullOrEmpty(ServiceConfig.Data.Url))// || !Utils.ValidateURL(ServiceConfig.Data.Url))//Anh Huynh, Don't need to check valid URL here
				{
					//MessageBox.Show(
					//	Commons.Resources.ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.MSG_VAID_URL),
					//	Window.GetWindow(this).Title, MessageBoxButton.OK, MessageBoxImage.Warning);
					ShowMessageBox(Commons.Resources.ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.MSG_VAID_URL),Window.GetWindow(this).Title, MessageBoxImage.Warning);
					return;
				}

				if (ServiceConfig.Data.ID == 0)
					convertDb.Insert<ConverterDB.Model.ServiceConfig>(ServiceConfig.Data);
				else
				{
					convertDb.ServiceConfig.Url = ServiceConfig.Data.Url;
					convertDb.ServiceConfig.NumDVRMsg = ServiceConfig.Data.NumDVRMsg;
					convertDb.ServiceConfig.LogRecycle = ServiceConfig.Data.LogRecycle;
					convertDb.ServiceConfig.Interval = ServiceConfig.Data.Interval;
					convertDb.Update<ConverterDB.Model.ServiceConfig>(convertDb.ServiceConfig);
				}
				convertDb.Save();
				SetSVRConfig(convertDb.ServiceConfig);
				_changeSetting = true;
			}
			this.Converter.RequestStart = false;
			ServiceConfig.ConfigChange = false;
		}

		private void SetSVRConfig(ConverterDB.Model.ServiceConfig input)
		{
			ServiceConfig.Data = input == null
				? new ConverterDB.Model.ServiceConfig
				{
					Interval = Consts.MIN_CONVERT_INTERVAL,
					NumDVRMsg = Consts.DEFAULT_DVR_MSG,
					LogRecycle = Consts.DEFAULT_LOG_RECYCLE
				}
				: new ConverterDB.Model.ServiceConfig
				{
					ID = input.ID,
					Url = input.Url,
					Interval = input.Interval,
					LogRecycle = input.LogRecycle,
					NumDVRMsg = input.NumDVRMsg
				};
		}

		private void LoadConverterConfig()
		{
			//ConverterConfig.DVRConvert = new ConverterDB.Model.DVRConverter{ DvrSocketRetry = convertDb.DvrConverter.DvrSocketRetry, Enable = convertDb.DvrConverter.Enable, ID = convertDb.DvrConverter.ID, TCPPort = convertDb.DvrConverter.TCPPort};
			//ConverterConfig.ConvertInfo = new System.Collections.ObjectModel.ObservableCollection<ConvertInfo>( convertDb.ConvertInfo);
		}

		private void ConverterConfig_OnButtonClick(object sender, RoutedEventArgs e)
		{
			Button btn = sender as Button;
			if (btn.IsCancel)
			{
				convertDb.CancelChanges();
				//LoadConverterConfig();
				ConverterConfig.ConfigChange = false;
				ConverterConfig.DVRSetting = DVRSetting;
				ConverterConfig.ConvertInfo = this.ConverterSetting;
                ToggleButton_Checked(btncvtConfig, e);
			}
			else
			{
				DVRConverter dvrcvt = ConverterConfig.DVRSetting;
				convertDb.DvrConverter.TCPPort = dvrcvt.TCPPort;
				convertDb.DvrConverter.Enable = dvrcvt.Enable;
				convertDb.DvrConverter.DvrSocketRetry = dvrcvt.DvrSocketRetry;
				ConvertInfo dbinfo = null;
				byte order_index = 0;
				foreach (ConvertInfo cvtinfo in ConverterConfig.ConvertInfo)
				{
					dbinfo = convertDb.ConvertInfo.FirstOrDefault(item => item.ID == cvtinfo.ID);
					if (dbinfo == null) continue;
					dbinfo.DvrDate = cvtinfo.DvrDate;
					dbinfo.Enable = cvtinfo.Enable;
					dbinfo.LastKey = cvtinfo.LastKey;
					dbinfo.Order = order_index++;
					convertDb.Update<ConvertInfo>(dbinfo);
				}
				convertDb.Save();
				_changeSetting = true;
			}
			this.Converter.RequestStart = false;
			ConverterConfig.ConfigChange = false;
		}

		private void ConverterConfig_OnSettingChange(UserControl sender, DataTransferEventArgs e, bool ischange)
		{
			this.Converter.RequestStart = this.ServiceConfig.ConfigChange | this.ConverterConfig.ConfigChange ;
			this.RestartRequest = _changeSetting | this.Converter.RequestStart | this.Cachecontrol.ConfigChange;
		}

		private void Converter_OnControlButtonClick(object sender, int svrStat)
		{
			this.BusyIndicator.IsBusy = true;
			//if ((sender as Button).Content.Equals("Start"))
			//{
			//	ValidateClientConfig(false);
			//}
		}
		#region Timer close
		private void StartUpgradeApp()
		{
			Process p_current = Process.GetCurrentProcess();
			string apppath = p_current.MainModule.FileName;
			string appdir = Path.GetDirectoryName(apppath);
			string src_appupgrade = Path.Combine(appdir,STR_UPGRADE, APP_Upgrade);
			ProcessStartInfo pinfo = new ProcessStartInfo(src_appupgrade);
			pinfo.Arguments += Cmd_Old_Version + string.Format(" \"{0}\" ", AppInfo.Instance.AppVersion);
			pinfo.Arguments += Cmd_New_Version + string.Format(" \"{0}\" ", NewVersion);
			pinfo.Arguments += Cmd_Start + string.Format(" \"{0}\" ", apppath);
			pinfo.Arguments += Cmd_PID + string.Format(" \"{0}\" ", p_current.Id);
			Process.Start(pinfo);
		}
		private void ExitDispatcher()
		{
			if( !Dispatcher.CheckAccess())
			{
				Dispatcher.BeginInvoke(new AppExitDelegate(ExitDispatcher));
				return;
			}
			else
			{
				// code goes here
				UpgradeInterval--;
				if( UpgradeInterval > 0)
				{
					msg_WaitingUpgrade = String.Format(MainWindow.msg_Timer_Upgrade_app, UpgradeInterval);
				}
				else
				{
					dispatcherTimer.Tick -= new EventHandler(dispatcherTimer_Tick);
					dispatcherTimer.Stop();
					isAsking = false;
					StartUpgradeApp();
					SystemCommands.CloseWindow(this);
				}
			}
		}

		private void InitTimerClose()
		{
			dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
			dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
			dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
			dispatcherTimer.Start();
		}

		private void dispatcherTimer_Tick(object sender, EventArgs e)
		{
			ExitDispatcher();
			
		}

		#endregion
		#region Pipe upgrade
		private void NofityVersion( string oldversion, string newversion)
		{
			if( !Dispatcher.CheckAccess())
			{
				Dispatcher.BeginInvoke(new VersionMessageDelegate(NofityVersion), oldversion, newversion );
				return;
			}
			else
			{
				if(!WaitingUpgrade)
				{
					this.NewVersion = newversion;
					WaitingUpgrade = true;
					UpgradeInterval = MainWindow.Timer_Upgrade_Interval;
					msg_WaitingUpgrade = String.Format( MainWindow.msg_Timer_Upgrade_app, UpgradeInterval);
					InitTimerClose();
				}
			}
		}
		private void InitPipe()
		{
			_pipeserver = new PipeServer( Consts.PIPE_NAME);
			//_pipeserver.ClientConnectedEvent += new EventHandler<ClientConnectedEventArgs>(ClientConnectedHandler);
			//_pipeserver.ClientDisconnectedEvent += new EventHandler<ClientDisconnectedEventArgs>(ClientDisconnectedHandler);
			_pipeserver.MessageReceivedModelEvent += new EventHandler<MessageReceivedModelEventArgs>(MessageReceivedHandler);
			_pipeserver.Start();
		}
		private void StopPipe()
		{
			if(_pipeserver != null)
			{
				_pipeserver.MessageReceivedModelEvent -= new EventHandler<MessageReceivedModelEventArgs>(MessageReceivedHandler);
				_pipeserver.Stop();
				_pipeserver = null;
			}
		}

		private void MessageReceivedHandler(object sender, MessageReceivedModelEventArgs eventArgs)
		{
			if( eventArgs == null || eventArgs.Message == null)
				return;
			if( eventArgs.Message is PipeModels.VersionModel)
			{
				PipeModels.VersionModel vmodel = eventArgs.Message as PipeModels.VersionModel;
				NofityVersion( vmodel.OldVersion, vmodel.NewVersion);
			}
			
		}
		#endregion
		#region MessageBox
		//Anh Huynh, Customize message box color style, Feb 05, 2015, begin
		public static BitmapSource ConvertBitmapTo96DPI(BitmapImage bitmapImage)
		{
			double dpi = 96;
			int width = bitmapImage.PixelWidth;
			int height = bitmapImage.PixelHeight;

			int stride = width * bitmapImage.Format.BitsPerPixel;
			byte[] pixelData = new byte[stride * height];
			bitmapImage.CopyPixels(pixelData, stride, 0);

			return BitmapSource.Create(width, height, dpi, dpi, bitmapImage.Format, null, pixelData, stride);
		}
		private static Style GetResourceStyle(string resName)
		{
			if (_mainForm != null)
			{
				return (Style)_mainForm.FindResource(resName);//()
			}
			return null;
		}
		private static BitmapSource GetMsgBoxImage(MessageBoxImage _img)
		{
			string imgSrc = string.Empty;
			switch (_img)
			{
				case MessageBoxImage.Warning:
					imgSrc = Consts.STR_Image_Warning;
					break;
				case MessageBoxImage.Error:
					imgSrc = Consts.STR_Image_Error;
					break;
				case MessageBoxImage.Question:
					imgSrc = Consts.STR_Image_Question;
					break;
				case MessageBoxImage.Information:
				default:
					imgSrc = Consts.STR_Image_Information;
					break;
			}
			return ConvertBitmapTo96DPI(new BitmapImage(new Uri(imgSrc, UriKind.Absolute)));
		}
		public static void ShowMessageBox(string _text)
		{
			ShowMessageBox(_text, Window.GetWindow(_mainForm).Title, MessageBoxImage.Information);
		}
		public static void ShowMessageBox(string _text, string _title)
		{
			ShowMessageBox(_text, _title, MessageBoxImage.Information);
		}
		public static void ShowMessageBox(string _text, string _title, MessageBoxImage _img)
		{
			MessageBox msgBox = new MessageBox();
			msgBox.Text = _text;
			msgBox.Caption = _title;
			msgBox.CaptionForeground = Brushes.White;
			msgBox.WindowBackground = Brushes.Gray;//Brushes.Black;

			SolidColorBrush mySolidColorBrush = new SolidColorBrush();
			mySolidColorBrush.Color = Color.FromArgb(255, 50, 50, 50); //(Color)FindResource("WindowBackgroundColor");//
			msgBox.Background = mySolidColorBrush; //(Brush)FindResource("WindowBackgroundColor");//
			msgBox.ButtonRegionBackground = mySolidColorBrush;

			msgBox.Foreground = Brushes.White;
			msgBox.CloseButtonStyle = GetResourceStyle("WindowButtonStyle");
			//msgBox.OkButtonStyle = (Style)FindResource("WindowButtonStyle");
			msgBox.ImageSource = GetMsgBoxImage(_img);
			msgBox.ShowDialog();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if(!WaitingUpgrade)
				return;
			UpgradeInterval = 0;
		}

		
		//Anh Huynh, Customize message box color style, Feb 05, 2015, end
		#endregion
	}
}

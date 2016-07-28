using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ConverterDB.Model;
using ConverterDB;
using System.Threading;

namespace ServiceConfig
{
	/// <summary>
	/// Interaction logic for LogsControl.xaml
	/// </summary>
	public partial class LogsControl : UserControl, INotifyPropertyChanged
	{
		const string STR_STOP = "Stop";
		const string STR_SEARCH = "Search";
		CancellationTokenSource search_tokensource;
		Task<List<Log>> SearchTask = null;
		public static DependencyProperty DataProperty = DependencyProperty.Register("Data", typeof(ConvertDB), typeof(LogsControl));

		public ConvertDB Data
		{
			get {
				return (ConvertDB)base.GetValue(DataProperty); 
				}
			set {
				base.SetValue(DataProperty, value);
			}
		}

		private DateTime _date = DateTime.Now;
		public DateTime Date
		{
			get{ return _date;}
			set
			{
				var date = new DateTime(value.Year, value.Month, value.Day, 0, 0, 0);
				_date = date;
				OnPropertyChanged("Date");
			}
		}

		private IEnumerable<Log> _Logs = new List<Log>();
		public IEnumerable<Log> Logs
		{
			get { return _Logs; }
			set
			{
				_Logs = value;
				OnPropertyChanged("Logs");
			}
		}

		private DateTime _startSate = DateTime.Now.AddDays(-1);
		public DateTime StartDate
		{
			get { return _startSate; }
			set
			{
				var date = new DateTime(value.Year, value.Month, value.Day, 0, 0, 0);
				_startSate = date;
				OnPropertyChanged("StartDate");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string PropertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
		}

		public LogsControl()
		{
			InitializeComponent();
			this.DataContext = this;
		}

		private void alertDGrid_Loaded(object sender, RoutedEventArgs e)
		{
			System.Windows.Controls.DataGrid dg = sender as System.Windows.Controls.DataGrid;
			Border border = VisualTreeHelper.GetChild(dg, 0) as Border;
			ScrollViewer scrollViewer = VisualTreeHelper.GetChild(border, 0) as ScrollViewer;
			Grid grid = VisualTreeHelper.GetChild(scrollViewer, 0) as Grid;
			System.Windows.Controls.Button button = VisualTreeHelper.GetChild(grid, 0) as System.Windows.Controls.Button;

			if (button != null && button.Command != null && button.Command == System.Windows.Controls.DataGrid.SelectAllCommand)
			{
				button.IsEnabled = false;
				button.Opacity = 0;
			}
		}
		private void btnSearch_Click(object sender, RoutedEventArgs e)
		{
			if( Data == null)
				return;
			if( string.Compare( btnSearch.Content.ToString(), STR_SEARCH, true) == 0)
			{
				if( !FDate.SelectedDate.HasValue || !TDate.SelectedDate.HasValue)
				{
					MainWindow.ShowMessageBox("From, To date must be selected.");
					return;
				}
				if( DateTime.Compare(FDate.SelectedDate.Value, TDate.SelectedDate.Value) > 0)
				{
					MainWindow.ShowMessageBox("To date must be greater than From date");
					return;
				}
				search_tokensource = new CancellationTokenSource();
				btnSearch.Content = STR_STOP;
				Logs = new List<Log>();
					ExecuteSearch(Data, StartDate, Date, search_tokensource.Token);
				
			}
			else
			{
				if( SearchTask != null)
				{
					search_tokensource.Cancel(false);
					SearchTask.Wait();
					search_tokensource =null;
					SearchTask = null;
				}
				btnSearch.Content = STR_SEARCH;
			}
			
		}
		private void ExecuteSearch(ConvertDB database, DateTime startdate, DateTime date, CancellationToken token)
		{
			var uiSched = TaskScheduler.FromCurrentSynchronizationContext();
			Int64 logid = 0;
			SearchTask = Task.Factory.StartNew<List<Log>>(() => ExecuteSearchLog(database, startdate, date, logid), token);
			SearchTask.ContinueWith(list =>
			{
				Logs = list.Result;
				btnSearch.Content = STR_SEARCH;
			}, token, TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously, uiSched);
			//Chinh 14/9/2014 begin
			try
			{
				SearchTask.Wait(token);
			}
			catch (AggregateException ex)
			{
				ex.Handle(inner =>
				{
					Console.WriteLine("Handle exception of type: {0}", inner.GetType());
					return true;
				});
			}
			//Chinh 14/9/2014 end
		}

		private List<Log> ExecuteSearchLog(ConvertDB database, DateTime startdate, DateTime date, Int64 minlogid)
		{
			//if(Loaded)
			//    database.Refresh<Log>();
			
			IEnumerable<Log> Result = database.Query(typeof(Log), item => item.DVRDate.Date <= Date && item.DVRDate.Date >= StartDate && item.ID > minlogid).OrderBy(x => x.DVRDate).Cast<Log>();
			//if( Result.Any())
			//    Loaded = true;
			return Result.ToList();
		}

	}
}

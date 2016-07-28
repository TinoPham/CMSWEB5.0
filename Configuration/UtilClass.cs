using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ServiceConfig
{
	public class Events
	{
		public delegate void NotifyRestart( object sender);
		public delegate void NotifySettingChange<T,U>( T sender, U e, bool ischange);
		public delegate void ServiceControlButtonClick( object sender, int svrStat);

	}
	public class Consts
	{
		public const string PIPE_NAME = "ServiceConfig_D37A4DC7-315C-45C4-9C40-9DEADD4F195E";
		public const int DVR_SOCKET_RETRY = 10;
		public const int MIN_CONVERT_INTERVAL = 10;
		public const int DEFAULT_DVR_MSG = 10;
		public const int DEFAULT_LOG_RECYCLE = 10;
		public const int DEFAULT_CMS_TCP_PORT = 1000;
		public const int MAX_CONVERT_INTERVAL = Int16.MaxValue;

		public const string Default_DBConnection = @"Data Source=|DataDirectory|\ConverterDB.sdf";
		public const string STR_Converter = "Converter";
		public const string STR_ConnectionName = "ConnectionName";
		public const string STR_ConverterServiceKey = "ConvertServiceName";
		public const string STR_LastMonthConvert = "LastMonthConvert";
		public const string STR_CMSServerPort = "CMSServerPort";
		public const string STR_ConverterURL = "api/converter/Converter/";
		public const string STR_Http = "http://";
		public const string STR_Https = "https://";

		public const string STR_Image_Error = "pack://application:,,,/ServiceConfig;component/Resources/Error.png";
		public const string STR_Image_Information = "pack://application:,,,/ServiceConfig;component/Resources/Information.png";
		public const string STR_Image_Question = "pack://application:,,,/ServiceConfig;component/Resources/Question.png";
		public const string STR_Image_Warning = "pack://application:,,,/ServiceConfig;component/Resources/Warning.png";
	}
	public class ForceGroundConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return new SolidColorBrush((bool)value == true ? Colors.Red : Color.FromArgb(255,186,186,186));
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return new SolidColorBrush((bool)value == true ? Colors.Red : Colors.Black);
		}
	}
	public class IntervalConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value == null || string.IsNullOrEmpty(value.ToString()) ? Consts.MIN_CONVERT_INTERVAL : value;
		}

		#endregion
	}

	public class CMSPortConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value == null || string.IsNullOrEmpty(value.ToString()) ? Consts.DEFAULT_CMS_TCP_PORT : value;
		}

		#endregion
	}

	public class ProgramsetConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Commons.Utils.GetEnum<Commons.Programset>(value).ToString();
			//return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value == null || string.IsNullOrEmpty(value.ToString()) ?  0 : (byte)Commons.Utils.GetEnum<Commons.Programset>(value);
		}

		#endregion
	}

	public class VisibilityConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (Boolean)value == true?  Visibility.Visible : Visibility.Collapsed;
			//return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (Visibility)value == Visibility.Visible? true : false;
		}

		#endregion
	}
}
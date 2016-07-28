using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Commons
{
    public static class Utils
    {
		/// <summary>
		/// Convert valid string/number to valid enumeration.
		/// </summary>
		/// <typeparam name="T"> type of enumeration</typeparam>
		/// <param name="strtype"> enumerable string/number </param>
		/// <returns>enumeration value</returns>
		public static T GetEnum<T>(string strtype)
		{
			if( string.IsNullOrEmpty(strtype))
				return default(T);

			int Num;
			bool isNum = int.TryParse(strtype, out Num);
			try
			{
			if (!isNum)
				return (T)Enum.Parse(typeof(T), strtype, true);
			else
			{
				T ret = (T)Enum.ToObject(typeof(T), Num);
				return ret;
			}
			}
			catch( System.ArgumentNullException ){}
			catch (System.ArgumentException) { }
			catch (System.OverflowException) { }

			return default(T);
		}
		
		/// <summary>
		/// Convert valid string/number to valid enumeration.
		/// </summary>
		/// <typeparam name="T"> type of enumeration</typeparam>
		/// <param name="data"> enumerable value </param>
		/// <returns>enumeration value</returns>
		public static T GetEnum<T>(object data)
		{
			if( data != null)
				return GetEnum<T>(data.ToString());
			return default(T);
		}
		
		public static object GetEnum( Type enumtype, string value)
		{
			Array values = Enum.GetValues(enumtype);
			if (string.IsNullOrEmpty(value))
				return values.GetValue(0);

			int Num;
			bool isNum = int.TryParse(value, out Num);
			
			if (!isNum)
			{
				var enumstring = Enum.GetNames(enumtype);
				var index = enumstring.Cast<string>().ToList().FindIndex( delegate( string item){ return string.Compare(item, value, true) == 0; });
				return values.GetValue(index >= 0? index : 0);
			}
			else
			{
				var index = values.Cast<int>().ToList().IndexOf(Num);
				return values.GetValue(index >= 0? index : 0);
			}
		}
		/// <summary>
		/// Converting a simple data type.
		/// </summary>
		/// <typeparam name="T">Simple data type. it can be: int, int?, string, double .... </typeparam>
		/// <param name="input"> The value need to convert</param>
		/// <param name="default_value"> default value when cannot convert</param>
		/// <returns>Return <T>value </returns>
		public static T ChangeSimpleType<T>(object input, object default_value = null)
		{
			Type type = typeof(T);
			
			if( input == null)
				goto Invalid_Input;

			try
			{
				return (T) Convert.ChangeType( input, typeof(T));
			}
			catch(Exception)
			{
				goto Invalid_Input;
			}

		 Invalid_Input:
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
				return (T)default_value;
			else
				return default(T);

		}

		public static object ChangeSimpleType( Type type,object input, object default_value = null)
		{
			if( input == null)
				goto Invalid_Input;

			try
			{
				return Convert.ChangeType(input, type);
			}
			catch(Exception)
			{
				goto Invalid_Input;
			}

		 Invalid_Input:
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
				return default_value;
			else
				return default_value;

		}
		 
		/// <summary>
		/// Validating input string.
		/// </summary>
		/// <param name="regex">the pattern will using to check data</param>
		/// <param name="data">String value need to validate</param>
		/// <param name="option">Optional to validate string. default value is nonsensitive</param>
		/// <returns>true: match with pattern otherwise false. Always return false when regex or data is empty</returns>
		public static bool ValidationString(string regex, string data, RegexOptions option = RegexOptions.IgnoreCase )
		{
			if (string.IsNullOrEmpty(regex) || string.IsNullOrEmpty(data))
				return false;

			Regex rx = new Regex(regex, option);
			return rx.IsMatch(data);
		}

		/// <summary>
		/// Get a property name from object type
		/// </summary>
		/// <typeparam name="T"> type of class</typeparam>
		/// <param name="propertyExpression">Property</param>
		/// <returns>property namse</returns>
		/// Example: Utils.GetPropertyName<Flexconfig>(() => Utils.CMSWebAllConfig); => return: CMSWebAllConfig 
		public static string GetPropertyName<T>(Expression<Func<T>> propertyExpression)
		{
			return (propertyExpression.Body as MemberExpression).Member.Name;
		}

		/// <summary>
		/// Convert string to version object
		/// </summary>
		/// <param name="strversion">Version string.</param>
		/// <returns>Version object.</returns>
		public static Version String2Version(string strversion = null)
		{
			if (string.IsNullOrWhiteSpace(strversion))
				return new Version(ConstEnums.STR_NONE_VERSION);

			try
			{
				string[] vsplit = strversion.Split(new char[] {ConstEnums.DOT_CHAR});

				if (vsplit == null || vsplit.Length == 0)
					return new Version(ConstEnums.STR_NONE_VERSION);

				int[] iversion = new int[] { 0, 0, 0, 0 };
				for (int i = 0; i < vsplit.Length; i++)
				{
					if (i >= iversion.Length)
						break;
					iversion[i] = Int32.Parse(vsplit[i]);
				}
				return new Version(iversion[0], iversion[1], iversion[2], iversion[3]);
			}
			catch (Exception)
			{
				return new Version(ConstEnums.STR_NONE_VERSION);
			}
		}

		public static Version ParserVersion( string value, string pattern = ConstEnums.Regex_Version)
		{
			if( string.IsNullOrEmpty(value))
				return null;
			Regex rx = new Regex(pattern);
			Match match = rx.Match(value);
			if( match.Success)
				return new Version(match.Value);
			return null;
		}
		/// <summary>
		/// validate a URL address
		/// </summary>
		/// <param name="strurl"></param>
		/// <returns></returns>
		public static bool ValidateURL(string strurl)
		{
			try
			{
				if (!ValidationString(ConstEnums.Regex_URL, strurl))
					return false;

				Uri uri = new Uri(strurl);
				uri = null;
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// validate an email address
		/// </summary>
		/// <param name="emailadddress"></param>
		/// <returns></returns>
		public static bool ValidateEmailAddress(string emailadddress)
		{
			return ValidationString(ConstEnums.Regex_EmailAddress, emailadddress);
		}
		/// <summary>
		/// validate TCP port
		/// </summary>
		/// <param name="port"></param>
		/// <returns></returns>
		public static bool ValidatePort(string port)
		{
			return ValidationString(ConstEnums.Regex_Port, port);
		}

		/// <summary>
		/// validate a IP address
		/// </summary>
		/// <param name="ipadd"></param>
		/// <returns></returns>
		public static bool ValidateIPAddress(string ipadd)
		{
			return ValidationString(ConstEnums.Regex_IP, ipadd);
		}

		public static bool Isnull(DataRow row, string column)
		{
			return (row == null || !row.Table.Columns.Contains(column) || row.IsNull(column));
		}

		public static string ExtractMac( string macaddress, char separate = ':' )
		{
			if( string.IsNullOrEmpty(macaddress))
				return null;
			if( ValidationString( ConstEnums.Regex_MAC_ADDRESS_COLONs, macaddress) )
				return macaddress.Replace(':',separate).Trim(); 
			if( ValidationString( ConstEnums.Regex_MAC_ADDRESS_DASHs, macaddress))
				return macaddress.Replace('-', separate).Trim();
			return null;
		}
		public static bool ValidateMacAddress( string macaddress)
		{
			return !ValidationString(ConstEnums.Regex_MAC_ADDRESS_COLONs, macaddress) ? ValidationString(ConstEnums.Regex_MAC_ADDRESS_DASHs, macaddress) : true;
		}

		public static string Base64toString( string base64)
		{
			if( string.IsNullOrEmpty(base64))
			return string.Empty;

			byte[] buff = Convert.FromBase64String(base64);
			return Encoding.UTF8.GetString(buff);
		}
		public static string ToBase64String( byte[] buff)
		{
			if( buff == null)
				return string.Empty;

			return Convert.ToBase64String(buff);
		}
		public static string String2Base64( string input)
		{
			if( string.IsNullOrEmpty(input))
				return string.Empty;

			byte[] buff = Encoding.UTF8.GetBytes( input);
			return ToBase64String(buff);
		}
		public static void ThrowExceptionMessage(string message)
		{
			throw new Exception( message);
		}

		public static byte[] String2Byte(string data)
		{
			return Encoding.UTF8.GetBytes(data);
		}
		public static string ByteArr2String(byte[] buff)
		{
			return Encoding.UTF8.GetString(buff);
		}

		public static DateTime? toSQLDate( DateTime? datetime)
		{
			if( datetime == null || !datetime.HasValue || datetime.Value == DateTime.MinValue ||datetime.Value == DateTime.MaxValue)
				return (DateTime?)null;
			return (DateTime?)datetime.Value;
		}
		public static DateTime toSQLDate( DateTime datetime)
		{
			if( datetime == DateTime.MinValue)
				return ConstEnums.SQL_SMALLDATETIME_MIN_VALUE;
			if( datetime == DateTime.MaxValue)
				return ConstEnums.SQL_SMALLDATETIME_MAX_VALUE;
			return datetime;
		}

		public static string StringtoHexstring(string value)
		{
			if( string.IsNullOrEmpty(value))
				return null;
			return ByteArrayToHexString( String2Byte( value));
		}
		public static string ByteArrayToHexString(byte [] bytes)
		{
			StringBuilder hex = new StringBuilder(bytes.Length * 2);

			foreach (byte b in bytes)

				hex.AppendFormat("{0:x2}", b);

			return hex.ToString();
		}
		
		public static byte [] HexStringToByte(string hexString)
		{
			try
			{

				int bytesCount = (hexString.Length) / 2;

				byte [] bytes = new byte [bytesCount];

				for (int x = 0; x < bytesCount; ++x)
				{
					bytes [x] = Convert.ToByte(hexString.Substring(x * 2, 2), 16);

				}

				return bytes;

			}
			catch
			{
				return null;
			}
		}

		public static bool IsNumber(string val)
		{
			if( string.IsNullOrEmpty(val))
				return false;

			return ValidationString(@"^(\d+)$", val);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

 namespace Commons
{
	#region Enums

	public enum Programset
	{
		UnknownType = 0,
		DVR,
		POS,
		IOPC,
		CA,
		ATM,
		LPR,
		LABOR,
		POS3RD
	}
	public enum ERROR_CODE : int
	{
		OK = 0,
		//Converter
		CONVERTER_INVALID_WEBAPI,
		CONVERTER_CANCELREQUEST,
		CONVERTER_DISABLE_DVRCONNECTION,
		CONVERTER_DISABLE_ALL_PACDATA,
		CONVERTER_CREATE_FOLDER_FAILED,
		CONVERTER_FILE_EXISTED,
		CONVERTER_KEEPALIVE,
		CONVERTER_INVALID_TOKEN,
		CONVERTER_DOWNLOAD_FAILED,
		CONVERTER_LOGIN,							///10
		//Service Exception begin
		SERVICE_CANNOT_BE_NULL,
		SERVICE_EXCEPTION,
		SERVICE_CANNOT_PARSER_DATA,
		SERVICE_DVR_PENDING,
		SERVICE_NOT_FOUND_DVR,
		SERVICE_TOKEN_INVALID,
		SERVICE_TOKEN_EXPIRED,
		SERVICE_UNSUPPORT_FUNCTION,
		SERVICE_TERMINAL,
		SERVICE_INTERNALSERVERERROR,				//20
		SERVICE_NOTIMPLEMENTED,
		SERVICE_BADGATEWAY,
		SERVICE_SERVICEUNAVAILABLE,
		SERVICE_GATEWAYTIMEOUT,
		SERVICE_HTTPVERSIONNOTSUPPORTED,
		HTTP_CLIENT_DISPOSEED,
		HTTP_CLIENT_TASK_CANCEL,
		HTTP_CLIENT_EXCEPTION,
		INVALID_COMPRESSION_REQUEST,
		//Service Exception end
		//DB Exception begin
		DB_CONNECTION_STRING_NULL,				//30
		DB_CONNECTION_INVALID,
		DB_CONNECTION_FAILED,
		DB_UPDATE_DATA_FAILED,
		DB_INSERT_DATA_FAILED,
		DB_INVALID_TABLE,
		DB_QUERY_EXCEPTION,
		DB_QUERY_NODATA,
		DB_CONVERT_DATA_FAILED,
		DB_CONVERT_IGNORED,
		DB_CANNOT_FIND_ITEMKEY,					//40
		DB_INVALID_DVR_GUI,
		DB_INVALID_CONVERT_CONFIG,
		//DB Exception end
		//Mapping begin
		INVALID_MAPPING,
		//Mapping end
		//Socket exception begin
		SOCKET_ERROR_DESTINATION_ADDRESS,
		CMS_SOCKET_SERVER_DISABLE,
		CMS_SOCKET_ERROR,
		CMS_SOCKET_SHUTDOWN,
		CMS_SOCKET_SEND_ERROR,
		CMS_SOCKET_RECEIVED_ERROR,
		//Socket exception end
		DVR_REGISTER_FAILED,				//50
		DVR_REGISTER_PENDING,
		DVR_LOCKED_BY_ADMIN,
		DVR_INFO_CHANGE,
		DVR_INVALID_INFO,
		DVR_FULL_CONNECTED,
		//Add new item here
		//DVR Convert exception begin
		DVR_ERR_PARSE_MSG,
		DVR_ERR_XML_LOAD_ERR,
		DVR_ERR_XML_DESERIALIZE,
		DVR_ERR_OBJ_DESERIALIZE,
		DVR_ERR_DB_CONNECT,					//60
		DVR_ERR_DB_UPDATE,
		//DVR Convert exception end

		//Message Alert begin
		MSG_TEST_OK,
		MSG_TEST_LOCKBYID,
		MSG_TEST_FAILED,
		MSG_VAID_LOCKBYID,
		MSG_VAID_FAILED,
		MSG_VAID_URL,
		MSG_VAID_SAVE,
		UNSUPPORT_MEDIA_TYPE,
		//Message Alert End
		MSG_REQUEST_CONVERTER_VERSION,		//70
		MSG_CONVERTER_VERSION_NOTFOUND,
		MSG_CONVERTER_VERSION_HIGHER_THAN_SERVER,
		UNKNOWN
	}
	#endregion
	public abstract class ConstEnums
	{
		#region Constant
		public const string STR_NONE_VERSION = "0.0.0.0";
		public const string Regex_MAC_ADDRESS_COLONs = "^[0-9A-F]{2}:[0-9A-F]{2}:[0-9A-F]{2}:[0-9A-F]{2}:[0-9A-F]{2}:[0-9A-F]{2}$";
		public const string Regex_MAC_ADDRESS_DASHs = "^[0-9A-F]{2}-[0-9A-F]{2}-[0-9A-F]{2}-[0-9A-F]{2}-[0-9A-F]{2}-[0-9A-F]{2}$";
		
		public const string Regex_EmailAddress = @"([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})";
		public const string Regex_URL = @"(?<Protocol>\w+):\/\/(?<Domain>[\w@][\w.:@]+)\/?[\w\.?=%&=\-@/$,]*";
		public const string Regex_IP = @"(?<First>2[0-4]\d|25[0-5]|[01]?\d\d?)\.(?<Second>2[0-4]\d|25[0-5]|[01]?\d\d?)\.(?<Third>2[0-4]\d|25[0-5]|[01]?\d\d?)\.(?<Fourth>2[0-4]\d|25[0-5]|[01]?\d\d?)";
		public const string Regex_Port = @"^(\d{2,5})$";
		public const char DOT_CHAR = '.';
		public const string Regex_Version = @"^(?<Ver>(\d+)\.(\d+)\.(\d+)(\.(\d+))?)";//@"(\d+)\.(\d+)\.(\d+)\.(\d+)";
		public const string Regex_VersionAny = @"(?<Ver>(\d+)\.(\d+)\.(\d+)(\.(\d+))?)";//@"(\d+)\.(\d+)\.(\d+)\.(\d+)";
		public static readonly DateTime SQL_SMALLDATETIME_MIN_VALUE = new DateTime(1900, 1, 1, 0, 0, 0);
		public static readonly DateTime SQL_SMALLDATETIME_MAX_VALUE = new DateTime(2079, 1, 1, 23, 59, 59);
		public const string Registry_PAC_Key = @"SOFTWARE\PAC";
		public const string Registry_FirstRun_Key = "FirstRun";
		public const string Registry_PACConverterVer = "PACConverterVer";
		public const string Registry_PACConverterVerOld = @"PACConverterVerOld";

		#region XML
		public const string XPATH_CHILD_NODE = "./{0}";
		public const string XPATH_ATTRIBUTE = "./{0}[@{1}='{2}']";
		public const string STR_and = "and";
		public const string XPATH_COMPARE_EQUAL = " @{0} = '{1}'";
		#endregion
		#region Internal exception messages
		internal const string INTERNAL_MSG_INVALID_PROPERTY = "The property name {0} is invalid.";
		internal const string INTERNAL_MSG_EMPTY_PROPERTY = "The property name cannot be empty.";
		internal const string INTERNAL_MSG_INVALID_FIELD = "The field name {0} is invalid.";
		internal const string INTERNAL_MSG_EMPTY_FIELD = "The field name cannot be empty.";
		#endregion
		#endregion
	
	}
	
	public abstract class HttpConstant
	{
		public const string STR_gzip = "gzip";
		public const string STR_deflate = "deflate";
		public const string STR_Application_Xml = "application/xml";
		public const string STR_Application_Json = "application/json";

	}

}

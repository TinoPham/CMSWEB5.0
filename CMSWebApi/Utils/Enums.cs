using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.Utils
{
	public enum SortDirection:int
	{
		ASC = 0,
		DESC
	}

	public enum CMSWebError: int
	{
		OK,
		INVALID_MODEL,
		STATUS,

		//begin resource error
		//Access is denied.
		ACCESS_DENIED,
		//session is expired
		SESSION_EXPIRED,
		//Validate Required
		REQUIRED_FIELD,
		//Validate Length
		EXCEEDS_LENGTH,
		//Validate Numeric
		INVALID_NUMBER,
		//Validate Greater Than Zero
		MUST_GREATER_THAN_ZERO,
		//Validate Greater Than 'Number'
		MUST_GREATER_THAN_NUMBER,
		//must not equal zero.
		MUST_NOT_EQUAL_ZERO,
		//must not equal number.
		MUST_NOT_EQUAL_NUMBER,
		//not selected.
		FIELD_NOT_SELECTED,
		//not a valid date
		INVALID_DATE,
		//greater than or equal to today.
		GREATER_THAN_OR_EQUAL_TODAY,
		//invalid an email address
		INVALID_EMAIL_ADDRESS,
		//invalid  url address
		INVALID_URL_ADDRESS,
		//parameters wrong
		PARAM_WRONG,
		PASSWORD_INCORRECT_MSG,
		//data not found on database
		DATA_NOT_FOUND,
		//ERRORCODE
		NO_PERMISION,
		SERVER_ERROR_MSG

		//Common 
		, ADD_SUCCESS_MSG
		, ADD_FAIL_MSG
		, EDIT_SUCCESS_MSG
		, EDIT_FAIL_MSG
		, DELETE_SUCCESS_MSG
		, DELETE_FAIL_MSG
		, DELETE_FAIL_ITEM_HAVE_CHILDS
		, DO_NOT_HAVE_PERMISSION_MSG
		, EMAIL_EXIST_MSG
        , EXCEED_LICENSE

		//Sites
		, HASP_LICENSE_EXIST_MSG
		, MAC_EXIST_MSG
		, MAC_FORMAT_WRONG
		, WORKING_FOLDER_CANNOT_SET
		, WORKING_FOLDER_CANNOT_FOUND
		, WORKING_FILE_CANNOT_DELETE
		, WORKING_FILE_EXIST
		, SITE_NAME_EXIST
		, REGION_NAME_EXIST
		, UNABLED_DELETE_REGION_EXIST_SITE
		, UNABLED_DELETE_REGION_EXIST_SUB_REGION
		, UNABLE_TO_DELETE_ROOT_TREE

		//Site Metric
		, SITEMETRIC_NAME_REQUIRED
		, SITEMETRIC_NAME_EXIST
		, SITEMETRIC_IS_USED

		//Goals Management
		, GOALTYPE_NAME_REQUIRED
		, GOALTYPE_NAME_EXIST
		, GOALTYPE_IS_USED

		//User 
		, USER_NAME_EXIST
		, USER_EMPLOYEE_EXIST
		, UNSUPPORT_MEDIA_TYPE

		//Job Title
		, JOB_NAME_EXIST_MSG
		, JOB_NAME_REQUIRED
		, JOB_IS_USED

		//User Groups
		, USERGROUPS_NAME_EXIST
		, USERGROUPS_NAME_REQUIRED

		// LDAP Configuration
		, LDAP_IP_EXIST
		
	}

	public enum Menulevel:int
	{
		MODULE = 1,
		FUNCTION,
		LEVEL
	}

	public enum SiteType:int
	{
		REGION = 0,
		SITE,
		DVR,
		CHANNEL
	}
	
	public enum OnlineStatus : int
	{
		OFFLINE = 0,
		ONLINE,
		BLOCKED,
		OFFLINE_SCHEDULE
	}

	public enum TodoStatus : int
	{
		Work,
		Done
	}

	public enum MetricType : int
	{
		GROUP =0,
		METRIC
	}

	public enum FiscalCalendarType
	{
 		NORMAL = 1,
		FISCAL,
		WEEKS_52,
		WEEKS_53,
		WEEKS_52_53
	}

	public enum ChartType : int
	{
		ALERT_COUNT = 0,
		DVR_COUNT,
		DVR_MOSTALERT,
		CONV_MAP_USA
	}

	public enum ChartTrafficType : int
	{
		HOURLY = 0,
		DAILY,
		MONTHLY
	}

    public enum ScheduleType : int
    {
        HOURLY = 1,
        DAILY,
        WEEKLY
    }

    public enum StatusSchedule : int
    {
        UPLOAD = -2,
        IN_ACTIVE = 0,
        ACTIVE,
        PENDING,
        COMPLETED,
        FAILED,
        INCOMPLETE,
        EXPIRED
    }

	public enum PeriodType : byte
	{
		Invalid = 0,
		Year,
		LastYear,
		Month,
		LastMonth,
		Week,
		LastWeek,
		Day,
		Yesterday,
		Now,
		Today,
		Hour,
		Min,
		Sec
		
	}

	public enum AlertSeverity: byte
	{
		Normal = 1,
		Caution,
		Warning,
		Urgent,
	}
	
	public enum AlertType: byte
	{
		DVR_System_Started = 1,
		DVR_System_Shutdown,
		DVR_Insufficient_Disk_Space_Backup,
		DVR_CPU_Temperature_High,
		DVR_Video_Loss,
		DVR_Backup_Started,
		DVR_Backup_Completed,
		DVR_Backup_Stopped,
		DVR_Sensor_Triggered,
		DVR_Control_Activated,
		DVR_HDD_Format_Started,
		DVR_HDD_Format_Completed,
		DVR_User_Added,
		DVR_User_Removed,
		DVR_User_Logged_in,
		DVR_User_Logged_out,
		DVR_disconnect_from_CMS_server,
		DVR_Storage_Setup_Changed,
		DVR_Video_Recycling_Began,
		DVR_Not_recording,
		Setup_Configuration_Changed,
		DVR_Partition_Dropped,
		CMS_Registration_Expire_Soon,
		CMS_Registration_Expired,
		Other_types,
		DVR_Partition_Added,
		DVR_HASP_Unplugged,
		DVR_HASP_Expired,
		DVR_Frame_Rate_Changed,
		DVR_Resolution_Changed,
		DVR_Time_Manually_Adjusted,
		DVR_is_off_line,
		DVR_connected_CMS_Server,
		DVR_Unstable_Video_Signal,
		DVR_Video_returned_to_normal,
		DVR_VA_detection,
		DVR_Record_Less_Than,
		//new DVR alert type here
		CMS_HASP_Unplugged = 101,
		CMS_HASP_Found,
		CMS_HASP_Removed,
		CMS_HASP_Expired,
		CMS_Server_HASP_Limit_Exceeded,
		CMSWEB_Conversion_rate_above_100,
		CMSWEB_Door_count_0,
		CMSWEB_POS_data_missing

	}

	public enum GoalType: int
	{
		Conversion = 1,
		Sale,
		Opportunity,
		Transaction,
		DPO,
		ATV,
		UPD,
        LaborHours
	}

	public enum ForecastFormular : byte
	{
		Period = 1,
		FiveWeek = 2
	}

	public enum BAMReportType : int
	{ 
		Hourly = 1,
		Daily,
		Weekly,
		WTD,
		PTD,
		YTD
	}

	public enum METRIC_DEFAULT : int
	{
		METRIC_OPPORTUNITIES = 1,
		METRIC_TRANSACTIONS,
		METRIC_SALES,
		METRIC_LABOUR_HOURS,
		METRIC_CONVERSION_RATE = 5,
		METRIC_ATV,
		METRIC_DPO,
		METRIC_UPD,
	}

	public enum RebarGroupByField
	{
		SiteName = 1,
		EmployeeName
	}

	public enum HM_SCHEDULE_TYPE : int
	{
		Hourly = 1,
		Daily,
		Weekly,
		WTD,
		Monthly,
		PTD
	}
	
	public enum Operator:  byte
	{
		EQual = 0,
		LessThan,
		GreaterThan,
		LessThanOrEqual,
		GreaterThanOrEqual,
		Contains,
	}
}

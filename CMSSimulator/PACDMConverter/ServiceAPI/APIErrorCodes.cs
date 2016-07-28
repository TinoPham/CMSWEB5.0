using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceManager
{
	class APIErrorCodes
	{
		public static string GetAPIErrorCode(int error_code)
		{
			string ret = string.Empty;
			switch (error_code)
			{
				case 0:
					ret = "The operation completed successfully.";
					break;
				case 1: 
					ret = "Incorrect function.";// ERROR_INVALID_FUNCTION 
					break;
				case 2:
					ret =  "The system cannot find the file specified.";//  ERROR_FILE_NOT_FOUND 
					break;
				case 3:
					ret = "The system cannot find the path specified.";//  ERROR_PATH_NOT_FOUND 
					break;
				case 4:
					ret = "The system cannot open the file.";//  ERROR_TOO_MANY_OPEN_FILES 
					break;
				case 5:
					ret = "Access is denied.";//  ERROR_ACCESS_DENIED 
					break;
				case 6:
					ret = "The handle is invalid.";//  ERROR_INVALID_HANDLE 
					break;
				case 7 :
					ret = "The storage control blocks were destroyed.";//  ERROR_ARENA_TRASHED 
					break;
				case 8:
					ret = "Not enough storage is available to process this command.";//  ERROR_NOT_ENOUGH_MEMORY 
					break;
				case 9:
					ret = "The storage control block address is invalid.";//  ERROR_INVALID_BLOCK 
					break;
				case 10:
					ret = "The environment is incorrect.";//  ERROR_BAD_ENVIRONMENT 
					break;
				case 11:
					ret = "An attempt was made to load a program with an incorrect format.";//  ERROR_BAD_FORMAT 
					break;
				case 87:
					ret = "The parameter is incorrect.";//ERROR_INVALID_PARAMETER
					break;
				case 122:
					ret = "The data area passed to a system call is too small.";//  ERROR_INSUFFICIENT_BUFFER 
					break;
				case 123:
					ret = "The filename, directory name, or volume label syntax is incorrect.";//  ERROR_INVALID_NAME 
					break;
				case 1051:
					ret = "A stop control has been sent to a service that other running services are dependent on.";//  ERROR_DEPENDENT_SERVICES_RUNNING 
					break;
				case 1052:
					ret = "The requested control is not valid for this service.";//  ERROR_INVALID_SERVICE_CONTROL 
					break;
				case 1053:
					ret = "The service did not respond to the start or control request in a timely fashion.";//  ERROR_SERVICE_REQUEST_TIMEOUT 
					break;
				case 1054:
					ret = "A thread could not be created for the service.";//  ERROR_SERVICE_NO_THREAD 
					break;
				case 1055:
					ret = "The service database is locked.";//  ERROR_SERVICE_DATABASE_LOCKED 
					break;
				case 1056:
					ret = "An instance of the service is already running.";//  ERROR_SERVICE_ALREADY_RUNNING 
					break;
				case 1057:
					ret = "The account name is invalid or does not exist.";//  ERROR_INVALID_SERVICE_ACCOUNT 
					break;
				case 1058://ERROR_SERVICE_DISABLED
					ret = "The service cannot be started, either because it is disabled or because it has no enabled devices associated with it.";
					break;
				case 1059:
					ret = "Circular service dependency was specified.";//  ERROR_CIRCULAR_DEPENDENCY 
					break;
				case 1060:
					ret = "The specified service does not exist as an installed service.";//  ERROR_SERVICE_DOES_NOT_EXIST 
					break;
				case 1061:
					ret = "The service cannot accept control messages at this time.";//  ERROR_SERVICE_CANNOT_ACCEPT_CTRL 
					break;
				case 1062:
					ret  = "The service has not been started.";//  ERROR_SERVICE_NOT_ACTIVE 
					break;
				case 1065:
					ret = "The database specified does not exist.";//  ERROR_DATABASE_DOES_NOT_EXIST
					break;
				case 1068:
					ret = "The dependency service or group failed to start.";//  ERROR_SERVICE_DEPENDENCY_FAIL 
					break;
				case 1069:
					ret = "The service did not start due to a logon failure.";//  ERROR_SERVICE_LOGON_FAILED 
					break;
				case 1072:
					ret = "The specified service has been marked for deletion.";//  ERROR_SERVICE_MARKED_FOR_DELETE 
					break;
				case 1073:
					ret = "The specified service already exists.";//  ERROR_SERVICE_EXISTS 
					break;
				case 1075:
					ret = "The dependency service does not exist or has been marked for deletion.";//  ERROR_SERVICE_DEPENDENCY_DELETED 
					break;
				case 1078:
					ret = "The name is already in use as either a service name or a service display name.";//  ERROR_DUPLICATE_SERVICE_NAME 
					break;
				case 1115:
					ret = "A system shutdown is in progress.";//ERROR_SHUTDOWN_IN_PROGRESS 
					break;
				default:
					ret = "Exception error.";
					break;
			}
			return ret;
		}
	}
}

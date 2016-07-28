using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HASPkey
{
	public class EnvConst
	{
		#region Common
		public const string MASTER_USERNAME = "admin";
		public const string MASTER_PASSWORD = "admin";

		public const string GUEST_USERNAME = "guest";
		public const string GUEST_PASSWORD = "guest";

		public const string AccessControlFile = "ACF.cfg";
		public const string HASP_KEYINFO = "<haspformat format=\"keyinfo\"/>";
		public const string HASP_UPDATEINFO = "<haspformat format=\"updateinfo\"/>";
		public const string HASP_SESSIONINFO = "<haspformat format=\"sessioninfo\"/>";
		public const string HASPID_XPATH = "hasp_info/keyspec/hasp/haspid";
		#endregion Common

		#region Hasp_lib

		public const int MEMO_BUFFER_SIZE = 32;     /* size in bytes */
		public const int PAC_OFFSET = 0;
		public const int VIDEO_FPS_OFFSET = 1;
		public const int AUDIO_OFFSET = 4;
		public const int IP_CAMERA_OFFSET = 7;

		//default values
		public const int PAC_DEFAULT_VALUE = 0;
		public const int FPS_DEFAULT_VALUE = 60;
		public const int AUDIO_DEFAULT_VALUE = 0;
		public const int IP_DEFAULT_VALUE = 0;

		//max values
		public const int PAC_MAX_VALUE = 16;
		public const int FPS_MAX_VALUE = 480;
		public const int AUDIO_MAX_VALUE = 8;
		public const int IP_MAX_VALUE = 24;

		//total FPS
		public const int FPS_VALUE_120 = 120;
		public const int FPS_VALUE_240 = 240;

		#endregion Hasp_lib
	}
	
	public enum USER_PRIVILEDGE
	{
		DISABLE_PRIVILEDGE = 0,
		READ_PRIVILEDGE = 1 << 1,
		WRITE_PRIVILEDGE = 1 << 2,

	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public struct USER_INFO
	{
		public int priviledge;
		public string username;
		public string password;
	}

	public class HASP_DATA
	{
		public int pacNumber;
		public int totalFPS;
		public int audioNumber;
		public int ipNumber;

		public HASP_DATA()
		{
			this.pacNumber = EnvConst.PAC_DEFAULT_VALUE;
			this.totalFPS = EnvConst.FPS_DEFAULT_VALUE;
			this.audioNumber = EnvConst.AUDIO_DEFAULT_VALUE;
			this.ipNumber = EnvConst.IP_DEFAULT_VALUE;
		}

		public HASP_DATA(int _pac, int _fps, int _audio, int _ip)
		{
			this.pacNumber = _pac;
			this.totalFPS = _fps;
			this.audioNumber = _audio;
			this.ipNumber = _ip;
		}
	}

}

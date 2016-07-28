using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.Utils;


namespace CMSWebApi.DataModels
{
	
	//public class  CMSWebSiteResultModel: TransactionalInformation
	//{
	//  public CMSWebSiteModel Sites{ get ; set;}
	//}

	public class CMSWebSiteModel
	{
		public int ID { get; set; }
		public string Name { get; set; }
		public string ServerID { get; set; }
        public bool IsVirtual { get; set; }
		public int? Status { get; set; }
		public int? OnlineStatus { get; set; }
		public Nullable<int> UserID { get; set; }
		public Nullable<int> ParentKey { get; set; }
		public string ImageSite{ get;set;}
		/// <summary>
		///Site already PACDM database
		/// </summary>
		public bool PACData { get; set; }
		public int PACIDs { get; set; }

		List<CMSWebSiteModel>sites = new List<CMSWebSiteModel>();
		public List<CMSWebSiteModel> Sites { get { return sites; } set { sites = value; } }
		public int SiteCount { get; set; }
		public SiteType Type { get; set; }
		private bool? ischecked = false;
		public bool? Checked{ get{ return ischecked;} set { ischecked = value;}}
		public string MACAddress { get; set; }
        public string HaspKey { get; set; }
	}

	public class CMSPACSiteModel {
		public int PacId { get; set; }
		public string SiteName { get; set; }
		public int SiteKey { get; set; }
		public int KDVR { get; set; }
	}

	public class TreeMetric
	{
		private List<TreeMetric> _childs = new List<TreeMetric>();
		public TreeMetric()
		{
			Childs = new List<TreeMetric>();
		}

		public int Id { get; set; }
		public string Name { get; set; }
		public int? ParentId { get; set; }
		public bool? Checked { get; set; }
		public MetricType MetricType { get; set; }
		public List<TreeMetric> Childs
		{
			get { return _childs; }
			set { _childs = value; }
		}
	}

	public class RegionModel
	{
		public int RegionKey { get; set; }
		public int? UserKey { get; set; }
		[Required(ErrorMessage = "REGIONNAME_REQUIRED_NAME")]
		[MaxLength(100, ErrorMessage = "MAX_LENGTH_100")]
		public string RegionName { get; set; }
		public int? RegionParentId { get; set; }
		[MaxLength(250, ErrorMessage = "MAX_LENGTH_250")]
		public string Description { get; set; }
	}

	public class WorkingHours
	{
		public int ScheduleId { get; set; }
		public int SiteId { get; set; }
		public DateTime? OpenTime { get; set; }
		public DateTime? CloseTime { get; set; }
	}

	public class CmsSites
	{
		public CmsSites()
		{
			WorkingHours = new List<WorkingHours>();
			CalendarEvent = new List<int>();
			DvrMetrics = new List<int>();
			Macs = new List<MAC>();
			DvrUsers = new List<int>();
			Files = new List<ImageModel>();
			HaspLicense = new List<HaspLicense>();
		}

		public int SiteKey { get; set; }
		[Required(ErrorMessage = "FIELD_REQUIRED")]
		[MaxLength(50, ErrorMessage = "MAX_LENGTH_50")]
		public string ServerId { get; set; }
		//[Required(ErrorMessage = "FIELD_REQUIRED")]
		//public string MacAddress { get; set; }
		[MaxLength(50, ErrorMessage = "MAX_LENGTH_50")]
		public string AddressLine1 { get; set; }
		[MaxLength(50, ErrorMessage = "MAX_LENGTH_50")]
		public string AddressLine2 { get; set; }
		[MaxLength(50, ErrorMessage = "MAX_LENGTH_50")]
		public string City { get; set; }
		public int? StateProvince { get; set; }
		public int? Country { get; set; }
		public string PostalZipCode { get; set; }
		public int? PostalZipCodeId { get; set; }
		public int? UserId { get; set; }
		public int? RegionKey { get; set; }
		[MaxLength(50, ErrorMessage = "MAX_LENGTH_50")]
		public string ImageSite { get; set; }
		public byte[] ImageSiteBytes { get; set; }
		//public short? MapX { get; set; }
		//public short? MapY { get; set; }
		[MaxLength(50, ErrorMessage = "MAX_LENGTH_50")]
		public string HeatMapImage { get; set; }
		public short? HeatMapX { get; set; }
		public short? HeatMapY { get; set; }
		[MaxLength(25, ErrorMessage = "MAX_LENGTH_25")]
		public string HaspkeyId { get; set; }
		public int? GoalId { get; set; }
		public bool? Deleted { get; set; }
		public int? Syn { get; set; }
		[MaxLength(50, ErrorMessage = "MAX_LENGTH_50")]
		public string LandlordCode { get; set; }
		[MaxLength(50, ErrorMessage = "MAX_LENGTH_50")]
		public string LandlordPhone { get; set; }
		[MaxLength(50, ErrorMessage = "MAX_LENGTH_50")]
		public string MallCode { get; set; }
		[MaxLength(50, ErrorMessage = "MAX_LENGTH_50")]
		public string Address3 { get; set; }
		[MaxLength(50, ErrorMessage = "MAX_LENGTH_50")]
		public string Management { get; set; }
		[MaxLength(50, ErrorMessage = "MAX_LENGTH_50")]
		public string MgtPhone { get; set; }
		[MaxLength(50, ErrorMessage = "MAX_LENGTH_50")]
		public string MgtAdress { get; set; }
		[MaxLength(50, ErrorMessage = "MAX_LENGTH_50")]
		public string StoreCategory { get; set; }
		[MaxLength(50, ErrorMessage = "MAX_LENGTH_50")]
		public string StoreType { get; set; }
		[MaxLength(50, ErrorMessage = "MAX_LENGTH_50")]
		public string StoreGroup { get; set; }
		[MaxLength(500, ErrorMessage = "MAX_LENGTH_500")]
		public string StoreDes { get; set; }
		[MaxLength(50, ErrorMessage = "MAX_LENGTH_50")]
		public string FinancialIns { get; set; }
		[MaxLength(50, ErrorMessage = "MAX_LENGTH_50")]
		public string PolicyNo { get; set; }
		[MaxLength(50, ErrorMessage = "MAX_LENGTH_50")]
		public string AccountNo { get; set; }
		[MaxLength(50, ErrorMessage = "MAX_LENGTH_50")]
		public string BranchNo { get; set; }
		[MaxLength(50, ErrorMessage = "MAX_LENGTH_50")]
		public string Address4 { get; set; }
		[MaxLength(50, ErrorMessage = "MAX_LENGTH_50")]
		public string PhoneSite { get; set; }
		[MaxLength(50, ErrorMessage = "MAX_LENGTH_50")]
		public string PhoneFinance { get; set; }
		[MaxLength(50, ErrorMessage = "MAX_LENGTH_50")]
		public string FaxSite { get; set; }
		[MaxLength(50, ErrorMessage = "MAX_LENGTH_50")]
		public string FaxFinance { get; set; }
		[MaxLength(50, ErrorMessage = "MAX_LENGTH_50")]
		public string TlogVersion { get; set; }
		public double? SquareFeet { get; set; }
		public double? StoreSellingSquareFeet { get; set; }
		public decimal? AnnualBudget { get; set; }
		[MaxLength(50, ErrorMessage = "MAX_LENGTH_50")]
		public string ActualBudget { get; set; }
		public byte[] ActualBudgetBytes { get; set; }
		public double? TotalBugetHour { get; set; }
		public double? TotalStaffHour { get; set; }
		public decimal? SalePerSquareFeet { get; set; }
		public double? TotalHourUsed { get; set; }
		[MaxLength(50, ErrorMessage = "MAX_LENGTH_50")]
		public string ManagerAsm { get; set; }
		[MaxLength(50, ErrorMessage = "MAX_LENGTH_50")]
		public string Layout { get; set; }
		[MaxLength(50, ErrorMessage = "MAX_LENGTH_50")]
		public string FixturePlan { get; set; }
		public byte[] FixtureBytes { get; set; }
		[MaxLength(50, ErrorMessage = "MAX_LENGTH_50")]
		public string SecuritySystemCode { get; set; }
		public DateTime? InstallRemoveDate { get; set; }
		[MaxLength(50, ErrorMessage = "MAX_LENGTH_50")]
		public string SecurityServiceCode { get; set; }
		public DateTime? ServiceStartDate { get; set; }
		public DateTime? ServiceEndDate { get; set; }
		public DateTime? OpenDate { get; set; }
		public DateTime? CloseDate { get; set; }
		public DateTime? RemodellingOpenDate { get; set; }
		public DateTime? RemodellingCloseDate { get; set; }
		public DateTime? LeaseStartDate { get; set; }
		public DateTime? LeaseEndDate { get; set; }
		//public byte[] StoreImage { get; set; }
		public bool? AlertImage { get; set; }
		public IEnumerable<WorkingHours> WorkingHours { get; set; }
		public IEnumerable<int> CalendarEvent { get; set; }
		public IEnumerable<int> DvrMetrics { get; set; }
		public IEnumerable<int> DvrUsers { get; set; }
		public IEnumerable<MAC> Macs { get; set; }
		public IEnumerable<ImageModel> Files { get; set; }
		public IEnumerable<HaspLicense> HaspLicense { get; set; }
	}

	public class MAC
	{
		public MAC()
		{
			Files = new List<FileSiteModel>();
		}

		public int Id { get; set; }
		public string MacAddress { get; set; }
		public string DvrName { get; set; }
		public string Image { get; set; }

		public ICollection<FileSiteModel> Files { get; set; }
	}

	public class HaspLicense
	{
		public int KDVR { get; set; }
		public string SerialNumber { get; set; }
		public string ServerID { get; set; }
	}

	public class DVRModel 
	{
		public int? KDVR;
		public string DVRGuid;
		public string ServerID;
		public string ServerIP;
		public int? Online;
		public string PublicServerIP;
		public int? TotalDiskSize;
		public int? FreeDiskSize;
		public string DVRAlias;
		public int? EnableActivation;
		public DateTime? ActivationDate;
		public DateTime? ExpirationDate;
		public int? RecordingDay;
		public DateTime? FirstAccess;
		public int? KLocation;
		public DateTime? TimeDisConnect;
		public int? DisConnectReason;
		public int? CMSMode;
		public int? LastConnectTime;
		public int? CurConnectTime;
		public int? KGroup;
		public int? KDVRVersion;
		public string HaspLicense;
		public string MinDateRec;
		public string MaxDateRec;
		public List<Channels> Channels;
	}

    public class DVRInfoModel
    {
        public int? KDVR;
        public string ServerID;
        public string ServerIP;
        public int Port;
    }

	public class Channels
	{
		public	int?  ChannelNo;
		public	int? KDVR;
		public	int?  KChannel;
		public	int?  VideoSource;
		public	int?  KAudioSource;
		public	int?  KPTZ;
		public	int?  Status;
		public	string  Name;
		public	int?  Enable;
		public	int?  DwellTime;
		public	int?  AP;
		public	int?  CameraID;
		public	int?  VideoCompressQual;
		public	int?  VideoType;
		public	int?  KVideo;
		public	bool?  EnableiSearch;
		public string DVRName;
		public string FPS;
		public string Resolution;
		public string ModelName;
	}

	public class ChannelDetails
	{
		public int? KDVR { get; set; }
		public int? KChannel { get; set; }
		public string Enabled { get; set; }
		public string ChannelName { get; set; }
		public int? VideoSource { get; set; }
		public string CameraName { get; set; }
		public string CameraModel { get; set; }
		public int? VideoType { get; set; }
		public string Format { get; set; }
		public string Resolution { get; set; }
		public string FPS { get; set; }
		public string EmergencyFPS { get; set; }
	}

	public class AlertModel 
	{
		public int KAlertEvent;
		public int? KAlertType;
		public int? KDVR;
		public DateTime? TimeZone;
		public string DVRUser;
		public string Description;
		public DateTime? Time;
		public int? Channel;
		public string AlertType;


	}

	public class AlertModelSummary
	{
		public int KDVR { get; set; }
		public int TotalAlert { get; set; }
		public DateTime TimeZone { get; set; }
	}

	public class SiteDetailParam
	{
		public string macArr;
		public int siteKey;
	}

	public class ZipCodeModel
	{
		public int ZipCodeID { get; set; }
		public string ZipCode { get; set; }
	}

	public class AllMacFilesModel
	{
		public int[] listKDVR { get; set; }
		public int siteKey { get; set; }
	}
	public class DVRPACChannel
	{
		public int KDVR{ get ;set;}
		public int KChannel{ get; set;}
		public int DVR_ChannelNo{ get ;set;}
		public string Name{ get ;set;}
		public int PAC_ChannelID{get ;set;}
		public int PAC_ChannelName{get ;set;}
		public int Dim_PACID{ get ;set;}
		public bool EnableTrafficCount{ get ;set;}
	}
    public class DVRInfoRebarTransact
    {
        public int KDVR { get; set; }
        public string MACAddress { get; set;}
        public string ServerID { get; set; }
        public string ServerIP { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string PublicServerIP { get; set; }
        public string DVRVersion { get; set; }

    }
}

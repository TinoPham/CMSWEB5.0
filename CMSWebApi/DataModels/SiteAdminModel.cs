using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace CMSWebApi.DataModels
{
	public class TreeSite
	{
		public TreeSite()
		{
			Nodes = new List<TreeSite>();
		}

		public int Id { get; set; }
		public string Title { get; set; }
		public bool IsRegion { get; set; }
		public bool IsRoot { get; set; }
		public int Status { get; set; } //1: selected, 0: Unselected, 2: for Region.
		public int RegionParentId { get; set; }
		public int SiteCount { get; set; }
		public int RegionKey { get; set; }
		public int? UserKey { get; set; }
		public int SiteKey { get; set; }
		public List<TreeSite> Nodes { get; set; }
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
		public List<TreeMetric> Childs {
			get { return _childs; }
			set { _childs = value; }
		}
	}

	public class RegionModel
	{
		public int RegionKey { get; set; }
		public int? UserKey { get; set; }
		[Required(ErrorMessage = "REGIONNAME_REQUIRED_NAME")]
		public string RegionName { get; set; }
		public int? RegionParentId { get; set; }
		[MaxLength(500,ErrorMessage = "MAX_LENGTH_500")]
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
			CalendarEvent = new List<int?>();
			DvrMetrics = new List<int>();
			DvrUsers = new List<int>();
		}

		public int SiteKey { get; set; }
		[Required(ErrorMessage = "FIELD_REQUIRED")]
		public string ServerId { get; set; }
		[Required(ErrorMessage = "FIELD_REQUIRED")]
		public string MacAddress { get; set; }
		public string AddressLine1 { get; set; }
		public string AddressLine2 { get; set; }
		public string City { get; set; }
		public int? StateProvince { get; set; }
		public int? Country { get; set; }
		public string PostalZipCode { get; set; }
		public int? UserId { get; set; }
		public int? RegionKey { get; set; }
		public string ImageSite { get; set; }
		public short? MapX { get; set; }
		public short? MapY { get; set; }
		public string HeatMapImage { get; set; }
		public short? HeatMapX { get; set; }
		public short? HeatMapY { get; set; }
		public string HaspkeyId { get; set; }
		public int? GoalId { get; set; }
		public bool? Deleted { get; set; }
		public int? Syn { get; set; }
		public string LandlordCode { get; set; }
		public string LandlordPhone { get; set; }
		public string MallCode { get; set; }
		public string Address3 { get; set; }
		public string Management { get; set; }
		public string MgtPhone { get; set; }
		public string MgtAdress { get; set; }
		public string StoreCategory { get; set; }
		public string StoreType { get; set; }
		public string StoreGroup { get; set; }
		public string StoreDes { get; set; }
		public string FinancialIns { get; set; }
		public string PolicyNo { get; set; }
		public string AccountNo { get; set; }
		public string BranchNo { get; set; }
		public string Address4 { get; set; }
		public string PhoneSite { get; set; }
		public string PhoneFinance { get; set; }
		public string FaxSite { get; set; }
		public string FaxFinance { get; set; }
		public string TlogVersion { get; set; }
		public double? SquareFeet { get; set; }
		public double? StoreSellingSquareFeet { get; set; }
		public decimal? AnnualBudget { get; set; }
		public string ActualBudget { get; set; }
		public double? TotalBugetHour { get; set; }
		public double? TotalStaffHour { get; set; }
		public decimal? SalePerSquareFeet { get; set; }
		public double? TotalHourUsed { get; set; }
		public string ManagerAsm { get; set; }
		public string Layout { get; set; }
		public string FixturePlan { get; set; }
		public string SecuritySystemCode { get; set; }
		public DateTime? InstallRemoveDate { get; set; }
		public string SecurityServiceCode { get; set; }
		public DateTime? ServiceStartDate { get; set; }
		public DateTime? ServiceEndDate { get; set; }
		public DateTime? OpenDate { get; set; }
		public DateTime? CloseDate { get; set; }
		public DateTime? RemodellingOpenDate { get; set; }
		public DateTime? RemodellingCloseDate { get; set; }
		public DateTime? LeaseStartDate { get; set; }
		public DateTime? LeaseEndDate { get; set; }
		public byte[] StoreImage { get; set; }
		public bool? AlertImage { get; set; }
		public List<WorkingHours> WorkingHours { get; set; }
		public List<int?> CalendarEvent { get; set; }
		public List<int> DvrMetrics { get; set; }
		public List<int> DvrUsers { get; set; }
	}
}

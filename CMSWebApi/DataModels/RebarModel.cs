using System.ComponentModel.DataAnnotations;
using CMSWebApi.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.DataModels
{
	public class GroupPaymentModel
	{
		public int Id { get; set; }
		public int? PacId { get; set; }
		public string Name { get; set; }
		public decimal TotalTran { get; set; }
	}

	public class BoxRebarParamModel
	{
		public List<int> Types { get; set; }
		public List<int> SiteKeys { get; set; }
		public DateTime StartTranDate { get; set; }
		public DateTime EndTranDate { get; set; }
	}
	public class BoxRebarModel
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public decimal? Total { get; set; }
		public decimal? TotalAmmount { get; set; }
		public decimal? TotalTran { get; set; }
	}
	public class EmployerParamModel
	{
		public List<int> Types { get; set; }
		public List<int> SiteKeys { get; set; }
		public DateTime StartTranDate { get; set; }
		public DateTime EndTranDate { get; set; }
		public int PageNo { get; set; }
		public int PageSize { get; set; }
	}

	public class PagingBase
	{
		public int CurrentPage { get; set; }
		public int TotalPages { get; set; }
	}

	public class EmployerPagingModel : PagingBase
	{
		public List<EmployerRebarModel> Data { get; set; }
	}
	public class EmployerModel : PagingBase
	{
		//public int CurrentPage { get; set; }
		//public int TotalPages { get; set; }
		public List<EmployerPagings> Data { get; set; }
	}
	public class EmployerPagings
	{
		public int? Id { get; set; }
		public int? PACID { get; set; }
		public int? SiteKey { get; set; }
		public string Name { get; set; }
		public string SiteName { get; set; }
		public decimal TotalException { get; set; }
		public List<EmployerRebarModel> Charts { get; set; }
	}
	public class EmployerRebarModel
	{
		public int? PacId { get; set; }
		public int? SiteKey { get; set; }
		public int? StoreId { get; set; }
		public string StoreName { get; set; }
		public int? EmployerId { get; set; }
		public string EmployerName { get; set; }
		public int TypeId { get; set; }
		public string TypeName { get; set; }
		public decimal Value { get; set; }
		public string Color { get; set; }
		public int? Weight { get; set; }
	}
	public class EmplTransacParam
	{
		public int PacId { get; set; }
		public List<int> FilterPayments { get; set; }
		public int? EmployerId { get; set; }
		public List<int> Types { get; set; }
		public string StartTranDate { get; set; }
		public string EndTranDate { get; set; }
		public int PageNo { get; set; }
		public int PageSize { get; set; }
	}
	public class EmplTranction
	{
		public long TranId { get; set; }
		public long? TranNo { get; set; }
		public int? PacId { get; set; }
		public int? EmpId { get; set; }
		public DateTime? DateTran { get; set; }
		public decimal? Total { get; set; }
		public IEnumerable<ExceptionNotes> Notes { get; set; }
		public IEnumerable<TranPayment> Payments { get; set; }
		public IEnumerable<TranExceptionType> ExceptionTypes { get; set; }
	}

	public class PaymTranction
	{
		public long TranId { get; set; }
		public long? TranNo { get; set; }
		public int? PacId { get; set; }
		public int? EmpId { get; set; }
		public DateTime? DateTran { get; set; }
		public int? PaymentId { get; set; }
		public string PaymentMethod { get; set; }
		public decimal? Total { get; set; }
		public IEnumerable<ExceptionNotes> Notes { get; set; }
		public IEnumerable<TranExceptionType> ExceptionTypes { get; set; }
	}


	public class PaymTranctionPagings : PagingBase
	{
		//public int CurrentPage { get; set; }
		//public int TotalPages { get; set; }
		public List<PaymTranction> Data { get; set; }
	}

	public class EmplTranctionPagings : PagingBase
	{
		//public int CurrentPage { get; set; }
		//public int TotalPages { get; set; }
		public List<EmplTranction> Data { get; set; }
	}


	public class AdhocParam
	{
		public List<int> keys { get; set; }
		public string Where { get; set; }
		public string Groupby { get; set; }
		public string Select { get; set; }
		public string Orderby { get; set; }
		public int PageNo { get; set; }
		public int PageSize { get; set; }
	}

	public class AhocFilterPagings : PagingBase
	{
		//public int CurrentPage { get; set; }
		//public int TotalPages { get; set; }
		public List<AhocFilterModel> Data { get; set; }
	}

	public class AhocFilterModel
	{
		public int? PacId { get; set; }
		public long Id { get; set; }
		public string Name { get; set; }
		public decimal Total { get; set; }
		public decimal Count { get; set; }
	}

	public class TransactionDetailViewerModel
	{
		public long TranId { get; set; }
		public long? TranNo { get; set; }
		public int? PacId { get; set; }
		public int? StoreId { get; set; }
		public string StoreName { get; set; }
		public int? EmployeeId { get; set; }
		public string EmployeeName { get; set; }
		public int? RegisterId { get; set; }
		public string RegisterName { get; set; }
		public int? CamId { get; set; }
		public string CamName { get; set; }
		public int? ShiftId { get; set; }
		public string ShiftName { get; set; }
		public int? CheckId { get; set; }
		public string CheckName { get; set; }
		public int? CardId { get; set; }
		public string CardName { get; set; }
		public int? TerminalId { get; set; }
		public string TerminalName { get; set; }
		public DateTime? TranDate { get; set; }
		public DateTime? DvrDate { get; set; }
		public int? Year { get; set; }
		public int? Quarter { get; set; }
		public int? Month { get; set; }
		public int? Week { get; set; }
		public int? Day { get; set; }
		public int? Hour { get; set; }
		public decimal? SubTotal { get; set; }
		public decimal? ChangeAmount { get; set; }
		public decimal Total { get; set; }
		public virtual IEnumerable<TranTax> Taxs { get; set; }
		public virtual IEnumerable<TranPayment> Payments { get; set; }
		public virtual IEnumerable<TranExceptionType> ExceptionTypes { get; set; }
		public virtual IEnumerable<ExceptionNotes> Notes { get; set; }
		public long RetailId { get; set; }
		public int? LineNo { get; set; }
		public double? Qty { get; set; }
		public decimal? Amount { get; set; }
		public long? DescId { get; set; }
		public string DescName { get; set; }
		public long? ItemCodeId { get; set; }
		public string ItemCode { get; set; }
		public long? TOBox { get; set; }
	}

	public class TransactionViewerModel
	{
		public long TranId { get; set; }
		public long? TranNo { get; set; }
		public int? PacId { get; set; }
		public int? StoreId { get; set; }
		public string StoreName { get; set; }
		public int? EmployeeId { get; set; }
		public string EmployeeName { get; set; }
		public int? RegisterId { get; set; }
		public string RegisterName { get; set; }
		public int? CamId { get; set; }
		public string CamName { get; set; }
		public int? ShiftId { get; set; }
		public string ShiftName { get; set; }
		public int? CheckId { get; set; }
		public string CheckName { get; set; }
		public int? CardId { get; set; }
		public string CardName { get; set; }
		public int? TerminalId { get; set; }
		public string TerminalName { get; set; }
		public DateTime? TranDate { get; set; }
		public DateTime? DvrDate { get; set; }
		public int? Year { get; set; }
		public int? Quarter { get; set; }
		public int? Month { get; set; }
		public int? Week { get; set; }
		public int? Day { get; set; }
		public int? Hour { get; set; }
		public decimal? SubTotal { get; set; }
		public decimal? ChangeAmount { get; set; }
		public decimal Total { get; set; }
		public virtual IEnumerable<TranTax> Taxs { get; set; }
		public virtual IEnumerable<TranPayment> Payments { get; set; }
		public virtual IEnumerable<TranExceptionType> ExceptionTypes { get; set; }
		public virtual IEnumerable<ExceptionNotes> Notes { get; set; }
		//public virtual IEnumerable<DescriptionModel> Descriptions { get; set; }
	}


	public class TranPaymentParam
	{
		public List<int> Types { get; set; }
		public List<int> SiteKeys { get; set; }
		public DateTime StartTranDate { get; set; }
		public DateTime EndTranDate { get; set; }
	}
	public class TranPaymentDetailParam
	{
		public int Type { get; set; }
		public List<int> SiteKeys { get; set; }
		public DateTime StartTranDate { get; set; }
		public DateTime EndTranDate { get; set; }
		public int PageNo { get; set; }
		public int PageSize { get; set; }
	}
	public class TranPaymentChartModel
	{
		public int? PaymentId { get; set; }
		public string PaymentName { get; set; }
		public decimal? Total { get; set; }
		public long TranCount { get; set; }
	}
	public class Transaction
	{
		public long TranId { get; set; }
		public byte[] CompanyLogo { get; set; }
		public long? TranNo { get; set; }
		public int? PacId { get; set; }
		public int? StoreId { get; set; }
		public string StoreName { get; set; }
		public int? EmployeeId { get; set; }
		public string EmployeeName { get; set; }
		public int? RegisterId { get; set; }
		public string RegisterName { get; set; }
		public int? CamId { get; set; }
		public string CamName { get; set; }
		public DateTime? TranDate { get; set; }
		public DateTime? DvrDate { get; set; }
		public IEnumerable<TransactionDetail> Details { get; set; }
		public decimal? SubTotal { get; set; }
		public IEnumerable<TranTax> Taxs { get; set; }
		public decimal? Total { get; set; }
		public IEnumerable<TranPayment> Payments { get; set; }
		public decimal? ChangeAmount { get; set; }
		public IEnumerable<TranExceptionType> ExceptionTypes { get; set; }
		public ExceptionNotes Note { get; set; }
	}

	public class TranExceptionType
	{
		[Key]  
		public int Id { get; set; }
		public string Name { get; set; }
		public string Desc { get; set; }
		public DateTime? FlagTime { get; set; }
		public int? TypeWeight { get; set; }
		public string Color { get; set; }
		public bool? IsSystem { get; set; }
	}
	
	public class ExceptionNotes
	{
		public int UserId { get; set; }
		public long TranId { get; set; }
		public string Note { get; set; }
		public DateTime? DateNotes { get; set; }
	}

	public class ExceptionModel
	{
		public long TranId { get; set; }
		public int? PacId { get; set; }
		public int? RegisterId { get; set; }
		public IEnumerable<TranExceptionType> ExceptionTypes { get; set; }
		public ExceptionNotes Note { get; set; }

	}


	public class TranPayment
	{
		[Key]
		public int? Id { get; set; }
		public string Name { get; set; }
		public decimal? Ammount { get; set; }
	}
	public class TranTax
	{
		[Key]
		public int Id { get; set; }
		public string Name { get; set; }
		public decimal? Ammount { get; set; }
	}
	public class TransactionDetail
	{
		public long Id { get; set; }
		public long? TranId { get; set; }
		public int? ItemLine { get; set; }
		public double? Qty { get; set; }
		public decimal? Total { get; set; }
		public string ItemCodeName { get; set; }
		public string DescriptionName { get; set; }
		public IEnumerable<TransactionSubDetail> SubDetails { get; set; }
	}
	public class TransactionSubDetail
	{
		public long Id { get; set; }
		public long? DetailId { get; set; }
		public int? ItemLine { get; set; }
		public double? Qty { get; set; }
		public decimal? Total { get; set; }
		public string DescriptionName { get; set; }
	}
	public class PaymentModel
	{
		public int ID { get; set; }
		public string Name { get; set; }
	}

	public class PaymentModelPaging : PagingBase
	{
		public IQueryable<PaymentModel> Data { get; set; }
		//public int CurrentPage { get; set; }
		//public int TotalPage { get; set; }
	}

	public class RegisterModel
	{
		public int ID { get; set; }
		public string Name { get; set; }
	}

	public class ModelPaging //: PagingBase
	{
		public IQueryable<object> Data { get; set; }
		public int CurrentPage { get; set; }
		public int TotalPage { get; set; }
	}

	public class OperatorModel
	{
		public int ID { get; set; }
		public string Name { get; set; }
	}
	public class DescriptionModel
	{
		public int ID { get; set; }
		public string Name { get; set; }
	}

	public class DescriptionTransModel
	{
		public int ID { get; set; }
		public string Name { get; set; }
		public long TransID { get; set; }
	}

	public class TransactionFilterParam
	{
		public long TranNo { get; set; }
		public string EmployeeId { get; set; }
		public string StartTranDate { get; set; }
		public string EndTranDate { get; set; }
		public int PageNo { get; set; }
		public int PageSize { get; set; }
        public List<int> Sites { get; set;}
	}

	public class TransactionFilterPagings : PagingBase
	{
		//public int CurrentPage { get; set; }
		//public int TotalPages { get; set; }
		public List<TransactionCompare> Data { get; set; }
	}

	public class TransactionCompare
	{
		public long TranId { get; set; }
		public long? TranNo { get; set; }
		public int? PacId { get; set; }
		public int? StoreId { get; set; }
		public string StoreName { get; set; }
		public int? RegisterId { get; set; }
		public int? EmployeeId { get; set; }
		public string EmployeeName { get; set; }
		public DateTime? TranDate { get; set; }
	}

	public class CannedRptModelBase
	{
		public decimal TransID { get; set; }
		public int TypeID { get; set; }
		public int PACID { get; set; }
		public int EmpID { get; set; }
		public string EmpName { get; set; }
		public int RegisID { get; set; }
		public string RegisName { get; set; }
		public int PaymentID { get; set; }
		public string PaymentName { get; set; }
		public decimal TransNB { get; set; }
		public decimal Amount { get; set; }
	}

	public class CannedRptModel: CannedRptModelBase
	{
		private List<ExceptionTransModel> _details = new List<ExceptionTransModel>();

		public int SiteKey { get; set; }
		public string SiteName { get; set; }
		public decimal TotalTrans { get; set; }
		public decimal TotalAmount { get; set; }
		public int GroupByField { get; set; }
		public List<CannedRptModel> Childs { get; set; }
		public List<ExceptionTransModel> Details {
			get { return _details; }
			set { _details = value; }
		}
	}

	public class ExceptionTransModel
	{
		public decimal TransID { get; set; }
		public decimal TransNB { get; set; }
		public int RegisID { get; set; }
		public string RegisName { get; set; }
		public int PaymentID { get; set; }
		public string PaymentName { get; set; }
		public decimal Amount { get; set; }
		public string Notes { get; set; }
		public bool Tracking { get; set; }
		public int TotalFlag { get; set; }
		public int TotalWeight { get; set; }
		public DateTime DVRDate { get; set; }
	}

	public class ExceptionResultModel
	{
		public int? PACID { get; set; }
		public long? TransID { get; set; }
		public int? TypeID { get; set; }
		public int? EmpID { get; set; }
		public string EmpName { get; set; }
		public int? RegisterID { get; set; }
		public string RegisterName { get; set; }
		public int? PaymentID { get; set; }
		public string PaymentName { get; set; }
		public long? TransNB { get; set; }
		public decimal? TotalAmount { get; set; }
		public int? DescriptionID { get; set; }
		public string DescriptionName { get; set; }
		public DateTime? DVRDate { get; set; }
		public DateTime? TransDate { get; set; }
		public decimal? TAX { get; set; }
		public decimal? ChangeAmount { get; set; }
		public int? TerminalID { get; set; }
		public int? StoreID { get; set; }
		public int? CheckID { get; set; }
		public string ExtraString { get; set; }
		public decimal? ExtraNumber { get; set; }
		public bool? Tracking { get; set; }
		public int? SiteKey { get; set; }
		public string SiteName { get; set; }
	}

	public class EmployeeRiskSummaryPagings : PagingBase
	{
		//public int CurrentPage { get; set; }
		//public int TotalPages { get; set; }
        public int SumRiskFactors { get; set; }
		public List<EmployeeRiskSummary> Data { get; set; }
	}

	public class EmployeeRiskSummary : TransInfoSite
	{
		//public int? SiteKey { get; set; }
		//public string SiteName { get; set; }
		//public int? PacId { get; set; }
		public int? StoreId { get; set; }
		public string StoreName { get; set; }
		public int? EmployerId { get; set; }
		public string EmployerName { get; set; }
		public decimal RiskFactor { get; set; }
		//public decimal TotalTran { get; set; }
		//public decimal TotalAmmount { get; set; }
		//public decimal PercentToSale { get; set; }
        public int Day { set; get; }
        public int Month { set; get; }
        public int Year { set; get; }
	}

	public class RebarWeekAtAGlanceParam
	{
		public int Type { get; set; }
		public List<int> SiteKeys { get; set; }
		public DateTime StartTranDate { get; set; }
		public DateTime EndTranDate { get; set; }
		public int PageNo { get; set; }
		public int PageSize { get; set; }
        public string Employees { get; set; }
        public int Sort { set; get; }
        public int GroupBy { set; get; }
	}

	public class SiteRiskSummaryPagings : PagingBase
	{
		//public int CurrentPage { get; set; }
		//public int TotalPages { get; set; }
		public int SumRiskFactors { get; set; }
		public List<SiteRiskSummarySummary> Data { get; set; }
	}

	public class SiteRiskSummarySummary : TransInfoSite
	{
		//public int? SiteKey { get; set; }
		//public string SiteName { get; set; }
		//public int? PacId { get; set; }
		//public decimal RiskFactor { get; set; }
		//public decimal TotalAmmount { get; set; }
		//public decimal PercentToSale { get; set; }
		//public decimal TotalTran { get; set; }
	}

	public class SummaryWeekAtGlanceModel
	{
		public int? Id { get; set; }
		public string Name { get; set; }
		public decimal TotalTrans { get; set; }
		public decimal TotalAmmount { get; set; }

		public string Channels { get; set; }
		public string KDVRs { get; set; }
	}

	public class PagingParam
	{
		public int PageSize { get; set; }
		public int PageNumber { get; set; }
	}

	public class FilterDataParam
	{
		public string FilterText { get; set; }
		public int DataType { get; set; } 
	}

	public class ColumnModel
	{
		public int ID { get; set; }
		public string Name { get; set; }
	}

	public class ColumnOptionModel
	{
		public string Name { get; set; }
		public string PrimaryField { get; set; }
		public IQueryable<ColumnModel> Data { get; set; }
	}

	public class ColumnOptionParams
	{
		public string Name { get; set; }
		public string PrimaryField { get; set; }
		public string Keys { get; set; }
	}

	public class SiteInfoBase
	{
		public int? SiteKey { get; set; }
		public string SiteName { get; set; }
		public int? PacId { get; set; }
	}
	public class TransInfoSite : SiteInfoBase
	{
		public decimal RiskFactor { get; set; }
		public decimal TotalAmmount { get; set; }
		public decimal PercentToSale { get; set; }
		public decimal TotalTran { get; set; }
		public DateTime Date { get; set; }
		public int EmployerId { get; set; }
		public string EmployerName { get; set; }
	}
	public class CustomersInfoSite : SiteInfoBase
	{
		public int RiskFactor { get; set; }
		public decimal Percent { get; set; }
		public int Total { get; set; }
		public DateTime Date { get; set; }
	}
	//public class CarsInfoSite : SiteInfoBase
	//{
	//	public int TotalAmmount { get; set; }
	//	public decimal Percent { get; set; }
	//	public decimal TotalTran { get; set; }
	//}

	public class SiteExceptionTransPagings : PagingBase
	{
		public List<TransInfoSite> Data { get; set; }
	}

	public class SiteExceptionCustsPagings : PagingBase
	{
		public List<CustomersInfoSite> Data { get; set; }
	}
	//public class SiteExceptionCarsPagings : PagingBase
	//{
	//	public List<CustomersInfoSite> Data { get; set; }
	//}

	public class ExceptionIOPC
	{
		public int PACID { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
	}
	public class ExceptionCarInfo : ExceptionIOPC
	{
		public int ExternalCamera { get; set; }
		public int InternalCamera { get; set; }
	}
	public class ExceptionCustomerInfo : ExceptionIOPC
	{
		public DateTime DVRDate { get; set; }
		public int RegionIndex { get; set; }
		public string RegionName { get; set; }
		public string PersonID { get; set; }
		public int ExternalCamera { get; set; }
	}
	public class ExceptionCustomerPagings : PagingBase
	{
		public List<ExceptionCustomerInfo> Data { get; set; }
	}
	public class ExceptionCarPagings : PagingBase
	{
		public List<ExceptionCarInfo> Data { get; set; }
	}

	public enum SortField 
	{
		Employee = 0,
		RiskFactor,
		TotalAmount,
		RatioToSale
	}

	public enum GroupBy
	{
		SITE = 0,
		EMPL,
		DATE
	}



}

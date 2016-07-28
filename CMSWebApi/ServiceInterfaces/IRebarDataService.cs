using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.DataModels;
using PACDMModel.Model;
using CMSWebApi.DataModels.ModelBinderProvider;

namespace CMSWebApi.ServiceInterfaces
{

	public interface IRebarDataService
	{
		IQueryable<tbl_Exception_Transact> GetMetricRebar();
		IQueryable<tbl_POS_TaxesList> GetTaxtLists();
		IQueryable<tbl_Exception_Transact> GetExceptionTransactions();
		IQueryable<tbl_Exception_Type> GetTransactionTypes();
		IQueryable<tbl_Exception_Transact> GetMetricEmployerRebar();
		IQueryable<tbl_Exception_FlaggedTrans> GetTransExceptionType();
		IQueryable<tbl_Exception_TransPayment> GetTransExceptionPayments();
		IQueryable<tbl_Exception_Retail> GetTransExceptionDetail();
		IQueryable<tbl_Exception_TransTaxes> GetTransExceptionTaxes();
		IQueryable<tbl_POS_PaymentList> GetPOSPaymentList();
		IQueryable<tbl_POS_RegisterList> GetPOSRegisterList();
		IQueryable<tbl_POS_OperatorList> GetPOSOperatorList();
		IQueryable<tbl_POS_DescriptionList> GetPOSDescriptionList();
		IQueryable<tbl_POS_TerminalList> GetPOSTerminalList();
		IQueryable<tbl_POS_StoreList> GetPOSStoreList();
		IQueryable<tbl_POS_CheckIDList> GetPOSCheckIDList();
		IQueryable<tbl_Exception_Notes> GetTransExceptionNotes();
		IQueryable<tbl_POS_Transact> GetPosTransactions();
		IQueryable<Fact_POS_Transact> GetFactTransactions();
		IQueryable<tbl_POS_Retail> GetPosTransactionsDetail();
		IQueryable<tbl_POS_TransPayment> GetPosTransactionPayments();
		IQueryable<tbl_POS_TransTaxes> GetPosTransactionTaxes();
		IQueryable<tbl_Exception_Type> GetExcepFlagTypes();
		IQueryable<tbl_IOPC_TrafficCount> GetIOPCTraffics();
		IQueryable<tbl_IOPC_DriveThrough> GetIOPCDriveThroughs();

		void InsertExcepFlagType(tbl_Exception_Type type);
		void UpdateExcepFlagType(tbl_Exception_Type type);
		void DeleteExcepFlagType(tbl_Exception_Type type);

		void SaveTransactionNotes(tbl_Exception_Notes note);
		void UpdateTransactionNotes(tbl_Exception_Notes note);
		void InsertExcepTransaction(tbl_Exception_Transact tran);
		void InsertExcepFlag(tbl_Exception_FlaggedTrans tranFlag);
		void UpdateExcepFlag(tbl_Exception_FlaggedTrans tranFlag);
		void DeleteExcepFlag(tbl_Exception_FlaggedTrans tranFlag);
		void DeleteExcepTranByTranId(long tranId);
		void DeleteExcepNoteByTranId(long tranId);
		void DeleteExcepFlagByTranId(long tranId);
		void DeleteExcepTranDetailByTranId(long tranId);
		void DeleteExcepTranTaxByTranId(long tranId);
		void DeleteExcepTranPaymentByTranId(long tranId);
		void DeleteExcepTranSubDetailByTranId(long tranDetailId);
		int Save();
		IQueryable<Proc_Exception_WeekAtAGlane_HeaderCount_Result> GetWeekAtGlanceSummary(string pacids, DateTime startdate,DateTime enddate);
		IQueryable<Proc_Exception_TransWOC_Result> GetTransWOCust(string pacids, DateTime startdate, DateTime enddate);
		IQueryable<Proc_Exception_CustsWOT_Result> GetCustsWOTran(string pacids, DateTime startdate, DateTime enddate);
		IQueryable<Proc_Exception_CarsWOT_Result> GetCarsWOTran(string pacids, DateTime startdate, DateTime enddate);
		IQueryable<tbl_IOPC_TrafficCountRegion> GetRegionName(List<int> regIndexs);
	}
}

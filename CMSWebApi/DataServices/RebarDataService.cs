using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.DataModels;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;
using System.Data.SqlClient;
using System.Data;
using CMSWebApi.DataModels.ModelBinderProvider;

namespace CMSWebApi.DataServices
{
	public class RebarDataService : ServiceBase, IRebarDataService
	{
		public RebarDataService(IResposity model) : base(model) { }

		public RebarDataService(ServiceBase ServiceBase) : base(ServiceBase.DBModel) { }

		public IQueryable<tbl_Exception_Transact> GetMetricRebar()
		{
			return DBModel.QueryNoTrack<tbl_Exception_Transact>();
		}

		public IQueryable<tbl_POS_TaxesList> GetTaxtLists()
		{
			return DBModel.QueryNoTrack<tbl_POS_TaxesList>();
		}

		public IQueryable<tbl_Exception_Transact> GetExceptionTransactions()
		{
			return DBModel.Query<tbl_Exception_Transact>();
		}

		public IQueryable<tbl_Exception_Type> GetTransactionTypes()
		{
			return DBModel.QueryNoTrack<tbl_Exception_Type>();
		}

		public IQueryable<tbl_Exception_Transact> GetMetricEmployerRebar()
		{
			return DBModel.QueryNoTrack<tbl_Exception_Transact>().Include(t => t.tbl_POS_OperatorList).Include(t=>t.tbl_POS_StoreList).AsNoTracking();
		}

		public IQueryable<tbl_Exception_FlaggedTrans> GetTransExceptionType()
		{
			return DBModel.QueryNoTrack<tbl_Exception_FlaggedTrans>();
		}

		public IQueryable<tbl_Exception_Retail> GetTransExceptionDetail()
		{
			return DBModel.QueryNoTrack<tbl_Exception_Retail>();
		}

		public IQueryable<tbl_Exception_TransPayment> GetTransExceptionPayments()
		{
			return DBModel.QueryNoTrack<tbl_Exception_TransPayment>().Include(t=>t.tbl_POS_PaymentList).AsNoTracking();
		}

		public IQueryable<tbl_Exception_TransTaxes> GetTransExceptionTaxes()
		{
			return DBModel.QueryNoTrack<tbl_Exception_TransTaxes>().Include(t => t.tbl_POS_TaxesList).AsNoTracking();
		}
		
		public IQueryable<tbl_POS_PaymentList> GetPOSPaymentList()
		{
			return DBModel.QueryNoTrack<tbl_POS_PaymentList>();
		}

		public IQueryable<tbl_POS_RegisterList> GetPOSRegisterList()
		{
			return DBModel.QueryNoTrack<tbl_POS_RegisterList>();
		}

		public IQueryable<tbl_POS_OperatorList> GetPOSOperatorList()
		{
			return DBModel.QueryNoTrack<tbl_POS_OperatorList>();
		}

		public IQueryable<tbl_POS_DescriptionList> GetPOSDescriptionList()
		{
			return DBModel.QueryNoTrack<tbl_POS_DescriptionList>();
		}
		public IQueryable<tbl_POS_TerminalList> GetPOSTerminalList()
		{
			return DBModel.QueryNoTrack<tbl_POS_TerminalList>();
		}
		public IQueryable<tbl_POS_StoreList> GetPOSStoreList()
		{
			return DBModel.QueryNoTrack<tbl_POS_StoreList>();
		}
		public IQueryable<tbl_POS_CheckIDList> GetPOSCheckIDList()
		{
			return DBModel.QueryNoTrack<tbl_POS_CheckIDList>();
		}
	
		public IQueryable<tbl_Exception_Notes> GetTransExceptionNotes()
		{
			return DBModel.QueryNoTrack<tbl_Exception_Notes>();
		}

		public IQueryable<tbl_POS_Transact> GetPosTransactions()
		{
			return DBModel.QueryNoTrack<tbl_POS_Transact>();
		}

		public IQueryable<Fact_POS_Transact> GetFactTransactions()
		{
			return DBModel.QueryNoTrack<Fact_POS_Transact>();
		}

		public IQueryable<tbl_POS_Retail> GetPosTransactionsDetail()
		{
			return DBModel.QueryNoTrack<tbl_POS_Retail>();
		}

		public IQueryable<tbl_POS_TransPayment> GetPosTransactionPayments()
		{
			return DBModel.QueryNoTrack<tbl_POS_TransPayment>().Include(t => t.tbl_POS_PaymentList).AsNoTracking();
		}

		public IQueryable<tbl_POS_TransTaxes> GetPosTransactionTaxes()
		{
			return DBModel.QueryNoTrack<tbl_POS_TransTaxes>().Include(t => t.tbl_POS_TaxesList).AsNoTracking();
		}

		public IQueryable<tbl_IOPC_TrafficCount> GetIOPCTraffics()
		{
			return DBModel.QueryNoTrack<tbl_IOPC_TrafficCount>().Include(t => t.tbl_IOPC_TrafficCountRegion).AsNoTracking();
		}

		public IQueryable<tbl_IOPC_DriveThrough> GetIOPCDriveThroughs()
		{
			return DBModel.QueryNoTrack<tbl_IOPC_DriveThrough>();
		}

		public void SaveTransactionNotes(tbl_Exception_Notes note)
		{
			DBModel.Insert(note);
		}

		public void UpdateTransactionNotes(tbl_Exception_Notes note)
		{
			DBModel.Update(note);
		}

		public void InsertExcepTransaction(tbl_Exception_Transact tran)
		{
			DBModel.Insert(tran);
		}

		public void InsertExcepFlag(tbl_Exception_FlaggedTrans tranFlag)
		{
			DBModel.Insert(tranFlag);
		}

		public void UpdateExcepFlag(tbl_Exception_FlaggedTrans tranFlag)
		{
			DBModel.Update(tranFlag);
		}

		public void DeleteExcepFlag(tbl_Exception_FlaggedTrans tranFlag)
		{
			DBModel.Delete(tranFlag);
		}

		public IQueryable<tbl_Exception_Type> GetExcepFlagTypes()
		{
			return DBModel.Query<tbl_Exception_Type>();
		}

		public void InsertExcepFlagType(tbl_Exception_Type type)
		{
			DBModel.Insert(type);
		}

		public void UpdateExcepFlagType(tbl_Exception_Type type)
		{
			DBModel.Update(type);
		}

		public void DeleteExcepFlagType(tbl_Exception_Type type)
		{
			DBModel.Delete(type);
		}

		public void DeleteExcepTranByTranId(long tranId)
		{
			DBModel.DeleteWhere<tbl_Exception_Transact>(t => t.TransID == tranId);
		}

		public void DeleteExcepNoteByTranId(long tranId)
		{
			DBModel.DeleteWhere<tbl_Exception_Notes>(t =>t.TransID == tranId);
		}

		public void DeleteExcepFlagByTranId(long tranId)
		{
			DBModel.DeleteWhere<tbl_Exception_FlaggedTrans>(t => t.TransID == tranId);
		}

		public void DeleteExcepTranDetailByTranId(long tranId)
		{
			DBModel.DeleteWhere<tbl_Exception_Retail>(t => t.TransID == tranId);
		}

		public void DeleteExcepTranTaxByTranId(long tranId)
		{
			DBModel.DeleteWhere<tbl_Exception_TransTaxes>(t => t.TransID == tranId);
		}

		public void DeleteExcepTranPaymentByTranId(long tranId)
		{
			DBModel.DeleteWhere<tbl_Exception_TransPayment>(t => t.TransID == tranId);
		}

		public void DeleteExcepTranSubDetailByTranId(long tranDetailId)
		{
			DBModel.DeleteWhere<tbl_Exception_SubRetail>(t => t.RetailID == tranDetailId);
		}

		public int Save()
		{
			return DBModel.Save();
		}

		public IQueryable<Proc_Exception_WeekAtAGlane_HeaderCount_Result> GetWeekAtGlanceSummary(string pacids, DateTime startdate, DateTime enddate)
		{
			List<SqlParameter> pram = new List<SqlParameter>();
		
			pram.Add(new SqlParameter("DateFrom", SqlDbType.DateTime) { Value = startdate, Direction = ParameterDirection.Input });
			pram.Add(new SqlParameter("DateTo", SqlDbType.DateTime) { Value = enddate, Direction = ParameterDirection.Input });
			pram.Add(new SqlParameter("PACIDs", SqlDbType.NVarChar) { Value = pacids, Direction = ParameterDirection.Input });

			string proc = string.Format(SQLProceduces.Proc_Exception_WeekAtAGlane_HeaderCount, pram.Select(p => p.ParameterName).ToArray());
			IQueryable<Proc_Exception_WeekAtAGlane_HeaderCount_Result> result = DBModel.ExecWithStoreProcedure<Proc_Exception_WeekAtAGlane_HeaderCount_Result>(proc, pram).AsQueryable();
			return result;
		}

		public IQueryable<Proc_Exception_TransWOC_Result> GetTransWOCust(string pacids, DateTime startdate, DateTime enddate)
		{
			List<SqlParameter> pram = new List<SqlParameter>();
		
			pram.Add(new SqlParameter("DateFrom", SqlDbType.DateTime) { Value = startdate, Direction = ParameterDirection.Input });
			pram.Add(new SqlParameter("DateTo", SqlDbType.DateTime) { Value = enddate, Direction = ParameterDirection.Input });
			pram.Add(new SqlParameter("PACIDs", SqlDbType.NVarChar) { Value = pacids, Direction = ParameterDirection.Input });

			string proc = string.Format(SQLProceduces.Proc_Exception_TransWOC, pram.Select(p => p.ParameterName).ToArray());
			IQueryable<Proc_Exception_TransWOC_Result> result = DBModel.ExecWithStoreProcedure<Proc_Exception_TransWOC_Result>(proc, pram).AsQueryable();
			return result;
		}

		public IQueryable<Proc_Exception_CustsWOT_Result> GetCustsWOTran(string pacids, DateTime startdate, DateTime enddate)
		{
			List<SqlParameter> pram = new List<SqlParameter>();

			pram.Add(new SqlParameter("DateFrom", SqlDbType.DateTime) { Value = startdate, Direction = ParameterDirection.Input });
			pram.Add(new SqlParameter("DateTo", SqlDbType.DateTime) { Value = enddate, Direction = ParameterDirection.Input });
			pram.Add(new SqlParameter("PACIDs", SqlDbType.NVarChar) { Value = pacids, Direction = ParameterDirection.Input });

			string proc = string.Format(SQLProceduces.Proc_Exception_CustsWOT, pram.Select(p => p.ParameterName).ToArray());
			IQueryable<Proc_Exception_CustsWOT_Result> result = DBModel.ExecWithStoreProcedure<Proc_Exception_CustsWOT_Result>(proc, pram).AsQueryable();
			return result;
		}

		public IQueryable<Proc_Exception_CarsWOT_Result> GetCarsWOTran(string pacids, DateTime startdate, DateTime enddate)
		{
			List<SqlParameter> pram = new List<SqlParameter>();

			pram.Add(new SqlParameter("DateFrom", SqlDbType.DateTime) { Value = startdate, Direction = ParameterDirection.Input });
			pram.Add(new SqlParameter("DateTo", SqlDbType.DateTime) { Value = enddate, Direction = ParameterDirection.Input });
			pram.Add(new SqlParameter("PACIDs", SqlDbType.NVarChar) { Value = pacids, Direction = ParameterDirection.Input });

			string proc = string.Format(SQLProceduces.Proc_Exception_CarsWOT, pram.Select(p => p.ParameterName).ToArray());
			IQueryable<Proc_Exception_CarsWOT_Result> result = DBModel.ExecWithStoreProcedure<Proc_Exception_CarsWOT_Result>(proc, pram).AsQueryable();
			return result;
		}

		public IQueryable<tbl_IOPC_TrafficCountRegion> GetRegionName(List<int> regIndexs)
		{
			string[] includes = new string[]
			{
				typeof (tbl_IOPC_TrafficCountRegionName).Name
			};
			return DBModel.Query<tbl_IOPC_TrafficCountRegion>(x => regIndexs.Contains(x.RegionIndex), includes);
		}
	}
}

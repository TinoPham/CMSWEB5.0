using System.Linq;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;

namespace CMSWebApi.DataServices
{
	public class AdhocDataService : ServiceBase, IAdhocDataService
	{
		public AdhocDataService(PACDMModel.Model.IResposity model) : base(model) { }

		public AdhocDataService(ServiceBase ServiceBase) : base(ServiceBase.DBModel) { }

		public tbl_Exception_Reports AddAdhocReport(tbl_Exception_Reports report)
		{
			return DBModel.Insert<tbl_Exception_Reports>(report);
		}

		public void UpdateAdhocReport(tbl_Exception_Reports report)
		{
			DBModel.Update<tbl_Exception_Reports>(report);
		}

		public IQueryable<tbl_Exception_Reports> GetAdhocReports()
		{
			return DBModel.Query<tbl_Exception_Reports>();
		}

		public void DeleteAdhocReport(tbl_Exception_Reports report)
		{
			DBModel.Delete<tbl_Exception_Reports>(report);
		}

		public tbl_Exception_ReportFolders AddAdhocReportFolder(tbl_Exception_ReportFolders report)
		{
			return DBModel.Insert<tbl_Exception_ReportFolders>(report);
		}

		public void UpdateAdhocReportFolder(tbl_Exception_ReportFolders report)
		{
			DBModel.Update<tbl_Exception_ReportFolders>(report);
		}

		public void DeleteAdhocReportFolder(tbl_Exception_ReportFolders report)
		{
			DBModel.Delete<tbl_Exception_ReportFolders>(report);
		}

		public IQueryable<tbl_Exception_ReportFolders> GetAdhocReportFolders()
		{
			return DBModel.Query<tbl_Exception_ReportFolders>();
		}

		public tbl_Exception_ReportColumns AddAdhocReportColumns(tbl_Exception_ReportColumns report)
		{
			return DBModel.Insert<tbl_Exception_ReportColumns>(report);
		}

		public void UpdateAdhocReportColumns(tbl_Exception_ReportColumns report)
		{
			DBModel.Update<tbl_Exception_ReportColumns>(report);
		}

		public void DeleteAdhocReportColumns(tbl_Exception_ReportColumns report)
		{
			DBModel.Delete<tbl_Exception_ReportColumns>(report);
		}

		public IQueryable<tbl_Exception_ReportColumns> GetAdhocReportColumns()
		{
			return DBModel.Query<tbl_Exception_ReportColumns>();
		}

		public tbl_Exception_ColumnList AddAdhocReportColumnList(tbl_Exception_ColumnList report)
		{
			return DBModel.Insert<tbl_Exception_ColumnList>(report);
		}

		public void UpdateAdhocReportColumnList(tbl_Exception_ColumnList report)
		{
			DBModel.Update<tbl_Exception_ColumnList>(report);
		}

		public void DeleteAdhocReportColumnList(tbl_Exception_ColumnList report)
		{
			DBModel.Delete<tbl_Exception_ColumnList>(report);
		}

		public IQueryable<tbl_Exception_ColumnList> GetAdhocReportColumnList()
		{
			return DBModel.Query<tbl_Exception_ColumnList>();
		}

		public tbl_Exception_Criteria AddAdhocCriteria(tbl_Exception_Criteria report)
		{
			return DBModel.Insert<tbl_Exception_Criteria>(report);
		}

		public void UpdateAdhocCriteria(tbl_Exception_Criteria report)
		{
			DBModel.Update<tbl_Exception_Criteria>(report);
		}

		public void DeleteAdhocCriteria(tbl_Exception_Criteria report)
		{
			DBModel.Delete<tbl_Exception_Criteria>(report);
		}

		public IQueryable<tbl_Exception_Criteria> GetAdhocCriterias()
		{
			return DBModel.Query<tbl_Exception_Criteria>();
		}

		public tbl_Exception_ReportCriteria AddAdhocReportCriteria(tbl_Exception_ReportCriteria report)
		{
			return DBModel.Insert<tbl_Exception_ReportCriteria>(report);
		}

		public void UpdateAdhocReportCriteria(tbl_Exception_ReportCriteria report)
		{
			DBModel.Update<tbl_Exception_ReportCriteria>(report);
		}

		public void DeleteAdhocReportCriteria(tbl_Exception_ReportCriteria report)
		{
			DBModel.Delete<tbl_Exception_ReportCriteria>(report);
		}

		public IQueryable<tbl_Exception_ReportCriteria> GetAdhocReportCriterias()
		{
			return DBModel.Query<tbl_Exception_ReportCriteria>();
		}

		public tbl_Exception_ReportAssignment AddAdhocReportAssignment(tbl_Exception_ReportAssignment report)
		{
			return DBModel.Insert<tbl_Exception_ReportAssignment>(report);
		}

		public void UpdateAdhocReportAssignment(tbl_Exception_ReportAssignment report)
		{
			DBModel.Update<tbl_Exception_ReportAssignment>(report);
		}

		public void DeleteAdhocReportAssignment(tbl_Exception_ReportAssignment report)
		{
			DBModel.Delete<tbl_Exception_ReportAssignment>(report);
		}

		public IQueryable<tbl_Exception_ReportAssignment> GetAdhocReportAssignments()
		{
			return DBModel.Query<tbl_Exception_ReportAssignment>();
		}

		public tbl_Exception_SharingPermission AddAdhocReportPermission(tbl_Exception_SharingPermission report)
		{
			return DBModel.Insert<tbl_Exception_SharingPermission>(report);
		}

		public void UpdateAdhocReportPermission(tbl_Exception_SharingPermission report)
		{
			DBModel.Update<tbl_Exception_SharingPermission>(report);
		}

		public void DeleteAdhocReportPermission(tbl_Exception_SharingPermission report)
		{
			DBModel.Delete<tbl_Exception_SharingPermission>(report);
		}

		public IQueryable<tbl_Exception_SharingPermission> GetAdhocReportPermissions()
		{
			return DBModel.Query<tbl_Exception_SharingPermission>();
		}

		public IQueryable<tbl_POS_CardIDList> GetCardList()
		{
			return DBModel.Query<tbl_POS_CardIDList>();
		}

		public IQueryable<tbl_POS_CameraNBList> GetCamList()
		{
			return DBModel.Query<tbl_POS_CameraNBList>();
		}

		public IQueryable<tbl_POS_ShiftList> GetShiftList()
		{
			return DBModel.Query<tbl_POS_ShiftList>();
		}

		public IQueryable<tbl_POS_StoreList> GetStoreList()
		{
			return DBModel.Query<tbl_POS_StoreList>();
		}

		public IQueryable<tbl_POS_CheckIDList> GetCheckList()
		{
			return DBModel.Query<tbl_POS_CheckIDList>();
		}

		public IQueryable<tbl_POS_TerminalList> GetTerminalList()
		{
			return DBModel.Query<tbl_POS_TerminalList>();
		}

		public IQueryable<tbl_POS_DescriptionList> GetDescList()
		{
			return DBModel.Query<tbl_POS_DescriptionList>();
		}

		public IQueryable<tbl_POS_ItemCodeList> GetItemList()
		{
			return DBModel.Query<tbl_POS_ItemCodeList>();
		}

		public IQueryable<tbl_POS_PaymentList> GetPaymentList()
		{
			return DBModel.Query<tbl_POS_PaymentList>();
		}

		public IQueryable<tbl_POS_RegisterList> GetRegisterList()
		{
			return DBModel.Query<tbl_POS_RegisterList>();
		}

		public IQueryable<tbl_POS_OperatorList> GetOperatorList()
		{
			return DBModel.Query<tbl_POS_OperatorList>();
		}

		public IQueryable<tbl_POS_TaxesList> GetTaxtLists()
		{
			return DBModel.Query<tbl_POS_TaxesList>();
		}

		public int Save()
		{
			return DBModel.Save();
		}
	}
}

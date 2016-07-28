using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PACDMModel.Model;

namespace CMSWebApi.ServiceInterfaces
{
	public interface IAdhocDataService
	{
		tbl_Exception_Reports AddAdhocReport(tbl_Exception_Reports report);
		void UpdateAdhocReport(tbl_Exception_Reports report);
		IQueryable<tbl_Exception_Reports> GetAdhocReports();
		void DeleteAdhocReport(tbl_Exception_Reports report);
		tbl_Exception_ReportFolders AddAdhocReportFolder(tbl_Exception_ReportFolders report);
		void UpdateAdhocReportFolder(tbl_Exception_ReportFolders report);
		void DeleteAdhocReportFolder(tbl_Exception_ReportFolders report);
		IQueryable<tbl_Exception_ReportFolders> GetAdhocReportFolders();
		tbl_Exception_ReportColumns AddAdhocReportColumns(tbl_Exception_ReportColumns report);
		void UpdateAdhocReportColumns(tbl_Exception_ReportColumns report);
		void DeleteAdhocReportColumns(tbl_Exception_ReportColumns report);
		IQueryable<tbl_Exception_ReportColumns> GetAdhocReportColumns();
		tbl_Exception_ColumnList AddAdhocReportColumnList(tbl_Exception_ColumnList report);
		void UpdateAdhocReportColumnList(tbl_Exception_ColumnList report);
		void DeleteAdhocReportColumnList(tbl_Exception_ColumnList report);
		IQueryable<tbl_Exception_ColumnList> GetAdhocReportColumnList();
		tbl_Exception_Criteria AddAdhocCriteria(tbl_Exception_Criteria report);
		void UpdateAdhocCriteria(tbl_Exception_Criteria report);
		void DeleteAdhocCriteria(tbl_Exception_Criteria report);
		IQueryable<tbl_Exception_Criteria> GetAdhocCriterias();
		tbl_Exception_ReportCriteria AddAdhocReportCriteria(tbl_Exception_ReportCriteria report);
		void UpdateAdhocReportCriteria(tbl_Exception_ReportCriteria report);
		void DeleteAdhocReportCriteria(tbl_Exception_ReportCriteria report);
		IQueryable<tbl_Exception_ReportCriteria> GetAdhocReportCriterias();
		tbl_Exception_ReportAssignment AddAdhocReportAssignment(tbl_Exception_ReportAssignment report);
		void UpdateAdhocReportAssignment(tbl_Exception_ReportAssignment report);
		void DeleteAdhocReportAssignment(tbl_Exception_ReportAssignment report);
		IQueryable<tbl_Exception_ReportAssignment> GetAdhocReportAssignments();
		tbl_Exception_SharingPermission AddAdhocReportPermission(tbl_Exception_SharingPermission report);
		void UpdateAdhocReportPermission(tbl_Exception_SharingPermission report);
		void DeleteAdhocReportPermission(tbl_Exception_SharingPermission report);
		IQueryable<tbl_Exception_SharingPermission> GetAdhocReportPermissions();
		IQueryable<tbl_POS_CardIDList> GetCardList();
		IQueryable<tbl_POS_CameraNBList> GetCamList();
		IQueryable<tbl_POS_ShiftList> GetShiftList();
		IQueryable<tbl_POS_StoreList> GetStoreList();
		IQueryable<tbl_POS_CheckIDList> GetCheckList();
		IQueryable<tbl_POS_TerminalList> GetTerminalList();
		IQueryable<tbl_POS_DescriptionList> GetDescList();
		IQueryable<tbl_POS_ItemCodeList> GetItemList();
		IQueryable<tbl_POS_PaymentList> GetPaymentList();
		IQueryable<tbl_POS_RegisterList> GetRegisterList();
		IQueryable<tbl_POS_OperatorList> GetOperatorList();
		IQueryable<tbl_POS_TaxesList> GetTaxtLists();
		int Save();
	}
}

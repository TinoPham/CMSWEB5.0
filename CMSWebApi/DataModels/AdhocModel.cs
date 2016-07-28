using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.DataModels
{
	public class AdhocModel
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int? FolderId { get; set; }
		public bool IsFolder { get; set; }
        public bool IsAssignee { get; set; }
	}

	public class AdhocReportModel
	{
		public int ReportID { get; set; }
		public string ReportName { get; set; }
		public int? UserID { get; set; }
		public int? FolderId { get; set; }
		public string ReportDesc { get; set; }
		public bool? PromoteToDashboard { get; set; }
        public bool IsAssignee { get; set; }
		public List<ReportAssign> Assign { get; set; }
		public List<AdhocGroupFieldModel> GroupFields { get; set; }
		public List<AdhocColModel> ColumnSelect { get; set; }
		public List<ReportCriteriaColumn> ColumnFilter { get; set; }
	}

	public class ReportAssign
	{
		public int AdminID { get; set; }
		public int UserID { get; set; }
		public int ReportID { get; set; }
		public DateTime? AssignedDate { get; set; }
		public byte? Sharing { get; set; }
	}

	public class ReportCriteriaColumn
	{
		public int ReportID { get; set; }
		public int CriteriaID { get; set; }
		public bool? AND_OP { get; set; }
		public List<ReportCriteriaValue> ColumnValue { get; set; }
	}

	public class ReportCriteriaValue
	{
		public int CriteriaID { get; set; }
		public int? ColID { get; set; }
		public string Operator { get; set; }
		public string Column { get; set; }
		public string CriteriaValue_1 { get; set; }
		public string CriteriaValue_2 { get; set; }
	}

	public class AdhocReportFolderModel
	{
		public int FolderID { get; set; }
		public int? UserID { get; set; }
		public string FolderName { get; set; }
		public IEnumerable<AdhocReportModel>  Reports { get; set; }
	}

	public class AdhocGroupFieldModel
	{
		public int ColID { get; set; }
		public int? GroupType { get; set; }
		public string GroupName {get; set; }
	}


	public class AdhocColModel
	{
		public int ReportID { get; set; }
		public int ColID { get; set; }
		public int? ColWidth { get; set; }
		public int? ColOrder { get; set; }
		public string DisplayName { get; set; }
		public int? SortOrder { get; set; }
		public bool? Ascending { get; set; }
		public bool? GroupBy { get; set; }
	}

	public class AdhocColDefindModel
	{
		public int ColID { get; set; }
		public string ColName { get; set; }
		public string ColDesc { get; set; }
		public string TableName { get; set; }
		public string DataField { get; set; }
		public string DataType { get; set; }
		public string DisplayField { get; set; }
		public bool? Sortable { get; set; }
		public bool? Groupable { get; set; }
		public bool? ShowList { get; set; }
	}

	public class ListModel
	{
		public int ID { get; set; }
		public string Name { get; set; }
	}
}

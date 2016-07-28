using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.DataModels
{
	public class IncidentModel
	{
		public int FieldsGUIID { get; set; }
		public string FieldsGUIName { get; set; }
		public Nullable<int> ParentFieldID { get; set; }
		public Nullable<bool> MandatoryField { get; set; }
		public Nullable<short> OrderField { get; set; }
		public Nullable<short> ObjectTypeID { get; set; }
		public Nullable<bool> isFixed { get; set; }
		public Nullable<bool> Status { get; set; }
	}

	public class IncidentMandatoryModel : IncidentModel
	{
		public IEnumerable<IncidentModel> ItemChilds { get; set; }
	}

	public class IncidentManagementModel
	{
		public IEnumerable<IncidentModel> FieldSelection { get; set; }
		public IEnumerable<IncidentMandatoryModel> MandatoryFields { get; set; }
	}

	public class CaseTypeModel 
	{
		public int CaseTypeID { get; set; }
		public string CaseTypeName { get; set; }
	}

	public class IncidentFieldModel
	{
		public int FieldsGUIID { get; set; }
		public int CaseTypeID { get; set; }
		public Nullable<bool> Status { get; set; }
	}

	//public class IncidentFieldData : TransactionalInformation
	//{
	//	public IncidentFieldModel[] IncidentFields { get; set; }
	//}
}

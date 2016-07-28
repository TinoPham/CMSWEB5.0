using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;
using PACDMModel;
using CMSWebApi.DataModels;
using System.Linq.Expressions;


namespace CMSWebApi.DataServices
{
	public partial class IncidentService : ServiceBase, IIncidentService
	{

		public IncidentService(PACDMModel.Model.IResposity model) : base(model) { }

		public IncidentService(ServiceBase ServiceBase) : base(ServiceBase.DBModel) { }

		public IQueryable<Tout> SelectIncidentField<Tout>(int? casetypeID, Expression<Func<tCMSWeb_CM_IncidentField, Tout>> selector, string [] includes) where Tout : class
		{
			
			IQueryable<Tout> result;
			if( casetypeID.HasValue)
			 result = Query<tCMSWeb_CM_IncidentField, Tout>(item => item.CaseTypeID == casetypeID.Value ,selector, includes);
			 else
				result = Query<tCMSWeb_CM_IncidentField, Tout>( null , selector, includes);
			return result;
		}

		public IQueryable<Tout> SelectIncidentField<Tout>(int field, int caseType, Expression<Func<tCMSWeb_CM_IncidentField, Tout>> selector, string [] includes) where Tout : class
		{
			IQueryable<Tout> result = Query<tCMSWeb_CM_IncidentField, Tout>(item =>item.CaseTypeID == caseType && item.FieldsGUIID == field ,selector, includes); 
			return result;
		}

		public IQueryable<Tout> SelectCaseType<Tout>(Expression<Func<tCMSWeb_CM_CaseType, Tout>> selector, string [] includes) where Tout : class
		{
			IQueryable<Tout> result = Query<tCMSWeb_CM_CaseType, Tout>( null, selector, includes);
			return result;
		}

		public IQueryable<Tout> SelectFieldsGUI<Tout>(bool MandatoryField, int? parentID, Expression<Func<tCMSWeb_CM_FieldsGUI, Tout>> selector, string [] includes) where Tout : class
		{
			IQueryable<Tout> result;
			if( parentID.HasValue)
				result = Query<tCMSWeb_CM_FieldsGUI, Tout>(item => item.MandatoryField == MandatoryField && item.ParentFieldID == parentID.Value, selector, includes);
			else
				result = Query<tCMSWeb_CM_FieldsGUI, Tout>(item => item.MandatoryField == MandatoryField, selector, includes);
			return result;
		}

		//public IQueryable<tCMSWeb_CM_FieldsGUI> SelectFieldsGUI()
		//{
		//	return DBModel.Query<tCMSWeb_CM_FieldsGUI>(i=> i.MandatoryField == false);
		//}

		//public IQueryable<tCMSWeb_CM_FieldsGUI> SelectFieldGUIParent()
		//{
		//	return DBModel.Query<tCMSWeb_CM_FieldsGUI>(i => i.MandatoryField == true && i.ParentFieldID==1);
		//}

		//public IQueryable<tCMSWeb_CM_FieldsGUI> SelectFieldGUIChild( int? parent)
		//{
		//	return DBModel.Query<tCMSWeb_CM_FieldsGUI>(i => i.MandatoryField == true && i.ParentFieldID == parent);
		//}

		

		public tCMSWeb_CM_IncidentField UpdateIncidentField(tCMSWeb_CM_IncidentField model)
		{
			DBModel.Update<tCMSWeb_CM_IncidentField>(model);
			return DBModel.Save() >= 0 ? model : null;
		}


	}
}

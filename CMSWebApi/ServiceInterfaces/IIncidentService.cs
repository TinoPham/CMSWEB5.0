using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using PACDMModel.Model;

namespace CMSWebApi.ServiceInterfaces
{
	public interface IIncidentService
	{
		//IQueryable<tCMSWeb_CM_IncidentField> SelectIncidentField(int casetypeID);
		//IQueryable<tCMSWeb_CM_IncidentField> SelectIncidentField();
		//IQueryable<tCMSWeb_CM_IncidentField> SelectIncidentField(int field, int caseType);
		IQueryable<Tout> SelectIncidentField<Tout>(int? casetypeID, Expression<Func<tCMSWeb_CM_IncidentField, Tout>> selector, string [] includes) where Tout : class;
		IQueryable<Tout> SelectIncidentField<Tout>(int field, int caseType, Expression<Func<tCMSWeb_CM_IncidentField, Tout>> selector, string [] includes) where Tout : class;

		//IQueryable<tCMSWeb_CM_CaseType> SelectCaseType();
		IQueryable<Tout> SelectCaseType<Tout>(Expression<Func<tCMSWeb_CM_CaseType, Tout>> selector, string [] includes) where Tout : class;
		IQueryable<Tout> SelectFieldsGUI<Tout>(bool MandatoryField, int? parentID, Expression<Func<tCMSWeb_CM_FieldsGUI, Tout>> selector, string [] includes) where Tout : class;

		//IQueryable<tCMSWeb_CM_FieldsGUI> SelectFieldsGUI();
		//IQueryable<tCMSWeb_CM_FieldsGUI> SelectFieldGUIParent();
		//IQueryable<tCMSWeb_CM_FieldsGUI> SelectFieldGUIChild(int? parent);
		
		tCMSWeb_CM_IncidentField UpdateIncidentField(tCMSWeb_CM_IncidentField model);
	}
}


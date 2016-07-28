using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.ServiceInterfaces;
using CMSWebApi.DataModels;
using System.Globalization;
using PACDMModel.Model;
using System.IO;
using CMSWebApi.APIFilters;
using CMSWebApi.Utils;

namespace CMSWebApi.BusinessServices.Incident
{
	public class IncidentBusinessService : BusinessBase<IIncidentService>
	{
		public IQueryable<CaseTypeModel> GetCaseType()
		{
			return DataService.SelectCaseType<CaseTypeModel>( item=> new CaseTypeModel{
										  CaseTypeID = item.CaseTypeID,
										  CaseTypeName = item.CaseTypeName}, null );
			//IQueryable<CaseTypeModel> models = (from item in DataService.SelectCaseType()
			//						  select new CaseTypeModel
			//						  {
			//							  CaseTypeID = item.CaseTypeID,
			//							  CaseTypeName = item.CaseTypeName}).ToArray();
			//return models;			
		}

		public IncidentManagementModel GetIncidentManagent(int caseType)
		{

			IEnumerable<IncidentMandatoryModel> mandatory = GetMandatoryField(caseType);
			IQueryable<IncidentModel> field = GetFieldSelection(caseType);
			IncidentManagementModel model = new IncidentManagementModel();
			model.FieldSelection = field;
			model.MandatoryFields = new List<IncidentMandatoryModel>(mandatory);
			return model;
		}

		private IEnumerable<IncidentMandatoryModel> GetMandatoryField(int caseType)
		{
			string childp = ServiceBase.ChildProperty(typeof(tCMSWeb_CM_FieldsGUI), typeof(tCMSWeb_CM_IncidentField));
			IQueryable<tCMSWeb_CM_IncidentField> icidentField = DataService.SelectIncidentField<tCMSWeb_CM_IncidentField>(caseType, item => item, null);
			IQueryable<tCMSWeb_CM_FieldsGUI> fieldParent = DataService.SelectFieldsGUI<tCMSWeb_CM_FieldsGUI>(true, null, item => item, new string [] { childp }).Where(item => item.tCMSWeb_CM_IncidentField.FirstOrDefault(ctype => ctype.CaseTypeID == caseType) != null);

			var parents = fieldParent.Where(item => item.ParentFieldID == 1).Join(icidentField, fgui => fgui.FieldsGUIID, infield => infield.FieldsGUIID, (fgui, infield) => new { fgui, infield });
			var childs = fieldParent.Where(item => item.ParentFieldID > 1).Join(icidentField, fgui => fgui.FieldsGUIID, infield => infield.FieldsGUIID, (fgui, infield) => new { fgui, infield });
			
			var result = parents.GroupJoin(childs, p => p.fgui.FieldsGUIID, c => c.fgui.ParentFieldID, (pit, cit) => new IncidentMandatoryModel
			{
				FieldsGUIID = pit.fgui.FieldsGUIID,
				FieldsGUIName = pit.fgui.FieldsGUIName,
				ParentFieldID = pit.fgui.ParentFieldID,
				//isFixed = pit.fgui.isFixed,
				MandatoryField = pit.fgui.MandatoryField,
				ObjectTypeID = pit.fgui.ObjectTypeID,
				OrderField = pit.fgui.OrderField,
				Status = pit.infield.Status,
				ItemChilds = cit.Select( it => new IncidentModel
														   {
															   FieldsGUIID = it.fgui.FieldsGUIID,
															   FieldsGUIName = it.fgui.FieldsGUIName,
															   ParentFieldID = it.fgui.ParentFieldID,
															   //isFixed = it.fgui.isFixed,
															   MandatoryField = it.fgui.MandatoryField,
															   ObjectTypeID = it.fgui.ObjectTypeID,
															   OrderField = it.fgui.OrderField,
															   Status = it.infield.Status
														   })
			 });
			 return result;
			//tCMSWeb_CM_IncidentField[] icidentField = DataService.SelectIncidentField<tCMSWeb_CM_IncidentField>(caseType, item => item,null).ToArray();
			//tCMSWeb_CM_FieldsGUI[] fieldParent = DataService.SelectFieldsGUI<tCMSWeb_CM_FieldsGUI>(true, 1, item=> item, null).ToArray();

			//IncidentMandatoryModel[] models = (from item in fieldParent//.ToArray()
			//								   join incident in icidentField on item.FieldsGUIID equals incident.FieldsGUIID
			//								   select new IncidentMandatoryModel
			//						  {
			//							  FieldsGUIID = item.FieldsGUIID,
			//							  FieldsGUIName = item.FieldsGUIName,
			//							  ParentFieldID = item.ParentFieldID,
			//							  isFixed= item.isFixed,
			//							  MandatoryField= item.MandatoryField,
			//							  ObjectTypeID=item.ObjectTypeID,
			//							  OrderField= item.OrderField,
			//							  Status= incident.Status,
			//							  ItemChilds = (from child in DataService.SelectFieldsGUI<tCMSWeb_CM_FieldsGUI>(true, item.FieldsGUIID, citem => citem, null).ToArray()//.SelectFieldGUIChild(item.FieldsGUIID).ToArray()
			//											   join field in icidentField on child.FieldsGUIID equals field.FieldsGUIID
			//											   select new IncidentModel
			//											   {
			//												   FieldsGUIID = child.FieldsGUIID,
			//												   FieldsGUIName = child.FieldsGUIName,
			//												   ParentFieldID = child.ParentFieldID,
			//												   isFixed = child.isFixed,
			//												   MandatoryField = child.MandatoryField,
			//												   ObjectTypeID = child.ObjectTypeID,
			//												   OrderField = child.OrderField,
			//												   Status = field.Status
			//											   }) as IQueryable<IncidentModel>
			//						  }).ToArray();
			//return result;
		}

		private IQueryable<IncidentModel> GetFieldSelection(int caseType)
		{
			string childp = ServiceBase.ChildProperty( typeof(tCMSWeb_CM_FieldsGUI), typeof(tCMSWeb_CM_IncidentField));
			IQueryable<tCMSWeb_CM_IncidentField> icidentField = DataService.SelectIncidentField < tCMSWeb_CM_IncidentField >( caseType, item=> item, null); //.SelectIncidentField(caseType);
			IQueryable<tCMSWeb_CM_FieldsGUI> fields = DataService.SelectFieldsGUI<tCMSWeb_CM_FieldsGUI>( false,null, item=> item, new string[]{childp} ); //.SelectFieldsGUI();

			IQueryable<tCMSWeb_CM_FieldsGUI> sel_fields = fields.Where(item => item.tCMSWeb_CM_IncidentField.FirstOrDefault(icse => icse.CaseTypeID == caseType) != null);

			IQueryable<IncidentModel> models = sel_fields.Select( item => new IncidentMandatoryModel
									   {
										   FieldsGUIID = item.FieldsGUIID,
										   FieldsGUIName = item.FieldsGUIName,
										   ParentFieldID = item.ParentFieldID,
										   //isFixed = item.isFixed,
										   MandatoryField = item.MandatoryField,
										   ObjectTypeID = item.ObjectTypeID,
										   OrderField = item.OrderField,
										   Status = icidentField.FirstOrDefault(  st => st.FieldsGUIID == item.FieldsGUIID).Status //item.tCMSWeb_CM_IncidentField.FirstOrDefault(
									   });

			//IQueryable<IncidentModel> models = (from item in fields
			//									join incident in icidentField on item.FieldsGUIID equals incident.FieldsGUIID
			//									select new IncidentMandatoryModel
			//						   {
			//							   FieldsGUIID = item.FieldsGUIID,
			//							   FieldsGUIName = item.FieldsGUIName,
			//							   ParentFieldID = item.ParentFieldID,
			//							   isFixed = item.isFixed,
			//							   MandatoryField = item.MandatoryField,
			//							   ObjectTypeID = item.ObjectTypeID,
			//							   OrderField = item.OrderField,
			//							   Status = incident.Status
			//						   });

			return models;
		}

		public TransactionalModel<List<IncidentFieldModel>> UpdateIncidentField(IncidentFieldModel [] models)
		{
			TransactionalModel<List<IncidentFieldModel>> response = new TransactionalModel<List<IncidentFieldModel>>();
			foreach (IncidentFieldModel item in models)
			{
				tCMSWeb_CM_IncidentField tField = DataService.SelectIncidentField<tCMSWeb_CM_IncidentField>(item.CaseTypeID, node => node, null).FirstOrDefault();//.SelectIncidentField(item.FieldsGUIID, item.CaseTypeID).FirstOrDefault();
				tField.Status = item.Status;
				if (DataService.UpdateIncidentField(tField) == null)
				{
					response.ReturnStatus = false;
					response.ReturnMessage.Add(CMSWebError.EDIT_FAIL_MSG.ToString());
					return response;
				}
			}

			response.ReturnStatus = true;
			response.ReturnMessage.Add(CMSWebError.EDIT_SUCCESS_MSG.ToString());
			response.Data = models.ToList();
			return response;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.APIFilters;
using CMSWebApi.DataModels;
using CMSWebApi.DataServices;
using CMSWebApi.ServiceInterfaces;
using CMSWebApi.Utils;
using PACDMModel.Model;

namespace CMSWebApi.BusinessServices.Rebar
{
	public class AdhocService : BusinessBase<IAdhocDataService>
	{
		public AdhocReportModel SettingAhocReport(UserContext context, AdhocReportModel report)
		{

			if (report.ReportID > 0)
			{
				var reportDb = DataService.GetAdhocReports()
					.Include(f => f.tbl_Exception_ReportAssignment)
					.Include(f => f.tbl_Exception_ReportColumns)
					.Include(f => f.tbl_Exception_ReportCriteria)
					.Include(f => f.tbl_Exception_ReportCriteria.Select(g => g.tbl_Exception_Criteria))
					.FirstOrDefault(f => f.ReportID == report.ReportID && f.UserID == context.ID);

				if (reportDb == null)
				{
					throw  new CmsErrorException(CMSWebError.NO_PERMISION);
				}

				reportDb.tbl_Exception_ReportCriteria.ToList().ForEach(c =>
				{
					DataService.DeleteAdhocCriteria(c.tbl_Exception_Criteria);
					DataService.DeleteAdhocReportCriteria(c);
				});
				reportDb.tbl_Exception_ReportAssignment.ToList().ForEach(c => DataService.DeleteAdhocReportAssignment(c));
				reportDb.tbl_Exception_ReportColumns.ToList().ForEach(c => DataService.DeleteAdhocReportColumns(c));

				var userassigns = new List<tbl_Exception_ReportAssignment>();
				report.Assign.ForEach((user) =>
				{
					var assign = new tbl_Exception_ReportAssignment()
					{
						AdminID = context.ID,
						AssignedDate = DateTime.Now,
						Sharing = user.Sharing,
						UserID = user.UserID
					};
					userassigns.Add(assign);
				});


				reportDb.tbl_Exception_ReportAssignment = userassigns;

				var colsets = new List<tbl_Exception_ReportColumns>();
				report.ColumnSelect.ForEach((col) =>
				{
					var colset = new tbl_Exception_ReportColumns()
					{
						Ascending = col.Ascending,
						ColID = col.ColID,
						ColOrder = col.ColOrder,
						ColWidth = col.ColWidth,
						DisplayName = col.DisplayName,
						GroupBy = col.GroupBy,
						SortOrder = col.SortOrder
					};
					colsets.Add(colset);
				});

				reportDb.tbl_Exception_ReportColumns = colsets;


				var criticals = new List<tbl_Exception_ReportCriteria>();
				report.ColumnFilter.ForEach((c) =>
				{
					var criticalcols = new List<tbl_Exception_Criteria>();
					c.ColumnValue.ForEach(l =>
					{
						var ldb = new tbl_Exception_Criteria
						{
							ColID = l.ColID,
							Operator = l.Operator,
							CriteriaValue_1 = l.CriteriaValue_1,
							CriteriaValue_2 = l.CriteriaValue_2
						};
						criticalcols.Add(ldb);
					});


					var cdb = new tbl_Exception_ReportCriteria()
					{
						AND_OP = c.AND_OP,
						tbl_Exception_Criteria = criticalcols.FirstOrDefault()
					};

					criticals.Add(cdb);
				});

				reportDb.tbl_Exception_ReportCriteria = criticals;



				reportDb.UserID = context.ID;
				reportDb.ReportName = report.ReportName;
				reportDb.ReportDesc = report.ReportDesc;
				reportDb.PromoteToDashboard = report.PromoteToDashboard;
				DataService.UpdateAdhocReport(reportDb);
				DataService.Save();
				return report;
			}
			else
			{
				return AddAdhocReport(context, report);
			}
		}


		public AdhocReportModel AddAdhocReport(UserContext context, AdhocReportModel report)
		{


			var userassigns = new List<tbl_Exception_ReportAssignment>();
			report.Assign.ForEach((user) =>
			{
				var assign = new tbl_Exception_ReportAssignment()
				{
					AdminID = context.ID,
					AssignedDate = DateTime.Now,
					Sharing = user.Sharing,
					UserID = user.UserID
				};
				userassigns.Add(assign);
			});


			var reportdb = new tbl_Exception_Reports()
			{
				UserID = context.ID,
				ReportName = report.ReportName,
				ReportDesc = report.ReportDesc,
				FolderID = report.FolderId == null || report.FolderId == 0 ? null : report.FolderId,
				PromoteToDashboard = report.PromoteToDashboard,
				CreatedDate = DateTime.Now
			};

			reportdb.tbl_Exception_ReportAssignment = userassigns;

			var colsets = new List<tbl_Exception_ReportColumns>();
			report.ColumnSelect.ForEach((col) =>
			{
				var colset = new tbl_Exception_ReportColumns()
				{
					Ascending = col.Ascending,
					ColID = col.ColID,
					ColOrder = col.ColOrder,
					ColWidth = col.ColWidth,
					DisplayName = col.DisplayName,
					GroupBy = col.GroupBy,
					SortOrder = col.SortOrder
				};
				colsets.Add(colset);
			});

			reportdb.tbl_Exception_ReportColumns = colsets;


			var criticals = new List<tbl_Exception_ReportCriteria>();
			report.ColumnFilter.ForEach((c) =>
			{
				var criticalcols = new List<tbl_Exception_Criteria>();
				c.ColumnValue.ForEach(l =>
				{
					var ldb = new tbl_Exception_Criteria
					{
						ColID = l.ColID,
						Operator = l.Operator,
						CriteriaValue_1 = l.CriteriaValue_1,
						CriteriaValue_2 = l.CriteriaValue_2
					};
					criticalcols.Add(ldb);
				});


				var cdb = new tbl_Exception_ReportCriteria()
				{
					AND_OP = c.AND_OP,
					tbl_Exception_Criteria = criticalcols.FirstOrDefault()
				};

				criticals.Add(cdb);
			});

			reportdb.tbl_Exception_ReportCriteria = criticals;
			DataService.AddAdhocReport(reportdb);
			DataService.Save();
			report.ReportID = reportdb.ReportID;

			return report;
		}

		public void UpdateAdhocReport(UserContext context, AdhocReportModel report)
		{

			var reportdb = DataService.GetAdhocReports().FirstOrDefault(t => t.ReportID == report.ReportID && t.UserID == context.ID);

			if (reportdb == null) return;

			reportdb.UserID = context.ID;
			reportdb.ReportName = report.ReportName;
			reportdb.ReportDesc = report.ReportDesc;
			reportdb.PromoteToDashboard = report.PromoteToDashboard;
		

			DataService.UpdateAdhocReport(reportdb);

			DataService.Save();
		}

		public void DeleteAdhocReport(UserContext context, int reportId)
		{
			var reportdb = DataService.GetAdhocReports()
				.Include(t=>t.tbl_Exception_ReportAssignment)
				.Include(t => t.tbl_Exception_ReportColumns)
				.Include(t => t.tbl_Exception_ReportCriteria)
				.Include(t => t.tbl_Exception_ReportCriteria.Select(f=>f.tbl_Exception_Criteria))
				.FirstOrDefault(t => t.ReportID == reportId && t.UserID == context.ID);

			if (reportdb == null) return;

			DataService.DeleteAdhocReport(reportdb);

			DataService.Save();
		}

		public IQueryable<AdhocReportModel> GetAdhocReports(UserContext context)
		{
			return DataService.GetAdhocReports().Select(report => new AdhocReportModel()
			{
				UserID = report.UserID,
				ReportName = report.ReportName,
				ReportDesc = report.ReportDesc,
				FolderId = report.FolderID,
				PromoteToDashboard = report.PromoteToDashboard
			});
		}

		public AdhocReportModel GetAdhocReportById(UserContext context, int reportId)
		{
            var getAssign = DataService.GetAdhocReportAssignments().Where(t => t.UserID == context.ID).Select(f => f.ReportID).ToList();
			return DataService.GetAdhocReports().Select(report => new AdhocReportModel()
			{
				UserID = report.UserID,
				ReportName = report.ReportName,
				ReportDesc = report.ReportDesc,
				FolderId = report.FolderID,
				PromoteToDashboard = report.PromoteToDashboard,
                IsAssignee = getAssign.Contains(report.ReportID) ? true : false
			}).FirstOrDefault(t=>t.ReportID == reportId && t.UserID == context.ID);
		}

		public AdhocReportFolderModel SettingAdhocReportFolder(UserContext context, AdhocReportFolderModel report)
		{
			if (report.FolderID > 0)
			{
				return UpdateAdhocReportFolder(context, report);
			}
			else
			{
				return AddAdhocReportFolder(context, report);
			}
		}

		public AdhocReportFolderModel AddAdhocReportFolder(UserContext context, AdhocReportFolderModel report)
		{
			var reportdb = new tbl_Exception_ReportFolders()
			{
				UserID = context.ID,
				 FolderName = report.FolderName,
				CreatedDate = DateTime.Now
			};

			DataService.AddAdhocReportFolder(reportdb);
			DataService.Save();
			report.FolderID = reportdb.FolderID;

			return report;
		}

		public AdhocReportFolderModel UpdateAdhocReportFolder(UserContext context, AdhocReportFolderModel report)
		{
			var reportdb = DataService.GetAdhocReportFolders().FirstOrDefault(t => t.FolderID == report.FolderID && t.UserID == context.ID);

			if (reportdb == null) throw new CmsErrorException(CMSWebError.NO_PERMISION);

			reportdb.UserID = context.ID;
			reportdb.FolderName = report.FolderName;
			reportdb.UserID = report.UserID;


			DataService.UpdateAdhocReportFolder(reportdb);

			DataService.Save();
			return report;
		}

		public void DeleteAdhocReportFolder(UserContext context, int folderId)
		{
			var reportdb = DataService.GetAdhocReportFolders().Include(t=>t.tbl_Exception_Reports).FirstOrDefault(t => t.FolderID == folderId && t.UserID == context.ID);

			if (reportdb == null) throw new CmsErrorException(CMSWebError.NO_PERMISION); ;

			if (reportdb.tbl_Exception_Reports.Count > 0)
			{
				throw new CmsErrorException(CMSWebError.DELETE_FAIL_ITEM_HAVE_CHILDS);
			}

			DataService.DeleteAdhocReportFolder(reportdb);

			DataService.Save();
		}

		public IQueryable<AdhocReportFolderModel> GetAdhocReportFolders(UserContext context)
		{
			return DataService.GetAdhocReportFolders().Select(report => new AdhocReportFolderModel()
			{
				UserID = report.UserID,
				FolderID = report.FolderID,
				FolderName = report.FolderName
			});
		}

		public List<AdhocModel> GetAdhocs(UserContext context, int? id)
		{
			int? folderId = id == 0 ? (int?) null : id;
			var adhoclist = new List<AdhocModel>();
			var getAssign = DataService.GetAdhocReportAssignments().Where(t => t.UserID == context.ID).Select(f => f.ReportID).ToList();
			var reports = DataService.GetAdhocReports().Where(t => (t.UserID == context.ID || getAssign.Contains(t.ReportID))).Select(t=> new AdhocModel()
			{
				Id = t.ReportID,
				Name = t.ReportName,
				FolderId = t.FolderID,
				IsFolder = false,
                IsAssignee = getAssign.Contains(t.ReportID) ? true : false
			}).ToList();

			if (folderId > 0)
			{
				var rps = reports.Where(t => t.FolderId == folderId).ToList();
				adhoclist.AddRange(rps);
			}
			else
			{
				var rps = reports.Where(t => t.FolderId == folderId).ToList();
				adhoclist.AddRange(rps);
			}

			if (folderId == null) {
				var listreport = reports.Where(f => f.FolderId != null).Select(f => f.FolderId).Cast<int>();
                var listFolder = new List<AdhocModel>();
				var folderqeury = DataService.GetAdhocReportFolders().Where(t => listreport.Contains(t.FolderID));
				var folders = folderqeury.Select(t => new AdhocModel()
				{
					Id = t.FolderID,
					Name = t.FolderName,
					FolderId = t.FolderID,
					IsFolder = true,
                    IsAssignee = context.ID == t.UserID ? false : true
				}).ToList();
                listFolder.AddRange(folders);

				folderqeury = DataService.GetAdhocReportFolders().Where(t => t.UserID == context.ID);
				folders = folderqeury.Select(t => new AdhocModel()
				{
					Id = t.FolderID,
					Name = t.FolderName,
					FolderId = t.FolderID,
					IsFolder = true,
                    IsAssignee = false
				}).ToList();
                listFolder.AddRange(folders);

                //var a = listFolder.Select(t=>t.Id).Distinct().ToList();
                listFolder = listFolder.GroupBy(g => new { g.Id, g.IsFolder, g.Name, g.FolderId, g.IsAssignee }).Select(g => new AdhocModel() { Id = g.Key.Id, IsFolder = g.Key.IsFolder, Name = g.Key.Name, FolderId = g.Key.FolderId, IsAssignee = g.Key.IsAssignee }).ToList();
                adhoclist.AddRange(listFolder);
			}

			return adhoclist;
		}

		public AdhocReportModel GetReport(UserContext context, int reportId)
		{

			var columnList = DataService.GetAdhocReportColumnList();
			var t =  DataService.GetAdhocReports()
				.Include(f => f.tbl_Exception_ReportAssignment)
				.Include(f => f.tbl_Exception_ReportColumns)
				.Include(f => f.tbl_Exception_ReportCriteria)
				.Include(f => f.tbl_Exception_ReportCriteria.Select(g => g.tbl_Exception_Criteria))
				.FirstOrDefault(f => f.ReportID == reportId);

            var getAssign = DataService.GetAdhocReportAssignments().Where(w => w.UserID == context.ID).Select(f => f.ReportID).ToList();

			return new AdhocReportModel
			{
				ReportID = t.ReportID,
				UserID = t.UserID,
				ReportName = t.ReportName,
				ReportDesc = t.ReportDesc,
				FolderId = t.FolderID,
				PromoteToDashboard = t.PromoteToDashboard,
                IsAssignee = getAssign.Contains(t.ReportID) ? true : false,
				Assign = t.tbl_Exception_ReportAssignment.Select(f => new ReportAssign
				{
					AdminID = f.AdminID,
					AssignedDate = f.AssignedDate,
					ReportID = f.ReportID,
					Sharing = f.Sharing,
					UserID = f.UserID
				}).ToList(),
				ColumnSelect = t.tbl_Exception_ReportColumns.Select(f => new AdhocColModel()
				{
					Ascending = f.Ascending,
					ColID = f.ColID,
					ColOrder = f.ColOrder,
					ColWidth = f.ColWidth,
					DisplayName = f.DisplayName,
					GroupBy = f.GroupBy,
					ReportID = f.ReportID,
					SortOrder = f.SortOrder
				}).ToList(),
				GroupFields = t.tbl_Exception_ReportColumns.Where(f=>f.GroupBy== true).Select(f => new AdhocGroupFieldModel()
				{
					ColID = f.ColID,
					GroupType = f.ColWidth,
					GroupName = f.DisplayName
				}).ToList(),
				ColumnFilter = t.tbl_Exception_ReportCriteria.Select(f =>
				{
					var tblExceptionColumnList = columnList.FirstOrDefault(g => g.ColID == f.tbl_Exception_Criteria.ColID);
					return new ReportCriteriaColumn()
					{
						AND_OP = f.AND_OP,
						ReportID = f.ReportID,
						CriteriaID = f.CriteriaID,
						ColumnValue = f.tbl_Exception_Criteria == null
							? null
							: new List<ReportCriteriaValue>()
							{

								new ReportCriteriaValue()
								{
									ColID = f.tbl_Exception_Criteria.ColID,
									Column = tblExceptionColumnList != null ? tblExceptionColumnList.ColName : "",
									CriteriaID = f.tbl_Exception_Criteria.CriteriaID,
									Operator = f.tbl_Exception_Criteria.Operator,
									CriteriaValue_1 = f.tbl_Exception_Criteria.CriteriaValue_1,
									CriteriaValue_2 = f.tbl_Exception_Criteria.CriteriaValue_2
								}
							}
					};
				}).ToList()
			};
		}


		public AdhocReportFolderModel GetAdhocReportFoldersById(UserContext context, int reportFolder)
		{

			return DataService.GetAdhocReportFolders().Select(report => new AdhocReportFolderModel()
			{
				UserID = report.UserID,
				FolderID = report.FolderID,
				FolderName = report.FolderName
			}).FirstOrDefault(t => t.FolderID == reportFolder);
		}

		public IQueryable<AdhocColDefindModel> GetAhocReportColumn(UserContext context)
		{
			return DataService.GetAdhocReportColumnList().Where(t=>t.ShowList == true).Select(t => new AdhocColDefindModel()
			{
				ColDesc = t.ColDesc,
				ColID = t.ColID,
				ColName = t.ColName,
				DataField = t.DataField,
				DataType = t.DataType,
				DisplayField = t.DisplayField,
				Groupable = t.Groupable,
				ShowList = t.ShowList,
				Sortable = t.Sortable,
				TableName = t.TableName
			});
		}

		public IQueryable<ListModel> GetCardList()
		{
			return DataService.GetCardList().Select(t => new ListModel()
			{
				ID = t.CardID_ID,
				Name = t.CardID_Name
			});
		}

		public IQueryable<ListModel> GetCamList()
		{
			return DataService.GetCamList().Select(t => new ListModel()
			{
				ID = t.CameraNB_ID,
				Name = t.CameraNB_Name
			});
		}

		public IQueryable<ListModel> GetShiftList()
		{
			return DataService.GetShiftList().Select(t => new ListModel()
			{
				ID = t.Shift_ID,
				Name = t.Shift_Name
			});
		}

		public IQueryable<ListModel> GetStoreList()
		{
			return DataService.GetStoreList().Select(t => new ListModel()
			{
				ID = t.Store_ID,
				Name = t.Store_Name
			});
		}

		public IQueryable<ListModel> GetCheckList()
		{
			return DataService.GetCheckList().Select(t => new ListModel()
			{
				ID = t.CheckID_ID,
				Name = t.CheckID_Name
			});
		}

		public IQueryable<ListModel> GetTerminalList()
		{
			return DataService.GetTerminalList().Select(t => new ListModel()
			{
				ID = t.Terminal_ID,
				Name = t.Terminal_Name
			});
		}

		public IQueryable<ListModel> GetDescList()
		{
			return DataService.GetDescList().Select(t => new ListModel()
			{
				ID = t.Description_ID,
				Name = t.Description_Name
			});
		}

		public IQueryable<ListModel> GetItemList()
		{
			return DataService.GetItemList().Select(t => new ListModel()
			{
				ID = t.ItemCode_ID,
				Name = t.ItemCode_Name
			});
		}

		public IQueryable<ListModel> GetPaymentList()
		{
			return DataService.GetPaymentList().Select(t => new ListModel()
			{
				ID = t.PaymentID,
				Name = t.PaymentName
			});
		}

		public IQueryable<ListModel> GetRegisterList()
		{
			return DataService.GetRegisterList().Select(t => new ListModel()
			{
				ID = t.Register_ID,
				Name = t.Register_Name
			});
		}

		public IQueryable<ListModel> GetOperatorList()
		{
			return DataService.GetOperatorList().Select(t => new ListModel()
			{
				ID = t.Operator_ID,
				Name = t.Operator_Name
			});
		}

		public IQueryable<ListModel> GetTaxtLists()
		{
			return DataService.GetTaxtLists().Select(t => new ListModel()
			{
				ID = t.TaxID,
				Name = t.TaxName
			});
		}
	}
}

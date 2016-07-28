using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Http.ModelBinding;
using CMSWebApi.APIFilters;
using CMSWebApi.DataModels;
using CMSWebApi.ServiceInterfaces;
using CMSWebApi.Utils;
using Newtonsoft.Json;
using PACDMModel.Model;

namespace CMSWebApi.BusinessServices.Dashboard
{
	public class DashboardBusinessService : BusinessBase<IDashboardService>
	{
		public DashboardUser GetDashboarduser(int userId, string advertisementPath = "")
		{
			var dashboard = DataService.GetDashbaoBoardUsers(userId);

			if (dashboard == null)
			{
				return new DashboardUser();
			}

			var element = ElementUser(userId);

			var dashboardUser = DashboardUser(dashboard);

			var rows = GetRows(dashboard.tCMSWeb_DashBoardLayouts.tCMSWeb_DashBoard_WidgetPositions, element);

			var resultRow = ProcessAdvertisement(dashboard.tCMSWeb_DashBoardLayouts.tCMSWeb_DashBoard_WidgetPositions, rows, advertisementPath);

			dashboardUser.Rows = resultRow;

			return dashboardUser;
		}

		public DashboardUser GetDefinedDashboard(int levelId, int userID, string advertisementPath="")
		{
			var dashboard = DataService.GetDashbaoBoard<tCMSWeb_DashBoardUserLevels, tCMSWeb_DashBoardUserLevels>(levelId, t => t.LevelID == levelId, t => t);//GetDashbaoBoardUserLevel(levelId);//

			if (dashboard == null)
			{
				return new DashboardUser();
			}

			var element = ElementUserLevel(levelId);

			var dashboardUser = DashboardUserLevel(dashboard, userID);

			var rows = GetRows(dashboard.tCMSWeb_DashBoardLayouts.tCMSWeb_DashBoard_WidgetPositions, element);
			var resultRow = ProcessAdvertisement(dashboard.tCMSWeb_DashBoardLayouts.tCMSWeb_DashBoard_WidgetPositions, rows, advertisementPath);
			dashboardUser.Rows = resultRow;

			return dashboardUser;
		}

		public IQueryable<Element> GetElements()
		{

			return DataService.GetElemements(t => new Element
			{
				Id = t.DshElementID,
				Name = t.DshElementName,
				Description = t.DshElementDesc,
				GroupSizeId = t.DshElementSize,
				TemplateJs = t.DshElemenJS,
				TemplateUrl = t.DshElementURL,
				TypeSize = t.DshElementType,
				TemplateParams = t.DshParams,
				Group = new WidgetGroup()
				{
					Id = t.tCMSWeb_DashBoard_WidgetGroupSize.WidgetGroupSizeID,
					Name = t.tCMSWeb_DashBoard_WidgetGroupSize.WidgetGroupSize,
					MaxWidgets = t.tCMSWeb_DashBoard_WidgetGroupSize.NumOfWidgets,
					IsHeader = t.tCMSWeb_DashBoard_WidgetGroupSize.IncludeHeader
				}
			});
		}

		public List<DataModels.Dashboard> GetDashboards(string advertisementPath = "")
		{
			var widgets = new List<Widget>();
			var dash = DataService.GetDashboards().ToList();

			var redash = dash.Select(t => new DataModels.Dashboard()
			{
				Id = t.DshLayoutID,
				Name = t.DshLayoutName,
				Description = t.DshLayoutDescription,
				Image = t.DshLayoutImage,
				Type = t.DshLayoutType,
				Rows = ProcessAdvertisementForDashboard(t.tCMSWeb_DashBoard_WidgetPositions, widgets, advertisementPath)
				//Rows = GetRows(t.tCMSWeb_DashBoard_WidgetPositions, widgets)
			}).ToList();

			


			return redash;
		}

		private List<Row> ProcessAdvertisementForDashboard(ICollection<tCMSWeb_DashBoard_WidgetPositions> pos, List<Widget> widgets,
			string advertisementPath)
		{
			var rows = GetRows(pos, widgets);
			var resultRow = ProcessAdvertisement(pos, rows, advertisementPath);
			return resultRow;
		}

		public void EditDashboard(DashboardUser dashboard, int userId)
		{
			var widgets = new List<tCMSWeb_DashBoard_User_Element>();
			foreach (var row in dashboard.Rows)
			{
				foreach (var col in row.Columns)
				{
					var wids = col.Widgets.Where(t=>t.Id > 0 && t.PositionId > 0).Select(t => new tCMSWeb_DashBoard_User_Element()
					{
						DshElementID = t.Id,
						DshElementOrder = t.Order,
						DshElementStyle = t.StyleId,
						DshTmpDisable = t.TemplateDisable,
						UserID = userId,
						WidgetGroupPositionID = t.PositionId
					});
					widgets.AddRange(wids);
				}
			}

			var dash = new tCMSWeb_DashBoardUsers()
			{
				UserID = userId,
				DshLayoutID = dashboard.Id,
				StyleID = dashboard.StyleId
			};

			DataService.DeleteDashboard(dash);

			DataService.InsertDashboard(dash);
			
			DataService.DeleteElements(userId);

			foreach (var widget in widgets)
			{
				DataService.InsertElement(widget);
			}

			DataService.SaveDashboard();
		}


		private List<Row> ProcessAdvertisement(IEnumerable<tCMSWeb_DashBoard_WidgetPositions> wds, List<Row> rows, string advertisementPath)
		{
			if (string.IsNullOrEmpty(advertisementPath)) return rows;
			var adRow = LoadAdvertisementRow(advertisementPath);
			if (adRow == null) return rows;

			var licenseRow = rows.FirstOrDefault(t => t.Id == adRow.Id);
			if (licenseRow != null)
			{
				foreach (var adColumn in adRow.Columns)
				{
					Column column = adColumn;

					foreach (var col in licenseRow.Columns)
					{
						//var wiposition = wds.FirstOrDefault(t => t.WidgetGroupPositionID == col.PositionId && t.WidgetGroupSizeID == col.GroupSizeId);
						if (col.Id == column.Id)
						{
							col.Group = column.Group;
							col.GroupSizeId = column.GroupSizeId;
							col.IsLock = column.IsLock;
							col.Widgets = column.Widgets;
						}
						
					}
				}
			}
			else
			{
				rows.Add(adRow);
			}
			return rows;
		}

		private Row LoadAdvertisementRow(string fpath)
		{
			TextReader reader = null;
			try
			{
				reader = File.OpenText(fpath);
				return JsonConvert.DeserializeObject<Row>(reader.ReadToEnd());
			}
			catch
			{ }
			finally
			{
				if (reader != null)
				{
					reader.Close();
					reader.Dispose();
					reader = null;
				}
			}
			return null;
		}

		private List<Row> GetRows(IEnumerable<tCMSWeb_DashBoard_WidgetPositions> dashboardPosition, List<Widget> element)
		{
			int tempId = 0;
			Row row = null;
			var rows = new List<Row>();
			foreach (var pos in dashboardPosition.OrderBy(t => t.StartRow))
			{
				if (tempId != pos.StartRow)
				{
					if (row != null)
					{
						rows.Add(row);
					}

					row = new Row() {Id = (int) pos.StartRow};
					row.Columns.Add(Column(pos, element));
				}
				else
				{
					row.Columns.Add(Column(pos, element));
				}

				tempId = (int) pos.StartRow;
			}
			if (tempId > 0)
			{
				rows.Add(row);
			}

			return rows;
		}

		private List<Widget> ElementUser(int userId)
		{
			var element = DataService.GetElementUsers(userId, t => new Widget()
			{
				Id = t.DshElementID,
				Name = t.tCMSWeb_DashBoardElements.DshElementName,
				Description = t.tCMSWeb_DashBoardElements.DshElementDesc,
				GroupSizeId = t.tCMSWeb_DashBoardElements.DshElementSize,
				Order = t.DshElementOrder,
				PositionId = t.WidgetGroupPositionID,
				StyleId = !t.DshElementStyle.HasValue ? 0 : t.DshElementStyle.Value,
				TemplateDisable = t.DshTmpDisable,
				TemplateJs = t.tCMSWeb_DashBoardElements.DshElemenJS,
				TypeSize = t.tCMSWeb_DashBoardElements.DshElementType,
				TemplateUrl = t.tCMSWeb_DashBoardElements.DshElementURL,
				TemplateParams = t.tCMSWeb_DashBoardElements.DshParams,
				Style = new DashboardStyle()
				{
					Id = t.tCMSWeb_DashBoardStyles.StyleID,
					BackgroudColor = t.tCMSWeb_DashBoardStyles.Background,
					FontColor = t.tCMSWeb_DashBoardStyles.FontColor
				},
				Group = new WidgetGroup()
				{
					Id = t.tCMSWeb_DashBoardElements.tCMSWeb_DashBoard_WidgetGroupSize.WidgetGroupSizeID,
					Name = t.tCMSWeb_DashBoardElements.tCMSWeb_DashBoard_WidgetGroupSize.WidgetGroupSize,
					IsHeader = t.tCMSWeb_DashBoardElements.tCMSWeb_DashBoard_WidgetGroupSize.IncludeHeader,
					MaxWidgets = t.tCMSWeb_DashBoardElements.tCMSWeb_DashBoard_WidgetGroupSize.NumOfWidgets
				}
			}).ToList();
			return element;
		}

		private DashboardUser DashboardUser(tCMSWeb_DashBoardUsers dashboard)
		{
			var dashboardUser = new DashboardUser()
			{
				Id = dashboard.DshLayoutID,
				UserId = dashboard.UserID,
				Name = dashboard.tCMSWeb_DashBoardLayouts.DshLayoutName,
				Description = dashboard.tCMSWeb_DashBoardLayouts.DshLayoutDescription,
				Image = dashboard.tCMSWeb_DashBoardLayouts.DshLayoutImage,
				Type = dashboard.tCMSWeb_DashBoardLayouts.DshLayoutType,
				StyleId = dashboard.StyleID,
				Style = dashboard.tCMSWeb_DashBoardStyles == null ? null : new DashboardStyle()
				{
					Id = dashboard.tCMSWeb_DashBoardStyles.StyleID,
					BackgroudColor = dashboard.tCMSWeb_DashBoardStyles.Background,
					FontColor = dashboard.tCMSWeb_DashBoardStyles.FontColor
				}
			};
			return dashboardUser;
		}

		private Column Column(tCMSWeb_DashBoard_WidgetPositions pos, IEnumerable<Widget> element)
		{
			var col = new Column()
			{
				Id = (int) pos.StartCol,
				GroupSizeId = pos.WidgetGroupSizeID,
				PositionId = pos.WidgetGroupPositionID,
				WidthSize = (int) pos.NumColsExpanded,
				Group = new WidgetGroup()
				{
					Id = pos.tCMSWeb_DashBoard_WidgetGroupSize.WidgetGroupSizeID,
					Name = pos.tCMSWeb_DashBoard_WidgetGroupSize.WidgetGroupSize,
					MaxWidgets = pos.tCMSWeb_DashBoard_WidgetGroupSize.NumOfWidgets
				},
				Widgets = element.Where(t => t.PositionId == pos.WidgetGroupPositionID).ToList()
			};
			return col;
		}

		private List<Widget> ElementUserLevel(int levelId)
		{
			var element = DataService.GetElementUserLevels(levelId, t => new Widget()
			{
				Id = t.DshElementID,
				Name = t.tCMSWeb_DashBoardElements.DshElementName,
				Description = t.tCMSWeb_DashBoardElements.DshElementDesc,
				GroupSizeId = t.tCMSWeb_DashBoardElements.DshElementSize,
				Order = t.DshElementOrder,
				PositionId = t.WidgetGroupPositionID,
				StyleId = !t.DshElementStyle.HasValue ? 0 : t.DshElementStyle.Value,
				TemplateDisable = t.DshTmpDisable,
				TemplateJs = t.tCMSWeb_DashBoardElements.DshElemenJS,
				TypeSize = t.tCMSWeb_DashBoardElements.DshElementType,
				TemplateUrl = t.tCMSWeb_DashBoardElements.DshElementURL,
				TemplateParams = t.tCMSWeb_DashBoardElements.DshParams,
				Style = new DashboardStyle()
				{
					Id = t.tCMSWeb_DashBoardStyles.StyleID,
					BackgroudColor = t.tCMSWeb_DashBoardStyles.Background,
					FontColor = t.tCMSWeb_DashBoardStyles.FontColor
				},
				Group = new WidgetGroup()
				{
					Id = t.tCMSWeb_DashBoardElements.tCMSWeb_DashBoard_WidgetGroupSize.WidgetGroupSizeID,
					Name = t.tCMSWeb_DashBoardElements.tCMSWeb_DashBoard_WidgetGroupSize.WidgetGroupSize,
					IsHeader = t.tCMSWeb_DashBoardElements.tCMSWeb_DashBoard_WidgetGroupSize.IncludeHeader,
					MaxWidgets = t.tCMSWeb_DashBoardElements.tCMSWeb_DashBoard_WidgetGroupSize.NumOfWidgets
				}
			}).ToList();
			return element;
		}

		private DashboardUser DashboardUserLevel(tCMSWeb_DashBoardUserLevels dashboard, int userID)
		{
			var dashboardUser = new DashboardUser()
			{
				Id = dashboard.DshLayoutID,
				UserId = userID,//,dashboard.UserID,
				Name = dashboard.tCMSWeb_DashBoardLayouts.DshLayoutName,
				Description = dashboard.tCMSWeb_DashBoardLayouts.DshLayoutDescription,
				Image = dashboard.tCMSWeb_DashBoardLayouts.DshLayoutImage,
				Type = dashboard.tCMSWeb_DashBoardLayouts.DshLayoutType,
				StyleId = dashboard.StyleID,
				Style = dashboard.tCMSWeb_DashBoardStyles == null ? null : new DashboardStyle()
				{
					Id = dashboard.tCMSWeb_DashBoardStyles.StyleID,
					BackgroudColor = dashboard.tCMSWeb_DashBoardStyles.Background,
					FontColor = dashboard.tCMSWeb_DashBoardStyles.FontColor
				}
			};
			return dashboardUser;
		}
	}
}

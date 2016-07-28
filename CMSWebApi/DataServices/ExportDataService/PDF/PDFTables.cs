using CMSWebApi.DataModels.ExportModel;
using CMSWebApi.Utils;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.DataServices.ExportDataService.PDF
{
	public class PDFTables
	{
		public static void InsertDataGrid(Document document, GridData model)
		{
			Table table = new Table();
			table.Borders = new Borders() { Width = 0.5 };
			table.Format.Alignment = ParagraphAlignment.Center;

			InitGrid(table, model.Name, model.RowDatas.FirstOrDefault().ColDatas);
			InsertRows(table, model.RowDatas);

			document.LastSection.Add(table);

			Paragraph paragraph = document.LastSection.AddParagraph();
			paragraph.Format.SpaceAfter = 10;
		}
		private static void InitGrid(Table table, string tableName, List<ColData> model)
		{
			Column tbCol;
			foreach (ColData col in model)
			{
				tbCol = table.AddColumn();
				tbCol.Format.Alignment = ParagraphAlignment.Center;

				if (tableName == ExportConst.GridNameList.DashboardMetric.ToString() || tableName == ExportConst.GridNameList.DashboardMetricDetail.ToString())
				{
					tbCol.Width = 55;
					if (col.Color == (int)ExportConst.CellFontFormat.GridHeaderFirstCell)
					{
						tbCol.Width = 110;
					}

				}
			}
		}
		private static void InsertRows(Table table, List<RowData> model)
		{
			foreach(RowData rowDB in model)
			{
				Row tbRow = table.AddRow();
				int colindex = 0;
				foreach (ColData colDB in rowDB.ColDatas)
				{
					Paragraph paragraph = new Paragraph();
					Cell cell = tbRow.Cells[colindex];
					switch (colDB.Color)
					{
						case (int)ExportConst.CellFontFormat.GridHeaderFirstCell:
							//tbRow.Shading.Color = Colors.DodgerBlue; //Color.FromCmyk(ExportConst.CMYK_GREEN.Cyan, ExportConst.CMYK_GREEN.Magenta, ExportConst.CMYK_GREEN.Yellow, ExportConst.CMYK_GREEN.Black);
							paragraph.Style = PDFStyles.StyleNameCSS.GridHeaderFirstCellCSS.ToString();
							cell.Shading.Color = Colors.SteelBlue;
							break;
						case (int)ExportConst.CellFontFormat.GridHeaderEndCell:
							//tbRow.Shading.Color = Colors.Black; //Color.FromCmyk(ExportConst.CMYK_BLACK.Cyan, ExportConst.CMYK_BLACK.Magenta, ExportConst.CMYK_BLACK.Yellow, ExportConst.CMYK_BLACK.Black);
							paragraph.Style = PDFStyles.StyleNameCSS.GridHeaderEndCellCSS.ToString();
							cell.Shading.Color = Colors.Black;
							break;
						case (int)ExportConst.CellFontFormat.GridHeaderCell:
							//tbRow.Shading.Color = Colors.CornflowerBlue; //Color.FromCmyk(ExportConst.CMYK_LESS_BLUE.Cyan, ExportConst.CMYK_LESS_BLUE.Magenta, ExportConst.CMYK_LESS_BLUE.Yellow, ExportConst.CMYK_LESS_BLUE.Black);
							paragraph.Style = PDFStyles.StyleNameCSS.GridHeaderCellCSS.ToString();
							cell.Shading.Color = Colors.LightSteelBlue;
							break;
						case (int)ExportConst.CellFontFormat.TextGreaterGoal:
							paragraph.Style = PDFStyles.StyleNameCSS.TextGreaterGoalCSS.ToString();
							break;
						case (int)ExportConst.CellFontFormat.TextInGoal:
							paragraph.Style = PDFStyles.StyleNameCSS.TextInGoalCSS.ToString();
							break;
						case (int)ExportConst.CellFontFormat.TextLessGoal:
							paragraph.Style = PDFStyles.StyleNameCSS.TextLessGoalCSS.ToString();
							break;
						case (int)ExportConst.CellFontFormat.GreaterGoalCell:
							paragraph.Style = PDFStyles.StyleNameCSS.BGGreaterGoalCSS.ToString();
							cell.Shading.Color = Colors.Green;
							break;
						case (int)ExportConst.CellFontFormat.InGoalCell:
							paragraph.Style = PDFStyles.StyleNameCSS.BGInGoalCSS.ToString();
							cell.Shading.Color = Colors.Gold;
							break;
						case (int)ExportConst.CellFontFormat.LessGoalCell:
							paragraph.Style = PDFStyles.StyleNameCSS.BGLessGoalCSS.ToString();
							cell.Shading.Color = Colors.Red;
							break;
						default:
							//tbRow.Shading.Color = Color.FromCmyk(ExportConst.CMYK_WHITE.Cyan, ExportConst.CMYK_WHITE.Magenta, ExportConst.CMYK_WHITE.Yellow, ExportConst.CMYK_WHITE.Black);
							paragraph.Style = PDFStyles.StyleNameCSS.TableDefaultTextCSS.ToString();
							break;
					}

					paragraph.AddText(colDB.Value);
					cell.Add(paragraph);
					colindex++;
				}
			}
		}
		private static void MergerCellCustom()
		{

		}
	}
}

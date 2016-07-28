using CMSWebApi.DataModels.ExportModel;
using CMSWebApi.Utils;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CMSWebApi.DataServices.ExportDataService.Excel
{
	public class BAMDashboardExcel
	{
		#region Properties
		private static WorksheetPart mainWorkSheetPart;
		private static SheetData mainSheetData;
	
		#endregion

		public static void ExportToExcel(string exportFile, byte[] logoImage, byte[] logoI3, ExportModel model)
		{
			//ThangPham, Mention for insert word, excel template, Feb 26 2016.
			//Excel requires that your elements are in the proper order per the schema.  
			//In contrast, Word is much more forgiving -you can in many cases reorder your elements in Word, 
			//and even though it is not technically accurate, Word will still open the document.This is not the case with Excel.
			//When I have a workbook that will not open, this is the first thing that I check.

			InitDocument(exportFile);

			InsertHeaderFooter(exportFile, logoImage, model);

			InsertContent(exportFile, model);

			//switch (model.ReportInfo.ReportType)
			//{
			//	case (int)ExportConst.ReportType.RebarDashBoard:
			//		RebarDashboardContent(exportFile, model);
			//		break;
			//	default:
			//		InsertContent(exportFile, model);
			//		break;
			//}

			InsertPictureInHeaderFooter(exportFile, logoImage, logoI3);
		}

		private static void InitDocument(string exportFile)
		{
			ExcelDocument excelDocument = new ExcelDocument();
			excelDocument.CreatePackage(exportFile);

			using (SpreadsheetDocument document = SpreadsheetDocument.Open(exportFile, true))
			{
				IEnumerable<Sheet> sheets = document.WorkbookPart.Workbook.Descendants<Sheet>().Where(s => s.Name == ExportConst.SHEET_NAME);
				if (sheets.Count() == 0) { return; }

				mainWorkSheetPart = (WorksheetPart)document.WorkbookPart.GetPartById(sheets.First().Id);
				mainSheetData = mainWorkSheetPart.Worksheet.GetFirstChild<SheetData>();
			}
		}

		private static void InsertContent(string exportFile, ExportModel model)
		{
			foreach (GridData grid in model.GridModels)
			{
				ExcelTables.InsertDataGrid(grid, exportFile);
			}
			
			if(model.ChartModels.Count > 0)
			{
				var rowIndex = 0;
				foreach(ChartData chart in model.ChartModels)
				{
					rowIndex = chart.Format.RowIndex;
					ExcelCharts.InsertCharts(chart, exportFile, chart.Format.ColIndex, ref rowIndex, chart.Format.Width, chart.Format.Height);
				}
			}
		}

		private static void RebarDashboardContent(string exportFile, ExportModel model)
		{
			var rowIndex = 0;
			GridData grid = model.GridModels.FirstOrDefault(g => g.Name == "MarkExceptionTable");
			if (grid !=null)
			{
				ExcelTables.InsertDataGrid(grid, exportFile);
			}

			grid = model.GridModels.FirstOrDefault(g => g.Name == "RiskFactorTable");
			if(grid != null)
			{
				
				ExcelTables.InsertDataGrid(grid, exportFile);
			}

			ChartData chart = model.ChartModels.FirstOrDefault(c => c.Name == "ChartMartException");
			if (chart != null)
			{
				ExcelCharts.InsertCharts(chart, exportFile, chart.Format.ColIndex, ref rowIndex, chart.Format.Width, chart.Format.Height);
			}

			chart = model.ChartModels.FirstOrDefault(c => c.Name == "ChartRiskFactor");
			if(chart != null)
			{
				ExcelCharts.InsertCharts(chart, exportFile, chart.Format.ColIndex, ref rowIndex, chart.Format.Width, chart.Format.Height);
			}

			//grid = model.GridModels.FirstOrDefault(g => g.Name == "RiskFactorTable");
			//ExcelTables.InsertDataGrid(grid, exportFile, 1, ref rowIndex, grid.Format);
		}

		private static void InsertHeaderFooter(string exportFile, byte[] logoImage, ExportModel model)
		{
			using (SpreadsheetDocument document = SpreadsheetDocument.Open(exportFile, true))
			{
				IEnumerable<Sheet> sheets = document.WorkbookPart.Workbook.Descendants<Sheet>().Where(s => s.Name == ExportConst.SHEET_NAME);
				if (sheets.Count() == 0) { return; }

				WorksheetPart worksheetPart = (WorksheetPart)document.WorkbookPart.GetPartById(sheets.First().Id);
				Worksheet worksheet = worksheetPart.Worksheet;
				HeaderFooter headF = worksheet.AppendChild(new HeaderFooter());

				//Insert header page
				OddHeader oddHeader = headF.AppendChild(new OddHeader());
				oddHeader.Space = SpaceProcessingModeValues.Preserve;
				oddHeader.Text = "&L&G&C&\"-,Bold\"&18&K04-020" + model.ReportInfo.ReportName + "&R" + model.ReportInfo.CreatedBy + "\n" + model.ReportInfo.CreateDate + "\n";

				//Insert footer page
				OddFooter oddFooter = headF.AppendChild(new OddFooter());
				oddFooter.Space = SpaceProcessingModeValues.Preserve;
				oddFooter.Text = "&L&G&C" + model.ReportInfo.Footer; //"&L&C" + model.ReportInfo.Footer + "&R&G";
			}

			//if (logoImage.Length > 0)
			//{
			//	InsertPicputeInHeader(exportFile, logoImage);
			//}
		}

		private static void InsertPictureInHeaderFooter(string exportFile, byte[] logoImage, byte[] logoI3)
		{
			using (SpreadsheetDocument document = SpreadsheetDocument.Open(exportFile, true))
			{
				IEnumerable<Sheet> sheets = document.WorkbookPart.Workbook.Descendants<Sheet>().Where(s => s.Name == ExportConst.SHEET_NAME);
				if (sheets.Count() == 0) { return; }

				WorksheetPart worksheetPart = (WorksheetPart)document.WorkbookPart.GetPartById(sheets.First().Id);
				Worksheet workSheet = worksheetPart.Worksheet;

				VmlDrawingPart vmlDrawingPart = worksheetPart.AddNewPart<VmlDrawingPart>(sheets.First().Id);
				ExcelDocument.GenerateVmlDrawingPartContent(vmlDrawingPart);

				//logo header
				ImagePart imgp = vmlDrawingPart.AddNewPart<ImagePart>("image/jpeg", "rId1");
				string imagedata = Convert.ToBase64String(logoImage);
				string strbase64 = Commons.Utils.String2Base64(imagedata);

				using (Stream fs = ExcelDocument.GetBinaryDataStream(imagedata))
				{
					imgp.FeedData(fs);
				}

				//logo footer
				ImagePart imgpF = vmlDrawingPart.AddNewPart<ImagePart>("image/jpeg", "rId2");
				string imageDataF = Convert.ToBase64String(logoI3);
				string strBase64F = Commons.Utils.String2Base64(imageDataF);

				using (Stream fs = ExcelDocument.GetBinaryDataStream(imageDataF))
				{
					imgpF.FeedData(fs);
				}

				LegacyDrawingHeaderFooter lhf = new LegacyDrawingHeaderFooter();
				lhf.Id = worksheetPart.GetIdOfPart(vmlDrawingPart);
				workSheet.Append(lhf);

				foreach (OpenXmlElement e in workSheet.Elements().ToList())
				{
					if (e.GetType().Name == "PageMargins")
					{
						((PageMargins)e).Top = 1.3;
					}
				}

			}
		}
	}
}

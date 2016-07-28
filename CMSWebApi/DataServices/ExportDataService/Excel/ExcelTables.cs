using CMSWebApi.DataModels.ExportModel;
using CMSWebApi.Utils;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.DataServices.ExportDataService.Excel
{
	public class ExcelTables
	{
		/// <summary>
		/// Insert table to report.
		/// </summary>
		/// <param name="model">data to export</param>
		/// <param name="exportFile">path of file name</param>
		/// <param name="colIndex">index of cell that used to insert data.</param>
		/// <param name="rowIndex">index of row that used to insert data.</param>
		/// <param name="widthSpace">number of columnn that used to merge for default coulmn.</param>
		/// <param name="headwidth">number of columnn that used to merge for First column.</param>
		public static void InsertDataGrid(GridData model, string exportFile)
		{
			using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Open(exportFile, true))
			{
				WorkbookPart workbook = spreadsheet.WorkbookPart;
				//create a reference to Sheet1  
				WorksheetPart worksheetPart = workbook.WorksheetParts.Last();
				SheetData sheetdata = worksheetPart.Worksheet.GetFirstChild<SheetData>();
				MergeCells mergeCells = worksheetPart.Worksheet.GetFirstChild<MergeCells>();
				Columns cols = worksheetPart.Worksheet.GetFirstChild<Columns>();
				Row rowHeader;

				//cols.Append(new Column() { Min = (UInt32Value)1U, Max = (UInt32Value)1U, Width = 30.6640625D, CustomWidth = true });
				foreach (RowData rowData in model.RowDatas)
				{
					if (rowData.Type == (int)ExportConst.RowDataType.Header)
					{
						//Header table
						if (sheetdata.Elements<Row>().Any(r => r.RowIndex == rowData.ColDatas.FirstOrDefault().RowIndex))
						{
							rowHeader = sheetdata.Elements<Row>().Where(r => r.RowIndex == rowData.ColDatas.FirstOrDefault().RowIndex).FirstOrDefault();
							foreach (ColData col in rowData.ColDatas)
							{
								if (col.CustomerWidth)
								{
									cols.Append(new Column() { Min = (UInt32)col.ColIndex, Max = (UInt32)col.ColIndex, Width = col.Width, CustomWidth = col.CustomerWidth });
								}

								Cell headerCell = InsertCell(col.ColIndex, col.RowIndex, col.Value);
								headerCell.StyleIndex = col.Color == 0 ? (int)ExportConst.CellFontFormat.Default : (UInt32)col.Color;
								rowHeader.AppendChild(headerCell);

								string fromCell, toCell = string.Empty;
								//The first cell of table, merge 2 rows
								fromCell = ExcelDocument.GetColumnName(col.ColIndex) + col.RowIndex;
								if (col.MergeCells.Rows > 1)
								{
									toCell = ExcelDocument.GetColumnName(col.ColIndex + col.MergeCells.Cells - 1) + col.MergeCells.Rows;
								}
								else
								{
									toCell = ExcelDocument.GetColumnName(col.ColIndex + col.MergeCells.Cells - 1) + col.RowIndex;
								}

								MergerCellCustom(ref mergeCells, fromCell, toCell);
							}
						}
						else
						{
							rowHeader = new Row() { RowIndex = (UInt32)rowData.ColDatas.FirstOrDefault().RowIndex, CustomHeight = true, Height = 30.75D };
							foreach (ColData col in rowData.ColDatas)
							{
								if (col.CustomerWidth)
								{
									cols.Append(new Column() { Min = (UInt32)col.ColIndex, Max = (UInt32)col.ColIndex, Width = col.Width, CustomWidth = col.CustomerWidth });
								}

								Cell headerCell = InsertCell(col.ColIndex, col.RowIndex, col.Value);
								headerCell.StyleIndex = col.Color == 0 ? (int)ExportConst.CellFontFormat.Default : (UInt32)col.Color;
								rowHeader.AppendChild(headerCell);

								string fromCell, toCell = string.Empty;
								//The first cell of table, merge 2 rows
								fromCell = ExcelDocument.GetColumnName(col.ColIndex) + col.RowIndex;
								if (col.MergeCells.Rows > 1)
								{
									toCell = ExcelDocument.GetColumnName(col.ColIndex + col.MergeCells.Cells - 1) + col.MergeCells.Rows;
								}
								else
								{
									toCell = ExcelDocument.GetColumnName(col.ColIndex + col.MergeCells.Cells - 1) + col.RowIndex;
								}

								MergerCellCustom(ref mergeCells, fromCell, toCell);
							}

							sheetdata.AppendChild(rowHeader);
						}
					}
					else
					{
						if (sheetdata.Elements<Row>().Any(r => r.RowIndex == rowData.ColDatas.FirstOrDefault().RowIndex))
						{
							rowHeader = sheetdata.Elements<Row>().Where(r => r.RowIndex == rowData.ColDatas.FirstOrDefault().RowIndex).First();
							UpdateRow(rowHeader, rowData.ColDatas, ref mergeCells);
						}
						else
						{
							rowHeader = new Row() { RowIndex = (UInt32)rowData.ColDatas.FirstOrDefault().RowIndex, CustomHeight = true, Height = 30.75D };
							sheetdata.AppendChild(InsertRow(rowData.ColDatas, ref mergeCells));
						}
					}
				}
			}
		}

		private static Cell InsertCell(int columnIndex, int rowIndex, object cellValue)
		{
			Cell cell = new Cell();

			cell.DataType = CellValues.InlineString;
			cell.CellReference = ExcelDocument.GetColumnName(columnIndex) + rowIndex;

			InlineString inlineString = new InlineString();
			Text t = new Text();

			t.Text = cellValue == null? string.Empty: cellValue.ToString();
			inlineString.AppendChild(t);
			cell.AppendChild(inlineString);

			return cell;
		}

		private static Row InsertRow(List<ColData> colDatas, ref MergeCells mergeCells)
		{
			Row row = new Row { RowIndex = (UInt32)colDatas.FirstOrDefault().RowIndex };
			string fromCell = string.Empty;
			string toCell = string.Empty;
			foreach (ColData col in colDatas)
			{
				Cell cell = InsertCell(col.ColIndex, col.RowIndex, col.Value);
				cell.StyleIndex = col.Color == 0 ? (int)ExportConst.CellFontFormat.Default : (UInt32)col.Color;
				row.AppendChild(cell);

				if(col.MergeCells.Cells > 1)
				{
					fromCell = ExcelDocument.GetColumnName(col.ColIndex) + col.RowIndex;
					if (col.MergeCells.Rows > 1)
					{
						toCell = ExcelDocument.GetColumnName(col.ColIndex + col.MergeCells.Cells - 1) + col.MergeCells.Rows;
					}
					else
					{
						toCell = ExcelDocument.GetColumnName(col.ColIndex + col.MergeCells.Cells - 1) + col.RowIndex;
					}
					MergerCellCustom(ref mergeCells, fromCell, toCell);
				}
			}

			return row;
		}

		private static void UpdateRow(Row row, List<ColData> colDatas, ref MergeCells mergeCells)
		{
			string fromCell = string.Empty;
			string toCell = string.Empty;
			foreach (ColData col in colDatas)
			{
				Cell cell = InsertCell(col.ColIndex, col.RowIndex, col.Value);
				cell.StyleIndex = col.Color == 0 ? (int)ExportConst.CellFontFormat.Default : (UInt32)col.Color;
				row.AppendChild(cell);

				if (col.MergeCells.Cells > 1)
				{
					fromCell = ExcelDocument.GetColumnName(col.ColIndex) + col.RowIndex;
					if (col.MergeCells.Rows > 1)
					{
						toCell = ExcelDocument.GetColumnName(col.ColIndex + col.MergeCells.Cells - 1) + col.MergeCells.Rows;
					}
					else
					{
						toCell = ExcelDocument.GetColumnName(col.ColIndex + col.MergeCells.Cells - 1) + col.RowIndex;
					}
					MergerCellCustom(ref mergeCells, fromCell, toCell);
				}
			}
		}

		private static void MergerCellCustom(ref MergeCells mergeCells, string fromCell, string toCell)
		{
			string cellRef = string.Format("{0}:{1}", fromCell, toCell);
			MergeCell mergeCell = new MergeCell() { Reference = cellRef };
			mergeCells.Append(mergeCell);
		}
	}
}

using CMSWebApi.DataModels.ExportModel;
using CMSWebApi.Utils;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using A = DocumentFormat.OpenXml.Drawing;
using C = DocumentFormat.OpenXml.Drawing.Charts;
using C14 = DocumentFormat.OpenXml.Office2010.Drawing.Charts;
using C15 = DocumentFormat.OpenXml.Office2013.Drawing.Chart;
using Cs = DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;

namespace CMSWebApi.DataServices.ExportDataService.Excel
{
	public class ExcelCharts
	{
		public enum ExportChartTypes
		{
			LineChart = 1,
			BarChart,
			ColumnChart,
			PieChart
		}

		public static void InsertCharts(ChartData model, string exportFile)
		{
			switch (model.ChartType)
			{
				case (int)ExportChartTypes.ColumnChart:
					InsertBarChart(model, exportFile);
					break;
				case (int)ExportChartTypes.LineChart:
					InsertLineChart(model, exportFile);
					break;
				default:
					InsertBarChart(model, exportFile);
					break;
			}
		}

		public static void InsertCharts(ChartData model, string exportFile, int colIndex, ref int rowIndex, int width, int height)
		{
			switch (model.ChartType)
			{
				case (int)ExportChartTypes.ColumnChart:
					InsertBarChart(model, exportFile, colIndex, ref rowIndex, width, height);
					break;
				case (int)ExportChartTypes.LineChart:
					InsertLineChart(model, exportFile, colIndex, ref rowIndex, width, height);
					break;
				default:
					InsertBarChart(model, exportFile, colIndex, ref rowIndex, width, height);
					break;
			}
		}

		public static void InsertBarChart(ChartData model, string exportFile)
		{
			// Open the document for editing.
			using (SpreadsheetDocument document = SpreadsheetDocument.Open(exportFile, true))
			{
				IEnumerable<Sheet> sheets = document.WorkbookPart.Workbook.Descendants<Sheet>().Where(s => s.Name == ExportConst.SHEET_NAME);
				if (sheets.Count() == 0) { return; }

				WorksheetPart worksheetPart = (WorksheetPart)document.WorkbookPart.GetPartById(sheets.First().Id);

				Worksheet worksheet = worksheetPart.Worksheet;
				SheetData sheetData = worksheet.Elements<SheetData>().FirstOrDefault();
				MergeCells mergeCells = worksheet.Elements<MergeCells>().FirstOrDefault();

				//Insert Chart Title
				Row rowExisted = sheetData.Elements<Row>().Where(r => r.RowIndex == model.Format.RowIndex).FirstOrDefault();
				if (rowExisted != null)
				{
					ExcelDocument.MergeCellCustom(rowExisted, ref mergeCells, ExcelDocument.GetColumnName(model.Format.ColIndex + 1) + model.Format.RowIndex, ExcelDocument.GetColumnName(model.Format.ColIndex + 5) + model.Format.RowIndex, model.Title, model.Format.RowIndex, (int)ExportConst.CellFontFormat.ChartTitle);
				}
				else
				{
					ExcelDocument.MergeCellCustom(ref sheetData, ref mergeCells, ExcelDocument.GetColumnName(model.Format.ColIndex + 1) + model.Format.RowIndex, ExcelDocument.GetColumnName(model.Format.ColIndex + 5) + model.Format.RowIndex, model.Title, model.Format.RowIndex, (int)ExportConst.CellFontFormat.ChartTitle);
				}

				DrawingsPart drawingsPart;
				if (worksheetPart.DrawingsPart != null)
				{
					drawingsPart = worksheetPart.DrawingsPart;
				}
				else
				{
					drawingsPart = worksheetPart.AddNewPart<DrawingsPart>();
					worksheetPart.Worksheet.Append(new Drawing() { Id = worksheetPart.GetIdOfPart(drawingsPart) });
					worksheetPart.Worksheet.Save();
				}

				// Add a new chart and set the chart language to English-US.
				ChartPart chartPart = drawingsPart.AddNewPart<ChartPart>();
				chartPart.ChartSpace = new ChartSpace();
				chartPart.ChartSpace.Append(new EditingLanguage() { Val = new StringValue("en-US") });
				DocumentFormat.OpenXml.Drawing.Charts.Chart chart = chartPart.ChartSpace.AppendChild(new DocumentFormat.OpenXml.Drawing.Charts.Chart());
				chart.Append(new AutoTitleDeleted() { Val = true });

				//Insert Chart Title
				A.Run run = new A.Run();
				A.RunProperties runProperties = new A.RunProperties() { Language = "en-US", FontSize = 12 };
				A.Text text = new A.Text();
				text.Text = model.Title;
				run.Append(runProperties);
				run.Append(text);
				Paragraph paragraph = new Paragraph();
				paragraph.Append(new ParagraphProperties());
				paragraph.Append(run);
				RichText richText = new RichText();
				richText.Append(new BodyProperties());
				richText.Append(new ListStyle());
				richText.Append(paragraph);
				ChartText chartText = new ChartText();
				chartText.Append(richText);
				C.Overlay overlay = new C.Overlay() { Val = false };
				Title titleChart = new Title();
				titleChart.Append(chartText);
				titleChart.Append(new Layout());
				titleChart.Append(overlay);
				chart.Append(titleChart);

				// Create a new clustered column chart.
				PlotArea plotArea = chart.AppendChild<PlotArea>(new PlotArea());
				Layout layout = plotArea.AppendChild<Layout>(new Layout());

				//ThangPham, Add chart type to report, Feb 24 2016
				BarChart barChart = new BarChart(new BarDirection() { Val = new EnumValue<BarDirectionValues>(BarDirectionValues.Bar) },
					new BarGrouping() { Val = new EnumValue<BarGroupingValues>(BarGroupingValues.Clustered) });
				if (model.ChartType == (int)ExportConst.ChartType.ColumnChart)
				{
					barChart = new BarChart(new BarDirection() { Val = new EnumValue<BarDirectionValues>(BarDirectionValues.Column) },
					new BarGrouping() { Val = new EnumValue<BarGroupingValues>(BarGroupingValues.Clustered) });
				}

				plotArea.AppendChild<BarChart>(barChart);

				GapWidth gapwidth = new GapWidth() { Val = (UInt16Value)80U }; //default
				if(model.ChartDataItems.Count >= 2)
				{
					gapwidth = new GapWidth() { Val = (UInt16Value)225U }; //set small column series.
				}
				barChart.AppendChild(gapwidth);
				uint index = 0;

				//=================================================================================================================================================
				// Iterate through each key in the Dictionary collection and add the key to the chart Series
				// and add the corresponding value to the chart Values.
				BarChartSeries barChartSeries = barChart.AppendChild<BarChartSeries>(new BarChartSeries(new Index()
				{
					Val = new UInt32Value(index)
				},
				new Order() { Val = new UInt32Value(index) },
				new SeriesText(new NumericValue() { Text = "" })));

				var chartItems = model.ChartDataItems;

				StringLiteral strLit = barChartSeries.AppendChild<CategoryAxisData>(new CategoryAxisData()).AppendChild<StringLiteral>(new StringLiteral());
				strLit.Append(new PointCount() { Val = new UInt32Value(Convert.ToUInt32(chartItems.Count)) });

				DocumentFormat.OpenXml.Drawing.Charts.Values cVal = barChartSeries.AppendChild(new DocumentFormat.OpenXml.Drawing.Charts.Values());
				NumberLiteral numLit = cVal.AppendChild(new NumberLiteral());

				numLit.AppendChild<FormatCode>(new FormatCode() { Text = Utils.ExportConst.exceldataformat[model.ChartType] });
				numLit.AppendChild<PointCount>(new PointCount() { Val = new UInt32Value(Convert.ToUInt32(chartItems.Count)) });

				foreach (ChartDataItem chartItem in chartItems)
				{
					DataPoint dtPoint = barChartSeries.AppendChild<DataPoint>(new DataPoint());
					dtPoint.Append(new Index() { Val = new UInt32Value(Convert.ToUInt32(index)) });

					ChartShapeProperties cSP = dtPoint.AppendChild(new ChartShapeProperties());
					SolidFill slF = cSP.AppendChild(new SolidFill());
					PresetColor pc = slF.AppendChild(new PresetColor());
					switch (chartItem.Color)
					{
						case (int)ExportConst.ChartColor.Green:
							pc.Val = PresetColorValues.Green;
							break;
						case (int)ExportConst.ChartColor.Yellow:
							pc.Val = PresetColorValues.Yellow;
							break;
						case (int)ExportConst.ChartColor.Red:
							pc.Val = PresetColorValues.Red;
							break;
						case (int)ExportConst.ChartColor.Orange:
							pc.Val = PresetColorValues.Orange;
							break;
						case (int)ExportConst.ChartColor.Blue:
							pc.Val = PresetColorValues.Blue;
							break;
						default:
							pc.Val = PresetColorValues.Black;
							break;
					}

					StringPoint stPoint = strLit.AppendChild<StringPoint>(new StringPoint() { Index = new UInt32Value(Convert.ToUInt32(index)) });
					NumericValue numval = new NumericValue();
					numval.Text = chartItem.Name;
					stPoint.Append(numval);

					NumericPoint numPoint = numLit.AppendChild<NumericPoint>(new NumericPoint() { Index = new UInt32Value(Convert.ToUInt32(index)) });
					NumericValue cNumVal = new NumericValue();
					cNumVal.Text = chartItem.Value;
					numPoint.Append(cNumVal);

					index++;
				}

				Overlap ovl = new Overlap();
				ovl.Val = -30;
				barChart.AppendChild<Overlap>(ovl);
				
				DataLabels dataLabels = new DataLabels();
				DataLabelPosition dataLabelPosition = new DataLabelPosition() { Val = DataLabelPositionValues.OutsideEnd };
				ShowValue showValue1 = new ShowValue() { Val = true };
				dataLabels.Append(dataLabelPosition);
				dataLabels.Append(showValue1);

				dataLabels.Append(new ShowPercent() { Val = true });

				barChart.Append(dataLabels);

				//=================================================================================================================================================
				barChart.Append(new AxisId() { Val = new UInt32Value(48650112u) });
				barChart.Append(new AxisId() { Val = new UInt32Value(48672768u) });

				// Add the Category Axis.
				CategoryAxis catAx = plotArea.AppendChild<CategoryAxis>(new CategoryAxis(new AxisId() { Val = new UInt32Value(48650112u) }, new Scaling(new Orientation()
				{
					Val = new EnumValue<DocumentFormat.
						OpenXml.Drawing.Charts.OrientationValues>(DocumentFormat.OpenXml.Drawing.Charts.OrientationValues.MinMax)
				}),
					new AxisPosition() { Val = new EnumValue<AxisPositionValues>(AxisPositionValues.Bottom) },
					new TickLabelPosition() { Val = new EnumValue<TickLabelPositionValues>(TickLabelPositionValues.NextTo) },
					new CrossingAxis() { Val = new UInt32Value(48672768U) },
					new Crosses() { Val = new EnumValue<CrossesValues>(CrossesValues.AutoZero) },
					new AutoLabeled() { Val = new BooleanValue(true) },
					new LabelAlignment() { Val = new EnumValue<LabelAlignmentValues>(LabelAlignmentValues.Center) },
					new LabelOffset() { Val = new UInt16Value((ushort)100) }));

				// Add the Value Axis.
				ValueAxis valAx = plotArea.AppendChild<ValueAxis>(new ValueAxis(new AxisId() { Val = new UInt32Value(48672768u) },
					new Scaling(new Orientation()
					{
						Val = new EnumValue<DocumentFormat.OpenXml.Drawing.Charts.OrientationValues>(
							DocumentFormat.OpenXml.Drawing.Charts.OrientationValues.MinMax)
					}),
					new AxisPosition() { Val = new EnumValue<AxisPositionValues>(AxisPositionValues.Left) },

					new DocumentFormat.OpenXml.Drawing.Charts.NumberingFormat()
					{
						FormatCode = new StringValue("General"),
						SourceLinked = new BooleanValue(true)
					}, new TickLabelPosition()
					{
						Val = new EnumValue<TickLabelPositionValues>
							(TickLabelPositionValues.NextTo)
					}, new CrossingAxis() { Val = new UInt32Value(48650112U) },
					new Crosses() { Val = new EnumValue<CrossesValues>(CrossesValues.AutoZero) },
					new CrossBetween() { Val = new EnumValue<CrossBetweenValues>(CrossBetweenValues.Between) }));

				// Add the chart Legend.
				//Legend legend = chart.AppendChild<Legend>(new Legend(new LegendPosition() { Val = new EnumValue<LegendPositionValues>(LegendPositionValues.Right) },
				//    new Layout()));

				chart.Append(new PlotVisibleOnly() { Val = new BooleanValue(true) });

				// Save the chart part.
				chartPart.ChartSpace.Save();

				// Position the chart on the worksheet using a TwoCellAnchor object.
				if (drawingsPart.WorksheetDrawing == null)
				{
					drawingsPart.WorksheetDrawing = new WorksheetDrawing();
				}
				TwoCellAnchor twoCellAnchor = drawingsPart.WorksheetDrawing.AppendChild<TwoCellAnchor>(new TwoCellAnchor());
				twoCellAnchor.Append(new DocumentFormat.OpenXml.Drawing.Spreadsheet.FromMarker(new ColumnId(model.Format.ColIndex.ToString()),
					new ColumnOffset("0"),
					new RowId(model.Format.RowIndex.ToString()),
					new RowOffset("0")));
				twoCellAnchor.Append(new DocumentFormat.OpenXml.Drawing.Spreadsheet.ToMarker(new ColumnId((model.Format.ColIndex + model.Format.Width).ToString()),
					new ColumnOffset("0"),
					new RowId((model.Format.RowIndex + model.Format.Height).ToString()),
					new RowOffset("0")));

				// Append a GraphicFrame to the TwoCellAnchor object.
				DocumentFormat.OpenXml.Drawing.Spreadsheet.GraphicFrame graphicFrame = twoCellAnchor.AppendChild(new DocumentFormat.OpenXml.Drawing.Spreadsheet.GraphicFrame());
				graphicFrame.Macro = "";

				graphicFrame.Append(new DocumentFormat.OpenXml.Drawing.Spreadsheet.NonVisualGraphicFrameProperties(
					new DocumentFormat.OpenXml.Drawing.Spreadsheet.NonVisualDrawingProperties() { Id = new UInt32Value(2u), Name = model.Name },
					new DocumentFormat.OpenXml.Drawing.Spreadsheet.NonVisualGraphicFrameDrawingProperties()));

				graphicFrame.Append(new Transform(new Offset() { X = 0L, Y = 0L }, new Extents() { Cx = 0L, Cy = 0L }));
				graphicFrame.Append(new Graphic(new GraphicData(new ChartReference() { Id = drawingsPart.GetIdOfPart(chartPart) }) { Uri = "http://schemas.openxmlformats.org/drawingml/2006/chart" }));

				twoCellAnchor.Append(new ClientData());

				// Save the WorksheetDrawing object.
				drawingsPart.WorksheetDrawing.Save();
			}
		}

		public static void InsertLineChart(ChartData model, string exportFile)
		{
			// Open the document for editing.
			using (SpreadsheetDocument document = SpreadsheetDocument.Open(exportFile, true))
			{
				IEnumerable<Sheet> sheets = document.WorkbookPart.Workbook.Descendants<Sheet>().Where(s => s.Name == ExportConst.SHEET_NAME);
				if (sheets.Count() == 0) { return; }

				WorksheetPart worksheetPart = (WorksheetPart)document.WorkbookPart.GetPartById(sheets.First().Id);
				
				Worksheet worksheet = worksheetPart.Worksheet;
				SheetData sheetData = worksheet.Elements<SheetData>().FirstOrDefault();
				MergeCells mergeCells = worksheet.Elements<MergeCells>().FirstOrDefault();

				//Insert Chart title
				ExcelDocument.MergeCellCustom(ref sheetData, ref mergeCells, ExcelDocument.GetColumnName(model.Format.ColIndex + 1) + model.Format.RowIndex, ExcelDocument.GetColumnName(model.Format.ColIndex + 5) + model.Format.RowIndex, model.Title, model.Format.RowIndex, (int)ExportConst.CellFontFormat.ChartTitle);

				DrawingsPart drawingsPart;
				if (worksheetPart.DrawingsPart != null)
				{
					drawingsPart = worksheetPart.DrawingsPart;
				}
				else
				{
					drawingsPart = worksheetPart.AddNewPart<DrawingsPart>();
					worksheetPart.Worksheet.Append(new Drawing() { Id = worksheetPart.GetIdOfPart(drawingsPart) });
					worksheetPart.Worksheet.Save();
				}

				// Add a new chart and set the chart language to English-US.
				ChartPart chartPart = drawingsPart.AddNewPart<ChartPart>();
				chartPart.ChartSpace = new ChartSpace();
				chartPart.ChartSpace.Append(new EditingLanguage() { Val = new StringValue("en-US") });
				DocumentFormat.OpenXml.Drawing.Charts.Chart chart = chartPart.ChartSpace.AppendChild(
					new DocumentFormat.OpenXml.Drawing.Charts.Chart());

				//Insert Chart Title
				A.Run run = new A.Run();
				A.RunProperties runProperties = new A.RunProperties() { Language = "en-US", FontSize = 12 };
				A.Text text = new A.Text();
				text.Text = model.Title;
				run.Append(runProperties);
				run.Append(text);
				Paragraph paragraph = new Paragraph();
				paragraph.Append(new ParagraphProperties());
				paragraph.Append(run);
				RichText richText = new RichText();
				richText.Append(new BodyProperties());
				richText.Append(new ListStyle());
				richText.Append(paragraph);
				ChartText chartText = new ChartText();
				chartText.Append(richText);
				C.Overlay overlay = new C.Overlay() { Val = false };
				Title titleChart = new Title();
				titleChart.Append(chartText);
				titleChart.Append(new Layout());
				titleChart.Append(overlay);
				chart.Append(titleChart);

				// Create a new clustered column chart.
				PlotArea plotArea = chart.AppendChild(new PlotArea());
				Layout layout = plotArea.AppendChild(new Layout());

				//ThangPham, Add the lineChart, Feb 24 2016
				plotArea.AppendChild(GenerateLineChart(model));

				// Add the Category Axis.
				plotArea.AppendChild(LineChartGenerateCategoryAxis());
				// Add the Value Axis.
				plotArea.AppendChild(LineChartGenerateValueAxis());

				// Add the chart Legend.
				Legend legend = chart.AppendChild(new Legend(new LegendPosition() { Val = new EnumValue<LegendPositionValues>(LegendPositionValues.Right) }, new Layout()));

				chart.Append(new PlotVisibleOnly() { Val = new BooleanValue(true) });

				// Save the chart part.
				chartPart.ChartSpace.Save();

				// Position the chart on the worksheet using a TwoCellAnchor object.
				if (drawingsPart.WorksheetDrawing == null)
				{
					drawingsPart.WorksheetDrawing = new WorksheetDrawing();
				}
				TwoCellAnchor twoCellAnchor = drawingsPart.WorksheetDrawing.AppendChild(new TwoCellAnchor());
				twoCellAnchor.Append(new DocumentFormat.OpenXml.Drawing.Spreadsheet.FromMarker(new ColumnId(model.Format.ColIndex.ToString()),
					new ColumnOffset("0"),
					new RowId(model.Format.RowIndex.ToString()),
					new RowOffset("0")));
				twoCellAnchor.Append(new DocumentFormat.OpenXml.Drawing.Spreadsheet.ToMarker(new ColumnId((model.Format.ColIndex + model.Format.Width).ToString()),
					new ColumnOffset("0"),
					new RowId((model.Format.RowIndex + model.Format.Height).ToString()),
					new RowOffset("0")));

				// Append a GraphicFrame to the TwoCellAnchor object.
				DocumentFormat.OpenXml.Drawing.Spreadsheet.GraphicFrame graphicFrame = twoCellAnchor.AppendChild(new DocumentFormat.OpenXml.Drawing.Spreadsheet.GraphicFrame());
				graphicFrame.Macro = "";

				graphicFrame.Append(new DocumentFormat.OpenXml.Drawing.Spreadsheet.NonVisualGraphicFrameProperties(
					new DocumentFormat.OpenXml.Drawing.Spreadsheet.NonVisualDrawingProperties() { Id = new UInt32Value(2u), Name = model.Name },
					new DocumentFormat.OpenXml.Drawing.Spreadsheet.NonVisualGraphicFrameDrawingProperties()));

				graphicFrame.Append(new Transform(new Offset() { X = 0L, Y = 0L }, new Extents() { Cx = 0L, Cy = 0L }));

				graphicFrame.Append(new Graphic(new GraphicData(new ChartReference() { Id = drawingsPart.GetIdOfPart(chartPart) }) { Uri = "http://schemas.openxmlformats.org/drawingml/2006/chart" }));

				twoCellAnchor.Append(new ClientData());

				// Save the WorksheetDrawing object.
				drawingsPart.WorksheetDrawing.Save();
			}
		}

		public static void InsertBarChart(ChartData model, string exportFile, int colIndex, ref int rowIndex, int width, int height)
		{
			// Open the document for editing.
			using (SpreadsheetDocument document = SpreadsheetDocument.Open(exportFile, true))
			{
				IEnumerable<Sheet> sheets = document.WorkbookPart.Workbook.Descendants<Sheet>().Where(s => s.Name == ExportConst.SHEET_NAME);
				if (sheets.Count() == 0) { return; }

				WorksheetPart worksheetPart = (WorksheetPart)document.WorkbookPart.GetPartById(sheets.First().Id);

				Worksheet worksheet = worksheetPart.Worksheet;
				SheetData sheetData = worksheet.Elements<SheetData>().FirstOrDefault();
				MergeCells mergeCells = worksheet.Elements<MergeCells>().FirstOrDefault();

				//Insert Chart Title
				ExcelDocument.MergeCellCustom(ref sheetData, ref mergeCells, ExcelDocument.GetColumnName(colIndex + 1) + rowIndex, ExcelDocument.GetColumnName(colIndex + 5) + rowIndex, model.Title, model.Format.RowIndex, (int)ExportConst.CellFontFormat.ChartTitle);

				DrawingsPart drawingsPart;
				if (worksheetPart.DrawingsPart != null)
				{
					drawingsPart = worksheetPart.DrawingsPart;
				}
				else
				{
					drawingsPart = worksheetPart.AddNewPart<DrawingsPart>();
					worksheetPart.Worksheet.Append(new Drawing() { Id = worksheetPart.GetIdOfPart(drawingsPart) });
					worksheetPart.Worksheet.Save();
				}

				// Add a new chart and set the chart language to English-US.
				ChartPart chartPart = drawingsPart.AddNewPart<ChartPart>();
				chartPart.ChartSpace = new ChartSpace();
				chartPart.ChartSpace.Append(new EditingLanguage() { Val = new StringValue("en-US") });
				DocumentFormat.OpenXml.Drawing.Charts.Chart chart = chartPart.ChartSpace.AppendChild(new DocumentFormat.OpenXml.Drawing.Charts.Chart());
				chart.Append(new AutoTitleDeleted() { Val = true });

				//Insert Chart Title
				//A.Run run = new A.Run();
				//A.RunProperties runProperties = new A.RunProperties() { Language = "en-US", FontSize = 12 };
				//A.Text text = new A.Text();
				//text.Text = model.Title;
				//run.Append(runProperties);
				//run.Append(text);
				//Paragraph paragraph = new Paragraph();
				//paragraph.Append(new ParagraphProperties());
				//paragraph.Append(run);
				//RichText richText = new RichText();
				//richText.Append(new BodyProperties());
				//richText.Append(new ListStyle());
				//richText.Append(paragraph);
				//ChartText chartText = new ChartText();
				//chartText.Append(richText);
				//C.Overlay overlay = new C.Overlay() { Val = false };
				//Title titleChart = new Title();
				//titleChart.Append(chartText);
				//titleChart.Append(new Layout());
				//titleChart.Append(overlay);
				//chart.Append(titleChart);

				// Create a new clustered column chart.
				PlotArea plotArea = chart.AppendChild<PlotArea>(new PlotArea());
				Layout layout = plotArea.AppendChild<Layout>(new Layout());

				//ThangPham, Add chart type to report, Feb 24 2016
				BarChart barChart = new BarChart(new BarDirection() { Val = new EnumValue<BarDirectionValues>(BarDirectionValues.Bar) },
					new BarGrouping() { Val = new EnumValue<BarGroupingValues>(BarGroupingValues.Clustered) });
				if (model.ChartType == (int)ExportConst.ChartType.ColumnChart)
				{
					barChart = new BarChart(new BarDirection() { Val = new EnumValue<BarDirectionValues>(BarDirectionValues.Column) },
					new BarGrouping() { Val = new EnumValue<BarGroupingValues>(BarGroupingValues.Clustered) });
				}

				plotArea.AppendChild<BarChart>(barChart);

				GapWidth gapwidth = new GapWidth() { Val = (UInt16Value)80U }; //default
				if (model.ChartDataItems.Count >= 2)
				{
					gapwidth = new GapWidth() { Val = (UInt16Value)225U }; //set small column series.
				}
				barChart.AppendChild(gapwidth);
				uint index = 0;

				//=================================================================================================================================================
				// Iterate through each key in the Dictionary collection and add the key to the chart Series
				// and add the corresponding value to the chart Values.
				BarChartSeries barChartSeries = barChart.AppendChild<BarChartSeries>(new BarChartSeries(new Index()
				{
					Val = new UInt32Value(index)
				},
				new Order() { Val = new UInt32Value(index) },
				new SeriesText(new NumericValue() { Text = "" })));

				var chartItems = model.ChartDataItems;

				StringLiteral strLit = barChartSeries.AppendChild<CategoryAxisData>(new CategoryAxisData()).AppendChild<StringLiteral>(new StringLiteral());
				strLit.Append(new PointCount() { Val = new UInt32Value(Convert.ToUInt32(chartItems.Count)) });

				DocumentFormat.OpenXml.Drawing.Charts.Values cVal = barChartSeries.AppendChild(new DocumentFormat.OpenXml.Drawing.Charts.Values());
				NumberLiteral numLit = cVal.AppendChild(new NumberLiteral());

				numLit.AppendChild<FormatCode>(new FormatCode() { Text = Utils.ExportConst.exceldataformat[model.ChartType] });
				numLit.AppendChild<PointCount>(new PointCount() { Val = new UInt32Value(Convert.ToUInt32(chartItems.Count)) });

				foreach (ChartDataItem chartItem in chartItems)
				{
					DataPoint dtPoint = barChartSeries.AppendChild<DataPoint>(new DataPoint());
					dtPoint.Append(new Index() { Val = new UInt32Value(Convert.ToUInt32(index)) });

					ChartShapeProperties cSP = dtPoint.AppendChild(new ChartShapeProperties());
					SolidFill slF = cSP.AppendChild(new SolidFill());
					PresetColor pc = slF.AppendChild(new PresetColor());
					switch (chartItem.Color)
					{
						case (int)ExportConst.ChartColor.Green:
							pc.Val = PresetColorValues.Green;
							break;
						case (int)ExportConst.ChartColor.Yellow:
							pc.Val = PresetColorValues.Yellow;
							break;
						case (int)ExportConst.ChartColor.Red:
							pc.Val = PresetColorValues.Red;
							break;
						case (int)ExportConst.ChartColor.Orange:
							pc.Val = PresetColorValues.Orange;
							break;
						case (int)ExportConst.ChartColor.Blue:
							pc.Val = PresetColorValues.Blue;
							break;
						default:
							pc.Val = PresetColorValues.Black;
							break;
					}

					StringPoint stPoint = strLit.AppendChild<StringPoint>(new StringPoint() { Index = new UInt32Value(Convert.ToUInt32(index)) });
					NumericValue numval = new NumericValue();
					numval.Text = chartItem.Name;
					stPoint.Append(numval);

					NumericPoint numPoint = numLit.AppendChild<NumericPoint>(new NumericPoint() { Index = new UInt32Value(Convert.ToUInt32(index)) });
					NumericValue cNumVal = new NumericValue();
					cNumVal.Text = chartItem.Value;
					numPoint.Append(cNumVal);

					index++;
				}

				Overlap ovl = new Overlap();
				ovl.Val = -30;
				barChart.AppendChild<Overlap>(ovl);

				DataLabels dataLabels = new DataLabels();
				DataLabelPosition dataLabelPosition = new DataLabelPosition() { Val = DataLabelPositionValues.OutsideEnd };
				ShowValue showValue1 = new ShowValue() { Val = true };
				dataLabels.Append(dataLabelPosition);
				dataLabels.Append(showValue1);

				dataLabels.Append(new ShowPercent() { Val = true });

				barChart.Append(dataLabels);

				//=================================================================================================================================================
				barChart.Append(new AxisId() { Val = new UInt32Value(48650112u) });
				barChart.Append(new AxisId() { Val = new UInt32Value(48672768u) });

				// Add the Category Axis.
				CategoryAxis catAx = plotArea.AppendChild<CategoryAxis>(new CategoryAxis(new AxisId() { Val = new UInt32Value(48650112u) }, new Scaling(new Orientation()
				{
					Val = new EnumValue<DocumentFormat.
						OpenXml.Drawing.Charts.OrientationValues>(DocumentFormat.OpenXml.Drawing.Charts.OrientationValues.MinMax)
				}),
					new AxisPosition() { Val = new EnumValue<AxisPositionValues>(AxisPositionValues.Bottom) },
					new TickLabelPosition() { Val = new EnumValue<TickLabelPositionValues>(TickLabelPositionValues.NextTo) },
					new CrossingAxis() { Val = new UInt32Value(48672768U) },
					new Crosses() { Val = new EnumValue<CrossesValues>(CrossesValues.AutoZero) },
					new AutoLabeled() { Val = new BooleanValue(true) },
					new LabelAlignment() { Val = new EnumValue<LabelAlignmentValues>(LabelAlignmentValues.Center) },
					new LabelOffset() { Val = new UInt16Value((ushort)100) }));

				// Add the Value Axis.
				ValueAxis valAx = plotArea.AppendChild<ValueAxis>(new ValueAxis(new AxisId() { Val = new UInt32Value(48672768u) },
					new Scaling(new Orientation()
					{
						Val = new EnumValue<DocumentFormat.OpenXml.Drawing.Charts.OrientationValues>(
							DocumentFormat.OpenXml.Drawing.Charts.OrientationValues.MinMax)
					}),
					new AxisPosition() { Val = new EnumValue<AxisPositionValues>(AxisPositionValues.Left) },

					new DocumentFormat.OpenXml.Drawing.Charts.NumberingFormat()
					{
						FormatCode = new StringValue("General"),
						SourceLinked = new BooleanValue(true)
					}, new TickLabelPosition()
					{
						Val = new EnumValue<TickLabelPositionValues>
							(TickLabelPositionValues.NextTo)
					}, new CrossingAxis() { Val = new UInt32Value(48650112U) },
					new Crosses() { Val = new EnumValue<CrossesValues>(CrossesValues.AutoZero) },
					new CrossBetween() { Val = new EnumValue<CrossBetweenValues>(CrossBetweenValues.Between) }));

				// Add the chart Legend.
				//Legend legend = chart.AppendChild<Legend>(new Legend(new LegendPosition() { Val = new EnumValue<LegendPositionValues>(LegendPositionValues.Right) },
				//    new Layout()));

				chart.Append(new PlotVisibleOnly() { Val = new BooleanValue(true) });

				// Save the chart part.
				chartPart.ChartSpace.Save();

				// Position the chart on the worksheet using a TwoCellAnchor object.
				if (drawingsPart.WorksheetDrawing == null)
				{
					drawingsPart.WorksheetDrawing = new WorksheetDrawing();
				}
				TwoCellAnchor twoCellAnchor = drawingsPart.WorksheetDrawing.AppendChild<TwoCellAnchor>(new TwoCellAnchor());
				twoCellAnchor.Append(new DocumentFormat.OpenXml.Drawing.Spreadsheet.FromMarker(new ColumnId(colIndex.ToString()),
					new ColumnOffset("0"),
					new RowId(rowIndex.ToString()),
					new RowOffset("0")));
				twoCellAnchor.Append(new DocumentFormat.OpenXml.Drawing.Spreadsheet.ToMarker(new ColumnId((colIndex + width).ToString()),
					new ColumnOffset("0"),
					new RowId((rowIndex + height).ToString()),
					new RowOffset("0")));

				// Append a GraphicFrame to the TwoCellAnchor object.
				DocumentFormat.OpenXml.Drawing.Spreadsheet.GraphicFrame graphicFrame = twoCellAnchor.AppendChild(new DocumentFormat.OpenXml.Drawing.Spreadsheet.GraphicFrame());
				graphicFrame.Macro = "";

				graphicFrame.Append(new DocumentFormat.OpenXml.Drawing.Spreadsheet.NonVisualGraphicFrameProperties(
					new DocumentFormat.OpenXml.Drawing.Spreadsheet.NonVisualDrawingProperties() { Id = new UInt32Value(2u), Name = model.Name },
					new DocumentFormat.OpenXml.Drawing.Spreadsheet.NonVisualGraphicFrameDrawingProperties()));

				graphicFrame.Append(new Transform(new Offset() { X = 0L, Y = 0L }, new Extents() { Cx = 0L, Cy = 0L }));
				graphicFrame.Append(new Graphic(new GraphicData(new ChartReference() { Id = drawingsPart.GetIdOfPart(chartPart) }) { Uri = "http://schemas.openxmlformats.org/drawingml/2006/chart" }));

				twoCellAnchor.Append(new ClientData());

				// Save the WorksheetDrawing object.
				drawingsPart.WorksheetDrawing.Save();
			}
		}

		public static void InsertLineChart(ChartData model, string exportFile, int colIndex, ref int rowIndex, int width, int height)
		{
			// Open the document for editing.
			using (SpreadsheetDocument document = SpreadsheetDocument.Open(exportFile, true))
			{
				IEnumerable<Sheet> sheets = document.WorkbookPart.Workbook.Descendants<Sheet>().Where(s => s.Name == ExportConst.SHEET_NAME);
				if (sheets.Count() == 0) { return; }

				WorksheetPart worksheetPart = (WorksheetPart)document.WorkbookPart.GetPartById(sheets.First().Id);

				//Insert Chart title
				Worksheet worksheet = worksheetPart.Worksheet;
				SheetData sheetData = worksheet.Elements<SheetData>().FirstOrDefault();
				MergeCells mergeCells = worksheet.Elements<MergeCells>().FirstOrDefault();
				ExcelDocument.MergeCellCustom(ref sheetData, ref mergeCells, ExcelDocument.GetColumnName(colIndex + 1) + rowIndex, ExcelDocument.GetColumnName(colIndex + 5) + rowIndex, model.Title, model.Format.RowIndex, (int)ExportConst.CellFontFormat.ChartTitle);

				DrawingsPart drawingsPart;
				if (worksheetPart.DrawingsPart != null)
				{
					drawingsPart = worksheetPart.DrawingsPart;
				}
				else
				{
					drawingsPart = worksheetPart.AddNewPart<DrawingsPart>();
					worksheetPart.Worksheet.Append(new Drawing() { Id = worksheetPart.GetIdOfPart(drawingsPart) });
					worksheetPart.Worksheet.Save();
				}

				// Add a new chart and set the chart language to English-US.
				ChartPart chartPart = drawingsPart.AddNewPart<ChartPart>();
				chartPart.ChartSpace = new ChartSpace();
				chartPart.ChartSpace.Append(new EditingLanguage() { Val = new StringValue("en-US") });
				DocumentFormat.OpenXml.Drawing.Charts.Chart chart = chartPart.ChartSpace.AppendChild(
					new DocumentFormat.OpenXml.Drawing.Charts.Chart());

				// Create a new clustered column chart.
				PlotArea plotArea = chart.AppendChild(new PlotArea());
				Layout layout = plotArea.AppendChild(new Layout());

				//ThangPham, Add the lineChart, Feb 24 2016
				plotArea.AppendChild(GenerateLineChart(model));

				// Add the Category Axis.
				plotArea.AppendChild(LineChartGenerateCategoryAxis());
				// Add the Value Axis.
				plotArea.AppendChild(LineChartGenerateValueAxis());


				// Add the chart Legend.
				Legend legend = chart.AppendChild(new Legend(new LegendPosition() { Val = new EnumValue<LegendPositionValues>(LegendPositionValues.Right) },
					new Layout()));

				chart.Append(new PlotVisibleOnly() { Val = new BooleanValue(true) });

				// Save the chart part.
				chartPart.ChartSpace.Save();

				// Position the chart on the worksheet using a TwoCellAnchor object.
				if (drawingsPart.WorksheetDrawing == null)
				{
					drawingsPart.WorksheetDrawing = new WorksheetDrawing();
				}
				TwoCellAnchor twoCellAnchor = drawingsPart.WorksheetDrawing.AppendChild(new TwoCellAnchor());
				twoCellAnchor.Append(new DocumentFormat.OpenXml.Drawing.Spreadsheet.FromMarker(new ColumnId(colIndex.ToString()),
					new ColumnOffset("0"),
					new RowId(rowIndex.ToString()),
					new RowOffset("0")));
				twoCellAnchor.Append(new DocumentFormat.OpenXml.Drawing.Spreadsheet.ToMarker(new ColumnId((colIndex + width).ToString()),
					new ColumnOffset("0"),
					new RowId((rowIndex + height).ToString()),
					new RowOffset("0")));

				// Append a GraphicFrame to the TwoCellAnchor object.
				DocumentFormat.OpenXml.Drawing.Spreadsheet.GraphicFrame graphicFrame = twoCellAnchor.AppendChild(new DocumentFormat.OpenXml.Drawing.Spreadsheet.GraphicFrame());
				graphicFrame.Macro = "";

				graphicFrame.Append(new DocumentFormat.OpenXml.Drawing.Spreadsheet.NonVisualGraphicFrameProperties(
					new DocumentFormat.OpenXml.Drawing.Spreadsheet.NonVisualDrawingProperties() { Id = new UInt32Value(2u), Name = model.Name },
					new DocumentFormat.OpenXml.Drawing.Spreadsheet.NonVisualGraphicFrameDrawingProperties()));

				graphicFrame.Append(new Transform(new Offset() { X = 0L, Y = 0L }, new Extents() { Cx = 0L, Cy = 0L }));

				graphicFrame.Append(new Graphic(new GraphicData(new ChartReference() { Id = drawingsPart.GetIdOfPart(chartPart) }) { Uri = "http://schemas.openxmlformats.org/drawingml/2006/chart" }));

				twoCellAnchor.Append(new ClientData());

				// Save the WorksheetDrawing object.
				drawingsPart.WorksheetDrawing.Save();
			}
		}

		public static void InsertDoughnutChart(ChartData model, string exportFile)
		{
			// Open the document for editing.
			using (SpreadsheetDocument document = SpreadsheetDocument.Open(exportFile, true))
			{
				IEnumerable<Sheet> sheets = document.WorkbookPart.Workbook.Descendants<Sheet>().Where(s => s.Name == ExportConst.SHEET_NAME);
				if (sheets.Count() == 0) { return; }

				WorksheetPart worksheetPart = (WorksheetPart)document.WorkbookPart.GetPartById(sheets.First().Id);

				//Insert Chart title
				Worksheet worksheet = worksheetPart.Worksheet;
				SheetData sheetData = worksheet.Elements<SheetData>().FirstOrDefault();
				MergeCells mergeCells = worksheet.Elements<MergeCells>().FirstOrDefault();
				ExcelDocument.MergeCellCustom(ref sheetData, ref mergeCells, ExcelDocument.GetColumnName(model.Format.ColIndex + 1) + model.Format.RowIndex, ExcelDocument.GetColumnName(model.Format.ColIndex + 5) + model.Format.RowIndex, model.Title, model.Format.RowIndex, (int)ExportConst.CellFontFormat.ChartTitle);

				DrawingsPart drawingsPart;
				if (worksheetPart.DrawingsPart != null)
				{
					drawingsPart = worksheetPart.DrawingsPart;
				}
				else
				{
					drawingsPart = worksheetPart.AddNewPart<DrawingsPart>();
					worksheetPart.Worksheet.Append(new Drawing() { Id = worksheetPart.GetIdOfPart(drawingsPart) });
					worksheetPart.Worksheet.Save();
				}

				// Add a new chart and set the chart language to English-US.
				ChartPart chartPart = drawingsPart.AddNewPart<ChartPart>();
				chartPart.ChartSpace = new ChartSpace();
				chartPart.ChartSpace.Append(new EditingLanguage() { Val = new StringValue("en-US") });


				chartPart.ChartSpace.Append(new Date1904() { Val = false });
				chartPart.ChartSpace.Append(new RoundedCorners() { Val = false });

				AlternateContent alternateContent = new AlternateContent();
				alternateContent.AddNamespaceDeclaration("mc", "http://schemas.openxmlformats.org/markup-compatibility/2006");
				AlternateContentChoice alternateContentChoice = new AlternateContentChoice() { Requires = "c14" };
				alternateContentChoice.AddNamespaceDeclaration("c14", "http://schemas.microsoft.com/office/drawing/2007/8/2/chart");
				DocumentFormat.OpenXml.Office2010.Drawing.Charts.Style style = new DocumentFormat.OpenXml.Office2010.Drawing.Charts.Style() { Val = 102 };
				alternateContentChoice.Append(style);
				alternateContent.Append(alternateContentChoice);
				AlternateContentFallback alternateContentFallback = new AlternateContentFallback();
				style = new DocumentFormat.OpenXml.Office2010.Drawing.Charts.Style() { Val = 2 };
				alternateContentFallback.Append(style);
				alternateContent.Append(alternateContentFallback);
				chartPart.ChartSpace.Append(alternateContent);

				//Add Chart - This is important
				DocumentFormat.OpenXml.Drawing.Charts.Chart chart = chartPart.ChartSpace.AppendChild(
						new DocumentFormat.OpenXml.Drawing.Charts.Chart());
				AutoTitleDeleted autoTitleDeleted = new AutoTitleDeleted() { Val = false };
				chart.Append(autoTitleDeleted);

				DocumentFormat.OpenXml.Drawing.ShapeProperties shapeProperties = new DocumentFormat.OpenXml.Drawing.ShapeProperties();
				SolidFill solidFill = new SolidFill();
				SchemeColor schemeColor = new SchemeColor() { Val = SchemeColorValues.Background1 };
				solidFill.Append(schemeColor);
				shapeProperties.Append(solidFill);
				DocumentFormat.OpenXml.Drawing.Outline outline = new DocumentFormat.OpenXml.Drawing.Outline()
				{
					Width = 9525,
					CapType = LineCapValues.Square,
					CompoundLineType = CompoundLineValues.Single,
					Alignment = PenAlignmentValues.Center
				};
				solidFill = new SolidFill();
				schemeColor = new SchemeColor() { Val = SchemeColorValues.Accent3 };
				LuminanceModulation luminanceModulation = new LuminanceModulation() { Val = 60000 };
				schemeColor.Append(luminanceModulation);
				LuminanceOffset luminanceOffset = new LuminanceOffset() { Val = 40000 };
				schemeColor.Append(luminanceOffset);
				solidFill.Append(schemeColor);
				outline.Append(solidFill);
				Miter miter = new Miter() { Limit = 800000 };
				outline.Append(miter);
				shapeProperties.Append(outline);
				EffectList effectList = new EffectList();
				shapeProperties.Append(effectList);
				chartPart.ChartSpace.Append(shapeProperties);

				DocumentFormat.OpenXml.Drawing.Charts.TextProperties textProperties = new DocumentFormat.OpenXml.Drawing.Charts.TextProperties();
				BodyProperties bodyProperties = new BodyProperties();
				textProperties.Append(bodyProperties);
				ListStyle listStyle = new ListStyle();
				textProperties.Append(listStyle);
				Paragraph paragraph = new Paragraph();
				ParagraphProperties paragraphProperties = new ParagraphProperties();
				DefaultRunProperties defaultRunProperties = new DefaultRunProperties();
				paragraphProperties.Append(defaultRunProperties);
				paragraph.Append(paragraphProperties);
				EndParagraphRunProperties endParagraphRunProperties = new EndParagraphRunProperties() { Language = "en-US" };
				paragraph.Append(endParagraphRunProperties);
				textProperties.Append(paragraph);
				chartPart.ChartSpace.Append(textProperties);

				PrintSettings printSettings = new PrintSettings();
				DocumentFormat.OpenXml.Drawing.Charts.HeaderFooter headerFooter = new DocumentFormat.OpenXml.Drawing.Charts.HeaderFooter();
				printSettings.Append(headerFooter);
				DocumentFormat.OpenXml.Drawing.Charts.PageMargins pageMargins = new DocumentFormat.OpenXml.Drawing.Charts.PageMargins() { Left = 0.7D, Right = 0.7D, Top = 0.75D, Bottom = 0.75D, Header = 0.3D, Footer = 0.3D };
				printSettings.Append(pageMargins);
				DocumentFormat.OpenXml.Drawing.Charts.PageSetup pageSetup = new DocumentFormat.OpenXml.Drawing.Charts.PageSetup();
				printSettings.Append(pageSetup);
				chartPart.ChartSpace.Append(printSettings);

				// Create a new clustered column chart.
				PlotArea plotArea = chart.AppendChild(new PlotArea());
				Layout layout = plotArea.AppendChild(new Layout());

				//ThangPham, Add the DoughnutChart, June 29 2016
				plotArea.AppendChild(GenerateDoughnutChart(model));

				//Add Shape properties
				shapeProperties = new DocumentFormat.OpenXml.Drawing.ShapeProperties();
				NoFill noFill = new NoFill();
				shapeProperties.Append(noFill);
				outline = new DocumentFormat.OpenXml.Drawing.Outline() { Width = 12700, CapType = LineCapValues.Round };
				noFill = new NoFill();
				outline.Append(noFill);
				shapeProperties.Append(outline);
				effectList = new EffectList();
				shapeProperties.Append(effectList);
				plotArea.Append(shapeProperties);

				//Add plot area to chart
				chart.Append(plotArea);

				PlotVisibleOnly plotVisibleOnly = new PlotVisibleOnly() { Val = true };
				chart.Append(plotVisibleOnly);
				DisplayBlanksAs displayBlanksAs = new DisplayBlanksAs() { Val = DisplayBlanksAsValues.Gap };
				chart.Append(displayBlanksAs);
				ShowDataLabelsOverMaximum showDataLabelsOverMaximum = new ShowDataLabelsOverMaximum() { Val = false };
				chart.Append(showDataLabelsOverMaximum);

				// Save the chart part.
				chartPart.ChartSpace.Save();

				// Position the chart on the worksheet using a TwoCellAnchor object.
				if (drawingsPart.WorksheetDrawing == null)
				{
					drawingsPart.WorksheetDrawing = new WorksheetDrawing();
				}
				TwoCellAnchor twoCellAnchor = drawingsPart.WorksheetDrawing.AppendChild(new TwoCellAnchor());
				twoCellAnchor.Append(new DocumentFormat.OpenXml.Drawing.Spreadsheet.FromMarker(new ColumnId(model.Format.ColIndex.ToString()),
					new ColumnOffset("0"),
					new RowId(model.Format.RowIndex.ToString()),
					new RowOffset("0")));
				twoCellAnchor.Append(new DocumentFormat.OpenXml.Drawing.Spreadsheet.ToMarker(new ColumnId((model.Format.ColIndex + model.Format.Width).ToString()),
					new ColumnOffset("0"),
					new RowId((model.Format.RowIndex + model.Format.Height).ToString()),
					new RowOffset("0")));

				// Append a GraphicFrame to the TwoCellAnchor object.
				DocumentFormat.OpenXml.Drawing.Spreadsheet.GraphicFrame graphicFrame = twoCellAnchor.AppendChild(new DocumentFormat.OpenXml.Drawing.Spreadsheet.GraphicFrame());
				graphicFrame.Macro = "";

				graphicFrame.Append(new DocumentFormat.OpenXml.Drawing.Spreadsheet.NonVisualGraphicFrameProperties(
					new DocumentFormat.OpenXml.Drawing.Spreadsheet.NonVisualDrawingProperties() { Id = new UInt32Value(2u), Name = "Chart 2" },
					new DocumentFormat.OpenXml.Drawing.Spreadsheet.NonVisualGraphicFrameDrawingProperties()));

				graphicFrame.Append(new Transform(new Offset() { X = 0L, Y = 0L }, new Extents() { Cx = 0L, Cy = 0L }));

				graphicFrame.Append(new Graphic(new GraphicData(new ChartReference() { Id = drawingsPart.GetIdOfPart(chartPart) }) { Uri = "http://schemas.openxmlformats.org/drawingml/2006/chart" }));

				twoCellAnchor.Append(new ClientData());

				// Save the WorksheetDrawing object.
				drawingsPart.WorksheetDrawing.Save();
			}
		}

		public static void InsertPlotAreaChart(ChartData model, string exportFile)
		{
			// Open the document for editing.
			using (SpreadsheetDocument document = SpreadsheetDocument.Open(exportFile, true))
			{
				IEnumerable<Sheet> sheets = document.WorkbookPart.Workbook.Descendants<Sheet>().Where(s => s.Name == ExportConst.SHEET_NAME);
				if (sheets.Count() == 0) { return; }

				WorksheetPart worksheetPart = (WorksheetPart)document.WorkbookPart.GetPartById(sheets.First().Id);

				//Insert Chart title
				Worksheet worksheet = worksheetPart.Worksheet;
				SheetData sheetData = worksheet.Elements<SheetData>().FirstOrDefault();
				MergeCells mergeCells = worksheet.Elements<MergeCells>().FirstOrDefault();
				ExcelDocument.MergeCellCustom(ref sheetData, ref mergeCells, ExcelDocument.GetColumnName(model.Format.ColIndex + 1) + model.Format.RowIndex, ExcelDocument.GetColumnName(model.Format.ColIndex + 5) + model.Format.RowIndex, model.Title, model.Format.RowIndex, (int)ExportConst.CellFontFormat.ChartTitle);

				DrawingsPart drawingsPart;
				if (worksheetPart.DrawingsPart != null)
				{
					drawingsPart = worksheetPart.DrawingsPart;
				}
				else
				{
					drawingsPart = worksheetPart.AddNewPart<DrawingsPart>();
					worksheetPart.Worksheet.Append(new Drawing() { Id = worksheetPart.GetIdOfPart(drawingsPart) });
					worksheetPart.Worksheet.Save();
				}

				// Add a new chart and set the chart language to English-US.
				ChartPart chartPart = drawingsPart.AddNewPart<ChartPart>();
				chartPart.ChartSpace = new ChartSpace();
				chartPart.ChartSpace.Append(new EditingLanguage() { Val = new StringValue("en-US") });


				chartPart.ChartSpace.Append(new Date1904() { Val = false });
				chartPart.ChartSpace.Append(new RoundedCorners() { Val = false });

				AlternateContent alternateContent = new AlternateContent();
				alternateContent.AddNamespaceDeclaration("mc", "http://schemas.openxmlformats.org/markup-compatibility/2006");
				AlternateContentChoice alternateContentChoice = new AlternateContentChoice() { Requires = "c14" };
				alternateContentChoice.AddNamespaceDeclaration("c14", "http://schemas.microsoft.com/office/drawing/2007/8/2/chart");
				DocumentFormat.OpenXml.Office2010.Drawing.Charts.Style style = new DocumentFormat.OpenXml.Office2010.Drawing.Charts.Style() { Val = 102 };
				alternateContentChoice.Append(style);
				alternateContent.Append(alternateContentChoice);
				AlternateContentFallback alternateContentFallback = new AlternateContentFallback();
				style = new DocumentFormat.OpenXml.Office2010.Drawing.Charts.Style() { Val = 2 };
				alternateContentFallback.Append(style);
				alternateContent.Append(alternateContentFallback);
				chartPart.ChartSpace.Append(alternateContent);

				//Add Chart - This is important
				DocumentFormat.OpenXml.Drawing.Charts.Chart chart = chartPart.ChartSpace.AppendChild(
						new DocumentFormat.OpenXml.Drawing.Charts.Chart());
				AutoTitleDeleted autoTitleDeleted = new AutoTitleDeleted() { Val = false };
				chart.Append(autoTitleDeleted);

				DocumentFormat.OpenXml.Drawing.ShapeProperties shapeProperties = new DocumentFormat.OpenXml.Drawing.ShapeProperties();
				SolidFill solidFill = new SolidFill();
				SchemeColor schemeColor = new SchemeColor() { Val = SchemeColorValues.Background1 };
				solidFill.Append(schemeColor);
				shapeProperties.Append(solidFill);
				DocumentFormat.OpenXml.Drawing.Outline outline = new DocumentFormat.OpenXml.Drawing.Outline()
				{
					Width = 9525,
					CapType = LineCapValues.Square,
					CompoundLineType = CompoundLineValues.Single,
					Alignment = PenAlignmentValues.Center
				};
				solidFill = new SolidFill();
				schemeColor = new SchemeColor() { Val = SchemeColorValues.Accent3 };
				LuminanceModulation luminanceModulation = new LuminanceModulation() { Val = 60000 };
				schemeColor.Append(luminanceModulation);
				LuminanceOffset luminanceOffset = new LuminanceOffset() { Val = 40000 };
				schemeColor.Append(luminanceOffset);
				solidFill.Append(schemeColor);
				outline.Append(solidFill);
				Miter miter = new Miter() { Limit = 800000 };
				outline.Append(miter);
				shapeProperties.Append(outline);
				EffectList effectList = new EffectList();
				shapeProperties.Append(effectList);
				chartPart.ChartSpace.Append(shapeProperties);

				DocumentFormat.OpenXml.Drawing.Charts.TextProperties textProperties = new DocumentFormat.OpenXml.Drawing.Charts.TextProperties();
				BodyProperties bodyProperties = new BodyProperties();
				textProperties.Append(bodyProperties);
				ListStyle listStyle = new ListStyle();
				textProperties.Append(listStyle);
				Paragraph paragraph = new Paragraph();
				ParagraphProperties paragraphProperties = new ParagraphProperties();
				DefaultRunProperties defaultRunProperties = new DefaultRunProperties();
				paragraphProperties.Append(defaultRunProperties);
				paragraph.Append(paragraphProperties);
				EndParagraphRunProperties endParagraphRunProperties = new EndParagraphRunProperties() { Language = "en-US" };
				paragraph.Append(endParagraphRunProperties);
				textProperties.Append(paragraph);
				chartPart.ChartSpace.Append(textProperties);

				PrintSettings printSettings = new PrintSettings();
				DocumentFormat.OpenXml.Drawing.Charts.HeaderFooter headerFooter = new DocumentFormat.OpenXml.Drawing.Charts.HeaderFooter();
				printSettings.Append(headerFooter);
				DocumentFormat.OpenXml.Drawing.Charts.PageMargins pageMargins = new DocumentFormat.OpenXml.Drawing.Charts.PageMargins() { Left = 0.7D, Right = 0.7D, Top = 0.75D, Bottom = 0.75D, Header = 0.3D, Footer = 0.3D };
				printSettings.Append(pageMargins);
				DocumentFormat.OpenXml.Drawing.Charts.PageSetup pageSetup = new DocumentFormat.OpenXml.Drawing.Charts.PageSetup();
				printSettings.Append(pageSetup);
				chartPart.ChartSpace.Append(printSettings);

				// Create a new clustered column chart.
				PlotArea plotArea = chart.AppendChild(new PlotArea());
				Layout layout = plotArea.AppendChild(new Layout());

				//ThangPham, Add the PlotArea, June 29 2016
				plotArea.AppendChild(GenerateDoughnutChart(model));

				//Add Shape properties
				shapeProperties = new DocumentFormat.OpenXml.Drawing.ShapeProperties();
				NoFill noFill = new NoFill();
				shapeProperties.Append(noFill);
				outline = new DocumentFormat.OpenXml.Drawing.Outline() { Width = 12700, CapType = LineCapValues.Round };
				noFill = new NoFill();
				outline.Append(noFill);
				shapeProperties.Append(outline);
				effectList = new EffectList();
				shapeProperties.Append(effectList);
				plotArea.Append(shapeProperties);

				//Add plot area to chart
				chart.Append(plotArea);

				PlotVisibleOnly plotVisibleOnly = new PlotVisibleOnly() { Val = true };
				chart.Append(plotVisibleOnly);
				DisplayBlanksAs displayBlanksAs = new DisplayBlanksAs() { Val = DisplayBlanksAsValues.Gap };
				chart.Append(displayBlanksAs);
				ShowDataLabelsOverMaximum showDataLabelsOverMaximum = new ShowDataLabelsOverMaximum() { Val = false };
				chart.Append(showDataLabelsOverMaximum);

				// Save the chart part.
				chartPart.ChartSpace.Save();

				// Position the chart on the worksheet using a TwoCellAnchor object.
				if (drawingsPart.WorksheetDrawing == null)
				{
					drawingsPart.WorksheetDrawing = new WorksheetDrawing();
				}
				TwoCellAnchor twoCellAnchor = drawingsPart.WorksheetDrawing.AppendChild(new TwoCellAnchor());
				twoCellAnchor.Append(new DocumentFormat.OpenXml.Drawing.Spreadsheet.FromMarker(new ColumnId(model.Format.ColIndex.ToString()),
					new ColumnOffset("0"),
					new RowId(model.Format.RowIndex.ToString()),
					new RowOffset("0")));
				twoCellAnchor.Append(new DocumentFormat.OpenXml.Drawing.Spreadsheet.ToMarker(new ColumnId((model.Format.ColIndex + model.Format.Width).ToString()),
					new ColumnOffset("0"),
					new RowId((model.Format.RowIndex + model.Format.Height).ToString()),
					new RowOffset("0")));

				// Append a GraphicFrame to the TwoCellAnchor object.
				DocumentFormat.OpenXml.Drawing.Spreadsheet.GraphicFrame graphicFrame = twoCellAnchor.AppendChild(new DocumentFormat.OpenXml.Drawing.Spreadsheet.GraphicFrame());
				graphicFrame.Macro = "";

				graphicFrame.Append(new DocumentFormat.OpenXml.Drawing.Spreadsheet.NonVisualGraphicFrameProperties(
					new DocumentFormat.OpenXml.Drawing.Spreadsheet.NonVisualDrawingProperties() { Id = new UInt32Value(2u), Name = "Chart 2" },
					new DocumentFormat.OpenXml.Drawing.Spreadsheet.NonVisualGraphicFrameDrawingProperties()));

				graphicFrame.Append(new Transform(new Offset() { X = 0L, Y = 0L }, new Extents() { Cx = 0L, Cy = 0L }));

				graphicFrame.Append(new Graphic(new GraphicData(new ChartReference() { Id = drawingsPart.GetIdOfPart(chartPart) }) { Uri = "http://schemas.openxmlformats.org/drawingml/2006/chart" }));

				twoCellAnchor.Append(new ClientData());

				// Save the WorksheetDrawing object.
				drawingsPart.WorksheetDrawing.Save();
			}
		}

		private static CategoryAxis LineChartGenerateCategoryAxis()
		{
			CategoryAxis categoryAxis = new CategoryAxis();
			AxisId axisId = new AxisId() { Val = (UInt32Value)89078016U };

			Scaling scaling = new Scaling();
			Orientation orientation = new Orientation() { Val = DocumentFormat.OpenXml.Drawing.Charts.OrientationValues.MinMax };

			scaling.Append(orientation);
			AxisPosition axisPosition = new AxisPosition() { Val = AxisPositionValues.Bottom };
			DocumentFormat.OpenXml.Drawing.Charts.NumberingFormat numberingFormat = new DocumentFormat.OpenXml.Drawing.Charts.NumberingFormat() { FormatCode = "h:mm", SourceLinked = true };
			TickLabelPosition tickLabelPosition = new TickLabelPosition() { Val = TickLabelPositionValues.NextTo };
			CrossingAxis crossingAxis = new CrossingAxis() { Val = (UInt32Value)89087360U };
			Crosses crosses = new Crosses() { Val = CrossesValues.AutoZero };
			AutoLabeled autoLabeled = new AutoLabeled() { Val = true };
			LabelAlignment labelAlignment = new LabelAlignment() { Val = LabelAlignmentValues.Center };
			LabelOffset labelOffset = new LabelOffset() { Val = (UInt16Value)100U };
			TickLabelSkip tickLabelSkip = new TickLabelSkip() { Val = 1 };

			categoryAxis.Append(axisId);
			categoryAxis.Append(scaling);
			categoryAxis.Append(axisPosition);
			categoryAxis.Append(numberingFormat);
			categoryAxis.Append(tickLabelPosition);
			categoryAxis.Append(crossingAxis);
			categoryAxis.Append(crosses);
			categoryAxis.Append(autoLabeled);
			categoryAxis.Append(labelAlignment);
			categoryAxis.Append(labelOffset);
			categoryAxis.Append(tickLabelSkip);
			return categoryAxis;
		}

		private static ValueAxis LineChartGenerateValueAxis()
		{
			ValueAxis valueAxis = new ValueAxis();
			AxisId axisId = new AxisId() { Val = (UInt32Value)89087360U };

			Scaling scaling = new Scaling();
			Orientation orientation = new Orientation() { Val = DocumentFormat.OpenXml.Drawing.Charts.OrientationValues.MinMax };

			scaling.Append(orientation);
			AxisPosition axisPosition = new AxisPosition() { Val = AxisPositionValues.Left };
			DocumentFormat.OpenXml.Drawing.Charts.NumberingFormat numberingFormat = new DocumentFormat.OpenXml.Drawing.Charts.NumberingFormat() { FormatCode = "General", SourceLinked = true };
			TickLabelPosition tickLabelPosition = new TickLabelPosition() { Val = TickLabelPositionValues.NextTo };
			CrossingAxis crossingAxis = new CrossingAxis() { Val = (UInt32Value)89078016U };
			Crosses crosses = new Crosses() { Val = CrossesValues.AutoZero };
			CrossBetween crossBetween = new CrossBetween() { Val = CrossBetweenValues.Between };

			valueAxis.Append(axisId);
			valueAxis.Append(scaling);
			valueAxis.Append(axisPosition);
			valueAxis.Append(numberingFormat);
			valueAxis.Append(tickLabelPosition);
			valueAxis.Append(crossingAxis);
			valueAxis.Append(crosses);
			valueAxis.Append(crossBetween);
			return valueAxis;
		}

		private static LineChart GenerateLineChart(ChartData model)
		{
			LineChart lineChart = new LineChart();
			Grouping grouping = new Grouping() { Val = GroupingValues.Standard };
			lineChart.Append(grouping);

			int counter = 0;
			foreach (ChartDataItem chartItem in model.ChartDataItems)
			{//loop to add lines to chart

				LineChartSeries lineChartSeries = new LineChartSeries();
				Index index = new Index() { Val = Convert.ToUInt32(counter) };
				Order order = new Order() { Val = Convert.ToUInt32(counter) };

				SeriesText seriesText = new SeriesText();
				NumericValue numericValue = new NumericValue();
				numericValue.Text = chartItem.Name;
				seriesText.Append(numericValue);

				DocumentFormat.OpenXml.Drawing.Charts.Values values = new DocumentFormat.OpenXml.Drawing.Charts.Values();
				NumberLiteral numberLiteral = new NumberLiteral();
				FormatCode formatCode = new FormatCode();
				formatCode.Text = "General";
				numberLiteral.Append(formatCode);

				string v = chartItem.Value;
				List<string> chartValues = new List<string>();
				if (!string.IsNullOrEmpty(v))
				{
					chartValues = v.Split('|').ToList();
				}

				uint indexCounter = 0;

				CategoryAxisData categoryAxisData = new CategoryAxisData();
				StringLiteral stringLiteral = new StringLiteral();
				PointCount pointCount = new PointCount() { Val = Convert.ToUInt32(chartValues.Count) };
				stringLiteral.Append(pointCount);
				pointCount = new PointCount() { Val = Convert.ToUInt32(chartValues.Count) };
				numberLiteral.Append(pointCount);

				foreach (string value in chartValues)
				{
					List<string> literalValues = new List<string>();
					if (!string.IsNullOrEmpty(value))
					{
						literalValues = value.Split(',').ToList(); //Ex: listeralValue = 08/20/2011,5804847
						if (!string.IsNullOrEmpty(literalValues[0]) && !string.IsNullOrEmpty(literalValues[1]))
						{
							StringPoint stringPoint = new StringPoint() { Index = Convert.ToUInt32(indexCounter) };
							NumericValue nv = new NumericValue();
							nv.Text = literalValues[0];
							stringPoint.Append(nv);
							stringLiteral.Append(stringPoint);

							NumericPoint numericPoint = new NumericPoint() { Index = Convert.ToUInt32(indexCounter) };
							nv = new NumericValue();
							nv.Text = literalValues[1];
							numericPoint.Append(nv);
							numberLiteral.Append(numericPoint);

							indexCounter++;
						}
					}
				}

				//======================================
				categoryAxisData.Append(stringLiteral);
				values.Append(numberLiteral);

				lineChartSeries.Append(index);
				lineChartSeries.Append(order);
				lineChartSeries.Append(seriesText);
				lineChartSeries.Append(categoryAxisData);
				lineChartSeries.Append(values);

				lineChart.Append(lineChartSeries);

				counter++;
			}

			ShowMarker showMarker1 = new ShowMarker() { Val = true };
			AxisId axisId1 = new AxisId() { Val = (UInt32Value)89078016U };
			AxisId axisId2 = new AxisId() { Val = (UInt32Value)89087360U };

			lineChart.Append(showMarker1);
			lineChart.Append(axisId1);
			lineChart.Append(axisId2);
			return lineChart;
		}

		private static DoughnutChart GenerateDoughnutChart(ChartData model)
		{
			DoughnutChart doughnutChart = new DoughnutChart();

			//Part 1 - Vary Colors
			VaryColors varyColors = new VaryColors() { Val = false };
			doughnutChart.Append(varyColors);

			//Part 2- Pie chart series
			PieChartSeries pieChartSeries = new PieChartSeries();

			Index index = new Index() { Val = (UInt32Value)0U };
			pieChartSeries.Append(index);

			Order order = new Order() { Val = (UInt32Value)0U };
			pieChartSeries.Append(order);

			ChartShapeProperties chartShapeProperties = new ChartShapeProperties();

			SolidFill solidFill = new SolidFill();
			SchemeColor schemeColor = new SchemeColor() { Val = SchemeColorValues.Accent1 };
			solidFill.Append(schemeColor);

			chartShapeProperties.Append(solidFill);

			DocumentFormat.OpenXml.Drawing.Outline outline = new DocumentFormat.OpenXml.Drawing.Outline() { Width = 19050 };
			solidFill = new SolidFill();
			schemeColor = new SchemeColor() { Val = SchemeColorValues.Light1 };
			solidFill.Append(schemeColor);
			outline.Append(solidFill);
			chartShapeProperties.Append(outline);

			EffectList effectList = new EffectList();
			chartShapeProperties.Append(effectList);

			pieChartSeries.Append(chartShapeProperties);

			DataPoint dataPoint = new DataPoint();

			index = new Index() { Val = (UInt32Value)0U };
			dataPoint.Append(index);

			Bubble3D bubble3D = new Bubble3D() { Val = false };
			dataPoint.Append(bubble3D);

			chartShapeProperties = new ChartShapeProperties();
			solidFill = new SolidFill();
			schemeColor = new SchemeColor() { Val = SchemeColorValues.Text1 };
			LuminanceModulation luminanceModulation = new LuminanceModulation() { Val = 50000 };
			schemeColor.Append(luminanceModulation);
			LuminanceOffset luminanceOffset = new LuminanceOffset() { Val = 50000 };
			schemeColor.Append(luminanceOffset);
			solidFill.Append(schemeColor);
			chartShapeProperties.Append(solidFill);

			outline = new DocumentFormat.OpenXml.Drawing.Outline() { Width = 12700 };
			solidFill = new SolidFill();
			schemeColor = new SchemeColor() { Val = SchemeColorValues.Background1 };
			solidFill.Append(schemeColor);
			outline.Append(solidFill);
			chartShapeProperties.Append(outline);

			effectList = new EffectList();
			chartShapeProperties.Append(effectList);
			dataPoint.Append(chartShapeProperties);
			pieChartSeries.Append(dataPoint);

			dataPoint = new DataPoint();
			index = new Index() { Val = (UInt32Value)1U };
			dataPoint.Append(index);

			bubble3D = new Bubble3D() { Val = false };
			dataPoint.Append(bubble3D);

			chartShapeProperties = new ChartShapeProperties();
			solidFill = new SolidFill();
			RgbColorModelHex rgbColorModelHex = new RgbColorModelHex() { Val = "FFC000" };
			solidFill.Append(rgbColorModelHex);
			chartShapeProperties.Append(solidFill);

			outline = new DocumentFormat.OpenXml.Drawing.Outline() { Width = 19050 };
			solidFill = new SolidFill();
			schemeColor = new SchemeColor() { Val = SchemeColorValues.Light1 };
			solidFill.Append(schemeColor);
			outline.Append(solidFill);
			chartShapeProperties.Append(outline);

			effectList = new EffectList();
			chartShapeProperties.Append(effectList);

			dataPoint.Append(chartShapeProperties);
			pieChartSeries.Append(dataPoint);

			DataLabels dataLabels = new DataLabels();

			DataLabel dataLabel = new DataLabel();

			index = new Index() { Val = (UInt32Value)0U };
			dataLabel.Append(index);

			Delete delete = new Delete() { Val = true };
			dataLabel.Append(delete);

			DLblExtensionList dLblExtensionList = new DLblExtensionList();

			DLblExtension dLblExtension = new DLblExtension() { Uri = "{CE6537A1-D6FC-4f65-9D91-7224C49458BB}" };
			dLblExtension.AddNamespaceDeclaration("c15", "http://schemas.microsoft.com/office/drawing/2012/chart");
			dLblExtensionList.Append(dLblExtension);
			dataLabel.Append(dLblExtensionList);

			dataLabels.Append(dataLabel);

			dataLabel = new DataLabel();

			index = new Index() { Val = (UInt32Value)1U };
			dataLabel.Append(index);

			Layout layout = new Layout();
			ManualLayout manualLayout = new ManualLayout();
			Left left = new Left() { Val = 0.30199731131169577D };
			Top top = new Top() { Val = 0.14246585587017277D };
			manualLayout.Append(left);
			manualLayout.Append(top);
			layout.Append(manualLayout);
			dataLabel.Append(layout);

			ChartText chartText = new ChartText();

			RichText richText = new RichText();
			BodyProperties bodyProperties = new BodyProperties()
			{
				Rotation = 0,
				UseParagraphSpacing = true,
				VerticalOverflow = TextVerticalOverflowValues.Clip,
				HorizontalOverflow = TextHorizontalOverflowValues.Clip,
				Vertical = TextVerticalValues.Horizontal,
				Wrap = TextWrappingValues.Square,
				LeftInset = 38100,
				TopInset = 19050,
				RightInset = 38100,
				BottomInset = 19050,
				Anchor = TextAnchoringTypeValues.Center,
				AnchorCenter = true
			};
			NoAutoFit noAutoFit = new NoAutoFit();
			bodyProperties.Append(noAutoFit);
			richText.Append(bodyProperties);

			ListStyle listStyle = new ListStyle();
			richText.Append(listStyle);

			Paragraph paragraph = new Paragraph();
			ParagraphProperties paragraphProperties = new ParagraphProperties();

			DefaultRunProperties defaultRunProperties = new DefaultRunProperties()
			{
				FontSize = 1100,
				Bold = true,
				Italic = false,
				Underline = TextUnderlineValues.None,
				Strike = TextStrikeValues.NoStrike,
				Kerning = 1200,
				Baseline = 0
			};

			solidFill = new SolidFill();

			schemeColor = new SchemeColor() { Val = SchemeColorValues.Dark1 };

			luminanceModulation = new LuminanceModulation() { Val = 65000 };
			schemeColor.Append(luminanceModulation);

			luminanceOffset = new LuminanceOffset() { Val = 35000 };
			schemeColor.Append(luminanceOffset);

			solidFill.Append(schemeColor);
			defaultRunProperties.Append(solidFill);

			LatinFont latinFont = new LatinFont() { Typeface = "+mn-lt" };
			defaultRunProperties.Append(latinFont);

			EastAsianFont eastAsianFont = new EastAsianFont() { Typeface = "+mn-ea" };
			defaultRunProperties.Append(eastAsianFont);

			ComplexScriptFont complexScriptFont = new ComplexScriptFont() { Typeface = "+mn-cs" };
			defaultRunProperties.Append(complexScriptFont);

			paragraphProperties.Append(defaultRunProperties);
			paragraph.Append(paragraphProperties);

			DocumentFormat.OpenXml.Drawing.Field field = new DocumentFormat.OpenXml.Drawing.Field() { Id = "{B5925DD6-69B2-40DC-A2DB-D4274C2522EA}", Type = "VALUE" };

			DocumentFormat.OpenXml.Drawing.RunProperties runProperties = new DocumentFormat.OpenXml.Drawing.RunProperties() { Language = "en-US", FontSize = 1100, Bold = true };
			field.Append(runProperties);

			paragraphProperties = new ParagraphProperties();

			defaultRunProperties = new DefaultRunProperties() { FontSize = 1100, Bold = true };
			paragraphProperties.Append(defaultRunProperties);
			field.Append(paragraphProperties);

			DocumentFormat.OpenXml.Drawing.Text text = new DocumentFormat.OpenXml.Drawing.Text();
			text.Text = "[VALUE]";
			field.Append(text);

			paragraph.Append(field);

			DocumentFormat.OpenXml.Drawing.Run run = new DocumentFormat.OpenXml.Drawing.Run();

			runProperties = new DocumentFormat.OpenXml.Drawing.RunProperties() { Language = "en-US", FontSize = 1100, Bold = true };
			run.Append(runProperties);

			text = new DocumentFormat.OpenXml.Drawing.Text();
			text.Text = "%";
			run.Append(text);

			paragraph.Append(run);
			richText.Append(paragraph);
			chartText.Append(richText);

			dataLabel.Append(chartText);

			chartShapeProperties = new ChartShapeProperties();

			solidFill = new SolidFill();
			SystemColor systemColor = new SystemColor() { Val = SystemColorValues.Window, LastColor = "FFFFFF" };
			solidFill.Append(systemColor);
			chartShapeProperties.Append(solidFill);

			outline = new DocumentFormat.OpenXml.Drawing.Outline();
			NoFill noFill = new NoFill();
			outline.Append(noFill);
			chartShapeProperties.Append(outline);

			effectList = new EffectList();
			chartShapeProperties.Append(effectList);

			dataLabel.Append(chartShapeProperties);

			DocumentFormat.OpenXml.Drawing.Charts.TextProperties textProperties = new DocumentFormat.OpenXml.Drawing.Charts.TextProperties();

			bodyProperties = new BodyProperties()
			{
				Rotation = 0,
				UseParagraphSpacing = true,
				VerticalOverflow = TextVerticalOverflowValues.Clip,
				HorizontalOverflow = TextHorizontalOverflowValues.Clip,
				Vertical = TextVerticalValues.Horizontal,
				Wrap = TextWrappingValues.Square,
				LeftInset = 38100,
				TopInset = 19050,
				RightInset = 38100,
				BottomInset = 19050,
				Anchor = TextAnchoringTypeValues.Center,
				AnchorCenter = true
			};

			noAutoFit = new NoAutoFit();
			bodyProperties.Append(noAutoFit);
			textProperties.Append(bodyProperties);

			listStyle = new ListStyle();
			textProperties.Append(listStyle);

			paragraph = new Paragraph();


			paragraphProperties = new ParagraphProperties();

			defaultRunProperties = new DefaultRunProperties()
			{
				FontSize = 1100,
				Bold = true,
				Italic = false,
				Underline = TextUnderlineValues.None,
				Strike = TextStrikeValues.NoStrike,
				Kerning = 1200,
				Baseline = 0
			};

			solidFill = new SolidFill();

			schemeColor = new SchemeColor() { Val = SchemeColorValues.Dark1 };

			luminanceModulation = new LuminanceModulation() { Val = 65000 };
			schemeColor.Append(luminanceModulation);

			luminanceOffset = new LuminanceOffset() { Val = 35000 };
			schemeColor.Append(luminanceOffset);

			solidFill.Append(schemeColor);
			defaultRunProperties.Append(solidFill);

			latinFont = new LatinFont() { Typeface = "+mn-lt" };
			defaultRunProperties.Append(latinFont);

			eastAsianFont = new EastAsianFont() { Typeface = "+mn-ea" };
			defaultRunProperties.Append(eastAsianFont);

			complexScriptFont = new ComplexScriptFont() { Typeface = "+mn-cs" };
			defaultRunProperties.Append(complexScriptFont);

			paragraphProperties.Append(defaultRunProperties);

			paragraph.Append(paragraphProperties);

			EndParagraphRunProperties endParagraphRunProperties = new EndParagraphRunProperties() { Language = "en-US" };
			paragraph.Append(endParagraphRunProperties);

			textProperties.Append(paragraph);

			dataLabel.Append(textProperties);

			ShowLegendKey showLegendKey = new ShowLegendKey() { Val = false };
			dataLabel.Append(showLegendKey);

			ShowValue showValue = new ShowValue() { Val = true };
			dataLabel.Append(showValue);

			ShowCategoryName showCategoryName = new ShowCategoryName() { Val = false };
			dataLabel.Append(showCategoryName);

			ShowSeriesName showSeriesName = new ShowSeriesName() { Val = false };
			dataLabel.Append(showSeriesName);

			ShowPercent showPercent = new ShowPercent() { Val = false };
			dataLabel.Append(showPercent);

			ShowBubbleSize showBubbleSize = new ShowBubbleSize() { Val = false };
			dataLabel.Append(showBubbleSize);

			dLblExtensionList = new DLblExtensionList();

			dLblExtension = new DLblExtension() { Uri = "{CE6537A1-D6FC-4f65-9D91-7224C49458BB}" };
			dLblExtension.AddNamespaceDeclaration("c15", "http://schemas.microsoft.com/office/drawing/2012/chart");

			DocumentFormat.OpenXml.Office2013.Drawing.Chart.ShapeProperties shapeProperties = new DocumentFormat.OpenXml.Office2013.Drawing.Chart.ShapeProperties();
			shapeProperties.AddNamespaceDeclaration("c15", "http://schemas.microsoft.com/office/drawing/2012/chart");

			PresetGeometry presetGeometry = new PresetGeometry() { Preset = ShapeTypeValues.Rectangle };
			AdjustValueList adjustValueList = new AdjustValueList();
			presetGeometry.Append(adjustValueList);
			shapeProperties.Append(presetGeometry);

			noFill = new NoFill();
			shapeProperties.Append(noFill);

			outline = new DocumentFormat.OpenXml.Drawing.Outline();
			noFill = new NoFill();
			outline.Append(noFill);
			shapeProperties.Append(outline);

			dLblExtension.Append(shapeProperties);

			DocumentFormat.OpenXml.Office2013.Drawing.Chart.Layout layoutOffice = new DocumentFormat.OpenXml.Office2013.Drawing.Chart.Layout();

			manualLayout = new ManualLayout();
			Width width = new Width() { Val = 0.45160553101594009D };
			manualLayout.Append(width);
			Height height = new Height() { Val = 0.27055569018494241D };
			manualLayout.Append(height);

			layoutOffice.Append(manualLayout);
			dLblExtension.Append(layoutOffice);

			DocumentFormat.OpenXml.Office2013.Drawing.Chart.DataLabelFieldTable dataLabelFieldTable = new DocumentFormat.OpenXml.Office2013.Drawing.Chart.DataLabelFieldTable();
			dLblExtension.Append(dataLabelFieldTable);

			DocumentFormat.OpenXml.Office2013.Drawing.Chart.ShowDataLabelsRange showDataLabelsRange = new DocumentFormat.OpenXml.Office2013.Drawing.Chart.ShowDataLabelsRange() { Val = false };
			dLblExtension.Append(showDataLabelsRange);

			dLblExtensionList.Append(dLblExtension);
			dataLabel.Append(dLblExtensionList);

			dataLabels.Append(dataLabel);

			chartShapeProperties = new ChartShapeProperties();

			solidFill = new SolidFill();
			systemColor = new SystemColor() { Val = SystemColorValues.Window, LastColor = "FFFFFF" };
			solidFill.Append(systemColor);
			chartShapeProperties.Append(solidFill);

			outline = new DocumentFormat.OpenXml.Drawing.Outline();
			noFill = new NoFill();
			outline.Append(noFill);
			chartShapeProperties.Append(outline);

			effectList = new EffectList();
			chartShapeProperties.Append(effectList);

			dataLabels.Append(chartShapeProperties);

			textProperties = new DocumentFormat.OpenXml.Drawing.Charts.TextProperties();

			bodyProperties = new BodyProperties()
			{
				Rotation = 0,
				UseParagraphSpacing = true,
				VerticalOverflow = TextVerticalOverflowValues.Clip,
				HorizontalOverflow = TextHorizontalOverflowValues.Clip,
				Vertical = TextVerticalValues.Horizontal,
				Wrap = TextWrappingValues.Square,
				LeftInset = 38100,
				TopInset = 19050,
				RightInset = 38100,
				BottomInset = 19050,
				Anchor = TextAnchoringTypeValues.Center,
				AnchorCenter = true
			};
			ShapeAutoFit shapeAutoFit = new ShapeAutoFit();
			bodyProperties.Append(shapeAutoFit);
			textProperties.Append(bodyProperties);

			listStyle = new ListStyle();
			textProperties.Append(listStyle);

			paragraph = new Paragraph();
			paragraphProperties = new ParagraphProperties();
			defaultRunProperties = new DefaultRunProperties()
			{
				FontSize = 1100,
				Bold = true,
				Italic = false,
				Underline = TextUnderlineValues.None,
				Strike = TextStrikeValues.NoStrike,
				Kerning = 1200,
				Baseline = 0
			};

			solidFill = new SolidFill();
			schemeColor = new SchemeColor() { Val = SchemeColorValues.Dark1 };
			luminanceModulation = new LuminanceModulation() { Val = 65000 };
			schemeColor.Append(luminanceModulation);
			luminanceOffset = new LuminanceOffset() { Val = 35000 };
			schemeColor.Append(luminanceOffset);

			solidFill.Append(schemeColor);
			defaultRunProperties.Append(solidFill);

			latinFont = new LatinFont() { Typeface = "+mn-lt" };
			defaultRunProperties.Append(latinFont);

			eastAsianFont = new EastAsianFont() { Typeface = "+mn-ea" };
			defaultRunProperties.Append(eastAsianFont);

			complexScriptFont = new ComplexScriptFont() { Typeface = "+mn-cs" };
			defaultRunProperties.Append(complexScriptFont);

			paragraphProperties.Append(defaultRunProperties);
			paragraph.Append(paragraphProperties);

			endParagraphRunProperties = new EndParagraphRunProperties() { Language = "en-US" };
			paragraph.Append(endParagraphRunProperties);

			textProperties.Append(paragraph);

			dataLabels.Append(textProperties);

			showLegendKey = new ShowLegendKey() { Val = false };
			dataLabels.Append(showLegendKey);

			showValue = new ShowValue() { Val = false };
			dataLabels.Append(showValue);

			showCategoryName = new ShowCategoryName() { Val = true };
			dataLabels.Append(showCategoryName);

			showSeriesName = new ShowSeriesName() { Val = false };
			dataLabels.Append(showSeriesName);

			showPercent = new ShowPercent() { Val = true };
			dataLabels.Append(showPercent);

			showBubbleSize = new ShowBubbleSize() { Val = false };
			dataLabels.Append(showBubbleSize);

			ShowLeaderLines showLeaderLines = new ShowLeaderLines() { Val = false };
			dataLabels.Append(showLeaderLines);

			pieChartSeries.Append(dataLabels);

			CategoryAxisData categoryAxisData = new CategoryAxisData();

			StringReference stringReference = new StringReference();

			DocumentFormat.OpenXml.Drawing.Charts.Formula formula = new DocumentFormat.OpenXml.Drawing.Charts.Formula();
			formula.Text = "Sheet1!$G$3:$G$4";
			stringReference.Append(formula);

			StringCache stringCache = new StringCache();

			PointCount pointCount = new PointCount() { Val = (UInt32Value)2U };
			stringCache.Append(pointCount);

			StringPoint stringPoint = new StringPoint() { Index = (UInt32Value)0U };
			NumericValue numericValue = new NumericValue();
			numericValue.Text = "Full";
			stringPoint.Append(numericValue);
			stringCache.Append(stringPoint);

			stringPoint = new StringPoint() { Index = (UInt32Value)1U };
			numericValue = new NumericValue();
			numericValue.Text = "Data";
			stringPoint.Append(numericValue);
			stringCache.Append(stringPoint);

			stringReference.Append(stringCache);
			categoryAxisData.Append(stringReference);
			pieChartSeries.Append(categoryAxisData);

			DocumentFormat.OpenXml.Drawing.Charts.Values values = new DocumentFormat.OpenXml.Drawing.Charts.Values();

			NumberReference numberReference = new NumberReference();

			formula = new DocumentFormat.OpenXml.Drawing.Charts.Formula();
			formula.Text = "Sheet1!$H$3:$H$4";
			numberReference.Append(formula);

			NumberingCache numberingCache = new NumberingCache();
			FormatCode formatCode = new FormatCode();
			formatCode.Text = "General";
			numberingCache.Append(formatCode);

			pointCount = new PointCount() { Val = (UInt32Value)2U };
			numberingCache.Append(pointCount);

			NumericPoint numericPoint = new NumericPoint() { Index = (UInt32Value)0U };

			numericValue = new NumericValue();
			numericValue.Text = "63.46";
			numericPoint.Append(numericValue);
			numberingCache.Append(numericPoint);

			numericPoint = new NumericPoint() { Index = (UInt32Value)1U };
			numericValue = new NumericValue();
			numericValue.Text = "36.54";
			numericPoint.Append(numericValue);
			numberingCache.Append(numericPoint);

			numberReference.Append(numberingCache);

			values.Append(numberReference);
			pieChartSeries.Append(values);

			doughnutChart.Append(pieChartSeries);

			//Part 3 - Data Label
			dataLabels = new DataLabels();

			showLegendKey = new ShowLegendKey() { Val = false };
			dataLabels.Append(showLegendKey);

			showValue = new ShowValue() { Val = false };
			dataLabels.Append(showValue);

			showCategoryName = new ShowCategoryName() { Val = false };
			dataLabels.Append(showCategoryName);

			showSeriesName = new ShowSeriesName() { Val = false };
			dataLabels.Append(showSeriesName);

			showPercent = new ShowPercent() { Val = false };
			dataLabels.Append(showPercent);

			showBubbleSize = new ShowBubbleSize() { Val = false };
			dataLabels.Append(showBubbleSize);

			showLeaderLines = new ShowLeaderLines() { Val = false };
			dataLabels.Append(showLeaderLines);

			doughnutChart.Append(dataLabels);

			//Part 4 - Slice Angle
			FirstSliceAngle firstSliceAngle = new FirstSliceAngle() { Val = (UInt16Value)0U };
			doughnutChart.Append(firstSliceAngle);

			//Part 5 - Hole Size
			HoleSize holeSize = new HoleSize() { Val = 75 };
			doughnutChart.Append(holeSize);

			return doughnutChart;
		}

		private static PlotArea GeneratePlotAreaChart(ChartData model)
		{
			PlotArea plotArea = new PlotArea();

			Layout layout = new Layout();

			ManualLayout manualLayout = new C.ManualLayout();
			LayoutTarget layoutTarget = new C.LayoutTarget() { Val = C.LayoutTargetValues.Inner };
			LeftMode leftMode = new C.LeftMode() { Val = C.LayoutModeValues.Edge };
			TopMode topMode = new C.TopMode() { Val = C.LayoutModeValues.Edge };
			Left left = new C.Left() { Val = 0.11764705882352941D };
			Top top = new C.Top() { Val = 0.10533468164592684D };
			Width width = new Width() { Val = 0.77389454994596263D };
			Height height = new Height() { Val = 0.94733265917703657D };

			manualLayout.Append(layoutTarget);
			manualLayout.Append(leftMode);
			manualLayout.Append(topMode);
			manualLayout.Append(left);
			manualLayout.Append(top);
			manualLayout.Append(width);
			manualLayout.Append(height);

			layout.Append(manualLayout);
			plotArea.Append(layout);

			C.DoughnutChart doughnutChart = new C.DoughnutChart();
			C.VaryColors varyColors = new C.VaryColors() { Val = false };
			doughnutChart.Append(varyColors);

			C.PieChartSeries pieChartSeries = new C.PieChartSeries();

			C.Index index = new C.Index() { Val = (UInt32Value)0U };
			pieChartSeries.Append(index);

			C.Order order = new C.Order() { Val = (UInt32Value)0U };
			pieChartSeries.Append(order);

			C.ChartShapeProperties chartShapeProperties = new C.ChartShapeProperties();
			A.SolidFill solidFill = new A.SolidFill();
			A.SchemeColor schemeColor = new A.SchemeColor() { Val = A.SchemeColorValues.Accent1 };
			solidFill.Append(schemeColor);
			chartShapeProperties.Append(solidFill);
			A.Outline outline = new A.Outline() { Width = 19050 };
			solidFill = new A.SolidFill();
			schemeColor = new A.SchemeColor() { Val = A.SchemeColorValues.Light1 };
			solidFill.Append(schemeColor);
			outline.Append(solidFill);
			chartShapeProperties.Append(outline);
			A.EffectList effectList = new A.EffectList();
			chartShapeProperties.Append(effectList);

			pieChartSeries.Append(chartShapeProperties);

			C.DataPoint dataPoint = new C.DataPoint();

			index = new C.Index() { Val = (UInt32Value)0U };
			dataPoint.Append(index);

			C.Bubble3D bubble3D = new C.Bubble3D() { Val = false };
			dataPoint.Append(bubble3D);

			chartShapeProperties = new C.ChartShapeProperties();

			solidFill = new A.SolidFill();
			schemeColor = new A.SchemeColor() { Val = A.SchemeColorValues.Text1 };
			A.LuminanceModulation luminanceModulation = new A.LuminanceModulation() { Val = 50000 };
			A.LuminanceOffset luminanceOffset = new A.LuminanceOffset() { Val = 50000 };
			schemeColor.Append(luminanceModulation);
			schemeColor.Append(luminanceOffset);
			solidFill.Append(schemeColor);
			chartShapeProperties.Append(solidFill);

			outline = new A.Outline() { Width = 12700 };
			solidFill = new A.SolidFill();
			schemeColor = new A.SchemeColor() { Val = A.SchemeColorValues.Background1 };
			solidFill.Append(schemeColor);
			outline.Append(solidFill);
			chartShapeProperties.Append(outline);
			effectList = new A.EffectList();
			chartShapeProperties.Append(effectList);

			dataPoint.Append(chartShapeProperties);

			pieChartSeries.Append(dataPoint);

			dataPoint = new C.DataPoint();

			index = new C.Index() { Val = (UInt32Value)1U };
			dataPoint.Append(index);

			bubble3D = new C.Bubble3D() { Val = false };
			dataPoint.Append(bubble3D);

			chartShapeProperties = new C.ChartShapeProperties();
			solidFill = new A.SolidFill();
			A.RgbColorModelHex rgbColorModelHex = new A.RgbColorModelHex() { Val = "FFC000" };
			solidFill.Append(rgbColorModelHex);
			chartShapeProperties.Append(solidFill);
			outline = new A.Outline() { Width = 19050 };
			solidFill = new A.SolidFill();
			schemeColor = new A.SchemeColor() { Val = A.SchemeColorValues.Light1 };
			solidFill.Append(schemeColor);
			outline.Append(solidFill);
			chartShapeProperties.Append(outline);
			effectList = new A.EffectList();
			chartShapeProperties.Append(effectList);

			dataPoint.Append(chartShapeProperties);
			pieChartSeries.Append(dataPoint);

			C.DataLabels dataLabels = new C.DataLabels();

			C.DataLabel dataLabel = new C.DataLabel();
			index = new C.Index() { Val = (UInt32Value)0U };
			dataLabel.Append(index);

			C.Delete delete = new C.Delete() { Val = true };
			dataLabel.Append(delete);

			C.DLblExtensionList dLblExtensionList = new C.DLblExtensionList();
			C.DLblExtension dLblExtension = new C.DLblExtension() { Uri = "{CE6537A1-D6FC-4f65-9D91-7224C49458BB}" };
			dLblExtension.AddNamespaceDeclaration("c15", "http://schemas.microsoft.com/office/drawing/2012/chart");
			dLblExtensionList.Append(dLblExtension);
			dataLabel.Append(dLblExtensionList);

			dataLabels.Append(dataLabel);

			dataLabel = new C.DataLabel();

			index = new C.Index() { Val = (UInt32Value)1U };
			dataLabel.Append(index);

			layout = new C.Layout();
			manualLayout = new C.ManualLayout();
			left = new C.Left() { Val = 0.30199731131169577D };
			top = new C.Top() { Val = 0.14246585587017277D };
			manualLayout.Append(left);
			manualLayout.Append(top);
			layout.Append(manualLayout);

			dataLabel.Append(layout);

			C.ChartText chartText = new C.ChartText();

			C.RichText richText = new C.RichText();

			A.BodyProperties bodyProperties = new A.BodyProperties() { Rotation = 0, UseParagraphSpacing = true, VerticalOverflow = A.TextVerticalOverflowValues.Clip, HorizontalOverflow = A.TextHorizontalOverflowValues.Clip, Vertical = A.TextVerticalValues.Horizontal, Wrap = A.TextWrappingValues.Square, LeftInset = 38100, TopInset = 19050, RightInset = 38100, BottomInset = 19050, Anchor = A.TextAnchoringTypeValues.Center, AnchorCenter = true };
			A.NoAutoFit noAutoFit = new A.NoAutoFit();
			bodyProperties.Append(noAutoFit);
			richText.Append(bodyProperties);

			A.ListStyle listStyle = new A.ListStyle();
			richText.Append(listStyle);

			A.Paragraph paragraph = new A.Paragraph();

			A.ParagraphProperties paragraphProperties = new A.ParagraphProperties();

			A.DefaultRunProperties defaultRunProperties = new A.DefaultRunProperties() { FontSize = 1100, Bold = true, Italic = false, Underline = A.TextUnderlineValues.None, Strike = A.TextStrikeValues.NoStrike, Kerning = 1200, Baseline = 0 };

			solidFill = new A.SolidFill();

			schemeColor = new A.SchemeColor() { Val = A.SchemeColorValues.Dark1 };
			luminanceModulation = new A.LuminanceModulation() { Val = 65000 };
			schemeColor.Append(luminanceModulation);
			luminanceOffset = new A.LuminanceOffset() { Val = 35000 };
			schemeColor.Append(luminanceOffset);

			solidFill.Append(schemeColor);

			defaultRunProperties.Append(solidFill);

			A.LatinFont latinFont = new A.LatinFont() { Typeface = "+mn-lt" };
			defaultRunProperties.Append(latinFont);

			A.EastAsianFont eastAsianFont = new A.EastAsianFont() { Typeface = "+mn-ea" };
			defaultRunProperties.Append(eastAsianFont);

			A.ComplexScriptFont complexScriptFont = new A.ComplexScriptFont() { Typeface = "+mn-cs" };
			defaultRunProperties.Append(complexScriptFont);

			paragraphProperties.Append(defaultRunProperties);

			paragraph.Append(paragraphProperties);

			A.Field field = new A.Field() { Id = "{B5925DD6-69B2-40DC-A2DB-D4274C2522EA}", Type = "VALUE" };
			A.RunProperties runProperties = new A.RunProperties() { Language = "en-US", FontSize = 1100, Bold = true };
			field.Append(runProperties);

			paragraphProperties = new A.ParagraphProperties();
			defaultRunProperties = new A.DefaultRunProperties() { FontSize = 1100, Bold = true };
			paragraphProperties.Append(defaultRunProperties);
			field.Append(paragraphProperties);
			A.Text text2 = new A.Text();
			text2.Text = "[VALUE]";

			field.Append(text2);

			paragraph.Append(field);

			A.Run run = new A.Run();
			runProperties = new A.RunProperties() { Language = "en-US", FontSize = 1100, Bold = true };
			run.Append(runProperties);
			A.Text text = new A.Text();
			text.Text = "%";
			run.Append(text);

			paragraph.Append(run);

			richText.Append(paragraph);

			chartText.Append(richText);

			dataLabel.Append(chartText);

			chartShapeProperties = new C.ChartShapeProperties();
			solidFill = new A.SolidFill();
			A.SystemColor systemColor = new A.SystemColor() { Val = A.SystemColorValues.Window, LastColor = "FFFFFF" };
			solidFill.Append(systemColor);
			chartShapeProperties.Append(solidFill);
			outline = new A.Outline();
			A.NoFill noFill = new A.NoFill();
			outline.Append(noFill);
			chartShapeProperties.Append(outline);
			effectList = new A.EffectList();

			chartShapeProperties.Append(effectList);

			dataLabel.Append(chartShapeProperties);

			C.TextProperties textProperties = new C.TextProperties();

			bodyProperties = new A.BodyProperties() { Rotation = 0, UseParagraphSpacing = true, VerticalOverflow = A.TextVerticalOverflowValues.Clip, HorizontalOverflow = A.TextHorizontalOverflowValues.Clip, Vertical = A.TextVerticalValues.Horizontal, Wrap = A.TextWrappingValues.Square, LeftInset = 38100, TopInset = 19050, RightInset = 38100, BottomInset = 19050, Anchor = A.TextAnchoringTypeValues.Center, AnchorCenter = true };
			noAutoFit = new A.NoAutoFit();

			bodyProperties.Append(noAutoFit);

			textProperties.Append(bodyProperties);

			listStyle = new A.ListStyle();
			textProperties.Append(listStyle);

			paragraph = new A.Paragraph();
			paragraphProperties = new A.ParagraphProperties();
			defaultRunProperties = new A.DefaultRunProperties() { FontSize = 1100, Bold = true, Italic = false, Underline = A.TextUnderlineValues.None, Strike = A.TextStrikeValues.NoStrike, Kerning = 1200, Baseline = 0 };
			solidFill = new A.SolidFill();
			schemeColor = new A.SchemeColor() { Val = A.SchemeColorValues.Dark1 };
			luminanceModulation = new A.LuminanceModulation() { Val = 65000 };
			luminanceOffset = new A.LuminanceOffset() { Val = 35000 };
			schemeColor.Append(luminanceModulation);
			schemeColor.Append(luminanceOffset);
			solidFill.Append(schemeColor);

			defaultRunProperties.Append(solidFill);

			latinFont = new A.LatinFont() { Typeface = "+mn-lt" };
			defaultRunProperties.Append(latinFont);

			eastAsianFont = new A.EastAsianFont() { Typeface = "+mn-ea" };
			defaultRunProperties.Append(eastAsianFont);

			complexScriptFont = new A.ComplexScriptFont() { Typeface = "+mn-cs" };
			defaultRunProperties.Append(complexScriptFont);

			paragraphProperties.Append(defaultRunProperties);
			paragraph.Append(paragraphProperties);

			A.EndParagraphRunProperties endParagraphRunProperties = new A.EndParagraphRunProperties() { Language = "en-US" };
			paragraph.Append(endParagraphRunProperties);

			textProperties.Append(paragraph);

			dataLabel.Append(textProperties);

			C.ShowLegendKey showLegendKey = new C.ShowLegendKey() { Val = false };
			C.ShowValue showValue = new C.ShowValue() { Val = true };
			C.ShowCategoryName showCategoryName = new C.ShowCategoryName() { Val = false };
			C.ShowSeriesName showSeriesName = new C.ShowSeriesName() { Val = false };
			C.ShowPercent showPercent = new C.ShowPercent() { Val = false };
			C.ShowBubbleSize showBubbleSize = new C.ShowBubbleSize() { Val = false };

			dataLabel.Append(showLegendKey);
			dataLabel.Append(showValue);
			dataLabel.Append(showCategoryName);
			dataLabel.Append(showSeriesName);
			dataLabel.Append(showPercent);
			dataLabel.Append(showBubbleSize);

			dLblExtensionList = new C.DLblExtensionList();
			dLblExtension = new C.DLblExtension() { Uri = "{CE6537A1-D6FC-4f65-9D91-7224C49458BB}" };
			dLblExtension.AddNamespaceDeclaration("c15", "http://schemas.microsoft.com/office/drawing/2012/chart");

			C15.ShapeProperties shapePropertiesC15 = new C15.ShapeProperties();
			shapePropertiesC15.AddNamespaceDeclaration("c15", "http://schemas.microsoft.com/office/drawing/2012/chart");

			A.PresetGeometry presetGeometry = new A.PresetGeometry() { Preset = A.ShapeTypeValues.Rectangle };
			A.AdjustValueList adjustValueList = new A.AdjustValueList();
			presetGeometry.Append(adjustValueList);
			shapePropertiesC15.Append(presetGeometry);

			noFill = new A.NoFill();
			shapePropertiesC15.Append(noFill);

			outline = new A.Outline();
			noFill = new A.NoFill();
			outline.Append(noFill);

			shapePropertiesC15.Append(outline);

			dLblExtension.Append(shapePropertiesC15);

			C15.Layout layoutOffice = new C15.Layout();
			manualLayout = new C.ManualLayout();
			width = new C.Width() { Val = 0.45160553101594009D };
			height = new C.Height() { Val = 0.27055569018494241D };
			manualLayout.Append(width);
			manualLayout.Append(height);

			layoutOffice.Append(manualLayout);

			dLblExtension.Append(layoutOffice);

			C15.DataLabelFieldTable dataLabelFieldTable = new C15.DataLabelFieldTable();
			C15.ShowDataLabelsRange showDataLabelsRange = new C15.ShowDataLabelsRange() { Val = false };

			dLblExtension.Append(dataLabelFieldTable);
			dLblExtension.Append(showDataLabelsRange);

			dLblExtensionList.Append(dLblExtension);

			dataLabel.Append(dLblExtensionList);

			dataLabels.Append(dataLabel);

			chartShapeProperties = new C.ChartShapeProperties();
			solidFill = new A.SolidFill();
			systemColor = new A.SystemColor() { Val = A.SystemColorValues.Window, LastColor = "FFFFFF" };
			solidFill.Append(systemColor);
			chartShapeProperties.Append(solidFill);
			outline = new A.Outline();
			noFill = new A.NoFill();
			outline.Append(noFill);
			chartShapeProperties.Append(outline);
			effectList = new A.EffectList();
			chartShapeProperties.Append(effectList);

			dataLabels.Append(chartShapeProperties);

			textProperties = new C.TextProperties();
			bodyProperties = new A.BodyProperties() { Rotation = 0, UseParagraphSpacing = true, VerticalOverflow = A.TextVerticalOverflowValues.Clip, HorizontalOverflow = A.TextHorizontalOverflowValues.Clip, Vertical = A.TextVerticalValues.Horizontal, Wrap = A.TextWrappingValues.Square, LeftInset = 38100, TopInset = 19050, RightInset = 38100, BottomInset = 19050, Anchor = A.TextAnchoringTypeValues.Center, AnchorCenter = true };
			A.ShapeAutoFit shapeAutoFit = new A.ShapeAutoFit();
			bodyProperties.Append(shapeAutoFit);
			textProperties.Append(bodyProperties);

			listStyle = new A.ListStyle();
			textProperties.Append(listStyle);

			paragraph = new A.Paragraph();
			paragraphProperties = new A.ParagraphProperties();
			defaultRunProperties = new A.DefaultRunProperties() { FontSize = 1100, Bold = true, Italic = false, Underline = A.TextUnderlineValues.None, Strike = A.TextStrikeValues.NoStrike, Kerning = 1200, Baseline = 0 };
			solidFill = new A.SolidFill();
			schemeColor = new A.SchemeColor() { Val = A.SchemeColorValues.Dark1 };
			luminanceModulation = new A.LuminanceModulation() { Val = 65000 };
			luminanceOffset = new A.LuminanceOffset() { Val = 35000 };
			schemeColor.Append(luminanceModulation);
			schemeColor.Append(luminanceOffset);
			solidFill.Append(schemeColor);

			defaultRunProperties.Append(solidFill);

			latinFont = new A.LatinFont() { Typeface = "+mn-lt" };
			eastAsianFont = new A.EastAsianFont() { Typeface = "+mn-ea" };
			complexScriptFont = new A.ComplexScriptFont() { Typeface = "+mn-cs" };
			defaultRunProperties.Append(latinFont);
			defaultRunProperties.Append(eastAsianFont);
			defaultRunProperties.Append(complexScriptFont);

			paragraphProperties.Append(defaultRunProperties);
			paragraph.Append(paragraphProperties);

			endParagraphRunProperties = new A.EndParagraphRunProperties() { Language = "en-US" };
			paragraph.Append(endParagraphRunProperties);

			textProperties.Append(paragraph);

			dataLabels.Append(textProperties);

			showLegendKey = new C.ShowLegendKey() { Val = false };
			showValue = new C.ShowValue() { Val = false };
			showCategoryName = new C.ShowCategoryName() { Val = true };
			showSeriesName = new C.ShowSeriesName() { Val = false };
			showPercent = new C.ShowPercent() { Val = true };
			showBubbleSize = new C.ShowBubbleSize() { Val = false };
			C.ShowLeaderLines showLeaderLines = new C.ShowLeaderLines() { Val = false };
			dataLabels.Append(showLegendKey);
			dataLabels.Append(showValue);
			dataLabels.Append(showCategoryName);
			dataLabels.Append(showSeriesName);
			dataLabels.Append(showPercent);
			dataLabels.Append(showBubbleSize);
			dataLabels.Append(showLeaderLines);

			C.DLblsExtensionList dLblsExtensionList = new C.DLblsExtensionList();
			C.DLblsExtension dLblsExtension = new C.DLblsExtension() { Uri = "{CE6537A1-D6FC-4f65-9D91-7224C49458BB}" };
			dLblsExtension.AddNamespaceDeclaration("c15", "http://schemas.microsoft.com/office/drawing/2012/chart");

			C15.ShapeProperties shapeProperties = new C15.ShapeProperties();
			shapeProperties.AddNamespaceDeclaration("c15", "http://schemas.microsoft.com/office/drawing/2012/chart");

			presetGeometry = new A.PresetGeometry() { Preset = A.ShapeTypeValues.Rectangle };
			adjustValueList = new A.AdjustValueList();
			presetGeometry.Append(adjustValueList);
			shapeProperties.Append(presetGeometry);

			noFill = new A.NoFill();
			shapeProperties.Append(noFill);

			outline = new A.Outline();
			noFill = new A.NoFill();
			outline.Append(noFill);
			shapeProperties.Append(outline);

			dLblsExtension.Append(shapeProperties);

			dLblsExtensionList.Append(dLblsExtension);

			dataLabels.Append(dLblsExtensionList);

			pieChartSeries.Append(dataLabels);

			C.CategoryAxisData categoryAxisData = new C.CategoryAxisData();

			C.StringReference stringReference = new C.StringReference();
			C.Formula formula = new C.Formula();
			formula.Text = "Sheet1!$G$3:$G$4";

			stringReference.Append(formula);

			C.StringCache stringCache = new C.StringCache();
			C.PointCount pointCount = new C.PointCount() { Val = (UInt32Value)2U };
			stringCache.Append(pointCount);

			C.StringPoint stringPoint = new C.StringPoint() { Index = (UInt32Value)0U };
			C.NumericValue numericValue = new C.NumericValue();
			numericValue.Text = "Full";
			stringPoint.Append(numericValue);
			stringCache.Append(stringPoint);

			stringPoint = new C.StringPoint() { Index = (UInt32Value)1U };
			numericValue = new C.NumericValue();
			numericValue.Text = "Data";
			stringPoint.Append(numericValue);

			stringCache.Append(stringPoint);

			stringReference.Append(stringCache);

			categoryAxisData.Append(stringReference);

			pieChartSeries.Append(categoryAxisData);

			C.Values values = new C.Values();
			C.NumberReference numberReference = new C.NumberReference();
			formula = new C.Formula();
			formula.Text = "Sheet1!$H$3:$H$4";

			numberReference.Append(formula);

			C.NumberingCache numberingCache = new C.NumberingCache();
			C.FormatCode formatCode = new C.FormatCode();
			formatCode.Text = "General";
			numberingCache.Append(formatCode);

			pointCount = new C.PointCount() { Val = (UInt32Value)2U };
			numberingCache.Append(pointCount);

			C.NumericPoint numericPoint = new C.NumericPoint() { Index = (UInt32Value)0U };
			numericValue = new C.NumericValue();
			numericValue.Text = "63.46";
			numericPoint.Append(numericValue);
			numberingCache.Append(numericPoint);

			numericPoint = new C.NumericPoint() { Index = (UInt32Value)1U };
			numericValue = new C.NumericValue();
			numericValue.Text = "36.54";
			numericPoint.Append(numericValue);

			numberingCache.Append(numericPoint);

			numberReference.Append(numberingCache);

			values.Append(numberReference);

			pieChartSeries.Append(values);

			doughnutChart.Append(pieChartSeries);

			dataLabels = new C.DataLabels();
			showLegendKey = new C.ShowLegendKey() { Val = false };
			showValue = new C.ShowValue() { Val = false };
			showCategoryName = new C.ShowCategoryName() { Val = false };
			showSeriesName = new C.ShowSeriesName() { Val = false };
			showPercent = new C.ShowPercent() { Val = false };
			showBubbleSize = new C.ShowBubbleSize() { Val = false };
			showLeaderLines = new C.ShowLeaderLines() { Val = false };
			dataLabels.Append(showLegendKey);
			dataLabels.Append(showValue);
			dataLabels.Append(showCategoryName);
			dataLabels.Append(showSeriesName);
			dataLabels.Append(showPercent);
			dataLabels.Append(showBubbleSize);
			dataLabels.Append(showLeaderLines);

			doughnutChart.Append(dataLabels);

			C.FirstSliceAngle firstSliceAngle = new C.FirstSliceAngle() { Val = (UInt16Value)0U };
			C.HoleSize holeSize = new C.HoleSize() { Val = 75 };
			doughnutChart.Append(firstSliceAngle);
			doughnutChart.Append(holeSize);

			plotArea.Append(doughnutChart);

			C.ShapeProperties shapeProperties1 = new C.ShapeProperties();
			noFill = new A.NoFill();
			shapeProperties1.Append(noFill);

			outline = new A.Outline() { Width = 12700, CapType = A.LineCapValues.Round };
			noFill = new A.NoFill();
			outline.Append(noFill);
			shapeProperties1.Append(outline);
			effectList = new A.EffectList();
			shapeProperties1.Append(effectList);

			plotArea.Append(shapeProperties1);

			return plotArea;
		}

		public void GenerateChartPart1Content(ChartPart chartPart1)
		{
			C.ChartSpace chartSpace1 = new C.ChartSpace();
			chartSpace1.AddNamespaceDeclaration("c", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			chartSpace1.AddNamespaceDeclaration("a", "http://schemas.openxmlformats.org/drawingml/2006/main");
			chartSpace1.AddNamespaceDeclaration("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
			C.Date1904 date19041 = new C.Date1904() { Val = false };
			C.EditingLanguage editingLanguage1 = new C.EditingLanguage() { Val = "en-US" };
			C.RoundedCorners roundedCorners1 = new C.RoundedCorners() { Val = false };

			AlternateContent alternateContent1 = new AlternateContent();
			alternateContent1.AddNamespaceDeclaration("mc", "http://schemas.openxmlformats.org/markup-compatibility/2006");

			AlternateContentChoice alternateContentChoice1 = new AlternateContentChoice() { Requires = "c14" };
			alternateContentChoice1.AddNamespaceDeclaration("c14", "http://schemas.microsoft.com/office/drawing/2007/8/2/chart");
			C14.Style style1 = new C14.Style() { Val = 102 };

			alternateContentChoice1.Append(style1);

			AlternateContentFallback alternateContentFallback1 = new AlternateContentFallback();
			C.Style style2 = new C.Style() { Val = 2 };

			alternateContentFallback1.Append(style2);

			alternateContent1.Append(alternateContentChoice1);
			alternateContent1.Append(alternateContentFallback1);

			C.Chart chart1 = new C.Chart();
			C.AutoTitleDeleted autoTitleDeleted1 = new C.AutoTitleDeleted() { Val = true };

			C.PlotArea plotArea1 = new C.PlotArea();
			C.Layout layout1 = new C.Layout();

			C.BarChart barChart1 = new C.BarChart();
			C.BarDirection barDirection1 = new C.BarDirection() { Val = C.BarDirectionValues.Column };
			C.BarGrouping barGrouping1 = new C.BarGrouping() { Val = C.BarGroupingValues.Clustered };
			C.VaryColors varyColors1 = new C.VaryColors() { Val = false };

			C.BarChartSeries barChartSeries1 = new C.BarChartSeries();
			C.Index index1 = new C.Index() { Val = (UInt32Value)0U };
			C.Order order1 = new C.Order() { Val = (UInt32Value)0U };

			C.SeriesText seriesText1 = new C.SeriesText();

			C.StringReference stringReference1 = new C.StringReference();
			C.Formula formula1 = new C.Formula();
			formula1.Text = "Sheet1!$B$1:$B$2";

			C.StringCache stringCache1 = new C.StringCache();
			C.PointCount pointCount1 = new C.PointCount() { Val = (UInt32Value)2U };

			C.StringPoint stringPoint1 = new C.StringPoint() { Index = (UInt32Value)0U };
			C.NumericValue numericValue1 = new C.NumericValue();
			numericValue1.Text = "I3DVR";

			stringPoint1.Append(numericValue1);

			C.StringPoint stringPoint2 = new C.StringPoint() { Index = (UInt32Value)1U };
			C.NumericValue numericValue2 = new C.NumericValue();
			numericValue2.Text = "Count";

			stringPoint2.Append(numericValue2);

			stringCache1.Append(pointCount1);
			stringCache1.Append(stringPoint1);
			stringCache1.Append(stringPoint2);

			stringReference1.Append(formula1);
			stringReference1.Append(stringCache1);

			seriesText1.Append(stringReference1);

			C.ChartShapeProperties chartShapeProperties1 = new C.ChartShapeProperties();

			A.SolidFill solidFill6 = new A.SolidFill();
			A.RgbColorModelHex rgbColorModelHex14 = new A.RgbColorModelHex() { Val = "0070C0" };

			solidFill6.Append(rgbColorModelHex14);

			A.Outline outline4 = new A.Outline();
			A.NoFill noFill1 = new A.NoFill();

			outline4.Append(noFill1);
			A.EffectList effectList4 = new A.EffectList();

			chartShapeProperties1.Append(solidFill6);
			chartShapeProperties1.Append(outline4);
			chartShapeProperties1.Append(effectList4);
			C.InvertIfNegative invertIfNegative1 = new C.InvertIfNegative() { Val = false };

			C.CategoryAxisData categoryAxisData1 = new C.CategoryAxisData();

			C.NumberReference numberReference1 = new C.NumberReference();
			C.Formula formula2 = new C.Formula();
			formula2.Text = "Sheet1!$A$3:$A$6";

			C.NumberingCache numberingCache1 = new C.NumberingCache();
			C.FormatCode formatCode1 = new C.FormatCode();
			formatCode1.Text = "m/d/yyyy";
			C.PointCount pointCount2 = new C.PointCount() { Val = (UInt32Value)4U };

			C.NumericPoint numericPoint1 = new C.NumericPoint() { Index = (UInt32Value)0U };
			C.NumericValue numericValue3 = new C.NumericValue();
			numericValue3.Text = "42372";

			numericPoint1.Append(numericValue3);

			C.NumericPoint numericPoint2 = new C.NumericPoint() { Index = (UInt32Value)1U };
			C.NumericValue numericValue4 = new C.NumericValue();
			numericValue4.Text = "42373";

			numericPoint2.Append(numericValue4);

			C.NumericPoint numericPoint3 = new C.NumericPoint() { Index = (UInt32Value)2U };
			C.NumericValue numericValue5 = new C.NumericValue();
			numericValue5.Text = "42374";

			numericPoint3.Append(numericValue5);

			C.NumericPoint numericPoint4 = new C.NumericPoint() { Index = (UInt32Value)3U };
			C.NumericValue numericValue6 = new C.NumericValue();
			numericValue6.Text = "42375";

			numericPoint4.Append(numericValue6);

			numberingCache1.Append(formatCode1);
			numberingCache1.Append(pointCount2);
			numberingCache1.Append(numericPoint1);
			numberingCache1.Append(numericPoint2);
			numberingCache1.Append(numericPoint3);
			numberingCache1.Append(numericPoint4);

			numberReference1.Append(formula2);
			numberReference1.Append(numberingCache1);

			categoryAxisData1.Append(numberReference1);

			C.Values values1 = new C.Values();

			C.NumberReference numberReference2 = new C.NumberReference();
			C.Formula formula3 = new C.Formula();
			formula3.Text = "Sheet1!$B$3:$B$6";

			C.NumberingCache numberingCache2 = new C.NumberingCache();
			C.FormatCode formatCode2 = new C.FormatCode();
			formatCode2.Text = "General";
			C.PointCount pointCount3 = new C.PointCount() { Val = (UInt32Value)4U };

			C.NumericPoint numericPoint5 = new C.NumericPoint() { Index = (UInt32Value)0U };
			C.NumericValue numericValue7 = new C.NumericValue();
			numericValue7.Text = "45";

			numericPoint5.Append(numericValue7);

			C.NumericPoint numericPoint6 = new C.NumericPoint() { Index = (UInt32Value)1U };
			C.NumericValue numericValue8 = new C.NumericValue();
			numericValue8.Text = "54";

			numericPoint6.Append(numericValue8);

			C.NumericPoint numericPoint7 = new C.NumericPoint() { Index = (UInt32Value)2U };
			C.NumericValue numericValue9 = new C.NumericValue();
			numericValue9.Text = "25";

			numericPoint7.Append(numericValue9);

			C.NumericPoint numericPoint8 = new C.NumericPoint() { Index = (UInt32Value)3U };
			C.NumericValue numericValue10 = new C.NumericValue();
			numericValue10.Text = "35";

			numericPoint8.Append(numericValue10);

			numberingCache2.Append(formatCode2);
			numberingCache2.Append(pointCount3);
			numberingCache2.Append(numericPoint5);
			numberingCache2.Append(numericPoint6);
			numberingCache2.Append(numericPoint7);
			numberingCache2.Append(numericPoint8);

			numberReference2.Append(formula3);
			numberReference2.Append(numberingCache2);

			values1.Append(numberReference2);

			barChartSeries1.Append(index1);
			barChartSeries1.Append(order1);
			barChartSeries1.Append(seriesText1);
			barChartSeries1.Append(chartShapeProperties1);
			barChartSeries1.Append(invertIfNegative1);
			barChartSeries1.Append(categoryAxisData1);
			barChartSeries1.Append(values1);

			C.BarChartSeries barChartSeries2 = new C.BarChartSeries();
			C.Index index2 = new C.Index() { Val = (UInt32Value)2U };
			C.Order order2 = new C.Order() { Val = (UInt32Value)2U };

			C.SeriesText seriesText2 = new C.SeriesText();

			C.StringReference stringReference2 = new C.StringReference();
			C.Formula formula4 = new C.Formula();
			formula4.Text = "Sheet1!$D$1:$D$2";

			C.StringCache stringCache2 = new C.StringCache();
			C.PointCount pointCount4 = new C.PointCount() { Val = (UInt32Value)2U };

			C.StringPoint stringPoint3 = new C.StringPoint() { Index = (UInt32Value)0U };
			C.NumericValue numericValue11 = new C.NumericValue();
			numericValue11.Text = "East";

			stringPoint3.Append(numericValue11);

			C.StringPoint stringPoint4 = new C.StringPoint() { Index = (UInt32Value)1U };
			C.NumericValue numericValue12 = new C.NumericValue();
			numericValue12.Text = "Count";

			stringPoint4.Append(numericValue12);

			stringCache2.Append(pointCount4);
			stringCache2.Append(stringPoint3);
			stringCache2.Append(stringPoint4);

			stringReference2.Append(formula4);
			stringReference2.Append(stringCache2);

			seriesText2.Append(stringReference2);

			C.ChartShapeProperties chartShapeProperties2 = new C.ChartShapeProperties();

			A.SolidFill solidFill7 = new A.SolidFill();
			A.RgbColorModelHex rgbColorModelHex15 = new A.RgbColorModelHex() { Val = "00B050" };

			solidFill7.Append(rgbColorModelHex15);

			A.Outline outline5 = new A.Outline();
			A.NoFill noFill2 = new A.NoFill();

			outline5.Append(noFill2);
			A.EffectList effectList5 = new A.EffectList();

			chartShapeProperties2.Append(solidFill7);
			chartShapeProperties2.Append(outline5);
			chartShapeProperties2.Append(effectList5);
			C.InvertIfNegative invertIfNegative2 = new C.InvertIfNegative() { Val = false };

			C.CategoryAxisData categoryAxisData2 = new C.CategoryAxisData();

			C.NumberReference numberReference3 = new C.NumberReference();
			C.Formula formula5 = new C.Formula();
			formula5.Text = "Sheet1!$A$3:$A$6";

			C.NumberingCache numberingCache3 = new C.NumberingCache();
			C.FormatCode formatCode3 = new C.FormatCode();
			formatCode3.Text = "m/d/yyyy";
			C.PointCount pointCount5 = new C.PointCount() { Val = (UInt32Value)4U };

			C.NumericPoint numericPoint9 = new C.NumericPoint() { Index = (UInt32Value)0U };
			C.NumericValue numericValue13 = new C.NumericValue();
			numericValue13.Text = "42372";

			numericPoint9.Append(numericValue13);

			C.NumericPoint numericPoint10 = new C.NumericPoint() { Index = (UInt32Value)1U };
			C.NumericValue numericValue14 = new C.NumericValue();
			numericValue14.Text = "42373";

			numericPoint10.Append(numericValue14);

			C.NumericPoint numericPoint11 = new C.NumericPoint() { Index = (UInt32Value)2U };
			C.NumericValue numericValue15 = new C.NumericValue();
			numericValue15.Text = "42374";

			numericPoint11.Append(numericValue15);

			C.NumericPoint numericPoint12 = new C.NumericPoint() { Index = (UInt32Value)3U };
			C.NumericValue numericValue16 = new C.NumericValue();
			numericValue16.Text = "42375";

			numericPoint12.Append(numericValue16);

			numberingCache3.Append(formatCode3);
			numberingCache3.Append(pointCount5);
			numberingCache3.Append(numericPoint9);
			numberingCache3.Append(numericPoint10);
			numberingCache3.Append(numericPoint11);
			numberingCache3.Append(numericPoint12);

			numberReference3.Append(formula5);
			numberReference3.Append(numberingCache3);

			categoryAxisData2.Append(numberReference3);

			C.Values values2 = new C.Values();

			C.NumberReference numberReference4 = new C.NumberReference();
			C.Formula formula6 = new C.Formula();
			formula6.Text = "Sheet1!$D$3:$D$6";

			C.NumberingCache numberingCache4 = new C.NumberingCache();
			C.FormatCode formatCode4 = new C.FormatCode();
			formatCode4.Text = "General";
			C.PointCount pointCount6 = new C.PointCount() { Val = (UInt32Value)4U };

			C.NumericPoint numericPoint13 = new C.NumericPoint() { Index = (UInt32Value)0U };
			C.NumericValue numericValue17 = new C.NumericValue();
			numericValue17.Text = "54";

			numericPoint13.Append(numericValue17);

			C.NumericPoint numericPoint14 = new C.NumericPoint() { Index = (UInt32Value)1U };
			C.NumericValue numericValue18 = new C.NumericValue();
			numericValue18.Text = "56";

			numericPoint14.Append(numericValue18);

			C.NumericPoint numericPoint15 = new C.NumericPoint() { Index = (UInt32Value)2U };
			C.NumericValue numericValue19 = new C.NumericValue();
			numericValue19.Text = "52";

			numericPoint15.Append(numericValue19);

			C.NumericPoint numericPoint16 = new C.NumericPoint() { Index = (UInt32Value)3U };
			C.NumericValue numericValue20 = new C.NumericValue();
			numericValue20.Text = "75";

			numericPoint16.Append(numericValue20);

			numberingCache4.Append(formatCode4);
			numberingCache4.Append(pointCount6);
			numberingCache4.Append(numericPoint13);
			numberingCache4.Append(numericPoint14);
			numberingCache4.Append(numericPoint15);
			numberingCache4.Append(numericPoint16);

			numberReference4.Append(formula6);
			numberReference4.Append(numberingCache4);

			values2.Append(numberReference4);

			barChartSeries2.Append(index2);
			barChartSeries2.Append(order2);
			barChartSeries2.Append(seriesText2);
			barChartSeries2.Append(chartShapeProperties2);
			barChartSeries2.Append(invertIfNegative2);
			barChartSeries2.Append(categoryAxisData2);
			barChartSeries2.Append(values2);

			C.BarChartSeries barChartSeries3 = new C.BarChartSeries();
			C.Index index3 = new C.Index() { Val = (UInt32Value)4U };
			C.Order order3 = new C.Order() { Val = (UInt32Value)4U };

			C.SeriesText seriesText3 = new C.SeriesText();

			C.StringReference stringReference3 = new C.StringReference();
			C.Formula formula7 = new C.Formula();
			formula7.Text = "Sheet1!$F$1:$F$2";

			C.StringCache stringCache3 = new C.StringCache();
			C.PointCount pointCount7 = new C.PointCount() { Val = (UInt32Value)2U };

			C.StringPoint stringPoint5 = new C.StringPoint() { Index = (UInt32Value)0U };
			C.NumericValue numericValue21 = new C.NumericValue();
			numericValue21.Text = "Site long name site";

			stringPoint5.Append(numericValue21);

			C.StringPoint stringPoint6 = new C.StringPoint() { Index = (UInt32Value)1U };
			C.NumericValue numericValue22 = new C.NumericValue();
			numericValue22.Text = "Count";

			stringPoint6.Append(numericValue22);

			stringCache3.Append(pointCount7);
			stringCache3.Append(stringPoint5);
			stringCache3.Append(stringPoint6);

			stringReference3.Append(formula7);
			stringReference3.Append(stringCache3);

			seriesText3.Append(stringReference3);

			C.ChartShapeProperties chartShapeProperties3 = new C.ChartShapeProperties();

			A.SolidFill solidFill8 = new A.SolidFill();
			A.RgbColorModelHex rgbColorModelHex16 = new A.RgbColorModelHex() { Val = "FFC000" };

			solidFill8.Append(rgbColorModelHex16);

			A.Outline outline6 = new A.Outline();
			A.NoFill noFill3 = new A.NoFill();

			outline6.Append(noFill3);
			A.EffectList effectList6 = new A.EffectList();

			chartShapeProperties3.Append(solidFill8);
			chartShapeProperties3.Append(outline6);
			chartShapeProperties3.Append(effectList6);
			C.InvertIfNegative invertIfNegative3 = new C.InvertIfNegative() { Val = false };

			C.CategoryAxisData categoryAxisData3 = new C.CategoryAxisData();

			C.NumberReference numberReference5 = new C.NumberReference();
			C.Formula formula8 = new C.Formula();
			formula8.Text = "Sheet1!$A$3:$A$6";

			C.NumberingCache numberingCache5 = new C.NumberingCache();
			C.FormatCode formatCode5 = new C.FormatCode();
			formatCode5.Text = "m/d/yyyy";
			C.PointCount pointCount8 = new C.PointCount() { Val = (UInt32Value)4U };

			C.NumericPoint numericPoint17 = new C.NumericPoint() { Index = (UInt32Value)0U };
			C.NumericValue numericValue23 = new C.NumericValue();
			numericValue23.Text = "42372";

			numericPoint17.Append(numericValue23);

			C.NumericPoint numericPoint18 = new C.NumericPoint() { Index = (UInt32Value)1U };
			C.NumericValue numericValue24 = new C.NumericValue();
			numericValue24.Text = "42373";

			numericPoint18.Append(numericValue24);

			C.NumericPoint numericPoint19 = new C.NumericPoint() { Index = (UInt32Value)2U };
			C.NumericValue numericValue25 = new C.NumericValue();
			numericValue25.Text = "42374";

			numericPoint19.Append(numericValue25);

			C.NumericPoint numericPoint20 = new C.NumericPoint() { Index = (UInt32Value)3U };
			C.NumericValue numericValue26 = new C.NumericValue();
			numericValue26.Text = "42375";

			numericPoint20.Append(numericValue26);

			numberingCache5.Append(formatCode5);
			numberingCache5.Append(pointCount8);
			numberingCache5.Append(numericPoint17);
			numberingCache5.Append(numericPoint18);
			numberingCache5.Append(numericPoint19);
			numberingCache5.Append(numericPoint20);

			numberReference5.Append(formula8);
			numberReference5.Append(numberingCache5);

			categoryAxisData3.Append(numberReference5);

			C.Values values3 = new C.Values();

			C.NumberReference numberReference6 = new C.NumberReference();
			C.Formula formula9 = new C.Formula();
			formula9.Text = "Sheet1!$F$3:$F$6";

			C.NumberingCache numberingCache6 = new C.NumberingCache();
			C.FormatCode formatCode6 = new C.FormatCode();
			formatCode6.Text = "General";
			C.PointCount pointCount9 = new C.PointCount() { Val = (UInt32Value)4U };

			C.NumericPoint numericPoint21 = new C.NumericPoint() { Index = (UInt32Value)0U };
			C.NumericValue numericValue27 = new C.NumericValue();
			numericValue27.Text = "91";

			numericPoint21.Append(numericValue27);

			C.NumericPoint numericPoint22 = new C.NumericPoint() { Index = (UInt32Value)1U };
			C.NumericValue numericValue28 = new C.NumericValue();
			numericValue28.Text = "55";

			numericPoint22.Append(numericValue28);

			C.NumericPoint numericPoint23 = new C.NumericPoint() { Index = (UInt32Value)2U };
			C.NumericValue numericValue29 = new C.NumericValue();
			numericValue29.Text = "25";

			numericPoint23.Append(numericValue29);

			C.NumericPoint numericPoint24 = new C.NumericPoint() { Index = (UInt32Value)3U };
			C.NumericValue numericValue30 = new C.NumericValue();
			numericValue30.Text = "12";

			numericPoint24.Append(numericValue30);

			numberingCache6.Append(formatCode6);
			numberingCache6.Append(pointCount9);
			numberingCache6.Append(numericPoint21);
			numberingCache6.Append(numericPoint22);
			numberingCache6.Append(numericPoint23);
			numberingCache6.Append(numericPoint24);

			numberReference6.Append(formula9);
			numberReference6.Append(numberingCache6);

			values3.Append(numberReference6);

			barChartSeries3.Append(index3);
			barChartSeries3.Append(order3);
			barChartSeries3.Append(seriesText3);
			barChartSeries3.Append(chartShapeProperties3);
			barChartSeries3.Append(invertIfNegative3);
			barChartSeries3.Append(categoryAxisData3);
			barChartSeries3.Append(values3);

			C.DataLabels dataLabels1 = new C.DataLabels();
			C.ShowLegendKey showLegendKey1 = new C.ShowLegendKey() { Val = false };
			C.ShowValue showValue1 = new C.ShowValue() { Val = false };
			C.ShowCategoryName showCategoryName1 = new C.ShowCategoryName() { Val = false };
			C.ShowSeriesName showSeriesName1 = new C.ShowSeriesName() { Val = false };
			C.ShowPercent showPercent1 = new C.ShowPercent() { Val = false };
			C.ShowBubbleSize showBubbleSize1 = new C.ShowBubbleSize() { Val = false };

			dataLabels1.Append(showLegendKey1);
			dataLabels1.Append(showValue1);
			dataLabels1.Append(showCategoryName1);
			dataLabels1.Append(showSeriesName1);
			dataLabels1.Append(showPercent1);
			dataLabels1.Append(showBubbleSize1);
			C.GapWidth gapWidth1 = new C.GapWidth() { Val = (UInt16Value)219U };
			C.AxisId axisId1 = new C.AxisId() { Val = new UInt32Value() { InnerText = "-162426480" } };
			C.AxisId axisId2 = new C.AxisId() { Val = new UInt32Value() { InnerText = "-162419408" } };

			barChart1.Append(barDirection1);
			barChart1.Append(barGrouping1);
			barChart1.Append(varyColors1);
			barChart1.Append(barChartSeries1);
			barChart1.Append(barChartSeries2);
			barChart1.Append(barChartSeries3);
			barChart1.Append(dataLabels1);
			barChart1.Append(gapWidth1);
			barChart1.Append(axisId1);
			barChart1.Append(axisId2);

			C.LineChart lineChart1 = new C.LineChart();
			C.Grouping grouping1 = new C.Grouping() { Val = C.GroupingValues.Standard };
			C.VaryColors varyColors2 = new C.VaryColors() { Val = false };

			C.LineChartSeries lineChartSeries1 = new C.LineChartSeries();
			C.Index index4 = new C.Index() { Val = (UInt32Value)1U };
			C.Order order4 = new C.Order() { Val = (UInt32Value)1U };

			C.SeriesText seriesText4 = new C.SeriesText();

			C.StringReference stringReference4 = new C.StringReference();
			C.Formula formula10 = new C.Formula();
			formula10.Text = "Sheet1!$C$1:$C$2";

			C.StringCache stringCache4 = new C.StringCache();
			C.PointCount pointCount10 = new C.PointCount() { Val = (UInt32Value)2U };

			C.StringPoint stringPoint7 = new C.StringPoint() { Index = (UInt32Value)0U };
			C.NumericValue numericValue31 = new C.NumericValue();
			numericValue31.Text = "I3DVR";

			stringPoint7.Append(numericValue31);

			C.StringPoint stringPoint8 = new C.StringPoint() { Index = (UInt32Value)1U };
			C.NumericValue numericValue32 = new C.NumericValue();
			numericValue32.Text = "Dwell";

			stringPoint8.Append(numericValue32);

			stringCache4.Append(pointCount10);
			stringCache4.Append(stringPoint7);
			stringCache4.Append(stringPoint8);

			stringReference4.Append(formula10);
			stringReference4.Append(stringCache4);

			seriesText4.Append(stringReference4);

			C.ChartShapeProperties chartShapeProperties4 = new C.ChartShapeProperties();

			A.Outline outline7 = new A.Outline() { Width = 28575, CapType = A.LineCapValues.Round };

			A.SolidFill solidFill9 = new A.SolidFill();
			A.RgbColorModelHex rgbColorModelHex17 = new A.RgbColorModelHex() { Val = "0070C0" };

			solidFill9.Append(rgbColorModelHex17);
			A.Round round1 = new A.Round();

			outline7.Append(solidFill9);
			outline7.Append(round1);
			A.EffectList effectList7 = new A.EffectList();

			chartShapeProperties4.Append(outline7);
			chartShapeProperties4.Append(effectList7);

			C.Marker marker1 = new C.Marker();
			C.Symbol symbol1 = new C.Symbol() { Val = C.MarkerStyleValues.Circle };
			C.Size size1 = new C.Size() { Val = 5 };

			C.ChartShapeProperties chartShapeProperties5 = new C.ChartShapeProperties();

			A.SolidFill solidFill10 = new A.SolidFill();
			A.SchemeColor schemeColor17 = new A.SchemeColor() { Val = A.SchemeColorValues.Accent2 };

			solidFill10.Append(schemeColor17);

			A.Outline outline8 = new A.Outline() { Width = 9525 };

			A.SolidFill solidFill11 = new A.SolidFill();
			A.SchemeColor schemeColor18 = new A.SchemeColor() { Val = A.SchemeColorValues.Accent2 };

			solidFill11.Append(schemeColor18);

			outline8.Append(solidFill11);
			A.EffectList effectList8 = new A.EffectList();

			chartShapeProperties5.Append(solidFill10);
			chartShapeProperties5.Append(outline8);
			chartShapeProperties5.Append(effectList8);

			marker1.Append(symbol1);
			marker1.Append(size1);
			marker1.Append(chartShapeProperties5);

			C.CategoryAxisData categoryAxisData4 = new C.CategoryAxisData();

			C.NumberReference numberReference7 = new C.NumberReference();
			C.Formula formula11 = new C.Formula();
			formula11.Text = "Sheet1!$A$3:$A$6";

			C.NumberingCache numberingCache7 = new C.NumberingCache();
			C.FormatCode formatCode7 = new C.FormatCode();
			formatCode7.Text = "m/d/yyyy";
			C.PointCount pointCount11 = new C.PointCount() { Val = (UInt32Value)4U };

			C.NumericPoint numericPoint25 = new C.NumericPoint() { Index = (UInt32Value)0U };
			C.NumericValue numericValue33 = new C.NumericValue();
			numericValue33.Text = "42372";

			numericPoint25.Append(numericValue33);

			C.NumericPoint numericPoint26 = new C.NumericPoint() { Index = (UInt32Value)1U };
			C.NumericValue numericValue34 = new C.NumericValue();
			numericValue34.Text = "42373";

			numericPoint26.Append(numericValue34);

			C.NumericPoint numericPoint27 = new C.NumericPoint() { Index = (UInt32Value)2U };
			C.NumericValue numericValue35 = new C.NumericValue();
			numericValue35.Text = "42374";

			numericPoint27.Append(numericValue35);

			C.NumericPoint numericPoint28 = new C.NumericPoint() { Index = (UInt32Value)3U };
			C.NumericValue numericValue36 = new C.NumericValue();
			numericValue36.Text = "42375";

			numericPoint28.Append(numericValue36);

			numberingCache7.Append(formatCode7);
			numberingCache7.Append(pointCount11);
			numberingCache7.Append(numericPoint25);
			numberingCache7.Append(numericPoint26);
			numberingCache7.Append(numericPoint27);
			numberingCache7.Append(numericPoint28);

			numberReference7.Append(formula11);
			numberReference7.Append(numberingCache7);

			categoryAxisData4.Append(numberReference7);

			C.Values values4 = new C.Values();

			C.NumberReference numberReference8 = new C.NumberReference();
			C.Formula formula12 = new C.Formula();
			formula12.Text = "Sheet1!$C$3:$C$6";

			C.NumberingCache numberingCache8 = new C.NumberingCache();
			C.FormatCode formatCode8 = new C.FormatCode();
			formatCode8.Text = "General";
			C.PointCount pointCount12 = new C.PointCount() { Val = (UInt32Value)4U };

			C.NumericPoint numericPoint29 = new C.NumericPoint() { Index = (UInt32Value)0U };
			C.NumericValue numericValue37 = new C.NumericValue();
			numericValue37.Text = "12";

			numericPoint29.Append(numericValue37);

			C.NumericPoint numericPoint30 = new C.NumericPoint() { Index = (UInt32Value)1U };
			C.NumericValue numericValue38 = new C.NumericValue();
			numericValue38.Text = "24";

			numericPoint30.Append(numericValue38);

			C.NumericPoint numericPoint31 = new C.NumericPoint() { Index = (UInt32Value)2U };
			C.NumericValue numericValue39 = new C.NumericValue();
			numericValue39.Text = "14";

			numericPoint31.Append(numericValue39);

			C.NumericPoint numericPoint32 = new C.NumericPoint() { Index = (UInt32Value)3U };
			C.NumericValue numericValue40 = new C.NumericValue();
			numericValue40.Text = "42";

			numericPoint32.Append(numericValue40);

			numberingCache8.Append(formatCode8);
			numberingCache8.Append(pointCount12);
			numberingCache8.Append(numericPoint29);
			numberingCache8.Append(numericPoint30);
			numberingCache8.Append(numericPoint31);
			numberingCache8.Append(numericPoint32);

			numberReference8.Append(formula12);
			numberReference8.Append(numberingCache8);

			values4.Append(numberReference8);
			C.Smooth smooth1 = new C.Smooth() { Val = false };

			lineChartSeries1.Append(index4);
			lineChartSeries1.Append(order4);
			lineChartSeries1.Append(seriesText4);
			lineChartSeries1.Append(chartShapeProperties4);
			lineChartSeries1.Append(marker1);
			lineChartSeries1.Append(categoryAxisData4);
			lineChartSeries1.Append(values4);
			lineChartSeries1.Append(smooth1);

			C.LineChartSeries lineChartSeries2 = new C.LineChartSeries();
			C.Index index5 = new C.Index() { Val = (UInt32Value)3U };
			C.Order order5 = new C.Order() { Val = (UInt32Value)3U };

			C.SeriesText seriesText5 = new C.SeriesText();

			C.StringReference stringReference5 = new C.StringReference();
			C.Formula formula13 = new C.Formula();
			formula13.Text = "Sheet1!$E$1:$E$2";

			C.StringCache stringCache5 = new C.StringCache();
			C.PointCount pointCount13 = new C.PointCount() { Val = (UInt32Value)2U };

			C.StringPoint stringPoint9 = new C.StringPoint() { Index = (UInt32Value)0U };
			C.NumericValue numericValue41 = new C.NumericValue();
			numericValue41.Text = "East";

			stringPoint9.Append(numericValue41);

			C.StringPoint stringPoint10 = new C.StringPoint() { Index = (UInt32Value)1U };
			C.NumericValue numericValue42 = new C.NumericValue();
			numericValue42.Text = "Dwell";

			stringPoint10.Append(numericValue42);

			stringCache5.Append(pointCount13);
			stringCache5.Append(stringPoint9);
			stringCache5.Append(stringPoint10);

			stringReference5.Append(formula13);
			stringReference5.Append(stringCache5);

			seriesText5.Append(stringReference5);

			C.ChartShapeProperties chartShapeProperties6 = new C.ChartShapeProperties();

			A.Outline outline9 = new A.Outline() { Width = 28575, CapType = A.LineCapValues.Round };

			A.SolidFill solidFill12 = new A.SolidFill();
			A.RgbColorModelHex rgbColorModelHex18 = new A.RgbColorModelHex() { Val = "00B050" };

			solidFill12.Append(rgbColorModelHex18);
			A.Round round2 = new A.Round();

			outline9.Append(solidFill12);
			outline9.Append(round2);
			A.EffectList effectList9 = new A.EffectList();

			chartShapeProperties6.Append(outline9);
			chartShapeProperties6.Append(effectList9);

			C.Marker marker2 = new C.Marker();
			C.Symbol symbol2 = new C.Symbol() { Val = C.MarkerStyleValues.Circle };
			C.Size size2 = new C.Size() { Val = 5 };

			C.ChartShapeProperties chartShapeProperties7 = new C.ChartShapeProperties();

			A.SolidFill solidFill13 = new A.SolidFill();
			A.SchemeColor schemeColor19 = new A.SchemeColor() { Val = A.SchemeColorValues.Accent4 };

			solidFill13.Append(schemeColor19);

			A.Outline outline10 = new A.Outline() { Width = 9525 };

			A.SolidFill solidFill14 = new A.SolidFill();
			A.SchemeColor schemeColor20 = new A.SchemeColor() { Val = A.SchemeColorValues.Accent4 };

			solidFill14.Append(schemeColor20);

			outline10.Append(solidFill14);
			A.EffectList effectList10 = new A.EffectList();

			chartShapeProperties7.Append(solidFill13);
			chartShapeProperties7.Append(outline10);
			chartShapeProperties7.Append(effectList10);

			marker2.Append(symbol2);
			marker2.Append(size2);
			marker2.Append(chartShapeProperties7);

			C.CategoryAxisData categoryAxisData5 = new C.CategoryAxisData();

			C.NumberReference numberReference9 = new C.NumberReference();
			C.Formula formula14 = new C.Formula();
			formula14.Text = "Sheet1!$A$3:$A$6";

			C.NumberingCache numberingCache9 = new C.NumberingCache();
			C.FormatCode formatCode9 = new C.FormatCode();
			formatCode9.Text = "m/d/yyyy";
			C.PointCount pointCount14 = new C.PointCount() { Val = (UInt32Value)4U };

			C.NumericPoint numericPoint33 = new C.NumericPoint() { Index = (UInt32Value)0U };
			C.NumericValue numericValue43 = new C.NumericValue();
			numericValue43.Text = "42372";

			numericPoint33.Append(numericValue43);

			C.NumericPoint numericPoint34 = new C.NumericPoint() { Index = (UInt32Value)1U };
			C.NumericValue numericValue44 = new C.NumericValue();
			numericValue44.Text = "42373";

			numericPoint34.Append(numericValue44);

			C.NumericPoint numericPoint35 = new C.NumericPoint() { Index = (UInt32Value)2U };
			C.NumericValue numericValue45 = new C.NumericValue();
			numericValue45.Text = "42374";

			numericPoint35.Append(numericValue45);

			C.NumericPoint numericPoint36 = new C.NumericPoint() { Index = (UInt32Value)3U };
			C.NumericValue numericValue46 = new C.NumericValue();
			numericValue46.Text = "42375";

			numericPoint36.Append(numericValue46);

			numberingCache9.Append(formatCode9);
			numberingCache9.Append(pointCount14);
			numberingCache9.Append(numericPoint33);
			numberingCache9.Append(numericPoint34);
			numberingCache9.Append(numericPoint35);
			numberingCache9.Append(numericPoint36);

			numberReference9.Append(formula14);
			numberReference9.Append(numberingCache9);

			categoryAxisData5.Append(numberReference9);

			C.Values values5 = new C.Values();

			C.NumberReference numberReference10 = new C.NumberReference();
			C.Formula formula15 = new C.Formula();
			formula15.Text = "Sheet1!$E$3:$E$6";

			C.NumberingCache numberingCache10 = new C.NumberingCache();
			C.FormatCode formatCode10 = new C.FormatCode();
			formatCode10.Text = "General";
			C.PointCount pointCount15 = new C.PointCount() { Val = (UInt32Value)4U };

			C.NumericPoint numericPoint37 = new C.NumericPoint() { Index = (UInt32Value)0U };
			C.NumericValue numericValue47 = new C.NumericValue();
			numericValue47.Text = "23";

			numericPoint37.Append(numericValue47);

			C.NumericPoint numericPoint38 = new C.NumericPoint() { Index = (UInt32Value)1U };
			C.NumericValue numericValue48 = new C.NumericValue();
			numericValue48.Text = "12";

			numericPoint38.Append(numericValue48);

			C.NumericPoint numericPoint39 = new C.NumericPoint() { Index = (UInt32Value)2U };
			C.NumericValue numericValue49 = new C.NumericValue();
			numericValue49.Text = "25";

			numericPoint39.Append(numericValue49);

			C.NumericPoint numericPoint40 = new C.NumericPoint() { Index = (UInt32Value)3U };
			C.NumericValue numericValue50 = new C.NumericValue();
			numericValue50.Text = "45";

			numericPoint40.Append(numericValue50);

			numberingCache10.Append(formatCode10);
			numberingCache10.Append(pointCount15);
			numberingCache10.Append(numericPoint37);
			numberingCache10.Append(numericPoint38);
			numberingCache10.Append(numericPoint39);
			numberingCache10.Append(numericPoint40);

			numberReference10.Append(formula15);
			numberReference10.Append(numberingCache10);

			values5.Append(numberReference10);
			C.Smooth smooth2 = new C.Smooth() { Val = false };

			lineChartSeries2.Append(index5);
			lineChartSeries2.Append(order5);
			lineChartSeries2.Append(seriesText5);
			lineChartSeries2.Append(chartShapeProperties6);
			lineChartSeries2.Append(marker2);
			lineChartSeries2.Append(categoryAxisData5);
			lineChartSeries2.Append(values5);
			lineChartSeries2.Append(smooth2);

			C.LineChartSeries lineChartSeries3 = new C.LineChartSeries();
			C.Index index6 = new C.Index() { Val = (UInt32Value)5U };
			C.Order order6 = new C.Order() { Val = (UInt32Value)5U };

			C.SeriesText seriesText6 = new C.SeriesText();

			C.StringReference stringReference6 = new C.StringReference();
			C.Formula formula16 = new C.Formula();
			formula16.Text = "Sheet1!$G$1:$G$2";

			C.StringCache stringCache6 = new C.StringCache();
			C.PointCount pointCount16 = new C.PointCount() { Val = (UInt32Value)2U };

			C.StringPoint stringPoint11 = new C.StringPoint() { Index = (UInt32Value)0U };
			C.NumericValue numericValue51 = new C.NumericValue();
			numericValue51.Text = "Site long name site";

			stringPoint11.Append(numericValue51);

			C.StringPoint stringPoint12 = new C.StringPoint() { Index = (UInt32Value)1U };
			C.NumericValue numericValue52 = new C.NumericValue();
			numericValue52.Text = "Dwell";

			stringPoint12.Append(numericValue52);

			stringCache6.Append(pointCount16);
			stringCache6.Append(stringPoint11);
			stringCache6.Append(stringPoint12);

			stringReference6.Append(formula16);
			stringReference6.Append(stringCache6);

			seriesText6.Append(stringReference6);

			C.ChartShapeProperties chartShapeProperties8 = new C.ChartShapeProperties();

			A.Outline outline11 = new A.Outline() { Width = 28575, CapType = A.LineCapValues.Round };

			A.SolidFill solidFill15 = new A.SolidFill();
			A.SchemeColor schemeColor21 = new A.SchemeColor() { Val = A.SchemeColorValues.Accent6 };

			solidFill15.Append(schemeColor21);
			A.Round round3 = new A.Round();

			outline11.Append(solidFill15);
			outline11.Append(round3);
			A.EffectList effectList11 = new A.EffectList();

			chartShapeProperties8.Append(outline11);
			chartShapeProperties8.Append(effectList11);

			C.Marker marker3 = new C.Marker();
			C.Symbol symbol3 = new C.Symbol() { Val = C.MarkerStyleValues.Circle };
			C.Size size3 = new C.Size() { Val = 5 };

			C.ChartShapeProperties chartShapeProperties9 = new C.ChartShapeProperties();

			A.SolidFill solidFill16 = new A.SolidFill();
			A.SchemeColor schemeColor22 = new A.SchemeColor() { Val = A.SchemeColorValues.Accent6 };

			solidFill16.Append(schemeColor22);

			A.Outline outline12 = new A.Outline() { Width = 9525 };

			A.SolidFill solidFill17 = new A.SolidFill();
			A.SchemeColor schemeColor23 = new A.SchemeColor() { Val = A.SchemeColorValues.Accent6 };

			solidFill17.Append(schemeColor23);

			outline12.Append(solidFill17);
			A.EffectList effectList12 = new A.EffectList();

			chartShapeProperties9.Append(solidFill16);
			chartShapeProperties9.Append(outline12);
			chartShapeProperties9.Append(effectList12);

			marker3.Append(symbol3);
			marker3.Append(size3);
			marker3.Append(chartShapeProperties9);

			C.CategoryAxisData categoryAxisData6 = new C.CategoryAxisData();

			C.NumberReference numberReference11 = new C.NumberReference();
			C.Formula formula17 = new C.Formula();
			formula17.Text = "Sheet1!$A$3:$A$6";

			C.NumberingCache numberingCache11 = new C.NumberingCache();
			C.FormatCode formatCode11 = new C.FormatCode();
			formatCode11.Text = "m/d/yyyy";
			C.PointCount pointCount17 = new C.PointCount() { Val = (UInt32Value)4U };

			C.NumericPoint numericPoint41 = new C.NumericPoint() { Index = (UInt32Value)0U };
			C.NumericValue numericValue53 = new C.NumericValue();
			numericValue53.Text = "42372";

			numericPoint41.Append(numericValue53);

			C.NumericPoint numericPoint42 = new C.NumericPoint() { Index = (UInt32Value)1U };
			C.NumericValue numericValue54 = new C.NumericValue();
			numericValue54.Text = "42373";

			numericPoint42.Append(numericValue54);

			C.NumericPoint numericPoint43 = new C.NumericPoint() { Index = (UInt32Value)2U };
			C.NumericValue numericValue55 = new C.NumericValue();
			numericValue55.Text = "42374";

			numericPoint43.Append(numericValue55);

			C.NumericPoint numericPoint44 = new C.NumericPoint() { Index = (UInt32Value)3U };
			C.NumericValue numericValue56 = new C.NumericValue();
			numericValue56.Text = "42375";

			numericPoint44.Append(numericValue56);

			numberingCache11.Append(formatCode11);
			numberingCache11.Append(pointCount17);
			numberingCache11.Append(numericPoint41);
			numberingCache11.Append(numericPoint42);
			numberingCache11.Append(numericPoint43);
			numberingCache11.Append(numericPoint44);

			numberReference11.Append(formula17);
			numberReference11.Append(numberingCache11);

			categoryAxisData6.Append(numberReference11);

			C.Values values6 = new C.Values();

			C.NumberReference numberReference12 = new C.NumberReference();
			C.Formula formula18 = new C.Formula();
			formula18.Text = "Sheet1!$G$3:$G$6";

			C.NumberingCache numberingCache12 = new C.NumberingCache();
			C.FormatCode formatCode12 = new C.FormatCode();
			formatCode12.Text = "General";
			C.PointCount pointCount18 = new C.PointCount() { Val = (UInt32Value)4U };

			C.NumericPoint numericPoint45 = new C.NumericPoint() { Index = (UInt32Value)0U };
			C.NumericValue numericValue57 = new C.NumericValue();
			numericValue57.Text = "65";

			numericPoint45.Append(numericValue57);

			C.NumericPoint numericPoint46 = new C.NumericPoint() { Index = (UInt32Value)1U };
			C.NumericValue numericValue58 = new C.NumericValue();
			numericValue58.Text = "32";

			numericPoint46.Append(numericValue58);

			C.NumericPoint numericPoint47 = new C.NumericPoint() { Index = (UInt32Value)2U };
			C.NumericValue numericValue59 = new C.NumericValue();
			numericValue59.Text = "35";

			numericPoint47.Append(numericValue59);

			C.NumericPoint numericPoint48 = new C.NumericPoint() { Index = (UInt32Value)3U };
			C.NumericValue numericValue60 = new C.NumericValue();
			numericValue60.Text = "32";

			numericPoint48.Append(numericValue60);

			numberingCache12.Append(formatCode12);
			numberingCache12.Append(pointCount18);
			numberingCache12.Append(numericPoint45);
			numberingCache12.Append(numericPoint46);
			numberingCache12.Append(numericPoint47);
			numberingCache12.Append(numericPoint48);

			numberReference12.Append(formula18);
			numberReference12.Append(numberingCache12);

			values6.Append(numberReference12);
			C.Smooth smooth3 = new C.Smooth() { Val = false };

			lineChartSeries3.Append(index6);
			lineChartSeries3.Append(order6);
			lineChartSeries3.Append(seriesText6);
			lineChartSeries3.Append(chartShapeProperties8);
			lineChartSeries3.Append(marker3);
			lineChartSeries3.Append(categoryAxisData6);
			lineChartSeries3.Append(values6);
			lineChartSeries3.Append(smooth3);

			C.DataLabels dataLabels2 = new C.DataLabels();
			C.ShowLegendKey showLegendKey2 = new C.ShowLegendKey() { Val = false };
			C.ShowValue showValue2 = new C.ShowValue() { Val = false };
			C.ShowCategoryName showCategoryName2 = new C.ShowCategoryName() { Val = false };
			C.ShowSeriesName showSeriesName2 = new C.ShowSeriesName() { Val = false };
			C.ShowPercent showPercent2 = new C.ShowPercent() { Val = false };
			C.ShowBubbleSize showBubbleSize2 = new C.ShowBubbleSize() { Val = false };

			dataLabels2.Append(showLegendKey2);
			dataLabels2.Append(showValue2);
			dataLabels2.Append(showCategoryName2);
			dataLabels2.Append(showSeriesName2);
			dataLabels2.Append(showPercent2);
			dataLabels2.Append(showBubbleSize2);
			C.ShowMarker showMarker1 = new C.ShowMarker() { Val = true };
			C.Smooth smooth4 = new C.Smooth() { Val = false };
			C.AxisId axisId3 = new C.AxisId() { Val = new UInt32Value() { InnerText = "-336834096" } };
			C.AxisId axisId4 = new C.AxisId() { Val = new UInt32Value() { InnerText = "-336831920" } };

			lineChart1.Append(grouping1);
			lineChart1.Append(varyColors2);
			lineChart1.Append(lineChartSeries1);
			lineChart1.Append(lineChartSeries2);
			lineChart1.Append(lineChartSeries3);
			lineChart1.Append(dataLabels2);
			lineChart1.Append(showMarker1);
			lineChart1.Append(smooth4);
			lineChart1.Append(axisId3);
			lineChart1.Append(axisId4);

			C.DateAxis dateAxis1 = new C.DateAxis();
			C.AxisId axisId5 = new C.AxisId() { Val = new UInt32Value() { InnerText = "-336834096" } };

			C.Scaling scaling1 = new C.Scaling();
			C.Orientation orientation1 = new C.Orientation() { Val = C.OrientationValues.MinMax };

			scaling1.Append(orientation1);
			C.Delete delete1 = new C.Delete() { Val = false };
			C.AxisPosition axisPosition1 = new C.AxisPosition() { Val = C.AxisPositionValues.Bottom };
			C.NumberingFormat numberingFormat1 = new C.NumberingFormat() { FormatCode = "m/d/yyyy", SourceLinked = true };
			C.MajorTickMark majorTickMark1 = new C.MajorTickMark() { Val = C.TickMarkValues.None };
			C.MinorTickMark minorTickMark1 = new C.MinorTickMark() { Val = C.TickMarkValues.None };
			C.TickLabelPosition tickLabelPosition1 = new C.TickLabelPosition() { Val = C.TickLabelPositionValues.NextTo };

			C.ChartShapeProperties chartShapeProperties10 = new C.ChartShapeProperties();
			A.NoFill noFill4 = new A.NoFill();

			A.Outline outline13 = new A.Outline() { Width = 9525, CapType = A.LineCapValues.Flat, CompoundLineType = A.CompoundLineValues.Single, Alignment = A.PenAlignmentValues.Center };

			A.SolidFill solidFill18 = new A.SolidFill();

			A.SchemeColor schemeColor24 = new A.SchemeColor() { Val = A.SchemeColorValues.Text1 };
			A.LuminanceModulation luminanceModulation1 = new A.LuminanceModulation() { Val = 15000 };
			A.LuminanceOffset luminanceOffset1 = new A.LuminanceOffset() { Val = 85000 };

			schemeColor24.Append(luminanceModulation1);
			schemeColor24.Append(luminanceOffset1);

			solidFill18.Append(schemeColor24);
			A.Round round4 = new A.Round();

			outline13.Append(solidFill18);
			outline13.Append(round4);
			A.EffectList effectList13 = new A.EffectList();

			chartShapeProperties10.Append(noFill4);
			chartShapeProperties10.Append(outline13);
			chartShapeProperties10.Append(effectList13);

			C.TextProperties textProperties1 = new C.TextProperties();
			A.BodyProperties bodyProperties1 = new A.BodyProperties() { Rotation = -60000000, UseParagraphSpacing = true, VerticalOverflow = A.TextVerticalOverflowValues.Ellipsis, Vertical = A.TextVerticalValues.Horizontal, Wrap = A.TextWrappingValues.Square, Anchor = A.TextAnchoringTypeValues.Center, AnchorCenter = true };
			A.ListStyle listStyle1 = new A.ListStyle();

			A.Paragraph paragraph1 = new A.Paragraph();

			A.ParagraphProperties paragraphProperties1 = new A.ParagraphProperties();

			A.DefaultRunProperties defaultRunProperties1 = new A.DefaultRunProperties() { FontSize = 900, Bold = false, Italic = false, Underline = A.TextUnderlineValues.None, Strike = A.TextStrikeValues.NoStrike, Kerning = 1200, Baseline = 0 };

			A.SolidFill solidFill19 = new A.SolidFill();

			A.SchemeColor schemeColor25 = new A.SchemeColor() { Val = A.SchemeColorValues.Text1 };
			A.LuminanceModulation luminanceModulation2 = new A.LuminanceModulation() { Val = 65000 };
			A.LuminanceOffset luminanceOffset2 = new A.LuminanceOffset() { Val = 35000 };

			schemeColor25.Append(luminanceModulation2);
			schemeColor25.Append(luminanceOffset2);

			solidFill19.Append(schemeColor25);
			A.LatinFont latinFont3 = new A.LatinFont() { Typeface = "+mn-lt" };
			A.EastAsianFont eastAsianFont3 = new A.EastAsianFont() { Typeface = "+mn-ea" };
			A.ComplexScriptFont complexScriptFont3 = new A.ComplexScriptFont() { Typeface = "+mn-cs" };

			defaultRunProperties1.Append(solidFill19);
			defaultRunProperties1.Append(latinFont3);
			defaultRunProperties1.Append(eastAsianFont3);
			defaultRunProperties1.Append(complexScriptFont3);

			paragraphProperties1.Append(defaultRunProperties1);
			A.EndParagraphRunProperties endParagraphRunProperties1 = new A.EndParagraphRunProperties() { Language = "en-US" };

			paragraph1.Append(paragraphProperties1);
			paragraph1.Append(endParagraphRunProperties1);

			textProperties1.Append(bodyProperties1);
			textProperties1.Append(listStyle1);
			textProperties1.Append(paragraph1);
			C.CrossingAxis crossingAxis1 = new C.CrossingAxis() { Val = new UInt32Value() { InnerText = "-336831920" } };
			C.Crosses crosses1 = new C.Crosses() { Val = C.CrossesValues.AutoZero };
			C.AutoLabeled autoLabeled1 = new C.AutoLabeled() { Val = true };
			C.LabelOffset labelOffset1 = new C.LabelOffset() { Val = (UInt16Value)100U };
			C.BaseTimeUnit baseTimeUnit1 = new C.BaseTimeUnit() { Val = C.TimeUnitValues.Days };

			dateAxis1.Append(axisId5);
			dateAxis1.Append(scaling1);
			dateAxis1.Append(delete1);
			dateAxis1.Append(axisPosition1);
			dateAxis1.Append(numberingFormat1);
			dateAxis1.Append(majorTickMark1);
			dateAxis1.Append(minorTickMark1);
			dateAxis1.Append(tickLabelPosition1);
			dateAxis1.Append(chartShapeProperties10);
			dateAxis1.Append(textProperties1);
			dateAxis1.Append(crossingAxis1);
			dateAxis1.Append(crosses1);
			dateAxis1.Append(autoLabeled1);
			dateAxis1.Append(labelOffset1);
			dateAxis1.Append(baseTimeUnit1);

			C.ValueAxis valueAxis1 = new C.ValueAxis();
			C.AxisId axisId6 = new C.AxisId() { Val = new UInt32Value() { InnerText = "-336831920" } };

			C.Scaling scaling2 = new C.Scaling();
			C.Orientation orientation2 = new C.Orientation() { Val = C.OrientationValues.MinMax };

			scaling2.Append(orientation2);
			C.Delete delete2 = new C.Delete() { Val = false };
			C.AxisPosition axisPosition2 = new C.AxisPosition() { Val = C.AxisPositionValues.Left };

			C.MajorGridlines majorGridlines1 = new C.MajorGridlines();

			C.ChartShapeProperties chartShapeProperties11 = new C.ChartShapeProperties();

			A.Outline outline14 = new A.Outline() { Width = 9525, CapType = A.LineCapValues.Flat, CompoundLineType = A.CompoundLineValues.Single, Alignment = A.PenAlignmentValues.Center };

			A.SolidFill solidFill20 = new A.SolidFill();

			A.SchemeColor schemeColor26 = new A.SchemeColor() { Val = A.SchemeColorValues.Text1 };
			A.LuminanceModulation luminanceModulation3 = new A.LuminanceModulation() { Val = 15000 };
			A.LuminanceOffset luminanceOffset3 = new A.LuminanceOffset() { Val = 85000 };

			schemeColor26.Append(luminanceModulation3);
			schemeColor26.Append(luminanceOffset3);

			solidFill20.Append(schemeColor26);
			A.Round round5 = new A.Round();

			outline14.Append(solidFill20);
			outline14.Append(round5);
			A.EffectList effectList14 = new A.EffectList();

			chartShapeProperties11.Append(outline14);
			chartShapeProperties11.Append(effectList14);

			majorGridlines1.Append(chartShapeProperties11);

			C.Title title1 = new C.Title();

			C.ChartText chartText1 = new C.ChartText();

			C.RichText richText1 = new C.RichText();
			A.BodyProperties bodyProperties2 = new A.BodyProperties() { Rotation = -5400000, UseParagraphSpacing = true, VerticalOverflow = A.TextVerticalOverflowValues.Ellipsis, Vertical = A.TextVerticalValues.Horizontal, Wrap = A.TextWrappingValues.Square, Anchor = A.TextAnchoringTypeValues.Center, AnchorCenter = true };
			A.ListStyle listStyle2 = new A.ListStyle();

			A.Paragraph paragraph2 = new A.Paragraph();

			A.ParagraphProperties paragraphProperties2 = new A.ParagraphProperties();

			A.DefaultRunProperties defaultRunProperties2 = new A.DefaultRunProperties() { FontSize = 1000, Bold = false, Italic = false, Underline = A.TextUnderlineValues.None, Strike = A.TextStrikeValues.NoStrike, Kerning = 1200, Baseline = 0 };

			A.SolidFill solidFill21 = new A.SolidFill();

			A.SchemeColor schemeColor27 = new A.SchemeColor() { Val = A.SchemeColorValues.Text1 };
			A.LuminanceModulation luminanceModulation4 = new A.LuminanceModulation() { Val = 65000 };
			A.LuminanceOffset luminanceOffset4 = new A.LuminanceOffset() { Val = 35000 };

			schemeColor27.Append(luminanceModulation4);
			schemeColor27.Append(luminanceOffset4);

			solidFill21.Append(schemeColor27);
			A.LatinFont latinFont4 = new A.LatinFont() { Typeface = "+mn-lt" };
			A.EastAsianFont eastAsianFont4 = new A.EastAsianFont() { Typeface = "+mn-ea" };
			A.ComplexScriptFont complexScriptFont4 = new A.ComplexScriptFont() { Typeface = "+mn-cs" };

			defaultRunProperties2.Append(solidFill21);
			defaultRunProperties2.Append(latinFont4);
			defaultRunProperties2.Append(eastAsianFont4);
			defaultRunProperties2.Append(complexScriptFont4);

			paragraphProperties2.Append(defaultRunProperties2);

			A.Run run1 = new A.Run();
			A.RunProperties runProperties1 = new A.RunProperties() { Language = "en-US" };
			A.Text text1 = new A.Text();
			text1.Text = "Dwell(s)";

			run1.Append(runProperties1);
			run1.Append(text1);

			paragraph2.Append(paragraphProperties2);
			paragraph2.Append(run1);

			richText1.Append(bodyProperties2);
			richText1.Append(listStyle2);
			richText1.Append(paragraph2);

			chartText1.Append(richText1);
			C.Layout layout2 = new C.Layout();
			C.Overlay overlay1 = new C.Overlay() { Val = false };

			C.ChartShapeProperties chartShapeProperties12 = new C.ChartShapeProperties();
			A.NoFill noFill5 = new A.NoFill();

			A.Outline outline15 = new A.Outline();
			A.NoFill noFill6 = new A.NoFill();

			outline15.Append(noFill6);
			A.EffectList effectList15 = new A.EffectList();

			chartShapeProperties12.Append(noFill5);
			chartShapeProperties12.Append(outline15);
			chartShapeProperties12.Append(effectList15);

			C.TextProperties textProperties2 = new C.TextProperties();
			A.BodyProperties bodyProperties3 = new A.BodyProperties() { Rotation = -5400000, UseParagraphSpacing = true, VerticalOverflow = A.TextVerticalOverflowValues.Ellipsis, Vertical = A.TextVerticalValues.Horizontal, Wrap = A.TextWrappingValues.Square, Anchor = A.TextAnchoringTypeValues.Center, AnchorCenter = true };
			A.ListStyle listStyle3 = new A.ListStyle();

			A.Paragraph paragraph3 = new A.Paragraph();

			A.ParagraphProperties paragraphProperties3 = new A.ParagraphProperties();

			A.DefaultRunProperties defaultRunProperties3 = new A.DefaultRunProperties() { FontSize = 1000, Bold = false, Italic = false, Underline = A.TextUnderlineValues.None, Strike = A.TextStrikeValues.NoStrike, Kerning = 1200, Baseline = 0 };

			A.SolidFill solidFill22 = new A.SolidFill();

			A.SchemeColor schemeColor28 = new A.SchemeColor() { Val = A.SchemeColorValues.Text1 };
			A.LuminanceModulation luminanceModulation5 = new A.LuminanceModulation() { Val = 65000 };
			A.LuminanceOffset luminanceOffset5 = new A.LuminanceOffset() { Val = 35000 };

			schemeColor28.Append(luminanceModulation5);
			schemeColor28.Append(luminanceOffset5);

			solidFill22.Append(schemeColor28);
			A.LatinFont latinFont5 = new A.LatinFont() { Typeface = "+mn-lt" };
			A.EastAsianFont eastAsianFont5 = new A.EastAsianFont() { Typeface = "+mn-ea" };
			A.ComplexScriptFont complexScriptFont5 = new A.ComplexScriptFont() { Typeface = "+mn-cs" };

			defaultRunProperties3.Append(solidFill22);
			defaultRunProperties3.Append(latinFont5);
			defaultRunProperties3.Append(eastAsianFont5);
			defaultRunProperties3.Append(complexScriptFont5);

			paragraphProperties3.Append(defaultRunProperties3);
			A.EndParagraphRunProperties endParagraphRunProperties2 = new A.EndParagraphRunProperties() { Language = "en-US" };

			paragraph3.Append(paragraphProperties3);
			paragraph3.Append(endParagraphRunProperties2);

			textProperties2.Append(bodyProperties3);
			textProperties2.Append(listStyle3);
			textProperties2.Append(paragraph3);

			title1.Append(chartText1);
			title1.Append(layout2);
			title1.Append(overlay1);
			title1.Append(chartShapeProperties12);
			title1.Append(textProperties2);
			C.NumberingFormat numberingFormat2 = new C.NumberingFormat() { FormatCode = "General", SourceLinked = true };
			C.MajorTickMark majorTickMark2 = new C.MajorTickMark() { Val = C.TickMarkValues.None };
			C.MinorTickMark minorTickMark2 = new C.MinorTickMark() { Val = C.TickMarkValues.None };
			C.TickLabelPosition tickLabelPosition2 = new C.TickLabelPosition() { Val = C.TickLabelPositionValues.NextTo };

			C.ChartShapeProperties chartShapeProperties13 = new C.ChartShapeProperties();
			A.NoFill noFill7 = new A.NoFill();

			A.Outline outline16 = new A.Outline();
			A.NoFill noFill8 = new A.NoFill();

			outline16.Append(noFill8);
			A.EffectList effectList16 = new A.EffectList();

			chartShapeProperties13.Append(noFill7);
			chartShapeProperties13.Append(outline16);
			chartShapeProperties13.Append(effectList16);

			C.TextProperties textProperties3 = new C.TextProperties();
			A.BodyProperties bodyProperties4 = new A.BodyProperties() { Rotation = -60000000, UseParagraphSpacing = true, VerticalOverflow = A.TextVerticalOverflowValues.Ellipsis, Vertical = A.TextVerticalValues.Horizontal, Wrap = A.TextWrappingValues.Square, Anchor = A.TextAnchoringTypeValues.Center, AnchorCenter = true };
			A.ListStyle listStyle4 = new A.ListStyle();

			A.Paragraph paragraph4 = new A.Paragraph();

			A.ParagraphProperties paragraphProperties4 = new A.ParagraphProperties();

			A.DefaultRunProperties defaultRunProperties4 = new A.DefaultRunProperties() { FontSize = 900, Bold = false, Italic = false, Underline = A.TextUnderlineValues.None, Strike = A.TextStrikeValues.NoStrike, Kerning = 1200, Baseline = 0 };

			A.SolidFill solidFill23 = new A.SolidFill();

			A.SchemeColor schemeColor29 = new A.SchemeColor() { Val = A.SchemeColorValues.Text1 };
			A.LuminanceModulation luminanceModulation6 = new A.LuminanceModulation() { Val = 65000 };
			A.LuminanceOffset luminanceOffset6 = new A.LuminanceOffset() { Val = 35000 };

			schemeColor29.Append(luminanceModulation6);
			schemeColor29.Append(luminanceOffset6);

			solidFill23.Append(schemeColor29);
			A.LatinFont latinFont6 = new A.LatinFont() { Typeface = "+mn-lt" };
			A.EastAsianFont eastAsianFont6 = new A.EastAsianFont() { Typeface = "+mn-ea" };
			A.ComplexScriptFont complexScriptFont6 = new A.ComplexScriptFont() { Typeface = "+mn-cs" };

			defaultRunProperties4.Append(solidFill23);
			defaultRunProperties4.Append(latinFont6);
			defaultRunProperties4.Append(eastAsianFont6);
			defaultRunProperties4.Append(complexScriptFont6);

			paragraphProperties4.Append(defaultRunProperties4);
			A.EndParagraphRunProperties endParagraphRunProperties3 = new A.EndParagraphRunProperties() { Language = "en-US" };

			paragraph4.Append(paragraphProperties4);
			paragraph4.Append(endParagraphRunProperties3);

			textProperties3.Append(bodyProperties4);
			textProperties3.Append(listStyle4);
			textProperties3.Append(paragraph4);
			C.CrossingAxis crossingAxis2 = new C.CrossingAxis() { Val = new UInt32Value() { InnerText = "-336834096" } };
			C.Crosses crosses2 = new C.Crosses() { Val = C.CrossesValues.AutoZero };
			C.CrossBetween crossBetween1 = new C.CrossBetween() { Val = C.CrossBetweenValues.Between };

			valueAxis1.Append(axisId6);
			valueAxis1.Append(scaling2);
			valueAxis1.Append(delete2);
			valueAxis1.Append(axisPosition2);
			valueAxis1.Append(majorGridlines1);
			valueAxis1.Append(title1);
			valueAxis1.Append(numberingFormat2);
			valueAxis1.Append(majorTickMark2);
			valueAxis1.Append(minorTickMark2);
			valueAxis1.Append(tickLabelPosition2);
			valueAxis1.Append(chartShapeProperties13);
			valueAxis1.Append(textProperties3);
			valueAxis1.Append(crossingAxis2);
			valueAxis1.Append(crosses2);
			valueAxis1.Append(crossBetween1);

			C.ValueAxis valueAxis2 = new C.ValueAxis();
			C.AxisId axisId7 = new C.AxisId() { Val = new UInt32Value() { InnerText = "-162419408" } };

			C.Scaling scaling3 = new C.Scaling();
			C.Orientation orientation3 = new C.Orientation() { Val = C.OrientationValues.MinMax };

			scaling3.Append(orientation3);
			C.Delete delete3 = new C.Delete() { Val = false };
			C.AxisPosition axisPosition3 = new C.AxisPosition() { Val = C.AxisPositionValues.Right };

			C.Title title2 = new C.Title();

			C.ChartText chartText2 = new C.ChartText();

			C.RichText richText2 = new C.RichText();
			A.BodyProperties bodyProperties5 = new A.BodyProperties() { Rotation = -5400000, UseParagraphSpacing = true, VerticalOverflow = A.TextVerticalOverflowValues.Ellipsis, Vertical = A.TextVerticalValues.Horizontal, Wrap = A.TextWrappingValues.Square, Anchor = A.TextAnchoringTypeValues.Center, AnchorCenter = true };
			A.ListStyle listStyle5 = new A.ListStyle();

			A.Paragraph paragraph5 = new A.Paragraph();

			A.ParagraphProperties paragraphProperties5 = new A.ParagraphProperties();

			A.DefaultRunProperties defaultRunProperties5 = new A.DefaultRunProperties() { FontSize = 1000, Bold = false, Italic = false, Underline = A.TextUnderlineValues.None, Strike = A.TextStrikeValues.NoStrike, Kerning = 1200, Baseline = 0 };

			A.SolidFill solidFill24 = new A.SolidFill();

			A.SchemeColor schemeColor30 = new A.SchemeColor() { Val = A.SchemeColorValues.Text1 };
			A.LuminanceModulation luminanceModulation7 = new A.LuminanceModulation() { Val = 65000 };
			A.LuminanceOffset luminanceOffset7 = new A.LuminanceOffset() { Val = 35000 };

			schemeColor30.Append(luminanceModulation7);
			schemeColor30.Append(luminanceOffset7);

			solidFill24.Append(schemeColor30);
			A.LatinFont latinFont7 = new A.LatinFont() { Typeface = "+mn-lt" };
			A.EastAsianFont eastAsianFont7 = new A.EastAsianFont() { Typeface = "+mn-ea" };
			A.ComplexScriptFont complexScriptFont7 = new A.ComplexScriptFont() { Typeface = "+mn-cs" };

			defaultRunProperties5.Append(solidFill24);
			defaultRunProperties5.Append(latinFont7);
			defaultRunProperties5.Append(eastAsianFont7);
			defaultRunProperties5.Append(complexScriptFont7);

			paragraphProperties5.Append(defaultRunProperties5);

			A.Run run2 = new A.Run();
			A.RunProperties runProperties2 = new A.RunProperties() { Language = "en-US" };
			A.Text text2 = new A.Text();
			text2.Text = "Count";

			run2.Append(runProperties2);
			run2.Append(text2);

			paragraph5.Append(paragraphProperties5);
			paragraph5.Append(run2);

			richText2.Append(bodyProperties5);
			richText2.Append(listStyle5);
			richText2.Append(paragraph5);

			chartText2.Append(richText2);
			C.Layout layout3 = new C.Layout();
			C.Overlay overlay2 = new C.Overlay() { Val = false };

			C.ChartShapeProperties chartShapeProperties14 = new C.ChartShapeProperties();
			A.NoFill noFill9 = new A.NoFill();

			A.Outline outline17 = new A.Outline();
			A.NoFill noFill10 = new A.NoFill();

			outline17.Append(noFill10);
			A.EffectList effectList17 = new A.EffectList();

			chartShapeProperties14.Append(noFill9);
			chartShapeProperties14.Append(outline17);
			chartShapeProperties14.Append(effectList17);

			C.TextProperties textProperties4 = new C.TextProperties();
			A.BodyProperties bodyProperties6 = new A.BodyProperties() { Rotation = -5400000, UseParagraphSpacing = true, VerticalOverflow = A.TextVerticalOverflowValues.Ellipsis, Vertical = A.TextVerticalValues.Horizontal, Wrap = A.TextWrappingValues.Square, Anchor = A.TextAnchoringTypeValues.Center, AnchorCenter = true };
			A.ListStyle listStyle6 = new A.ListStyle();

			A.Paragraph paragraph6 = new A.Paragraph();

			A.ParagraphProperties paragraphProperties6 = new A.ParagraphProperties();

			A.DefaultRunProperties defaultRunProperties6 = new A.DefaultRunProperties() { FontSize = 1000, Bold = false, Italic = false, Underline = A.TextUnderlineValues.None, Strike = A.TextStrikeValues.NoStrike, Kerning = 1200, Baseline = 0 };

			A.SolidFill solidFill25 = new A.SolidFill();

			A.SchemeColor schemeColor31 = new A.SchemeColor() { Val = A.SchemeColorValues.Text1 };
			A.LuminanceModulation luminanceModulation8 = new A.LuminanceModulation() { Val = 65000 };
			A.LuminanceOffset luminanceOffset8 = new A.LuminanceOffset() { Val = 35000 };

			schemeColor31.Append(luminanceModulation8);
			schemeColor31.Append(luminanceOffset8);

			solidFill25.Append(schemeColor31);
			A.LatinFont latinFont8 = new A.LatinFont() { Typeface = "+mn-lt" };
			A.EastAsianFont eastAsianFont8 = new A.EastAsianFont() { Typeface = "+mn-ea" };
			A.ComplexScriptFont complexScriptFont8 = new A.ComplexScriptFont() { Typeface = "+mn-cs" };

			defaultRunProperties6.Append(solidFill25);
			defaultRunProperties6.Append(latinFont8);
			defaultRunProperties6.Append(eastAsianFont8);
			defaultRunProperties6.Append(complexScriptFont8);

			paragraphProperties6.Append(defaultRunProperties6);
			A.EndParagraphRunProperties endParagraphRunProperties4 = new A.EndParagraphRunProperties() { Language = "en-US" };

			paragraph6.Append(paragraphProperties6);
			paragraph6.Append(endParagraphRunProperties4);

			textProperties4.Append(bodyProperties6);
			textProperties4.Append(listStyle6);
			textProperties4.Append(paragraph6);

			title2.Append(chartText2);
			title2.Append(layout3);
			title2.Append(overlay2);
			title2.Append(chartShapeProperties14);
			title2.Append(textProperties4);
			C.NumberingFormat numberingFormat3 = new C.NumberingFormat() { FormatCode = "General", SourceLinked = true };
			C.MajorTickMark majorTickMark3 = new C.MajorTickMark() { Val = C.TickMarkValues.Outside };
			C.MinorTickMark minorTickMark3 = new C.MinorTickMark() { Val = C.TickMarkValues.None };
			C.TickLabelPosition tickLabelPosition3 = new C.TickLabelPosition() { Val = C.TickLabelPositionValues.NextTo };

			C.ChartShapeProperties chartShapeProperties15 = new C.ChartShapeProperties();
			A.NoFill noFill11 = new A.NoFill();

			A.Outline outline18 = new A.Outline();
			A.NoFill noFill12 = new A.NoFill();

			outline18.Append(noFill12);
			A.EffectList effectList18 = new A.EffectList();

			chartShapeProperties15.Append(noFill11);
			chartShapeProperties15.Append(outline18);
			chartShapeProperties15.Append(effectList18);

			C.TextProperties textProperties5 = new C.TextProperties();
			A.BodyProperties bodyProperties7 = new A.BodyProperties() { Rotation = -60000000, UseParagraphSpacing = true, VerticalOverflow = A.TextVerticalOverflowValues.Ellipsis, Vertical = A.TextVerticalValues.Horizontal, Wrap = A.TextWrappingValues.Square, Anchor = A.TextAnchoringTypeValues.Center, AnchorCenter = true };
			A.ListStyle listStyle7 = new A.ListStyle();

			A.Paragraph paragraph7 = new A.Paragraph();

			A.ParagraphProperties paragraphProperties7 = new A.ParagraphProperties();

			A.DefaultRunProperties defaultRunProperties7 = new A.DefaultRunProperties() { FontSize = 900, Bold = false, Italic = false, Underline = A.TextUnderlineValues.None, Strike = A.TextStrikeValues.NoStrike, Kerning = 1200, Baseline = 0 };

			A.SolidFill solidFill26 = new A.SolidFill();

			A.SchemeColor schemeColor32 = new A.SchemeColor() { Val = A.SchemeColorValues.Text1 };
			A.LuminanceModulation luminanceModulation9 = new A.LuminanceModulation() { Val = 65000 };
			A.LuminanceOffset luminanceOffset9 = new A.LuminanceOffset() { Val = 35000 };

			schemeColor32.Append(luminanceModulation9);
			schemeColor32.Append(luminanceOffset9);

			solidFill26.Append(schemeColor32);
			A.LatinFont latinFont9 = new A.LatinFont() { Typeface = "+mn-lt" };
			A.EastAsianFont eastAsianFont9 = new A.EastAsianFont() { Typeface = "+mn-ea" };
			A.ComplexScriptFont complexScriptFont9 = new A.ComplexScriptFont() { Typeface = "+mn-cs" };

			defaultRunProperties7.Append(solidFill26);
			defaultRunProperties7.Append(latinFont9);
			defaultRunProperties7.Append(eastAsianFont9);
			defaultRunProperties7.Append(complexScriptFont9);

			paragraphProperties7.Append(defaultRunProperties7);
			A.EndParagraphRunProperties endParagraphRunProperties5 = new A.EndParagraphRunProperties() { Language = "en-US" };

			paragraph7.Append(paragraphProperties7);
			paragraph7.Append(endParagraphRunProperties5);

			textProperties5.Append(bodyProperties7);
			textProperties5.Append(listStyle7);
			textProperties5.Append(paragraph7);
			C.CrossingAxis crossingAxis3 = new C.CrossingAxis() { Val = new UInt32Value() { InnerText = "-162426480" } };
			C.Crosses crosses3 = new C.Crosses() { Val = C.CrossesValues.Maximum };
			C.CrossBetween crossBetween2 = new C.CrossBetween() { Val = C.CrossBetweenValues.Between };

			valueAxis2.Append(axisId7);
			valueAxis2.Append(scaling3);
			valueAxis2.Append(delete3);
			valueAxis2.Append(axisPosition3);
			valueAxis2.Append(title2);
			valueAxis2.Append(numberingFormat3);
			valueAxis2.Append(majorTickMark3);
			valueAxis2.Append(minorTickMark3);
			valueAxis2.Append(tickLabelPosition3);
			valueAxis2.Append(chartShapeProperties15);
			valueAxis2.Append(textProperties5);
			valueAxis2.Append(crossingAxis3);
			valueAxis2.Append(crosses3);
			valueAxis2.Append(crossBetween2);

			C.DateAxis dateAxis2 = new C.DateAxis();
			C.AxisId axisId8 = new C.AxisId() { Val = new UInt32Value() { InnerText = "-162426480" } };

			C.Scaling scaling4 = new C.Scaling();
			C.Orientation orientation4 = new C.Orientation() { Val = C.OrientationValues.MinMax };

			scaling4.Append(orientation4);
			C.Delete delete4 = new C.Delete() { Val = true };
			C.AxisPosition axisPosition4 = new C.AxisPosition() { Val = C.AxisPositionValues.Bottom };
			C.NumberingFormat numberingFormat4 = new C.NumberingFormat() { FormatCode = "m/d/yyyy", SourceLinked = true };
			C.MajorTickMark majorTickMark4 = new C.MajorTickMark() { Val = C.TickMarkValues.Outside };
			C.MinorTickMark minorTickMark4 = new C.MinorTickMark() { Val = C.TickMarkValues.None };
			C.TickLabelPosition tickLabelPosition4 = new C.TickLabelPosition() { Val = C.TickLabelPositionValues.NextTo };
			C.CrossingAxis crossingAxis4 = new C.CrossingAxis() { Val = new UInt32Value() { InnerText = "-162419408" } };
			C.AutoLabeled autoLabeled2 = new C.AutoLabeled() { Val = true };
			C.LabelOffset labelOffset2 = new C.LabelOffset() { Val = (UInt16Value)100U };
			C.BaseTimeUnit baseTimeUnit2 = new C.BaseTimeUnit() { Val = C.TimeUnitValues.Days };

			dateAxis2.Append(axisId8);
			dateAxis2.Append(scaling4);
			dateAxis2.Append(delete4);
			dateAxis2.Append(axisPosition4);
			dateAxis2.Append(numberingFormat4);
			dateAxis2.Append(majorTickMark4);
			dateAxis2.Append(minorTickMark4);
			dateAxis2.Append(tickLabelPosition4);
			dateAxis2.Append(crossingAxis4);
			dateAxis2.Append(autoLabeled2);
			dateAxis2.Append(labelOffset2);
			dateAxis2.Append(baseTimeUnit2);

			C.ShapeProperties shapeProperties1 = new C.ShapeProperties();
			A.NoFill noFill13 = new A.NoFill();

			A.Outline outline19 = new A.Outline();
			A.NoFill noFill14 = new A.NoFill();

			outline19.Append(noFill14);
			A.EffectList effectList19 = new A.EffectList();

			shapeProperties1.Append(noFill13);
			shapeProperties1.Append(outline19);
			shapeProperties1.Append(effectList19);

			plotArea1.Append(layout1);
			plotArea1.Append(barChart1);
			plotArea1.Append(lineChart1);
			plotArea1.Append(dateAxis1);
			plotArea1.Append(valueAxis1);
			plotArea1.Append(valueAxis2);
			plotArea1.Append(dateAxis2);
			plotArea1.Append(shapeProperties1);

			C.Legend legend1 = new C.Legend();
			C.LegendPosition legendPosition1 = new C.LegendPosition() { Val = C.LegendPositionValues.Bottom };

			C.LegendEntry legendEntry1 = new C.LegendEntry();
			C.Index index7 = new C.Index() { Val = (UInt32Value)3U };
			C.Delete delete5 = new C.Delete() { Val = true };

			legendEntry1.Append(index7);
			legendEntry1.Append(delete5);

			C.LegendEntry legendEntry2 = new C.LegendEntry();
			C.Index index8 = new C.Index() { Val = (UInt32Value)4U };
			C.Delete delete6 = new C.Delete() { Val = true };

			legendEntry2.Append(index8);
			legendEntry2.Append(delete6);

			C.LegendEntry legendEntry3 = new C.LegendEntry();
			C.Index index9 = new C.Index() { Val = (UInt32Value)5U };
			C.Delete delete7 = new C.Delete() { Val = true };

			legendEntry3.Append(index9);
			legendEntry3.Append(delete7);
			C.Layout layout4 = new C.Layout();
			C.Overlay overlay3 = new C.Overlay() { Val = false };

			C.ChartShapeProperties chartShapeProperties16 = new C.ChartShapeProperties();
			A.NoFill noFill15 = new A.NoFill();

			A.Outline outline20 = new A.Outline();
			A.NoFill noFill16 = new A.NoFill();

			outline20.Append(noFill16);
			A.EffectList effectList20 = new A.EffectList();

			chartShapeProperties16.Append(noFill15);
			chartShapeProperties16.Append(outline20);
			chartShapeProperties16.Append(effectList20);

			C.TextProperties textProperties6 = new C.TextProperties();
			A.BodyProperties bodyProperties8 = new A.BodyProperties() { Rotation = 0, UseParagraphSpacing = true, VerticalOverflow = A.TextVerticalOverflowValues.Ellipsis, Vertical = A.TextVerticalValues.Horizontal, Wrap = A.TextWrappingValues.Square, Anchor = A.TextAnchoringTypeValues.Center, AnchorCenter = true };
			A.ListStyle listStyle8 = new A.ListStyle();

			A.Paragraph paragraph8 = new A.Paragraph();

			A.ParagraphProperties paragraphProperties8 = new A.ParagraphProperties();

			A.DefaultRunProperties defaultRunProperties8 = new A.DefaultRunProperties() { FontSize = 900, Bold = false, Italic = false, Underline = A.TextUnderlineValues.None, Strike = A.TextStrikeValues.NoStrike, Kerning = 1200, Baseline = 0 };

			A.SolidFill solidFill27 = new A.SolidFill();

			A.SchemeColor schemeColor33 = new A.SchemeColor() { Val = A.SchemeColorValues.Text1 };
			A.LuminanceModulation luminanceModulation10 = new A.LuminanceModulation() { Val = 65000 };
			A.LuminanceOffset luminanceOffset10 = new A.LuminanceOffset() { Val = 35000 };

			schemeColor33.Append(luminanceModulation10);
			schemeColor33.Append(luminanceOffset10);

			solidFill27.Append(schemeColor33);
			A.LatinFont latinFont10 = new A.LatinFont() { Typeface = "+mn-lt" };
			A.EastAsianFont eastAsianFont10 = new A.EastAsianFont() { Typeface = "+mn-ea" };
			A.ComplexScriptFont complexScriptFont10 = new A.ComplexScriptFont() { Typeface = "+mn-cs" };

			defaultRunProperties8.Append(solidFill27);
			defaultRunProperties8.Append(latinFont10);
			defaultRunProperties8.Append(eastAsianFont10);
			defaultRunProperties8.Append(complexScriptFont10);

			paragraphProperties8.Append(defaultRunProperties8);
			A.EndParagraphRunProperties endParagraphRunProperties6 = new A.EndParagraphRunProperties() { Language = "en-US" };

			paragraph8.Append(paragraphProperties8);
			paragraph8.Append(endParagraphRunProperties6);

			textProperties6.Append(bodyProperties8);
			textProperties6.Append(listStyle8);
			textProperties6.Append(paragraph8);

			legend1.Append(legendPosition1);
			legend1.Append(legendEntry1);
			legend1.Append(legendEntry2);
			legend1.Append(legendEntry3);
			legend1.Append(layout4);
			legend1.Append(overlay3);
			legend1.Append(chartShapeProperties16);
			legend1.Append(textProperties6);
			C.PlotVisibleOnly plotVisibleOnly1 = new C.PlotVisibleOnly() { Val = true };
			C.DisplayBlanksAs displayBlanksAs1 = new C.DisplayBlanksAs() { Val = C.DisplayBlanksAsValues.Gap };
			C.ShowDataLabelsOverMaximum showDataLabelsOverMaximum1 = new C.ShowDataLabelsOverMaximum() { Val = false };

			chart1.Append(autoTitleDeleted1);
			chart1.Append(plotArea1);
			chart1.Append(legend1);
			chart1.Append(plotVisibleOnly1);
			chart1.Append(displayBlanksAs1);
			chart1.Append(showDataLabelsOverMaximum1);

			C.ShapeProperties shapeProperties2 = new C.ShapeProperties();

			A.SolidFill solidFill28 = new A.SolidFill();
			A.SchemeColor schemeColor34 = new A.SchemeColor() { Val = A.SchemeColorValues.Background1 };

			solidFill28.Append(schemeColor34);

			A.Outline outline21 = new A.Outline() { Width = 9525, CapType = A.LineCapValues.Flat, CompoundLineType = A.CompoundLineValues.Single, Alignment = A.PenAlignmentValues.Center };

			A.SolidFill solidFill29 = new A.SolidFill();

			A.SchemeColor schemeColor35 = new A.SchemeColor() { Val = A.SchemeColorValues.Text1 };
			A.LuminanceModulation luminanceModulation11 = new A.LuminanceModulation() { Val = 15000 };
			A.LuminanceOffset luminanceOffset11 = new A.LuminanceOffset() { Val = 85000 };

			schemeColor35.Append(luminanceModulation11);
			schemeColor35.Append(luminanceOffset11);

			solidFill29.Append(schemeColor35);
			A.Round round6 = new A.Round();

			outline21.Append(solidFill29);
			outline21.Append(round6);
			A.EffectList effectList21 = new A.EffectList();

			shapeProperties2.Append(solidFill28);
			shapeProperties2.Append(outline21);
			shapeProperties2.Append(effectList21);

			C.TextProperties textProperties7 = new C.TextProperties();
			A.BodyProperties bodyProperties9 = new A.BodyProperties();
			A.ListStyle listStyle9 = new A.ListStyle();

			A.Paragraph paragraph9 = new A.Paragraph();

			A.ParagraphProperties paragraphProperties9 = new A.ParagraphProperties();
			A.DefaultRunProperties defaultRunProperties9 = new A.DefaultRunProperties();

			paragraphProperties9.Append(defaultRunProperties9);
			A.EndParagraphRunProperties endParagraphRunProperties7 = new A.EndParagraphRunProperties() { Language = "en-US" };

			paragraph9.Append(paragraphProperties9);
			paragraph9.Append(endParagraphRunProperties7);

			textProperties7.Append(bodyProperties9);
			textProperties7.Append(listStyle9);
			textProperties7.Append(paragraph9);

			C.PrintSettings printSettings1 = new C.PrintSettings();
			C.HeaderFooter headerFooter1 = new C.HeaderFooter();
			C.PageMargins pageMargins2 = new C.PageMargins() { Left = 0.7D, Right = 0.7D, Top = 0.75D, Bottom = 0.75D, Header = 0.3D, Footer = 0.3D };
			C.PageSetup pageSetup2 = new C.PageSetup();

			printSettings1.Append(headerFooter1);
			printSettings1.Append(pageMargins2);
			printSettings1.Append(pageSetup2);

			chartSpace1.Append(date19041);
			chartSpace1.Append(editingLanguage1);
			chartSpace1.Append(roundedCorners1);
			chartSpace1.Append(alternateContent1);
			chartSpace1.Append(chart1);
			chartSpace1.Append(shapeProperties2);
			chartSpace1.Append(textProperties7);
			chartSpace1.Append(printSettings1);

			chartPart1.ChartSpace = chartSpace1;
		}

		private BarChart GenerateBarChart()
		{
			BarChart barChart = new BarChart();

			BarDirection barDirection = new BarDirection() { Val = BarDirectionValues.Column };
			barChart.Append(barDirection);

			BarGrouping barGrouping = new BarGrouping() { Val = BarGroupingValues.Clustered };
			barChart.Append(barGrouping);

			VaryColors varyColors = new VaryColors() { Val = false };
			barChart.Append(varyColors);

			//barChartSeries1, start
			BarChartSeries barChartSeries = new BarChartSeries();

			Index index = new C.Index() { Val = (UInt32Value)0U };
			barChartSeries.Append(index);

			Order order = new C.Order() { Val = (UInt32Value)0U };
			barChartSeries.Append(order);

			//seriesText1, start
			SeriesText seriesText = new SeriesText();
			StringReference stringReference = new StringReference();

			C.Formula formula = new C.Formula();
			formula.Text = "Sheet1!$B$1:$B$2";
			stringReference.Append(formula);

			C.StringCache stringCache = new C.StringCache();
			C.PointCount pointCount = new C.PointCount() { Val = (UInt32Value)2U };
			stringCache.Append(pointCount);

			C.StringPoint stringPoint = new C.StringPoint() { Index = (UInt32Value)0U };
			C.NumericValue numericValue = new C.NumericValue();
			numericValue.Text = "I3DVR";
			stringPoint.Append(numericValue);
			stringCache.Append(stringPoint);

			stringPoint = new C.StringPoint() { Index = (UInt32Value)1U };
			numericValue = new C.NumericValue();
			numericValue.Text = "Count";
			stringPoint.Append(numericValue);
			stringCache.Append(stringPoint);

			stringReference.Append(stringCache);
			seriesText.Append(stringReference);
			barChartSeries.Append(seriesText);
			//seriesText1, end

			C.ChartShapeProperties chartShapeProperties = new C.ChartShapeProperties();
			A.SolidFill solidFill = new A.SolidFill();
			chartShapeProperties.Append(solidFill);
			A.RgbColorModelHex rgbColorModelHex = new A.RgbColorModelHex() { Val = "0070C0" };
			solidFill.Append(rgbColorModelHex);
			A.Outline outline = new A.Outline();
			A.NoFill noFill = new A.NoFill();
			outline.Append(noFill);
			chartShapeProperties.Append(outline);
			A.EffectList effectList = new A.EffectList();
			chartShapeProperties.Append(effectList);

			barChartSeries.Append(chartShapeProperties);

			C.InvertIfNegative invertIfNegative = new C.InvertIfNegative() { Val = false };
			barChartSeries.Append(invertIfNegative);

			//categoryAxisData1, begin
			C.CategoryAxisData categoryAxisData = new C.CategoryAxisData();
			C.NumberReference numberReference = new C.NumberReference();

			formula = new C.Formula();
			formula.Text = "Sheet1!$A$3:$A$6";
			numberReference.Append(formula);

			C.NumberingCache numberingCache = new C.NumberingCache();

			C.FormatCode formatCode = new C.FormatCode();
			formatCode.Text = "m/d/yyyy";
			numberingCache.Append(formatCode);

			pointCount = new C.PointCount() { Val = (UInt32Value)4U };
			numberingCache.Append(pointCount);

			C.NumericPoint numericPoint = new C.NumericPoint() { Index = (UInt32Value)0U };
			numericValue = new C.NumericValue();
			numericValue.Text = "42372";
			numericPoint.Append(numericValue);
			numberingCache.Append(numericPoint);

			numericPoint = new C.NumericPoint() { Index = (UInt32Value)1U };
			numericValue = new C.NumericValue();
			numericValue.Text = "42373";
			numericPoint.Append(numericValue);
			numberingCache.Append(numericPoint);

			numericPoint = new C.NumericPoint() { Index = (UInt32Value)2U };
			numericValue = new C.NumericValue();
			numericValue.Text = "42374";
			numericPoint.Append(numericValue);
			numberingCache.Append(numericPoint);

			numericPoint = new C.NumericPoint() { Index = (UInt32Value)3U };
			numericValue = new C.NumericValue();
			numericValue.Text = "42375";
			numericPoint.Append(numericValue);
			numberingCache.Append(numericPoint);

			numberReference.Append(numberingCache);

			categoryAxisData.Append(numberReference);
			//categoryAxisData1, end
			barChartSeries.Append(categoryAxisData);


			C.Values values1 = new C.Values();

			C.NumberReference numberReference2 = new C.NumberReference();
			C.Formula formula3 = new C.Formula();
			formula3.Text = "Sheet1!$B$3:$B$6";

			C.NumberingCache numberingCache2 = new C.NumberingCache();
			C.FormatCode formatCode2 = new C.FormatCode();
			formatCode2.Text = "General";
			C.PointCount pointCount3 = new C.PointCount() { Val = (UInt32Value)4U };

			C.NumericPoint numericPoint5 = new C.NumericPoint() { Index = (UInt32Value)0U };
			C.NumericValue numericValue7 = new C.NumericValue();
			numericValue7.Text = "45";

			numericPoint5.Append(numericValue7);

			C.NumericPoint numericPoint6 = new C.NumericPoint() { Index = (UInt32Value)1U };
			C.NumericValue numericValue8 = new C.NumericValue();
			numericValue8.Text = "54";

			numericPoint6.Append(numericValue8);

			C.NumericPoint numericPoint7 = new C.NumericPoint() { Index = (UInt32Value)2U };
			C.NumericValue numericValue9 = new C.NumericValue();
			numericValue9.Text = "25";

			numericPoint7.Append(numericValue9);

			C.NumericPoint numericPoint8 = new C.NumericPoint() { Index = (UInt32Value)3U };
			C.NumericValue numericValue10 = new C.NumericValue();
			numericValue10.Text = "35";

			numericPoint8.Append(numericValue10);

			numberingCache2.Append(formatCode2);
			numberingCache2.Append(pointCount3);
			numberingCache2.Append(numericPoint5);
			numberingCache2.Append(numericPoint6);
			numberingCache2.Append(numericPoint7);
			numberingCache2.Append(numericPoint8);

			numberReference2.Append(formula3);
			numberReference2.Append(numberingCache2);

			values1.Append(numberReference2);







			barChartSeries.Append(values1);

			barChart.Append(barChartSeries);
			//barChartSeries1, end

			C.BarChartSeries barChartSeries2 = new C.BarChartSeries();
			C.Index index2 = new C.Index() { Val = (UInt32Value)2U };
			C.Order order2 = new C.Order() { Val = (UInt32Value)2U };

			C.SeriesText seriesText2 = new C.SeriesText();

			C.StringReference stringReference2 = new C.StringReference();
			C.Formula formula4 = new C.Formula();
			formula4.Text = "Sheet1!$D$1:$D$2";

			C.StringCache stringCache2 = new C.StringCache();
			C.PointCount pointCount4 = new C.PointCount() { Val = (UInt32Value)2U };

			C.StringPoint stringPoint3 = new C.StringPoint() { Index = (UInt32Value)0U };
			C.NumericValue numericValue11 = new C.NumericValue();
			numericValue11.Text = "East";

			stringPoint3.Append(numericValue11);

			C.StringPoint stringPoint4 = new C.StringPoint() { Index = (UInt32Value)1U };
			C.NumericValue numericValue12 = new C.NumericValue();
			numericValue12.Text = "Count";

			stringPoint4.Append(numericValue12);

			stringCache2.Append(pointCount4);
			stringCache2.Append(stringPoint3);
			stringCache2.Append(stringPoint4);

			stringReference2.Append(formula4);
			stringReference2.Append(stringCache2);

			seriesText2.Append(stringReference2);

			C.ChartShapeProperties chartShapeProperties2 = new C.ChartShapeProperties();

			A.SolidFill solidFill7 = new A.SolidFill();
			A.RgbColorModelHex rgbColorModelHex15 = new A.RgbColorModelHex() { Val = "00B050" };

			solidFill7.Append(rgbColorModelHex15);

			A.Outline outline5 = new A.Outline();
			A.NoFill noFill2 = new A.NoFill();

			outline5.Append(noFill2);
			A.EffectList effectList5 = new A.EffectList();

			chartShapeProperties2.Append(solidFill7);
			chartShapeProperties2.Append(outline5);
			chartShapeProperties2.Append(effectList5);
			C.InvertIfNegative invertIfNegative2 = new C.InvertIfNegative() { Val = false };

			C.CategoryAxisData categoryAxisData2 = new C.CategoryAxisData();

			C.NumberReference numberReference3 = new C.NumberReference();
			C.Formula formula5 = new C.Formula();
			formula5.Text = "Sheet1!$A$3:$A$6";

			C.NumberingCache numberingCache3 = new C.NumberingCache();
			C.FormatCode formatCode3 = new C.FormatCode();
			formatCode3.Text = "m/d/yyyy";
			C.PointCount pointCount5 = new C.PointCount() { Val = (UInt32Value)4U };

			C.NumericPoint numericPoint9 = new C.NumericPoint() { Index = (UInt32Value)0U };
			C.NumericValue numericValue13 = new C.NumericValue();
			numericValue13.Text = "42372";

			numericPoint9.Append(numericValue13);

			C.NumericPoint numericPoint10 = new C.NumericPoint() { Index = (UInt32Value)1U };
			C.NumericValue numericValue14 = new C.NumericValue();
			numericValue14.Text = "42373";

			numericPoint10.Append(numericValue14);

			C.NumericPoint numericPoint11 = new C.NumericPoint() { Index = (UInt32Value)2U };
			C.NumericValue numericValue15 = new C.NumericValue();
			numericValue15.Text = "42374";

			numericPoint11.Append(numericValue15);

			C.NumericPoint numericPoint12 = new C.NumericPoint() { Index = (UInt32Value)3U };
			C.NumericValue numericValue16 = new C.NumericValue();
			numericValue16.Text = "42375";

			numericPoint12.Append(numericValue16);

			numberingCache3.Append(formatCode3);
			numberingCache3.Append(pointCount5);
			numberingCache3.Append(numericPoint9);
			numberingCache3.Append(numericPoint10);
			numberingCache3.Append(numericPoint11);
			numberingCache3.Append(numericPoint12);

			numberReference3.Append(formula5);
			numberReference3.Append(numberingCache3);

			categoryAxisData2.Append(numberReference3);

			C.Values values2 = new C.Values();

			C.NumberReference numberReference4 = new C.NumberReference();
			C.Formula formula6 = new C.Formula();
			formula6.Text = "Sheet1!$D$3:$D$6";

			C.NumberingCache numberingCache4 = new C.NumberingCache();
			C.FormatCode formatCode4 = new C.FormatCode();
			formatCode4.Text = "General";
			C.PointCount pointCount6 = new C.PointCount() { Val = (UInt32Value)4U };

			C.NumericPoint numericPoint13 = new C.NumericPoint() { Index = (UInt32Value)0U };
			C.NumericValue numericValue17 = new C.NumericValue();
			numericValue17.Text = "54";

			numericPoint13.Append(numericValue17);

			C.NumericPoint numericPoint14 = new C.NumericPoint() { Index = (UInt32Value)1U };
			C.NumericValue numericValue18 = new C.NumericValue();
			numericValue18.Text = "56";

			numericPoint14.Append(numericValue18);

			C.NumericPoint numericPoint15 = new C.NumericPoint() { Index = (UInt32Value)2U };
			C.NumericValue numericValue19 = new C.NumericValue();
			numericValue19.Text = "52";

			numericPoint15.Append(numericValue19);

			C.NumericPoint numericPoint16 = new C.NumericPoint() { Index = (UInt32Value)3U };
			C.NumericValue numericValue20 = new C.NumericValue();
			numericValue20.Text = "75";

			numericPoint16.Append(numericValue20);

			numberingCache4.Append(formatCode4);
			numberingCache4.Append(pointCount6);
			numberingCache4.Append(numericPoint13);
			numberingCache4.Append(numericPoint14);
			numberingCache4.Append(numericPoint15);
			numberingCache4.Append(numericPoint16);

			numberReference4.Append(formula6);
			numberReference4.Append(numberingCache4);

			values2.Append(numberReference4);

			barChartSeries2.Append(index2);
			barChartSeries2.Append(order2);
			barChartSeries2.Append(seriesText2);
			barChartSeries2.Append(chartShapeProperties2);
			barChartSeries2.Append(invertIfNegative2);
			barChartSeries2.Append(categoryAxisData2);
			barChartSeries2.Append(values2);

			C.BarChartSeries barChartSeries3 = new C.BarChartSeries();
			C.Index index3 = new C.Index() { Val = (UInt32Value)4U };
			C.Order order3 = new C.Order() { Val = (UInt32Value)4U };

			C.SeriesText seriesText3 = new C.SeriesText();

			C.StringReference stringReference3 = new C.StringReference();
			C.Formula formula7 = new C.Formula();
			formula7.Text = "Sheet1!$F$1:$F$2";

			C.StringCache stringCache3 = new C.StringCache();
			C.PointCount pointCount7 = new C.PointCount() { Val = (UInt32Value)2U };

			C.StringPoint stringPoint5 = new C.StringPoint() { Index = (UInt32Value)0U };
			C.NumericValue numericValue21 = new C.NumericValue();
			numericValue21.Text = "Site long name site";

			stringPoint5.Append(numericValue21);

			C.StringPoint stringPoint6 = new C.StringPoint() { Index = (UInt32Value)1U };
			C.NumericValue numericValue22 = new C.NumericValue();
			numericValue22.Text = "Count";

			stringPoint6.Append(numericValue22);

			stringCache3.Append(pointCount7);
			stringCache3.Append(stringPoint5);
			stringCache3.Append(stringPoint6);

			stringReference3.Append(formula7);
			stringReference3.Append(stringCache3);

			seriesText3.Append(stringReference3);

			C.ChartShapeProperties chartShapeProperties3 = new C.ChartShapeProperties();

			A.SolidFill solidFill8 = new A.SolidFill();
			A.RgbColorModelHex rgbColorModelHex16 = new A.RgbColorModelHex() { Val = "FFC000" };

			solidFill8.Append(rgbColorModelHex16);

			A.Outline outline6 = new A.Outline();
			A.NoFill noFill3 = new A.NoFill();

			outline6.Append(noFill3);
			A.EffectList effectList6 = new A.EffectList();

			chartShapeProperties3.Append(solidFill8);
			chartShapeProperties3.Append(outline6);
			chartShapeProperties3.Append(effectList6);
			C.InvertIfNegative invertIfNegative3 = new C.InvertIfNegative() { Val = false };

			C.CategoryAxisData categoryAxisData3 = new C.CategoryAxisData();

			C.NumberReference numberReference5 = new C.NumberReference();
			C.Formula formula8 = new C.Formula();
			formula8.Text = "Sheet1!$A$3:$A$6";

			C.NumberingCache numberingCache5 = new C.NumberingCache();
			C.FormatCode formatCode5 = new C.FormatCode();
			formatCode5.Text = "m/d/yyyy";
			C.PointCount pointCount8 = new C.PointCount() { Val = (UInt32Value)4U };

			C.NumericPoint numericPoint17 = new C.NumericPoint() { Index = (UInt32Value)0U };
			C.NumericValue numericValue23 = new C.NumericValue();
			numericValue23.Text = "42372";

			numericPoint17.Append(numericValue23);

			C.NumericPoint numericPoint18 = new C.NumericPoint() { Index = (UInt32Value)1U };
			C.NumericValue numericValue24 = new C.NumericValue();
			numericValue24.Text = "42373";

			numericPoint18.Append(numericValue24);

			C.NumericPoint numericPoint19 = new C.NumericPoint() { Index = (UInt32Value)2U };
			C.NumericValue numericValue25 = new C.NumericValue();
			numericValue25.Text = "42374";

			numericPoint19.Append(numericValue25);

			C.NumericPoint numericPoint20 = new C.NumericPoint() { Index = (UInt32Value)3U };
			C.NumericValue numericValue26 = new C.NumericValue();
			numericValue26.Text = "42375";

			numericPoint20.Append(numericValue26);

			numberingCache5.Append(formatCode5);
			numberingCache5.Append(pointCount8);
			numberingCache5.Append(numericPoint17);
			numberingCache5.Append(numericPoint18);
			numberingCache5.Append(numericPoint19);
			numberingCache5.Append(numericPoint20);

			numberReference5.Append(formula8);
			numberReference5.Append(numberingCache5);

			categoryAxisData3.Append(numberReference5);

			C.Values values3 = new C.Values();

			C.NumberReference numberReference6 = new C.NumberReference();
			C.Formula formula9 = new C.Formula();
			formula9.Text = "Sheet1!$F$3:$F$6";

			C.NumberingCache numberingCache6 = new C.NumberingCache();
			C.FormatCode formatCode6 = new C.FormatCode();
			formatCode6.Text = "General";
			C.PointCount pointCount9 = new C.PointCount() { Val = (UInt32Value)4U };

			C.NumericPoint numericPoint21 = new C.NumericPoint() { Index = (UInt32Value)0U };
			C.NumericValue numericValue27 = new C.NumericValue();
			numericValue27.Text = "91";

			numericPoint21.Append(numericValue27);

			C.NumericPoint numericPoint22 = new C.NumericPoint() { Index = (UInt32Value)1U };
			C.NumericValue numericValue28 = new C.NumericValue();
			numericValue28.Text = "55";

			numericPoint22.Append(numericValue28);

			C.NumericPoint numericPoint23 = new C.NumericPoint() { Index = (UInt32Value)2U };
			C.NumericValue numericValue29 = new C.NumericValue();
			numericValue29.Text = "25";

			numericPoint23.Append(numericValue29);

			C.NumericPoint numericPoint24 = new C.NumericPoint() { Index = (UInt32Value)3U };
			C.NumericValue numericValue30 = new C.NumericValue();
			numericValue30.Text = "12";

			numericPoint24.Append(numericValue30);

			numberingCache6.Append(formatCode6);
			numberingCache6.Append(pointCount9);
			numberingCache6.Append(numericPoint21);
			numberingCache6.Append(numericPoint22);
			numberingCache6.Append(numericPoint23);
			numberingCache6.Append(numericPoint24);

			numberReference6.Append(formula9);
			numberReference6.Append(numberingCache6);

			values3.Append(numberReference6);

			barChartSeries3.Append(index3);
			barChartSeries3.Append(order3);
			barChartSeries3.Append(seriesText3);
			barChartSeries3.Append(chartShapeProperties3);
			barChartSeries3.Append(invertIfNegative3);
			barChartSeries3.Append(categoryAxisData3);
			barChartSeries3.Append(values3);

			C.DataLabels dataLabels1 = new C.DataLabels();
			C.ShowLegendKey showLegendKey1 = new C.ShowLegendKey() { Val = false };
			C.ShowValue showValue1 = new C.ShowValue() { Val = false };
			C.ShowCategoryName showCategoryName1 = new C.ShowCategoryName() { Val = false };
			C.ShowSeriesName showSeriesName1 = new C.ShowSeriesName() { Val = false };
			C.ShowPercent showPercent1 = new C.ShowPercent() { Val = false };
			C.ShowBubbleSize showBubbleSize1 = new C.ShowBubbleSize() { Val = false };

			dataLabels1.Append(showLegendKey1);
			dataLabels1.Append(showValue1);
			dataLabels1.Append(showCategoryName1);
			dataLabels1.Append(showSeriesName1);
			dataLabels1.Append(showPercent1);
			dataLabels1.Append(showBubbleSize1);
			C.GapWidth gapWidth1 = new C.GapWidth() { Val = (UInt16Value)219U };
			C.AxisId axisId1 = new C.AxisId() { Val = new UInt32Value() { InnerText = "-162426480" } };
			C.AxisId axisId2 = new C.AxisId() { Val = new UInt32Value() { InnerText = "-162419408" } };





			barChart.Append(barChartSeries2);
			barChart.Append(barChartSeries3);
			barChart.Append(dataLabels1);
			barChart.Append(gapWidth1);
			barChart.Append(axisId1);
			barChart.Append(axisId2);

			return barChart;
		}
	}
}

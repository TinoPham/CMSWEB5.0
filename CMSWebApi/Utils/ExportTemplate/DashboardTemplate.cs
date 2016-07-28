using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using Ap = DocumentFormat.OpenXml.ExtendedProperties;
using Vt = DocumentFormat.OpenXml.VariantTypes;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using X14 = DocumentFormat.OpenXml.Office2010.Excel;
using A = DocumentFormat.OpenXml.Drawing;
using Xdr = DocumentFormat.OpenXml.Drawing.Spreadsheet;
using C = DocumentFormat.OpenXml.Drawing.Charts;
using C14 = DocumentFormat.OpenXml.Office2010.Drawing.Charts;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using DocumentFormat.OpenXml.Drawing;
using System.Xml.Linq;
using System.IO;


namespace CMSWebApi.Utils.ExportTemplate
{

    public class DashboardTemplate
    {
        public const string BAM_VALUE_LV1_HEADER_COLOR_BLUE_STRONG = "426D8F";
        public const string BAM_VALUE_LV_2_HEADER_COLOR_BLUE_NORMAL = "89A9C2";
        public const string BAM_VALUE_LV_3_HEADER_COLOR_BLACK_NORMAL = "434955";
        public const string BAM_VALUE_GREEN = "17B374";
        public const string BAM_VALUE_RED = "EB605B";
        public const string BAM_VALUE_GRAY = "ABB7B7";
        public const string BAM_VALUE_ORANGE = "EFAD4D";
        public const string BAM_WHITE_HEADER = "FFFFFF";
        public const string BAM_BLACK_HEADER = "000000";

        /* private font*/

        enum FONTID
        {
            FONT_STRONG_BLACK_ID = 0,
            FONT_STRONG_WHITE_ID,
            FONT_NORMAL_BLACK_ID,
            FONT_NORMAL_WHITE_ID,
        };

        enum FILL_COLOR
        {
            FILL_COLOR_HEADER_METRIC_ID = 2,
            FILL_COLOR_HEADER_CONTENT_ID,
            FILL_COLOR_HEADER_STOREGOALD_ID
        };


        public enum STYLE_INDEX
        {
            STYLE_INDEX_REPORT_DEFAULT = 0,
            STYLE_INDEX_REPORTNAME,
            STYLE_INDEX_REPORTWEEK,
            STYLE_INDEX_REPORT_METRIC_HEADER,
            STYLE_INDEX_REPORT_CONTENT_HEADER,
            STYLE_INDEX_REPORT_CONTENT_STOREGOAL_HEADER,
            STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL,
            STYLE_INDEX_REPORT_CONTENT_METRIC_DATA,
        };

        public string Export(string filePath,DashboardExportData data,string filename)
        {
              CreateSpreadsheetWorkbook(filePath,data);
              return filename; 

        }


        public void CustomInsertPicture(ref WorksheetPart wsp, ref Worksheet ws, string sImagePath)
        {
            DrawingsPart dp = wsp.AddNewPart<DrawingsPart>();
            ImagePart imgp = dp.AddImagePart(ImagePartType.Png, wsp.GetIdOfPart(dp));
            using (FileStream fs = new FileStream(sImagePath, FileMode.Open))
            {
                imgp.FeedData(fs);
            }

            DocumentFormat.OpenXml.Drawing.Spreadsheet.NonVisualDrawingProperties nvdp = new DocumentFormat.OpenXml.Drawing.Spreadsheet.NonVisualDrawingProperties();
            nvdp.Id = 1025;
            nvdp.Name = "Picture 1";
            nvdp.Description = "logo";
            DocumentFormat.OpenXml.Drawing.PictureLocks picLocks = new DocumentFormat.OpenXml.Drawing.PictureLocks();
            picLocks.NoChangeAspect = true;
            picLocks.NoChangeArrowheads = true;
            DocumentFormat.OpenXml.Drawing.Spreadsheet.NonVisualPictureDrawingProperties nvpdp = new DocumentFormat.OpenXml.Drawing.Spreadsheet.NonVisualPictureDrawingProperties();
            nvpdp.PictureLocks = picLocks;
            DocumentFormat.OpenXml.Drawing.Spreadsheet.NonVisualPictureProperties nvpp = new DocumentFormat.OpenXml.Drawing.Spreadsheet.NonVisualPictureProperties();
            nvpp.NonVisualDrawingProperties = nvdp;
            nvpp.NonVisualPictureDrawingProperties = nvpdp;

            DocumentFormat.OpenXml.Drawing.Stretch stretch = new DocumentFormat.OpenXml.Drawing.Stretch();
            stretch.FillRectangle = new DocumentFormat.OpenXml.Drawing.FillRectangle();

            DocumentFormat.OpenXml.Drawing.Spreadsheet.BlipFill blipFill = new DocumentFormat.OpenXml.Drawing.Spreadsheet.BlipFill();
            DocumentFormat.OpenXml.Drawing.Blip blip = new DocumentFormat.OpenXml.Drawing.Blip();
            blip.Embed = dp.GetIdOfPart(imgp);
            blip.CompressionState = DocumentFormat.OpenXml.Drawing.BlipCompressionValues.Print;
            blipFill.Blip = blip;
            blipFill.SourceRectangle = new DocumentFormat.OpenXml.Drawing.SourceRectangle();
            blipFill.Append(stretch);

            DocumentFormat.OpenXml.Drawing.Transform2D t2d = new DocumentFormat.OpenXml.Drawing.Transform2D();
            DocumentFormat.OpenXml.Drawing.Offset offset = new DocumentFormat.OpenXml.Drawing.Offset();
            offset.X = 0;
            offset.Y = 0;
            t2d.Offset = offset;
            System.Drawing.Bitmap bm = new System.Drawing.Bitmap(sImagePath);

            DocumentFormat.OpenXml.Drawing.Extents extents = new DocumentFormat.OpenXml.Drawing.Extents();
            extents.Cx = (long)bm.Width * (long)((float)914400 / bm.HorizontalResolution);
            extents.Cy = (long)bm.Height * (long)((float)914400 / bm.VerticalResolution);
            bm.Dispose();
            t2d.Extents = extents;
            DocumentFormat.OpenXml.Drawing.Spreadsheet.ShapeProperties sp = new DocumentFormat.OpenXml.Drawing.Spreadsheet.ShapeProperties();
            sp.BlackWhiteMode = DocumentFormat.OpenXml.Drawing.BlackWhiteModeValues.Auto;
            sp.Transform2D = t2d;
            DocumentFormat.OpenXml.Drawing.PresetGeometry prstGeom = new DocumentFormat.OpenXml.Drawing.PresetGeometry();
            prstGeom.Preset = DocumentFormat.OpenXml.Drawing.ShapeTypeValues.Rectangle;
            prstGeom.AdjustValueList = new DocumentFormat.OpenXml.Drawing.AdjustValueList();
            sp.Append(prstGeom);
            sp.Append(new DocumentFormat.OpenXml.Drawing.NoFill());

            DocumentFormat.OpenXml.Drawing.Spreadsheet.Picture picture = new DocumentFormat.OpenXml.Drawing.Spreadsheet.Picture();
            picture.NonVisualPictureProperties = nvpp;
            picture.BlipFill = blipFill;
            picture.ShapeProperties = sp;

            DocumentFormat.OpenXml.Drawing.Spreadsheet.Position pos = new DocumentFormat.OpenXml.Drawing.Spreadsheet.Position();
            pos.X = 50;
            pos.Y = 0;
            Extent ext = new Extent();
            ext.Cx = extents.Cx;
            ext.Cy = extents.Cy;
            AbsoluteAnchor anchor = new AbsoluteAnchor();
            anchor.Position = pos;
            anchor.Extent = ext;
            anchor.Append(picture);
            anchor.Append(new ClientData());
            WorksheetDrawing wsd = new WorksheetDrawing();
            wsd.Append(anchor);
            Drawing drawing = new Drawing();
            drawing.Id = dp.GetIdOfPart(imgp);

            wsd.Save(dp);
            ws.Append(drawing);
        }
        public void InsertPicputeInSpreadsheet(string sImagePath, WorksheetPart wsp, WorkbookPart wbp)
        {
            try
            {
                Worksheet ws = wsp.Worksheet;
                SheetData sd = wsp.Worksheet.GetFirstChild<SheetData>();
                WorkbookStylesPart wbsp = wbp.WorkbookStylesPart;
             //   CustomInsertPicture(ref wsp, ref ws, sImagePath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.ReadLine();
            }

        }

        private string getColumnName(int columnIndex)
        {
            int dividend = columnIndex;
            string columnName = String.Empty;
            int modifier;

            while (dividend > 0)
            {
                modifier = (dividend - 1) % 26;
                columnName =
                    Convert.ToChar(65 + modifier).ToString() + columnName;
                dividend = (int)((dividend - modifier) / 26);
            }

            return columnName;
        }


        private void InitUsedFontsInSheet(WorkbookStylesPart stylesPart)
        {
            //Blank font is requried
            stylesPart.Stylesheet.Fonts = new DocumentFormat.OpenXml.Spreadsheet.Fonts();
            stylesPart.Stylesheet.Fonts.Count = 1;

            //Black-Bold font for Name of Reports
            stylesPart.Stylesheet.Fonts.AppendChild(new Font() { Bold = new Bold() { Val = true }, Color = new Color() { Rgb = HexBinaryValue.FromString(BAM_BLACK_HEADER) } }); // ID = 0
            stylesPart.Stylesheet.Fonts.Count = 2;
            // FONT_STRONG_BLACK_ID = 0;
            //White - Bold font for Header of Table
            stylesPart.Stylesheet.Fonts.AppendChild(new Font() { Bold = new Bold() { Val = true }, Color = new Color() { Rgb = HexBinaryValue.FromString(BAM_WHITE_HEADER) } }); // ID = 1
            stylesPart.Stylesheet.Fonts.Count = 3;
            // FONT_STRONG_WHITE_ID = 1;
            //Black - Normal font for Text Content
            stylesPart.Stylesheet.Fonts.AppendChild(new Font() { Bold = new Bold() { Val = false }, Color = new Color() { Rgb = HexBinaryValue.FromString(BAM_BLACK_HEADER) } }); // ID = 2
            stylesPart.Stylesheet.Fonts.Count = 4;
            //FONT_NORMAL_BLACK_ID = 2;
            //White - Normal font for Text Header content
            stylesPart.Stylesheet.Fonts.AppendChild(new Font() { Bold = new Bold() { Val = false }, Color = new Color() { Rgb = HexBinaryValue.FromString(BAM_WHITE_HEADER) } }); // ID = 3
            stylesPart.Stylesheet.Fonts.Count = 5;
            // FONT_NORMAL_WHITE_ID = 3;

        }

        private void InitFilledColorInSheet(WorkbookStylesPart stylesPart)
        {

            stylesPart.Stylesheet.Fills = new Fills();
            stylesPart.Stylesheet.Fills.AppendChild(new DocumentFormat.OpenXml.Spreadsheet.Fill { PatternFill = new DocumentFormat.OpenXml.Spreadsheet.PatternFill { PatternType = PatternValues.None } }); // required, reserved by Excel  

            stylesPart.Stylesheet.Fills.Count = 1;
            stylesPart.Stylesheet.Fills.AppendChild(new DocumentFormat.OpenXml.Spreadsheet.Fill { PatternFill = new DocumentFormat.OpenXml.Spreadsheet.PatternFill { PatternType = PatternValues.Gray125 } }); // required, reserved by Excel
            stylesPart.Stylesheet.Fills.Count = 2;

            var solidColor = new DocumentFormat.OpenXml.Spreadsheet.PatternFill() { PatternType = PatternValues.Solid }; // ID =1
            solidColor.ForegroundColor = new DocumentFormat.OpenXml.Spreadsheet.ForegroundColor() { Rgb = HexBinaryValue.FromString(BAM_VALUE_LV1_HEADER_COLOR_BLUE_STRONG) };
            solidColor.BackgroundColor = new DocumentFormat.OpenXml.Spreadsheet.BackgroundColor { Indexed = 64 };
            stylesPart.Stylesheet.Fills.AppendChild(new DocumentFormat.OpenXml.Spreadsheet.Fill { PatternFill = solidColor });
            stylesPart.Stylesheet.Fills.Count = 3;
            //FILL_COLOR_HEADER_METRIC_ID = 1;


            solidColor = new DocumentFormat.OpenXml.Spreadsheet.PatternFill() { PatternType = PatternValues.Solid }; //ID = 2
            solidColor.ForegroundColor = new DocumentFormat.OpenXml.Spreadsheet.ForegroundColor() { Rgb = HexBinaryValue.FromString(BAM_VALUE_LV_2_HEADER_COLOR_BLUE_NORMAL) };
            solidColor.BackgroundColor = new DocumentFormat.OpenXml.Spreadsheet.BackgroundColor { Indexed = 64 };
            stylesPart.Stylesheet.Fills.AppendChild(new DocumentFormat.OpenXml.Spreadsheet.Fill { PatternFill = solidColor });
            stylesPart.Stylesheet.Fills.Count = 4;
            //FILL_COLOR_HEADER_CONTENT_ID = 2;

            solidColor = new DocumentFormat.OpenXml.Spreadsheet.PatternFill() { PatternType = PatternValues.Solid }; //ID = 3
            solidColor.ForegroundColor = new DocumentFormat.OpenXml.Spreadsheet.ForegroundColor() { Rgb = HexBinaryValue.FromString(BAM_VALUE_LV_3_HEADER_COLOR_BLACK_NORMAL) };
            solidColor.BackgroundColor = new DocumentFormat.OpenXml.Spreadsheet.BackgroundColor { Indexed = 64 };
            stylesPart.Stylesheet.Fills.AppendChild(new DocumentFormat.OpenXml.Spreadsheet.Fill { PatternFill = solidColor });
            stylesPart.Stylesheet.Fills.Count = 4;
            // FILL_COLOR_HEADER_CONTENT_ID = 3;


        }


        private void InitBoderStyleInSheet(WorkbookStylesPart stylesPart)
        {
            //this part is required
            /*-------------------------------------*/
            stylesPart.Stylesheet.Borders = new Borders();
            stylesPart.Stylesheet.Borders.Count = 1;
            stylesPart.Stylesheet.Borders.AppendChild(new Border());
            /*-------------------------------------*/
        }

        private void InitCellStyleFormatInSheet(WorkbookStylesPart stylesPart)
        {
            //this part is required
            /*-------------------------------------*/
            stylesPart.Stylesheet.CellStyleFormats = new CellStyleFormats();
            stylesPart.Stylesheet.CellStyleFormats.Count = 1;
            stylesPart.Stylesheet.CellStyleFormats.AppendChild(new CellFormat());
            /*-------------------------------------*/
        }

        private void InitCellFormatInSheet(WorkbookStylesPart stylesPart)
        {
            //this part is required
            /*-------------------------------------*/
            stylesPart.Stylesheet.CellFormats = new CellFormats();
            stylesPart.Stylesheet.CellFormats.AppendChild(new CellFormat()); //StyleIDex = 0
            /*-------------------------------------*/

            stylesPart.Stylesheet.CellFormats.AppendChild(new CellFormat { FormatId = 0, FontId = (UInt32)FONTID.FONT_STRONG_BLACK_ID, BorderId = 0, FillId = 0, ApplyFill = true }).AppendChild(new Alignment { Horizontal = HorizontalAlignmentValues.Left });//StyleIndex= 1
            stylesPart.Stylesheet.CellFormats.Count = 2;

            stylesPart.Stylesheet.CellFormats.AppendChild(new CellFormat { FormatId = 0, FontId = (UInt32)FONTID.FONT_NORMAL_BLACK_ID, BorderId = 0, FillId = 0, ApplyFill = true }).AppendChild(new Alignment { Horizontal = HorizontalAlignmentValues.Left }); //StyleIndex =2
            stylesPart.Stylesheet.CellFormats.Count = 3;

            stylesPart.Stylesheet.CellFormats.AppendChild(new CellFormat { FormatId = 0, FontId = (UInt32)FONTID.FONT_STRONG_WHITE_ID, BorderId = 0, FillId = (UInt32)FILL_COLOR.FILL_COLOR_HEADER_METRIC_ID, ApplyFill = true }).AppendChild(new Alignment { Horizontal = HorizontalAlignmentValues.Center });//StyleIndex= 3
            stylesPart.Stylesheet.CellFormats.Count = 4;

            stylesPart.Stylesheet.CellFormats.AppendChild(new CellFormat { FormatId = 0, FontId = (UInt32)FONTID.FONT_NORMAL_WHITE_ID, BorderId = 0, FillId = (UInt32)FILL_COLOR.FILL_COLOR_HEADER_CONTENT_ID, ApplyFill = true }).AppendChild(new Alignment { Horizontal = HorizontalAlignmentValues.Center }); //StyleIndex =4
            stylesPart.Stylesheet.CellFormats.Count = 5;

            stylesPart.Stylesheet.CellFormats.AppendChild(new CellFormat { FormatId = 0, FontId = (UInt32)FONTID.FONT_STRONG_WHITE_ID, BorderId = 0, FillId = (UInt32)FILL_COLOR.FILL_COLOR_HEADER_STOREGOALD_ID, ApplyFill = true }).AppendChild(new Alignment { Horizontal = HorizontalAlignmentValues.Center }); //StyleIndex =5
            stylesPart.Stylesheet.CellFormats.Count = 6;

            stylesPart.Stylesheet.CellFormats.AppendChild(new CellFormat { FormatId = 0, FontId = (UInt32)FONTID.FONT_STRONG_BLACK_ID, BorderId = 0, FillId = 0, ApplyFill = true }).AppendChild(new Alignment { Horizontal = HorizontalAlignmentValues.Center }); //StyleIndex =5
            stylesPart.Stylesheet.CellFormats.Count = 7;

            stylesPart.Stylesheet.CellFormats.AppendChild(new CellFormat { FormatId = 0, FontId = (UInt32)FONTID.FONT_NORMAL_BLACK_ID, BorderId = 0, FillId = 0, ApplyFill = true }).AppendChild(new Alignment { Horizontal = HorizontalAlignmentValues.Center }); //StyleIndex =5
            stylesPart.Stylesheet.CellFormats.Count = 7;
        }

        private void MergeTwoCells(Worksheet worksheet, string cell1, string cell2)
        {
            MergeCells mergeCells;
            if (worksheet.Elements<MergeCells>().Count() > 0)
            {
                mergeCells = worksheet.Elements<MergeCells>().First();
            }
            else
            {
                mergeCells = new MergeCells();
                if (worksheet.Elements<CustomSheetView>().Count() > 0)
                {
                    worksheet.InsertAfter(mergeCells, worksheet.Elements<CustomSheetView>().First());
                }
                else if (worksheet.Elements<DataConsolidate>().Count() > 0)
                {
                    worksheet.InsertAfter(mergeCells, worksheet.Elements<DataConsolidate>().First());
                }
                else if (worksheet.Elements<SortState>().Count() > 0)
                {
                    worksheet.InsertAfter(mergeCells, worksheet.Elements<SortState>().First());
                }
                else if (worksheet.Elements<AutoFilter>().Count() > 0)
                {
                    worksheet.InsertAfter(mergeCells, worksheet.Elements<AutoFilter>().First());
                }
                else if (worksheet.Elements<Scenarios>().Count() > 0)
                {
                    worksheet.InsertAfter(mergeCells, worksheet.Elements<Scenarios>().First());
                }
                else if (worksheet.Elements<ProtectedRanges>().Count() > 0)
                {
                    worksheet.InsertAfter(mergeCells, worksheet.Elements<ProtectedRanges>().First());
                }
                else if (worksheet.Elements<SheetProtection>().Count() > 0)
                {
                    worksheet.InsertAfter(mergeCells, worksheet.Elements<SheetProtection>().First());
                }
                else if (worksheet.Elements<SheetCalculationProperties>().Count() > 0)
                {
                    worksheet.InsertAfter(mergeCells, worksheet.Elements<SheetCalculationProperties>().First());
                }
                else
                {
                    worksheet.InsertAfter(mergeCells, worksheet.Elements<SheetData>().First());
                }
            }

            MergeCell mcell = new MergeCell() { Reference = cell1 + ":" + cell2 };
            mergeCells.AppendChild<MergeCell>(mcell);
        }

        public void InsertBarChartInSpreadsheet(WorksheetPart worksheetPart, int colIndex, int rowIndex, int width, int height, string[][] Chart1, float GoalMaxValue = 0, float GoalMinValue = 0, string Title = "")
        {
            DrawingsPart drawingsPart;
            if (worksheetPart.DrawingsPart != null)
            {
                drawingsPart = worksheetPart.DrawingsPart;
            }
            else
            {
                drawingsPart = worksheetPart.AddNewPart<DrawingsPart>();
                worksheetPart.Worksheet.Append(new DocumentFormat.OpenXml.Spreadsheet.Drawing() { Id = worksheetPart.GetIdOfPart(drawingsPart) });
                worksheetPart.Worksheet.Save();
            }


            // Add a new chart and set the chart language to English-US.
            ChartPart chartPart = drawingsPart.AddNewPart<ChartPart>();
            chartPart.ChartSpace = new ChartSpace();
            chartPart.ChartSpace.Append(new EditingLanguage() { Val = new StringValue("en-US") });
            DocumentFormat.OpenXml.Drawing.Charts.Chart chart = chartPart.ChartSpace.AppendChild<DocumentFormat.OpenXml.Drawing.Charts.Chart>(
                new DocumentFormat.OpenXml.Drawing.Charts.Chart());
            chart.Append(new AutoTitleDeleted() { Val = false });


            Title title1 = new Title();

            ChartText chartText1 = new ChartText();

            RichText richText1 = new RichText();
            A.BodyProperties bodyProperties1 = new A.BodyProperties();
            A.ListStyle listStyle1 = new A.ListStyle();

            A.Paragraph paragraph1 = new A.Paragraph();

            A.ParagraphProperties paragraphProperties1 = new A.ParagraphProperties();
            A.DefaultRunProperties defaultRunProperties1 = new A.DefaultRunProperties();

            paragraphProperties1.Append(defaultRunProperties1);

            A.Run run1 = new A.Run();

            A.RunProperties runProperties1 = new A.RunProperties() { Language = "en-US", FontSize = 1000, Bold = true, Italic = false, Underline = A.TextUnderlineValues.None, Strike = A.TextStrikeValues.NoStrike, Baseline = 0 };
            A.EffectList effectList1 = new A.EffectList();

            runProperties1.Append(effectList1);
            A.Text text1 = new A.Text();
            text1.Text = Title;

            run1.Append(runProperties1);
            run1.Append(text1);
            A.EndParagraphRunProperties endParagraphRunProperties1 = new A.EndParagraphRunProperties() { Language = "en-US" };

            paragraph1.Append(paragraphProperties1);
            paragraph1.Append(run1);
            paragraph1.Append(endParagraphRunProperties1);

            richText1.Append(bodyProperties1);
            richText1.Append(listStyle1);
            richText1.Append(paragraph1);

            chartText1.Append(richText1);
            Layout layout1 = new Layout();
            Overlay overlay1 = new Overlay() { Val = false };

            title1.Append(chartText1);
            title1.Append(layout1);
            title1.Append(overlay1);

            chart.Append(title1);

           


            // Create a new clustered column chart.
            PlotArea plotArea = chart.AppendChild<PlotArea>(new PlotArea());
            // Layout layout = plotArea.AppendChild<Layout>(new Layout());
            BarChart barChart = plotArea.AppendChild<BarChart>(new BarChart(new BarDirection() { Val = new EnumValue<BarDirectionValues>(BarDirectionValues.Column) },
                new BarGrouping() { Val = new EnumValue<BarGroupingValues>(BarGroupingValues.Clustered) }));
            barChart.AppendChild(new GapWidth() { Val = (UInt16Value)80U });
            uint i = 0;


            Worksheet worksheet = worksheetPart.Worksheet;
            SheetData sheetData = worksheet.Elements<SheetData>().FirstOrDefault();
            MergeCells mergeCells = worksheet.Elements<MergeCells>().FirstOrDefault();
            //  excelDocument.CustomMergeCell(ref sheetData, ref mergeCells, getColumnName(colIndex + 1) + rowIndex, getColumnName(colIndex + 5) + rowIndex, xmldata.Attribute("Title").Value.ToString(), App_Cons.FONT_CONTENT_TEXT_MARGIN_LEFT);

            //=================================================================================================================================================
            // Iterate through each key in the Dictionary collection and add the key to the chart Series
            // and add the corresponding value to the chart Values.
            BarChartSeries barChartSeries = barChart.AppendChild<BarChartSeries>(new BarChartSeries(new Index()
            {
                Val =
                    new UInt32Value(i)
            },
            new Order() { Val = new UInt32Value(i) },
            new SeriesText(new NumericValue() { Text = "" })));


            Dictionary<string, string> dataElement1 = new Dictionary<string, string>();
            foreach (var item in Chart1)
            {
                dataElement1.Add(item[0], item[1]);
            }


            StringLiteral strLit = barChartSeries.AppendChild<CategoryAxisData>(new CategoryAxisData()).AppendChild<StringLiteral>(new StringLiteral());
            strLit.Append(new PointCount() { Val = new UInt32Value(Convert.ToUInt32(dataElement1.Count)) });

            DocumentFormat.OpenXml.Drawing.Charts.Values cVal = barChartSeries.AppendChild(new DocumentFormat.OpenXml.Drawing.Charts.Values());
            NumberLiteral numLit = cVal.AppendChild(new NumberLiteral());

            numLit.AppendChild<FormatCode>(new FormatCode() { Text = ""  /*"\"$\"#,##0.00_);[Red]\\(\"$\"#,##0.00\\)"*/ });
            numLit.AppendChild<PointCount>(new PointCount() { Val = new UInt32Value(Convert.ToUInt32(dataElement1.Count)) });

            foreach (var data in dataElement1)
            {


                DataPoint dtPoint = barChartSeries.AppendChild<DataPoint>(new DataPoint());
                dtPoint.Append(new Index() { Val = new UInt32Value(Convert.ToUInt32(i)) });

                ChartShapeProperties cSP = dtPoint.AppendChild(new ChartShapeProperties());
                SolidFill slF = cSP.AppendChild(new SolidFill());
                PresetColor pc = slF.AppendChild(new PresetColor());
                if (float.Parse(data.Value) >= GoalMaxValue)
                {
                    pc.Val = PresetColorValues.Green;
                }
                if (float.Parse(data.Value) < GoalMaxValue && float.Parse(data.Value) > GoalMinValue)
                {
                    pc.Val = PresetColorValues.Orange;
                }
                if ( float.Parse(data.Value) <= GoalMinValue)
                {
                    pc.Val = PresetColorValues.Red;
                }

                StringPoint stPoint = strLit.AppendChild<StringPoint>(new StringPoint() { Index = new UInt32Value(Convert.ToUInt32(i)) });
                NumericValue numval = new NumericValue();
                numval.Text = data.Key.ToString();
                stPoint.Append(numval);


                NumericPoint numPoint = numLit.AppendChild<NumericPoint>(new NumericPoint() { Index = new UInt32Value(Convert.ToUInt32(i)) });
                NumericValue cNumVal = new NumericValue();
                cNumVal.Text = data.Value.ToString();
                numPoint.Append(cNumVal);

                i++;

            }


            Overlap ovl = new Overlap();
            ovl.Val = -30;
            barChart.AppendChild<Overlap>(ovl);



            DataLabels dataLabels1 = new DataLabels(new ShowLegendKey() { Val = false }, new ShowCategoryName() { Val = false }, new ShowBubbleSize() { Val = false }, new ShowSeriesName() { Val = false });
            DataLabelPosition dataLabelPosition1 = new DataLabelPosition() { Val = DataLabelPositionValues.OutsideEnd };
            ShowValue showValue1 = new ShowValue() { Val = true };
            dataLabels1.Append(dataLabelPosition1);
            dataLabels1.Append(showValue1);

            dataLabels1.Append(new ShowPercent() { Val = false });


            barChart.Append(dataLabels1);

            //=================================================================================================================================================
            barChart.Append(new AxisId() { Val = new UInt32Value(188794368U) });
            barChart.Append(new AxisId() { Val = new UInt32Value(188795904U) });


            CategoryAxis catAx = plotArea.AppendChild<CategoryAxis>
            (new CategoryAxis(new AxisId() { Val = new UInt32Value(188794368U) }, new MajorTickMark() { Val = TickMarkValues.Outside }, new Delete() { Val = false }, new Scaling(new Orientation()
            {
                Val = new EnumValue<DocumentFormat.
                    OpenXml.Drawing.Charts.OrientationValues>(DocumentFormat.OpenXml.Drawing.Charts.OrientationValues.MinMax)
            }),
            new AxisPosition() { Val = new EnumValue<AxisPositionValues>(AxisPositionValues.Bottom) },
            new TickLabelPosition() { Val = new EnumValue<TickLabelPositionValues>(TickLabelPositionValues.NextTo) },
            new CrossingAxis() { Val = new UInt32Value(188794368U) },
            new Crosses() { Val = new EnumValue<CrossesValues>(CrossesValues.AutoZero) },
            new AutoLabeled() { Val = new BooleanValue(true) },
            new LabelAlignment() { Val = new EnumValue<LabelAlignmentValues>(LabelAlignmentValues.Center) },
            new LabelOffset() { Val = new UInt16Value((ushort)100) }));

            // Add the Value Axis.
            ValueAxis valAx = plotArea.AppendChild<ValueAxis>(new ValueAxis(new AxisId() { Val = new UInt32Value(188795904U) }, new MajorTickMark() { Val = TickMarkValues.Outside }, new Delete() { Val = false },
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
                    Val = new EnumValue<TickLabelPositionValues>(TickLabelPositionValues.NextTo)
                },
                    new CrossingAxis() { Val = new UInt32Value(188795904U) },
                    new Crosses() { Val = new EnumValue<CrossesValues>(CrossesValues.AutoZero) },
                    new CrossBetween()
                    {
                        Val = new EnumValue<CrossBetweenValues>(CrossBetweenValues.Between)
                    }));

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
            DocumentFormat.OpenXml.Drawing.Spreadsheet.GraphicFrame graphicFrame = twoCellAnchor.AppendChild<DocumentFormat.OpenXml.Drawing.Spreadsheet.GraphicFrame>(new DocumentFormat.OpenXml.Drawing.
            Spreadsheet.GraphicFrame());
            graphicFrame.Macro = "";

            graphicFrame.Append(new DocumentFormat.OpenXml.Drawing.Spreadsheet.NonVisualGraphicFrameProperties(
                new DocumentFormat.OpenXml.Drawing.Spreadsheet.NonVisualDrawingProperties() { Id = new UInt32Value(2u), Name = "Chart 1" },
                new DocumentFormat.OpenXml.Drawing.Spreadsheet.NonVisualGraphicFrameDrawingProperties()));

            graphicFrame.Append(new Transform(new Offset() { X = 0L, Y = 0L }, new Extents() { Cx = 0L, Cy = 0L }));

            graphicFrame.Append(new Graphic(new GraphicData(new ChartReference() { Id = drawingsPart.GetIdOfPart(chartPart) }) { Uri = "http://schemas.openxmlformats.org/drawingml/2006/chart" }));

            twoCellAnchor.Append(new ClientData());

            drawingsPart.WorksheetDrawing.Save();
        }

        public void InsertScatterChartInSpreadsheet(WorksheetPart worksheetPart, int colIndex, int rowIndex, int width, int height,string[] Chart2,string Title ="")
        {

            DrawingsPart drawingsPart;
            if (worksheetPart.DrawingsPart != null)
            {
                drawingsPart = worksheetPart.DrawingsPart;
            }
            else
            {
                drawingsPart = worksheetPart.AddNewPart<DrawingsPart>();
                worksheetPart.Worksheet.Append(new DocumentFormat.OpenXml.Spreadsheet.Drawing() { Id = worksheetPart.GetIdOfPart(drawingsPart) });
                worksheetPart.Worksheet.Save();
            }

            // Add a new chart and set the chart language to English-US.
            ChartPart chartPart = drawingsPart.AddNewPart<ChartPart>();
            chartPart.ChartSpace = new ChartSpace();
            chartPart.ChartSpace.Append(new EditingLanguage() { Val = new StringValue("en-US") });
            DocumentFormat.OpenXml.Drawing.Charts.Chart chart = chartPart.ChartSpace.AppendChild<DocumentFormat.OpenXml.Drawing.Charts.Chart>(
                new DocumentFormat.OpenXml.Drawing.Charts.Chart() { AutoTitleDeleted = new AutoTitleDeleted() { Val = false }, Legend = new Legend(new Overlay() { Val = false }, new LegendPosition() { Val = LegendPositionValues.Right }, new Layout()) });

            Title title1 = new Title();

            ChartText chartText1 = new ChartText();

            RichText richText1 = new RichText();
            A.BodyProperties bodyProperties1 = new A.BodyProperties();
            A.ListStyle listStyle1 = new A.ListStyle();

            A.Paragraph paragraph1 = new A.Paragraph();

            A.ParagraphProperties paragraphProperties1 = new A.ParagraphProperties();
            A.DefaultRunProperties defaultRunProperties1 = new A.DefaultRunProperties();

            paragraphProperties1.Append(defaultRunProperties1);

            A.Run run1 = new A.Run();

            A.RunProperties runProperties1 = new A.RunProperties() { Language = "en-US", FontSize = 1000, Bold = true, Italic = false, Underline = A.TextUnderlineValues.None, Strike = A.TextStrikeValues.NoStrike, Baseline = 0 };
            A.EffectList effectList1 = new A.EffectList();

            runProperties1.Append(effectList1);
            A.Text text1 = new A.Text();
            text1.Text = Title;

            run1.Append(runProperties1);
            run1.Append(text1);
            A.EndParagraphRunProperties endParagraphRunProperties1 = new A.EndParagraphRunProperties() { Language = "en-US" };

            paragraph1.Append(paragraphProperties1);
            paragraph1.Append(run1);
            paragraph1.Append(endParagraphRunProperties1);

            richText1.Append(bodyProperties1);
            richText1.Append(listStyle1);
            richText1.Append(paragraph1);

            chartText1.Append(richText1);
            Layout layout1 = new Layout();
            Overlay overlay1 = new Overlay() { Val = false };

            title1.Append(chartText1);
            title1.Append(layout1);
            title1.Append(overlay1);

            chart.Append(title1);

            // Create a new clustered column chart.
            PlotArea plotArea = chart.AppendChild<PlotArea>(new PlotArea());
            Layout layout = plotArea.AppendChild<Layout>(new Layout());

            ScatterChart scatChart = plotArea.AppendChild<ScatterChart>(new ScatterChart());
            ScatterStyle scatStyle = new ScatterStyle() { Val = ScatterStyleValues.SmoothMarker };
            scatChart.AppendChild<ScatterStyle>(scatStyle);

            Worksheet worksheet = worksheetPart.Worksheet;
            SheetData sheetData = worksheet.Elements<SheetData>().FirstOrDefault();
            MergeCells mergeCells = worksheet.Elements<MergeCells>().FirstOrDefault();

            Dictionary<string, string> dataElement1 = new Dictionary<string, string>();
            for (int j = 0;j < Chart2.Length; j++)
            {
                string []s = Chart2[j].Split('|');
                string rs = Chart2[j].Replace(s[0]+'|',"");
                dataElement1.Add(s[0], rs);
            }

            uint i = 0;
            foreach (var data in dataElement1)
            {
                ScatterChartSeries scatterSeries = scatChart.AppendChild<ScatterChartSeries>(new ScatterChartSeries(new Index()
                {
                    Val = new UInt32Value(i)
                },
                new Order() { Val = new UInt32Value(i) },
                new SeriesText(new NumericValue() { Text = data.Key }))
                );

                XValues XValues = new XValues();
                NumberLiteral XLiteral = new NumberLiteral();

                YValues YValues = new YValues();
                NumberLiteral YLiteral = new NumberLiteral();
                FormatCode YformatCode = new FormatCode();
                YformatCode.Text = "General";
                YLiteral.Append(YformatCode);

                string v = data.Value.ToString();
                string[] values = v.Split(',');
                uint indexCounter = 0;
                foreach (string value in values)
                {
                    string[] t = value.Split('|');
                    if (!String.IsNullOrEmpty(t[0]) && !String.IsNullOrEmpty(t[1]))
                    {
                        NumericPoint XPoint = new NumericPoint() { Index = indexCounter };
                        NumericValue XValue = new NumericValue();
                        DateTime minD = new DateTime(1900, 1, 1);

                        //DateTime valDate = minD.AddDays(int.Parse(t[0]));
                        DateTime valDate = DateTime.ParseExact(t[0], "MM/dd/yyyy", null);
                        TimeSpan span = valDate.Subtract(minD);

                        int diffDays = (int)span.TotalDays;
                        diffDays += 2;
                        XValue.Text = diffDays.ToString();
                       // XValue.Text = t[0];
                        XPoint.Append(XValue);

                        NumericPoint YPoint = new NumericPoint() { Index = indexCounter };
                        NumericValue YValue = new NumericValue();
                        YValue.Text = t[1].ToString();
                        YPoint.Append(YValue);

                        XLiteral.Append(XPoint);
                        YLiteral.Append(YPoint);

                        indexCounter++;
                    }
                }

                PointCount XpointCount = new PointCount() { Val = indexCounter };
                PointCount YpointCount = new PointCount() { Val = indexCounter };


                XLiteral.Append(XpointCount);
                YLiteral.Append(YpointCount);

                XValues.Append(XLiteral);
                YValues.Append(YLiteral);

                scatterSeries.Append(XValues);
                scatterSeries.Append(YValues);
                Smooth sm = new Smooth() { Val = true };
                scatterSeries.Append(sm);
                i++;

            }

            scatChart.Append(new AxisId() { Val = new UInt32Value(48650112u) });
            scatChart.Append(new AxisId() { Val = new UInt32Value(48672768u) });


            // Add the Category Axis.
            CategoryAxis catAx = plotArea.AppendChild<CategoryAxis>(new CategoryAxis(new AxisId() { Val = new UInt32Value(48650112u) }, new Scaling(new Orientation()
            {
                Val = new EnumValue<DocumentFormat.
                    OpenXml.Drawing.Charts.OrientationValues>(DocumentFormat.OpenXml.Drawing.Charts.OrientationValues.MinMax)
            }),
                new AxisPosition() { Val = new EnumValue<AxisPositionValues>(AxisPositionValues.Bottom) },
                new DocumentFormat.OpenXml.Drawing.Charts.NumberingFormat()
                {
                    FormatCode = new StringValue("mm/dd/yyyy"),
                    SourceLinked = new BooleanValue(true)
                },
                new TickLabelPosition() { Val = new EnumValue<TickLabelPositionValues>(TickLabelPositionValues.NextTo) },
                new CrossingAxis() { Val = new UInt32Value(48672768U) },
                new Crosses() { Val = new EnumValue<CrossesValues>(CrossesValues.AutoZero) },
                new AutoLabeled() { Val = new BooleanValue(false) },
                new LabelAlignment() { Val = new EnumValue<LabelAlignmentValues>(LabelAlignmentValues.Center) },
                new LabelOffset() { Val = new UInt16Value((ushort)100) },
                new Delete() { Val = false },
                new MajorTickMark() { Val = TickMarkValues.None },
                new MinorTickMark() { Val = TickMarkValues.None }));



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
                new CrossBetween() { Val = new EnumValue<CrossBetweenValues>(CrossBetweenValues.Between) },
                new Delete() { Val = false },
                new MajorGridlines(),
                new MajorTickMark() { Val = TickMarkValues.None },
                new MinorTickMark() { Val = TickMarkValues.None }));

            // Add the chart Legend.
            Legend legend = chart.AppendChild<Legend>(new Legend(new LegendPosition() { Val = new EnumValue<LegendPositionValues>(LegendPositionValues.Right) },
                new Layout()));

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
            DocumentFormat.OpenXml.Drawing.Spreadsheet.GraphicFrame graphicFrame =
                twoCellAnchor.AppendChild<DocumentFormat.OpenXml.
    Drawing.Spreadsheet.GraphicFrame>(new DocumentFormat.OpenXml.Drawing.
    Spreadsheet.GraphicFrame());
            graphicFrame.Macro = "";

            graphicFrame.Append(new DocumentFormat.OpenXml.Drawing.Spreadsheet.NonVisualGraphicFrameProperties(
                new DocumentFormat.OpenXml.Drawing.Spreadsheet.NonVisualDrawingProperties() { Id = new UInt32Value(2u), Name = "Chart 1" },
                new DocumentFormat.OpenXml.Drawing.Spreadsheet.NonVisualGraphicFrameDrawingProperties()));

            graphicFrame.Append(new Transform(new Offset() { X = 0L, Y = 0L },
                                                                    new Extents() { Cx = 0L, Cy = 0L }));

            graphicFrame.Append(new Graphic(new GraphicData(new ChartReference() { Id = drawingsPart.GetIdOfPart(chartPart) }) { Uri = "http://schemas.openxmlformats.org/drawingml/2006/chart" }));

            twoCellAnchor.Append(new ClientData());

            // Save the WorksheetDrawing object.
            drawingsPart.WorksheetDrawing.Save();

        }




        public void CreateSpreadsheetWorkbook(string filepath,DashboardExportData data)
        {
            // Create a spreadsheet document by supplying the filepath.
            // By default, AutoSave = true, Editable = true, and Type = xlsx.
            SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Create(filepath, SpreadsheetDocumentType.Workbook);

            // Add a WorkbookPart to the document.
            WorkbookPart workbookpart = spreadsheetDocument.AddWorkbookPart();
            workbookpart.Workbook = new Workbook();

            // Add a WorksheetPart to the WorkbookPart.
            WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(
                                                    new SheetViews(new SheetView(new DocumentFormat.OpenXml.Spreadsheet.Selection() { ActiveCell = "S8", SequenceOfReferences = new ListValue<StringValue>() { InnerText = "Q8:S8" } }) { TabSelected = true, View = SheetViewValues.PageLayout, WorkbookViewId = (UInt32Value)0U, }),
                                                    new SheetFormatProperties() { DefaultRowHeight = 13.5, DefaultColumnWidth = 3,CustomHeight =true },
                                                    new Columns(new Column() { Max = 5000, Width = 6.5D, Min = 1, CustomWidth = true }),
                                                    new SheetData(),
                                                    new DocumentFormat.OpenXml.Spreadsheet.PageMargins() { Left = 0.7D, Right = 0.7D, Top = 0.75D, Bottom = 0.75D, Header = 0.3D, Footer = 0.3D },
                                                    new DocumentFormat.OpenXml.Spreadsheet.PageSetup() { Orientation = DocumentFormat.OpenXml.Spreadsheet.OrientationValues.Landscape }
                                                  );


            WorkbookStylesPart stylesPart = workbookpart.AddNewPart<WorkbookStylesPart>();
            stylesPart.Stylesheet = new Stylesheet();

            Worksheet ws = worksheetPart.Worksheet;
            //CustomInsertPicture(ref worksheetPart, ref ws, "I:\\Cun.jpg");

            /* Init all things about style in Sheet */
            /*--------------------------------------*/
            #region StyleSheet
            InitUsedFontsInSheet(stylesPart);

            InitFilledColorInSheet(stylesPart);

            InitBoderStyleInSheet(stylesPart);

            InitCellStyleFormatInSheet(stylesPart);

            InitCellFormatInSheet(stylesPart);



            stylesPart.Stylesheet.Save();

            #endregion
            /*--------------------------------------*/


            // Add Sheets to the Workbook.
            Sheets sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());

            // Append a new worksheet and associate it with the workbook.
            Sheet sheet = new Sheet() { Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Sales Report" };
            sheets.Append(sheet);

            // Get the sheetData cell table.
            SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

            // Add a row to the cell table.

            Row row;
            ExampleData raw = new ExampleData();
            List<List<CellsData>> cdata = data.Cells; //raw.DataCells();
            int colConst = 19;
            int rowConst = 38;
            int colIndex1 = 0;
            int rowIndex = 1;
            for (UInt32 i = 0; i < cdata.Count(); i++)
            {

                UInt32 flag = 0;
                UInt32 rCount = i % (UInt32)rowConst == 0 ? 1 : i % (UInt32)rowConst + 1;


                if (sheetData.Elements<Row>().Any(rC => rC.RowIndex == rCount))
                {
                    row = sheetData.Elements<Row>().First(rC => rC.RowIndex == rCount);
                }
                else
                {
                    row = new Row() { RowIndex = rCount };
                    sheetData.Append(row);
                }


                for (UInt32 j = 0; j < cdata.ElementAt((int)i).Count(); j++)
                {
                    CellsData c = cdata.ElementAt((int)i).ElementAt((int)j);
                    UInt32 cCount = j + flag + 1 + (UInt32)c.StartCol + (UInt32)(i / rowConst) * 19;


                    string currentCell = getColumnName((int)cCount) + rCount.ToString();
                    string nextCell = getColumnName((int)cCount + c.Merge - 1) + rCount.ToString();
                    Cell cellData = new Cell() { CellReference = currentCell, StyleIndex = (UInt32)c.StyleIndex, CellValue = new CellValue(c.Name), DataType = new EnumValue<CellValues>(CellValues.String) };
                    row.InsertBefore(cellData, null);
                    if (c.Merge != 0)
                    {
                        MergeTwoCells(worksheetPart.Worksheet, currentCell, nextCell);
                        flag = (UInt32)c.Merge + flag - 1;
                    }


                    colIndex1 = (int)cCount;
                    rowIndex = (int)rCount;
                }

            }

            if (rowIndex + 1 <= 25)
            {
                colIndex1 = (colIndex1 / 19) * 19;
                rowIndex++;
            }
            else
            {
                colIndex1++;
                rowIndex = 1;
            }
            InsertBarChartInSpreadsheet(worksheetPart, colIndex1, rowIndex, 9, 13, data.Chart1, data.GoalValueMax, data.GoalValueMin,data.Chart1Title);
            InsertScatterChartInSpreadsheet(worksheetPart, colIndex1 + 10, rowIndex, 9, 13, data.Chart2, data.Chart2Title);
            spreadsheetDocument.Close();
        }
    }
}

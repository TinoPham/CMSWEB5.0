using CMSWebApi.Utils;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using A = DocumentFormat.OpenXml.Drawing;
using Ap = DocumentFormat.OpenXml.ExtendedProperties;
using Vt = DocumentFormat.OpenXml.VariantTypes;
using Xdr = DocumentFormat.OpenXml.Drawing.Spreadsheet;
using Cs = DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;

namespace CMSWebApi.DataServices.ExportDataService.Excel
{
	public class ExcelDocument
	{
		// Creates a SpreadsheetDocument.
		public void CreatePackage(string filePath)
		{
			using (SpreadsheetDocument package = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook))
			{
				CreateParts(package);
			}
		}

		// Adds child parts and generates content of the specified part.
		private void CreateParts(SpreadsheetDocument document)
		{
			ExtendedFilePropertiesPart extendedFilePropertiesPart = document.AddNewPart<ExtendedFilePropertiesPart>("rId3");
			GenerateExtendedFilePropertiesPartContent(extendedFilePropertiesPart);

			WorkbookPart workbookPart = document.AddWorkbookPart();
			GenerateWorkbookPartContent(workbookPart);

			WorkbookStylesPart workbookStylesPart = workbookPart.AddNewPart<WorkbookStylesPart>("rId3");
			GenerateWorkbookStylesPartContent(workbookStylesPart);

			ThemePart themePart = workbookPart.AddNewPart<ThemePart>("rId2");
			GenerateThemePartContent(themePart);

			WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>("rId1");
			GenerateWorksheetPartContent(worksheetPart);

			SharedStringTablePart sharedStringTablePart = workbookPart.AddNewPart<SharedStringTablePart>("rId4");
			GenerateSharedStringTablePartContent(sharedStringTablePart);

			SetPackageProperties(document);
		}

		// Generates content of extendedFilePropertiesPart.
		private void GenerateExtendedFilePropertiesPartContent(ExtendedFilePropertiesPart extendedFilePropertiesPart)
		{
			Ap.Properties properties = new Ap.Properties();
			properties.AddNamespaceDeclaration("vt", "http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes");
			Ap.Application application = new Ap.Application();
			application.Text = "Microsoft Excel";
			Ap.DocumentSecurity documentSecurity = new Ap.DocumentSecurity();
			documentSecurity.Text = "0";
			Ap.ScaleCrop scaleCrop = new Ap.ScaleCrop();
			scaleCrop.Text = "false";

			Ap.HeadingPairs headingPairs = new Ap.HeadingPairs();

			Vt.VTVector vTVector = new Vt.VTVector() { BaseType = Vt.VectorBaseValues.Variant, Size = (UInt32Value)2U };

			Vt.Variant variant = new Vt.Variant();
			Vt.VTLPSTR vTLPSTR = new Vt.VTLPSTR();
			vTLPSTR.Text = "Worksheets";
			variant.Append(vTLPSTR);
			vTVector.Append(variant);

			variant = new Vt.Variant();
			Vt.VTInt32 vTInt32 = new Vt.VTInt32();
			vTInt32.Text = "1";
			variant.Append(vTInt32);
			vTVector.Append(variant);

			headingPairs.Append(vTVector);

			Ap.TitlesOfParts titlesOfParts = new Ap.TitlesOfParts();

			vTVector = new Vt.VTVector() { BaseType = Vt.VectorBaseValues.Lpstr, Size = (UInt32Value)1U };
			vTLPSTR = new Vt.VTLPSTR();
			vTLPSTR.Text = "Report";
			vTVector.Append(vTLPSTR);
			titlesOfParts.Append(vTVector);

			Ap.Company company = new Ap.Company();
			company.Text = "Grizli777";
			Ap.LinksUpToDate linksUpToDate = new Ap.LinksUpToDate();
			linksUpToDate.Text = "false";
			Ap.SharedDocument sharedDocument = new Ap.SharedDocument();
			sharedDocument.Text = "false";
			Ap.HyperlinksChanged hyperlinksChanged = new Ap.HyperlinksChanged();
			hyperlinksChanged.Text = "false";
			Ap.ApplicationVersion applicationVersion = new Ap.ApplicationVersion();
			applicationVersion.Text = "12.0000";

			properties.Append(application);
			properties.Append(documentSecurity);
			properties.Append(scaleCrop);
			properties.Append(headingPairs);
			properties.Append(titlesOfParts);
			properties.Append(company);
			properties.Append(linksUpToDate);
			properties.Append(sharedDocument);
			properties.Append(hyperlinksChanged);
			properties.Append(applicationVersion);

			extendedFilePropertiesPart.Properties = properties;
		}

		// Generates content of workbookPart.
		private void GenerateWorkbookPartContent(WorkbookPart workbookPart)
		{
			Workbook workbook = new Workbook();
			workbook.AddNamespaceDeclaration("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
			FileVersion fileVersion = new FileVersion() { ApplicationName = "xl", LastEdited = "4", LowestEdited = "4", BuildVersion = "4507" };
			WorkbookProperties workbookProperties = new WorkbookProperties() { DefaultThemeVersion = (UInt32Value)124226U };

			BookViews bookViews = new BookViews();
			WorkbookView workbookView = new WorkbookView() { XWindow = 0, YWindow = 45, WindowWidth = (UInt32Value)19155U, WindowHeight = (UInt32Value)11820U };

			bookViews.Append(workbookView);

			Sheets sheets = new Sheets();
			Sheet sheet = new Sheet() { Name = Utils.ExportConst.SHEET_NAME, SheetId = (UInt32Value)1U, Id = "rId1" };

			sheets.Append(sheet);
			CalculationProperties calculationProperties = new CalculationProperties() { CalculationId = (UInt32Value)125725U };

			workbook.Append(fileVersion);
			workbook.Append(workbookProperties);
			workbook.Append(bookViews);
			workbook.Append(sheets);
			workbook.Append(calculationProperties);

			workbookPart.Workbook = workbook;
		}

		// Generates content of workbookStylesPart.
		private void GenerateWorkbookStylesPartContent(WorkbookStylesPart workbookStylesPart)
		{
			workbookStylesPart.Stylesheet = new ExcelStyles().GenerateStyleSheet();

			#region move to ExcelStyles class
			//workbookStylesPart.Stylesheet = new Stylesheet();

			////Font
			//workbookStylesPart.Stylesheet.Fonts = new Fonts();
			////Index 0 - default
			//workbookStylesPart.Stylesheet.Fonts.Append(new DocumentFormat.OpenXml.Spreadsheet.Font(
			//	new FontSize() { Val = 10 },
			//	new DocumentFormat.OpenXml.Spreadsheet.Color() { Rgb = new HexBinaryValue() { Value = "000000" } },
			//	new FontName() { Val = "Calibri" }));

			////Index 1 - Report Title
			//workbookStylesPart.Stylesheet.Fonts.Append(new DocumentFormat.OpenXml.Spreadsheet.Font(
			//	new Bold(),
			//	new FontSize() { Val = 20 },
			//	new DocumentFormat.OpenXml.Spreadsheet.Color() { Rgb = new HexBinaryValue() { Value = "000000" } },
			//	new FontName() { Val = "Calibri" }));

			////Index 2 - content - green
			//workbookStylesPart.Stylesheet.Fonts.Append(new DocumentFormat.OpenXml.Spreadsheet.Font(
			//	new FontSize() { Val = 8 },
			//	new DocumentFormat.OpenXml.Spreadsheet.Color() { Rgb = new HexBinaryValue() { Value = ExportConst.COLOR_GREEN } },
			//	new FontName() { Val = "Calibri" }));

			////Index 3 - content - yellow
			//workbookStylesPart.Stylesheet.Fonts.Append(new DocumentFormat.OpenXml.Spreadsheet.Font(
			//	new FontSize() { Val = 8 },
			//	new DocumentFormat.OpenXml.Spreadsheet.Color() { Rgb = new HexBinaryValue() { Value = ExportConst.COLOR_YELLOW } },
			//	new FontName() { Val = "Calibri" }));

			////Index 4 - content - red
			//workbookStylesPart.Stylesheet.Fonts.Append(new DocumentFormat.OpenXml.Spreadsheet.Font(
			//	new FontSize() { Val = 8 },
			//	new DocumentFormat.OpenXml.Spreadsheet.Color() { Rgb = new HexBinaryValue() { Value = ExportConst.COLOR_RED } },
			//	new FontName() { Val = "Calibri" }));

			////Index 5- conten bold
			//workbookStylesPart.Stylesheet.Fonts.Append(new DocumentFormat.OpenXml.Spreadsheet.Font(
			//	new Bold(),
			//	new FontSize() { Val = 6 },
			//	new DocumentFormat.OpenXml.Spreadsheet.Color() { Rgb = new HexBinaryValue() { Value = "00000" } },
			//	new FontName() { Val = "Calibri" }));


			//// Fills
			//workbookStylesPart.Stylesheet.Fills = new Fills();
			//// Index 0 - The default fill.
			//workbookStylesPart.Stylesheet.Fills.Append(new Fill(new PatternFill() { PatternType = PatternValues.None }));

			////Index 1 - The default fill of gray 125 (required)
			//workbookStylesPart.Stylesheet.Fills.Append(new Fill(new PatternFill() { PatternType = PatternValues.Gray125 }));

			////Index 2 - Fill Green
			//workbookStylesPart.Stylesheet.Fills.Append(new Fill(new PatternFill(new ForegroundColor() { Rgb = new HexBinaryValue() { Value = ExportConst.COLOR_GREEN } }) { PatternType = PatternValues.Solid }));

			////Index 3 - Fill Yellow
			//workbookStylesPart.Stylesheet.Fills.Append(new Fill(new PatternFill(new ForegroundColor() { Rgb = new HexBinaryValue() { Value = ExportConst.COLOR_YELLOW } }) { PatternType = PatternValues.Solid }));

			////Index 4 - Fill Red
			//workbookStylesPart.Stylesheet.Fills.Append(new Fill(new PatternFill(new ForegroundColor() { Rgb = new HexBinaryValue() { Value = ExportConst.COLOR_RED } }) { PatternType = PatternValues.Solid }));

			////Index 5 - Fill Grey
			//workbookStylesPart.Stylesheet.Fills.Append(new Fill(new PatternFill(new ForegroundColor() { Rgb = new HexBinaryValue() { Value = ExportConst.COLOR_GRAY } }) { PatternType = PatternValues.Solid }));


			//// Border
			//workbookStylesPart.Stylesheet.Borders = new Borders();
			//// Index 0 - The default border.
			//workbookStylesPart.Stylesheet.Borders.Append(new Border(
			//			new LeftBorder(),
			//			new RightBorder(),
			//			new TopBorder(),
			//			new BottomBorder(),
			//			new DiagonalBorder()));

			//Border border = new Border();

			//LeftBorder leftBorder = new LeftBorder() { Style = BorderStyleValues.Thin };
			//DocumentFormat.OpenXml.Spreadsheet.Color color = new DocumentFormat.OpenXml.Spreadsheet.Color() { Indexed = (UInt32Value)64U };
			//leftBorder.Append(color);
			//RightBorder rightBorder = new RightBorder() { Style = BorderStyleValues.Thin };
			//color = new DocumentFormat.OpenXml.Spreadsheet.Color() { Indexed = (UInt32Value)64U };
			//rightBorder.Append(color);
			//TopBorder topBorder = new TopBorder() { Style = BorderStyleValues.Thin };
			//color = new DocumentFormat.OpenXml.Spreadsheet.Color() { Indexed = (UInt32Value)64U };
			//topBorder.Append(color);
			//BottomBorder bottomBorder = new BottomBorder() { Style = BorderStyleValues.Thin };
			//color = new DocumentFormat.OpenXml.Spreadsheet.Color() { Indexed = (UInt32Value)64U };
			//bottomBorder.Append(color);

			//border.Append(leftBorder);
			//border.Append(rightBorder);
			//border.Append(topBorder);
			//border.Append(bottomBorder);

			//// Index 1 - OutSide border.
			//workbookStylesPart.Stylesheet.Borders.Append(border);

			//// Index 2 - TOP -BOT BORDER
			//border = new Border();
			//topBorder = new TopBorder() { Style = BorderStyleValues.Thin };
			//bottomBorder = new BottomBorder() { Style = BorderStyleValues.Thin };

			//border.Append(topBorder);
			//border.Append(bottomBorder);
			//workbookStylesPart.Stylesheet.Borders.Append(border);

			//// CellFormats
			//workbookStylesPart.Stylesheet.CellFormats = new CellFormats();
			//// Index 0 - default
			//workbookStylesPart.Stylesheet.CellFormats.Append(new CellFormat() { FontId = 0, FillId = 0, BorderId = 0 });

			//// Index 1 - Report Title
			//workbookStylesPart.Stylesheet.CellFormats.Append(new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }) { FontId = 1, FillId = 0, BorderId = 0, ApplyFont = true });

			//// Index 2 - Text Green
			//workbookStylesPart.Stylesheet.CellFormats.Append(new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }) { FontId = 2, FillId = 0, BorderId = 0, ApplyFont = true });

			//// Index 3 - Text Yellow
			//workbookStylesPart.Stylesheet.CellFormats.Append(new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }) { FontId = 3, FillId = 0, BorderId = 0, ApplyFont = true });

			//// Index 4 - Text Red
			//workbookStylesPart.Stylesheet.CellFormats.Append(new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }) { FontId = 4, FillId = 0, BorderId = 0, ApplyFont = true });

			//// Index 5 - Field Grey
			//workbookStylesPart.Stylesheet.CellFormats.Append(new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }) { FontId = 5, FillId = 5, BorderId = 0, ApplyFont = true });

			//// Index 6 - Field Green
			//workbookStylesPart.Stylesheet.CellFormats.Append(new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }) { FontId = 0, FillId = 2, BorderId = 0, ApplyFont = true });

			//// Index 7 - Field Green
			//workbookStylesPart.Stylesheet.CellFormats.Append(new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }) { FontId = 0, FillId = 3, BorderId = 0, ApplyFont = true });

			//// Index 8 - Field Green
			//workbookStylesPart.Stylesheet.CellFormats.Append(new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }) { FontId = 0, FillId = 4, BorderId = 0, ApplyFont = true });

			//// Index 9 - FONT_CONTENT_TEXT_MARGIN_MIDDLE
			//workbookStylesPart.Stylesheet.CellFormats.Append(new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }) { FontId = 0, FillId = 0, BorderId = 0, ApplyFont = true });

			//// Index 10 - FONT_CONTENT_TEXT_MARGIN_RIGHT
			//workbookStylesPart.Stylesheet.CellFormats.Append(new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Right, Vertical = VerticalAlignmentValues.Center }) { FontId = 0, FillId = 0, BorderId = 0, ApplyFont = true });

			//// Index 11 - FONT_CONTENT_TEXT_MARGIN_LEFT_BOLD
			//workbookStylesPart.Stylesheet.CellFormats.Append(new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }) { FontId = 5, FillId = 0, BorderId = 0, ApplyFont = true });

			//// Index 12 - Field Grey - border
			//workbookStylesPart.Stylesheet.CellFormats.Append(new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }) { FontId = 5, FillId = 5, BorderId = 2, ApplyFont = true });

			//// Index 13 - Field Grey - Left
			//workbookStylesPart.Stylesheet.CellFormats.Append(new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Left, Vertical = VerticalAlignmentValues.Center }) { FontId = 5, FillId = 5, BorderId = 0, ApplyFont = true });
			#endregion
		}

		// Generates content of themePart.
		private void GenerateThemePartContent(ThemePart themePart)
		{
			A.Theme theme = new A.Theme() { Name = "Office Theme" };
			theme.AddNamespaceDeclaration("a", "http://schemas.openxmlformats.org/drawingml/2006/main");

			A.ThemeElements themeElements = new A.ThemeElements();

			A.ColorScheme colorScheme = new A.ColorScheme() { Name = "Office" };

			A.Dark1Color dark1Color = new A.Dark1Color();
			A.SystemColor systemColor = new A.SystemColor() { Val = A.SystemColorValues.WindowText, LastColor = "000000" };

			dark1Color.Append(systemColor);

			A.Light1Color light1Color = new A.Light1Color();
			systemColor = new A.SystemColor() { Val = A.SystemColorValues.Window, LastColor = "FFFFFF" };

			light1Color.Append(systemColor);

			A.Dark2Color dark2Color = new A.Dark2Color();
			A.RgbColorModelHex rgbColorModelHex = new A.RgbColorModelHex() { Val = "1F497D" };

			dark2Color.Append(rgbColorModelHex);

			A.Light2Color light2Color = new A.Light2Color();
			rgbColorModelHex = new A.RgbColorModelHex() { Val = "EEECE1" };

			light2Color.Append(rgbColorModelHex);

			A.Accent1Color accent1Color = new A.Accent1Color();
			rgbColorModelHex = new A.RgbColorModelHex() { Val = "4F81BD" };

			accent1Color.Append(rgbColorModelHex);

			A.Accent2Color accent2Color = new A.Accent2Color();
			rgbColorModelHex = new A.RgbColorModelHex() { Val = "C0504D" };

			accent2Color.Append(rgbColorModelHex);

			A.Accent3Color accent3Color = new A.Accent3Color();
			rgbColorModelHex = new A.RgbColorModelHex() { Val = "9BBB59" };

			accent3Color.Append(rgbColorModelHex);

			A.Accent4Color accent4Color = new A.Accent4Color();
			rgbColorModelHex = new A.RgbColorModelHex() { Val = "8064A2" };

			accent4Color.Append(rgbColorModelHex);

			A.Accent5Color accent5Color = new A.Accent5Color();
			rgbColorModelHex = new A.RgbColorModelHex() { Val = "4BACC6" };

			accent5Color.Append(rgbColorModelHex);

			A.Accent6Color accent6Color = new A.Accent6Color();
			rgbColorModelHex = new A.RgbColorModelHex() { Val = "F79646" };

			accent6Color.Append(rgbColorModelHex);

			A.Hyperlink hyperlink = new A.Hyperlink();
			rgbColorModelHex = new A.RgbColorModelHex() { Val = "0000FF" };

			hyperlink.Append(rgbColorModelHex);

			A.FollowedHyperlinkColor followedHyperlinkColor = new A.FollowedHyperlinkColor();
			rgbColorModelHex = new A.RgbColorModelHex() { Val = "800080" };

			followedHyperlinkColor.Append(rgbColorModelHex);

			colorScheme.Append(dark1Color);
			colorScheme.Append(light1Color);
			colorScheme.Append(dark2Color);
			colorScheme.Append(light2Color);
			colorScheme.Append(accent1Color);
			colorScheme.Append(accent2Color);
			colorScheme.Append(accent3Color);
			colorScheme.Append(accent4Color);
			colorScheme.Append(accent5Color);
			colorScheme.Append(accent6Color);
			colorScheme.Append(hyperlink);
			colorScheme.Append(followedHyperlinkColor);

			A.FontScheme fontScheme = new A.FontScheme() { Name = "Office" };

			A.MajorFont majorFont = new A.MajorFont();
			A.LatinFont latinFont = new A.LatinFont() { Typeface = "Cambria" };
			majorFont.Append(latinFont);
			A.EastAsianFont eastAsianFont = new A.EastAsianFont() { Typeface = "" };
			majorFont.Append(eastAsianFont);
			A.ComplexScriptFont complexScriptFont = new A.ComplexScriptFont() { Typeface = "" };
			majorFont.Append(complexScriptFont);

			A.SupplementalFont supplementalFont = new A.SupplementalFont() { Script = "Jpan", Typeface = "ＭＳ Ｐゴシック" };
			majorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Jpan", Typeface = "ＭＳ Ｐゴシック" };
			majorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Hang", Typeface = "맑은 고딕" };
			majorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Hans", Typeface = "宋体" };
			majorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Hant", Typeface = "新細明體" };
			majorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Arab", Typeface = "Times New Roman" };
			majorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Hebr", Typeface = "Times New Roman" };
			majorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Thai", Typeface = "Tahoma" };
			majorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Ethi", Typeface = "Nyala" };
			majorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Beng", Typeface = "Vrinda" };
			majorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Gujr", Typeface = "Shruti" };
			majorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Khmr", Typeface = "MoolBoran" };
			majorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Knda", Typeface = "Tunga" };
			majorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Guru", Typeface = "Raavi" };
			majorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Cans", Typeface = "Euphemia" };
			majorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Cher", Typeface = "Plantagenet Cherokee" };
			majorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Yiii", Typeface = "Microsoft Yi Baiti" };
			majorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Tibt", Typeface = "Microsoft Himalaya" };
			majorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Thaa", Typeface = "MV Boli" };
			majorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Deva", Typeface = "Mangal" };
			majorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Telu", Typeface = "Gautami" };
			majorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Taml", Typeface = "Latha" };
			majorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Syrc", Typeface = "Estrangelo Edessa" };
			majorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Orya", Typeface = "Kalinga" };
			majorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Mlym", Typeface = "Kartika" };
			majorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Laoo", Typeface = "DokChampa" };
			majorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Sinh", Typeface = "Iskoola Pota" };
			majorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Mong", Typeface = "Mongolian Baiti" };
			majorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Viet", Typeface = "Times New Roman" };
			majorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Uigh", Typeface = "Microsoft Uighur" };
			majorFont.Append(supplementalFont);

			fontScheme.Append(majorFont);

			A.MinorFont minorFont = new A.MinorFont();
			latinFont = new A.LatinFont() { Typeface = "Calibri" };
			minorFont.Append(latinFont);
			eastAsianFont = new A.EastAsianFont() { Typeface = "" };
			minorFont.Append(eastAsianFont);
			complexScriptFont = new A.ComplexScriptFont() { Typeface = "" };
			minorFont.Append(complexScriptFont);

			supplementalFont = new A.SupplementalFont() { Script = "Jpan", Typeface = "ＭＳ Ｐゴシック" };
			minorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Jpan", Typeface = "ＭＳ Ｐゴシック" };
			minorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Hang", Typeface = "맑은 고딕" };
			minorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Hans", Typeface = "宋体" };
			minorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Hant", Typeface = "新細明體" };
			minorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Arab", Typeface = "Times New Roman" };
			minorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Hebr", Typeface = "Times New Roman" };
			minorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Thai", Typeface = "Tahoma" };
			minorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Ethi", Typeface = "Nyala" };
			minorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Beng", Typeface = "Vrinda" };
			minorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Gujr", Typeface = "Shruti" };
			minorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Khmr", Typeface = "MoolBoran" };
			minorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Knda", Typeface = "Tunga" };
			minorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Guru", Typeface = "Raavi" };
			minorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Cans", Typeface = "Euphemia" };
			minorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Cher", Typeface = "Plantagenet Cherokee" };
			minorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Yiii", Typeface = "Microsoft Yi Baiti" };
			minorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Tibt", Typeface = "Microsoft Himalaya" };
			minorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Thaa", Typeface = "MV Boli" };
			minorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Deva", Typeface = "Mangal" };
			minorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Telu", Typeface = "Gautami" };
			minorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Taml", Typeface = "Latha" };
			minorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Syrc", Typeface = "Estrangelo Edessa" };
			minorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Orya", Typeface = "Kalinga" };
			minorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Mlym", Typeface = "Kartika" };
			minorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Laoo", Typeface = "DokChampa" };
			minorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Sinh", Typeface = "Iskoola Pota" };
			minorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Mong", Typeface = "Mongolian Baiti" };
			minorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Viet", Typeface = "Times New Roman" };
			minorFont.Append(supplementalFont);
			supplementalFont = new A.SupplementalFont() { Script = "Uigh", Typeface = "Microsoft Uighur" };
			minorFont.Append(supplementalFont);

			fontScheme.Append(minorFont);

			A.FormatScheme formatScheme = new A.FormatScheme() { Name = "Office" };

			A.FillStyleList fillStyleList = new A.FillStyleList();

			A.SolidFill solidFill = new A.SolidFill();
			A.SchemeColor schemeColor = new A.SchemeColor() { Val = A.SchemeColorValues.PhColor };

			solidFill.Append(schemeColor);

			A.GradientFill gradientFill = new A.GradientFill() { RotateWithShape = true };

			A.GradientStopList gradientStopList = new A.GradientStopList();

			A.GradientStop gradientStop = new A.GradientStop() { Position = 0 };

			schemeColor = new A.SchemeColor() { Val = A.SchemeColorValues.PhColor };
			A.Tint tint = new A.Tint() { Val = 50000 };
			A.SaturationModulation saturationModulation = new A.SaturationModulation() { Val = 300000 };

			schemeColor.Append(tint);
			schemeColor.Append(saturationModulation);

			gradientStop.Append(schemeColor);
			gradientStopList.Append(gradientStop);

			gradientStop = new A.GradientStop() { Position = 35000 };

			schemeColor = new A.SchemeColor() { Val = A.SchemeColorValues.PhColor };
			tint = new A.Tint() { Val = 37000 };
			saturationModulation = new A.SaturationModulation() { Val = 300000 };

			schemeColor.Append(tint);
			schemeColor.Append(saturationModulation);

			gradientStop.Append(schemeColor);
			gradientStopList.Append(gradientStop);

			gradientStop = new A.GradientStop() { Position = 100000 };

			schemeColor = new A.SchemeColor() { Val = A.SchemeColorValues.PhColor };
			tint = new A.Tint() { Val = 15000 };
			saturationModulation = new A.SaturationModulation() { Val = 350000 };

			schemeColor.Append(tint);
			schemeColor.Append(saturationModulation);

			gradientStop.Append(schemeColor);
			gradientStopList.Append(gradientStop);

			A.LinearGradientFill linearGradientFill = new A.LinearGradientFill() { Angle = 16200000, Scaled = true };

			gradientFill.Append(gradientStopList);
			gradientFill.Append(linearGradientFill);
			fillStyleList.Append(gradientFill);

			gradientFill = new A.GradientFill() { RotateWithShape = true };

			gradientStopList = new A.GradientStopList();

			gradientStop = new A.GradientStop() { Position = 0 };

			schemeColor = new A.SchemeColor() { Val = A.SchemeColorValues.PhColor };
			A.Shade shade = new A.Shade() { Val = 51000 };
			saturationModulation = new A.SaturationModulation() { Val = 130000 };

			schemeColor.Append(shade);
			schemeColor.Append(saturationModulation);

			gradientStop.Append(schemeColor);
			gradientStopList.Append(gradientStop);

			gradientStop = new A.GradientStop() { Position = 80000 };

			schemeColor = new A.SchemeColor() { Val = A.SchemeColorValues.PhColor };
			shade = new A.Shade() { Val = 93000 };
			saturationModulation = new A.SaturationModulation() { Val = 130000 };

			schemeColor.Append(shade);
			schemeColor.Append(saturationModulation);

			gradientStop.Append(schemeColor);
			gradientStopList.Append(gradientStop);

			gradientStop = new A.GradientStop() { Position = 100000 };

			schemeColor = new A.SchemeColor() { Val = A.SchemeColorValues.PhColor };
			shade = new A.Shade() { Val = 94000 };
			saturationModulation = new A.SaturationModulation() { Val = 135000 };

			schemeColor.Append(shade);
			schemeColor.Append(saturationModulation);

			gradientStop.Append(schemeColor);
			gradientStopList.Append(gradientStop);

			linearGradientFill = new A.LinearGradientFill() { Angle = 16200000, Scaled = false };

			gradientFill.Append(gradientStopList);
			gradientFill.Append(linearGradientFill);

			fillStyleList.Append(solidFill);
			fillStyleList.Append(gradientFill);

			A.LineStyleList lineStyleList = new A.LineStyleList();

			A.Outline outline = new A.Outline() { Width = 9525, CapType = A.LineCapValues.Flat, CompoundLineType = A.CompoundLineValues.Single, Alignment = A.PenAlignmentValues.Center };

			solidFill = new A.SolidFill();

			schemeColor = new A.SchemeColor() { Val = A.SchemeColorValues.PhColor };
			shade = new A.Shade() { Val = 95000 };
			saturationModulation = new A.SaturationModulation() { Val = 105000 };

			schemeColor.Append(shade);
			schemeColor.Append(saturationModulation);

			solidFill.Append(schemeColor);
			A.PresetDash presetDash = new A.PresetDash() { Val = A.PresetLineDashValues.Solid };

			outline.Append(solidFill);
			outline.Append(presetDash);
			lineStyleList.Append(outline);

			outline = new A.Outline() { Width = 25400, CapType = A.LineCapValues.Flat, CompoundLineType = A.CompoundLineValues.Single, Alignment = A.PenAlignmentValues.Center };

			solidFill = new A.SolidFill();
			schemeColor = new A.SchemeColor() { Val = A.SchemeColorValues.PhColor };

			solidFill.Append(schemeColor);
			presetDash = new A.PresetDash() { Val = A.PresetLineDashValues.Solid };

			outline.Append(solidFill);
			outline.Append(presetDash);
			lineStyleList.Append(outline);

			outline = new A.Outline() { Width = 38100, CapType = A.LineCapValues.Flat, CompoundLineType = A.CompoundLineValues.Single, Alignment = A.PenAlignmentValues.Center };

			solidFill = new A.SolidFill();
			schemeColor = new A.SchemeColor() { Val = A.SchemeColorValues.PhColor };

			solidFill.Append(schemeColor);
			presetDash = new A.PresetDash() { Val = A.PresetLineDashValues.Solid };

			outline.Append(solidFill);
			outline.Append(presetDash);
			lineStyleList.Append(outline);


			A.EffectStyleList effectStyleList = new A.EffectStyleList();

			A.EffectStyle effectStyle = new A.EffectStyle();

			A.EffectList effectList = new A.EffectList();

			A.OuterShadow outerShadow = new A.OuterShadow() { BlurRadius = 40000L, Distance = 20000L, Direction = 5400000, RotateWithShape = false };

			rgbColorModelHex = new A.RgbColorModelHex() { Val = "000000" };
			A.Alpha alpha = new A.Alpha() { Val = 38000 };

			rgbColorModelHex.Append(alpha);

			outerShadow.Append(rgbColorModelHex);

			effectList.Append(outerShadow);

			effectStyle.Append(effectList);
			effectStyleList.Append(effectStyle);

			effectStyle = new A.EffectStyle();

			effectList = new A.EffectList();

			outerShadow = new A.OuterShadow() { BlurRadius = 40000L, Distance = 23000L, Direction = 5400000, RotateWithShape = false };

			rgbColorModelHex = new A.RgbColorModelHex() { Val = "000000" };
			alpha = new A.Alpha() { Val = 35000 };

			rgbColorModelHex.Append(alpha);

			outerShadow.Append(rgbColorModelHex);

			effectList.Append(outerShadow);

			effectStyle.Append(effectList);
			effectStyleList.Append(effectStyle);

			effectStyle = new A.EffectStyle();

			effectList = new A.EffectList();

			outerShadow = new A.OuterShadow() { BlurRadius = 40000L, Distance = 23000L, Direction = 5400000, RotateWithShape = false };

			rgbColorModelHex = new A.RgbColorModelHex() { Val = "000000" };
			alpha = new A.Alpha() { Val = 35000 };

			rgbColorModelHex.Append(alpha);

			outerShadow.Append(rgbColorModelHex);

			effectList.Append(outerShadow);

			A.Scene3DType scene3DType = new A.Scene3DType();

			A.Camera camera = new A.Camera() { Preset = A.PresetCameraValues.OrthographicFront };
			A.Rotation rotation = new A.Rotation() { Latitude = 0, Longitude = 0, Revolution = 0 };

			camera.Append(rotation);

			A.LightRig lightRig = new A.LightRig() { Rig = A.LightRigValues.ThreePoints, Direction = A.LightRigDirectionValues.Top };
			rotation = new A.Rotation() { Latitude = 0, Longitude = 0, Revolution = 1200000 };

			lightRig.Append(rotation);

			scene3DType.Append(camera);
			scene3DType.Append(lightRig);

			A.Shape3DType shape3DType = new A.Shape3DType();
			A.BevelTop bevelTop = new A.BevelTop() { Width = 63500L, Height = 25400L };

			shape3DType.Append(bevelTop);

			effectStyle.Append(effectList);
			effectStyle.Append(scene3DType);
			effectStyle.Append(shape3DType);
			effectStyleList.Append(effectStyle);

			A.BackgroundFillStyleList backgroundFillStyleList = new A.BackgroundFillStyleList();

			solidFill = new A.SolidFill();
			schemeColor = new A.SchemeColor() { Val = A.SchemeColorValues.PhColor };

			solidFill.Append(schemeColor);

			gradientFill = new A.GradientFill() { RotateWithShape = true };

			gradientStopList = new A.GradientStopList();

			gradientStop = new A.GradientStop() { Position = 0 };

			schemeColor = new A.SchemeColor() { Val = A.SchemeColorValues.PhColor };
			tint = new A.Tint() { Val = 40000 };
			saturationModulation = new A.SaturationModulation() { Val = 350000 };

			schemeColor.Append(tint);
			schemeColor.Append(saturationModulation);

			gradientStop.Append(schemeColor);
			gradientStopList.Append(gradientStop);

			gradientStop = new A.GradientStop() { Position = 40000 };

			schemeColor = new A.SchemeColor() { Val = A.SchemeColorValues.PhColor };
			tint = new A.Tint() { Val = 45000 };
			shade = new A.Shade() { Val = 99000 };
			saturationModulation = new A.SaturationModulation() { Val = 350000 };

			schemeColor.Append(tint);
			schemeColor.Append(shade);
			schemeColor.Append(saturationModulation);

			gradientStop.Append(schemeColor);
			gradientStopList.Append(gradientStop);

			gradientStop = new A.GradientStop() { Position = 100000 };

			schemeColor = new A.SchemeColor() { Val = A.SchemeColorValues.PhColor };
			shade = new A.Shade() { Val = 20000 };
			saturationModulation = new A.SaturationModulation() { Val = 255000 };

			schemeColor.Append(shade);
			schemeColor.Append(saturationModulation);

			gradientStop.Append(schemeColor);
			gradientStopList.Append(gradientStop);

			A.PathGradientFill pathGradientFill = new A.PathGradientFill() { Path = A.PathShadeValues.Circle };
			A.FillToRectangle fillToRectangle = new A.FillToRectangle() { Left = 50000, Top = -80000, Right = 50000, Bottom = 180000 };

			pathGradientFill.Append(fillToRectangle);

			gradientFill.Append(gradientStopList);
			gradientFill.Append(pathGradientFill);
			backgroundFillStyleList.Append(gradientFill);

			gradientFill = new A.GradientFill() { RotateWithShape = true };

			gradientStopList = new A.GradientStopList();

			gradientStop = new A.GradientStop() { Position = 0 };

			schemeColor = new A.SchemeColor() { Val = A.SchemeColorValues.PhColor };
			tint = new A.Tint() { Val = 80000 };
			saturationModulation = new A.SaturationModulation() { Val = 300000 };

			schemeColor.Append(tint);
			schemeColor.Append(saturationModulation);

			gradientStop.Append(schemeColor);
			gradientStopList.Append(gradientStop);

			gradientStop = new A.GradientStop() { Position = 100000 };

			schemeColor = new A.SchemeColor() { Val = A.SchemeColorValues.PhColor };
			shade = new A.Shade() { Val = 30000 };
			saturationModulation = new A.SaturationModulation() { Val = 200000 };

			schemeColor.Append(shade);
			schemeColor.Append(saturationModulation);

			gradientStop.Append(schemeColor);
			gradientStopList.Append(gradientStop);

			pathGradientFill = new A.PathGradientFill() { Path = A.PathShadeValues.Circle };
			fillToRectangle = new A.FillToRectangle() { Left = 50000, Top = 50000, Right = 50000, Bottom = 50000 };

			pathGradientFill.Append(fillToRectangle);

			gradientFill.Append(gradientStopList);
			gradientFill.Append(pathGradientFill);

			backgroundFillStyleList.Append(solidFill);
			backgroundFillStyleList.Append(gradientFill);

			formatScheme.Append(fillStyleList);
			formatScheme.Append(lineStyleList);
			formatScheme.Append(effectStyleList);
			formatScheme.Append(backgroundFillStyleList);

			themeElements.Append(colorScheme);
			themeElements.Append(fontScheme);
			themeElements.Append(formatScheme);

			A.ObjectDefaults objectDefaults = new A.ObjectDefaults();
			A.ExtraColorSchemeList extraColorSchemeList = new A.ExtraColorSchemeList();

			theme.Append(themeElements);
			theme.Append(objectDefaults);
			theme.Append(extraColorSchemeList);

			themePart.Theme = theme;
		}

		// Generates content of worksheetPart.
		private void GenerateWorksheetPartContent(WorksheetPart worksheetPart)
		{
			Worksheet worksheet = new Worksheet();
			worksheet.AddNamespaceDeclaration("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
			//SheetDimension sheetDimension1 = new SheetDimension() { Reference = "A1" };

			SheetViews sheetViews = new SheetViews();
			SheetView sheetView = new SheetView() { TabSelected = true, WorkbookViewId = (UInt32Value)0U, View = SheetViewValues.PageLayout };
			Selection selection = new Selection() { ActiveCell = "A1", SequenceOfReferences = new ListValue<StringValue>() { InnerText = "A1" } };

			sheetView.Append(selection);
			sheetViews.Append(sheetView);
			SheetFormatProperties sheetFormatProperties = new SheetFormatProperties() { DefaultRowHeight = 15D };
			SheetData sheetData = new SheetData();
			PageMargins pageMargins = new PageMargins() { Left = 0.7D, Right = 0.7D, Top = 0.75D, Bottom = 0.75D, Header = 0.3D, Footer = 0.3D };

			MergeCells mergeCells = new MergeCells();
			//CustomMergeCell(ref sheetData1, ref mergeCells1, "AA1", "AA1", "", App_Cons.FONT_CONTENT_TEXT_MARGIN_MIDDLE);
			//-----------------------------------

			Columns cols = new Columns();
			Column col1 = cols.AppendChild(new Column() { Min = (UInt32Value)1U, Max = (UInt32Value)5000U, Width = 6.9D, CustomWidth = true });

			PageSetup pageSetup = new PageSetup() { Orientation = OrientationValues.Landscape, Id = "Report" };

			worksheet.Append(sheetViews);
			worksheet.Append(sheetFormatProperties);
			worksheet.Append(cols);
			worksheet.Append(sheetData);
			worksheet.Append(mergeCells);
			worksheet.Append(pageMargins);
			worksheet.Append(pageSetup);

			worksheetPart.Worksheet = worksheet;
		}

		// Generates content of sharedStringTablePart.
		private void GenerateSharedStringTablePartContent(SharedStringTablePart sharedStringTablePart)
		{
			SharedStringTable sharedStringTable = new SharedStringTable() { Count = (UInt32Value)1U, UniqueCount = (UInt32Value)1U };

			SharedStringItem sharedStringItem = new SharedStringItem();
			Text text = new Text();
			text.Text = "";

			sharedStringItem.Append(text);

			sharedStringTable.Append(sharedStringItem);

			sharedStringTablePart.SharedStringTable = sharedStringTable;
		}

		//Chart Color for DoughnutChart
		private void GenerateChartColorStylePartContent(ChartColorStylePart chartColorStylePart)
		{
			Cs.ColorStyle colorStyle = new Cs.ColorStyle() { Method = "cycle", Id = (UInt32Value)10U };
			colorStyle.AddNamespaceDeclaration("cs", "http://schemas.microsoft.com/office/drawing/2012/chartStyle");
			colorStyle.AddNamespaceDeclaration("a", "http://schemas.openxmlformats.org/drawingml/2006/main");

			A.SchemeColor schemeColor = new A.SchemeColor() { Val = A.SchemeColorValues.Accent1 };
			colorStyle.Append(schemeColor);

			schemeColor = new A.SchemeColor() { Val = A.SchemeColorValues.Accent2 };
			colorStyle.Append(schemeColor);

			schemeColor = new A.SchemeColor() { Val = A.SchemeColorValues.Accent3 };
			colorStyle.Append(schemeColor);

			schemeColor = new A.SchemeColor() { Val = A.SchemeColorValues.Accent4 };
			colorStyle.Append(schemeColor);

			schemeColor = new A.SchemeColor() { Val = A.SchemeColorValues.Accent5 };
			colorStyle.Append(schemeColor);

			schemeColor = new A.SchemeColor() { Val = A.SchemeColorValues.Accent6 };
			colorStyle.Append(schemeColor);

			Cs.ColorStyleVariation colorStyleVariation = new Cs.ColorStyleVariation();
			colorStyle.Append(colorStyleVariation);

			colorStyleVariation = new Cs.ColorStyleVariation();
			A.LuminanceModulation luminanceModulation = new A.LuminanceModulation() { Val = 60000 };

			colorStyleVariation.Append(luminanceModulation);
			colorStyle.Append(colorStyleVariation);

			colorStyleVariation = new Cs.ColorStyleVariation();
			luminanceModulation = new A.LuminanceModulation() { Val = 80000 };
			A.LuminanceOffset luminanceOffset = new A.LuminanceOffset() { Val = 20000 };

			colorStyleVariation.Append(luminanceModulation);
			colorStyleVariation.Append(luminanceOffset);
			colorStyle.Append(colorStyleVariation);

			colorStyleVariation = new Cs.ColorStyleVariation();
			luminanceModulation = new A.LuminanceModulation() { Val = 80000 };

			colorStyleVariation.Append(luminanceModulation);
			colorStyle.Append(colorStyleVariation);

			colorStyleVariation = new Cs.ColorStyleVariation();
			luminanceModulation = new A.LuminanceModulation() { Val = 60000 };
			luminanceOffset = new A.LuminanceOffset() { Val = 40000 };

			colorStyleVariation.Append(luminanceModulation);
			colorStyleVariation.Append(luminanceOffset);
			colorStyle.Append(colorStyleVariation);

			colorStyleVariation = new Cs.ColorStyleVariation();
			luminanceModulation = new A.LuminanceModulation() { Val = 50000 };

			colorStyleVariation.Append(luminanceModulation);
			colorStyle.Append(colorStyleVariation);

			colorStyleVariation = new Cs.ColorStyleVariation();
			luminanceModulation = new A.LuminanceModulation() { Val = 70000 };
			luminanceOffset = new A.LuminanceOffset() { Val = 30000 };

			colorStyleVariation.Append(luminanceModulation);
			colorStyleVariation.Append(luminanceOffset);
			colorStyle.Append(colorStyleVariation);

			colorStyleVariation = new Cs.ColorStyleVariation();
			luminanceModulation = new A.LuminanceModulation() { Val = 70000 };

			colorStyleVariation.Append(luminanceModulation);
			colorStyle.Append(colorStyleVariation);

			colorStyleVariation = new Cs.ColorStyleVariation();
			luminanceModulation = new A.LuminanceModulation() { Val = 50000 };
			luminanceOffset = new A.LuminanceOffset() { Val = 50000 };

			colorStyleVariation.Append(luminanceModulation);
			colorStyleVariation.Append(luminanceOffset);
			colorStyle.Append(colorStyleVariation);

			chartColorStylePart.ColorStyle = colorStyle;
		}

		private void SetPackageProperties(OpenXmlPackage document)
		{
			document.PackageProperties.Creator = "Thang Pham";
		}

		/** COMMON METHOD **/
		public static void MergeCellCustom(ref SheetData sheetdata, ref MergeCells mergeCells, string fCell, string tCell, string value, int rowIndex, int styleIndex)
		{
			Cell cell = new Cell();
			cell.CellReference = fCell;
			cell.DataType = CellValues.InlineString;
			cell.InlineString = new InlineString() { Text = new Text(value) };
			cell.StyleIndex = (uint)styleIndex;

			Row row = new Row { RowIndex = (UInt32)rowIndex };
			row.Append(cell);
			sheetdata.Append(row);

			string cellRef = string.Format("{0}:{1}", fCell, tCell);
			MergeCell mergeCell = new MergeCell() { Reference = cellRef };
			mergeCells.Append(mergeCell);
		}

		public static void MergeCellCustom(Row row, ref MergeCells mergeCells, string fCell, string tCell, string value, int rowIndex, int styleIndex)
		{
			Cell cell = new Cell();
			cell.CellReference = fCell;
			cell.DataType = CellValues.InlineString;
			cell.InlineString = new InlineString() { Text = new Text(value) };
			cell.StyleIndex = (uint)styleIndex;

			row.Append(cell);

			MergerCellCustom(ref mergeCells, fCell, tCell);
		}

		private static void MergerCellCustom(ref MergeCells mergeCells, string fromCell, string toCell)
		{
			string cellRef = string.Format("{0}:{1}", fromCell, toCell);
			MergeCell mergeCell = new MergeCell() { Reference = cellRef };
			mergeCells.Append(mergeCell);
		}

		public static void CustomInsertPicture(ref WorksheetPart wsp, ref Worksheet ws, string sImagePath)
		{
			DrawingsPart dp = wsp.AddNewPart<DrawingsPart>();
			ImagePart imgp = dp.AddImagePart(ImagePartType.Png, wsp.GetIdOfPart(dp));
			using (FileStream fs = new FileStream(sImagePath, FileMode.Open))
			{
				imgp.FeedData(fs);
			}

			NonVisualDrawingProperties nvdp = new NonVisualDrawingProperties();
			nvdp.Id = 1025;
			nvdp.Name = "Picture 1";
			nvdp.Description = "logo";
			A.PictureLocks picLocks = new A.PictureLocks();
			picLocks.NoChangeAspect = true;
			picLocks.NoChangeArrowheads = true;
			NonVisualPictureDrawingProperties nvpdp = new NonVisualPictureDrawingProperties();
			nvpdp.PictureLocks = picLocks;
			NonVisualPictureProperties nvpp = new NonVisualPictureProperties();
			nvpp.NonVisualDrawingProperties = nvdp;
			nvpp.NonVisualPictureDrawingProperties = nvpdp;

			A.Stretch stretch = new A.Stretch();
			stretch.FillRectangle = new A.FillRectangle();

			BlipFill blipFill = new BlipFill();
			A.Blip blip = new A.Blip();
			blip.Embed = dp.GetIdOfPart(imgp);
			blip.CompressionState = A.BlipCompressionValues.Print;
			blipFill.Blip = blip;
			blipFill.SourceRectangle = new A.SourceRectangle();
			blipFill.Append(stretch);

			A.Transform2D t2d = new A.Transform2D();
			A.Offset offset = new A.Offset();
			offset.X = 0;
			offset.Y = 0;
			t2d.Offset = offset;
			Bitmap bm = new Bitmap(sImagePath);

			A.Extents extents = new A.Extents();
			extents.Cx = (long)bm.Width * (long)((float)914400 / bm.HorizontalResolution);
			extents.Cy = (long)bm.Height * (long)((float)914400 / bm.VerticalResolution);
			bm.Dispose();
			t2d.Extents = extents;
			ShapeProperties sp = new ShapeProperties();
			sp.BlackWhiteMode = A.BlackWhiteModeValues.Auto;
			sp.Transform2D = t2d;
			A.PresetGeometry prstGeom = new A.PresetGeometry();
			prstGeom.Preset = A.ShapeTypeValues.Rectangle;
			prstGeom.AdjustValueList = new A.AdjustValueList();
			sp.Append(prstGeom);
			sp.Append(new A.NoFill());

			Xdr.Picture picture = new Xdr.Picture();
			picture.NonVisualPictureProperties = nvpp;
			picture.BlipFill = blipFill;
			picture.ShapeProperties = sp;

			Position pos = new Position();
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

		public static string GetColumnName(int columnIndex)
		{
			int dividend = columnIndex;
			string columnName = String.Empty;
			int modifier;

			while (dividend > 0)
			{
				modifier = (dividend - 1) % 26;
				columnName = Convert.ToChar(65 + modifier).ToString() + columnName;
				dividend = (int)((dividend - modifier) / 26);
			}

			return columnName;
		}

		public static void GenerateVmlDrawingPartContent(VmlDrawingPart vmlDrawingPart)
		{
			//Insert Logo: header, footer
			System.Xml.XmlTextWriter writer = new System.Xml.XmlTextWriter(vmlDrawingPart.GetStream(FileMode.Create), Encoding.UTF8);
			//string xmlString = "<xml xmlns:v=\"urn: schemas - microsoft - com:vml\"\r\n xmlns:o=\"urn:schemas-microsoft-com:office:office\"\r\n" +
			//	"xmlns: x =\"urn:schemas-microsoft-com:office:excel\">\r\n <o:shapelayout v:ext=\"edit\">\r\n" +
			//	"<o:idmap v:ext=\"edit\" data=\"1\"/>\r\n </o:shapelayout><v:shapetype id=\"_x0000_t75\" coordsize=\"21600,21600\"" +
			//	"o:spt=\"75\"\r\n  o:preferrelative=\"t\" path=\"m@4@5l@4@11@9@11@9@5xe\" filled=\"f\" stroked=\"f\">\r\n" +
			//	"<v:stroke joinstyle=\"miter\"/>\r\n  <v:formulas>\r\n   <v:f eqn=\"if lineDrawn pixelLineWidth 0\"/>\r\n" +
			//	"<v:f eqn=\"sum @0 1 0\"/>\r\n   <v:f eqn=\"sum 0 0 @1\"/>\r\n   <v:f eqn=\"prod @2 1 2\"/>\r\n" +
			//	"<v:f eqn=\"prod @3 21600 pixelWidth\"/>\r\n   <v:f eqn=\"prod @3 21600 pixelHeight\"/>\r\n" +
			//	"<v:f eqn=\"sum @0 0 1\"/>\r\n   <v:f eqn=\"prod @6 1 2\"/>\r\n   <v:f eqn=\"prod @7 21600 pixelWidth\"/>\r\n" +
			//	"<v:f eqn=\"sum @8 21600 0\"/>\r\n   <v:f eqn=\"prod @7 21600 pixelHeight\"/>\r\n   <v:f eqn=\"sum @10 21600 0\"/>\r\n" +
			//	"</v:formulas>\r\n  <v:path o:extrusionok=\"f\" gradientshapeok=\"t\" o:connecttype=\"rect\"/>\r\n" +
			//	"<o:lock v:ext=\"edit\" aspectratio=\"t\"/>\r\n </v:shapetype>" +
			//	"<v:shape id=\"LH\" o:spid=\"_x0000_s1025\" type=\"#_x0000_t75\"\r\n" +
			//	"style=\'position:absolute;margin-left:0;margin-top:0;width:154.5pt;height:72pt;\r\n  z-index:1\' o:preferrelative=\"f\">\r\n" +
			//	"<v:imagedata o:relid=\"rId1\" o:title=\"logoHeader\"/>\r\n" +
			//	"<o:lock v:ext=\"edit\" rotation=\"t\" aspectratio=\"f\"/>\r\n </v:shape>" +
			//	"</xml>";

			////"<v:shape id =\"LF\" o:spid=\"_x0000_s1025\" type=\"#_x0000_t75\"\r\n" +
			////	"style=\'position:absolute;margin-left:0;margin-top:0;width:133pt;height:31.5pt;\r\n  z-index:1\' o:preferrelative=\"f\">\r\n" +
			////	"<v:imagedata o:relid=\"rId2\" o:title=\"logoFooter\"/>\r\n" +
			////	"<o:lock v:ext=\"edit\" rotation=\"t\" aspectratio=\"f\"/>\r\n </v:shape>

			////writer.WriteRaw(xmlString);

			writer.WriteRaw("<xml xmlns:v=\"urn:schemas-microsoft-com:vml\"\r\n xmlns:o=\"urn:schemas-microsoft-com:office:office\"\r\n xmlns:x=\"urn:schemas-microsoft-com:office:excel\">\r\n <o:shapelayout v:ext=\"edit\">\r\n  <o:idmap v:ext=\"edit\" data=\"1\"/>\r\n </o:shapelayout><v:shapetype id=\"_x0000_t75\" coordsize=\"21600,21600\" o:spt=\"75\"\r\n  o:preferrelative=\"t\" path=\"m@4@5l@4@11@9@11@9@5xe\" filled=\"f\" stroked=\"f\">\r\n  <v:stroke joinstyle=\"miter\"/>\r\n  <v:formulas>\r\n   <v:f eqn=\"if lineDrawn pixelLineWidth 0\"/>\r\n   <v:f eqn=\"sum @0 1 0\"/>\r\n   <v:f eqn=\"sum 0 0 @1\"/>\r\n   <v:f eqn=\"prod @2 1 2\"/>\r\n   <v:f eqn=\"prod @3 21600 pixelWidth\"/>\r\n   <v:f eqn=\"prod @3 21600 pixelHeight\"/>\r\n   <v:f eqn=\"sum @0 0 1\"/>\r\n   <v:f eqn=\"prod @6 1 2\"/>\r\n   <v:f eqn=\"prod @7 21600 pixelWidth\"/>\r\n   <v:f eqn=\"sum @8 21600 0\"/>\r\n   <v:f eqn=\"prod @7 21600 pixelHeight\"/>\r\n   <v:f eqn=\"sum @10 21600 0\"/>\r\n  </v:formulas>\r\n  <v:path o:extrusionok=\"f\" gradientshapeok=\"t\" o:connecttype=\"rect\"/>\r\n  <o:lock v:ext=\"edit\" aspectratio=\"t\"/>\r\n </v:shapetype><v:shape id=\"LH\" o:spid=\"_x0000_s1025\" type=\"#_x0000_t75\"\r\n  style=\'position:absolute;margin-left:0;margin-top:0;width:154.5pt;height:72pt;\r\n  z-index:1\' o:preferrelative=\"f\">\r\n  <v:imagedata o:relid=\"rId1\" o:title=\"logoHeader\"/>\r\n  <o:lock v:ext=\"edit\" rotation=\"t\" aspectratio=\"f\"/>\r\n </v:shape><v:shape id=\"LF\" o:spid=\"_x0000_s1025\" type=\"#_x0000_t75\"\r\n  style=\'position:absolute;margin-left:0;margin-top:0;width:133pt;height:31.5pt;\r\n  z-index:1\' o:preferrelative=\"f\">\r\n  <v:imagedata o:relid=\"rId2\" o:title=\"logoFooter\"/>\r\n  <o:lock v:ext=\"edit\" rotation=\"t\" aspectratio=\"f\"/>\r\n </v:shape></xml>");
			writer.Flush();
			writer.Close();
		}

		public static Stream GetBinaryDataStream(string base64String)
		{
			return new MemoryStream(Convert.FromBase64String(base64String));
		}
	}
}

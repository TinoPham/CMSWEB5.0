using CMSWebApi.Utils;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.DataServices.ExportDataService.Excel
{
	public class ExcelStyles
	{
		public Stylesheet GenerateStyleSheet()
		{
			return new Stylesheet(
				//Font
				new Fonts(
					//Index 0 - default
					new Font(
						new FontSize() { Val = 8 },
						new Color() { Rgb = new HexBinaryValue() { Value = "000000" } },
						new FontName() { Val = "Calibri" }),
					//Index 1 - Report Title
					new Font(
						new Bold(),
						new FontSize() { Val = 20 },
						new Color() { Rgb = new HexBinaryValue() { Value = "000000" } },
						new FontName() { Val = "Calibri" }),
					//Index 2 - Chart Title
					new Font(
						new Bold(),
						new FontSize() { Val = 10 },
						new Color() { Rgb = new HexBinaryValue() { Value = "000000" } },
						new FontName() { Val = "Calibri" }),
					//Index 3 - Grid header
					new Font(
						new Bold(),
						new FontSize() { Val = 10 },
						new Color() { Rgb = new HexBinaryValue() { Value = ExportConst.COLOR_WHITE } },
						new FontName() { Val = "Calibri" }),
					//Index 4 - content - green
					new Font(
						new FontSize() { Val = 8 },
						new Color() { Rgb = new HexBinaryValue() { Value = ExportConst.COLOR_GREEN } },
						new FontName() { Val = "Calibri" }),
					//Index 5 - content - yellow
					new Font(
						new FontSize() { Val = 8 },
						new Color() { Rgb = new HexBinaryValue() { Value = ExportConst.COLOR_YELLOW } },
						new FontName() { Val = "Calibri" }),
					//Index 6 - content - red
					new Font(
						new FontSize() { Val = 8 },
						new Color() { Rgb = new HexBinaryValue() { Value = ExportConst.COLOR_RED } },
						new FontName() { Val = "Calibri" }),
					//Index 7- Grid body first, end cell
					new Font(
						new Bold(),
						new FontSize() { Val = 8 },
						new Color() { Rgb = new HexBinaryValue() { Value = "00000" } },
						new FontName() { Val = "Calibri" }),
					//Index 8 - Risk Factor Number font cell
					new Font(
						new Bold(),
						new FontSize() { Val = 18 },
						new Color() { Rgb = new HexBinaryValue() { Value = ExportConst.COLOR_WHITE } },
						new FontName() { Val = "Calibri" }),
					//Index 9 - Grid group first cell
					new Font(
						new Bold(),
						new FontSize() { Val = 11 },
						new Color() { Rgb = new HexBinaryValue() { Value = "000000" } },
						new FontName() { Val = "Calibri" }),
					//Index 10 - Grid group cell
					new Font(
						new FontSize() { Val = 11 },
						new Color() { Rgb = new HexBinaryValue() { Value = "000000" } },
						new FontName() { Val = "Calibri" })
				),

				//Fill - background cell
				new Fills(
					// Index 0 - The default fill.
					new Fill(new PatternFill() { PatternType = PatternValues.None }),
					//Index 1 - The default fill of gray 125 (required)
					new Fill(new PatternFill() { PatternType = PatternValues.Gray125 }),
					//Index 2 - Fill Green
					new Fill(new PatternFill(new ForegroundColor() { Rgb = new HexBinaryValue() { Value = ExportConst.COLOR_GREEN } }) { PatternType = PatternValues.Solid }),
					//Index 3 - Fill Yellow
					new Fill(new PatternFill(new ForegroundColor() { Rgb = new HexBinaryValue() { Value = ExportConst.COLOR_YELLOW } }) { PatternType = PatternValues.Solid }),
					//Index 4 - Fill Red
					new Fill(new PatternFill(new ForegroundColor() { Rgb = new HexBinaryValue() { Value = ExportConst.COLOR_RED } }) { PatternType = PatternValues.Solid }),
					//Index 5 - Fill Less Blue
					new Fill(new PatternFill(new ForegroundColor() { Rgb = new HexBinaryValue() { Value = ExportConst.COLOR_LESS_BLUE } }) { PatternType = PatternValues.Solid }),
					//Index 6 - Fill Blue
					new Fill(new PatternFill(new ForegroundColor() { Rgb = new HexBinaryValue() { Value = ExportConst.COLOR_BLUE } }) { PatternType = PatternValues.Solid }),
					//Index 7 - Fill Black
					new Fill(new PatternFill(new ForegroundColor() { Rgb = new HexBinaryValue() { Value = ExportConst.COLOR_BLACK } }) { PatternType = PatternValues.Solid }),
					//Index 8 - Fill Orange
					new Fill(new PatternFill(new ForegroundColor() { Rgb = new HexBinaryValue() { Value = ExportConst.COLOR_ORANGE } }) { PatternType = PatternValues.Solid }),
					//Index 9 - Fill Gray
					new Fill(new PatternFill(new ForegroundColor() { Rgb = new HexBinaryValue() { Value = ExportConst.COLOR_GRAY } }) { PatternType = PatternValues.Solid })
				),

				//Border
				new Borders(
					// Index 0 - The default border.
					new Border(
						new LeftBorder(),
						new RightBorder(),
						new TopBorder(),
						new BottomBorder(),
						new DiagonalBorder()),

					// Index 1 - OutSide border.
					new Border(
						new LeftBorder(
							new Color() { Indexed = (UInt32Value)64U }
						)
						{ Style = BorderStyleValues.Thin },
						new RightBorder(
							new Color() { Indexed = (UInt32Value)64U }
						)
						{ Style = BorderStyleValues.Thin },
						new TopBorder(
							new Color() { Indexed = (UInt32Value)64U }
						)
						{ Style = BorderStyleValues.Thin },
						new BottomBorder(
							new Color() { Indexed = (UInt32Value)64U }
						)
						{ Style = BorderStyleValues.Thin },
						new DiagonalBorder()),

					// Index 2 - TOP -BOT BORDER
					new Border(
						new TopBorder(
							new Color() { Indexed = (UInt32Value)64U }
						)
						{ Style = BorderStyleValues.Thin },
						new BottomBorder(
							new Color() { Indexed = (UInt32Value)64U }
						)
						{ Style = BorderStyleValues.Thin },
						new DiagonalBorder())
				),

				//Cell Format
				new CellFormats(
					// Index 0 - default
					new CellFormat() { FontId = 0, FillId = 0, BorderId = 0 },
					// Index 1 - Report Title
					new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }) { FontId = 1, FillId = 0, BorderId = 0, ApplyFont = true },
					// Index 2 - Chart title
					new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Left, Vertical = VerticalAlignmentValues.Center }) { FontId = 2, FillId = 0, BorderId = 0, ApplyFont = true },
					// Index 3 - Grid header cell
					new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center, WrapText = true }) { FontId = 3, FillId = 5, BorderId = 0, ApplyFont = true },
					// Index 4 - Grid header first cell
					new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }) { FontId = 3, FillId = 6, BorderId = 0, ApplyFont = true },
					// Index 5 - Grid header end cell
					new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }) { FontId = 3, FillId = 7, BorderId = 0, ApplyFont = true },
					// Index 6 - Text greater than Goal
					new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }) { FontId = 4, FillId = 0, BorderId = 0, ApplyFont = true },
					// Index 7 - Text in Goal
					new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }) { FontId = 5, FillId = 0, BorderId = 0, ApplyFont = true },
					// Index 8 - Text less than Goal
					new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }) { FontId = 6, FillId = 0, BorderId = 0, ApplyFont = true },
					// Index 9 - Background greater than Goal
					new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }) { FontId = 0, FillId = 2, BorderId = 0, ApplyFont = true },
					// Index 10 - Background in Goal
					new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }) { FontId = 0, FillId = 3, BorderId = 0, ApplyFont = true },
					// Index 11 - Background less than Goal
					new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }) { FontId = 0, FillId = 4, BorderId = 0, ApplyFont = true },
					// Index 12 - Grid body first, end cell
					new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }) { FontId = 7, FillId = 0, BorderId = 0, ApplyFont = true },
					// Index 13 - Grid forecast cell
					new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }) { FontId = 0, FillId = 0, BorderId = 0, ApplyFont = true },
					// Index 14 - Grid Sub header cell
					new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }) { FontId = 3, FillId = 5, BorderId = 0, ApplyFont = true },
					// Index 15 - Grid Normal cell
					new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }) { FontId = 0, FillId = 0, BorderId = 0 },
					// Index 16 - Grid Region Cell
					new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Left, Vertical = VerticalAlignmentValues.Center, WrapText = true }) { FontId = 7, FillId = 0, BorderId = 0, ApplyFont = true },
					// Index 17 - Grid Site Cell
					new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Right, Vertical = VerticalAlignmentValues.Center, WrapText = true }) { FontId = 0, FillId = 0, BorderId = 0, ApplyFont = true },
					// Index 18 - Risk Factor Number Cell
					new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center, WrapText = true }) { FontId = 8, FillId = 6, BorderId = 0, ApplyFont = true },
					// Index 19 - Grid header list group
					new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center, WrapText = true }) { FontId = 3, FillId = 8, BorderId = 0, ApplyFont = true },
					// Index 20 - Grid Group First Cell
					new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Left, Vertical = VerticalAlignmentValues.Center, WrapText = true }) { FontId = 9, FillId = 9, BorderId = 0, ApplyFont = true },
					// Index 21 - Grid Group Cell
					new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center, WrapText = true }) { FontId = 10, FillId = 9, BorderId = 0, ApplyFont = true },
					// Index 22 - Grid Group Header Cell
					new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Left, Vertical = VerticalAlignmentValues.Center, WrapText = true }) { FontId = 3, FillId = 5, BorderId = 0, ApplyFont = true }
				)
			); // return
		}
	}
}

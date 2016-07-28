using MigraDoc.DocumentObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.DataServices.ExportDataService.PDF
{
	public class PDFStyles
	{

		#region Properties
		public enum StyleNameCSS
		{
			Normal,
			TableDefaultTextCSS,
			RptNameCSS,
			CreatedByCSS,
			CreatedDateCSS,
			PageFooterCSS,
			TextGreaterGoalCSS,
			TextInGoalCSS,
			TextLessGoalCSS,
			BGGreaterGoalCSS,
			BGInGoalCSS,
			BGLessGoalCSS,
			GridHeaderFirstCellCSS,
			GridHeaderEndCellCSS,
			GridHeaderCellCSS,
			ChartTitleCSS
		}

		#endregion

		public static void DefineStyles(Document document)
		{
			// Get the predefined style Normal.
			Style style = document.Styles[StyleNameCSS.Normal.ToString()];
			// Because all styles are derived from Normal, the next line changes the 
			// font of the whole document. Or, more exactly, it changes the font of
			// all styles and paragraphs that do not redefine the font.
			style.Font.Name = "Times New Roman";

			// Heading1 to Heading9 are predefined styles with an outline level. An outline level
			// other than OutlineLevel.BodyText automatically creates the outline (or bookmarks) 
			// in PDF.

			style = document.Styles["Heading1"];
			style.Font.Name = "Tahoma";
			style.Font.Size = 14;
			style.Font.Bold = true;
			style.Font.Color = Colors.DarkBlue;
			style.ParagraphFormat.PageBreakBefore = true;
			style.ParagraphFormat.SpaceAfter = 6;

			style = document.Styles["Heading2"];
			style.Font.Size = 12;
			style.Font.Bold = true;
			style.ParagraphFormat.PageBreakBefore = false;
			style.ParagraphFormat.SpaceBefore = 6;
			style.ParagraphFormat.SpaceAfter = 6;

			style = document.Styles["Heading3"];
			style.Font.Size = 10;
			style.Font.Bold = true;
			style.Font.Italic = true;
			style.ParagraphFormat.SpaceBefore = 6;
			style.ParagraphFormat.SpaceAfter = 3;

			style = document.Styles[StyleNames.Header];
			style.ParagraphFormat.AddTabStop("16cm", TabAlignment.Right);

			style = document.Styles[StyleNames.Footer];
			style.ParagraphFormat.AddTabStop("8cm", TabAlignment.Center);

			// Create a new style called TextBox based on style Normal
			style = document.Styles.AddStyle("TextBox", StyleNameCSS.Normal.ToString());
			style.ParagraphFormat.Alignment = ParagraphAlignment.Justify;
			style.ParagraphFormat.Borders.Width = 2.5;
			style.ParagraphFormat.Borders.Distance = "3pt";
			//TODO: Colors
			style.ParagraphFormat.Shading.Color = Colors.SkyBlue;

			// Create a new style called TOC based on style Normal
			style = document.Styles.AddStyle("TOC", StyleNameCSS.Normal.ToString());
			style.ParagraphFormat.AddTabStop("16cm", TabAlignment.Right, TabLeader.Dots);
			style.ParagraphFormat.Font.Color = Colors.Blue;

			/***ThangPham, Customize Style for PDF document, Feb 26 2016***/

			/* style default table text */
			style = document.Styles.AddStyle(StyleNameCSS.TableDefaultTextCSS.ToString(), StyleNameCSS.Normal.ToString());
			style.Font.Size = 7;
			style.ParagraphFormat.Alignment = ParagraphAlignment.Center;

			/* style report title */
			style = document.Styles.AddStyle(StyleNameCSS.RptNameCSS.ToString(), StyleNameCSS.Normal.ToString());
			style.Font.Size = 20;
			style.Font.Bold = true;
			style.ParagraphFormat.Alignment = ParagraphAlignment.Center;

			/* style chart title */
			style = document.Styles.AddStyle(StyleNameCSS.ChartTitleCSS.ToString(), StyleNameCSS.Normal.ToString());
			style.Font.Size = 10;
			style.Font.Bold = true;
			style.ParagraphFormat.Alignment = ParagraphAlignment.Left;

			/* style created by */
			style = document.Styles.AddStyle(StyleNameCSS.CreatedByCSS.ToString(), StyleNameCSS.Normal.ToString());
			style.Font.Size = 10;
			style.ParagraphFormat.Alignment = ParagraphAlignment.Right;

			/* style created date */
			style = document.Styles.AddStyle(StyleNameCSS.CreatedDateCSS.ToString(), StyleNameCSS.Normal.ToString());
			style.Font.Size = 10;
			style.ParagraphFormat.Alignment = ParagraphAlignment.Right;

			/* Style footer page */
			style = document.Styles.AddStyle(StyleNameCSS.PageFooterCSS.ToString(), StyleNameCSS.Normal.ToString());
			style.Font.Size = 7;
			style.ParagraphFormat.Alignment = ParagraphAlignment.Center;

			/* Text Greater than Goal */
			style = document.Styles.AddStyle(StyleNameCSS.TextGreaterGoalCSS.ToString(), StyleNameCSS.Normal.ToString());
			style.Font.Size = 7;
			style.Font.Color = Colors.Green;
			style.ParagraphFormat.Alignment = ParagraphAlignment.Center;

			/* Text In Goal */
			style = document.Styles.AddStyle(StyleNameCSS.TextInGoalCSS.ToString(), StyleNameCSS.Normal.ToString());
			style.Font.Size = 7;
			style.Font.Color = Colors.Gold;
			style.ParagraphFormat.Alignment = ParagraphAlignment.Center;

			/* Text Less than Goal */
			style = document.Styles.AddStyle(StyleNameCSS.TextLessGoalCSS.ToString(), StyleNameCSS.Normal.ToString());
			style.Font.Size = 7;
			style.Font.Color = Colors.Red;
			style.ParagraphFormat.Alignment = ParagraphAlignment.Center;

			/* Text Grid header cell */
			style = document.Styles.AddStyle(StyleNameCSS.GridHeaderCellCSS.ToString(), StyleNameCSS.Normal.ToString());
			style.Font.Size = 7;
			style.Font.Color = Colors.White;
			style.ParagraphFormat.Alignment = ParagraphAlignment.Center;

			/* Text Grid header first cell */
			style = document.Styles.AddStyle(StyleNameCSS.GridHeaderFirstCellCSS.ToString(), StyleNameCSS.Normal.ToString());
			style.Font.Size = 7;
			style.Font.Color = Colors.White;
			style.ParagraphFormat.Alignment = ParagraphAlignment.Center;

			/* Text Grid header end cell */
			style = document.Styles.AddStyle(StyleNameCSS.GridHeaderEndCellCSS.ToString(), StyleNameCSS.Normal.ToString());
			style.Font.Size = 7;
			style.Font.Color = Colors.White;
			style.ParagraphFormat.Alignment = ParagraphAlignment.Center;

			/* Background greater than goal cell */
			style = document.Styles.AddStyle(StyleNameCSS.BGGreaterGoalCSS.ToString(), StyleNameCSS.Normal.ToString());
			style.Font.Size = 7;
			style.Font.Color = Colors.White;
			style.ParagraphFormat.Alignment = ParagraphAlignment.Center;

			/* Background in than goal cell */
			style = document.Styles.AddStyle(StyleNameCSS.BGInGoalCSS.ToString(), StyleNameCSS.Normal.ToString());
			style.Font.Size = 7;
			style.Font.Color = Colors.White;
			style.ParagraphFormat.Alignment = ParagraphAlignment.Center;

			/* Background less than goal cell */
			style = document.Styles.AddStyle(StyleNameCSS.BGLessGoalCSS.ToString(), StyleNameCSS.Normal.ToString());
			style.Font.Size = 7;
			style.Font.Color = Colors.White;
			style.ParagraphFormat.Alignment = ParagraphAlignment.Center;


		}

		
	}
}

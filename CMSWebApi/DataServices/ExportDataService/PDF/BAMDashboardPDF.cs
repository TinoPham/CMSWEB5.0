using CMSWebApi.DataModels.ExportModel;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.Rendering;
using PdfSharp.Drawing;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CMSWebApi.DataServices.ExportDataService.PDF
{
	public class BAMDashboardPDF
	{
		#region Properties
		private static Document document { get; set; }
		private static Section section { get; set; } //section is page
		private static Paragraph paragraph { get; set; }

		private static readonly string LOGO_EXPORT_NAME = "logo_export.jpg";

		#endregion

		public static void ExportToPDF(string exportFile, byte[] logoImage, ExportModel model)
		{
			//MigraDoc 1.50 will support insert image from byte/memory stream.

			document = PDFDocument.CreateBAMDashboardDocument();

			InsertHeader(exportFile, logoImage, model);

			InsertFooter(model.ReportInfo.Footer);

			InsertContent(model);

			PdfDocumentRenderer renderer = new PdfDocumentRenderer(true, PdfSharp.Pdf.PdfFontEmbedding.Always);
			renderer.Document = document;
			renderer.RenderDocument();
			renderer.PdfDocument.Save(exportFile);
		}

		private static void InsertHeader(string exportFile, byte[] logoImage, ExportModel model)
		{
			section = document.AddSection();

			//PDFDocument.AddImage(document, logoImage);

			string filename = Path.GetFileName(exportFile);
			string folderPath = exportFile.Substring(0, exportFile.Length - filename.Length);
			string logoPath = Path.Combine(folderPath, LOGO_EXPORT_NAME);
			WriteFile(logoPath, logoImage).ConfigureAwait(true);

			//insert header page
			Image imgHeader = section.Headers.Primary.AddImage(logoPath);
			imgHeader.Width = 100;
			imgHeader.Height = 50;
			imgHeader.LockAspectRatio = true;
			imgHeader.RelativeVertical = RelativeVertical.Line;
			imgHeader.RelativeHorizontal = RelativeHorizontal.Margin;
			imgHeader.Top = ShapePosition.Top;
			imgHeader.Left = ShapePosition.Left;
			imgHeader.WrapFormat.Style = WrapStyle.Through;

			paragraph = section.Headers.Primary.AddParagraph();
			paragraph.AddText(model.ReportInfo.ReportName);
			paragraph.Style = PDFStyles.StyleNameCSS.RptNameCSS.ToString();

			paragraph = section.Headers.Primary.AddParagraph();
			paragraph.AddText(model.ReportInfo.CreatedBy);
			paragraph.Style = PDFStyles.StyleNameCSS.CreatedByCSS.ToString();

			paragraph = section.Headers.Primary.AddParagraph();
			paragraph.AddText(model.ReportInfo.CreateDate);
			paragraph.Style = PDFStyles.StyleNameCSS.CreatedDateCSS.ToString();

			paragraph = document.LastSection.AddParagraph();
			paragraph.Format.SpaceAfter = 70;
		}

		private static void InsertContent(ExportModel model)
		{
			PDFTables.InsertDataGrid(document, model.GridModels[0]);
			PDFTables.InsertDataGrid(document, model.GridModels[1]);

			PDFCharts.InsertColumnChart(document, model.ChartModels[0]);
			//PDFCharts.InsertColumnChart(document, model.ChartModels[1]);
		}

		private static void InsertFooter(string footer)
		{
			//insert footer page
			paragraph = section.Footers.Primary.AddParagraph();
			paragraph.AddText(footer);
			paragraph.Style = PDFStyles.StyleNameCSS.PageFooterCSS.ToString();
		}

		private static async Task WriteFile(string fpath, byte[] mem)
		{
			if (mem == null || mem.Length == 0 || string.IsNullOrEmpty(fpath))
			{
				throw new Exception("Cannot save to: " + fpath);
			}

			if (File.Exists(fpath))
			{
				throw new Exception("Not found path: " + fpath);
			}

			using (var fs = new FileStream(fpath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
			{
				await fs.WriteAsync(mem, 0, mem.Length);
				fs.Close();
			}
		}
	}
}

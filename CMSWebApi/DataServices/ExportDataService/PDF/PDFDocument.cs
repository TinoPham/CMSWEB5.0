using MigraDoc.DocumentObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.DataServices.ExportDataService.PDF
{
	public class PDFDocument
	{
		public static Document CreateBAMDashboardDocument()
		{
			// Create a new MigraDoc document
			Document document = new Document();

			document.DefaultPageSetup.PageFormat = PageFormat.A4;
			document.DefaultPageSetup.Orientation = Orientation.Portrait;

			document.DefaultPageSetup.TopMargin = 25;
			document.DefaultPageSetup.BottomMargin = 25;
			document.DefaultPageSetup.LeftMargin = 25;
			document.DefaultPageSetup.RightMargin = 25;

			document.Info.Title = "CMS Web Reports";
			document.Info.Subject = "Export";
			document.Info.Author = "Thang Pham";

			PDFStyles.DefineStyles(document);

			return document;
		}

		public static void AddImage(Document document, byte[] imgBytes)
		{
			Section section = document.AddSection();
			string imageData = Convert.ToBase64String(imgBytes);
			string strBase64 = Commons.Utils.String2Base64(imageData);
			section.AddImage(imageData);
		}
	}
}

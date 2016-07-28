using CMSWebApi.DataModels.ExportModel;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes.Charts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.DataServices.ExportDataService.PDF
{
	public class PDFCharts
	{
		public static void InsertColumnChart(Document document, ChartData model)
		{
			if(model.ChartDataItems.Count == 0) { return; }
			Chart chart = new Chart();
			chart.Left = 0;
			chart.Width = 200;
			chart.Height = 200;

			RenderSerials(chart, model.ChartDataItems);

			chart.XAxis.MajorTickMark = TickMarkType.Outside;
			chart.YAxis.MajorTickMark = TickMarkType.Outside;
			chart.YAxis.HasMajorGridlines = true;

			document.LastSection.Add(chart);

			Paragraph paragraph = document.LastSection.AddParagraph();
			paragraph.Format.SpaceAfter = 10;
		}

		public static void InsertLineChart(Document document, ChartData model)
		{
			Chart chart = new Chart();
			chart.Left = 0;

			chart.Width = Unit.FromCentimeter(16);
			chart.Height = Unit.FromCentimeter(12);

			RenderSerials(chart, model.ChartDataItems);

			//series = chart.SeriesCollection.AddSeries();
			//series.ChartType = ChartType.Line;
			//series.Add(new double[] { 41, 7, 5, 45, 13, 10, 21, 13, 18, 9 });

			XSeries xseries = chart.XValues.AddXSeries();
			xseries.Add("A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N");

			chart.XAxis.MajorTickMark = TickMarkType.Outside;
			chart.XAxis.Title.Caption = "X-Axis";

			chart.YAxis.MajorTickMark = TickMarkType.Outside;
			chart.YAxis.HasMajorGridlines = true;

			chart.PlotArea.LineFormat.Color = Colors.DarkGray;
			chart.PlotArea.LineFormat.Width = 1;

			document.LastSection.Add(chart);
		}

		private static void RenderSerials(Chart chart, List<ChartDataItem> model)
		{
			Series series = chart.SeriesCollection.AddSeries();
			series.ChartType = ChartType.Column2D;
			series.HasDataLabel = true;
			series.FillFormat.Color = Colors.Orange;

			double[] values = model.Select(s => double.Parse(s.Value)).ToArray();
			series.Add(values);

			XSeries xseries = chart.XValues.AddXSeries();
			string[] names = model.Select(s => s.Name).ToArray();
			xseries.Add(names);
			chart.XAxis.TickLabels.Style = PDFStyles.StyleNameCSS.TableDefaultTextCSS.ToString();
		}
	}
}

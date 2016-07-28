using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.DataModels;
using CMSWebApi.DataModels.Alert;
using CMSWebApi.Email;
using PACDMModel.Model;
using System.IO;
using System.Net.Mime;
using System.Web;
namespace CMSWebApi.BusinessServices.InternalBusinessService
{
	internal class DVRAlertFormat : Commons.SingletonClassBase<DVRAlertFormat>
	{
		readonly int? COL_SITE_WIDTH = 250;
		readonly int? COL_SERVER_ID_WIDTH = 100;
		readonly int? COL_ALERT_TYPE_WIDTH = 200;
		readonly int? COL_DETAIL_WIDTH = null;
		readonly int? COL_DATE_TIME_WIDTH = 95;
		readonly int? COL_IMG_LINK_WIDTH = 87;
		readonly string Report_Date_Format = "MM/dd/yyyy";
		readonly string Report_Alert_Date_Format = "MM/dd/yyyy HH:mm:ss tt";
		readonly string backgroundcolor = "#EFEFEF";
		readonly string td_style = "background-color:#EFEFEF;padding:2.25pt;border-style:none solid none none;border-right-width:1pt;border-right-color:#CCCCCC;";
		readonly string img_Source = "data:image/png;base64, iVBORw0KGgoAAAANSUhEUgAAAIAAAACACAMAAAD04JH5AAAB6VBMVEUAAABER0pIqpdJqpdKq5hLq5hMq5hNrJpPrZlQrZpSrZtTrptTr51UV1hVsJ5Wr51WsJ9Xr51Xr55Yr55bsaBbsqBdsaBesqBfsqFftKNhs6JkZ2hktKNntqRotaVqtqVrt6Ztt6dxvKxyuqlyvK11uqp4eXp4u6x4vq95vK17fHx+va+Avq+Ev7GEwLGJiYmJwbOMjIyMw7SNw7WQxLaSxbeWxriYx7qbyLubybucnZueyryhyr2kzL+lzL+nzsGpzcGsrKqszsKuz8Ovr62wz8SysrGz0ca1tbK20sa6ube71Mi71Mm8u7m8vLq/v7zA1cvA1svBwb3Cwr/C1svDw8DD29HE3tXIx8XKyMXK2c7K2c/L4NjM2tDN2tDN2tHP49vQz8zQ29HQ4djR4trS3NPT0c7V3dTW1dHW3dTX3tTX5t7Y1tPa3tba39bb39fc4Nfd3Njd4Nfd6eLe3tnf3trf4Nnf6uLf6uPg3trg4djh4djh4dni4tni4trj4t7k4trl4trl49vl7Obm49vm5eDn5Nzn5d3n5eHo5N3o5d3o5d7o7efq5+Hq6OPr6ODr6OHs6eLs6ePt6uTt6+Tt7Ofu7Obv6+Xv7ebv7ejw7ufw7ujx7+nx8Orz8ezz8e308ez08uz08u25qfU8AAAAAXRSTlMAQObYZgAAAiFJREFUeNrt2FdTU1EUxXFcGLGLvYu9odi7iEjsDXtDsYKxISo2UBA3imBDAY1Y9yf1ITnM3DxcH8y5e4as3xfIfzLn3rPm5uURERERERH9k/4HBjCAAQMo4E9vt3P1rUVAnzg3xk+9qar1h8K9z3JA0v3+8znAXFUtHxSuJusBT9fUiYisBkbeV9VjS8PVZzsgMQsLWkQqgPxTFmcgeWEYsF7OFgCbTJ6CpOwGsGMysOiVUYCsAwBMuduhqqpHFwad8B/QMg/A0HOSCsh8CuL+A+T2BGCnpAMeHA5qiCBAzi/bLiJvbF7FXR+cr7wNGZCbAb8+dzlXTAbJt8xBcr08XJvPQTLfeJAUPlbVM6vCPfE3SGJVJtdxdf8g2Wa0B/amB0nJa6tBsgEAUNSUuo73TQ866D/gxRIA4xKGg+TeNAw57gZJW01QexQvotq1B0Sk0+QM/Oz+5PTxNmRAbgb8/vjOOfnI6FWcdnn46CpVvbQ5XLOvgIczgMUmg+ROcUJEWouBiS8t/oHqSZj9TGQrELtocgZqxwArpXIwUGa0B47EgI1jgeVmg6QUADCzMXUd7xoVtMd/QGsJgBHXDAdJUxHy97tB0tMc1BPFp9q6LZXiAiK/C770f6z+ztuQATk/SHrVdpCk3wOnV4Rr8B0Q/SDJCLgVD9fuO8D8DDDAYJCEPwXxqANMBklKp/Ug+cHbkAEMYEDOBRARERER0QD3FxITRc121NxPAAAAAElFTkSuQmCC";
		readonly string CSS= @"<style type=""text/css"">
					h1{font-size:20pt;}td,p,body{font-size:10pt;padding:3px;} tr{ padding:0; margin:0}
					.head{background-color:white;color:black;border-right:0px solid black;}
					.odd_row{background-color:#FAFAFA;border-right:1px solid black;}
					.even_row{background-color:#EFEFEF;border-right:1px solid black;}
					body{background-color:white; border:0px 0px 0px 0px}
					.separator{background-color: #426d8f;}
					.separator td{ height:7px;}
					.Title{ padding-left:15; color: #426d8f;}
					.tbinfo{ border:0px; width:100%; padding:3px;}
					.info_name{ color:Black; padding-right:20px;}
					.info_value{ color: #426d8f; font-weight:bold;}
					.tbSummary{ padding-bottom:50; padding-top:50; width:100%; background-color:#f0f0f0;}
					.tdSummaryValue{width: 25%; font-size: 35; font-weight: bold;color: #426d8f; padding:0; margin:0;}
					.tdSummaryCaption{ width:25%;padding:0; margin:0;}
					.report{border:0px solid black;padding:0;width:100%;}
					.tbreport{border:1px solid #CCCCCC;padding:0; margin:0; background-color:#f0f0f0;width:100%;}
					.reportcolumn{border-right: 1 solid #CCCCCC; border-bottom: 1 solid #CCCCCC; border-left: 1 solid #CCCCCC;background-color: #426d8f; color: white; font-weight: bold; height: 40;}
					.reportcell{border-right: 1 solid #CCCCCC; border-bottom: 1 solid #CCCCCC; }
					.tbreport td{border-right: 1px solid #CCCCCC;border-bottom: 1px solid #CCCCCC;}
					.Footer{background-color: #88a9c2; height: 40; color: White;}
					</style>";
		
		public StringBuilder EmailDataFormat(IEnumerable<EmailAlertModel> SiteAlerts, DateTime date, string title, string company, string createdby, string frequency, int totalDVR, int totalConnected,  int StogareLessThan, int StogareExcess, int default_StogareLessThan, int default_StogareExcess)
		{
			StringBuilder html = new StringBuilder();
			//html.AppendFormat("<html><head>{0}</head>", CSS);
			html.Append("<html><head></head>");
			html.Append("<body style=\"border:none 0 white\"><div style=\"border:none 0 white;padding:0px;width:100%;\"><table  style=\"border:none 0 white;padding:0;width:100%;\" cellpadding=\"0\">");
			//header line
			html.Append("<tr style=\"background-color: #426d8f;\"><td style=\"height:7px;\"></td></tr>");
			#region Title/general info row

			html.Append("<tr><td style=\"padding-bottom: 30px;padding-top: 30px;\">");
			TableTitleGeneral(html, title, date.ToString(Report_Date_Format), company, createdby, frequency);
			html.Append("</td></tr>");
			#endregion
			#region Summary table
			html.Append("<tr><td style=\"padding-top: 30px; padding-bottom: 30px; background-color: #f0f0f0;\">");
			TableSummaryInfo(html, totalDVR, totalConnected, StogareLessThan, StogareExcess, default_StogareLessThan, default_StogareExcess);
			html.Append("</td></tr>");
			#endregion
			#region Alerts
			html.Append("<tr><td style=\"padding-top:30px\">");
			TableAlerts(html,SiteAlerts);
			html.Append("</td></tr>");
			#endregion
			#region Powerby
			html.AppendFormat("<tr><td style=\"padding-top:30px\"><table style=\"background: #88a9c2; width: 100%; padding: 20px; color: white; font-size: 12px; text-align: center\"><tr><td>Power by {0}.&nbsp;</td></tr></table></td></tr>", HtmlEncode("I3 International"));
			#endregion
			html.Append("</table></div></body></html>");
			return html;
		}

		#region header
		private void TableTitleGeneral(StringBuilder html, string title, string reportdate, string company, string createdby, string frequency)
		{
			html.Append("<table style=\"border:0px; width:100%; padding:3px;\" cellpadding=\"0\">");
			html.AppendFormat("<tr><td align=\"left\" style=\"padding-left:15px; color: #426d8f;\" ><H1>{0}&nbsp;</H1></td>",HtmlEncode(title));
			html.Append("<td style=\"width:40%\"><table align=\"right\" style=\"text-transform: uppercase;\">");
			html.Append("<tr><td style=\"color:Black; padding-right:20px;\">Report Date :</td>");
			html.AppendFormat("<td style=\"color: #426d8f; font-weight:bold;\">{0}&nbsp;</td></tr>", HtmlEncode(reportdate));
			html.AppendFormat("<tr><td style=\"color:Black; padding-right:20px;\">Company  :</td><td style=\"color: #426d8f; font-weight:bold;\">{0}&nbsp;</td></tr>", HtmlEncode(company));
			html.AppendFormat("<tr><td style=\"color:Black; padding-right:20px;\">Created By :</td><td style=\"color: #426d8f; font-weight:bold;\">{0}&nbsp;</td></tr>", HtmlEncode(createdby));
			html.AppendFormat("<tr><td style=\"color:Black; padding-right:20px;\">Frequency :</td><td style=\"color: #426d8f; font-weight:bold;\">{0}&nbsp;</td></tr>", HtmlEncode(frequency));
			html.Append("</table>");
			html.Append("</td></tr></table>");
		}
		/*
		private void TableHeader( StringBuilder html, string title, string reportdate, string company, string createdby, string frequency )
		{
			TableTitle(html, title);
			TableGeneralInfo(html, reportdate, company, createdby, frequency);
			html.Append("</tr></table>");
		}
		private void TableTitle( StringBuilder html, string title)
		{
			html.Append("<table style=\"width: 100%\"><tr style=\"width: 100%;\"><td colspan=\"3\" style=\" height:7; width: 100%; background : #426d8f;\"></td></tr>");
			html.Append("<tr style=\"background : white;\">");
			html.Append("<td style=\"width: 10%; border-rigth: 1 solid #ccc\">");
			html.Append("<img width=\"80%\" src=\"" + img_Source + "\" alt=\"\" /> </td>");

			html.Append("<td style=\"width: 50%; color : #426d8f;padding : 40px 20px\">");
			html.Append(title + "</td>");
			
			
		}
		private void TableGeneralInfo( StringBuilder html, string reportdate, string company, string createdby, string frequency )
		{
			html.Append("<td style=\"width: 40%;\"><table align=\"right\" style=\"font-size: 12px;text-transform: uppercase;\"><tr><td style=\"padding-right: 20px\">Report Date :</td><td style=\"color : #426d8f; font-weight : bold ; text-transform: uppercase;\">");
			html.Append( reportdate);
			html.Append("</td></tr><tr><td>Company  :</td><td style=\"color : #426d8f; font-weight : bold ; text-transform: uppercase;\">");
			html.Append(company);
			html.Append("</td></tr><tr><td>Created By :</td><td style=\"color : #426d8f; font-weight : bold ; text-transform: uppercase;\">");
			html.Append(createdby);
			html.Append("</td></tr><tr><td>Frequency :</td><td style=\"color : #426d8f; font-weight : bold; text-transform: uppercase;\">");
			html.Append( frequency);
			html.Append("</td></tr></table></td>");
		}
		*/
		#endregion
		#region Summary info
		private void TableSummaryInfo( StringBuilder html, int totalDVR, int totalConnected,  int StogareLessThan, int StogareExcess, int default_StogareLessThan, int default_StogareExcess)
		{
			html.Append("<table style=\"width:100%; background-color:#f0f0f0;\"><tr align=\"center\">");
			html.AppendFormat("<td style=\"width: 25%; font-size: 35pt; font-weight: bold;color: #426d8f; padding:0px; margin:0px;\">{0}&nbsp;</td>", totalDVR);
			html.AppendFormat("<td style=\"width: 25%; font-size: 35pt; font-weight: bold;color: #426d8f; padding:0px; margin:0px;\">{0}&nbsp;</td>", totalConnected);
			html.AppendFormat("<td style=\"width: 25%; font-size: 35pt; font-weight: bold;color: #426d8f; padding:0px; margin:0px;\">{0}&nbsp;</td>", StogareLessThan);
			html.AppendFormat("<td style=\"width: 25%; font-size: 35pt; font-weight: bold;color: #426d8f; padding:0px; margin:0px;\">{0}&nbsp;</td></tr>", StogareExcess);
			html.Append("<tr align=\"center\">");
			html.AppendFormat("<td style=\"width:25%;padding:0px; margin:0px;\">{0}&nbsp;</td>", HtmlEncode("DVR Total"));
			html.AppendFormat("<td style=\"width:25%;padding:0px; margin:0px;\">{0}&nbsp;</td>", HtmlEncode("DVR Connection with CMS"));
			html.AppendFormat("<td style=\"width:25%;padding:0px; margin:0px;\">Storage less than {0} days&nbsp;</td>", default_StogareLessThan);
			html.AppendFormat("<td style=\"width:25%;padding:0px; margin:0px;\">Storage excess {0} days&nbsp;</td>", default_StogareExcess);
			html.Append("</tr></table>");
		}
		/*
		private void TableSummary( StringBuilder html, int totalDVR, int totalConnected,  int StogareLessThan, int StogareExcess, int default_StogareLessThan, int default_StogareExcess)
		{
			html.Append("<div style=\"width : 1000px ; background : #f0f0f0; height : 150px\"><table style=\"width: 100%;\"><tr style=\"width: 100%\">");
			SummaryValue(html,totalDVR);
			SummaryValue(html, totalConnected);
			SummaryValue(html, StogareLessThan);
			SummaryValue(html, StogareExcess);
			html.Append("</tr><tr>");
			SummaryName(html, "DVR Total");
			SummaryName(html, "DVR Connection with CMS");
			SummaryName(html, string.Format("Storage less than {0} days", default_StogareLessThan));
			SummaryName(html, string.Format("Storage excess {0} days", default_StogareExcess));
			html.Append("</tr></table></div>");
		}
		private void SummaryName(StringBuilder html, string name)
		{
			html.AppendFormat("<td align=\"center\" style=\"width : 25%\"><span>{0}</span></td>", name);
		}
		private void SummaryValue(StringBuilder html, int num)
		{
			html.AppendFormat("<td align=\"center\" style=\"width : 25%;padding-top: 50px;\"><h1 style=\"line-height: 0px; color : #426d8f\" > {0} </h1></td>", num);
		}
		*/
		#endregion
		#region Alerts
		private void TableAlerts(StringBuilder html, IEnumerable<EmailAlertModel> SiteAlerts)
		{
			html.Append("<table style=\"border:1px solid #CCCCCC;padding:0px; margin:0px; background-color:#f0f0f0;width:100%;\" cellpadding=\"3\" cellspacing= \"0\">");
			AlertColumns(html);
			AlertRows(html,SiteAlerts);
			html.Append("</table>");
		}
		void AlertColumn(StringBuilder html, int? width, string name, string classname)
		{
			
			html.Append("<td style=\"border-right:solid 1px #CCCCCC; border-bottom:solid 1px #CCCCCC; border-left:solid 1px #CCCCCC;background-color: #426d8f; color: white; font-weight: bold; height: 40px;");
			if(classname != null)
				html.AppendFormat("{0};", classname);

			if( width.HasValue)
				html.AppendFormat(" width:{0}px;", width.Value);

			html.AppendFormat("\">{0}</td>", HtmlEncode(name));
		}
		private void AlertColumns(StringBuilder html)
		{
			html.Append("<tr align=\"center\">");
			AlertColumn(html, COL_SITE_WIDTH, HtmlEncode("Site Name"), null);
			AlertColumn(html, COL_SERVER_ID_WIDTH, HtmlEncode("ServerID"), null);
			AlertColumn(html, COL_ALERT_TYPE_WIDTH, HtmlEncode("Alert"), null);
			AlertColumn(html, COL_DETAIL_WIDTH, HtmlEncode("Detail"), null);
			AlertColumn(html, COL_DATE_TIME_WIDTH, HtmlEncode("Date/Time"), null);
			AlertColumn(html, COL_IMG_LINK_WIDTH, HtmlEncode("Hyperlink"), null);
			html.Append("</tr>");
		}
		private void AlertRows(StringBuilder html, IEnumerable<EmailAlertModel> SiteAlerts)
		{
			var sites = SiteAlerts.OrderBy(it => it.SiteName);
			int row_span = 0;
			string _cell = null;
			int rowindex = 0;
			foreach( var alt in sites)
			{
				html.Append("<tr>");
				
				row_span = alt.DVRAlerts.Sum(it => it.Alerts.Count());
				_cell = ALertCellValue(row_span > 1 ? (int?)row_span : null, COL_SITE_WIDTH, HtmlEncode(alt.SiteName), true);
				html.Append( _cell);

				foreach (var dvrs in alt.DVRAlerts)
				{
					DVRAlerts(html, dvrs, rowindex);
					rowindex++;

				}
				rowindex = 0;

			}
		}
		void DVRAlerts( StringBuilder html, EmailALertSiteDVR dvr, int rowindex )
		{
			int row_span = dvr.Alerts.Count();
			if( rowindex > 0)
				html.Append("<tr>");

			string cell = ALertCellValue( row_span > 1?(int?)row_span : null, COL_SERVER_ID_WIDTH, HtmlEncode(dvr.ServerID));
			html.Append(cell);
			var alerts = dvr.Alerts.OrderBy(it => it.TimeZone);
			bool first_row = true;
			foreach( var alt in alerts)
			{
				if (first_row == false)
				{
					html.Append("<tr>");
				}
				cell = ALertCellValue(null, COL_ALERT_TYPE_WIDTH, HtmlEncode(alt.KAlertName));
				html.Append(cell);
				cell = ALertCellValue(null, COL_DETAIL_WIDTH, HtmlEncode(alt.Description));
				html.Append(cell);
				cell = ALertCellValue(null, COL_DATE_TIME_WIDTH, HtmlEncode(alt.TimeZone.ToString(Report_Alert_Date_Format)));
				html.Append(cell);
				if (string.IsNullOrEmpty(alt.ImageUrl))
					cell = ALertCellValue(null, COL_IMG_LINK_WIDTH, "<i>No image</i>");
				else
					cell = ALertCellValue(null, COL_IMG_LINK_WIDTH, string.Format("<a href=\"{0}\" target=\"_blank\"><strong>Show image</strong></a>", alt.ImageUrl));

				html.Append(cell);
				html.Append("</tr>");

				first_row = false;
			}

		}
		string ALertCellValue( int? rowspan, int? width, string value, bool border_left = false)
		{
			string result = rowspan.HasValue ? string.Format("<td rowspan=\"{0}\" ", rowspan.Value) : "<td ";
			result += " style=\"border-right:solid 1px #CCCCCC; border-bottom:solid 1px #CCCCCC;";
			if(border_left)
				result += "border-left:solid 1px #CCCCCC;";
			result += width.HasValue ? string.Format(" width:{0}; padding-left:3px;\">", width.Value) : " padding-left:3px;\">";
			result += value + "&nbsp;</td>";
			return result;
		}
		/*
		private void _TableAlerts(StringBuilder html, IEnumerable<EmailAlertModel> SiteAlerts)
		{
			//add column
			html.Append( "<table width=\"1000px\" border=\"1\" cellspacing=\"0\" cellpadding=\"0\" style=\"width:100%;background-color:white;border-size:1pt; border-color: #CCCCCC;margin-top: 30px;margin-bottom: 30px;\"><tbody><tr>");
			ColumnName( html, "Site Name");
			ColumnName( html, "ServerID");
			ColumnName( html, "Alert", 10);
			ColumnName( html, "Detail");
			ColumnName( html, "Date/Time", 15);
			ColumnName( html, "Hyperlink");
			html.Append("</tr>");
			//Add row
			//html.Append("<tr>");
			var sites = SiteAlerts.OrderBy( it => it.SiteName);
			int row_span = 0;

			foreach( var alt in sites)
			{
				html.Append("<tr>");
				row_span = alt.DVRAlerts.Sum( it => it.Alerts.Count());
				//site name
				CellValue(html, alt.SiteName, row_span > 1 ? row_span : 0);

				foreach( var dvrs in alt.DVRAlerts)
				{
					AlertbyDVR( html, dvrs);
				}

				//html.Append("</tr>");
			}
		}
		private void AlertbyDVR( StringBuilder html, EmailALertSiteDVR site)
		{
			int row_span = site.Alerts.Count();
			
			CellValue(html, site.ServerID, row_span > 1? row_span : 0);
			var alerts = site.Alerts.OrderBy( it => it.TimeZone);
			bool first_row = true;
			foreach( var alt in alerts)
			{
				if (first_row == false)
				{
					html.Append("<tr>");
				}
				CellValue(html, alt.KAlertName,0);
				CellValue(html, alt.Description ,0);
				CellValue(html, alt.TimeZone.ToString(Report_Alert_Date_Format) ,0);
				if( string.IsNullOrEmpty(alt.ImageUrl ))
					CellValue(html, "<i>No image</i>", 0);
				else
					CellValue(html, string.Format( "<a href=\"{0}\" target=\"_blank\"><strong>Show image</strong></a>", alt.ImageUrl), 0);

				if (first_row  == false)
					html.Append("</tr>");
				else
					html.Append("</tr>"); 
				first_row = false;
			}
		}
		private void ColumnName( StringBuilder html, string name, int col_percentwidth = 0)
		{
			if( col_percentwidth == 0)
				html.Append("<td width=\"0\" style=\"width:0.3pt;background-color:#426d8f;padding:2.25pt;border-style:none solid solid none;border-right-width:1pt;border-right-color:#CCCCCC;height: 40px;\">");
			else
				html.AppendFormat("<td width=\"0\" style=\"width:{0}%;background-color:#426d8f;padding:2.25pt;border-style:none solid solid none;border-right-width:1pt;border-right-color:#CCCCCC;height: 40px;\">", col_percentwidth);
			html.AppendFormat("<span style=\"background-color:#426d8f;\"><div style=\"margin:0;\"><font><span style=\"font-size:12pt;\"><font color=\"white\"><span><b>{0}&nbsp;</b></span></font></span></div></td>", HtmlEncode(name));
		}

		private void CellValue( StringBuilder html, string value, int rowspan )
		{
			if( rowspan > 0)
				html.AppendFormat("<td rowspan=\"{0}\" ", rowspan);
			else
				html.Append("<td ");

			html.Append("style=\"background-color:#EFEFEF;padding:3.25pt;border-style:none solid solid none;border-right-width:1pt;border-right-color:#CCCCCC;\"><span style=\"background-color:#EFEFEF;\"><div style=\"margin:0;\"><font><span>");
			html.AppendFormat("{0}&nbsp;</span></font></div></span></td>", HtmlEncode(value));
			
		}
		*/
		#endregion
		
		private void TablePowerby( StringBuilder html, string powerby)
		{
			html.AppendFormat("<table style=\"background: #88a9c2;width: 1000px;color: white;font-size: 12px;text-align: center\"><tr><td>Power by {0}</td></tr></table>", powerby);
		} 
		private string HtmlEncode( string value)
		{
			if( string.IsNullOrEmpty(value))
				return value;
			return HttpUtility.HtmlEncode(value);
		}
	}
}

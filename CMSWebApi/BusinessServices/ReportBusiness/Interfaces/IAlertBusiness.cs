using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.DataModels;
using CMSWebApi.Utils;
using PACDMModel.Model;

namespace CMSWebApi.BusinessServices.ReportBusiness.Interfaces
{
	internal interface IAlertBusiness
	{
		IQueryable<ChartWithImageModel> AlertChartbyAlerttype(IEnumerable<int> sites, DateTime sDate, DateTime eDate, IEnumerable<byte> altTypes);

		Task<IQueryable<ChartWithImageModel>> AlertChartbyAlerttype(DateTime sdate, DateTime edate, IEnumerable<byte> alttypes, IEnumerable<int> sites);

		Task<IQueryable<ChartWithImageModel>> AlertChartbyDVR(DateTime sdate, DateTime edate, IEnumerable<int> sites, IEnumerable<byte> altTypes);

		Task<IQueryable<ChartWithImageModel>> AlertChartbyServerity(DateTime sdate, DateTime edate, IEnumerable<byte> altsever, IEnumerable<int> sites);

		Task<IQueryable<ChartWithImageModel>> AlertChartMostDVR(DateTime sdate, DateTime edate, IEnumerable<int> sites, int top);

		Task<ALertCompModel> AlertSeverityComapre(IEnumerable<int> kdvrs, AlertSeverity? AlertSeverity, DateTime altsdate, DateTime altDate, DateTime cmpsdate, DateTime cmpDate);

		Task<ALertCompModel> AlertTypeComapre(IEnumerable<int> kdvrs, List<int> alerttypes, DateTime altsdate, DateTime altDate, DateTime cmpsdate, DateTime cmpDate);

		Task<ALertCompModel> AlertTypeComapreByTZ(IEnumerable<int> kdvrs, List<int> alerttypes, DateTime altsdate, DateTime altDate, DateTime cmpsdate, DateTime cmpDate);

		Task<int> CountAlertbyALertType(DateTime sdate, DateTime edate, IEnumerable<int> kdvrs, AlertType? alerttype);

		Task<int> CountAlertbyALertTypes(DateTime sdate, DateTime edate, IEnumerable<int> kdvrs, IEnumerable<byte> alerttypes);

		Task<int> CountALertbyDate(DateTime sdate, DateTime edate, IEnumerable<int> kdvrs);

		Task<ALertCompModel> DVR_On_Offline(IEnumerable<int> kdvrs, DateTime pram, bool isoffline, int keepaliveint);

		IQueryable<ChartWithImageModel> GroupAlertType(IQueryable<IGrouping<byte, PACDMModel.Model.tAlertEvent>> altgroups);

		Task<IQueryable<ChartWithImageModel>> OverallStatisticChart(DateTime sdate, DateTime edate, IEnumerable<byte> altTypes, IEnumerable<int> sites, UserContext user, int keepaliveint);

		Task<int> CountDVRRecordingLess(int limitday, IEnumerable<int> kdvrs);
	}
}

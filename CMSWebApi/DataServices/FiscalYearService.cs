using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;
using PACDMModel;
using CMSWebApi.DataModels;
using CMSWebApi.Utils;

namespace CMSWebApi.DataServices
{
	public partial class FiscalYearService:ServiceBase,IFiscalYearServices
	{
		public FiscalYearService(PACDMModel.Model.IResposity model) : base(model) { }

		public FiscalYearService(ServiceBase ServiceBase) : base(ServiceBase.DBModel) { }

		public tCMSWeb_FiscalYear  Add(tCMSWeb_FiscalYear fiscalYear)
		{
			DBModel.Insert<tCMSWeb_FiscalYear>(fiscalYear);
			return DBModel.Save() > 0 ? fiscalYear : null;
		}
		public tCMSWeb_FiscalYear Update(tCMSWeb_FiscalYear fiscalYear)
		{
			DBModel.Update<tCMSWeb_FiscalYear>(fiscalYear);
			return DBModel.Save() >= 0 ? fiscalYear : null;
		}
		public tCMSWeb_FiscalYear GetFiscalYearInfo(int userID, bool isClone = false)
		{
			try
			{
                tCMSWeb_FiscalYear rs = new tCMSWeb_FiscalYear();
                rs = DBModel.FirstOrDefault<tCMSWeb_FiscalYear>(item => item.CreatedBy == userID);
                if (rs != null && isClone)
                {
                    tCMSWeb_FiscalYear result = new tCMSWeb_FiscalYear();
                    result.CalendarStyle = rs.CalendarStyle;
                    result.CreatedBy = rs.CreatedBy;
                    result.FYClosest = rs.FYClosest;
                    result.FYDate = rs.FYDate;
                    result.FYDateEnd = rs.FYDateEnd;
                    result.FYDateStart = rs.FYDateStart;
                    result.FYID = rs.FYID;
                    result.FYName = rs.FYName;
                    result.FYNoOfWeeks = rs.FYNoOfWeeks;
                    result.FYTypesID = rs.FYTypesID;
                    result.tCMSWeb_FiscalYear_Types = rs.tCMSWeb_FiscalYear_Types;
                    result.tCMSWeb_UserList = rs.tCMSWeb_UserList;
                    return result;
                }
                else
                {
                    return rs;
                }
			}
			catch (Exception msg)
			{
				Console.WriteLine(msg.StackTrace);
				return null;
			}

		}
        public tCMSWeb_FiscalYear GetFiscalYearInfo(UserContext user, DateTime searchDate, bool isClone = false)
		{
			tCMSWeb_FiscalYear fyInfo = new tCMSWeb_FiscalYear();
            fyInfo = GetFiscalYearInfo(user.ID, isClone);
			if (fyInfo == null && user.Createdby.HasValue)
			{
                fyInfo = GetFiscalYearInfo(user.Createdby.Value, isClone);
			}
			if (fyInfo == null)
			{
				fyInfo = new tCMSWeb_FiscalYear() { 
					CalendarStyle = FiscalCalendarConst.CStyle_454,
					CreatedBy = user.ID,
					FYClosest = (int)DayOfWeek.Saturday,
					FYDate = DateTime.Now,
					FYDateStart = new DateTime(new DateTime().Year, 1, 1),
					FYDateEnd = new DateTime(new DateTime().Year, 12, 31),
					FYID = 0,
					FYName = null,
					FYNoOfWeeks = 52,
					FYTypesID = (int)FiscalCalendarType.NORMAL
				};
			}

			if (searchDate.Date == DateTime.MinValue) { return fyInfo; }
			fyInfo = CalculateFiscalYear(searchDate, fyInfo);
			return fyInfo;
		}
		public List<FiscalPeriod> GetFiscalPeriods(tCMSWeb_FiscalYear fyInfo, DateTime date, DateTime fyStart)
		{
			DateTime fyStartDate = DateTime.MinValue;
			if (fyStart == DateTime.MinValue)
			{
				if (fyInfo == null || !fyInfo.FYDateStart.HasValue)
					fyStartDate = new DateTime(date.Year, 1, 1);
				else
					fyStartDate = fyInfo.FYDateStart.Value;
			}
			else
			{
				fyStartDate = fyStart;
			}
			List<FiscalPeriod> lsResult = new List<FiscalPeriod>();
			if (fyInfo == null || !fyInfo.FYDateStart.HasValue)
			{
				int curMonth = date.Month;
				DateTime startYear = new DateTime(date.Year, 1, 1);
				if (fyStartDate < startYear || fyStartDate > date)
					fyStartDate = startYear;

				int startMonth = fyStartDate.Month;
				DateTime curDate = new DateTime(fyStartDate.Year, fyStartDate.Month, 1);
				for (int i = startMonth; i <= curMonth; i++)
				{
					FiscalPeriod fp = new FiscalPeriod();
					fp.Period = i;
					fp.StartDate = curDate;
					curDate = curDate.AddMonths(1);
					fp.EndDate = curDate.AddDays(-1);
					fp.Weeks = GetFiscalWeeks(fp.StartDate, fp.EndDate, NumberWeekOfPeriod(fp.Period, fyInfo.CalendarStyle));
					lsResult.Add(fp);
				}
			}
			else
			{
				for (int i = 0; i < Consts.PERIOD_PER_YEAR; i++)
				{
					FiscalPeriod fp = new FiscalPeriod();
					fp.Period = i + 1;
					int numberWeek = NumberWeekOfPeriod(fp.Period, fyInfo.CalendarStyle);
					fp.StartDate = fyStartDate.Date;
					fp.EndDate = fp.StartDate.AddDays(numberWeek * 7 - 1);
					fp.Weeks = GetFiscalWeeks(fp.StartDate, fp.EndDate, numberWeek);
					lsResult.Add(fp);
					if (fp.EndDate.Date >= date.Date)
						break;
					fyStartDate = fp.EndDate.AddDays(1);
				}
				if (lsResult.Count == Consts.PERIOD_PER_YEAR && fyInfo.FYDateEnd.HasValue)
				{
					FiscalPeriod lastPeriod = lsResult.Last();
					if (lastPeriod.EndDate < fyInfo.FYDateEnd.Value.Date)
					{
						int maxWeekIdx = lastPeriod.Weeks.Any() ? lastPeriod.Weeks.Max(x => x.WeekIndex) : 0;
						FiscalWeek lastWeek = new FiscalWeek();
						lastWeek.WeekIndex = maxWeekIdx + 1;
						lastWeek.StartDate = lastPeriod.EndDate.AddDays(1).Date;
						lastWeek.EndDate = fyInfo.FYDateEnd.Value.Date;
						lastPeriod.EndDate = fyInfo.FYDateEnd.Value.Date;
						lastPeriod.Weeks.Add(lastWeek);
					}
				}
			}
			return lsResult;
		}
		public FiscalWeek GetFiscalWeek(tCMSWeb_FiscalYear fyInfo, DateTime searchDate, DateTime fyStartDate)
		{
			FiscalWeek ret = new FiscalWeek();
			List<FiscalPeriod> fyPeriods = GetFiscalPeriods(fyInfo, searchDate, fyStartDate);
			foreach (FiscalPeriod period in fyPeriods)
			{
				ret = period.Weeks.FirstOrDefault(w => searchDate.Date.CompareTo(w.StartDate.Date) >= 0 && searchDate.Date.CompareTo(w.EndDate.Date) <= 0);
				if (ret != null) { break; }
			}
			return ret;
		}

		private List<FiscalWeek> GetFiscalWeeks(DateTime sDate, DateTime eDate, int numberWeek)
		{
			List<FiscalWeek> ret = new List<FiscalWeek>();
			DateTime curDate = sDate;
			for (int i = 1; i <= numberWeek; i++)
			{
				FiscalWeek fyWeekInfo = new FiscalWeek();
				fyWeekInfo.WeekIndex = i;
				fyWeekInfo.StartDate = curDate;
				fyWeekInfo.EndDate = fyWeekInfo.StartDate.AddDays(6);
				ret.Add(fyWeekInfo);
				curDate = curDate.AddDays(7);
			}
			return ret;
		}
		private int NumberWeekOfPeriod(int period, string calStyle)
		{
			int iNumber = 4;
			if (calStyle.CompareTo(FiscalCalendarConst.CStyle_445) == 0)
			{
				iNumber = (period % 3 == 0) ? 5 : 4;
			}
			else if (calStyle.CompareTo(FiscalCalendarConst.CStyle_544) == 0)
			{
				iNumber = (period % 3 == 1) ? 5 : 4;
			}
			else //if (calStyle.CompareTo(FiscalCalendarConst.CStyle_454) == 0)
			{
				iNumber = (period % 3 == 2) ? 5 : 4;
			}
			return iNumber;
		}
		private tCMSWeb_FiscalYear CalculateFiscalYear(DateTime searchDate, tCMSWeb_FiscalYear fyInfo)
		{
			switch (fyInfo.FYTypesID)
			{
				case (int)FiscalCalendarType.NORMAL:
					fyInfo.FYDateStart = new DateTime(searchDate.Year, 1, 1);
					fyInfo.FYDateEnd = new DateTime(searchDate.Year, 12, 31);
					break;
				case (int)FiscalCalendarType.FISCAL:
					fyInfo.FYDateStart = new DateTime(searchDate.Year, fyInfo.FYDateStart.Value.Month, 1);
					if (searchDate.Date < fyInfo.FYDateStart.Value.Date)
					{
						fyInfo.FYDateStart = new DateTime(searchDate.Year - 1, fyInfo.FYDateStart.Value.Month, 1);
					}
					fyInfo.FYDateEnd = fyInfo.FYDateStart.Value.Date.AddYears(1).AddDays(-1);
					break;
				case (int)FiscalCalendarType.WEEKS_52:
					if (searchDate.Date >= fyInfo.FYDateStart.Value.Date)
					{
						double datediff = (searchDate - fyInfo.FYDateStart.Value).TotalDays;
						double weeksmod = datediff % FiscalCalendarConst.NumDayOf52Weeks;
						fyInfo.FYDateStart = fyInfo.FYDateStart.Value.AddDays(datediff - weeksmod);
						fyInfo.FYDateEnd = fyInfo.FYDateStart.Value.AddDays(FiscalCalendarConst.NumDayOf52Weeks - 1);
					}
					else
					{
						double datediff = (fyInfo.FYDateEnd.Value - searchDate).TotalDays;
						double weeksmod = datediff % FiscalCalendarConst.NumDayOf52Weeks;
						fyInfo.FYDateEnd = fyInfo.FYDateEnd.Value.AddDays(-(datediff - weeksmod));
						fyInfo.FYDateStart = fyInfo.FYDateEnd.Value.AddDays(1 - FiscalCalendarConst.NumDayOf52Weeks);
					}
					break;
				case (int)FiscalCalendarType.WEEKS_53:
					if (searchDate.Date >= fyInfo.FYDateStart.Value.Date)
					{
						double datediff = (searchDate - fyInfo.FYDateStart.Value).TotalDays;
						double weeksmod = datediff % FiscalCalendarConst.NumDayOf53Weeks;
						fyInfo.FYDateStart = fyInfo.FYDateStart.Value.AddDays(datediff - weeksmod);
						fyInfo.FYDateEnd = fyInfo.FYDateStart.Value.AddDays(FiscalCalendarConst.NumDayOf53Weeks - 1);
					}
					else
					{
						double datediff = (fyInfo.FYDateEnd.Value - searchDate).TotalDays;
						double weeksmod = datediff % FiscalCalendarConst.NumDayOf53Weeks;
						fyInfo.FYDateEnd = fyInfo.FYDateEnd.Value.AddDays(-(datediff - weeksmod));
						fyInfo.FYDateStart = fyInfo.FYDateEnd.Value.AddDays(1 - FiscalCalendarConst.NumDayOf53Weeks);
					}
					break;
				case (int)FiscalCalendarType.WEEKS_52_53:
					DateTime date1 = new DateTime(searchDate.Year, fyInfo.FYDate.Value.Month, fyInfo.FYDate.Value.Day);
					while ((int)date1.DayOfWeek != fyInfo.FYClosest)
					{
						date1 = date1.AddDays(-1);
					}
					if (date1.Date < searchDate.Date)
					{

						fyInfo.FYDateStart = date1.AddDays(1);
						DateTime _tempDate1 = date1.AddDays(FiscalCalendarConst.NumDayOf53Weeks);
						DateTime _tempDate2 = new DateTime(_tempDate1.Year, fyInfo.FYDate.Value.Month, fyInfo.FYDate.Value.Day);
						if (_tempDate1 > _tempDate2)
						{

							_tempDate1 = date1.AddDays(FiscalCalendarConst.NumDayOf52Weeks);
						}
						fyInfo.FYDateEnd = _tempDate1;
					}
					else
					{
						fyInfo.FYDateEnd = date1;
						DateTime _tempDate1 = date1.AddDays(1 - FiscalCalendarConst.NumDayOf52Weeks);
						DateTime _tempDate2 = new DateTime(_tempDate1.Year, fyInfo.FYDate.Value.Month, fyInfo.FYDate.Value.Day);
						if (_tempDate1 > _tempDate2)
						{
							_tempDate1 = date1.AddDays(1 - FiscalCalendarConst.NumDayOf53Weeks);
						}
						fyInfo.FYDateStart = _tempDate1;
					}
					break;

			}
			return fyInfo;
		}
	}
}

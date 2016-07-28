using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;
using CMSWebApi.DataModels;
using CMSWebApi.Utils;

namespace CMSWebApi.BusinessServices.FiscalYear
{
	public class FiscalYearBusinessService : BusinessBase<IFiscalYearServices>
	{
		public FiscalYearModel GetFiscalYear(UserContext user)
		{
			tCMSWeb_FiscalYear obj = DataService.GetFiscalYearInfo(user.ParentID);
			FiscalYearModel objFC = new FiscalYearModel();
			//objFC.data = new FiscalYearModel();
			if (obj == null)
			{
				objFC.CalendarStyle = FiscalCalendarConst.CStyle_454;
				objFC.CreatedBy = user.ID;
				objFC.FYClosest = (int)DayOfWeek.Saturday;
				objFC.FYDate = DateTime.Now;
				objFC.FYDateStart = new DateTime(new DateTime().Year, 1, 1);
				objFC.FYDateEnd = new DateTime(new DateTime().Year, 12, 31);
				objFC.FYID = 0;
				objFC.FYName = null;
				objFC.FYNoOfWeeks = 52;
				objFC.FYTypesID = (int)FiscalCalendarType.NORMAL;
			}
			else
			{
				objFC.CalendarStyle = obj.CalendarStyle;
				objFC.CreatedBy = obj.CreatedBy.HasValue ? obj.CreatedBy.Value : 0;
				objFC.FYClosest = obj.FYClosest.HasValue ? obj.FYClosest.Value : (int)DayOfWeek.Saturday;
				objFC.FYDate = obj.FYDate != null ? obj.FYDate.Value : new DateTime();
				objFC.FYDateStart = obj.FYDateStart.HasValue ? obj.FYDateStart.Value : new DateTime();
				objFC.FYDateEnd = obj.FYDateEnd.HasValue ? obj.FYDateEnd.Value : new DateTime();
				objFC.FYID = obj.FYID;
				objFC.FYName = obj.FYName;
				objFC.FYNoOfWeeks = obj.FYNoOfWeeks.Value;
				objFC.FYTypesID = obj.FYTypesID.Value;
			}
			return objFC;

		}

		public FiscalYearModel GetCustomFiscalYear(UserContext user, DateTime date)
		{
			FiscalYearModel objFC = GetFiscalYear(user);
			CalculateFiscalYear(date, objFC);
			return objFC;
		}

		public FiscalYearModel Update(FiscalYearModel objFC)
		{
			tCMSWeb_FiscalYear obj = DataService.GetFiscalYearInfo(objFC.CreatedBy);
			if (obj == null) obj = new tCMSWeb_FiscalYear();

			obj.CalendarStyle = objFC.CalendarStyle;
			obj.CreatedBy = objFC.CreatedBy;
			obj.FYClosest = (byte)objFC.FYClosest;
			obj.FYDate = objFC.FYDate;
			obj.FYDateEnd = objFC.FYDateEnd;
			obj.FYDateStart = objFC.FYDateStart;
			obj.FYID = objFC.FYID;
			obj.FYNoOfWeeks = objFC.FYNoOfWeeks;
			obj.FYTypesID = objFC.FYTypesID;
			if (objFC.FYID == 0)
			{
				obj = DataService.Add(obj);
			}
			else
			{
				tCMSWeb_FiscalYear Temp = DataService.Update(obj);
				obj = Temp != null ? Temp : obj;
			}

			objFC.CalendarStyle = obj.CalendarStyle;
			objFC.CreatedBy = obj.CreatedBy.HasValue ? obj.CreatedBy.Value : 0;
			objFC.FYClosest = obj.FYClosest.HasValue ? obj.FYClosest.Value : (int)DayOfWeek.Saturday;
			objFC.FYDate = obj.FYDate != null ? obj.FYDate.Value : DateTime.Now;
			objFC.FYDateStart = obj.FYDateStart.HasValue ? obj.FYDateStart.Value : DateTime.Now;
			objFC.FYDateEnd = obj.FYDateEnd.HasValue ? obj.FYDateEnd.Value : DateTime.Now;
			objFC.FYID = obj.FYID;
			objFC.FYName = obj.FYName;
			objFC.FYNoOfWeeks = obj.FYNoOfWeeks.Value;
			objFC.FYTypesID = obj.FYTypesID.Value;

			return objFC;
		}

		private FiscalYearModel CalculateFiscalYear(DateTime date, FiscalYearModel objFC)
		{
			switch (objFC.FYTypesID)
			{
				case (int)FiscalCalendarType.NORMAL:
					objFC.FYDateStart = new DateTime(date.Year, 1, 1);
					objFC.FYDateEnd = new DateTime(date.Year, 12, 31);
					break;
				case (int)FiscalCalendarType.FISCAL:
					objFC.FYDateStart = new DateTime(date.Year, objFC.FYDateStart.Month, 1);
					if (date.Date < objFC.FYDateStart.Date)
					{
						objFC.FYDateStart = new DateTime(date.Year - 1, objFC.FYDateStart.Month, 1);
					}
					objFC.FYDateEnd = objFC.FYDateStart.AddYears(1).AddDays(-1);
					break;
				case (int)FiscalCalendarType.WEEKS_52:
					if (date.Date >= objFC.FYDateStart.Date)
					{
						double datediff = (date - objFC.FYDateStart).TotalDays;
						double weeksmod = datediff % FiscalCalendarConst.NumDayOf52Weeks;
						objFC.FYDateStart = objFC.FYDateStart.AddDays(datediff - weeksmod);
						objFC.FYDateEnd = objFC.FYDateStart.AddDays(FiscalCalendarConst.NumDayOf52Weeks - 1);
					}
					else
					{
						double datediff = (objFC.FYDateEnd - date).TotalDays;
						double weeksmod = datediff % FiscalCalendarConst.NumDayOf52Weeks;
						objFC.FYDateEnd = objFC.FYDateEnd.AddDays(-(datediff - weeksmod));
						objFC.FYDateStart = objFC.FYDateEnd.AddDays(1 - FiscalCalendarConst.NumDayOf52Weeks);

					}
					break;
				case (int)FiscalCalendarType.WEEKS_53:
					if (date.Date >= objFC.FYDateStart.Date)
					{
						double datediff = (date - objFC.FYDateStart).TotalDays;
						double weeksmod = datediff % FiscalCalendarConst.NumDayOf53Weeks;
						objFC.FYDateStart = objFC.FYDateStart.AddDays(datediff - weeksmod);
						objFC.FYDateEnd = objFC.FYDateStart.AddDays(FiscalCalendarConst.NumDayOf53Weeks - 1);
					}
					else
					{
						double datediff = (objFC.FYDateEnd - date).TotalDays;
						double weeksmod = datediff % FiscalCalendarConst.NumDayOf53Weeks;
						objFC.FYDateEnd = objFC.FYDateEnd.AddDays(-(datediff - weeksmod));
						objFC.FYDateStart = objFC.FYDateEnd.AddDays(1 - FiscalCalendarConst.NumDayOf53Weeks);
					}
					break;
				case (int)FiscalCalendarType.WEEKS_52_53:
					DateTime date1 = new DateTime(date.Year, objFC.FYDate.Month, objFC.FYDate.Day);
					while ((int)date1.DayOfWeek != objFC.FYClosest)
					{
						date1 = date1.AddDays(-1);
					}
					if (date1.Date < date.Date)
					{

						objFC.FYDateStart = date1.AddDays(1);
						DateTime _tempDate1 = date1.AddDays(FiscalCalendarConst.NumDayOf53Weeks);
                        DateTime _tempDate2 = new DateTime();
                        if (objFC.FYDate.Month == 2 && objFC.FYDate.Day == 29)
                        {
                            _tempDate2 = DateTime.IsLeapYear(_tempDate1.Year) ? new DateTime(_tempDate1.Year, objFC.FYDate.Month, objFC.FYDate.Day) : new DateTime(_tempDate1.Year, objFC.FYDate.Month, 28);

                        }
                        else
                        {
                            _tempDate2 = new DateTime(_tempDate1.Year, objFC.FYDate.Month, objFC.FYDate.Day);
                        }
						
						if (_tempDate1 > _tempDate2)
						{

							_tempDate1 = date1.AddDays(FiscalCalendarConst.NumDayOf52Weeks);
						}
						objFC.FYDateEnd = _tempDate1;
					}
					else
					{
						objFC.FYDateEnd = date1;
						DateTime _tempDate1 = date1.AddDays(1 - FiscalCalendarConst.NumDayOf52Weeks);
                        DateTime _tempDate2 = new DateTime(); //new DateTime(_tempDate1.Year, objFC.FYDate.Month, objFC.FYDate.Day);

                        if (objFC.FYDate.Month == 2 && objFC.FYDate.Day == 29)
                        {
                            _tempDate2 = DateTime.IsLeapYear(_tempDate1.Year) ? new DateTime(_tempDate1.Year, objFC.FYDate.Month, objFC.FYDate.Day) : new DateTime(_tempDate1.Year, objFC.FYDate.Month, 28);

                        }
                        else
                        {
                            _tempDate2 = new DateTime(_tempDate1.Year, objFC.FYDate.Month, objFC.FYDate.Day);
                        }
						if (_tempDate1 > _tempDate2)
						{
							_tempDate1 = date1.AddDays(1 - FiscalCalendarConst.NumDayOf53Weeks);
						}
						objFC.FYDateStart = _tempDate1;
					}
					break;

			}
			return objFC;
		}
	}
}

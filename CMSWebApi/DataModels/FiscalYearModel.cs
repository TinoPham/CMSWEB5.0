using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.DataModels
{
	public class FiscalBase
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
	}

	public class FiscalYearModel
	{
		public int FYID{get;set;}
		public string FYName{get;set;}
		public int FYTypesID{get;set;}
		public DateTime FYDateStart{get;set;}
		public DateTime FYDateEnd{get;set;}
		public int FYClosest{get;set;}
		public int FYNoOfWeeks{get;set;}
		public int CreatedBy{get;set;}
		public DateTime FYDate{get;set;}
		public string CalendarStyle{get;set;}
	}
	
	public class FiscalPeriod:FiscalBase
	{
		public int Period { get; set; }
		public List<FiscalWeek> Weeks { get; set; }
	}

	public class FiscalWeek:FiscalBase
	{
		public int WeekIndex { get; set; }
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.DataModels
{
	public class ALertCompModel
	{
		public decimal Value{ get ;set;}
		public decimal CmpValue{ get ;set;}
		public Nullable<bool> Increase{ get; set;}
	}
	public class POSConversionModel
	{
		public int PACID { get; set; }
		public DateTime DVRDateKey { get; set; }
		public decimal TotalSales { get; set; }
		public int TotalTrans { get; set; }
		public int TotalTraffic { get; set; }
		public decimal AvgConv { get; set; }
		//public CMSWebApi.Utils.PeriodType Period{ get; set;}
	}

	/*
	public class BoxGadgetModel
	{
		public string NameBox { get; set; }
		public decimal Value { get; set; }
		public decimal ValueCompare { get; set; }
		public bool Increase { get; set; }
	}

	public class POSValueModel
	{
		public int PACID { get; set; }
		public DateTime DVRDateKey { get; set; }
		public decimal TotalSales { get; set; }
		public int TotalTrans { get; set; }
		public decimal Conversion { get; set; }
		public int TotalTraffic { get; set; }
	}

	public class BoxgagetDataModel
	{
		public BoxgagetDataModel()
		{
			BoxgadGet = new BoxGadgetModel();
		}

		public string NameGadget { get; set; }
		public BoxGadgetModel BoxgadGet { get; set; }
	}
	*/
}

using CMSWebApi.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Net.Http;

namespace CMSWebApi.DataModels.ModelBinderProvider
{
	public class ParamBase
	{
		public DateTime DateFrom { get; set; }
		public DateTime DateTo { get; set; }
		public List<int> SiteKeys { get; set; }
		public List<int> PaymentIDs { get; set; }
		public bool PaymentIDs_AND { get; set; }
		public List<int> RegIDs { get; set; }
		public bool RegIDs_AND { get; set; }
		public List<int> EmpIDs { get; set; }
		public bool EmpIDs_AND { get; set; }
		public decimal TransNB { get; set; }
		public string TransNB_OP { get; set; }
		public bool TransNB_AND { get; set; }
		public decimal TransAmount { get; set; }
		public string TransAmount_OP { get; set; }
		public bool TransAmount_AND { get; set; }
		public int GroupByField { get; set; }
		public List<int> PACIDs { get; set; }
		public int MaxRows { get; set; }
		public bool TypeMatch { get; set; }
	}
	public class CannedRptParam:ParamBase
	{
		public int ReportID { get; set; }
	}

	public class QuickSearchParam: ParamBase
	{
		public List<int> DescIDs { get; set; }
		public bool DescIDs_AND { get; set; }
	}
}

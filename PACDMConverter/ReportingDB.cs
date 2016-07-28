using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSAccessObjects;

namespace PACDMConverter
{
	internal class ReportingDB : Commons.SingletonClassBase<ReportingDB>
	{
		//private static readonly Lazy<ReportingDB> Lazy = new Lazy<ReportingDB>(() => new ReportingDB());
		//public static ReportingDB Instance { get { return Lazy.Value; } }

		const string SQL_AliasDiscrete = "Select FixedInternalName, CustomName FROM AliasDiscrete";
		const string SQL_CA_AliasDiscrete = "Select FixedInternalName, CustomName FROM CA_AliasDiscrete";
		const string SQL_AliasNumbers = "Select FixedInternalName, CustomName FROM AliasNumbers";
		const string STR_FixedInternalName = "FixedInternalName";
		const string STR_CustomName = "CustomName";
		Dictionary<string, string>_CA_AliasDiscrete;

		Dictionary<string, string> _AliasDiscrete;

		Dictionary<string, string> _ATM_Discrete;

		Dictionary<string, string> _AliasNumber;
		static MSAccess Reportingdb;

		public Dictionary<string, string> ATM_AliasDiscrete
		{
			get{
				if (_ATM_Discrete == null)
				{
					_ATM_Discrete = new Dictionary<string, string>();
					_ATM_Discrete.Add("T_TransXString1", "Extra String 1");
					_ATM_Discrete.Add("T_TransXString2", "Extra String 2");
				}
				return _ATM_Discrete;
			}
		}

		public Dictionary<string, string> AliasDiscrete
		{
			get
			{
				if (_AliasDiscrete == null)
				{
					_AliasDiscrete = GetDiscreteData(SQL_AliasDiscrete);
				}

				return _AliasDiscrete;
			}

		}

		public Dictionary<string, string> AliasNumber
		{
			get
			{
				if (_AliasNumber == null)
				{
					_AliasNumber = GetDiscreteData(SQL_AliasNumbers);
				}

				return _AliasNumber;
			}

		}

		public Dictionary<string, string> CA_AliasDiscrete
		{
			get {
				if( _CA_AliasDiscrete == null)
					_CA_AliasDiscrete = GetDiscreteData(SQL_CA_AliasDiscrete);
				return _CA_AliasDiscrete; 
			}
		}
		static ReportingDB()
		{
			Reportingdb = new MSAccess();
			Reportingdb.DBFile = DVRInfos.Instance.PACDMInfo.ReportingDBPath;
		}
		private Dictionary<string, string> GetDiscreteData(string sqlcommand)
		{
			DataTable tblDiscrete = Reportingdb.SelectDatabySQLComand(sqlcommand);
			if (tblDiscrete == null)
				return new Dictionary<string, string>();
			Dictionary<string, string> dict = new Dictionary<string, string>();
			foreach (DataRow row in tblDiscrete.Rows)
				dict.Add(row.Field<string>(STR_FixedInternalName), row.Field<string>(STR_CustomName));
			return dict;
		}

	}
}

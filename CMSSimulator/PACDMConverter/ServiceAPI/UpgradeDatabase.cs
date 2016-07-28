using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace ServiceManager
{
	public class UpgradeDatabase
	{
		public const string PreConnStr = " Provider = Microsoft.Jet.OLEDB.4.0; Data Source = ";
		public const string REPORTINGDB_NAME = "ReportingDb.mdb";
		public const string POS_EXCEPTION_TABLE_NAME = "POS_Exception";
		public const string POS_EXC_BANYCONTENT_COL_NAME = "POS_Exc_Desc_ContentAny";//tram modifies Apr 13, 2011
		public const string POS_EXCEPTION_CHANNEL_COLUMN = "POS_Exc_CameraNo";
		public const int POS_CHANNEL_SUPPORT = 64;
		//public string DbPath = "D:\\PAC\\";
		private static DBConnection GetReportingDBConnection(string pacdir)
		{
			if (!pacdir.EndsWith("\\"))
				pacdir += @"\\";
			//Open connection to  ReportingDB  database
			string connString = PreConnStr + pacdir + REPORTINGDB_NAME;
			DBConnection db = new DBConnection(connString);
			return db;
		}
		public static bool UpgratePOSExceptionStructure(string pacdir)
		{
			DBConnection db = GetReportingDBConnection(pacdir);
			string sqlQuery = "SELECT TOP 1 * FROM " + POS_EXCEPTION_TABLE_NAME;
			if (db.ExecuteQuery(sqlQuery) && db.ResultDataTable != null)//Exist POS_Exception table
			{
				if (!db.ResultDataTable.Columns.Contains(POS_EXC_BANYCONTENT_COL_NAME))
				{
					sqlQuery = "ALTER TABLE " + POS_EXCEPTION_TABLE_NAME + " ADD COLUMN " + POS_EXC_BANYCONTENT_COL_NAME + " TINYINT DEFAULT 1 ";//Tram modifies Apr 22, 2011 - fix bug #19682 - Exception setup: 'Content any' should be checked as default. 
					db.ExecuteNoneQuery(sqlQuery);
				}
			}
			db.CloseConnection();
			db = null;

			return true;
		}
		public static bool CheckExcpetionChannelColumn(string pacdir)
		{
			DBConnection dbcon = GetReportingDBConnection(pacdir);
			if (dbcon == null || !dbcon.OpenConnection())
				return false;
			DataTable tblSchema = dbcon.GetSchema(POS_EXCEPTION_TABLE_NAME);
			if (tblSchema == null)
				return false;
			DataRow[] Rows = tblSchema.Select(string.Format("ColumnName = '{0}'", POS_EXCEPTION_CHANNEL_COLUMN));
			if (Rows.Length == 0)
				return false;
			DataRow r_channel = Rows[0];
			int col_size = 0;
			bool ret = false;
			Int32.TryParse(r_channel["ColumnSize"].ToString(), out col_size);
			if( col_size < POS_CHANNEL_SUPPORT)
			{
				string sql = string.Format("ALTER TABLE {0} ALTER COLUMN {1} TEXT({2})", POS_EXCEPTION_TABLE_NAME,POS_EXCEPTION_CHANNEL_COLUMN, POS_CHANNEL_SUPPORT);
				ret = dbcon.ExecuteNoneQuery(sql);
			}
			dbcon.CloseConnection();
			dbcon = null;
			return ret;
		}
	}
}

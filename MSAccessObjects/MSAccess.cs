using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.OleDb;
using System.IO;

namespace MSAccessObjects
{
	public class MSAccess
	{
		const string MS_Access_ConnectionString = "Provider = Microsoft.ACE.OLEDB.12.0; Data Source = ";
		const string MS_Access_ConnectionString_x32 = "Provider = Microsoft.Jet.OLEDB.4.0; Data Source = ";
		const string STR_TABLE = "TABLE";
		public const string STR_TABLE_NAME = "TABLE_NAME";
		private string _DBFile;
		public string DBFile
		{
			get { return _DBFile;}
			set { _DBFile = value;}
		}
		private string GetConnectionString(string filepath)
		{
			if (IntPtr.Size == 4)
				return MS_Access_ConnectionString_x32 + filepath;
			else
				return MS_Access_ConnectionString + filepath;
		}

		private OleDbConnection GetAccessConnection(string filename)
		{
			if (File.Exists(filename))
				return new OleDbConnection(GetConnectionString(filename));
			return null;
		}

		public DataTable DBSchema()
		{
			OleDbConnection con = GetAccessConnection(DBFile);
			try
			{
				con.Open();
				DataTable tblNamse = con.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, STR_TABLE });
				return tblNamse;

			}

			catch (Exception)
			{
				return null;
			}

			finally
			{
				if (con != null)
				{
					con.Close();
					con.Dispose();
					con = null;
				}

			}
		}

		public List<T> SelectListDatabySQLComand<T>(string sqlcommand, string tableName)
		{
			DataTable tblret = SelectDatabySQLComand(DBFile, sqlcommand);
			if (tblret != null)
				tblret.TableName = tableName;
			return Commons.ObjectUtils.TabletoList<T>(tblret);
		}
		public T SelectDatabySQLComand<T>(string sqlcommand, string tableName, int objectindex = 0)
		{
			DataTable tblret = SelectDatabySQLComand(DBFile, sqlcommand);
			if (tblret == null || tblret.Rows.Count <= objectindex)
				return default(T);
			tblret.TableName = tableName;
			return Commons.ObjectUtils.RowToObject<T>(tblret.Rows[objectindex]);
		}
		/// <summary>
		/// Select Data by sql command. Return null if any exception happen.
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="sqlcommand"></param>
		/// <returns></returns>
		private DataTable SelectDatabySQLComand(string filename, string sqlcommand)
		{
			DataTable ret_table = null;
			using (OleDbConnection con = GetAccessConnection(filename))
			{
				try
				{
					ret_table = new DataTable();
					using (OleDbDataAdapter ad = new OleDbDataAdapter(sqlcommand, con))
					{
						ad.Fill(ret_table);
					}
				}
				catch (Exception)
				{
					ret_table = null;
				}
				return ret_table;
			}
		}
		public DataTable SelectDatabySQLComand(string sqlcommand)
		{
			 return SelectDatabySQLComand(DBFile, sqlcommand);
		}

	}
}

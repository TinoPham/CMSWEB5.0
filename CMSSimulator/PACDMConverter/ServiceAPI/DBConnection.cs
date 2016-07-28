using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;
using System.Data;

namespace ServiceManager
{
	public class DBConnection
	{
		#region Members
		private OleDbConnection dbConnection;
		private OleDbCommand dbCommand;
		private OleDbDataAdapter dbAdapter;
		private DataTable resultDataTable;

		private string msgErr;
		private string sqlQuery;
		#endregion

		#region Properties
		public DataTable ResultDataTable
		{
			get
			{
				return resultDataTable;
			}
		}

		public string SqlQuery
		{
			get { return sqlQuery; }
			set { sqlQuery = value; }
		}
		#endregion

		#region Constructors
		public DBConnection(string connString)
		{
			try
			{
				dbConnection = new OleDbConnection(connString);
				
			}
			catch (Exception e)
			{
				msgErr = e.Message;
			}
		}
		#endregion

		#region Open/Close Connection
		public bool OpenConnection()
		{
			try
			{
				if (this.dbConnection.State != System.Data.ConnectionState.Closed)
					this.dbConnection.Close();
				this.dbConnection.Open();
				return true;
			}
			catch
			{
				return false;
			}
		}
		/// <summary>
		/// Close the connection to database by DBConnection
		/// </summary>
		/// <returns></returns>
		public bool CloseConnection()
		{
			try
			{
				if (this.dbConnection != null)
				{
					if (this.dbConnection.State != System.Data.ConnectionState.Closed)
						this.dbConnection.Close();
					dbConnection.Dispose();
					dbConnection = null;
				}
				return true;
			}
			catch
			{
				return false;
			}
		}
		#endregion


		#region Execute
		public bool ExecuteNoneQuery()
		{
			return ExecuteNoneQuery(this.sqlQuery);
		}
		public bool ExecuteNoneQuery(string _sqlQuery)
		{
			if (string.IsNullOrEmpty(_sqlQuery))
			{
				return false;
			}

			bool bSuccess = false;

			try
			{
				if (this.dbConnection.State == System.Data.ConnectionState.Closed)
				{
					this.dbConnection.Open();
				}

				if (this.dbCommand == null)
				{
					this.dbCommand = new OleDbCommand(_sqlQuery, this.dbConnection);
				}
				else
				{
					if (this.dbCommand.Connection == null)
						this.dbCommand.Connection = this.dbConnection;
					this.dbCommand.CommandText = _sqlQuery;
				}

				this.dbCommand.ExecuteNonQuery();
				bSuccess = true;
			}
			catch (System.Exception e)
			{
				msgErr = e.Message;
				bSuccess = false;
			}
			return bSuccess;	
		}
		public bool ExecuteQuery()
		{
			return ExecuteQuery(this.sqlQuery);
		}
		public bool ExecuteQuery(string _sqlQuery)
		{
			if (string.IsNullOrEmpty(_sqlQuery))
			{
				return false;
			}

			bool _bSuccess = true;
			try
			{
				if (this.dbCommand == null)
					this.dbCommand = new OleDbCommand(_sqlQuery, this.dbConnection);
				else
				{
					if (this.dbCommand.Connection == null)
						this.dbCommand.Connection = this.dbConnection;
					this.dbCommand.CommandText = _sqlQuery;
				}

				if (this.dbAdapter == null)
					this.dbAdapter = new OleDbDataAdapter(this.dbCommand);
				else
					this.dbAdapter.SelectCommand = this.dbCommand;

				this.resultDataTable = new DataTable();
				this.dbAdapter.Fill(this.resultDataTable);
				if (this.resultDataTable == null)
					_bSuccess = false;
			}
			catch(Exception e)
			{
				msgErr = e.Message;
				this.resultDataTable = null;
				_bSuccess = false;
			}
			
			return _bSuccess;
		}
		public DataTable GetSchema(string tableName)
		{
			string sql = "Select * from " + tableName;
			if (dbCommand == null)
				this.dbCommand = new OleDbCommand(sql, this.dbConnection);
			else
				dbCommand.CommandText = sql;

			dbCommand.Connection = dbConnection;
			DataTable tblSchema = null;
			OleDbDataReader dbReader = null;
			try
			{

				dbReader = dbCommand.ExecuteReader(CommandBehavior.SchemaOnly);
				tblSchema = dbReader.GetSchemaTable();
			}
			catch (System.Exception ex)
			{
				msgErr = ex.Message;
				return null;
			}
			finally
			{
				if( dbReader != null)
				{
					dbReader.Close();
					dbReader.Dispose();
					dbReader = null;
				}
			}
			return tblSchema;
		}
		#endregion


	}
}

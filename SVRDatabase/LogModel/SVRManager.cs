using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Commons.Resources;
using SVRDatabase.Model;

namespace SVRDatabase
{
    public class SVRManager
    {
        private string _connection = null;
        private SVRDB _svrDB = null;
		
		ServiceConfig _SVRConfig = null;
		public ServiceConfig SVRConfig
		{
			get{
					if( _SVRConfig == null)
						_SVRConfig = CurrentServiceConfig();
					return _SVRConfig;
			}
		}
		//private static readonly Lazy<SVRManager> sv = new Lazy<SVRManager>(() => new SVRManager());
		
		//public static SVRManager Instance { get { return sv.Value; } }

		public SVRManager():this("LogContext")
		{

		}

		public SVRManager(string connect)
		{
			_connection = string.IsNullOrEmpty(connect) ? "LogContext" : connect;
			_svrDB = new SVRDB(connect);
		}

		public void ConnectDB()
		{
			_svrDB = new SVRDB(_connection);
		}

		public IQueryable<T>GetData<T>( Expression<Func<T, bool>> filter = null ) where T:class
		{
			return _svrDB.Query<T>( filter);
		}
        #region -----------------------DVRMessage CRUD
        public void InsertDVRMessage(int messageId, byte[] messageBody, DateTime dvrDate)
        {
            var dvrMsg = new DVRMessage();
            dvrMsg.MessageID = messageId;
            dvrMsg.MessageBody = messageBody;
            dvrMsg.DvrDate = dvrDate;
            _svrDB.Insert(dvrMsg);
            _svrDB.Save();
        }

        public void InsertDVRMessage(DVRMessage dvrm)
        {
            _svrDB.Insert(dvrm);
            _svrDB.Save();
        }

        public List<DVRMessage> GetDVRMessages(Expression<Func<DVRMessage, bool>> filter = null)
        {
            return _svrDB.Query<DVRMessage>(filter).ToList();
        }
        public void UpdateDVRMessage(int messageId, byte[] messageBody, DateTime dvrDate)
        {
            var dvrMsg = new DVRMessage();
            dvrMsg.MessageID = messageId;
            dvrMsg.MessageBody = messageBody;
            dvrMsg.DvrDate = dvrDate;
            _svrDB.Update(dvrMsg);
            _svrDB.Save();
        }
        public void UpdateDVRMessage(DVRMessage dvrm)
        {
            _svrDB.Update(dvrm);
            _svrDB.Save();
        }
        public void DeleteDVRMessage(int ID)
        {
            var dvrMsg = new DVRMessage();
            dvrMsg.ID = ID;
            _svrDB.Delete(dvrMsg);
            _svrDB.Save();
        }
        #endregion

        #region -----------------------Log CRUD
        /// <summary>
        /// Insert Log
        /// </summary>
        /// <param name="logId"></param>
        /// <param name="message"></param>
        /// <param name="messageData"></param>
        /// <param name="programset"></param>
        /// <param name="dvrDate"></param>
        public void InsertLog(int logId, string message, byte[] messageData, byte programset, DateTime dvrDate)
        {
            var log = new Log();
            log.LogID = logId;
            log.Message = message;
            log.MessageData = messageData;
            log.ProgramSet = programset;
            log.DVRDate = dvrDate;
            _svrDB.Insert(log);
            _svrDB.Save();
        }
        public void InsertLog(Log log)
        {
            _svrDB.Insert(log);
            _svrDB.Save();
        }
        public void UpdateLogs(Log log)
        {
            _svrDB.Update(log);
            _svrDB.Save();
        }
        public void DeleteLog(long ID)
        {
            var log = new Log();

            log.ID = ID;
            _svrDB.Delete(log);
            _svrDB.Save();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
		//public List<Log> GetLogs(Expression<Func<Log, bool>> filter = null)
		//{
		//	return _svrDB.Query<Log>(filter).ToList();
		//}
		public IQueryable<Log> GetLogs(Expression<Func<Log, bool>> filter = null)
        {
			return _svrDB.Query<Log>(filter);
        }
        #endregion

        #region -----------------------DBConfig CRUD
        public void InsertDBConfig(string name, string server, string userid, string password, bool trusted, DateTime createDate, DateTime lastEditDate, bool IsActive)
        {
            var dbConfig = new DBConfig();
            dbConfig.Name = name;
            dbConfig.Server = server;
            dbConfig.UserID = userid;
            dbConfig.Password = password;
            dbConfig.Trusted = trusted;
            dbConfig.CreateDate = createDate;
            dbConfig.LastEditDate = lastEditDate;
            dbConfig.IsActive = IsActive;
            _svrDB.Insert(dbConfig);
            _svrDB.Save();
        }
        public void InsertDBConfig(DBConfig dbc)
        {
            _svrDB.Insert(dbc);
            _svrDB.Save();
        }
        public List<DBConfig> GetDBConfigs(Expression<Func<DBConfig, bool>> filter = null)
        {
          return _svrDB.Query<DBConfig>(filter).ToList();
        }
        public void UpdateDBConfig(string name, string server, string userid, string password, bool trusted, DateTime createDate, DateTime lastEditDate, bool IsActive)
        {
            var dbConfig = new DBConfig();
            dbConfig.Name = name;
            dbConfig.Server = server;
            dbConfig.UserID = userid;
            dbConfig.Password = password;
            dbConfig.Trusted = trusted;
            dbConfig.CreateDate = createDate;
            dbConfig.LastEditDate = lastEditDate;
            dbConfig.IsActive = IsActive;
            _svrDB.Update(dbConfig);
            _svrDB.Save();
        }

        public void UpdateDBConfig(DBConfig dbc)
        {
            _svrDB.Update(dbc);
            _svrDB.Save();
        }

        public void DeleteDBConfig(int ID)
        {
            var dbConfig = new DBConfig();
            dbConfig.ID = ID;
            _svrDB.Delete(dbConfig);
            _svrDB.Save();
        }

        public bool Login(string userName, string password)
        {
            var login = _svrDB.Query<DBConfig>(e => e.Name == userName && e.Password == password).ToList();
            if (login.Count > 0)
            {
                return true;
            }
            return false;   
        }
        #endregion

        #region -----------------------ServicesConfig
        public void InsertServiceConfig(byte converterLimit, string dvrAuthenticate, byte converterInterval)
        {
            var serviceConfig = new ServiceConfig();
            serviceConfig.ConverterLimit = converterLimit;
            serviceConfig.DVRAuthenticate = dvrAuthenticate;
            serviceConfig.ConverterInterval = converterInterval;
            _svrDB.Insert(serviceConfig);
            _svrDB.Save();
        }
        public void InsertServiceConfig(ServiceConfig ser)
        {
            _svrDB.Insert(ser);
            _svrDB.Save();
        }

        public List<ServiceConfig> GetServiceConfigs(Expression<Func<ServiceConfig, bool>> filter = null)
        {
            return _svrDB.Query<ServiceConfig>(filter).ToList();
        }
        public void UpdateServiceConfig(byte converterLimit, string dvrAuthenticate, byte converterInterval)
        {
            var serviceConfig = new ServiceConfig();
            serviceConfig.ConverterLimit = converterLimit;
            serviceConfig.DVRAuthenticate = dvrAuthenticate;
            serviceConfig.ConverterInterval = converterInterval;
            _svrDB.Update(serviceConfig);
            _svrDB.Save();;
        }
        public void UpdateServiceConfig(ServiceConfig ser)
        {
            _svrDB.Update(ser);
            _svrDB.Save();
        }
        public void DeleteServiceConfig(int ID)
        {
            var serviceConfig = new ServiceConfig();
            serviceConfig.ID = ID;
            _svrDB.Delete(serviceConfig);
            _svrDB.Save();
        }

		private ServiceConfig CurrentServiceConfig()
		{
			List<ServiceConfig> lstconfig = GetServiceConfigs();
			ServiceConfig itemMax = (from i in lstconfig
									 let maxId = lstconfig.Max(m => m.ID)
									 where i.ID == maxId
									 select i).FirstOrDefault();
			return itemMax;
		}
        #endregion

		#region -----------------------DVRDetail
		
		public IEnumerable<DVRDetail>GetDVRDetails(DVRInfo dvrinfo)
		{
			if( dvrinfo == null || dvrinfo.ID <= 0)
				return null;

			return GetDVRDetails(dvrinfo.ID);
		}

		public IEnumerable<DVRDetail>GetDVRDetails(Int64 DVRID)
		{
			return _svrDB.Query<DVRDetail>( item => item.ID == DVRID).AsEnumerable();
		}
		public bool InsertDVRDetail( DVRDetail DVRDetail)
		{
			try
			{
			_svrDB.Insert<DVRDetail>(DVRDetail);
			_svrDB.Save();
			return true;
			}
			catch(Exception ){ return false;}
		}
		public bool DeleteDVRDetail(DVRDetail detail)
		{
			try
			{
			_svrDB.Delete<DVRDetail>(detail);
			_svrDB.Save();
			return true;
			}
			catch(Exception){ return false;}
		}
		#endregion
		#region -----------------------DVRInfo
		public void InsertDVRInfo(long KDVR, DateTime lastConvert, DateTime lastDVRMessage, DateTime createDate, bool forceStop, byte converterInterval)
        {
                var dvrInfo = new DVRInfo();
                dvrInfo.KDVR = KDVR;
                dvrInfo.LastConvert = lastConvert;
                dvrInfo.LastDvrMessage = lastDVRMessage;
                dvrInfo.CreateDate = createDate;
                dvrInfo.ForceStop = forceStop;
                dvrInfo.ConverterInterval = converterInterval;
                _svrDB.Insert(dvrInfo);
                _svrDB.Save();
        }
        public void InsertDVRInfo(DVRInfo dvrInfo)
        {
                _svrDB.Insert(dvrInfo);
                _svrDB.Save();
        }

        public List<DVRInfo> GetDVRInfo(Expression<Func<DVRInfo, bool>> filter = null)
        {
            try
            {
                return _svrDB.Query<DVRInfo>(filter).ToList();
            }
            catch
            { 
                return null;
            }
        }
        public void UpdateDVRInfo(long KDVR, DateTime lastConvert, DateTime lastDVRMessage, DateTime createDate, bool forceStop, byte converterInterval)
        {
            var dvrInfo = new DVRInfo();
            dvrInfo.KDVR = KDVR;
            dvrInfo.LastConvert = lastConvert;
            dvrInfo.LastDvrMessage = lastDVRMessage;
            dvrInfo.CreateDate = createDate;
            dvrInfo.ForceStop = forceStop;
            dvrInfo.ConverterInterval = converterInterval;
            _svrDB.Update(dvrInfo);
            _svrDB.Save();
        }

        public void UpdateDVRInfo(DVRInfo dvrInfo)
        {
            _svrDB.Update(dvrInfo);
            _svrDB.Save();
        }

        public void DeleteDVRInfo(int ID)
        {
                var dvrInfo = new DVRInfo();
                dvrInfo.KDVR = ID;
                _svrDB.Delete(dvrInfo);
                _svrDB.Save();
        }
        #endregion

        #region -----------------------SeqCount CRUD

        public void InsertSeqCount(SeqCount md)
        {
            _svrDB.Insert(md);
            _svrDB.Save();
        }
        public List<SeqCount> GetSeqCounts(Expression<Func<SeqCount, bool>> filter = null)
        {
            return _svrDB.Query<SeqCount>(filter).ToList();
        }
        public Int64 GetNextId(Expression<Func<SeqCount, bool>> filter = null)
        {
            SeqCount sq = _svrDB.Query<SeqCount>(filter).FirstOrDefault();
            try
            {
                sq.Sequence = sq.Sequence + 1;
            }
            catch 
            {
                return 0;
            }
            _svrDB.Update<SeqCount>(sq);
            _svrDB.Save();
            return sq.Sequence;
        }
        public void UpdateSeqCount(SeqCount seq)
        {
            _svrDB.Update(seq);
            _svrDB.Save();
        }
        public void DeleteSeqCount(int ID)
        {
            var sq = new SeqCount();
            sq.ID = ID;
            _svrDB.Delete(sq);
            _svrDB.Save();
        }
        #endregion

        #region -----------------------API USER CRUD
		public ApiUser CheckUser(Expression<Func<ApiUser, bool>> filter = null)
		{
			return _svrDB.FirstOrDefault<ApiUser>( filter);
		}
        public void InsertApiUser(ApiUser user)
        {
            _svrDB.Insert(user);
            _svrDB.Save();
        }
        public List<ApiUser> GetApiUser(Expression<Func<ApiUser, bool>> filter = null)
        {
            return _svrDB.Query<ApiUser>(filter).ToList();
        }
        public void UpdateApiUser(ApiUser user)
        {
            _svrDB.Update(user);
            _svrDB.Save();
        }
        public void DeleteApiUser(int ID)
        {
            var user = new ApiUser();
            user.ID = ID;
            _svrDB.Delete(user);
            _svrDB.Save();

        }
        #endregion

        #region -----------------------AuthToken CRUD
        public void InsertAuthToken(AuthToken token)
        {
            _svrDB.Insert(token);
            _svrDB.Save();
        }
        public List<AuthToken> GetAuthToken(Expression<Func<AuthToken, bool>> filter = null)
        {
            return _svrDB.Query<AuthToken>(filter).ToList();
        }
        public void UpdateAuthToken(AuthToken token)
        {
            _svrDB.Update(token);
            _svrDB.Save();
        }
        public void DeleteAuthToken(int ID)
        {
            var token = new AuthToken();
            token.ID = ID;
            _svrDB.Delete(token);
            _svrDB.Save();
        }
        #endregion

		public void DeleteWhere<T>( Expression<Func<T, bool>> predicate) where T : class
		{
			_svrDB.Delete<T>(predicate);
			_svrDB.Save();
		}

    }
}

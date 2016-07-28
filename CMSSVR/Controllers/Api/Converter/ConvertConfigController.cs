using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using CMSSVR.Models.Api.Configuration;
using Commons;
using ConverterSVR.Services;
using PACDMModel;
using PACDMModel.Model;
using SVRDatabase;
using SVRDatabase.Model;

namespace CMSSVR.Controllers.Api.Converter
{
	public class ConvertConfigController : ApiController
	{
		protected SVRManager DBModel = GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(SVRManager)) as SVRManager;
		protected PACDMModel.PACDMDB PACDMModel = GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(PACDMModel.PACDMDB)) as PACDMModel.PACDMDB;

		public override System.Threading.Tasks.Task<HttpResponseMessage> ExecuteAsync(System.Web.Http.Controllers.HttpControllerContext controllerContext, System.Threading.CancellationToken cancellationToken)
		{
			if(DBModel == null)
			{
				return Task.FromResult<HttpResponseMessage>(Request.CreateResponse(HttpStatusCode.InternalServerError, Commons.Resources.ResourceManagers.Instance.GetResourceString(ERROR_CODE.DB_CONNECTION_FAILED)));
			}

			return base.ExecuteAsync(controllerContext, cancellationToken);
		}

		#region Account
		[HttpGet]
		public HttpResponseMessage GetAccount()
		{
			return Request.CreateResponse( HttpStatusCode.OK, DBModel.CheckUser() == null? 0 : 1);
		}

		[HttpPost]
		public HttpResponseMessage LogIn(ApiUser User)
		{
			ApiUser n_user = DBModel.CheckUser(item => string.Compare(item.UserName, User.UserName, true) == 0 && string.Compare(item.Password, User.Password, false) == 0);
			if( n_user == null)
				return Request.CreateResponse( HttpStatusCode.Unauthorized ,"Log in failed");

			return Request.CreateResponse(HttpStatusCode.OK, n_user);
		}

		[HttpPost]
		public HttpResponseMessage AddAccount(UserEditModel User)
		{
			ApiUser n_user = new ApiUser{ Name = User.Name, UserName = User.UserName, Password = User.Password};
			DBModel.InsertApiUser(n_user);
			n_user = DBModel.CheckUser( item => string.Compare(item.UserName, User.UserName, true) == 0 && string.Compare(item.Password, User.Password, false) == 0);

			bool ret = string.Compare(User.Password, User.UserName) == 0;

			return Request.CreateResponse(ret ? HttpStatusCode.OK : HttpStatusCode.Unauthorized, ret ? User : new UserModel());
		}
		
		[HttpPost]
		public HttpResponseMessage EditAccount(UserEditModel User)
		{
			ApiUser n_user = DBModel.CheckUser(item => string.Compare(item.UserName, User.UserName, true) == 0 && string.Compare(item.Password, User.Password, false) == 0);
			if( n_user == null)
				return Request.CreateResponse( HttpStatusCode.NotFound, "User not found");
			n_user.UserName = User.UserName;
			n_user.Password = User.Password;
			n_user.Name = User.Name;
			DBModel.UpdateApiUser(n_user);

			return Request.CreateResponse(HttpStatusCode.OK, n_user);
		}
		#endregion

		#region DBconfig

		[HttpGet]
		public HttpResponseMessage GetDBConnection()
		{
			DBConfigModel result = DalConnect.Instance.GetPACConfig();
			if( result == null)
				return Request.CreateResponse( HttpStatusCode.InternalServerError,"Cannot get DB config");

			return Request.CreateResponse<DBConfigModel>(HttpStatusCode.OK, result);
		}

		[HttpPost]
		public HttpResponseMessage SetDBConnection(DBConfigModel config)
		{
			 bool ret = DalConnect.Instance.EditConnection(config);
			 return ret == true? GetDBConnection() : Request.CreateResponse( HttpStatusCode.Conflict, "Cannot edit data.");
		}
		[HttpPost]
		public HttpResponseMessage TestDBConnection(DBConfigModel config)
		{
			string ret = DalConnect.Instance.TestConnection(config);
			return Request.CreateResponse(HttpStatusCode.OK,ret);
		}
		#endregion

		#region SVR config
		[HttpGet]
		public HttpResponseMessage GetDVRAuthConfig()
		{
			List<ServiceConfig> configs = DBModel.GetServiceConfigs();
			if( configs == null)
				return Request.CreateResponse(HttpStatusCode.NotFound);
			ServiceConfig config = configs.First( item => item.ID == configs.Max( litem => litem.ID));
			if (config == null)
			{
				config = new ServiceConfig{ AuthenticateMode = true, ConverterInterval = DVRRegisterSupports.DVR_ConverterInterval
											, ConverterLimit = DVRRegisterSupports.DVR_ConverterInterval
											, DVRAuthenticate = DVRRegisterSupports.Instance.Default_Authenticate
											, KeepAliveInterval = DVRRegisterSupports.DVR_KeepALive };
				DBModel.InsertServiceConfig( config);
			}

			List<DVRAuth> Auths = new List<DVRAuth>();
			ReadOnlyDictionary<string, string> registertypes = DVRRegisterSupports.Instance.RegisterSupportList;
			IEnumerable<Match> Matchs = DVRRegisterSupports.Instance.ParserDVRRegister(config.DVRAuthenticate);
			char ops = DVRRegisterSupports.Instance.combineOperator( Matchs.First().Groups[ DVRRegisterSupports.Str_Sign].Value);
			IEnumerable<string> matchKeys = Matchs.Select( match => match.Groups[ DVRRegisterSupports.Str_Match].Value);

			
			var maps = 
					   from key in matchKeys
					   join rtype in registertypes
					   on key equals rtype.Key into joingroup
					   from it in joingroup.DefaultIfEmpty()
					   select new { key = key, value = it };

			DVRAuth dvrauth;
			foreach(var item in maps)
			{
				dvrauth = new DVRAuth();
				dvrauth.Key = item.key;
				dvrauth.Name = item.value.Value;
				dvrauth.Checked = true;
				Auths.Add(dvrauth);
				
			}
			var unmap = registertypes.Where( item => !matchKeys.Contains( item.Key));
			foreach (KeyValuePair<string, string> item in unmap)
			{
				dvrauth = new DVRAuth();
				dvrauth.Key = item.Key;
				dvrauth.Name = item.Value;
				dvrauth.Checked = false;
				Auths.Add(dvrauth);
			}
			svrConfigmodels ret = new svrConfigmodels{ ConvertInterval = config.ConverterInterval, Authenticate = config.AuthenticateMode
														, MatchAllinfo = ops == DVRRegisterSupports.Register_Split_item_AND
														, DVRAuths = Auths, ConfigID = config.ID
														, DVRKeepAlive = config.KeepAliveInterval
													};
			return Request.CreateResponse<svrConfigmodels>( HttpStatusCode.OK, ret);

		}

		[HttpPost]
		public HttpResponseMessage SetDVRAuthConfig(svrConfigmodels model)
		{
			string str_auth = string.Empty;
			
			IEnumerable<string> mapKeys = model.DVRAuths.Where( item => item.Checked == true).Select( sel => sel.Key);
			string match = DVRRegisterSupports.Instance.BuildDVRAuth(mapKeys.ToList(),model.MatchAllinfo);
			ServiceConfig cfg =  null;
			if( model.ConfigID > 0)
			{
				cfg = DBModel.GetServiceConfigs( item => item.ID == model.ConfigID).FirstOrDefault();
				
			}
			if( cfg == null)
				cfg = new ServiceConfig();

			cfg.AuthenticateMode = model.Authenticate;
			cfg.ConverterInterval = model.ConvertInterval;
			cfg.ConverterLimit = 0;
			cfg.DVRAuthenticate = match;
			cfg.KeepAliveInterval = model.DVRKeepAlive;
			if( cfg.ID == 0)
				DBModel.InsertServiceConfig(cfg);
			else
				DBModel.UpdateServiceConfig(cfg);
			 return	 GetDVRAuthConfig();
		}

		#endregion

		#region Logs
		[HttpGet]
		[ActionName("Logs")]
		public HttpResponseMessage Logs([ModelBinder(typeof(UriLogFilterBinder))] LogFilter id)
		{
		LogFilter filter = id;
			IQueryable<Log> Logs = DBModel.GetLogs();
			
			if(filter.Date.HasValue)
			{
				DateTime sdate = filter.Date.Value.Date;
				DateTime edae = sdate.AddDays(1).AddMilliseconds(-1);
				Logs = Logs.Where(log => log.DVRDate >= sdate && log.DVRDate <=  edae );
			}

			if( filter.Programset.HasValue)
				Logs = Logs.Where(log => log.ProgramSet == (byte)filter.Programset);
			IOrderedQueryable<Log> orLogs = Logs.OrderByDescending( item => item.DVRDate);

			int nTotalItems = orLogs.Count();
			int skip = (filter.PageNumber - 1) * filter.PageSize;

			IEnumerable<Log> retsultlog = orLogs.Skip(skip);
			retsultlog = retsultlog.Take(filter.PageSize);
			var result = new { TotalItem = nTotalItems, Logs = retsultlog.ToArray() };
			return Request.CreateResponse( HttpStatusCode.OK, result);
		}

		[HttpPost]
		public HttpResponseMessage DeleteLog( Log log)
		{
			DBModel.DeleteWhere<Log>( item => item.ID == log.ID);
			return Request.CreateResponse(HttpStatusCode.OK, log);

		}
		[HttpPost]
		public HttpResponseMessage DeleteLogs([FromBody] LogFilter filter)
		{
			
			DateTime sdate = filter.Date.Value;
			DateTime edae = sdate.AddDays(1).AddMilliseconds(-1);
			if (filter.Programset.HasValue && filter.Programset > 0)
				DBModel.DeleteWhere<Log>(item => item.DVRDate >= sdate && item.DVRDate <= edae && item.ProgramSet == (byte)filter.Programset);
			else
				DBModel.DeleteWhere<Log>(item => item.DVRDate >= sdate && item.DVRDate <= edae);
			return Logs(filter);

		}
		#endregion

		#region DVR Info
		[HttpGet]
		public HttpResponseMessage GetDVRs()
		{
			if( PACDMModel == null)
			{
				return Request.CreateResponse( HttpStatusCode.NoContent,"Can not connect to PACDM database.");
			}
			var DBDVRs = PACDMModel.Query<tDVRAddressBook>().Select(i => new LiteDVRAddressbook { KDVR = i.KDVR, DVRGuid = i.DVRGuid, ServerIP = i.ServerIP, DVRAlias = i.DVRAlias, FirstAccess = i.FirstAccess, CMSMode = i.CMSMode, HaspLicense = i.HaspLicense }).ToList();

			IEnumerable<DVRInfo> dvrInfo = DBModel.GetData<DVRInfo>().ToList();
			IEnumerable<DVRDetail> dvrDetail = DBModel.GetData<DVRDetail>().ToList();
			var infoDVR = from info in dvrInfo
					join dvr in DBDVRs
					on info.KDVR equals dvr.KDVR into infodvr
					from map in infodvr.DefaultIfEmpty()
					select new{ info = info, dvr = map == null? new LiteDVRAddressbook() : map};
			var dvrinfo = from dvr in DBDVRs
						join info in dvrInfo
						on dvr.KDVR equals info.KDVR into gdvrinfo
						from map in gdvrinfo.DefaultIfEmpty()
						select new{ info= map == null? new DVRInfo() : map, dvr = dvr  };
			var alldvr = infoDVR.Union( dvrinfo);


			return Request.CreateResponse( HttpStatusCode.OK, alldvr.ToList());
		}
		#endregion

		// GET api/<controller>
		public IEnumerable<string> Get()
		{
			return new string[] { "value1", "value2" };
		}

		// GET api/<controller>/5
		public string Get(int id)
		{
			return "value";
		}

		// POST api/<controller>
		public void Post([FromBody]string value)
		{
		}

		// PUT api/<controller>/5
		public void Put(int id, [FromBody]string value)
		{
		}

		// DELETE api/<controller>/5
		public void Delete(int id)
		{
		}

	}

	internal class DalConnect : SingletonClassBase<DalConnect>
	{
		readonly object locker = new object();

		public DBConfigModel GetPACConfig()
		{
			SqlConnectionStringBuilder sqlcon = null;
			lock(locker)
			{
				EntityConnectionStringBuilder conbuider = new EntityConnectionStringBuilder(AppSettings.AppSettings.Instance.PACDMConnection);
				sqlcon = new SqlConnectionStringBuilder(conbuider.ProviderConnectionString);
			}
			if( sqlcon == null)
				return null;
			return new DBConfigModel { DBName = sqlcon.InitialCatalog, Password = sqlcon.Password, Server = sqlcon.DataSource, UserID = sqlcon.UserID, Trusted = sqlcon.IntegratedSecurity };
			
		}

		public string TestConnection(DBConfigModel model)
		{
			if (model == null)
				return "Invalid data.";
			SqlConnectionStringBuilder sqlcon = new SqlConnectionStringBuilder();

			SqlConnection con = null;
			try
			{
				sqlcon.DataSource = model.Server;
				sqlcon.InitialCatalog = model.DBName;
				sqlcon.Password = model.Password;
				sqlcon.UserID = model.UserID;
				sqlcon.IntegratedSecurity = model.Trusted;

				con = new SqlConnection(sqlcon.ConnectionString);
				con.Open();
				if (con.State == ConnectionState.Open)
					return "Test connection succeeded";
				return string.Format("Login failed for user '{0}'", model.UserID);
			}
			catch (Exception ex)
			{
				return ex.Message;
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

		public bool EditConnection(DBConfigModel model)
		{
			bool ret = false;
			lock(locker)
			{
				try
				{
					EntityConnectionStringBuilder conbuider = new EntityConnectionStringBuilder(AppSettings.AppSettings.Instance.PACDM_Model);
					SqlConnectionStringBuilder sqlcon = new SqlConnectionStringBuilder(conbuider.ProviderConnectionString);
					sqlcon.InitialCatalog = model.DBName;
					sqlcon.Password = model.Password;
					sqlcon.DataSource = model.Server;
					sqlcon.UserID = model.UserID;
					sqlcon.IntegratedSecurity = model.Trusted;
					conbuider.ProviderConnectionString = sqlcon.ConnectionString;

					var configuration = WebConfigurationManager.OpenWebConfiguration("~");
					ConnectionStringSettings setting = configuration.ConnectionStrings.ConnectionStrings [AppSettings.AppSettings.Instance.PACDM_Model];
					setting.ConnectionString = conbuider.ConnectionString;
					configuration.Save();
					ret = true;
				}
				catch (Exception)
				{
					ret = false;
				}
			}
			return ret;
		}
	}
}
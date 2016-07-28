using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using PACDMModel;
using System.Data.SqlClient;
using Extensions.Linq;

namespace CMSWebApi.DataServices
{
	public abstract class ServiceBase
	{
		protected class Defines
		{
			public class ConstNormalizes
			{
                public const string Succecss = "Succecss";
                public const string Error = "Error";
            }
			public class ALert
			{
				public const string KDVR = "KDVR";
				public const string KAlertType = "KAlertType";
				public const string Time = "Time";
				public const string TotalAlert = "TotalAlert";
				public const string TimeZone = "TimeZone";
				public const string BeginDate = "BeginDate";
				public const string EndDate = "EndDate";

			}
			public class DVRAddressBook
			{
				public const string DVRGuid = "DVRGuid";
				public const string ServerID = "ServerID";
				public const string ServerIP = "ServerIP";
				public const string Online = "Online";
				public const string PublicServerIP = "PublicServerIP";
				public const string TotalDiskSize = "TotalDiskSize";
				public const string FreeDiskSize = "FreeDiskSize";
				public const string DVRAlias = "DVRAlias";
				public const string EnableActivation = "EnableActivation";
				public const string ActivationDate = "ActivationDate";
				public const string ExpirationDate = "ExpirationDate";
				public const string RecordingDay = "RecordingDay";
				public const string FirstAccess = "FirstAccess";
				public const string KLocation = "KLocation";
				public const string TimeDisConnect = "TimeDisConnect";
				public const string DisConnectReason = "DisConnectReason";
				public const string CMSMode = "CMSMode";
				public const string LastConnectTime = "LastConnectTime";
				public const string CurConnectTime = "CurConnectTime";
				public const string KGroup = "KGroup";
				public const string KDVRVersion = "KDVRVersion";
				public const string HaspLicense = "HaspLicense";
			}
		}

		protected class SQLFunctions
		{
			/// <summary>
			/// get list DVR by user ID
			/// </summary>
			public const string Func_CMSWebReport_Schedule = "SELECT * FROM Func_CMSWebReport_Schedule(@{0})";
			public const string Func_CMSWeb_DVRFollowUSer = "SELECT * FROM Func_CMSWeb_DVRFollowUSer(@{0})";
			public const string Func_CMSWeb_DVRFollowUSer_Filter = "SELECT * FROM Func_CMSWeb_DVRFollowUSer(@{0}) WHERE {1} IN ({2})";
			public const string Func_BAM_Normalize_IOPC_Count = "SELECT * FROM Func_BAM_Normalize_IOPC_Count(@{0}, @{1}, @{2}, @{3}, @{4})";
			public const string Func_Fact_IOPC_Periodic_Hourly_Traffic = "SELECT * FROM Func_Fact_IOPC_Periodic_Hourly_Traffic(@{0}, @{1}, @{2}, @{3}, @{4})";
			public const string Func_Count_Exception_Trans = "SELECT * FROM Func_Count_Exception_Trans(@{0}, @{1}, @{2}, @{3})";
			public const string Func_DVR_Offline = "SELECT * FROM Func_DVR_Offline(@{0}, @{1}, @{2}, @{3});";
			public const string Func_CMSWeb_Cache_ALert = "Select * FROM CMSWeb_Cache_ALert(@{0}, @{1})";
			public const string Func_CMSWeb_Cache_POS_Periodic_Hourly_Traffic = "Select * FROM CMSWeb_Cache_POS_Periodic_Hourly_Traffic(@{0}, @{1})";
			public const string Func_CMSWeb_Cache_IOPC_Periodic_Hourly_Traffic = "Select * FROM CMSWeb_Cache_IOPC_Periodic_Hourly_Traffic(@{0}, @{1})";
			public const string Func_Fact_IOPC_Periodic_Daily_Traffic = "SELECT * FROM Func_Fact_IOPC_Periodic_Daily_Traffic(@{0}, @{1}, @{2})";
			public const string Func_DVR_HasNormalize = "SELECT * FROM Func_DVR_HasNormalize(@{0}, @{1}, @{2})";
			public const string Func_DVR_Offline_ByHours = "SELECT * FROM Func_DVR_Offline(@{0}, @{1}, @{2}, @{3}) WHERE [Minutes] > {4};";
			//public const string Func_BAM_Get_DashBoard_ForeCast = "SELECT * FROM Func_BAM_Get_DashBoard_ForeCast(@{0}, @{1}, @{2}, @{3});";
			public const string Func_Alerts_SummaryBySites = "SELECT * FROM Func_Alerts_SummaryBySites(@{0},@{1},@{2},@{3})";
			public const string Func_Fact_POS_Periodic_Daily_Transact = "SELECT * FROM Func_Fact_POS_Periodic_Daily_Transact(@{0}, @{1}, @{2})";
			public const string Func_Fact_IOPC_Periodic_Daily_Traffic_Channels = "SELECT * FROM Func_Fact_IOPC_Periodic_Daily_Traffic_Channels(@{0}, @{1}, @{2})";
			public const string Func_Fact_IOPC_Periodic_Hourly_Traffic_Channels = "SELECT * FROM Func_Fact_IOPC_Periodic_Hourly_Traffic_Channels(@{0}, @{1})";
            public const string Func_ChannelsByKDVR = "select * from Func_ChannelsByKDVR({0})";
			public const string Func_BAM_TrafficCountReportMonthly = "SELECT * FROM Func_BAM_TrafficCountReportMonthly(@{0}, @{1}, @{2}, @{3})";
			public const string Func_BAM_TrafficCountReportHourly = "SELECT * FROM Func_BAM_TrafficCountReportHourly(@{0}, @{1}, @{2}, @{3})";
			public const string Func_BAM_DriveThroughMonthly = "SELECT * FROM Func_BAM_DriveThroughMonthly(@{0}, @{1}, @{2})";
			public const string Func_BAM_DriveThroughHourly = "SELECT * FROM Func_BAM_DriveThroughHourly(@{0}, @{1}, @{2})";
            public const string Func_BAM_TrueTraffic_Opportunity = "SELECT * FROM Func_BAM_TrueTraffic_Opportunity(@{0}, @{1}, @{2})";
            public const string Func_BAM_LaborHourlyWorkingHour = "SELECT * FROM Func_BAM_LaborHourlyWorkingHour(@{0}, @{1}, @{2})";
            public const string Func_BAM_LaborHourlyMinSecsWorkingHour = "SELECT * FROM Func_BAM_LaborHourlyMinSecsWorkingHour(@{0}, @{1}, @{2})";
			public const string Func_Exception_Total_FlagWeight = "SELECT * FROM Func_Exception_Total_FlagWeight(@{0}, @{1})";

		}
		protected class SQLProceduces
		{
			public const string TrafficCountRegionInQueue = "EXEC TrafficCountRegionInQueue @{0}, @{1}";
			public const string Proc_DashBoard_Conversion = "EXEC Proc_DashBoard_Conversion @{0}, @{1}, @{2}";
			public const string Proc_DashBoard_Conversion_Hourly = "EXEC Proc_DashBoard_Conversion_Hourly @{0}, @{1}, @{2}";
			public const string Proc_DashBoard_Traffic_ForeCast = "EXEC Proc_DashBoard_Traffic_ForeCast_Period @{0}, @{1}, @{2}";
			public const string Proc_DashBoard_Traffic_ForeCast_5Weeks = "EXEC Proc_DashBoard_Traffic_ForeCast_5Weeks @{0}, @{1}, @{2}, @{3}";
			public const string Proc_DashBoard_Traffic_ForeCast_Hourly = "EXEC Proc_DashBoard_Traffic_ForeCast_Period_Hourly @{0}, @{1}, @{2}";
			public const string Proc_DashBoard_Traffic_ForeCast_5Weeks_Hourly = "EXEC Proc_DashBoard_Traffic_ForeCast_5Weeks_Hourly @{0}, @{1}, @{2}, @{3}";
			public const string Proc_DashBoard_Channel_EnableTrafficCount = "EXEC Proc_DashBoard_Channel_EnableTrafficCount @{0}";
			public const string Proc_BAM_Get_DashBoard_ForeCast_Period = "EXEC Proc_BAM_Get_DashBoard_ForeCast_Period @{0}, @{1}, @{2}";
			public const string Proc_BAM_Get_DashBoard_ForeCast_5Weeks = "EXEC Proc_BAM_Get_DashBoard_ForeCast_5Weeks @{0}, @{1}, @{2}, @{3}";
			public const string Proc_BAM_Get_Header_Stores_Count = "EXEC Proc_BAM_Get_Header_Stores_Count @{0}, @{1}";
			public const string sp_DeleteTransact = "EXEC sp_DeleteTransact @{0}";
			public const string Proc_BAM_QueueByRegionIndex = "EXEC Proc_BAM_QueueByRegionIndex @{0}, @{1}, @{2}";
			public const string Proc_BAM_DashBoard_LaborHour_ForeCast_5Weeks = "EXEC Proc_BAM_DashBoard_LaborHour_ForeCast_5Weeks @{0}, @{1}, @{2}, @{3}";
			public const string Proc_BAM_DashBoard_LaborHour_ForeCast_Period = "EXEC Proc_BAM_DashBoard_LaborHour_ForeCast_Period @{0}, @{1}, @{2}";
			public const string Proc_Exception_GetReport = "EXEC Proc_Exception_GetReport @{0}, @{1}, @{2}, @{3}, @{4}, @{5}, @{6}, @{7}, @{8}, @{9}, @{10}, @{11}, @{12}, @{13}, @{14}, @{15}, @{16}, @{17}";
			public const string Proc_Exception_QuickSearch = "EXEC Proc_Exception_QuickSearch @{0}, @{1}, @{2}, @{3}, @{4}, @{5}, @{6}, @{7}, @{8}, @{9}, @{10}, @{11}, @{12}, @{13}, @{14}, @{15}, @{16}, @{17}, @{18}";
			public const string Proc_Exception_WeekAtAGlane_HeaderCount = "EXEC Proc_Exception_WeekAtAGlane_HeaderCount @{0}, @{1}, @{2}";
			public const string Proc_Exception_TransWOC = "EXEC Proc_Exception_TransWOC @{0}, @{1}, @{2}";
			public const string Proc_Exception_CustsWOT = "EXEC Proc_Exception_CustsWOT @{0}, @{1}, @{2}";
			public const string Proc_Exception_CarsWOT = "EXEC Proc_Exception_CarsWOT @{0}, @{1}, @{2}";
		}
		internal PACDMModel.Model.IResposity DBModel { get; private set; }
		protected ServiceBase(){}

		public ServiceBase(PACDMModel.Model.IResposity dbModel)
		{
			DBModel = dbModel;
		}

		protected SqlParameter SQLFunctionParamater(string name, object value)
		{
			SqlParameter p = new SqlParameter(name,value);
			return p;

		}

		protected Expression MemberExpression(ParameterExpression parg, string field)
		{
			return  Expression.Property(parg, field);
		}
		
		protected Expression ValueExpression<T>(ParameterExpression parg, T  value)
		{
			Expression exp = Expression.Constant(value);
			return  Expression.Convert(exp, typeof(T));
		}
		
		protected BinaryExpression NotEqual<T>(ParameterExpression parg, string field, T value)
		{
			Expression property = MemberExpression(parg, field);
			Expression val = ValueExpression<T>(parg, value);
			return Expression.NotEqual(property, val);
		}
		
		protected BinaryExpression Equal<T>(ParameterExpression parg, string field, T value)
		{
			Expression property = MemberExpression(parg, field) ;
			Expression val = ValueExpression<T>(parg, value);

			return Expression.Equal(property, val);
		}

		protected BinaryExpression GreaterThan<T>(ParameterExpression parg, string field, T value)
		{
			Expression property = MemberExpression(parg, field);
			Expression val = ValueExpression<T>(parg, value);
			  
			return Expression.GreaterThan(property, val);
		}

		protected BinaryExpression LessThan<T>(ParameterExpression parg, string field, T value)
		{
			Expression property = MemberExpression(parg, field);
			Expression val = ValueExpression<T>(parg, value);
			return Expression.LessThan(property, val);
		}

		protected BinaryExpression LessThanOrEqual<T>(ParameterExpression parg, string field, T value)
		{
			Expression property = MemberExpression(parg, field);
			Expression val = ValueExpression<T>(parg, value);
			return Expression.LessThanOrEqual(property, val);
		}
		
		protected BinaryExpression GreaterThanOrEqual<T>(ParameterExpression parg, string field, T value)
		{
			Expression property = MemberExpression(parg, field);
			Expression val = ValueExpression<T>(parg, value);
			return Expression.GreaterThanOrEqual(property, val);
		}

		 public IQueryable<Tout> Query<Tin,Tout>(Expression<Func<Tin, bool>> filter, Expression<Func<Tin, Tout>> selector, string[] includes = null)
		  where  Tin: class
		//where Tout : class
		 {
			IQueryable<Tin> query = DBModel.Query<Tin>( filter, includes);
			return query.Select<Tin, Tout>(selector);
		}

		 public IQueryable<Tout> QueryNoTrack<Tin, Tout>(Expression<Func<Tin, bool>> filter, Expression<Func<Tin, Tout>> selector, string [] includes = null)
		   where Tin : class
		 //where Tout : class
		 {
			 IQueryable<Tin> query = DBModel.QueryNoTrack<Tin>(filter, includes);
			 return query.Select<Tin, Tout>(selector);
		 }

		 public Tout FirstOrDefault<Tin, Tout>(Expression<Func<Tin, bool>> filter, Expression<Func<Tin, Tout>> selector, string [] includes = null)
		   where Tin : class
		 //where Tout : class
		 {
			 Tin query = DBModel.FirstOrDefault<Tin>(filter, includes);
			 if( query == null)
				return default(Tout);

			 return selector.Compile().Invoke(query);
		 }

		public void Includes< Tin, TProperty>( Tin entity, Expression<Func< Tin, ICollection<TProperty>>> predicate) where Tin: class where TProperty: class 
		{
			DBModel.Include<Tin, TProperty>( entity, predicate);
		}

		public IEnumerable<string> ChildProperty(Type entity, params Type [] collections)
		{
			
			if ( entity == null || collections == null || !collections.Any())
				return null;
			List<string> result = new List<string>();
			Type parent = entity;
			Type child = null;
			string propertyname = string.Empty;
			foreach (Type type in collections)
			{
				child = type;
				propertyname = ChildProperty(parent, child, false);
				if (string.IsNullOrEmpty(propertyname))
					break;
				result.Add(propertyname);
				parent = child;
			}
			return result;
		}

		public IEnumerable<string> ChildProperty<Tin>(Tin entity, params Type [] collections)
		{
			Type parent = typeof(Tin);
			return ChildProperty(parent, collections);
		}
		
		public KeyValuePair<string,Type> ChildPropertyMap(Type entity, Type Property)
		{
			if( Property == null)
				return new  KeyValuePair<string,Type>() ;

			IEnumerable<KeyValuePair<string, Type>> childs = ChildPropertyMaps(entity);
			return childs == null? new  KeyValuePair<string,Type>() : childs.FirstOrDefault( item => item.Value.FullName == Property.FullName && item.Key== Property.Name);
		}
	
		public IEnumerable<KeyValuePair<string, Type>> ChildPropertyMaps(Type entity)
		{
			return ChildPropertyMaps(entity, true);

			//if (entity == null)
			//	return null;
			//string org_NS = entity.Namespace;
			//IEnumerable<PropertyInfo> pinfos = entity.GetProperties().Where(item => (item.GetMethod == null ? false : item.GetMethod.IsVirtual) == true && item.PropertyType.IsGenericType);
			//return pinfos.Where(item => item.PropertyType.GenericTypeArguments.First().Namespace == org_NS).Select(p =>new KeyValuePair<string, Type>(p.Name, p.PropertyType.GenericTypeArguments.First()));
		}

		public string ChildProperty<TParent, TChild>() where TParent :class where TChild : class
		{
			return ChildProperty( typeof(TParent), typeof(TChild));
		}

		public string ChildProperty(Type entity, Type Property)
		{
			KeyValuePair<string, Type> p = ChildPropertyMap(entity, Property);
			return p.Key;
		}
		
		public IEnumerable<string> ChildProperties( Type entity)
		{
			IEnumerable<KeyValuePair<string, Type>> pinfos = ChildPropertyMaps(entity);
			return pinfos == null? null : pinfos.Select( item => item.Key);
			
		}
		
		protected IEnumerable<string> GetProperties( Type type)
		{
			if( type == null || !type.IsClass)
				return new List<string>();

			 PropertyInfo[]pinfos = type.GetProperties( BindingFlags.Public | BindingFlags.Instance);
			 return pinfos.Select( p => p.Name);
		}
	
		protected IEnumerable<string> GetProperties<T>() where T : class
		{
			return GetProperties(typeof(T));
		}

		protected string [] ParameterNames(IEnumerable<SqlParameter> prams)
		{
			return (prams != null && prams.Any())? prams.Select( p => p.ParameterName).ToArray() : null;
		}
		
		protected string Format_SqlCommand(string sqlcmd, IEnumerable<SqlParameter> pram)
		{
			return string.Format(sqlcmd, ParameterNames(pram));
		}
		
		public void ModifyDataRelation<TEntity, TProperty, TK>(TEntity dbsite, IEnumerable<TProperty> current, IEnumerable<TProperty> news, Func<TProperty, TK> keyselector, Expression<Func<TEntity, object>> properties) where TProperty : class where TEntity : class
		{
			if (dbsite == null)
				return;
			IEnumerable<TProperty> deletes = null;
			IEnumerable<TProperty> adds = null;
			if (current != null && news != null)
			{
				var maps = current.FullOuterJoin(news, c => keyselector(c), n => keyselector(n), (c, n, k) => new { delele = c, add = n, key = k });
				deletes = maps.Where(it => it.add == null).Select(sel => sel.delele);
				adds = maps.Where(it => it.delele == null).Select(sel => sel.add);
			}
			else
			{
				adds = news;
				deletes = current;
			}
			if (deletes != null && deletes.Any())
				DBModel.DeleteItemRelation<TEntity, TProperty>(dbsite, properties, deletes.ToArray());
			if (adds != null && adds.Any())
				DBModel.AddItemRelation<TEntity, TProperty>(dbsite, properties, adds.ToArray());
		}


		private KeyValuePair<string, Type> ChildPropertyMap(Type entity, Type Property, bool iscollectiononly)
		{
			if (Property == null)
				return new KeyValuePair<string, Type>();

			IEnumerable<KeyValuePair<string, Type>> childs = ChildPropertyMaps(entity, iscollectiononly);
			return childs == null ? new KeyValuePair<string, Type>() : childs.FirstOrDefault(item => item.Value.FullName == Property.FullName && item.Key == Property.Name);
		}

		public string ChildProperty(Type entity, Type Property, bool iscollectiononly)
		{
			KeyValuePair<string, Type> p = ChildPropertyMap(entity, Property,iscollectiononly);
			return p.Key;
		}

		private IEnumerable<KeyValuePair<string, Type>> ChildPropertyMaps(Type entity, bool iscollectiononly)
		{
			if (entity == null)
				return null;
			string org_NS = entity.Namespace;
			IEnumerable<PropertyInfo> pinfos = null;
			if( iscollectiononly)
			{
				pinfos = entity.GetProperties().Where(item => (item.GetMethod == null ? false : item.GetMethod.IsVirtual) == true && item.PropertyType.IsGenericType);
				return pinfos.Where(item => item.PropertyType.GenericTypeArguments.First().Namespace == org_NS).Select(p => new KeyValuePair<string, Type>(p.Name, p.PropertyType.GenericTypeArguments.First()));
			}
			else
			{
				pinfos = entity.GetProperties().Where(item => (item.GetMethod == null ? false : true) == true);
				return pinfos.Where(item => (item.PropertyType.IsGenericType? item.PropertyType.GenericTypeArguments.First().Namespace : item.PropertyType.Namespace ) == org_NS).Select(p => new KeyValuePair<string, Type>(p.Name, p.PropertyType.IsGenericType? p.PropertyType.GenericTypeArguments.First() : p.PropertyType));
			}
		}

	}
}


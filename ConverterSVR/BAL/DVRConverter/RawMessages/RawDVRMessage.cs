using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using PACDMModel.Model;
using System.Globalization;
using SVRDatabase;
using ConvertMessage;
using System.Reflection;
using Commons;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Drawing;
using Extensions;
using ConverterSVR.BAL.DVRConverter.RawMessages.AlertHandlers;

namespace ConverterSVR.BAL.DVRConverter
{
	public abstract class RawDVRConfig<TBody> : IDisposable, IDVRMsg where TBody : class
	{
		protected const int Time_Loop_Delay = 5;
		protected const int MAX_CHANNEL = 64;
		protected const int CMS_MODE_STANDARD = 255;
		protected const int RAWIMAGE_TIMEOUT = 300;//720; //Refresh DVR raw image every 300 minutes = 5 hours

		protected PACDMModel.PACDMDB db;
		protected MessageDVRInfo dvrInfo;
		protected tDVRAddressBook DVRAdressBook;
		protected SVRManager LogDB;
		protected string ReconnectDvrGuid = null;
		protected bool ReconnectFlag = false;
		public readonly int[] _notSend = { 3, 19, 20, 21 };

		public int VersionProSchedule = 85;
		public int VersionPro3_3 = 92;
		public readonly int VersionForBandwidth = 71;

		[XmlElement(MessageDefines.STR_Header)]
		public RawMsgHeader msgHeader { get; set; }

		[XmlElement(MessageDefines.STR_Body)]
		public TBody msgBody { get; set; }


		ERROR_CODE msgLogId = ERROR_CODE.DVR_ERR_PARSE_MSG;
		protected bool isCreated = false;

		public ERROR_CODE MsgLogCode
		{
			get { return msgLogId; }
			set { msgLogId = value; }
		}

		public bool IsCreated
		{
			get { return isCreated; }
		}

		protected RawDVRConfig()
		{
			
		}

		public virtual void SetEvnVars(PACDMModel.PACDMDB pacDB, SVRManager Logdb, MessageDVRInfo dvrinfo)
		{
			db = pacDB;
			LogDB = Logdb;
			dvrInfo = dvrinfo;
			DVRAdressBook = db.FirstOrDefault<tDVRAddressBook>(item => item.KDVR == dvrinfo.KDVR);
			
		}

		#region PRO_VERSION
		public enum PRO_VERSION
		{
			NONE_VERSION = 0,
			PRO_1405 = 1,
			PRO_1500,
			PRO_1512,
			PRO_1520,
			PRO_1530,
			PRO_1600,
			PRO_2000,
			PRO_2100,
			PRO_2200,
			PRO_2300,
			PRO_3000,
			PRO_3100,
			PRO_3200,
			PRO_VERSION_COUNT
		}
		#endregion
		#region PRO_PRODUCT
		public enum PRO_PRODUCT
		{
			NONE_PRODUCT = 0,
			REG = 1,
			IP_PRO,
			OCC,
			LITE,
			VA,
			IP_VA,
			RECORDINGONLY_SERVER,
			PRO_PRODUCT_COUNT
		}
		#endregion


		protected MemberExpression GetMemberExpression<T>(Expression<Func<T, object>> expr)
		{
			MemberExpression me;
			switch (expr.Body.NodeType)
			{
				case ExpressionType.Convert:
				case ExpressionType.ConvertChecked:
					var ue = expr.Body as UnaryExpression;
					me = ((ue != null) ? ue.Operand : null) as MemberExpression;
					break;
				default:
					me = expr.Body as MemberExpression;
					break;
			}

			while (me != null)
			{
				if (me.Expression as MemberExpression != null)
					me = me.Expression as MemberExpression;
				else 
					break;
			}

			return me;
		}

		protected MemberExpression GetMemberExpression<T, TProperty>(Expression<Func<T, TProperty>> expr)
		{
			MemberExpression me;
			switch (expr.Body.NodeType)
			{
				case ExpressionType.Convert:
				case ExpressionType.ConvertChecked:
					var ue = expr.Body as UnaryExpression;
					me = ((ue != null) ? ue.Operand : null) as MemberExpression;
					break;
				default:
					me = expr.Body as MemberExpression;
					break;
			}

			while (me != null)
			{
				if (me.Expression as MemberExpression != null)
					me = me.Expression as MemberExpression;
				else
					break;
			}

			return me;
		}

		protected TProperty GetPropertyValue<TSource, TProperty>(TSource source, Expression<Func<TSource, TProperty>> propertyLambda)
		{
			Type type = typeof(TSource);

			MemberExpression member = GetMemberExpression<TSource, TProperty>(propertyLambda);//propertyLambda.Body as MemberExpression;
			if (member == null)
				throw new ArgumentException(string.Format(
					"Expression '{0}' refers to a method, not a property.",
					propertyLambda.ToString()));

			PropertyInfo propInfo = member.Member as PropertyInfo;
			if (propInfo == null)
				throw new ArgumentException(string.Format(
					"Expression '{0}' refers to a field, not a property.",
					propertyLambda.ToString()));

			if (type != propInfo.ReflectedType &&
				!type.IsSubclassOf(propInfo.ReflectedType))
				throw new ArgumentException(string.Format(
					"Expresion '{0}' refers to a property that is not from type {1}.",
					propertyLambda.ToString(),
					type));

			return propInfo == null ? default(TProperty) : (TProperty)propInfo.GetValue(source, null);
		}

		protected void SetPropertyValue<T>(T target, Expression<Func<T, object>> memberLamda, object value) where T : class
		{
			var memberSelectorExpression = GetMemberExpression<T>(memberLamda); //memberLamda.Body as MemberExpression;
			if (memberSelectorExpression != null)
			{
				var property = memberSelectorExpression.Member as PropertyInfo;
				if (property != null)
				{
					property.SetValue(target, value, null);
				}
			}
		}
		private void SetPropertyValue<T>(T target, List<Expression<Func<T, object>>> expr , List<object> values) where T : class
		{
			for(int i = 0; i < values.Count; i++)
			{
				SetPropertyValue<T>(target, expr[i], values[i]);
			}

			
		}
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="tEntity">type of Entity</typeparam>
		/// <typeparam name="tinfo">type of message data object</typeparam>
		/// <param name="dvrItems">list of entity</param>
		/// <param name="infoItem">list of items</param>
		/// <param name="filter_update">all entity items that match with filter_update will be updated to DB</param>
		/// <param name="compare_update"></param>

		/// <returns></returns>
		protected bool UpdateDBData<tEntity, tinfo, tdbkey, tinfokey>(ICollection<tEntity> dvrItems,
													List<tinfo> infoItems, Func<tEntity, tinfo, bool> filter_update,
													Func<tEntity, tinfo, bool> compare_update,
													Expression<Func<tEntity, object>> updatedata, object value,	//addition data need to update.
													Expression<Func<tEntity, tdbkey>> db_key,
													Expression<Func<tinfo, tinfokey>> info_key
													)
			where tEntity : class
			where tinfo : class
		{
			 return UpdateDBData(dvrItems, infoItems, filter_update, compare_update, new List<Expression<Func<tEntity,object>>>{updatedata}, new List<object>{value}, db_key, info_key);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="tEntity">type of Entity</typeparam>
		/// <typeparam name="tinfo">type of message data object</typeparam>
		/// <param name="dvrItems">list of entity</param>
		/// <param name="infoItem">list of items</param>
		/// <param name="filter_update">all entity items that match with filter_update will be updated to DB</param>
		/// <param name="compare_update"></param>
		
		/// <returns></returns>
		protected bool UpdateDBData<tEntity, tinfo, tdbkey, tinfokey >(ICollection<tEntity> dvrItems, 
													List<tinfo> infoItems, Func<tEntity, tinfo, bool> filter_update, 
													Func<tEntity, tinfo, bool> compare_update,
													List<Expression<Func<tEntity, object>>> updatedata, List<object> value,	//addition data need to update.
													Expression<Func<tEntity, tdbkey>> db_key,
													Expression<Func<tinfo, tinfokey>> info_key
													)
			where tEntity : class
			where tinfo : class
		{
			bool ret = false;
			if( dvrItems == null)
				dvrItems = new List<tEntity>();
			if (infoItems == null)// || infoItems.Count == 0)
				return false;
				
			var updates = from dvritem in dvrItems
						  from infoitem in infoItems
						  where filter_update.Invoke(dvritem, infoitem) == true
						  select new { DVR = dvritem, Info = infoitem };
			IMessageEntity<tEntity> icompare = null;
			tEntity dbitem = null;
			tinfo iitem = null;
			List<tdbkey> dbkeys = new List<tdbkey>();
			List<tinfokey> infokeys = new List<tinfokey>();
			//update data
			foreach (var item in updates)
			{
				dbitem = item.DVR;
				iitem = item.Info;
				dbkeys.Add( GetPropertyValue<tEntity,tdbkey>( dbitem , db_key) );
				infokeys.Add( GetPropertyValue<tinfo,tinfokey>( iitem , info_key)  );
				icompare = item.Info as IMessageEntity<tEntity>;
				if (icompare == null)
					continue;
				if (icompare.Equal(dbitem))
					continue;
				if ( compare_update != null && compare_update.Invoke(dbitem, item.Info) == true )
					continue;

				icompare.SetEntity(ref dbitem);

				if(updatedata != null)
					SetPropertyValue<tEntity>(dbitem, updatedata, value);

				db.Update<tEntity>(dbitem);
				ret = true;
				System.Threading.Thread.Sleep(Time_Loop_Delay);
			}
			//delete data
			var deletes = dvrItems.Where( item => !dbkeys.Contains( GetPropertyValue<tEntity, tdbkey>(item, db_key) ) ).ToList();
			foreach( tEntity del in deletes)
			{
				db.Delete<tEntity>(del);
				ret = true;
				System.Threading.Thread.Sleep(Time_Loop_Delay);
			}
			var addnews = infoItems.Where(item => !infokeys.Contains(GetPropertyValue<tinfo, tinfokey>(item, info_key)));
			foreach (tinfo nitem in addnews)
			{
				icompare = nitem as IMessageEntity<tEntity>;
				if (icompare == null)
					continue;
				dbitem = null;
				icompare.SetEntity( ref dbitem);
				if (updatedata != null)
					SetPropertyValue<tEntity>(dbitem, updatedata, value);
				db.Insert<tEntity>(dbitem);
				ret = true;
				System.Threading.Thread.Sleep(Time_Loop_Delay);

			}
			return ret;
		}

		protected bool UpdateDBData<tEntity, tinfo>(ICollection<tEntity> dvrItems,
													List<tinfo> infoItems, Func<tEntity, tinfo, bool> filter_update,
													Func<tEntity, tinfo, bool> compare_update,
													Expression<Func<tEntity, object>> updatedata, object value//addition data need to update.
													)
			where tEntity : class
			where tinfo : class
		{
			return	UpdateDBData<tEntity, tinfo>( dvrItems, infoItems, filter_update, compare_update, new List<Expression<Func<tEntity,object>>>{ updatedata}, new List<object>{ value});
		}
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="tEntity">type of Entity</typeparam>
		/// <typeparam name="tinfo">type of message data object</typeparam>
		/// <param name="dvrItems">list of entity</param>
		/// <param name="infoItem">list of items</param>
		/// <param name="filter_update">all entity items that match with filter_update will be updated to DB</param>
		/// <param name="compare_update"></param>

		/// <returns></returns>
		protected bool UpdateDBData<tEntity, tinfo>(ICollection<tEntity> dvrItems,
													List<tinfo> infoItems, Func<tEntity, tinfo, bool> filter_update,
													Func<tEntity, tinfo, bool> compare_update,
													List<Expression<Func<tEntity, object>>> updatedata, List<object> value//addition data need to update.
													)
			where tEntity : class
			where tinfo : class
		{
			bool ret = false;
			if (dvrItems == null)
				dvrItems = new List<tEntity>();
			if (infoItems == null || infoItems.Count == 0)
				return false;

			var updates = from dvritem in dvrItems
						  from infoitem in infoItems
						  where filter_update.Invoke(dvritem, infoitem) == true
						  select new { DVR = dvritem, Info = infoitem };
			IMessageEntity<tEntity> icompare = null;
			tEntity dbitem = null;
			tinfo iitem = null;
			//update data
			foreach (var item in updates)
			{
				dbitem = item.DVR;
				iitem = item.Info;

				icompare = item.Info as IMessageEntity<tEntity>;
				if (icompare == null)
					continue;
				if (icompare.Equal(dbitem))
					continue;
				if ( compare_update != null && compare_update.Invoke(dbitem, item.Info) == true )
					continue;
				icompare.SetEntity(ref dbitem);

				if (updatedata != null)
					SetPropertyValue<tEntity>(dbitem, updatedata, value);

				db.Update<tEntity>(dbitem);
				ret = true;
				System.Threading.Thread.Sleep(Time_Loop_Delay);
			}
			//delete data
			var deletes = dvrItems.Except( updates.Select(item => item.DVR)).ToList();
			foreach (tEntity del in deletes)
			{
				db.Delete<tEntity>(del);
				ret = true;
				System.Threading.Thread.Sleep(Time_Loop_Delay);
			}
			var addnews = infoItems.Except(updates.Select(item => item.Info));
			foreach (tinfo nitem in addnews)
			{
				icompare = nitem as IMessageEntity<tEntity>;
				if (icompare == null)
					continue;
				dbitem = null;
				icompare.SetEntity(ref dbitem);
				if (updatedata != null)
					SetPropertyValue<tEntity>(dbitem, updatedata, value);
				db.Insert<tEntity>(dbitem);
				ret = true;
				System.Threading.Thread.Sleep(Time_Loop_Delay);

			}
			return ret;
		}

		public static T LoadConfigFile<T>(string filepath)
		{
			T config = DeserializefromFile<T>(filepath);
			return config;
		}
		
		public static T LoadConfig<T>(string xmlStr)
		{
			T config = DeserializeFromXMLString<T>(xmlStr);
			return config;
		}
		
		public static T LoadConfig<T>(XmlDocument xDoc)
		{
			T config = DeserializeFromXMLDoc<T>(xDoc);
			return config;
		}

		public static T DeserializeFromXMLDoc<T>(XmlDocument xmlDoc)
		{
			if (xmlDoc == null)
				return default(T);

			T obj = default(T);
			using (XmlReader reader = new XmlNodeReader(xmlDoc.DocumentElement))
			{
				obj = Deserialize<T>(reader);
			}

			return obj;
		}

		private static T DeserializeFromXMLString<T>(string xmlData)
		{
			if (string.IsNullOrEmpty(xmlData))
				return default(T);

			MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(xmlData));
			T obj = default(T);
			obj = Deserialize<T>(stream);
			if (stream != null)
			{
				stream.Close();
				stream.Dispose();
				stream = null;
			}
			return obj;
		}

		private static T DeserializefromFile<T>(string path)
		{
			if (string.IsNullOrEmpty(path) || !File.Exists(path))
			{
				return default(T);
			}
			System.IO.FileStream st = null;
			T cfgData = default(T);
			try
			{
				st = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read);
				cfgData = Deserialize<T>(st);
			}
			catch (Exception)
			{
			}
			finally
			{
				if (st != null)
					st.Close();
				st = null;
			}
			return cfgData;
		}

		private static T Deserialize<T>(Stream stream)
		{
			T cfgData = default(T);
			try
			{
				System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(typeof(T));
				cfgData = (T)ser.Deserialize(stream);
			}
			catch (Exception)
			{
			}
			return cfgData;
		}

		private static T Deserialize<T>(XmlReader xmlReader)
		{
			T cfgData = default(T);
			try
			{
				System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(typeof(T));
				cfgData = (T)ser.Deserialize(xmlReader);
			}
			catch (Exception)
			{
			}
			return cfgData;
		}

		static public string ProcessScheduleData(string sXML)
		{
			string sStartTag = "<![CDATA[";
			string sEndTag = "]]>";
			int iStartLen = sStartTag.Length;
			int iEndLen = sEndTag.Length;

			int nOldStart = 0, nCDStart = 0, nCDEnd = 0;
			string sCData = string.Empty;
			string sValidXML = string.Empty;
			string sBase64 = string.Empty;
			nCDStart = sXML.IndexOf(sStartTag/*"<![CDATA["*/);
			if (nCDStart >= 0)
			{
				nCDEnd = sXML.IndexOf(sEndTag/*"]]>"*/, nCDStart);
				sValidXML = sXML.Substring(0, nCDStart + iStartLen);
			}
			int nCount = 0;
			try
			{
				while (nCDStart > 0 && nCDEnd > 0)
				{
					nCount++;
					sCData = sXML.Substring(nCDStart + iStartLen, nCDEnd - nCDStart - iStartLen);
					byte[] buffData = Encoding.UTF8.GetBytes(sCData);
					sBase64 = Convert.ToBase64String(buffData);
					if (buffData != null)
					{
						Array.Clear(buffData, 0, buffData.Length);
						buffData = null;
					}

					sValidXML += sBase64 + sEndTag/*"]]>"*/;
					nOldStart = nCDStart;
					nCDStart = sXML.IndexOf(sStartTag/*"<![CDATA["*/, nCDEnd + iEndLen);
					if (nCDStart > 0)
					{
						sValidXML += sXML.Substring(nCDEnd + iEndLen, nCDStart + iStartLen - nCDEnd - iEndLen);
						nCDEnd = sXML.IndexOf(sEndTag/*"]]>"*/, nCDStart);
					}
					else
					{
						sValidXML += sXML.Substring(nCDEnd + iEndLen);
						nCDEnd = -1;
					}
				}
			}
			catch (System.Exception)
			{
				sValidXML = string.Empty;
			}
			return sValidXML;
		}

		public virtual async Task<Commons.ERROR_CODE> UpdateToDB()
		{
			return await Task.FromResult<Commons.ERROR_CODE>(ERROR_CODE.OK);
		}
		
		public virtual async Task<string> GetResponseMsg()
		{
			if (ReconnectFlag)
			{
				return await Task.FromResult<string>(GetResponseDataConnect(ReconnectDvrGuid));
			}
			else
			{
				return await Task.FromResult<string>(string.Empty);
			}
		}

		protected void SetDvrReconnect(string dvrGuid, bool reconnectFlag)
		{
			this.ReconnectDvrGuid = dvrGuid;
			this.ReconnectFlag = reconnectFlag;
		}

		private string GetResponseDataConnect(string dvrGuid)
		{
			var seqMessage = new List<string>();
			int keepAliveInterval = LogDB.SVRConfig.KeepAliveInterval;
			seqMessage.Add(Commons.Utils.String2Base64(GetConnectResponseMsg(dvrGuid, keepAliveInterval)));
			seqMessage.Add(Commons.Utils.String2Base64(GetRequestConfigMsg(dvrGuid, (int) DVR_CONFIGURATION.EMS_CFG_HARDWARE)));
			var combined = string.Join(", ", seqMessage);
			ReconnectFlag = false;
			return combined;
		}

		public string GetConnectResponseMsg(string sDVRGuid, int iKeepAliveInterval)
		{
			int iMsgID = (int)CMSMsg.MSG_DVR_CONNECT_RESPONSE;//10001;//(int)I3CMS.common.CMSMsg.MSG_DVR_CONNECT_RESPONSE;
			string strResponse = String.Format(MessageDefines.MSG_CONNECT_RESPONSE, iMsgID, MessageDefines.CMS_SERVER_RESPONSE_VERSION, sDVRGuid, iKeepAliveInterval, AppSettings.AppSettings.Instance.ImageAlertWidth, AppSettings.AppSettings.Instance.ImageAlertHeight);
			return strResponse;
		}

		//AAAA
		public static string GetRequestConfigMsg(string sDVRGuid, int iConfig)
		{
			return GetRequestConfigMsg(sDVRGuid, iConfig, string.Empty);
		}

		public static string GetRequestConfigMsg(string sDVRGuid, int iConfig, string extra)
		{
			int iMsgID = (int)CMSMsg.MSG_DVR_GET_CONFIG;
			string strResponse = String.Format(MessageDefines.MSG_GET_CONFIG, iMsgID, MessageDefines.CMS_SERVER_RESPONSE_VERSION, sDVRGuid, iConfig, extra);
			return strResponse;
		}

		public static string GetDisconnectMsg(string sDvrGuid)
		{
			int iMsgID = (int)CMSMsg.MSG_DVR_DISCONNECT_RESPONSE;//10003;//(int)I3CMS.common.CMSMsg.MSG_DVR_DISCONNECT;
			string sMessage = "<message><header><id>" + iMsgID.ToString() + "</id><name>MSG_DVR_DISCONNECT_RESPONSE</name><version>" + MessageDefines.CMS_SERVER_RESPONSE_VERSION + "</version><dvr_guid>" + sDvrGuid + "</dvr_guid></header><body /></message>";
			return sMessage;
		}

		public static string GetRequestSnapshotMsg(string sDVRGuid, int iChannel, long lTime, bool liveImage = false)
		{
			int iMsgID = (int)CMSMsg.MSG_DVR_GET_VIDEO;//10011;//(int)I3CMS.common.CMSMsg.MSG_DVR_GET_VIDEO;
			string strResponse = String.Format(MessageDefines.MSG_GET_SNAPSHOT, iMsgID, MessageDefines.CMS_SERVER_RESPONSE_VERSION, sDVRGuid, iChannel, lTime, liveImage ? MessageDefines.STR_VideoInput : MessageDefines.STR_ChannelID, 
				AppSettings.AppSettings.Instance.ImageAlertWidth, AppSettings.AppSettings.Instance.ImageAlertHeight, liveImage ? MessageDefines.STR_VideoMask : MessageDefines.STR_Time);
			return strResponse;
		}

		//Anh Huynh, Update for DVR Express, Apr 14, 2015
		public static string GetResponseAllInfoMsg(string sDVRGuid)
		{
			int iMsgID = (int)CMSMsg.MSG_DVR_SENT_ALL_INFO_RESPONSE;
			string strResponse = String.Format(MessageDefines.MSG_RESPONSE_ALLINFO, iMsgID, sDVRGuid);
			return strResponse;
		}

		//public static string GetHeatMapConfigMsg()
		//{
		//	int iMsgID = (int)CMSMsg.MSG_DVR_HEATMAP_CONFIG;
		//	string strResponse = String.Format(MessageDefines.MSG_HEATMAP_CONFIG, iMsgID);
		//	return strResponse;
		//}

		public static string GetHeatMapImageMsg(UInt64 chanMask, DateTime from, DateTime to, int schedule)
		{
			int iMsgID = (int)CMSMsg.MSG_DVR_GET_HEATMAP_IMAGE;
			string strResponse = String.Format(MessageDefines.MSG_HEATMAP_IMAGE, iMsgID, chanMask, from, to, schedule);
			return Commons.Utils.String2Base64(strResponse);
		}

		public DateTime ToDateTime(string strDate)
		{
			try
			{
				return DateTime.ParseExact(strDate, MessageDefines.STR_ALERT_DATE_FORMAT, CultureInfo.InvariantCulture);
			}
			catch (FormatException)
			{
				return DateTime.Now;
			}
		}

		public DateTime ToDateTime(string strDate, string strFormat)
		{
			try
			{
				return DateTime.ParseExact(strDate, strFormat, CultureInfo.InvariantCulture);
			}
			catch (FormatException)
			{
				return DateTime.Now;
			}
		}

		//public DateTime ToDateTime(Int64 unixTime)
		//{
		//	DateTime dtDate = new DateTime(1970, 1, 1, 0, 0, 0);
		//	return dtDate.AddSeconds(unixTime);
		//}
		//public static long ToUnixTimestamp(DateTime dateTime)
		//{
		//	TimeSpan unixTimeSpan = (dateTime - new DateTime(1970, 1, 1, 0, 0, 0));
		//	return (long)unixTimeSpan.TotalSeconds;
		//}

		public static bool GetPoints(List<Point> lsPoint, string strPoint, int numPoint)
		{
			lsPoint.Clear();
			int iStart = 0, nPoint = 0, index = 0;
			string sNumber;
			if (String.IsNullOrEmpty(strPoint))
			{
				return false;
			}
			string[] arrSub = strPoint.Split(';');
			foreach (string sSub in arrSub)
			{
				if (String.IsNullOrEmpty(sSub))
					continue;
				nPoint++;
				iStart = sSub.IndexOf(",");
				if (iStart > 0 && iStart < sSub.Length - 1)
				{
					Point point = new Point();
					sNumber = sSub.Substring(0, iStart);
					point.x = Convert.ToInt32(sNumber);
					sNumber = sSub.Substring(iStart + 1);
					point.y = Convert.ToInt32(sNumber);
					point.ID = index;
					index++;
					lsPoint.Add(point);
				}
			}

			if (nPoint != numPoint)
			{
				return false;
			}
			return true;
		}

		public static int Compare<T>(T x, T y)
		{
			Type type = typeof(T);
			PropertyInfo[] properties = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public);
			FieldInfo[] fields = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public);
			int compareValue = 0;

			foreach (PropertyInfo property in properties)
			{
				IComparable valx = property.GetValue(x, null) as IComparable;
				if (valx == null)
					continue;
				object valy = property.GetValue(y, null);
				compareValue = valx.CompareTo(valy);
				if (compareValue != 0)
					return compareValue;
			}
			foreach (FieldInfo field in fields)
			{
				IComparable valx = field.GetValue(x) as IComparable;
				if (valx == null)
					continue;
				object valy = field.GetValue(y);
				compareValue = valx.CompareTo(valy);
				if (compareValue != 0)
					return compareValue;
			}

			return compareValue;
		}

		public static bool Compare2Object<T>(T object1, T object2)
		{
			Type type = typeof(T);

			if (object1 == null || object2 == null)
				return false;

			foreach (System.Reflection.PropertyInfo property in type.GetProperties())
			{
				if (property.Name != "ExtensionData")
				{
					IComparable valx = property.GetValue(object1, null) as IComparable;
					if (valx == null)
						continue;
					object valy = property.GetValue(object2, null);
					int compareValue = valx.CompareTo(valy);
					if (compareValue != 0)
						return false;
				}
			}
			return true;
		}

		private static bool IsPrimitiveType(Type type)
		{
			return (type == typeof(object) || Type.GetTypeCode(type) != TypeCode.Object);
		}

		public static void CopyPropertyPrimitiveValues(object source, object destination)
		{
			var destProperties = destination.GetType().GetProperties();

			foreach (var sourceProperty in source.GetType().GetProperties())
			{
				if (!IsPrimitiveType(sourceProperty.PropertyType)) continue;
				foreach (var destProperty in destProperties)
				{
					if (!destProperty.CanWrite) continue;
					if (destProperty.Name == sourceProperty.Name && destProperty.PropertyType.IsAssignableFrom(sourceProperty.PropertyType))
					{
						destProperty.SetValue(destination, sourceProperty.GetValue(source, new object[] { }), new object[] { });
						break;
					}
				}
			}
		}

		public static void CopyPropertyValues(object source, object destination)
		{
			var destProperties = destination.GetType().GetProperties();
			foreach (var sourceProperty in source.GetType().GetProperties())
			{
				foreach (var destProperty in destProperties)
				{
					if (!destProperty.CanWrite) continue;
					if (destProperty.Name == sourceProperty.Name && destProperty.PropertyType.IsAssignableFrom(sourceProperty.PropertyType))
					{
						destProperty.SetValue(destination, sourceProperty.GetValue(source, new object[] { }), new object[] { });
						break;
					}
				}
			}

		}

		//public void UpdateChecksum(int iConfig, Int64 checksum, Int64 timechange, DateTime dvrTime)
		//{
		//	tDVRConfigChangeTime cfgChecksum = DVRAdressBook.tDVRConfigChangeTimes.FirstOrDefault(x => x.KConfig == iConfig);
		//	if (cfgChecksum == null)
		//	{
		//		cfgChecksum = new tDVRConfigChangeTime()
		//		{
		//			KConfig = (byte)iConfig,
		//			Checksum = checksum,
		//			TimeChange = (int)timechange,
		//			DVRTime = dvrTime,
		//			CMSTime = DateTime.Now
		//		};
		//		db.Insert<tDVRConfigChangeTime>(cfgChecksum);
		//	}
		//	else
		//	{
		//		cfgChecksum.Checksum = checksum;
		//		cfgChecksum.TimeChange = (int)timechange;
		//		cfgChecksum.DVRTime = dvrTime;
		//		cfgChecksum.CMSTime = DateTime.Now;

		//		db.Update<tDVRConfigChangeTime>(cfgChecksum);
		//	}
		//}

		public bool UpdateChecksum(int iConfig, RawMsgCommon msgCommon) //Int64 checksum, Int64 timechange, DateTime dvrTime)
		{
			if (msgCommon == null)
				return false;

			db.Include<tDVRAddressBook, tDVRConfigChangeTime>(DVRAdressBook, item => item.tDVRConfigChangeTimes);
			tDVRConfigChangeTime cfgChecksum = DVRAdressBook.tDVRConfigChangeTimes.FirstOrDefault(x => x.KConfig == iConfig);
			if (cfgChecksum == null)
			{
				tCMSConfigPage cfgPage = db.FirstOrDefault<tCMSConfigPage>(x => x.KConfig == (byte)iConfig);
				cfgChecksum = new tDVRConfigChangeTime()
				{
					KDVR = DVRAdressBook.KDVR,
					tCMSConfigPage = cfgPage,
					KConfig = (byte)iConfig,
					Checksum = msgCommon.Checksum,
					TimeChange = (int)msgCommon.TimeChange,
					DVRTime = msgCommon.dtDVRTime,
					CMSTime = DateTime.Now,
					tDVRAddressBook = DVRAdressBook
				};
				db.Insert<tDVRConfigChangeTime>(cfgChecksum);
			}
			else
			{
				cfgChecksum.Checksum = msgCommon.Checksum;
				cfgChecksum.TimeChange = (int)msgCommon.TimeChange;
				cfgChecksum.DVRTime = msgCommon.dtDVRTime;
				cfgChecksum.CMSTime = DateTime.Now;

				db.Update<tDVRConfigChangeTime>(cfgChecksum);
			}
			return true;
		}

		public List<string> CheckAndGetImages(tDVRAddressBook dvr, Int64 imgTime, int ichan = -1, bool ignoreTimeout = false)
		{
			List<string> seqMessage = new List<string>();
			string sPath = System.IO.Path.Combine(AppSettings.AppSettings.Instance.AppData, MessageDefines.PATH_RAW_DVRIMAGE, dvr.KDVR.ToString());//AppDomain.CurrentDomain.GetData("DataDirectory").ToString()
			try
			{
				bool isExist = Directory.Exists(sPath);
				if (!isExist)
				{
					Directory.CreateDirectory(sPath);
					isExist = Directory.Exists(sPath);
				}
				if (!ignoreTimeout)
				{
					DirectoryInfo dirInf = new DirectoryInfo(sPath);
					TimeSpan tsLastImg = DateTime.Now - dirInf.LastWriteTime;
					if (isExist && tsLastImg.TotalMinutes < RAWIMAGE_TIMEOUT)
					{
						return seqMessage;
					}
				}
				//else
				//{
				//	using (StreamWriter sw = new StreamWriter("D:\\getImgTimes.txt", true))
				//	{
				//		sw.WriteLine(imgTime);
				//		sw.Close();
				//	}
				//}
			}
			catch(Exception)
			{
				//return null;
			}

			//if (DVRAdressBook.CMSMode == (short)CMS_MODE_STANDARD) //Only need to request image for standard DVR
			if (ichan == -1) //Get all channels here
			{
				if (imgTime == 0)
				{
					seqMessage.Add(Commons.Utils.String2Base64(GetRequestSnapshotMsg(dvr.DVRGuid, -1, 0, true))); //get all images
				}
				else
				{
					db.Include<tDVRAddressBook, tDVRChannels>(dvr, item => item.tDVRChannels);
					List<int> lsChannels = dvr.tDVRChannels.Where(ch => ch.VideoSource.HasValue && ch.VideoSource > 0).Select(x => (int)x.ChannelNo).Distinct().ToList();
					foreach (int ch in lsChannels)
					{
						seqMessage.Add(Commons.Utils.String2Base64(GetRequestSnapshotMsg(dvr.DVRGuid, ch, imgTime, false)));
					}
				}
			}
			else
			{
				seqMessage.Add(Commons.Utils.String2Base64(GetRequestSnapshotMsg(dvr.DVRGuid, ichan, imgTime, false)));
			}

			SnapshotStorage.Instance.RecycleData(System.IO.Path.Combine(AppSettings.AppSettings.Instance.AppData, MessageDefines.PATH_ALERT_IMAGE));
			return seqMessage;
		}

		public virtual void Dispose()
		{
			db = null;
			LogDB = null;
			dvrInfo = null;
			DVRAdressBook = null;
			msgHeader = null;
			msgBody = null;

		}
	}

	#region XML Classes
	public enum CMSMsg
	{
		MSG_DVR_CONNECT = 10000,
		MSG_DVR_CONNECT_RESPONSE,
		MSG_DVR_DISCONNECT,
		MSG_DVR_DISCONNECT_RESPONSE,
		MSG_DVR_KEEPALIVE,
		MSG_DVR_GET_CONFIG,						// 10005
		MSG_DVR_GET_CONFIG_RESPONSE,
		MSG_DVR_SET_CONFIG,
		MSG_DVR_SET_CONFIG_RESPONSE,
		MSG_DVR_CONFIG_CHANGED,
		MSG_DVR_SERVER_ALERT,					// 10010
		MSG_DVR_GET_VIDEO,
		MSG_DVR_GET_VIDEO_RESPONSE,
		MSG_DVR_TIME_CHANGED,
		MSG_DVR_DOWNLOAD_FILE,					// LP add to reserve for download file function
		MSG_DVR_DOWNLOAD_FILE_RESPONSE,			// 10015, LP add to reserve for download file function
		MSG_DVR_GET_SNAPSHOT,
		MSG_DVR_GET_SNAPSHOT_RESPONSE,
		MSG_DVR_PERFORMANCE_STATUS,				// Collecting performance of DVR
		MSG_DVR_SERVER_ALERT_ALL,				// all alert in a connection interval
		MSG_DVR_SENT_ALL_INFO,					// 10020, DVR Express: announce CMS done sending all information
		MSG_DVR_SENT_ALL_INFO_RESPONSE,			// CMS Server -> DVR Express: CMS receive all information
		MSG_DVR_GET_KEEP_ALIVE_INTERVAL,		// CMS Server -> DVR: CMS update keep alive interval
		MSG_DVR_GET_TECHNICAL_LOG,
		MSG_DVR_GET_TECHNICAL_LOG_RESPONSE,
		MSG_DVR_CONNECT_PENDING,// => 10025 (msg connect response)
		MSG_DVR_XML_CONFIG_CHANGED, // => 10026
		MSG_DVR_NETWORK_CONNECTED,
		MSG_DVR_GET_HEATMAP_IMAGE,
		MSG_DVR_GET_HEATMAP_IMAGE_RESPONSE,
		MSG_DVR_RECONNECT,						//10030, update config to API when change from Pending -> Connected
		MSG_DVR_END
	}

	public enum DVR_CONFIGURATION : int
	{
		EMS_CFG_START = 0,
		EMS_CFG_MANAGE_USERS,						// 1, the start of group synchronize message
		EMS_CFG_IP_CAMERA,
		EMS_CFG_HARDWARE,
		EMS_CFG_SYSTEM_INFO,
		EMS_CFG_SERVER_INFO,						// 5
		EMS_CFG_RECORD_SCHEDULE,
		EMS_CFG_VIDEO_LOGIX,						// 7
		EMS_CFG_RECORDING,
		EMS_CFG_VIDEO,
		EMS_CFG_MOTION,								// 10
		EMS_CFG_INTELLI_ZONE,						// 11
		EMS_CFG_INTELLI_GUARD,
		EMS_CFG_TEXT_OVERLAY,
		EMS_CFG_VIRTUAL_RULER,
		EMS_CFG_EMAIL,								// 15
		EMS_CFG_NETWORK,							// 16, the end of group synchronize message
		EMS_CFG_STORAGE,
		EMS_CFG_LOG_RECORD,
		EMS_CFG_LOG_CONTENT,
		EMS_CFG_ALERTS,								// 20
		EMS_CFG_DVR_INFO,							// 21
		EMS_CFG_BOOK_MARK,							// 22
		EMS_CFG_VISION_COUNT,						// 23
		EMS_CFG_VIDEO_PRIVACY,
		EMS_CFG_EMAP,								// not support now
		EMS_CFG_ISEARCH,
		EMS_CFG_END
	}

	[XmlRoot(MessageDefines.STR_Body)]
	public class RawMsgBody
	{
		[XmlElement(MessageDefines.STR_TimeChange)]
		public Int64 TimeChange { get; set; }

		[XmlElement(MessageDefines.STR_ConfigID)]
		public Int32 ConfigID { get; set; }

		[XmlElement(MessageDefines.STR_ConfigName)]
		public string ConfigName { get; set; }
		//Data here ...
		//End data
		[XmlElement(MessageDefines.STR_Whois)]
		public RawMsgWhois msgWhois { get; set; }
	}

	[Serializable()]
	[XmlRoot(MessageDefines.STR_Header)]
	public class RawMsgHeader
	{
		Int32 _msgID = 0;
		[XmlElement(MessageDefines.STR_id)]
		public Int32 MsgID
		{
			get { return _msgID; }
			set { _msgID = value; }
		}

		string _name = string.Empty;
		[XmlElement(MessageDefines.STR_Name)]
		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		string _version = string.Empty;
		[XmlElement(MessageDefines.STR_Version)]
		public string Version
		{
			get { return _version; }
			set { _version = value; }
		}

		string _guid = string.Empty;
		[XmlElement(MessageDefines.STR_DVRGuid)]
		public string DVRGuid
		{
			get { return _guid; }
			set { _guid = value; }
		}

		public RawMsgHeader()
		{
		}
	}
	[XmlRoot(MessageDefines.STR_Common)]
	public class RawMsgCommon
	{
		[XmlElement(MessageDefines.STR_TimeChange)]
		public Int64 TimeChange { get; set; }

		[XmlElement(MessageDefines.STR_Checksum)]
		public Int64 Checksum { get; set; }

		[XmlElement(MessageDefines.STR_DVRTime)]
		public string DVRTime { get; set; }
		public DateTime? dtDVRTime
		{
			get
			{
				if (String.IsNullOrEmpty(DVRTime)) return null;
				return DateTime.ParseExact(DVRTime, MessageDefines.STR_DVR_DATETIME_FORMAT, CultureInfo.InvariantCulture);
			}
		}

		[XmlElement(MessageDefines.STR_ConfigID)]
		public Int32 ConfigID { get; set; }

		[XmlElement(MessageDefines.STR_ConfigName)]
		public string ConfigName { get; set; }

		public RawMsgCommon()
		{
		}
	}
	[XmlRoot(MessageDefines.STR_Whois)]
	public class RawMsgWhois
	{
		[XmlElement(MessageDefines.STR_Time)]
		public Int64 Time { get; set; }

		[XmlElement(MessageDefines.STR_User)]
		public string User { get; set; }

		[XmlElement(MessageDefines.STR_From)]
		public Int32 From { get; set; }

		[XmlElement(MessageDefines.STR_IP)]
		public string IP { get; set; }

		[XmlElement(MessageDefines.STR_DVRTime)]
		public RawDVRTime dvrTime { get; set; }

		public RawMsgWhois()
		{
		}
	}

	[XmlRoot(MessageDefines.STR_DVRTime)]
	public class RawDVRTime
	{
		[XmlElement(MessageDefines.STR_Year)]
		public Int32 Year { get; set; }

		[XmlElement(MessageDefines.STR_Month)]
		public Int32 Month { get; set; }

		[XmlElement(MessageDefines.STR_Day)]
		public Int32 Day { get; set; }

		[XmlElement(MessageDefines.STR_Hour)]
		public Int32 Hour { get; set; }

		[XmlElement(MessageDefines.STR_Minute)]
		public Int32 Minute { get; set; }

		[XmlElement(MessageDefines.STR_Second)]
		public Int32 Second { get; set; }

		public DateTime Value
		{
			get
			{
				return new DateTime(Year, Month, Day, Hour, Minute, Second);
			}
		}

		public RawDVRTime()
		{
		}
	}

	[Serializable()]
	public class DateInfo
	{
		[XmlElement(MessageDefines.STR_Day)]
		public Int32 Day1 { get; set; }
		
		[XmlElement(MessageDefines.STR_Date)]
		public Int32 Day2 { get; set; }

		[XmlElement(MessageDefines.STR_Month)]
		public Int32 Month { get; set; }

		[XmlElement(MessageDefines.STR_Year)]
		public Int32 Year { get; set; }

		[XmlElement(MessageDefines.STR_DayOfWeek)]
		public Int32 DayOfWeek { get; set; }

		public Int32 Day
		{
			get
			{
				return (Day1 > 0) ? Day1 : Day2;
			}
		}

		public DateTime Value
		{
			get
			{
				if (Year >= 1970)
					return new DateTime(Year, Month, Day);
				else
					return new DateTime(1970, 1, 1);
			}
		}
	}

	[Serializable()]
	public class TimeInfo
	{
		[XmlElement(MessageDefines.STR_Hour)]
		public Byte Hour { get; set; }

		[XmlElement(MessageDefines.STR_Minute)]
		public Byte Minute { get; set; }

		[XmlElement(MessageDefines.STR_Second)]
		public Byte Second { get; set; }

		[XmlElement(MessageDefines.STR_DVRTime)]
		public RawDVRTime dvrTime { get; set; }

		public DateTime Value
		{
			get
			{	
				return DateTime.Now.Date.Add(new TimeSpan(Hour, Minute, Second));
			}
		}
	}

	[Serializable()]
	public class IdleTime
	{
		[XmlElement(MessageDefines.STR_Hour)]
		public int Hour { get; set; }

		[XmlElement(MessageDefines.STR_Minute)]
		public int Minute { get; set; }

		[XmlElement(MessageDefines.STR_Second)]
		public int Second { get; set; }

		public DateTime Value
		{
			get
			{
				return DateTime.Now.Date.Add(new TimeSpan(Hour, Minute, Second));
			}
		}
	}
	[Serializable()]
	public class DVRTimeStamp
	{
		[XmlAttribute(MessageDefines.STR_TimeStamp)]
		public Int64 TimeStamp { get; set; }

		[XmlAttribute(MessageDefines.STR_TimeZone)]
		public int TimeZone { get; set; }

		[XmlText]
		public string DVRTime { get; set; }
	}

	public class Point : IMessageEntity<tDVRVCPoints>, IMessageEntity<tDVRVLPoints>, IMessageEntity<tDVRVPPoints>
	{
		public int x;
		public int y;
		public int ID{ get; set;}
		public Point()
		{
			x = 0;
			y = 0;
		}
		public Point(int _x, int _y)
		{
			x = _x;
			y = _y;
		}
		public Point(int _x, int _y, int _id)
		{
			x = _x;
			y = _y;
			ID = _id;
		}

		public bool Equal(tDVRVCPoints value)
		{
			return value.x == x
			 && value.y == y
			&& value.VCPointIndex == ID;
		}

		public void SetEntity(ref tDVRVCPoints value)
		{
			if(value == null)
				value = new tDVRVCPoints();
			value.x = x;
			value.y = y;
			value.VCPointIndex = ID;
		}

		public bool Equal(tDVRVLPoints value)
		{
			return value.x == x
			 && value.y == y
			&& value.VLPointIndex == ID;
		}

		public void SetEntity(ref tDVRVLPoints value)
		{
			if(value == null)
				value = new tDVRVLPoints();
			value.x = x;
			value.y = y;
			value.VLPointIndex = ID;
		}

		public bool Equal(tDVRVPPoints value)
		{
			return value.x == x
			 && value.y == y
			&& value.VPPointIndex == ID;
		}

		public void SetEntity(ref tDVRVPPoints value)
		{
			if(value==null)
				value = new tDVRVPPoints();
			value.x = x;
			value.y = y;
			value.VPPointIndex = ID;
		}
	}

	public interface IToEntityObject<tSource, tDes> where tSource: class where tDes: class
	{
		tDes ToEntity(tSource source);

	}
	public interface IToEntityObject<tDes> where tDes : class
	{
		tDes ToEntity();
		void SetEntity( ref tDes value);


	}

	
	#endregion
	public interface IMessageEntityCompare<T> where T : class
	{
		bool Equal(T value);
	}

	public interface IMessageEntity<T> : IMessageEntityCompare<T> where T : class
	{
		//bool Equal(T value);
		void SetEntity(ref T value);

	}
	public static class MessageDefines
	{
		public const string CMS_SERVER_RESPONSE_VERSION = "5.0";

		#region paramXML
		public const string STR_Message = "message";
		public const string STR_Header = "header";
		public const string STR_id = "id";
		public const string STR_Name = "name";
		public const string STR_Version = "version";
		public const string STR_DVRGuid = "dvr_guid";
		public const string STR_Body = "body";

		public const string STR_Whois = "whois";
		public const string STR_Time = "time";
		public const string STR_User = "user";
		public const string STR_From = "from";
		public const string STR_IP = "ip";
		public const string STR_DVRTime = "dvr_time";

		public const string STR_Year = "year";
		public const string STR_Month = "month";
		public const string STR_Day = "day";
		public const string STR_Hour = "hour";
		public const string STR_Minute = "minute";
		public const string STR_Second = "second";
		public const string STR_Date = "date";

		public const string STR_DVRVersion = "dvr_version";
		public const string STR_DVRProduct = "dvr_product";
		public const string STR_TimeResult = "time_result";

		public const string STR_TimeChange = "time_change";
		public const string STR_ConfigID = "configuration_id";
		public const string STR_ConfigName = "configuration_name";
		public const string STR_Size = "size";
		public const string STR_Checksum = "checksum";
		public const string STR_Common = "common";
		public const string STR_DayOfWeek = "dayofweek";
		//public const string STR_DVRTime = "dvr_time";

		public const string STR_ChannelID = "channel_id";
		public const string STR_VideoInput = "video_input";
		public const string STR_TimeStamp = "timestamp";
		public const string STR_TimeZone = "timezone";
		//public const string STR_Time = "time";
		public const string STR_VideoMask = "video_mask";

		public const string STR_ALERT_DATE_FORMAT = "yyyy/MM/dd - HH:mm:ss";
		public const string STR_DVR_DATETIME_FORMAT = "yyyy-MM-dd HH:mm:ss";
		public const string STR_IMG_DATE_FORMAT = "yyyy/MM/dd-HH:mm:ss";

		public const string MSG_CONNECT_RESPONSE = @"<message><header><id>{0}</id><name>MSG_DVR_CONNECT_RESPONSE</name><version>{1}</version><dvr_guid>{2}</dvr_guid></header>
											<body><keep_alive_interval>{3}</keep_alive_interval><image_width>{4}</image_width><image_height>{5}</image_height></body></message>";

		public const string MSG_GET_CONFIG = @"<message><header><id>{0}</id><name>MSG_DVR_GET_CONFIG</name><version>{1}</version><dvr_guid>{2}</dvr_guid></header>
									<body><configuration_id>{3}</configuration_id><next_configs>{4}</next_configs></body></message>";

		public const string MSG_GET_SNAPSHOT = @"<message><header><id>{0}</id><name>MSG_DVR_GET_VIDEO</name><version>{1}</version><dvr_guid>{2}</dvr_guid></header>
									<body><{5}>{3}</{5}><{8}>{4}</{8}><image_width>{6}</image_width><image_height>{7}</image_height></body></message>";
		//public const string MSG_GET_LIVEIMAGE = @"<message><header><id>{0}</id><name>MSG_DVR_GET_VIDEO</name><version>{1}</version><dvr_guid>{2}</dvr_guid></header>
		//							<body><video_input>{3}</video_input><video_mask>{4}</video_mask><image_width>{6}</image_width><image_height>{7}</image_height></body></message>";

		public const string MSG_RESPONSE_ALLINFO = @"<message><header><id>{0}</id><name>MSG_DVR_SENT_ALL_INFO_RESPONSE</name><version>" + CMS_SERVER_RESPONSE_VERSION + "</version><dvr_guid>{1}</dvr_guid></header></message>";

		public const string MSG_HEATMAP_IMAGE = @"<message><header><id>{0}</id><name>MSG_DVR_GET_HEATMAP_IMAGE</name><version>" + CMS_SERVER_RESPONSE_VERSION + "</version><dvr_guid>DVR_GUI</dvr_guid></header><body><channels>{1}</channels><from>{2:yyyy-MM-dd-HH-mm-ss}</from><to>{3:yyyy-MM-dd-HH-mm-ss}</to><schedule>{4}</schedule><image>1</image><raw>0</raw></body></message>";

		public const string PATH_RAW_DVRIMAGE = @"DVR\RawImages";
		public const string PATH_ALERT_IMAGE = @"DVR\AlertImages";
		public const string PATH_HEATMAP_IMAGE = @"DVR\Heatmap";
		public const string IMAGE_FILENAME = "channel_{0}_{1}.jpg";
		#endregion

		public struct VideoHeader
		{
			[MarshalAs(UnmanagedType.I4)]
			public Int32 param;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string guid;
			[MarshalAs(UnmanagedType.I4)]
			public Int32 time;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string stime;
		};
	}

	public class StringEqualityComparer:IEqualityComparer<string>
	{
		bool ignoredcase = false;
		public StringEqualityComparer( bool ignoredCase = false)
		{
			ignoredcase = ignoredCase;
		}
		public bool Equals(string x, string y)
		{
			return string.Compare(x,y, ignoredcase) == 0;
		}

		public int GetHashCode(string obj)
		{
			if(obj == null)
				return 0;

			return obj.GetHashCode();
		}
	}
	public interface IDVRMsg
	{
		void SetEvnVars(PACDMModel.PACDMDB pacDB, SVRManager Logdb, MessageDVRInfo dvrinfo);
		Task<Commons.ERROR_CODE> UpdateToDB();
		Task<string> GetResponseMsg();

	}

	public class i3Image : IDisposable
	{
		MemoryStream mem = null;
		Image _img = null;
		public int Width { get { return _img == null ? 0 : _img.Width; } }
		public int Height { get { return _img == null ? 0 : _img.Height; } }
		public System.Drawing.Image Image
		{
			get { return _img; }
			set { _img = value; }
		}

		public i3Image(byte[] buff)
		{
			InitImage(buff);
		}

		public i3Image(Image img)
		{
			Image = img;
		}

		public Image GetThumbnail(int width = 0, int height = 0)
		{
			Image retImage = null;
			if (width > 0 && height > 0 && _img != null)
			{
				retImage = _img.GetThumbnailImage(width, height, null, IntPtr.Zero);
			}
			return retImage;
		}
		private void InitImage(byte[] buff)
		{
			if (buff == null)
				return;
			try
			{
				mem = new MemoryStream(buff);
				Image = Image.FromStream(mem, true, false);
			}
			catch (Exception ex)
			{
			}
		}
		~i3Image()
		{
			Dispose();
		}
		public void Dispose()
		{
			RelaseMem();
		}
		private void RelaseMem()
		{
			if (_img != null)
			{
				_img.Dispose();
				_img = null;
			}
			if (mem != null)
			{
				mem.Dispose();
				mem.Close();
				mem = null;
			}
		}

		public string SaveImage(string savedName, int width = 0, int height = 0)
		{
			return SaveImage(savedName, false,DateTime.MinValue, 0, width, height);
			/*
			string sError = string.Empty;
			if (_img == null || savedName == null || savedName.Length <= 0)
				return "Invalid parameter";
			if (File.Exists(savedName))
				return "File exits";

			string sPath = savedName.Substring(0, savedName.LastIndexOf("\\"));
			if (!Directory.Exists(sPath))
				Directory.CreateDirectory(sPath);

			try
			{
				if (width > 0 && height > 0)
				{
					Image.GetThumbnailImageAbort myCallback = new Image.GetThumbnailImageAbort(ThumbnailCallback);
					Image imageToSave = _img.GetThumbnailImage(width, height, myCallback, IntPtr.Zero);
					imageToSave.Save(savedName, System.Drawing.Imaging.ImageFormat.Jpeg);
					imageToSave.Dispose();
					imageToSave = null;
				}
				else
				{
					_img.Save(savedName, System.Drawing.Imaging.ImageFormat.Jpeg);
				}
			}
			catch (Exception ex)
			{
				sError = ex.ToString();
			}
			return sError;*/
		}
		public string SaveImage(string savedName, bool overWrite, DateTime dvrTime, int owTimeout = 0, int width = 0, int height = 0)
		{
			string sError = string.Empty;
			if (_img == null || savedName == null || savedName.Length <= 0)
				return "Invalid parameter";
			if (File.Exists(savedName))
			{
				if (!overWrite)
					return "File exits";
				else
				{
					try
					{
						FileInfo fi = new FileInfo(savedName);
						TimeSpan ts = DateTime.Now - fi.CreationTime;
						if (ts.TotalMinutes > owTimeout)
						{
							File.Delete(savedName);
						}
						else
						{
							return "";
						}
					}
					catch(Exception)
					{}
				}
			}

			string sPath = savedName.Substring(0, savedName.LastIndexOf("\\"));
			if (!Directory.Exists(sPath))
				Directory.CreateDirectory(sPath);

			bool bUpdateTime = false;
			DateTime dtDVRTime = DateTime.Now;
			if (dvrTime != DateTime.MinValue)
			{
				bUpdateTime = true;
				dtDVRTime = dvrTime;// dvrTime.unixTime_ToUnDateTime();//RawDVRConfig.
			}

			try
			{
				if (width > 0 && height > 0)
				{
					Image.GetThumbnailImageAbort myCallback = new Image.GetThumbnailImageAbort(ThumbnailCallback);
					Image imageToSave = _img.GetThumbnailImage(width, height, myCallback, IntPtr.Zero);
					imageToSave.Save(savedName, System.Drawing.Imaging.ImageFormat.Jpeg);
					imageToSave.Dispose();
					imageToSave = null;
					if (bUpdateTime)
						File.SetLastWriteTimeUtc(savedName, dtDVRTime);

				}
				else
				{
					_img.Save(savedName, System.Drawing.Imaging.ImageFormat.Jpeg);
					if (bUpdateTime)
						File.SetLastWriteTimeUtc(savedName, dtDVRTime);
				}
			}
			catch (Exception ex)
			{
				sError = ex.ToString();
			}
			return sError;
		}
		private bool ThumbnailCallback() { return false; }
	}
}

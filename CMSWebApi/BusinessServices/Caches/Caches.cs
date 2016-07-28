using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.Cache;
using CMSWebApi.Cache.Caches;
using CMSWebApi.DataModels.DashBoardCache;
using CMSWebApi.ServiceInterfaces;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using Newtonsoft.Json;

namespace CMSWebApi.BusinessServices.Caches
{
	public class Caches: BusinessBase<IAccountService>
	{
		#region Get cache
		public MemoryStream GetCache( string name, string format, out string cache_Status)
		{
			MemoryStream mem = null;
			bool isjson = (string.IsNullOrEmpty(format) || string.Compare(format, CMSWebApi.Utils.Consts.STR_JSON, true) == 0)? true : false;
			cache_Status = null;

			CMSWebApi.Cache.CacheStatus status = Cache.CacheStatus.Not_ready;

			switch(name.ToLower())
			{
				case CMSWebApi.Cache.Defines.Alert_Cache_Name:
					mem = GetCache<AlertCacheModel, int>(isjson, it => it.Time, out status);
				break;
				case CMSWebApi.Cache.Defines.POS_Cache_Name:
				mem = GetCache<POSPeriodicCacheModel, DateTime>(isjson, it => it.DVRDateHourKey, out status);
				break;
				case CMSWebApi.Cache.Defines.IOPCCount_Cache_Name:
				mem = GetCache<IOPCCountPeriodicCacheModel, DateTime>(isjson, it => it.DVRDateHourKey, out status);
				break;
			}
			cache_Status = CacheStatusDescription(status);
			return mem;
		}
	
		private string CacheStatusDescription(CMSWebApi.Cache.CacheStatus status)
		{
			string ret =string.Empty;
			switch( status)
			{
				case Cache.CacheStatus.Loading:
					ret = CMSWebApi.Utils.Consts.CacheStatus_Defines.CACHE_LOADING;
				break;
				case Cache.CacheStatus.Not_ready:
					ret = CMSWebApi.Utils.Consts.CacheStatus_Defines.CACHE_NOT_READY;
				break;
				case Cache.CacheStatus.Rebuild:
					ret = CMSWebApi.Utils.Consts.CacheStatus_Defines.CACHE_REBUILD;
				break;
				case Cache.CacheStatus.Ready:
					ret = CMSWebApi.Utils.Consts.CacheStatus_Defines.CACHE_READY;
				break;
			}
			return ret;
		}

		private MemoryStream GetJson<T>(IEnumerable<T> items) where T : CacheModelBase
		{
			string data = JsonConvert.SerializeObject(items, new JsonSerializerSettings{ DateTimeZoneHandling = DateTimeZoneHandling.Utc, PreserveReferencesHandling = PreserveReferencesHandling.None});
			MemoryStream stream = new MemoryStream();
			StreamWriter writer = new StreamWriter(stream);
			writer.Write(data);
			writer.Flush();
			stream.Position = 0;
			return stream;
		}
		
		private MemoryStream GetXml<T>(IEnumerable<T> items)
		{
			XmlAttributeOverrides overrides = new XmlAttributeOverrides();
			XmlAttributes attr = new XmlAttributes();
			attr.XmlRoot = new XmlRootAttribute("Data");
			overrides.Add(typeof(List<T>), attr);

			XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
			ns.Add("", "");
			XmlDocument xmlDoc = new XmlDocument();
			XPathNavigator nav = xmlDoc.CreateNavigator();
			using (XmlWriter writer = nav.AppendChild())
			{
				try
				{
				XmlSerializer ser = new XmlSerializer(typeof(List<T>), overrides);
				List<T> parameters = items.ToList();
				ser.Serialize(writer, parameters, ns);
				}
				catch(Exception){}
			}

			MemoryStream xmlStream = new MemoryStream();
			xmlDoc.Save(xmlStream);

			xmlStream.Flush();//Adjust this if you want read your data 
			xmlStream.Position = 0;

			return xmlStream;
		}

		private MemoryStream GetCache<T, TKey>(bool isjson, Func<T, TKey> sortselector, out CMSWebApi.Cache.CacheStatus status) where T : CacheModelBase
		{
			status = CacheStatus.Not_ready;
			ICache<T> cache = BackgroundTaskManager.Instance.GetCache<T>();
			if(cache == null)
				return null;
			status = cache.Status;
			if(status != CacheStatus.Ready)
				return null;
			IEnumerable<T> items = cache.Query( null, it => it);
			IOrderedEnumerable<T> isorts = items.OrderBy<T,TKey>(sortselector);
			return isjson ? GetJson<T>(isorts) : GetXml<T>(isorts);
		}
		#endregion
		#region Refesh cache
		public bool RefreshCache( string name)
		{
			return true;
		}
		#endregion
	}
}

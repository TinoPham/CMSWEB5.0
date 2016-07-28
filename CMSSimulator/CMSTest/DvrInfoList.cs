using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSTest
{
	public static class DvrInfoList
	{
		private const string STR_DVR_SITES = "DVRSites";
		private const string STR_FILENAME_SITE_LIST = "Siteinfo.xml";

		public static List<Utils.DVR_Info> GetDVRInfo(bool dvrstandard, string serverId, int dvrPort, string location, string model, string name, string version, string serverUrl,int total)
		{
			var dvrsInfo = new List<Utils.DVR_Info>();
			var tmp = new List<string>();
			string tmpguid;
			for (int i = 0; i < total; i++)
			{
				tmpguid = Utils.GetRandomMacAddress();
				while (tmp.FindIndex(new Predicate<string>(delegate(string aa)
				{
					return (System.String.Compare(aa, tmpguid, System.StringComparison.OrdinalIgnoreCase) == 0);
				})) >= 0)
				{
					tmpguid = Utils.GetRandomMacAddress();
				}
				tmp.Add(tmpguid);
				var dvr = new Utils.DVR_Info() { DVRPort = dvrPort, ServerUrl = serverUrl, Id = i, Mac = tmpguid, Server_ID = serverId, Server_IP = Utils.RandomIP(), Server_Location = location, Server_Model = model, Server_Name = name, Server_Version = version, Server_standard = dvrstandard, HaspKey = Utils.GetRandomNumber(10000, 199999).ToString() };
				dvrsInfo.Add(dvr);
			}
			tmp.Clear();
			tmp = null;
			return dvrsInfo;
		}

		public static List<Utils.DVR_Info> ReadXml(string apppath)
		{
			if (!apppath.EndsWith("\\"))
				apppath += "\\" + STR_DVR_SITES;
			if (!Directory.Exists(apppath))
				Directory.CreateDirectory(apppath);
			string fpath = apppath + "\\" + STR_FILENAME_SITE_LIST;

			var reader =new System.Xml.Serialization.XmlSerializer(typeof(List<Utils.DVR_Info>));
			using (var file = new System.IO.StreamReader(fpath))
			{
				var dvrList	= (List<Utils.DVR_Info>) reader.Deserialize(file);
				return dvrList;
			}
		}

		public static void WriteXml(List<Utils.DVR_Info> dvrList, string apppath)
		{
			if (!apppath.EndsWith("\\"))
				apppath += "\\" + STR_DVR_SITES;
			if (!Directory.Exists(apppath))
				Directory.CreateDirectory(apppath);
			string fpath = apppath + "\\" + STR_FILENAME_SITE_LIST;
			var writer = new System.Xml.Serialization.XmlSerializer(typeof(List<Utils.DVR_Info>));

			using (var file = new System.IO.StreamWriter(fpath))
			{
				writer.Serialize(file, dvrList);
				file.Close();
			}
		}
	}
}

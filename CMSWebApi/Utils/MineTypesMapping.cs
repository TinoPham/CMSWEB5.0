using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.Utils
{
	public static class MineTypesMapping
	{
		public enum MineTypes
		{
			webm,
			flv,
			mp4,
			jpg,
			png,
			jfif,
			jpe,
			pdf,
			xml,
			json,
			txt,
			xls,
			xlsx,
			doc,
			docx,
			htm,
			html,
			text,
			stream
		}
		private static Dictionary<string, string> ExtensionMap = new Dictionary<string, string>();
		static MineTypesMapping()
		{
			ExtensionMap.Add(".webm", "video/webm");
			ExtensionMap.Add(".flv", "video/x-flv");
			ExtensionMap.Add(".mp4", "video/mp4");
			ExtensionMap.Add(".jpg", "image/jpeg");
			ExtensionMap.Add(".png", "image/jpeg");
			ExtensionMap.Add(".jfif", "image/pjpeg");
			ExtensionMap.Add(".jpe", "image/jpeg");
			ExtensionMap.Add(".pdf", "application/pdf");
			ExtensionMap.Add(".xml", "application/xml");
			ExtensionMap.Add(".json", "application/json");
			ExtensionMap.Add(".txt", "text/plain");
			ExtensionMap.Add(".xls", "application/vnd.ms-excel");
			ExtensionMap.Add(".xlsx", "application/vnd.ms-excel");
			ExtensionMap.Add(".doc", "application/msword");
			ExtensionMap.Add(".docx", "application/msword");
			ExtensionMap.Add(".htm", "text/html");
			ExtensionMap.Add(".html", "text/html");
			ExtensionMap.Add(".text", "text/plain");
			ExtensionMap.Add(".csv", "text/csv");
			ExtensionMap.Add(".*", "application/octet-stream");
		}

		static string _MinetypeHeader(string fileExtension)
		{
			KeyValuePair<string, string> sitem = ExtensionMap.FirstOrDefault(item => string.Compare(item.Key, fileExtension, true) == 0);
			return sitem.Key == null ? ExtensionMap [".*"] : sitem.Value;
		}
		public static string MinetypeHeader(string fileExtension)
		{
			return _MinetypeHeader(fileExtension.StartsWith(".") ? fileExtension : "." + fileExtension);
		}
		public static string MinetypeHeader(MineTypes minetype)
		{
			return MinetypeHeader("." + minetype.ToString());
		}
	}
}

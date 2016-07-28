using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using CMSWebApi.DataModels;
using CMSWebApi.ServiceInterfaces;
using Newtonsoft.Json;

namespace CMSWebApi.BusinessServices.ReportService
{
	public static class ReportFactory
	{
		private const string NameSpace = "CMSWebApi.BusinessServices.ReportService.";
		public static IReportService GetService(string serviceName, object[] paramContructor = null)
		{
			Type type = GetType(String.Format(NameSpace + "{0}", serviceName)); 

			//var parameters = type.GetConstructors().Single().GetParameters().Select(p => paramContructor).ToArray();

			return Activator.CreateInstance(type, paramContructor) as IReportService;
		}

		public static Type GetType(string typeName)
		{
			var type = Type.GetType(typeName);
			if (type != null) return type;
			foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
			{
				type = a.GetType(typeName);
				if (type != null)
					return type;
			}
			return null;
		}

		public static DataTable ToDataTable<T>(List<T> items)
		{
			DataTable dataTable = new DataTable(typeof(T).Name);
			//Get all the properties
			PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
			foreach (PropertyInfo prop in Props)
			{
				//Setting column names as Property names
				dataTable.Columns.Add(prop.Name);
			}
			foreach (T item in items)
			{
				var values = new object[Props.Length];
				for (int i = 0; i < Props.Length; i++)
				{
					//inserting property values to datatable rows
					values[i] = Props[i].GetValue(item, null);
				}
				dataTable.Rows.Add(values);
			}
			//put a breakpoint here and check datatable
			return dataTable;
		}

		public static object DynamicProjection(object input, IEnumerable<string> properties)
		{
			var type = input.GetType();
			dynamic dObject = new ExpandoObject();
			var dDict = dObject as IDictionary<string, object>;

			foreach (var p in properties)
			{
				var field = type.GetField(p);
				if (field != null)
					dDict[p] = field.GetValue(input);

				var prop = type.GetProperty(p);
				if (prop != null && prop.GetIndexParameters().Length == 0)
					dDict[p] = prop.GetValue(input, null);
			}

			return dObject;

			//var names = new[] { "Id", "Name", "Tracks" };
			//var projection = collection.Select(x => DynamicProjection(x, names));
		}

		public static List<TableParamModel> LoadParamResources(string fpath)
		{
			TextReader reader = null;
			try
			{
				reader = File.OpenText(fpath);
				return JsonConvert.DeserializeObject<List<TableParamModel>>(reader.ReadToEnd());
			}
			catch
			{ }
			finally
			{
				if (reader != null)
				{
					reader.Close();
					reader.Dispose();
					reader = null;
				}
			}
			return new List<TableParamModel>();
		}
	}
}

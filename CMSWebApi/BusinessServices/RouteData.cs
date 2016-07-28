using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.DataModels;
using Commons;
using Newtonsoft.Json;
using CMSWebApi.Utils;

namespace CMSWebApi.BusinessServices
{
	public class RouteData : SingletonClassBase<RouteData> 
	{

		private class RouteInfo
		{
			public long Tick{ get ;set;}
			public ApplicationMenu Data{ get; set;}
		}

		const string MODULE_CONFIG_EXTENSION = ".cfg";
		readonly object locker = new object();

		public string AppData_dir{ get ; private set;}
		public string Bin_dir{ get ; private set;}
		public string App_root { get; private set;}
		public string Route_Dir{ get; private set;}
		Dictionary<string, RouteInfo> Routes;
		

		private RouteData()
		{
			Bin_dir = CurrentPath();
			DirectoryInfo root  = Directory.GetParent(Bin_dir);
			App_root = root.FullName;
			AppData_dir = Path.Combine(App_root, "App_Data");
			Route_Dir = Path.Combine(AppData_dir,"CMSWeb.Routes" );
			Routes = new Dictionary<string, RouteInfo>();

		}

		protected string CurrentPath()
		{
			return Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
		}


		private ApplicationMenu LoadRoute(string fpath)
		{
			TextReader reader = null;
			try
			{
				reader = File.OpenText( fpath);
				return JsonConvert.DeserializeObject<ApplicationMenu>(reader.ReadToEnd());
			}
			catch(Exception)
			{}
			finally
			{ 
				if( reader != null)
				{
					 reader.Close();
					 reader.Dispose();
					 reader = null;
				}
			 }
			 return null;
		}

		public ApplicationMenu GetRoute(string routename, bool isMaster)
		{
			ApplicationMenu ret = null;
			lock(locker)
			{
				RouteInfo appmenu = Routes.ContainsKey(routename)? Routes[routename] : null;
				string fpath = Path.Combine(Path.Combine(Route_Dir, routename + MODULE_CONFIG_EXTENSION));
				FileInfo finfo = new FileInfo(fpath);
				if( !finfo.Exists)
					return null;

				if( appmenu == null)
				{
					ret = LoadRoute( fpath);
					if(ret == null)
						return ret;
					//
					// check if user is Admin, just show functions have isAddmin= true (Master show all)
					//
					if (!isMaster && routename == Consts.CONFIGURATIONS)
					{
						List<ApplicationMenu> childs = ret.childs.Where(item => item.isAdmin == true).ToList();
						ret.childs = childs;
					}

					appmenu = new RouteInfo { Tick = DateTime.UtcNow.Ticks, Data = ret as ApplicationMenu };
					Routes.Add(routename,appmenu); 
				}
				else //if (appmenu.Tick < finfo.LastWriteTimeUtc.Ticks)
				{
					ret = LoadRoute(fpath);
					if (ret != null)
					{
						//
						// check if user is Admin, just show functions have isAddmin= true (Master show all)
						//
						if (!isMaster && routename == Consts.CONFIGURATIONS)
						{
							List<ApplicationMenu> childs = ret.childs.Where(item => item.isAdmin == true).ToList();
							ret.childs = childs;
						}

						appmenu.Data = ret as ApplicationMenu;
						appmenu.Tick =DateTime.UtcNow.Ticks;
					}
				}
				//else
				//{
				//	ret = appmenu.Data as ApplicationMenu;
					
				//}

			}
			return ret;
		}
		
	}
}

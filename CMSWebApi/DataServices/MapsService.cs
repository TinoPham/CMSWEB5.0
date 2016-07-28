using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.DataModels;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;

namespace CMSWebApi.DataServices
{
	public partial class MapsService : ServiceBase, IMapsService
	{
		public MapsService(PACDMModel.Model.IResposity model) : base(model) { }

		public MapsService(ServiceBase ServiceBase) : base(ServiceBase.DBModel) { }

		public tCMSWebSiteMapAreas Insert(tCMSWebSiteMapAreas item)
		{
			return DBModel.Insert(item);
		}

		public void Update(tCMSWebSiteMapAreas item)
		{
			DBModel.Update(item);
		}
		public void Delete(tCMSWebSiteMapAreas item)
		{
			DBModel.Delete(item);
		}

		public tCMSWebSiteImage Insert(tCMSWebSiteImage item) 
		{
			return DBModel.Insert(item);
		}

		public void Update(tCMSWebSiteImage item)
		{
			DBModel.Update(item);
		}
		public void Delete(tCMSWebSiteImage item)
		{
			DBModel.Delete(item);
		}
		public bool Save() {
			return DBModel.Save() > 0;
		}
		public IQueryable<Tout> GetImages<Tout>(int sitekey, Expression<Func<tCMSWebSiteImage, Tout>> selector, string[] includes) where Tout : class
		{
			return Query<tCMSWebSiteImage, Tout>(img => img.siteKey == sitekey, selector, includes);
		}

		public IQueryable<Tout> GetChannels<Tout>(int ImageID, Expression<Func<tCMSWebSiteMapAreas, Tout>> selector, string[] includes) where Tout : class
		{
			return Query<tCMSWebSiteMapAreas, Tout>(img => img.ImageID == ImageID, selector, includes);
		}

		public IQueryable<Tout> GetSites<Tout>(int sitekey, Expression<Func<tCMSWebSites, Tout>> selector, string[] includes) where Tout : class
		{
			return Query<tCMSWebSites, Tout>(t => t.siteKey == sitekey, selector, includes);
		}
	}
}

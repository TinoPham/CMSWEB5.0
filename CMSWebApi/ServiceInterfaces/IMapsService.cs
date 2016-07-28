using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.DataModels;
using PACDMModel.Model;

namespace CMSWebApi.ServiceInterfaces
{
	public interface IMapsService
	{

	
		tCMSWebSiteImage Insert(tCMSWebSiteImage item);
		void Update(tCMSWebSiteImage item) ;
		void Delete(tCMSWebSiteImage item) ;


		tCMSWebSiteMapAreas Insert(tCMSWebSiteMapAreas item);
		void Update(tCMSWebSiteMapAreas item) ;
		void Delete(tCMSWebSiteMapAreas item) ;


		bool Save();
		IQueryable<Tout> GetSites<Tout>(int sitekey, Expression<Func<tCMSWebSites, Tout>> selector, string[] includes) where Tout : class;
		IQueryable<Tout> GetImages<Tout>(int sitekey, Expression<Func<tCMSWebSiteImage, Tout>> selector, string[] includes) where Tout : class;
		IQueryable<Tout> GetChannels<Tout>(int ImageID, Expression<Func<tCMSWebSiteMapAreas, Tout>> selector, string[] includes) where Tout : class;

	
	}
}

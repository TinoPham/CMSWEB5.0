using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;

namespace CMSWebApi.DataServices
{
	public class DvrChanelService : ServiceBase, IDvrChanelService
	{
		public DvrChanelService(IResposity model)
			: base(model)
		{
		}
		public DvrChanelService(ServiceBase ServiceBase) : base(ServiceBase.DBModel) { }

		public IQueryable<Tout> GetDvrPACChannels<Tout>(int kdvr, Expression<Func<V_DVR_PAC_Channels, Tout>> selector)
		{
			return base.Query<V_DVR_PAC_Channels, Tout>(it => it.KDVR == kdvr, selector);
		}
		public IQueryable<Tout> GetDvrPACChannels<Tout>(IEnumerable<int> kDvr, Expression<Func<V_DVR_PAC_Channels, Tout>> selector)
		{
			IQueryable<V_DVR_PAC_Channels> vdvrchannels = DBModel.Query<V_DVR_PAC_Channels>(null, null);
			return vdvrchannels.Join<V_DVR_PAC_Channels, int, int, V_DVR_PAC_Channels>(kDvr, vdvr => vdvr.KDVR, kdvr => kdvr, (a, b) => a).Select(selector);

		}

		public void AddDVRChannel( tDVRChannels channel)
		{
			if( channel == null || (channel.KDVR == 0 && channel.tDVRAddressBook == null) )
				return;
			DBModel.Insert<tDVRChannels>(channel);
		}
		public IQueryable<Tout> GetChannels<Tout>(Expression<Func<tDVRChannels, bool>> filter, Expression<Func<tDVRChannels, Tout>> selector, string[] includes = null) where Tout : class
		{
			return DBModel.Query<tDVRChannels, Tout>(selector, filter, includes);
		}

		public Tout GetDvrAddressBookById<Tout>(int kDvr, Expression<Func<tDVRAddressBook, Tout>> selector, string[] includes) where Tout : class
		{
			return base.FirstOrDefault<tDVRAddressBook, Tout>(t => t.KDVR == kDvr, selector, includes);
		}

		public Tout GetDvrAddressBookByMac<Tout>(string mac, Expression<Func<tDVRAddressBook, Tout>> selector, string[] includes) where Tout : class
		{
			return base.FirstOrDefault<tDVRAddressBook, Tout>(t => t.DVRGuid.Equals(mac, StringComparison.OrdinalIgnoreCase), selector, includes);
		}

		public IQueryable<Tout> GetDvrAddressBookByMacs<Tout>(IEnumerable<string> mac, Expression<Func<tDVRAddressBook, Tout>> selector, string [] includes) where Tout : class
		{
			if( mac != null && mac.Any())
			{
				return base.Query<tDVRAddressBook, Tout>(it => mac.Contains(it.DVRGuid) , selector, includes);
				//IQueryable<tDVRAddressBook> result = dvrs.Join( mac, kdvr=> kdvr.DVRGuid, m=> m, (dvr,m)=> dvr);
				//return result.Select<tDVRAddressBook, Tout>(selector);

			}
			return Query<tDVRAddressBook, Tout>( null, selector, includes);

		}
		public IQueryable<Tout> GetDvrAddressBookByHaspLicense<Tout>(IEnumerable<string> hasps, Expression<Func<tDVRAddressBook, Tout>> selector, string[] includes) where Tout : class
		{
			if (hasps != null && hasps.Any())
			{
				return base.Query<tDVRAddressBook, Tout>(it => hasps.Contains(it.HaspLicense), selector, includes);
			}
			return Query<tDVRAddressBook, Tout>(null, selector, includes);

		}

		public IQueryable<Tout> GetSites<Tout>(IEnumerable<int> kDvr, Expression<Func<tDVRAddressBook, Tout>> selector, string[] includes)
		{
			return base.Query<tDVRAddressBook, Tout>(t => kDvr.Contains(t.KDVR), selector, includes);
		} 
	}
}

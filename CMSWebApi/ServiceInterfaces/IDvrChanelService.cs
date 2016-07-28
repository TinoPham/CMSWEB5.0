using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using PACDMModel.Model;

namespace CMSWebApi.ServiceInterfaces
{
	public interface IDvrChanelService
	{
		IQueryable<Tout> GetChannels<Tout>(Expression<Func<tDVRChannels, bool>> filter, Expression<Func<tDVRChannels, Tout>> selector, string[] includes = null) where Tout : class;
		Tout GetDvrAddressBookById<Tout>(int kDvr, Expression<Func<tDVRAddressBook, Tout>> selector, string[] includes) where Tout : class;
		Tout GetDvrAddressBookByMac<Tout>(string mac, Expression<Func<tDVRAddressBook, Tout>> selector, string[] includes) where Tout : class;
		IQueryable<Tout> GetDvrAddressBookByMacs<Tout>(IEnumerable<string> mac, Expression<Func<tDVRAddressBook, Tout>> selector, string [] includes) where Tout : class;
		IQueryable<Tout> GetDvrAddressBookByHaspLicense<Tout>(IEnumerable<string> hasps, Expression<Func<tDVRAddressBook, Tout>> selector, string[] includes) where Tout : class;
		IQueryable<Tout> GetSites<Tout>(IEnumerable<int> kDvr, Expression<Func<tDVRAddressBook, Tout>> selector, string[] includes);
		IQueryable<Tout> GetDvrPACChannels<Tout>(int kdvr, Expression<Func<V_DVR_PAC_Channels, Tout>> selector);
		IQueryable<Tout> GetDvrPACChannels<Tout>(IEnumerable<int> kDvr, Expression<Func<V_DVR_PAC_Channels, Tout>> selector);
		void AddDVRChannel( tDVRChannels channel);

	}

}

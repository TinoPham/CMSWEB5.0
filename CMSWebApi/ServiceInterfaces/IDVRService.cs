using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using PACDMModel.Model;
using System.Threading.Tasks;

namespace CMSWebApi.ServiceInterfaces
{
	public interface IDVRService
	{
		Tout GetDVR<Tout>(int kdvr, Expression<Func<tDVRAddressBook, Tout>> selector, string[] includes);
		int Add(tDVRAddressBook value, bool save = false);
		void Update(tDVRAddressBook value);
		IQueryable<Tout> GetDVRs<Tout>(IEnumerable<int> kdvrs, Expression<Func<tDVRAddressBook, Tout>> selector, string [] includes);
		IQueryable<Tout> GetDVRs<Tout>(Expression<Func<tDVRAddressBook, bool>> filter, Expression<Func<tDVRAddressBook, Tout>> selector, string [] includes);
		IQueryable<Tout> GetDVRs<Tout>(string DVRGuid, string serverID, string serverIP, string pubServerIP, int? Online, string DVRAlias, int? TotalDiskSize, 
										int? FreeDiskSize, int? EnableActivation, DateTime? ActivationDate, DateTime? ExpirationDate, int? RecordingDay, DateTime? FirstAccess, 
										int? KLocation, DateTime? TimeDisConnect, int? DisConnectReason, Int16? CMSMode, int? LastConnectTime, int? CurConnectTime, int? KGroup,
										 int? KDVRVersion, Int64? HaspLicense, Expression<Func<tDVRAddressBook, Tout>> selector, string [] includes);
	}
}

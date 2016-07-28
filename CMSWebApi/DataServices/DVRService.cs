using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;


namespace CMSWebApi.DataServices
{
	public class DVRService : DvrChanelService, IDVRService
	{
		public DVRService(IResposity model): base( model){}

		public DVRService(ServiceBase ServiceBase) : base(ServiceBase.DBModel) { }

		public int Add(tDVRAddressBook value, bool save = false)
		{
			DBModel.Insert<tDVRAddressBook>(value);
			if( save)
				return DBModel.Save();

			return 1;
		}
		public void Update(tDVRAddressBook dvr)
		{
			if( dvr== null || dvr.KDVR == 0)
				return;
			DBModel.Update<tDVRAddressBook>(dvr);
		}
		public Tout GetDVR<Tout>(int kdvr, Expression<Func<tDVRAddressBook, Tout>> selector, string [] includes)
		{
			return base.FirstOrDefault<tDVRAddressBook, Tout>(dvr => dvr.KDVR == kdvr, selector, includes);
		}

		public IQueryable<Tout> GetDVRs<Tout>(IEnumerable<int> kdvrs, Expression<Func<tDVRAddressBook, Tout>> selector, string [] includes)
		{
			return Query<tDVRAddressBook,Tout>(dvr => kdvrs.Contains(dvr.KDVR), selector, includes);
		}

		public IQueryable<Tout> GetDVRs<Tout>(Expression<Func<tDVRAddressBook, bool>> filter, Expression<Func<tDVRAddressBook, Tout>> selector, string [] includes)
		{
			return base.Query<tDVRAddressBook,Tout>(filter, selector, includes);
		}
		
		public IQueryable<Tout> GetDVRs<Tout>(string DVRGuid, string serverID, string serverIP, string pubServerIP, int? Online, string DVRAlias, int? TotalDiskSize, 
										int? FreeDiskSize, int? EnableActivation, DateTime? ActivationDate, DateTime? ExpirationDate, int? RecordingDay, DateTime? FirstAccess, 
										int? KLocation, DateTime? TimeDisConnect, int? DisConnectReason, Int16? CMSMode, int? LastConnectTime, int? CurConnectTime, int? KGroup,
										 int? KDVRVersion, Int64? HaspLicense, Expression<Func<tDVRAddressBook, Tout>> selector, string [] includes)
		{
			ParameterExpression pram = Expression.Parameter(typeof(tDVRAddressBook), "it");
			BinaryExpression expression = null;
			BinaryExpression current = null;
			#region build conditions
			if(!string.IsNullOrEmpty(DVRGuid))
			{
				current = base.Equal<string>(pram, Defines.DVRAddressBook.DVRGuid, DVRGuid);
				expression = expression == null? current : Expression.AndAlso(expression, current);;
			}

			if(!string.IsNullOrEmpty(serverID))
			{
				current = base.Equal<string>(pram, Defines.DVRAddressBook.ServerID, serverID);
				expression = expression == null? current : Expression.AndAlso(expression, current);;
			}

			if(!string.IsNullOrEmpty(serverIP))
			{
				current = base.Equal<string>(pram, Defines.DVRAddressBook.ServerIP, serverIP);
				expression = expression == null? current : Expression.AndAlso(expression, current);;
			}

			if(!string.IsNullOrEmpty(pubServerIP))
			{
				current = base.Equal<string>(pram, Defines.DVRAddressBook.PublicServerIP, pubServerIP);
				expression = expression == null? current : Expression.AndAlso(expression, current);;
			}

			if( Online.HasValue)
			{
				current = base.Equal<int?>(pram, Defines.DVRAddressBook.Online, Online);
				expression = expression == null? current : Expression.AndAlso(expression, current);;
			}

			if( !string.IsNullOrEmpty(DVRAlias))
			{
				current = base.Equal<string>(pram, Defines.DVRAddressBook.DVRAlias, DVRAlias);
				expression = expression == null? current : Expression.AndAlso(expression, current);;
			}
			if(TotalDiskSize.HasValue)
			{
				current = base.Equal<int?>( pram, Defines.DVRAddressBook.TotalDiskSize, TotalDiskSize);
				expression = expression == null? current : Expression.AndAlso(expression, current);;
			}

			if( FreeDiskSize.HasValue)
			{
				current = base.Equal<int?>( pram, Defines.DVRAddressBook.FreeDiskSize, FreeDiskSize);
				expression = expression == null? current : Expression.AndAlso(expression, current);;
			}

			if( EnableActivation.HasValue)
			{
				current = base.Equal<int?>( pram, Defines.DVRAddressBook.EnableActivation, EnableActivation);
				expression = expression == null? current : Expression.AndAlso(expression, current);;
			}

			if( ActivationDate.HasValue)
			{
				current = base.Equal<DateTime?>( pram, Defines.DVRAddressBook.ActivationDate, ActivationDate);
				expression = expression == null? current : Expression.AndAlso(expression, current);;
			}

			if( ExpirationDate.HasValue)
			{
				current = base.Equal<DateTime?>( pram, Defines.DVRAddressBook.ExpirationDate, ExpirationDate);
				expression = expression == null? current : Expression.AndAlso(expression, current);;
			}

			if(RecordingDay.HasValue)
			{
				current = base.Equal<int?>( pram, Defines.DVRAddressBook.RecordingDay, RecordingDay);
				expression = expression == null? current : Expression.AndAlso(expression, current);;
			}

			if(FirstAccess.HasValue)
			{
				current = base.Equal<DateTime?>( pram, Defines.DVRAddressBook.FirstAccess, FirstAccess);
				expression = expression == null? current : Expression.AndAlso(expression, current);;
			}

			if(KLocation.HasValue)
			{
				current = base.Equal<int?>( pram, Defines.DVRAddressBook.KLocation, KLocation);
				expression = expression == null? current : Expression.AndAlso(expression, current);;
			}

			if( TimeDisConnect.HasValue)
			{
				current = base.Equal<DateTime?>( pram, Defines.DVRAddressBook.TimeDisConnect, TimeDisConnect);
				expression = expression == null? current : Expression.AndAlso(expression, current);;
			}

			if( DisConnectReason.HasValue)
			{
				current = base.Equal<int?>( pram, Defines.DVRAddressBook.DisConnectReason, DisConnectReason);
				expression = expression == null? current : Expression.AndAlso(expression, current);;
			}

			if( CMSMode.HasValue)
			{
				current = base.Equal<Int16?>( pram, Defines.DVRAddressBook.CMSMode, CMSMode);
				expression = expression == null? current : Expression.AndAlso(expression, current);;
			}

			if( LastConnectTime.HasValue)
			{
				current = base.Equal<int?>( pram, Defines.DVRAddressBook.LastConnectTime, LastConnectTime);
				expression = expression == null? current : Expression.AndAlso(expression, current);;
			}

			if( CurConnectTime.HasValue)
			{
				current = base.Equal<int?>( pram, Defines.DVRAddressBook.CurConnectTime, CurConnectTime);
				expression = expression == null? current : Expression.AndAlso(expression, current);;
			}

			if( KGroup.HasValue)
			{
				current = base.Equal<int?>( pram, Defines.DVRAddressBook.KGroup, KGroup);
				expression = expression == null? current : Expression.AndAlso(expression, current);;
			}

			if( KDVRVersion.HasValue)
			{
				current = base.Equal<int?>( pram, Defines.DVRAddressBook.KDVRVersion, KDVRVersion);
				expression = expression == null? current : Expression.AndAlso(expression, current);;
			}

			if( HaspLicense.HasValue)
			{
				current = base.Equal<Int64?>( pram, Defines.DVRAddressBook.HaspLicense, HaspLicense);
				expression = expression == null? current : Expression.AndAlso(expression, current);;
			}
			#endregion

			var lambda = expression == null? null : Expression.Lambda<Func<tDVRAddressBook, bool>>(expression, pram);
			return base.Query<tDVRAddressBook, Tout>(lambda, selector, includes);
		}
	}
}

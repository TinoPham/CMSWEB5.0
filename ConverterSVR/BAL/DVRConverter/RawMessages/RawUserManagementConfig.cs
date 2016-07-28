using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using PACDMModel.Model;
using SVRDatabase;

namespace ConverterSVR.BAL.DVRConverter
{
	[XmlRoot(MessageDefines.STR_Message)]
	public class RawUserManagementConfig : RawDVRConfig<RawUserManagementBody>
	{
		#region Parameter
		public const string STR_UserManagement = "user_management";
		public const string STR_Ldap = "ldap";
		public const string STR_Enable = "enable";
		public const string STR_Server = "server";
		public const string STR_Username = "username";
		public const string STR_Password = "password";
		public const string STR_UseSSL = "useSSL";
		public const string STR_SyncInterval = "sync_interval";
		public const string STR_Port = "port";
		public const string STR_LimitEntry = "limit_entry";
		public const string STR_GroupDN = "group_dn";
		public const string STR_UserDN = "user_dn";
		public const string STR_LDAPGroup = "ldap_group";
		public const string STR_Group = "group";
		public const string STR_Profile = "profile";
		//public const string STR_Group = "group";
		//public const string STR_Server = "server";
		public const string STR_Users = "users";
		//public const string STR_User = "user";
		//public const string STR_Name = "name";
		public const string STR_Description = "description";
		public const string STR_PrivilegeMask = "privilege_mask";
		public const string STR_PrivilegeValue = "privilege_value";

		public const string STR_UserName = "user_name";
		//public const string STR_Password = "password";
		public const string STR_UserType = "user_type";
		public const string STR_AllPermission = "all_permission";
		public const string STR_LivePermission = "live_permission";
		public const string STR_ItselfPermission = "itself_permission";
		public const string STR_RealTimePermission = "real_time_mode_permission";
		public const string STR_InsSearchPermission = "instant_search_permission";
		public const string STR_MuxPermission = "mux_mode_permission";
		public const string STR_ChannelPermission = "channels_permission";
		public const string STR_Channels = "channels";
		public const string STR_Channel = "channel";
		public const string STR_SearchPermission = "search_permission";
		public const string STR_DumpPermission = "dump_permission";

		public const string STR_SetupPermission = "setup_permission";
		public const string STR_AllSetupPermission = "all_setup_permission";
		public const string STR_Setups = "setups";
		public const string STR_Setup = "setup";
		public const string STR_PacPermission = "pac_permission";
		public const string STR_PtzPermission = "ptz_permission";
		public const string STR_PanicPermission = "panic_permission";
		public const string STR_ToDisplayPermission = "to_display_permission";
		public const string STR_AutoLogout = "auto_logout";
		//public const string STR_Enable = "enable";
		public const string STR_TimeOut = "time_out";
		#region unused
		//[XmlElement(STR_Header)]
		//public RawMsgHeader msgHeader { get; set; }

		//[XmlElement(STR_Body)]
		//public RawUserManagementBody msgBody { get; set; }

		//List<tDVRChannel> _channellst;
		//List<tDVRUserPrivSetup> _lstUsersSetups;
		//List<tDVRUserPrivOp> _lstUsersOperations;
		//List<tDVRUserOperation> _userOps;
		//List<tDVRUserPrivChannel> _lstUsersChans;
		//List<tDVRSetupPage> _lstUserPage;
		//List<tDVRUserPrivOp> _lstUserPrivOpWork;
		#endregion
		#endregion
		public override void SetEvnVars(PACDMModel.PACDMDB pacDB, SVRManager Logdb, ConvertMessage.MessageDVRInfo dvrinfo)
		{
			base.SetEvnVars(pacDB, Logdb, dvrinfo);
			if( DVRAdressBook == null)
				return;
			db.Include<tDVRAddressBook, tDVRChannels>(DVRAdressBook,item => item.tDVRChannels);
			
		}
		public override async Task<Commons.ERROR_CODE> UpdateToDB()
		{
			if (DVRAdressBook == null)
				return await base.UpdateToDB();
			if(DVRAdressBook.tDVRChannels == null || DVRAdressBook.tDVRChannels.Count == 0)
			{
				ReconnectFlag  = true;
				return await base.UpdateToDB();
			}
			
			int ret = 0;
			if( UpdateUserconfig( DVRAdressBook) )
			{
				if( ( ret = db.Save() ) == -1 )
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			 }
			//LDapGroup
			
			if( UpdateLdapGroups(DVRAdressBook))
			{
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);

			}

			if( UpdateUsers(base.DVRAdressBook))
			{
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}

			if (UpdateChecksum((int)DVR_CONFIGURATION.EMS_CFG_MANAGE_USERS, msgBody.msgCommon))
			{
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}
			return await base.UpdateToDB();
		}

		private bool UpdateUserconfig(tDVRAddressBook dvradd)
		{
			tDVRUserConfig DBuserCfg = db.FirstOrDefault<tDVRUserConfig>(item => item.KDVR == dvradd.KDVR);

			if (DBuserCfg == null) //insert 
			{
				DBuserCfg = msgBody.UMData.ToEntity();
				DBuserCfg.tDVRAddressBook = this.DVRAdressBook;
				db.Insert<tDVRUserConfig>(DBuserCfg);
				return true;
			}
			else //update
			{

				if (!msgBody.UMData.Equal(DBuserCfg))
				{
					msgBody.UMData.SetEntity(ref DBuserCfg);
					db.Update<tDVRUserConfig>(DBuserCfg);
					return  true;
				}
				return false;
			}
		}

		private bool UpdateLdapGroups(tDVRAddressBook dvradd)
		{
			if( msgBody.UMData == null || msgBody.UMData.ldap == null || msgBody.UMData.ldap.Groups == null)
				return false;
			List<tDVRLDAPGroup> DBLDapGroups = db.Query<tDVRLDAPGroup>(x => x.KDVR == dvradd.KDVR).ToList();

			List<LDAPGroupInfo> msgLDaps = msgBody.UMData.ldap.Groups;//MessageLdapList();
			Func<tDVRLDAPGroup, LDAPGroupInfo, bool> func_filter = (dbdata, msg) => msg.Profile.Trim().Equals(dbdata.Profile.Trim());
			Func<tDVRLDAPGroup, LDAPGroupInfo, bool> compare_update = null;
			Expression<Func<tDVRLDAPGroup, object>> updatedata	= dbitem => dbitem.KDVR;
			Expression<Func<tDVRLDAPGroup, string>> db_key = dbitem => dbitem.Profile;
			Expression<Func<LDAPGroupInfo, string>> info_key = info => info.Profile;
			return base.UpdateDBData<tDVRLDAPGroup, LDAPGroupInfo, string, string>(DBLDapGroups, msgLDaps, func_filter, compare_update, updatedata, dvradd.KDVR, db_key, info_key);

			//var match = from msg in msgLDaps
			//			from dbdata in DBLDapGroups
			//			where string.Compare(msg.Profile, dbdata.Profile, true) == 0
			//			select new { DBlDap = dbdata, msgldap = msg };
			////update exist data
			//List<int> kgroup = new List<int>();
			//List<string> profile = new List<string>();
			//bool ret = false;
			//tDVRLDAPGroup dbupdate = null;
			//foreach (var item in match)
			//{
			//	kgroup.Add(item.DBlDap.KGroup);
			//	profile.Add(item.msgldap.Profile);

			//	dbupdate = item.DBlDap;
			//	if (!item.msgldap.Equal( dbupdate) )
			//		continue;

			//	item.msgldap.SetEntity( ref dbupdate);
			//	db.Update<tDVRLDAPGroup>(item.DBlDap);
			//	ret = true;
				
			//}
			////delete unused
			//var unused = DBLDapGroups.Where(item => !kgroup.Contains(item.KGroup));
			//foreach (var item in unused)
			//{
			//	db.Delete<tDVRLDAPGroup>(item);
			//	ret = true;
			//}
			////add new
			//var newitem = msgLDaps.Where(item => !profile.Contains(item.Profile));
			//tDVRLDAPGroup dbitem = null;
			//foreach (var item in newitem)
			//{
			//	item.SetEntity( ref dbitem);
			//	dbitem.KDVR = dvradd.KDVR;
			//	db.Insert<tDVRLDAPGroup>(dbitem);
			//	ret = true;
			//}
			//return ret;
		}

		private bool UpdateUsers(tDVRAddressBook dvradd)
		{
			if ( dvradd.tDVRChannels == null || dvradd.tDVRChannels.Count == 0|| msgBody.UMData == null || msgBody.UMData.Users == null) 
				return false;

			List<UserInfo> msgUsers = msgBody.UMData.Users;
			db.Include<tDVRAddressBook, tDVRUsers>( dvradd, item => item.tDVRUsers);
			ICollection<tDVRUsers> dvrUsers = dvradd.tDVRUsers.ToList();  //db.Query<tDVRUser>(user => user.KDVR == dvradd.KDVR).ToList();
			bool ret = false;
			if(msgUsers.Count == 0)
			{
				foreach( tDVRUsers dvruser in dvrUsers)
				{
					DeleteDvrUser(dvruser);
					ret= true;
					Thread.Sleep(Time_Loop_Delay);
				 }
				return ret;
			}

			ICollection<tDVRChannels> dvrChannels = dvradd.tDVRChannels;  //db.Query<tDVRChannel>(t => t.KDVR == dvradd.KDVR).ToList();
			List<tDVRSetupPages> dvrsetuppages = db.Query<tDVRSetupPages>().ToList();
			List<tDVRUserOperation> dvrOPList = db.Query<tDVRUserOperation>().ToList();
			
			var match = from userinfo in msgUsers
						from dvruser in dvrUsers
						where string.Compare(userinfo.UserName, dvruser.UserName, false) == 0
						select new{ MSGUser = userinfo,
								DVRUser = dvruser };
			tDVRUsers tdvruser = null;
			//List<int>matchId = new List<int>();
			List<string> username = new List<string>();
			foreach( var item in match)
			{
				tdvruser = item.DVRUser;
				//matchId.Add(tdvruser.KUser);
				username.Add(tdvruser.UserName);
				if (!item.MSGUser.Equal(tdvruser))
				{
					tdvruser.Password = item.MSGUser.Password;
					tdvruser.Usertype = item.MSGUser.UserType;
					db.Update<tDVRUsers>(tdvruser);
					ret = true;
				}

				db.Include<tDVRUsers, tDVRUserPrivSetup>(tdvruser, child => child.tDVRUserPrivSetup);
				db.Include<tDVRUsers, tDVRUserPrivOp>(tdvruser, child => child.tDVRUserPrivOp);
				db.Include<tDVRUsers, tDVRUserPrivChannel>(tdvruser, child => child.tDVRUserPrivChannel);
				ret |= UpdateDVRUser(item.MSGUser, item.DVRUser, dvrsetuppages, dvrChannels, dvrOPList);
				Thread.Sleep(Time_Loop_Delay);
			}
			//delete unused user
			var deleteUsers = dvrUsers.Where(dvruser => !username.Contains(dvruser.UserName));
			foreach( tDVRUsers del_user in deleteUsers)
			{
				DeleteDvrUser(del_user);
				ret = true;
			}
			//add new user
			var add_users = msgUsers.Where( userinfo => !username.Contains( userinfo.UserName));
			foreach( UserInfo add_user in add_users)
			{
				tdvruser = new tDVRUsers{ UserName = add_user.UserName,
										Password = add_user.Password,
										Usertype = add_user.UserType,
										tDVRAddressBook = dvradd};
				db.Insert<tDVRUsers>(tdvruser);
				UpdateDVRUser(add_user, tdvruser, dvrsetuppages, dvrChannels, dvrOPList);
				ret = true;
				Thread.Sleep(Time_Loop_Delay);
			}
			return ret;
			
		}
		
		private void DeleteDvrUser(tDVRUsers dvruser)
		{
			db.DeleteWhere<tDVRUserPrivChannel>( item => item.KUser == dvruser.KUser );
			db.DeleteWhere<tDVRUserPrivOp>(item => item.KUser == dvruser.KUser);
			db.DeleteWhere<tDVRUserPrivSetup>(item => item.KUser == dvruser.KUser);
			db.Delete<tDVRUsers>(dvruser);
		}
		private bool UpdateDVRUser(UserInfo msguser, tDVRUsers dvruser, List<tDVRSetupPages> dvrsetuppages, ICollection<tDVRChannels> dvrchannels, List<tDVRUserOperation> dvrOPList)
		{
			if( dvruser == null || msguser.Permissions == null)
				return false;

			bool ret = LiveSearchPermission(msguser.Permissions.Live, msguser.Permissions.Search, dvruser, dvrchannels, dvrOPList);
			ret |= SetupPermissionInfo(msguser.Permissions.Setup, dvruser,dvrsetuppages, dvrOPList);
			ret |= UpdateSelfPermissionInfo(msguser.Permissions.PAC, dvruser, dvrOPList);
			ret |= UpdateSelfPermissionInfo(msguser.Permissions.PTZ, dvruser, dvrOPList);
			ret |= UpdateSelfPermissionInfo(msguser.Permissions.Panic, dvruser, dvrOPList);
			ret |= UpdateSelfPermissionInfo(msguser.Permissions.ToDisplay, dvruser, dvrOPList);
			return ret;
		}

		private bool LiveSearchPermission(LivePermissionInfo liveinfo,SearchPermissionInfo searchinfo, tDVRUsers dvruser, ICollection<tDVRChannels> dvrchannels, List<tDVRUserOperation> dvrOPList)
		{
			bool ret = UpdateUserpriv(liveinfo.InsSearch, dvruser, dvrOPList);
			ret |= UpdateUserpriv(liveinfo.Mux, dvruser, dvrOPList);
			ret |= UpdateUserpriv(liveinfo.Permits, dvruser, dvrOPList);
			ret |= UpdateUserpriv(liveinfo.RealTime, dvruser, dvrOPList);
			//live search operation channel in DB always 'Channel'=> min Kop for first KOP for live and  next Kop for search
			ChannelPermissionInfo live_channels = liveinfo.Channels;
			UserPrivInfo privInfo = live_channels.Permits;
			tDVRUserOperation OP = null;
			if( privInfo != null)
			{
				OP = dvrOPList.FirstOrDefault(item => string.Compare(item.OpName, privInfo.Name, true) == 0);
				ret |= UpdateUserpriv(live_channels.Permits, dvruser, OP);
			}

			ret |= UpdateUserpriv(searchinfo.Dump, dvruser, dvrOPList);
			ret |= UpdateUserpriv(searchinfo.Permits, dvruser, dvrOPList);

			ChannelPermissionInfo search_channels = searchinfo.Channels;
			privInfo = search_channels.Permits;
			if( privInfo != null)
			{
				OP = dvrOPList.FirstOrDefault(item => string.Compare(item.OpName, privInfo.Name, true) == 0 && item.KOp > OP.KOp);
				ret |= UpdateUserpriv(search_channels.Permits, dvruser, OP);
			}
			for(int i = 0; i< search_channels.Channels.Count; i++)
			{
				ret |= UpdateChanelPriv(live_channels.Channels[i], search_channels.Channels[i], dvruser, dvrchannels);
				Thread.Sleep(Time_Loop_Delay);
			}
			return ret;
		}

		private bool UpdateChanelPriv(UserPrivInfo live, UserPrivInfo search, tDVRUsers dvruser, ICollection<tDVRChannels> dvrchanels)
		{
			tDVRChannels dvrchannel = dvrchanels.FirstOrDefault( chan => string.Compare( chan.Name, live.Name, true) == 0);
			if (dvrchannel == null)
			{
				return false;
			}
				else
			{
				tDVRUserPrivChannel userchanpriv =
					dvruser.tDVRUserPrivChannel.FirstOrDefault(chan => dvrchannel != null && chan.KChannel == dvrchannel.KChannel);
				bool ret = false;
				if (userchanpriv == null)
				{
					userchanpriv = new tDVRUserPrivChannel();
					userchanpriv.tDVRChannels = dvrchannel;
					userchanpriv.tDVRUsers = dvruser;
					userchanpriv.DescriptionLive = live.Description;
					userchanpriv.KPrivilegeLive = (short) live.PrivilegeValue;
					userchanpriv.MaskLive = live.PrivilegeMask;
					userchanpriv.MaskSearch = search.PrivilegeMask;
					userchanpriv.KPrivilegeSearch = (short) search.PrivilegeValue;
					userchanpriv.DescriptionSearch = search.Description;
					db.Insert<tDVRUserPrivChannel>(userchanpriv);
					ret = true;
				}
				else
				{
					if (live.PrivilegeMask != userchanpriv.MaskLive || live.PrivilegeValue != userchanpriv.KPrivilegeLive ||
					    string.Compare(live.Description, userchanpriv.DescriptionLive, false) != 0
					    || search.PrivilegeValue != userchanpriv.KPrivilegeSearch || search.PrivilegeMask != userchanpriv.MaskSearch ||
					    string.Compare(search.Description, userchanpriv.DescriptionSearch, false) != 0)
					{
						userchanpriv.DescriptionLive = live.Description;
						userchanpriv.KPrivilegeLive = (short) live.PrivilegeValue;
						userchanpriv.MaskLive = live.PrivilegeMask;
						userchanpriv.MaskSearch = search.PrivilegeMask;
						userchanpriv.KPrivilegeSearch = (short) search.PrivilegeValue;
						userchanpriv.DescriptionSearch = search.Description;
						db.Update<tDVRUserPrivChannel>(userchanpriv);
						ret = true;
					}
					ret = false;
				}
				return ret;
			}
		}
		
		private bool SetupPermissionInfo(SetupPermissionInfo setupinfo, tDVRUsers dvruser,List<tDVRSetupPages> dvrsetuppages, List<tDVRUserOperation> dvrOPList)
		{
			bool ret = false;
			ret |= UpdateUserpriv(setupinfo.Permits, dvruser, dvrOPList);
			AllSetupPermissionInfo setups = setupinfo.AllSetup;
			ret |= UpdateUserpriv(setups.Permits, dvruser, dvrOPList);
			List<UserPrivInfo> infos = setups.Setups;
			foreach(UserPrivInfo item in infos)
			{
				ret |= UpdatesetupInfo(item, dvruser, dvrsetuppages);
				Thread.Sleep(Time_Loop_Delay);
			}
			return ret;

		}

		private bool UpdatesetupInfo(UserPrivInfo userinfo, tDVRUsers dvruser, List<tDVRSetupPages> dvrsetuppages)
		{
			tDVRSetupPages page = dvrsetuppages.FirstOrDefault( item => string.Compare( item.SetupName, userinfo.Name, true) == 0);
			tDVRUserPrivSetup dvrsetupPriv = dvruser.tDVRUserPrivSetup.FirstOrDefault( item => item.KSetupPage == page.KSetupPage);
			if( dvrsetupPriv == null)
			{
				userinfo.SetEntity(ref dvrsetupPriv);
				dvrsetupPriv.tDVRUsers = dvruser;
				dvrsetupPriv.tDVRSetupPages = page;
				db.Insert<tDVRUserPrivSetup>(dvrsetupPriv);
				return true;
			}
			else
			{
				if( !userinfo.Equal(dvrsetupPriv))
				{
					userinfo.SetEntity(ref dvrsetupPriv);
					db.Update<tDVRUserPrivSetup>(dvrsetupPriv);
					return true;
				} 
				return false;
			}

		}

		private bool UpdateUserpriv(UserPrivInfo userprivinfo, tDVRUsers dvruser, tDVRUserOperation dvrOP)
		{
			if (userprivinfo == null || dvrOP == null)
				return false;

			tDVRUserPrivOp OPPriv = dvruser.tDVRUserPrivOp.FirstOrDefault(item => item.KOp == dvrOP.KOp);
			if (OPPriv == null)//insert new OPPriv
			{
				userprivinfo.SetEntity(ref OPPriv);
				OPPriv.tDVRUsers = dvruser;
				OPPriv.tDVRUserOperation = dvrOP;
				db.Insert<tDVRUserPrivOp>(OPPriv);
				return true;
			}
			else
			{
				if (!userprivinfo.Equal(OPPriv))
				{
					userprivinfo.SetEntity(ref OPPriv);
					db.Update<tDVRUserPrivOp>(OPPriv);
					return true;
				}
				return false;

			}

		}

		private bool UpdateUserpriv(UserPrivInfo userprivinfo, tDVRUsers dvruser, List<tDVRUserOperation> dvrOPList)
		{
			if( userprivinfo == null)
				return false;
			tDVRUserOperation OP = dvrOPList.FirstOrDefault( item => string.Compare(item.OpName, userprivinfo.Name, true) == 0);
			return UpdateUserpriv( userprivinfo, dvruser, OP);
			
		}

		private bool UpdateSelfPermissionInfo(SelfPermissionInfo seftinfo, tDVRUsers dvruser, List<tDVRUserOperation> dvrOPList)
		{
			if( seftinfo == null)
				return false;
			return UpdateUserpriv(seftinfo.Permits, dvruser, dvrOPList);
		}

		#region unused
		/*
		private List<tDVRUser> GetGlobalList(int kDvr)
		{
			List<tDVRUser> lstUsers = db.Query<tDVRUser>(x => x.KDVR == kDvr).ToList(); //.OrderBy(x => x.KUser).ToList();

			tDVRUserPrivOp op = lstUsers.FirstOrDefault().tDVRUserPrivOps.FirstOrDefault();
			List<int> userlst = lstUsers.Select(t => t.KUser).ToList();
			_lstUsersSetups = db.Query<tDVRUserPrivSetup>(x => userlst.Contains(x.KUser)).ToList();
			_lstUsersOperations = db.Query<tDVRUserPrivOp>(x => userlst.Contains(x.KUser)).ToList();
			_userOps = db.Query<tDVRUserOperation>().ToList();
			_lstUsersChans = db.Query<tDVRUserPrivChannel>(x => userlst.Contains(x.KUser)).ToList();
			return lstUsers;
		}

		private bool CheckDvrSync()
		{
			if (_channellst.Count == 0)
			{
				SetDvrReconnect(msgHeader.DVRGuid, reconnectFlag: true);
				return true;
			}
			return false;
		}

		private List<UserInfo> MsgUserInfo()
		{
			List<UserInfo> uInfoList = IsUsersData() ? msgBody.UMData.Users : new List<UserInfo>();
			return uInfoList;
		}

		private bool IsSetupData(UserInfo userInfo)
		{
			if (!IsPermisionData(userInfo)) return false;
			return userInfo.Permissions.Setup != null;
		}

		private bool IsAllSetupData(UserInfo userInfo)
		{
			if (!IsSetupData(userInfo)) return false;
			return userInfo.Permissions.Setup.AllSetup != null;
		}

		private bool IsSetupInfoData(UserInfo userInfo)
		{
			if (!IsAllSetupData(userInfo)) return false;
			return userInfo.Permissions.Setup.AllSetup.Setups != null;
		}

		private List<UserPrivInfo> GetUserPrivList(UserInfo userInfo)
		{
			List<UserPrivInfo> userPrivLst;
			if (IsSetupInfoData(userInfo))
			{
				userPrivLst = userInfo.Permissions.Setup.AllSetup.Setups;

			}
			else
			{
				userPrivLst = new List<UserPrivInfo>();
			}
			return userPrivLst;
		}

		private void UpdateUserPrivilegeSetup(tDVRUser user)
		{
			List<tDVRUserPrivSetup> lstUsersSetup = _lstUsersSetups.Where(x => x.KUser == user.KUser).ToList();//.OrderBy(x => x.KSetupPage).ToList();

			UserInfo userInfo = GetUserList().FirstOrDefault(x => x.UserName.ToUpper() == user.UserName.ToUpper());

			List<UserPrivInfo> userPrivLst = GetUserPrivList(userInfo);

			foreach (var userPrivSetup in lstUsersSetup)
			{
				tDVRUserPrivSetup userPriv = userPrivSetup;
				tDVRSetupPage sup = _lstUserPage.FirstOrDefault(t => t.KSetupPage == userPriv.KSetupPage);
				UserPrivInfo userPrivInfo = userPrivLst.FirstOrDefault(x => x.Name.Trim().ToUpper() == sup.SetupName.Trim().ToUpper());
				
				if (userPrivInfo != null)
				{
					if (!ComparetUserPrivilegeSetup(user, userPriv, userPrivInfo, sup))
					{
						SetUserPrivilegeSetup(user, ref userPriv, userPrivInfo, sup);
						db.Update<tDVRUserPrivSetup>(userPriv);
					}
					userPrivLst.Remove(userPrivInfo);
				}
				else
				{
					db.Delete<tDVRUserPrivSetup>(userPriv);
				}
			}

			foreach (var userPrivInfo in userPrivLst)
			{
				var usersSetup = new tDVRUserPrivSetup();
				tDVRSetupPage sup = _lstUserPage.FirstOrDefault(t => t.SetupName.Trim().ToUpper() == userPrivInfo.Name.Trim().ToUpper());
				SetUserPrivilegeSetup(user, ref usersSetup, userPrivInfo, sup);
				db.Insert<tDVRUserPrivSetup>(usersSetup);
			}

		}

		private void SetUserPrivilegeSetup(tDVRUser user, ref tDVRUserPrivSetup usersSetup, UserPrivInfo userPrivInfo, tDVRSetupPage sup)
		{
			usersSetup.tDVRUser = user;
			usersSetup.Description = userPrivInfo.Description;
			usersSetup.KPrivilege = (short)userPrivInfo.PrivilegeValue;
			usersSetup.Mask = userPrivInfo.PrivilegeMask;
			usersSetup.KSetupPage = sup.KSetupPage;
		}

		private bool ComparetUserPrivilegeSetup(tDVRUser user, tDVRUserPrivSetup usersSetup, UserPrivInfo userPrivInfo,
			tDVRSetupPage sup)
		{
			bool result = usersSetup.KUser == user.KUser &&
			              usersSetup.Description == userPrivInfo.Description &&
			              usersSetup.KPrivilege == (short) userPrivInfo.PrivilegeValue &&
			              usersSetup.Mask == userPrivInfo.PrivilegeMask &&
			              usersSetup.KSetupPage == sup.KSetupPage;
			return result;
		}

		private void UpdateUserPrivilegeOperation(tDVRUser user)
		{
			List<tDVRUserPrivOp> lstUsersOperation = _lstUsersOperations.Where(x => x.KUser == user.KUser).ToList();//.OrderBy(x => x.KOp).ToList();
			_lstUserPrivOpWork = new List<tDVRUserPrivOp>();

			var useroption = msgBody.UMData.Users.FirstOrDefault(x => x.UserName.ToUpper() == user.UserName.ToUpper());
			if (useroption == null) return;

			var useroperation = new tDVRUserOperation();
			var op1 = useroption.Permissions.ToDisplay;
			if (op1 != null)
				useroperation = _userOps.FirstOrDefault(x => x.OpName.Trim().ToUpper() == op1.Permits.Name.Trim().ToUpper());
			AddUserPrivOp(useroperation, op1.Permits, user);
			
			var op2 = useroption.Permissions.Setup.Permits;
			if (op2 != null)
				useroperation = _userOps.FirstOrDefault(x => x.OpName.Trim().ToUpper() == op2.Name.Trim().ToUpper());
			AddUserPrivOp(useroperation, op2, user);

			var op3 = useroption.Permissions.Setup.AllSetup.Permits;
			if (op3 != null)
				useroperation = _userOps.FirstOrDefault(x => x.OpName.Trim().ToUpper() == op3.Name.Trim().ToUpper());
			AddUserPrivOp(useroperation, op3, user);

			var op4 = useroption.Permissions.Search.Permits;
			if (op4 != null)
				useroperation = _userOps.FirstOrDefault(x => x.OpName.Trim().ToUpper() == op4.Name.Trim().ToUpper());
			AddUserPrivOp(useroperation, op4, user);

			//var op5 = useroption.Permissions.Search.Channels.Permits;
			//if (op5 != null)
			//useroperation = db.Query<tDVRUserOperation>(x => x.OpName.Trim().ToUpper() == op5.Name.Trim().ToUpper()).FirstOrDefault();
			//AddUserPrivOp(useroperation, op5, user.KUser);

			var op6 = useroption.Permissions.Search.Dump;
			if (op6 != null)
				useroperation = _userOps.FirstOrDefault(x => x.OpName.Trim().ToUpper() == op6.Name.Trim().ToUpper());
			AddUserPrivOp(useroperation, op6, user);

			var op7 = useroption.Permissions.PTZ.Permits;
			if (op7 != null)
				useroperation = _userOps.FirstOrDefault(x => x.OpName.Trim().ToUpper() == op7.Name.Trim().ToUpper());
			AddUserPrivOp(useroperation, op7, user);

			var op8 = useroption.Permissions.Panic.Permits;
			if (op8 != null)
				useroperation = _userOps.FirstOrDefault(x => x.OpName.Trim().ToUpper() == op8.Name.Trim().ToUpper());
			AddUserPrivOp(useroperation, op8, user);

			var op9 = useroption.Permissions.PAC.Permits;
			if (op9 != null)
				useroperation = _userOps.FirstOrDefault(x => x.OpName.Trim().ToUpper() == op9.Name.Trim().ToUpper());
			AddUserPrivOp(useroperation, op9, user);

			var op10 = useroption.Permissions.Live.Channels.Permits;
			if (op10 != null)
				useroperation = _userOps.FirstOrDefault(x => x.OpName.Trim().ToUpper() == op10.Name.Trim().ToUpper());
			AddUserPrivOp(useroperation, op10, user);

			var op11 = useroption.Permissions.Live.InsSearch;
			if (op11 != null)
				useroperation = _userOps.FirstOrDefault(x => x.OpName.Trim().ToUpper() == op11.Name.Trim().ToUpper());
			AddUserPrivOp(useroperation, op11, user);

			var op12 = useroption.Permissions.Live.Mux;
			if (op12 != null)
				useroperation = _userOps.FirstOrDefault(x => x.OpName.Trim().ToUpper() == op12.Name.Trim().ToUpper());
			AddUserPrivOp(useroperation, op12, user);

			var op13 = useroption.Permissions.Live.Permits;
			if (op13 != null)
				useroperation = _userOps.FirstOrDefault(x => x.OpName.Trim().ToUpper() == op13.Name.Trim().ToUpper());
			AddUserPrivOp(useroperation, op13, user);

			var op14 = useroption.Permissions.Live.RealTime;
			if (op14 != null)
				useroperation = _userOps.FirstOrDefault(x => x.OpName.Trim().ToUpper() == op14.Name.Trim().ToUpper());
			AddUserPrivOp(useroperation, op14, user);

			foreach (var uPrivOp in lstUsersOperation)
			{
				tDVRUserPrivOp privOp = uPrivOp;
				tDVRUserPrivOp priOpInfo = _lstUserPrivOpWork.FirstOrDefault(x => x.KUser == privOp.KUser && x.KOp == privOp.KOp);
				if (priOpInfo != null)
				{
					if (!CompareUserPrivOp(privOp, priOpInfo))
					{
						privOp = CopyUserPrivOp(priOpInfo, privOp);
						db.Update<tDVRUserPrivOp>(privOp);
					}
					_lstUserPrivOpWork.Remove(priOpInfo);
				}
				else
				{
					db.Delete<tDVRUserPrivOp>(privOp);
				}
			}


			foreach (var uso in _lstUserPrivOpWork)
			{
				db.Insert<tDVRUserPrivOp>(uso);
			}

		}

		private tDVRUserPrivOp CopyUserPrivOp(tDVRUserPrivOp sourUser, tDVRUserPrivOp densUser)
		{
			densUser.Description = sourUser.Description;
			densUser.KOp = sourUser.KOp;
			densUser.KPrivilege = sourUser.KPrivilege;
			densUser.KUser = sourUser.KUser;
			densUser.Mask = sourUser.Mask;
			return densUser;
		}

		private bool CompareUserPrivOp(tDVRUserPrivOp user1, tDVRUserPrivOp user2)
		{
			bool result = user1.Description == user2.Description &&
			              user1.KOp == user2.KOp &&
			              user1.KPrivilege == user2.KPrivilege &&
			              user1.KUser == user2.KUser &&
			              user1.Mask == user2.Mask;
			return result;
		}

		private void AddUserPrivOp(tDVRUserOperation useroperation, UserPrivInfo op, tDVRUser user)
		{
			var userop = new tDVRUserPrivOp
			{
				Description = op.Description,
				KOp = useroperation.KOp,
				KPrivilege = op.PrivilegeValue,
				tDVRUser = user,
				Mask = op.PrivilegeMask
			};
			_lstUserPrivOpWork.Add(userop);
			//db.Insert<tDVRUserPrivOp>(userop);
		}

		private bool IsBodyData()
		{
			return msgBody != null;
		}

		private bool IsUmData()
		{
			if (!IsBodyData()) return false;
			return msgBody.UMData != null;
		}

		private bool IsUsersData()
		{
			if (!IsUmData()) return false;
			return msgBody.UMData.Users != null;
		}

		private bool IsPermisionData(UserInfo user)
		{
			if (user == null) return false;
			return user.Permissions != null;
		}

		private bool IsLiveData(UserInfo user)
		{
			if (!IsPermisionData(user)) return false;
			return user.Permissions.Live != null;
		}

		private bool IsLiveChannelsData(UserInfo user)
		{
			if (!IsLiveData(user)) return false;
			return user.Permissions.Live.Channels != null;
		}

		private bool IsLiveChannelInfoData(UserInfo user)
		{
			if (!IsLiveChannelsData(user)) return false;
			return user.Permissions.Live.Channels.Channels != null;
		}

		private bool IsSearchData(UserInfo user)
		{
			if (!IsPermisionData(user)) return false;
			return user.Permissions.Search != null;
		}

		private bool IsSearchChannelsData(UserInfo user)
		{
			if (!IsSearchData(user)) return false;
			return user.Permissions.Search.Channels != null;
		}

		private bool IsSearchChannelInfoData(UserInfo user)
		{
			if (!IsSearchChannelsData(user)) return false;
			return user.Permissions.Search.Channels.Channels != null;
		}

		private List<UserInfo> GetUserList()
		{
			List<UserInfo> userlist = IsUsersData() ? msgBody.UMData.Users : new List<UserInfo>();
			return userlist;
		}

		private List<UserPrivInfo> GetLiveUserChannelList(UserInfo userInfo)
		{
			List<UserPrivInfo> liveList = IsLiveChannelInfoData(userInfo) ? userInfo.Permissions.Live.Channels.Channels.ToList() : new List<UserPrivInfo>();
			return liveList;
		}

		private List<UserPrivInfo> GetSearchUserChannelList(UserInfo userInfo)
		{
			List<UserPrivInfo> seachList = IsSearchChannelInfoData(userInfo) ? userInfo.Permissions.Search.Channels.Channels.ToList() : new List<UserPrivInfo>();
			return seachList;
		}

		private List<tDVRUserPrivChannel> GetUserPrivChanels(tDVRUser user)
		{
			UserInfo userInfo = GetUserList().FirstOrDefault(x => x.UserName.ToUpper() == user.UserName.ToUpper());

			List<UserPrivInfo> liveList = GetLiveUserChannelList(userInfo);
			List<UserPrivInfo> seachList = GetSearchUserChannelList(userInfo);

			tDVRChannel cn;
			var userPrivChanelList = new List<tDVRUserPrivChannel>();
			foreach (var live in liveList)
			{
				cn = _channellst.FirstOrDefault(x => x.Name.Trim().ToUpper() == live.Name.Trim().ToUpper() && x.KDVR == user.KDVR);
				AddUserPrivChannel(live, cn, user, ref userPrivChanelList);
			}

			foreach (var search in seachList)
			{
				cn = _channellst.FirstOrDefault(x => x.Name.Trim().ToUpper() == search.Name.Trim().ToUpper() && x.KDVR == user.KDVR);
				AddUserPrivChannelSearch(search, cn, user, ref userPrivChanelList);
			}

			return userPrivChanelList;
		}

		private bool CompareUserPrivChannel(tDVRUserPrivChannel priv1, tDVRUserPrivChannel priv2)
		{
			bool result = priv1.DescriptionLive == priv2.DescriptionLive &&
			              priv1.DescriptionSearch == priv2.DescriptionSearch &&
			              priv1.KChannel == priv2.KChannel &&
			              priv1.KPrivilegeLive == priv2.KPrivilegeLive &&
			              priv1.KUser == priv2.KUser &&
			              priv1.MaskLive == priv2.MaskLive &&
			              priv1.MaskSearch == priv2.MaskSearch;
			return result;
		}

		private tDVRUserPrivChannel SetUserPrivChannel(tDVRUserPrivChannel sourceChannel, tDVRUserPrivChannel denstinationChannel)
		{
			denstinationChannel.DescriptionLive = sourceChannel.DescriptionLive;
			denstinationChannel.DescriptionSearch = sourceChannel.DescriptionSearch;
			denstinationChannel.KChannel = sourceChannel.KChannel;
			denstinationChannel.KPrivilegeLive = sourceChannel.KPrivilegeLive;
			denstinationChannel.KUser = sourceChannel.KUser;
			denstinationChannel.MaskLive = sourceChannel.MaskLive;
			denstinationChannel.MaskSearch = sourceChannel.MaskSearch;
			return denstinationChannel;
		}

		private void UpdateUserPrivilegeChanels(tDVRUser user)
		{
			List<tDVRUserPrivChannel> lstUserschanel = _lstUsersChans.Where(x => x.KUser == user.KUser).ToList();//.OrderBy(x => x.KChannel).OrderBy(x => x.KChannel).ToList();
			
			List<tDVRUserPrivChannel> userPrivChanelList = GetUserPrivChanels(user);

			foreach (var uso in lstUserschanel)
			{
				tDVRUserPrivChannel priv = uso;
				tDVRUserPrivChannel prichanel = userPrivChanelList.FirstOrDefault(x => x.KChannel == uso.KChannel && x.KUser == uso.KUser);
				if (prichanel != null)
				{
					if (!CompareUserPrivChannel(priv, prichanel))
					{
						priv = SetUserPrivChannel(prichanel, priv);
						db.Update<tDVRUserPrivChannel>(priv);
					}
					userPrivChanelList.Remove(prichanel);
				}
				else
				{
					db.Delete<tDVRUserPrivChannel>(priv);
				}
			}

			foreach (var prichanel in userPrivChanelList)
			{
				db.Insert<tDVRUserPrivChannel>(prichanel);
			}
		}

		private void AddUserPrivChannelSearch(UserPrivInfo priChannel, tDVRChannel cn, tDVRUser user, ref List<tDVRUserPrivChannel> userPrivChanelList)
		{
			tDVRUserPrivChannel userchannel = userPrivChanelList.FirstOrDefault(x => x.KChannel == cn.KChannel);

			if (userchannel == null)
			{
				userchannel  = new tDVRUserPrivChannel();
				userchannel.DescriptionSearch = priChannel.Description;
				userchannel.KChannel = cn.KChannel;
				userchannel.KPrivilegeSearch = (short)priChannel.PrivilegeValue;
				userchannel.tDVRUser = user;
				userchannel.MaskSearch = priChannel.PrivilegeMask;
				userPrivChanelList.Add(userchannel);
			}
			else
			{
				userPrivChanelList.Remove(userchannel);
				userchannel.DescriptionSearch = priChannel.Description;
				userchannel.KChannel = cn.KChannel;
				userchannel.KPrivilegeSearch = (short)priChannel.PrivilegeValue;
				userchannel.tDVRUser = user;
				userchannel.MaskSearch = priChannel.PrivilegeMask;
				userPrivChanelList.Add(userchannel);
			}
			
		}

		private void AddUserPrivChannel(UserPrivInfo priChannel, tDVRChannel cn, tDVRUser user, ref List<tDVRUserPrivChannel> userPrivChanelList)
		{
			var userchannel = new tDVRUserPrivChannel
			{
				DescriptionLive = priChannel.Description,
				KChannel = cn.KChannel,
				KPrivilegeLive = (short) priChannel.PrivilegeValue,
				tDVRUser = user,
				MaskLive = priChannel.PrivilegeMask
			};
			userPrivChanelList.Add(userchannel);
		}

		private void DeleteUserPrivilegeSetup(tDVRUser user)
		{
			List<tDVRUserPrivSetup> lstUsersSetup = _lstUsersSetups.Where(x => x.KUser == user.KUser).ToList();

			foreach (var usersSetup in lstUsersSetup)
			{
				db.Delete<tDVRUserPrivSetup>(usersSetup);
			}
		}

		private void DeleteUserPrivilegeOperation(tDVRUser user)
		{
			List<tDVRUserPrivOp> lstUsersOperation = _lstUsersOperations.Where(x => x.KUser == user.KUser).ToList();//.OrderBy(x => x.KOp).ToList();
			foreach (var uso in lstUsersOperation)
			{
				db.Delete<tDVRUserPrivOp>(uso);
			}
		}

		private void DeleteUserPrivilegeChanel(tDVRUser user)
		{
			List<tDVRUserPrivChannel> lstUsersPrivilegeChanel = _lstUsersChans.Where(x => x.KUser == user.KUser).ToList();//.OrderBy(x => x.KUser).ToList();
			foreach (var pri in lstUsersPrivilegeChanel)
			{
				db.Delete<tDVRUserPrivChannel>(pri);
			}
		}

		private void SetUsers(Int32 kDvr, UserInfo user, ref tDVRUser users)
		{
			users.Password = user.Password;
			users.KDVR = kDvr;
			users.UserName = user.UserName;
			users.Usertype = user.UserType;
		}

		private bool CompareUserData(Int32 kDvr, UserInfo user, tDVRUser users)
		{
			bool result = users.Password == user.Password &&
			              users.KDVR == kDvr &&
			              users.UserName == user.UserName &&
			              users.Usertype == user.UserType;
			return result;
		}

		private void SetUserConfig(Int32 kDvr, ref tDVRUserConfig userCfg)
		{
			userCfg.KDVR = kDvr;

			if (msgBody.UMData.AutoLogout != null)
			{
				userCfg.EnableAutoLogOut = msgBody.UMData.AutoLogout.Enable;
				userCfg.TimeOutLogOut = msgBody.UMData.AutoLogout.TimeOut;
			}
			if (msgBody.UMData.ldap != null)
			{
				userCfg.LDAPEnable = msgBody.UMData.ldap.Enable;
				userCfg.LDAPServer = msgBody.UMData.ldap.Server;
				userCfg.LDAPUsername = msgBody.UMData.ldap.Username;
				userCfg.LDAPPassword = msgBody.UMData.ldap.Password;
				userCfg.LDAPUseSSL = msgBody.UMData.ldap.UseSSL;
				userCfg.LDAPSyncInterval = msgBody.UMData.ldap.SyncInterval;
				userCfg.LDAPPort = msgBody.UMData.ldap.Port;
				userCfg.LDAPLimitEntryNumer = msgBody.UMData.ldap.LimitEntry;
			}
		}

		private bool CompareUser(int kDvr, tDVRUserConfig userCfg)
		{
			bool result = userCfg.KDVR == kDvr;

			if (msgBody.UMData.AutoLogout != null)
			{
				result = result &&
				         userCfg.EnableAutoLogOut == msgBody.UMData.AutoLogout.Enable &&
				         userCfg.TimeOutLogOut == msgBody.UMData.AutoLogout.TimeOut;
			}
			if (msgBody.UMData.ldap != null)
			{
				result = result &&
				         userCfg.LDAPEnable == msgBody.UMData.ldap.Enable &&
				         userCfg.LDAPServer == msgBody.UMData.ldap.Server &&
				         userCfg.LDAPUsername == msgBody.UMData.ldap.Username &&
				         userCfg.LDAPPassword == msgBody.UMData.ldap.Password &&
				         userCfg.LDAPUseSSL == msgBody.UMData.ldap.UseSSL &&
				         userCfg.LDAPSyncInterval == msgBody.UMData.ldap.SyncInterval &&
				         userCfg.LDAPPort == msgBody.UMData.ldap.Port &&
				         userCfg.LDAPLimitEntryNumer == msgBody.UMData.ldap.LimitEntry;
			}
			return result;
		}

		private void SetLdapGroupInfo(Int32 kDvr, LDAPGroupInfo ldapInfo, ref tDVRLDAPGroup ldGroup)
		{
			ldGroup.KDVR = kDvr;
			ldGroup.Group = ldapInfo.Group;
			ldGroup.Profile = ldapInfo.Profile;
		}

		private bool CompareLdapList(Int32 kDvr, LDAPGroupInfo ldapInfo, tDVRLDAPGroup ldGroup)
		{
			bool result = ldGroup.KDVR == kDvr &&
			              ldGroup.Group == ldapInfo.Group &&
			              ldGroup.Profile == ldapInfo.Profile;
			return result;
		}
		
		private List<LDAPGroupInfo> MessageLdapList()
		{
			List<LDAPGroupInfo> lLdap = null;
			if (msgBody.UMData == null) return null;
			if (msgBody.UMData.ldap == null) return null;
			if (msgBody.UMData.ldap.Groups != null)
			{
				lLdap = msgBody.UMData.ldap.Groups;
			}
			return lLdap;
		}
		*/
		#endregion
	}

	#region XML Classes

	[XmlRoot(MessageDefines.STR_Body)]
	public class RawUserManagementBody
	{
		[XmlElement(MessageDefines.STR_Common)]
		public RawMsgCommon msgCommon { get; set; }
		/*
		[XmlElement(MessageDefines.STR_TimeChange)]
		public Int64 TimeChange { get; set; }

		[XmlElement(MessageDefines.STR_ConfigID)]
		public Int32 ConfigID { get; set; }

		[XmlElement(MessageDefines.STR_ConfigName)]
		public string ConfigName { get; set; }
		*/
		[XmlElement(RawUserManagementConfig.STR_UserManagement)]
		public UserManagementData UMData { get; set; }

		[XmlElement(MessageDefines.STR_Whois)]
		public RawMsgWhois msgWhois { get; set; }
	}

	[XmlRoot(RawUserManagementConfig.STR_UserManagement)]
	public class UserManagementData : IMessageEntityCompare<tDVRUserConfig>, IToEntityObject<tDVRUserConfig>
	{
		[XmlElement(RawUserManagementConfig.STR_Ldap)]
		public LDAPInfo ldap { get; set; }

		[XmlElement(RawUserManagementConfig.STR_Server)]
		public string Server { get; set; }

		[XmlArray(RawUserManagementConfig.STR_Users)]
		[XmlArrayItem(MessageDefines.STR_User)]
		public List<UserInfo> Users = new List<UserInfo>();

		[XmlElement(RawUserManagementConfig.STR_AutoLogout)]
		public AutoLogoutInfo AutoLogout { get; set; }

		public bool Equal(tDVRUserConfig Value)
		{
			bool result = true;
			if( AutoLogout != null)
				result = AutoLogout.Enable == Value.EnableAutoLogOut && AutoLogout.TimeOut == Value.TimeOutLogOut;
			if( !result)
				return result;

			if (ldap != null)
			{
				result = ldap.Enable == Value.LDAPEnable && string.Compare(ldap.Server, Value.LDAPServer, true) == 0
				         && string.Compare(ldap.Username, Value.LDAPUsername, false) == 0
				         && string.Compare(ldap.Password, Value.LDAPPassword, false) == 0
				         && ldap.UseSSL == Value.LDAPUseSSL
				         && ldap.SyncInterval == Value.LDAPSyncInterval
				         && ldap.Port == Value.LDAPPort
				         && ldap.LimitEntry == Value.LDAPLimitEntryNumer
						 && string.Compare(ldap.GroupDN, Value.LDAPGroupDN, false) == 0
						 && string.Compare(ldap.UserDN, Value.LDAPUserDN, false) == 0;
			}
			return result;
		}

		public void SetEntity(ref tDVRUserConfig value)
		{	
			if( value == null)
			{
				value = ToEntity();
				return;
			}

			value.EnableAutoLogOut = AutoLogout == null ? (int?)null : AutoLogout.Enable;
			value.TimeOutLogOut = AutoLogout == null ? (int?)null : AutoLogout.TimeOut;
			if (ldap != null)
			{
				value.LDAPEnable = ldap.Enable;
				value.LDAPServer = ldap.Server;
				value.LDAPUsername = ldap.Username;
				value.LDAPPassword = ldap.Password;
				value.LDAPUseSSL = ldap.UseSSL;
				value.LDAPSyncInterval = ldap.SyncInterval;
				value.LDAPPort = ldap.Port;
				value.LDAPLimitEntryNumer = ldap.LimitEntry;
				value.LDAPGroupDN = ldap.GroupDN;
				value.LDAPUserDN = ldap.UserDN;
			}

		}

		public tDVRUserConfig ToEntity()
		{
			var userConfig = new tDVRUserConfig();

			userConfig.EnableAutoLogOut = AutoLogout == null ? (int?) null : AutoLogout.Enable;
			userConfig.TimeOutLogOut = AutoLogout == null ? (int?) null : AutoLogout.TimeOut;
		
			if (ldap != null)
			{
				userConfig.LDAPEnable = AutoLogout == null ? (int?)null : ldap.Enable;
				userConfig.LDAPServer = ldap.Server;
				userConfig.LDAPUsername = ldap.Username;
				userConfig.LDAPPassword = ldap.Password;
				userConfig.LDAPUseSSL = ldap.UseSSL;
				userConfig.LDAPSyncInterval = ldap.SyncInterval;
				userConfig.LDAPPort = ldap.Port;
				userConfig.LDAPLimitEntryNumer = ldap.LimitEntry;
				userConfig.LDAPGroupDN = ldap.GroupDN;
				userConfig.LDAPUserDN = ldap.UserDN;
			}

			return userConfig;
		}

		
	}

	[XmlRoot(RawUserManagementConfig.STR_Ldap)]
	public class LDAPInfo
	{
		[XmlElement(RawUserManagementConfig.STR_Enable)]
		public Int32 Enable { get; set; }

		[XmlElement(RawUserManagementConfig.STR_Server)]
		public string Server { get; set; }

		[XmlElement(RawUserManagementConfig.STR_Username)]
		public string Username { get; set; }

		[XmlElement(RawUserManagementConfig.STR_Password)]
		public string Password { get; set; }

		[XmlElement(RawUserManagementConfig.STR_UseSSL)]
		public Int32 UseSSL { get; set; }

		[XmlElement(RawUserManagementConfig.STR_SyncInterval)]
		public Int32 SyncInterval { get; set; }

		[XmlElement(RawUserManagementConfig.STR_Port)]
		public Int32 Port { get; set; }

		[XmlElement(RawUserManagementConfig.STR_LimitEntry)]
		public Int32 LimitEntry { get; set; }

		[XmlElement(RawUserManagementConfig.STR_GroupDN)]
		public string GroupDN { get; set; }

		[XmlElement(RawUserManagementConfig.STR_UserDN)]
		public string UserDN { get; set; }

		[XmlArray(RawUserManagementConfig.STR_LDAPGroup)]
		[XmlArrayItem(RawUserManagementConfig.STR_Group)]
		public List<LDAPGroupInfo> Groups = new List<LDAPGroupInfo>();
	}

	[XmlRoot(RawUserManagementConfig.STR_Group)]
	public class LDAPGroupInfo : IMessageEntity<tDVRLDAPGroup>
	{
		[XmlAttribute(RawUserManagementConfig.STR_Profile)]
		public string Profile { get; set; }

		[XmlAttribute(RawUserManagementConfig.STR_Group)]
		public string Group { get; set; }

		public bool Equal(tDVRLDAPGroup Value)
		{
			return string.Compare(Group, Value.Group, false) == 0 && string.Compare(Profile, Value.Profile, false) == 0;
		}
		public void SetEntity(ref tDVRLDAPGroup value)
		{
			if( value == null)
				value = new tDVRLDAPGroup();
			value.Group = Group;
			value.Profile = this.Profile;
		}
		//public tDVRLDAPGroup ToEntity()
		//{
		//	return new tDVRLDAPGroup
		//	{
		//		Group = Group,
		//		Profile = this.Profile
		//	};
		//}

	}


	public class SelfPermissionInfo
	{
		[XmlElement(RawUserManagementConfig.STR_ItselfPermission)]
		public UserPrivInfo Permits { get; set; }

	}


	public class UserPrivInfo : IMessageEntity<tDVRUserPrivOp>, IMessageEntity<tDVRUserPrivSetup>
	{
		[XmlElement(MessageDefines.STR_Name)]
		public string Name { get; set; }

		[XmlElement(RawUserManagementConfig.STR_Description)]
		public string Description { get; set; }

		[XmlElement(RawUserManagementConfig.STR_PrivilegeMask)]
		public Int32 PrivilegeMask { get; set; }

		[XmlElement(RawUserManagementConfig.STR_PrivilegeValue)]
		public Int32 PrivilegeValue { get; set; }

		public bool Equal(tDVRUserPrivOp Value)
		{
			if( Value == null)
				return false;
			return PrivilegeValue == Value.KPrivilege && Value.Mask == PrivilegeMask && string.Compare(Description, Value.Description, false) == 0;
		}

		public void SetEntity(ref tDVRUserPrivOp value)
		{
			if( value == null)
				value = new tDVRUserPrivOp();

			value.Description = this.Description;
			value.KPrivilege = this.PrivilegeValue;
			value.Mask = this.PrivilegeMask;
		}

		public bool Equal(tDVRUserPrivSetup Value)
		{
			if( Value == null)
				return false;

			return PrivilegeValue == Value.KPrivilege && Value.Mask == PrivilegeMask && string.Compare(Description, Value.Description, false) == 0;
		}

		public void SetEntity(ref tDVRUserPrivSetup value)
		{
			if( value == null)
				value = new tDVRUserPrivSetup();
			value.Description = this.Description;
			value.KPrivilege = (byte)this.PrivilegeValue;
			value.Mask = this.PrivilegeMask;
		}
	}

	[XmlRoot(MessageDefines.STR_User)]
	public class UserInfo : IMessageEntity<tDVRUsers>
	{
		[XmlElement(RawUserManagementConfig.STR_UserName)]
		public string UserName { get; set; }

		[XmlElement(RawUserManagementConfig.STR_Password)]
		public string Password { get; set; }

		[XmlElement(RawUserManagementConfig.STR_UserType)]
		public string UserType { get; set; }

		[XmlElement(RawUserManagementConfig.STR_AllPermission)]
		public AllPermissionInfo Permissions { get; set; }

		public bool Equal(tDVRUsers Value)
		{
			if( Value == null)
				return false;
			return string.Compare(UserName, Value.UserName, false) == 0 && string.Compare(Password, Value.Password, false) == 0 && string.Compare(this.UserType, Value.Usertype, false) == 0;
		}

		public void SetEntity(ref tDVRUsers value)
		{
			if (value == null)
				value = new tDVRUsers();
			value.Usertype = this.UserType;
			value.UserName = UserName;
			value.Password = this.Password;
		}

	}

	[XmlRoot(RawUserManagementConfig.STR_ChannelPermission)]
	public class ChannelPermissionInfo
	{
		[XmlElement(RawUserManagementConfig.STR_ItselfPermission)]
		public UserPrivInfo Permits { get; set; }

		[XmlArray(RawUserManagementConfig.STR_Channels)]
		[XmlArrayItem(RawUserManagementConfig.STR_Channel)]
		public List<UserPrivInfo> Channels = new List<UserPrivInfo>();
	}

	[XmlRoot(RawUserManagementConfig.STR_LivePermission)]
	public class LivePermissionInfo
	{
		[XmlElement(RawUserManagementConfig.STR_ItselfPermission)]
		public UserPrivInfo Permits { get; set; }

		[XmlElement(RawUserManagementConfig.STR_RealTimePermission)]
		public UserPrivInfo RealTime { get; set; }

		[XmlElement(RawUserManagementConfig.STR_InsSearchPermission)]
		public UserPrivInfo InsSearch { get; set; }

		[XmlElement(RawUserManagementConfig.STR_MuxPermission)]
		public UserPrivInfo Mux { get; set; }

		[XmlElement(RawUserManagementConfig.STR_ChannelPermission)]
		public ChannelPermissionInfo Channels { get; set; }
	}

	[XmlRoot(RawUserManagementConfig.STR_SearchPermission)]
	public class SearchPermissionInfo
	{
		[XmlElement(RawUserManagementConfig.STR_ItselfPermission)]
		public UserPrivInfo Permits { get; set; }

		[XmlElement(RawUserManagementConfig.STR_DumpPermission)]
		public UserPrivInfo Dump { get; set; }

		[XmlElement(RawUserManagementConfig.STR_ChannelPermission)]
		public ChannelPermissionInfo Channels { get; set; }
	}

	[XmlRoot(RawUserManagementConfig.STR_AllSetupPermission)]
	public class AllSetupPermissionInfo
	{
		[XmlElement(RawUserManagementConfig.STR_ItselfPermission)]
		public UserPrivInfo Permits { get;set;} //tDVRUserPrivOp

		[XmlArray(RawUserManagementConfig.STR_Setups)]
		[XmlArrayItem(RawUserManagementConfig.STR_Setup)]
		public List<UserPrivInfo> Setups = new List<UserPrivInfo>();//tDVRUserPrivSetup

	}

	[XmlRoot(RawUserManagementConfig.STR_SetupPermission)]
	public class SetupPermissionInfo
	{
		[XmlElement(RawUserManagementConfig.STR_ItselfPermission)]
		public UserPrivInfo Permits { get; set; } //tDVRUserPrivOp

		[XmlElement(RawUserManagementConfig.STR_AllSetupPermission)]
		public AllSetupPermissionInfo AllSetup { get; set; }

	}

	[XmlRoot(RawUserManagementConfig.STR_AllPermission)]
	public class AllPermissionInfo
	{
		[XmlElement(RawUserManagementConfig.STR_LivePermission)]
		public LivePermissionInfo Live { get; set; }

		[XmlElement(RawUserManagementConfig.STR_SearchPermission)]
		public SearchPermissionInfo Search { get; set; }

		[XmlElement(RawUserManagementConfig.STR_SetupPermission)]
		public SetupPermissionInfo Setup { get; set; }

		[XmlElement(RawUserManagementConfig.STR_PacPermission)]
		public SelfPermissionInfo PAC { get; set; }

		[XmlElement(RawUserManagementConfig.STR_PtzPermission)]
		public SelfPermissionInfo PTZ { get; set; }

		[XmlElement(RawUserManagementConfig.STR_PanicPermission)]
		public SelfPermissionInfo Panic { get; set; }

		[XmlElement(RawUserManagementConfig.STR_ToDisplayPermission)]
		public SelfPermissionInfo ToDisplay { get; set; }
	}

	[XmlRoot(RawUserManagementConfig.STR_AutoLogout)]
	public class AutoLogoutInfo
	{
		[XmlElement(RawUserManagementConfig.STR_Enable)]
		public Int32 Enable { get; set; }

		[XmlElement(RawUserManagementConfig.STR_TimeOut)]
		public Int32 TimeOut { get; set; }
	}

#endregion
	
}

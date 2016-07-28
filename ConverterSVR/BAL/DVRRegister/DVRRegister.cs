using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using ConverterSVR.Services;
using ConvertMessage;
using PACDMModel.Model;
using SVRDatabase;
using SVRDatabase.Model;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Commons;

namespace ConverterSVR.BAL.DVRRegister
{
	internal class DVRRegister : SingletonClassBase<DVRRegister>
	{

		public const string IPAddress_Subnet_separate = "-";

		private DVRRegister(){}

		public MessageResult CheckDvrAviliable(ref MessageDVRInfo dvrinfo, PACDMModel.PACDMDB pacmodel, SVRManager LogModel)
		{
			if (dvrinfo == null || string.IsNullOrEmpty(dvrinfo.HASPK))
				return new MessageResult { ErrorID = Commons.ERROR_CODE.DVR_INVALID_INFO, Data = Commons.Resources.ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.DVR_INVALID_INFO) };
			ServiceConfig svrconfig = LogModel.SVRConfig;
			string dvr_haskey = dvrinfo.HASPK;
			DVRInfo dvrconfig = LogModel.GetDVRInfo(item => item.HASPK == dvr_haskey).FirstOrDefault();
			//return when DVR was locked 
			if (dvrconfig != null && dvrconfig.Locked)
				return new MessageResult { ErrorID = Commons.ERROR_CODE.DVR_LOCKED_BY_ADMIN, Data = Commons.Resources.ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.DVR_LOCKED_BY_ADMIN) };

			Commons.ERROR_CODE ret = FindDVR(ref dvrinfo, pacmodel, svrconfig);
			
			return new MessageResult { ErrorID = ret, Data = Commons.Resources.ResourceManagers.Instance.GetResourceString(ret) };
		}

		public MessageResult RegisterDVR(ref MessageDVRInfo dvrinfo, PACDMModel.PACDMDB pacmodel, SVRManager LogModel, out bool newDVR)
		{
			newDVR = false;
			//Nghi hide because we can ignore HaspID( request from Trang, Dung) Nov 18 2015 begin
			//if( dvrinfo == null || string.IsNullOrEmpty( dvrinfo.HASPK))
			//{
			//	return new MessageResult{ ErrorID = Commons.ERROR_CODE.DVR_INVALID_INFO, Data = Commons.Resources.ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.DVR_INVALID_INFO) };
			//}
			//Nghi hide because we can ignore HaspID( request from Trang, Dung) Nov 18 2015 end
			ServiceConfig svrconfig = LogModel.SVRConfig;
			string dvr_haskey = dvrinfo.HASPK;
			DVRInfo dvrconfig = LogModel.GetDVRInfo(item => item.HASPK == dvr_haskey).FirstOrDefault();
			//return when DVR was locked 
			if( dvrconfig != null && dvrconfig.Locked)
				return new MessageResult { ErrorID =  Commons.ERROR_CODE.DVR_LOCKED_BY_ADMIN, Data = Commons.Resources.ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.DVR_LOCKED_BY_ADMIN) };

			Commons.ERROR_CODE ret = FindDVR(ref dvrinfo, pacmodel, svrconfig);
			if( dvrconfig == null)
            {
                ret = CheckLicense(pacmodel);
                if(ret == ERROR_CODE.OK)  dvrconfig = AddDVRInfo(LogModel, dvrinfo, svrconfig.ConverterInterval);
            }

			switch(ret)
			{
				case Commons.ERROR_CODE.OK:
						CheckPACInfo(dvrinfo, pacmodel);
						UpdateDVRInfo(dvrinfo, pacmodel);
						//msg_result = new MessageResult { ErrorID = ret, Data = Commons.Resources.ResourceManagers.Instance.GetResourceString(ret) };
						if( dvrconfig.KDVR != dvrinfo.KDVR)
						{
							dvrconfig.KDVR = dvrinfo.KDVR;
							LogModel.UpdateDVRInfo(dvrconfig);
						}
						
					break;
				case Commons.ERROR_CODE.DVR_INFO_CHANGE:
						dvrconfig.Locked = true;
						dvrconfig.Note = Commons.Resources.ResourceManagers.Instance.GetResourceString(ret);
						LogModel.UpdateDVRInfo(dvrconfig);
					break;
                case ERROR_CODE.DVR_FULL_CONNECTED:
                    break;
				default:
                            ret = CheckLicense(pacmodel);
                            if (ret == ERROR_CODE.OK)
                            {
						if( svrconfig.AuthenticateMode)//Auto create new DVR
						{
							ret = AddDVRInfo(ref dvrinfo, pacmodel);
							newDVR = true;
							if (ret == Commons.ERROR_CODE.OK)
								CheckPACInfo(dvrinfo, pacmodel);
						}
						else
						{
							ret = Commons.ERROR_CODE.DVR_REGISTER_PENDING;
                            //2015-05-31 Tri fix unLock connect DVR
                            dvrconfig.Locked = false;
							dvrconfig.Note = Commons.Resources.ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.DVR_REGISTER_PENDING);
                            LogModel.UpdateDVRInfo(dvrconfig);
						}
                            }
                            else
                            {
                                ret = Commons.ERROR_CODE.DVR_REGISTER_PENDING;
                                //2015-05-31 Tri fix unLock connect DVR
                                dvrconfig.Locked = false;
                                dvrconfig.Note = Commons.Resources.ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.DVR_REGISTER_PENDING);
                                LogModel.UpdateDVRInfo(dvrconfig);
                            }
				break;
			}
			return new MessageResult{ ErrorID = ret, Data = Commons.Resources.ResourceManagers.Instance.GetResourceString(ret) };
		}

		private Commons.ERROR_CODE FindDVR(ref MessageDVRInfo dvrinfo, PACDMModel.PACDMDB pacmodel, ServiceConfig svrcfig)
		{
			if(svrcfig == null || string.IsNullOrEmpty( svrcfig.DVRAuthenticate))
				return Commons.ERROR_CODE.SERVICE_DVR_PENDING;
			IEnumerable<Match> IE_Match = DVRRegisterSupports.Instance.ParserDVRRegister(svrcfig.DVRAuthenticate);
			if( IE_Match == null || IE_Match.Count() == 0)
				return Commons.ERROR_CODE.SERVICE_DVR_PENDING;
			//nghi add july 29 2015 begin
			//to support add Site before converter connect to API
			IQueryable<tDVRAddressBook> kdvrs = pacmodel.Query<tDVRAddressBook>(it => string.IsNullOrEmpty(it.HaspLicense), null);
			if( kdvrs != null && kdvrs.Any())
			{
				 IEnumerable<string> macs = dvrinfo.MACs.Select( it => it.MAC_Address).Distinct();
				 IEnumerable<tDVRAddressBook> KDVRs = from dvr in kdvrs
										from mac in macs
										where string.Compare(dvr.DVRGuid, mac, true) == 0
										select dvr;
				if( KDVRs.Any())
				{
					dvrinfo.KDVR = KDVRs.Max<tDVRAddressBook>( it => it.KDVR);
					//when dvr have 2 NICs and user try to register 2 NICs => CMsweb will create 2 Virtual DVRs
					//we need to update haspID to make sure only 1 dvr is working
					if(KDVRs.Count() > 1)
					{
						foreach(tDVRAddressBook dvr in KDVRs )
						{
							dvr.HaspLicense = dvrinfo.HASPK;
							pacmodel.Update<tDVRAddressBook>( dvr);
						}
						pacmodel.Save();
					}
					return ERROR_CODE.OK;
				}
				 
			}
			//nghi add july 29 2015 end

			char sign = DVRRegisterSupports.Instance.combineOperator(IE_Match.FirstOrDefault().Groups[DVRRegisterSupports.Str_Sign].Value);
			string[] config = IE_Match.Where(item => string.IsNullOrEmpty(item.Groups[DVRRegisterSupports.Str_Match].Value) == false).Select(group => group.Groups[DVRRegisterSupports.Str_Match].Value).ToArray();
			if (sign == DVRRegisterSupports.Register_Split_item_OR)
				return FindDVRAnyInfo(ref dvrinfo, pacmodel, config);

			return FindDVRAllInfo(ref dvrinfo,pacmodel, config);
		}
		private Commons.ERROR_CODE UpdateDVRInfo(MessageDVRInfo newinfo, PACDMModel.PACDMDB pacModel)
		{
			tDVRAddressBook addbook = pacModel.FirstOrDefault<tDVRAddressBook>(it => it.KDVR == newinfo.KDVR);
			 if( string.IsNullOrEmpty(addbook.HaspLicense) || string.Compare( newinfo.HASPK, addbook.HaspLicense, true) != 0)
			 {

				addbook.HaspLicense = newinfo.HASPK;
				MacInfo activemac = GetActiveMAC(newinfo);
				if (activemac == null)
					return Commons.ERROR_CODE.DB_INVALID_DVR_GUI;
				IQueryable<tDVRNetworkCard> nets = pacModel.Query<tDVRNetworkCard>(it => it.KDVR == newinfo.KDVR);
				addbook.DVRGuid = activemac.MAC_Address;
				pacModel.Update<tDVRAddressBook>(addbook);
				List<tDVRNetworkCard> lstNetCards = MACInfos2NetworkCard(newinfo);
				foreach( tDVRNetworkCard del in nets)
					pacModel.Delete<tDVRNetworkCard>(del);
				foreach (tDVRNetworkCard add in lstNetCards)
				{
					add.KDVR = newinfo.KDVR;
					pacModel.Insert<tDVRNetworkCard>(add);
#if DEBUG
					System.Diagnostics.Debug.WriteLine("UpdateDVRInfo: " + add.MACAddress );
#endif
				}
				return  pacModel.Save() > 0? ERROR_CODE.OK : ERROR_CODE.DB_UPDATE_DATA_FAILED;
			}
			return ERROR_CODE.OK;

		
		}
		private Commons.ERROR_CODE AddDVRInfo(ref MessageDVRInfo newinfo, PACDMModel.PACDMDB pacModel)
		{
			List<tDVRNetworkCard> lstNetCards = MACInfos2NetworkCard(newinfo);
			tDVRAddressBook addbook = new tDVRAddressBook();
			addbook.HaspLicense =  newinfo.HASPK;
			MacInfo activemac = GetActiveMAC(newinfo);
			if( activemac == null)
				return Commons.ERROR_CODE.DB_INVALID_DVR_GUI;
			addbook.DVRGuid = activemac.MAC_Address;
			bool ret = true;

			ret = pacModel.InsertWithTransaction<tDVRAddressBook>(addbook);
			if( addbook.KDVR == 0 ||ret == false)
			{
				pacModel.RollBackTransaction();
				return Commons.ERROR_CODE.DB_INSERT_DATA_FAILED;
			}
			
			foreach( tDVRNetworkCard ncard in lstNetCards)
			{
				ncard.KDVR = addbook.KDVR;
				
				ret = pacModel.InsertWithTransaction<tDVRNetworkCard>(ncard);
				System.Diagnostics.Debug.WriteLine("AddDVRInfo:" + ncard.MACAddress);
				if (ncard.NetworkCardID == 0 || ret == false)
				{
					ret = false;
					break;
				}
			}
			if( ret == false)
			{
				pacModel.RollBackTransaction();
				return Commons.ERROR_CODE.DB_INSERT_DATA_FAILED;

			}
			pacModel.CommitTransaction();
			newinfo.KDVR = addbook.KDVR;
			
			return Commons.ERROR_CODE.OK;
			
		}

		private Commons.ERROR_CODE FindDVRAllInfo(ref MessageDVRInfo dvrinfo, PACDMModel.PACDMDB pacmodel, string[]config)
		{
			IDVRRegister iregister = null;
			Type RegisterType = null;
			IEnumerable<int> Kdvrs = null;

			List<int>MatchDvr = null;
			foreach (string item in config)
			{
				RegisterType = RegisterType = DVRRegisterSupports.Instance.GetMapping(item).Value;
				if (RegisterType == null)
					continue;

				iregister = Commons.ObjectUtils.InitObject(RegisterType) as IDVRRegister;
				if (iregister == null)
					continue;

				Kdvrs = iregister.FindDVR(dvrinfo, pacmodel);
				if (Kdvrs == null || !Kdvrs.Any())
					return Commons.ERROR_CODE.SERVICE_NOT_FOUND_DVR;

				if( MatchDvr == null)
					MatchDvr = Kdvrs.ToList();
				else
				{
					var filter_dvr = from kdvr in Kdvrs
									from mdvr in MatchDvr
										where kdvr == mdvr
										select kdvr;

					if(!filter_dvr.Any())//stop find if cannot match with any DVR
					{
						MatchDvr = null;
						return Commons.ERROR_CODE.DVR_INFO_CHANGE;
					}
					else
					{
						MatchDvr = filter_dvr.ToList();
					}
				}

			}

			if( MatchDvr == null || MatchDvr.Count == 0)
				return  Commons.ERROR_CODE.SERVICE_NOT_FOUND_DVR ;

			dvrinfo.KDVR = MatchDvr.Max();
			return  Commons.ERROR_CODE.OK;
		}
		
		private Commons.ERROR_CODE FindDVRAnyInfo(ref MessageDVRInfo dvrinfo, PACDMModel.PACDMDB pacmodel, string[]config)
		{
			IDVRRegister iregister = null;
			Type RegisterType = null;
			IEnumerable<int> Kdvrs = null;
			foreach (string item in config)
			{
				RegisterType = RegisterType = DVRRegisterSupports.Instance.GetMapping(item).Value;
				if (RegisterType == null)
					continue;

				iregister = Commons.ObjectUtils.InitObject(RegisterType) as IDVRRegister;
				if (iregister == null)
					continue;

				Kdvrs = iregister.FindDVR(dvrinfo, pacmodel);
				try
				{
					if (Kdvrs == null || !Kdvrs.Any())
						continue;
					dvrinfo.KDVR = Kdvrs.Max();
					break;
				}
				catch (Exception)
				{
					continue;
				}
				//break;
			}
			return dvrinfo.KDVR == 0 ? Commons.ERROR_CODE.SERVICE_NOT_FOUND_DVR : Commons.ERROR_CODE.OK;
		}
		
		private MacInfo GetActiveMAC(MessageDVRInfo newinfo)
		{
			return newinfo.MACs.FirstOrDefault(info => info.Active == true &&  string.Compare( info.IP_Version, AddressFamily.InterNetwork.ToString(), true) == 0);
		}
		
		private List<tDVRNetworkCard> MACInfos2NetworkCard(MessageDVRInfo newinfo)
		{
			IEnumerable<string> MACs = newinfo.MACs.Where(item => string.Compare(item.IP_Version, AddressFamily.InterNetwork.ToString(), true) == 0).Select(mac => mac.MAC_Address).Distinct();
			List<tDVRNetworkCard> lstNetCards = new List<tDVRNetworkCard>();
			tDVRNetworkCard netcard = null;
			IEnumerable<MacInfo> iemacinfos;
			foreach (string mac in MACs)
			{
				iemacinfos = newinfo.MACs.Where(item => string.Compare(item.MAC_Address, mac, true) == 0 && string.Compare(item.IP_Version, AddressFamily.InterNetwork.ToString(), true) == 0);
				if (iemacinfos.Count() == 0)
					continue;
				netcard = MACInfos2NetworkCard(iemacinfos);
				if (string.IsNullOrEmpty(netcard.MACAddress))
					continue;
				lstNetCards.Add(netcard);
			}
			return lstNetCards;
		}

		private tDVRNetworkCard MACInfos2NetworkCard( IEnumerable<MacInfo> ieMacs)
		{
			tDVRNetworkCard	netCard = new tDVRNetworkCard();
			string all_ipaddress = string.Empty;
			IEnumerable<string> ie_ipaddress = ieMacs.Where(item => string.Compare(item.IP_Version,AddressFamily.InterNetwork.ToString(), true)== 0 ).Select( item => item.IP_Address).ToList();
			all_ipaddress = String.Join(Consts.cms_ip_split.ToString(), ie_ipaddress);
			netCard.NetworkCardID = ieMacs.First().MacOrder;
			netCard.MACAddress = ieMacs.First().MAC_Address;
			netCard.IPv4List = all_ipaddress;
			return netCard;
		}

		private bool CheckPACInfo( MessageDVRInfo dvrinfo, PACDMModel.PACDMDB pacmodel)
		{
			if( dvrinfo == null || dvrinfo.KDVR == 0)
				return false;

			MacInfo activemac = GetActiveMAC(dvrinfo);
			if( activemac == null)
				return false;
			tbl_POS_PACID pacid = null;
			try
			{
				pacid = pacmodel.FirstOrDefault<tbl_POS_PACID>(item => item.KDVR == dvrinfo.KDVR);
			}
			catch(Exception ex)
			{
				pacid = null;
			}
			if( pacid == null)
			{
				pacid = new tbl_POS_PACID();
				pacid.KDVR = dvrinfo.KDVR;
				pacid.PACID_Name = dvrinfo.PACinfo.PACID;
				//pacid.MAC_Address = activemac.MAC_Address;
				pacid.SPK_KeyID = dvrinfo.HASPK;
				pacid.CreationDate = DateTime.Now; 
				pacmodel.Insert<tbl_POS_PACID>(pacid);
				pacmodel.Save();
				//Nghi change to update data to WareHouse July 09 2015 begin
				if (pacid.KDVR > 0)
				{
					CMSWebApi.Wrappers.Wrapper.Instance.DBWareHouse.UpdateDim(pacmodel, pacid);
					//CMSWebApi.Cache.DBWarehouse.WarehouseManager.Instance.UpdateDim(pacmodel, pacid);
					
					
				}
				//Nghi change to update data to WareHouse July 09 2015 begin
				return true;
			}
			else
			{
				bool is_update = false;
				//if( string.Compare(pacid.MAC_Address, activemac.MAC_Address, true) != 0)
				//{
				//	pacid.MAC_Address = activemac.MAC_Address;
				//	is_update = true;
				//}

				if( string.Compare( pacid.SPK_KeyID, dvrinfo.HASPK, true) != 0)
				{
					pacid.SPK_KeyID = dvrinfo.HASPK;
					is_update = true;

				}
				if( string.Compare( pacid.PACID_Name, dvrinfo.PACinfo.PACID, true) != 0)
				{
					pacid.PACID_Name = dvrinfo.PACinfo.PACID;
					is_update = true;
				}
				if(is_update)
				{
					try
					{
						pacmodel.Update<tbl_POS_PACID>(pacid);
						pacmodel.Save();
						CMSWebApi.Wrappers.Wrapper.Instance.DBWareHouse.ModifyDim(pacmodel, pacid);
						return true;
					}catch(Exception){}
				}
				return true;
			}
		}

		private DVRInfo AddDVRInfo(SVRManager logmodel, MessageDVRInfo dvrmsginfo, Int16 convertinterval)
		{
			DVRInfo dvrinfo = new DVRInfo { KDVR = dvrmsginfo.KDVR, Locked = false, HASPK = dvrmsginfo.HASPK, HostName = dvrmsginfo.HostName, ForceStop = false, CreateDate = DateTime.Now, ConverterInterval = convertinterval };
			logmodel.InsertDVRInfo(dvrinfo);
			dvrinfo = logmodel.GetDVRInfo( item => item.HASPK == dvrinfo.HASPK).FirstOrDefault();
			AddDVRDetail(logmodel, dvrinfo, dvrmsginfo);

			return dvrinfo;
		}

		private void AddDVRDetail(SVRManager logmodel, DVRInfo dvrinfo, MacInfo macinfo)
		{
			logmodel.InsertDVRDetail(new DVRDetail
			{
				IDInfo = dvrinfo.ID,
				IPAddress = macinfo.IP_Address,
				MACAddress = macinfo.MAC_Address,
				IPV4 = string.Compare(macinfo.IP_Version, AddressFamily.InterNetwork.ToString(), true) == 0
			}
										);
		}
		
		private void AddDVRDetail(SVRManager logmodel, DVRInfo dvrinfo, MessageDVRInfo dvrmsginfo)
		{
			List<MacInfo> MACs = dvrmsginfo.MACs;
			MACs.ToList().ForEach( item =>
			{
				AddDVRDetail(logmodel, dvrinfo, item);
			}

			);

		}
		
        private ERROR_CODE CheckLicense(PACDMModel.PACDMDB pacmodel)
        {
            LicenseInfo.Models.LicenseModel model = AppSettings.AppSettings.Instance.Licenseinfo;
            if (model == null) return ERROR_CODE.UNKNOWN;
            var addressbook = pacmodel.Query<tDVRAddressBook>();
            if (addressbook.Any())
            {
                int count = addressbook.Count();
                if (model.DVRNumber <= count)
                {
                    return ERROR_CODE.DVR_FULL_CONNECTED;
                }
            }
            return ERROR_CODE.OK;
        }
		
	}
}

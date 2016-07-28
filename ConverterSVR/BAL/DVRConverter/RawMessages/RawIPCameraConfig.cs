using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using PACDMModel.Model;
using SVRDatabase;

namespace ConverterSVR.BAL.DVRConverter
{
	[XmlRoot(MessageDefines.STR_Message)]
	public class RawIPCameraConfig : RawDVRConfig<RawIPCameraBody>
	{
		#region Parameter
		public const string STR_IPCamera = "ip_camera";
		public const string STR_UpdateRes = "update_res";
		public const string STR_Cameras = "cameras";
		public const string STR_Camera = "camera";
		public const string STR_Model = "model";
		//public const string STR_IP = "ip";
		public const string STR_Port = "port";
		public const string STR_FullModelName = "fullmodelname";
		public const string STR_MACAddress = "mac_address";
		public const string STR_Mask = "mask";
		public const string STR_Params = "params";
		public const string STR_Param = "param";
		public const string STR_VideoSource = "video_source";
		public const string STR_ResolutionWidth = "resolution_width";
		public const string STR_ResolutionHeight = "resolution_height";
		public const string STR_FrameRate = "frame_rate";
		public const string STR_UserName = "user_name";
		public const string STR_Password = "password";
		public const string STR_LocationName = "location_name";
		public const string STR_MaxFPS = "max_fps";
		public const string STR_SupportedResolution = "supported_resolution";
		public const string STR_Resolution = "resolution";
		public const string STR_Width = "width";
		public const string STR_Height = "height";
		public const string STR_AdapterName = "adapter_name";
		public const string STR_Gateway = "gateway";

		//[XmlElement(STR_Body)]
		//public RawIPCameraBody msgBody { get; set; }

		//List<tDVRIPCameraInput> _iPCamInputList;
		//List<tDVRIPRe> _ipReList;
		//List<tDVRIPCameraUser> _ipCamerauserList;
		#endregion

		public override async Task<Commons.ERROR_CODE> UpdateToDB()
		{
			if (DVRAdressBook == null || msgBody.ipData == null)
				return await base.UpdateToDB();

			if (UpdateIPCameraConfig(DVRAdressBook, msgBody.ipData))
			{
				int ret = 0;
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}

			//UpdateChecksum((int)DVR_CONFIGURATION.EMS_CFG_IP_CAMERA, msgBody.msgCommon.Checksum, msgBody.msgCommon.TimeChange, msgBody.msgCommon.dtDVRTime); //AAAA
			if (UpdateChecksum((int)DVR_CONFIGURATION.EMS_CFG_IP_CAMERA, msgBody.msgCommon))
			{
				int ret = 0;
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}
			return await base.UpdateToDB();
		}

		private bool UpdateIPCameraConfig(tDVRAddressBook dvrAdressBook, IPCameraData ipData)
		{
			if (ipData.IPCameras == null) return false;
			//string consMac="MAC_";

			//Join 2 list to 1 Object list with samme channel ID
			var dvripCameras = db.Query<tDVRIPCameras>(x => x.KDVR == dvrAdressBook.KDVR).ToList();
			var ipCameras = ipData.IPCameras;
			//Chinh 16/9/2014 support for pro <3.2
			/*for (int i = 0; i < ipCameras.Count; i++)
			{
				if (ipCameras[i].MACAddress == null)
					ipCameras[i].MACAddress = consMac + (i+1).ToString();
			}*/
			bool result = false;
			var updates = from dvripCamera in dvripCameras
						  from cameraInfo in ipCameras
						  where (((cameraInfo.IP + ":" + cameraInfo.Port) == (dvripCamera.IPAddress + ":" + dvripCamera.Port)) && !String.IsNullOrEmpty(cameraInfo.IP) && !String.IsNullOrEmpty(dvripCamera.IPAddress))
						  select new { Item = dvripCamera, InfoItem = cameraInfo };

			//Update Object list above
			tDVRIPCameras tDvripCamera;
			foreach (var item in updates)
			{
				if (!item.InfoItem.Equal(item.Item))
				{
					tDvripCamera = item.Item;
					item.InfoItem.SetEntity(ref tDvripCamera);
					db.Update<tDVRIPCameras>(tDvripCamera);
					result = true;
				}
				result |= UpdateIpCamInputs(item.InfoItem, item.Item);
			}

			//Use channels list and except item have updates list for deleting.
			if (ipData.UpdateRes == 0)
			{
				IEnumerable<tDVRIPCameras> deletes = dvripCameras.Except(updates.Select(item => item.Item)).ToList();
				foreach (tDVRIPCameras delete in deletes)
				{
					DeleteIpCameras(delete);
					System.Threading.Thread.Sleep(Time_Loop_Delay);
					result = true;
				}
			}

			//Use channels Info list and except item have updates list for inserting.
			IEnumerable<CameraInfo> newitems = ipCameras.Except(updates.Select(item => item.InfoItem));
			//Chinh 16/9/2014 support for pro <3.2
			/*int macId = 0;
			if (dvripCameras.Count() > 0 && (from Mac in dvripCameras select Mac.MACAddress).Last().Contains(consMac))
			{
				string macAddress=(from Mac in dvripCameras select Mac.MACAddress).Last();
				macId = string.IsNullOrEmpty(macAddress) ? 0 : Convert.ToInt32(macAddress.Remove(0, consMac.Length));
			}*/
			
			foreach (CameraInfo newitem in newitems)
			{
				/*if (newitem.MACAddress == null)
				{
					macId += 1;
					newitem.MACAddress = consMac + macId.ToString();
				}*/
				tDvripCamera = new tDVRIPCameras() { KDVR = dvrAdressBook.KDVR };
				newitem.SetEntity(ref tDvripCamera);
				db.Insert<tDVRIPCameras>(tDvripCamera);
				result |= UpdateIpCamInputs(newitem, tDvripCamera);
				result = true;
			}

			return result;
		}

		private void DeleteIpCameras(tDVRIPCameras delete)
		{
			db.DeleteWhere<tDVRIPRes>(t => t.KDVRIPCamera == delete.KIPCamera);
			db.DeleteWhere<tDVRIPCameraInputs>(t => t.KIPCamera == delete.KIPCamera);
			db.DeleteWhere<tDVRIPCameraUsers>(t => t.KIPCamera == delete.KIPCamera);
			db.Delete<tDVRIPCameras>(delete);
		}

		private bool UpdateIpCamInputs(CameraInfo infoItem, tDVRIPCameras dvrIpCamera)
		{
			if (infoItem.Params == null) return false;

			//Join 2 list to 1 Object list with samme channel ID
			var cameraInputs = db.Query<tDVRIPCameraInputs>(x => x.KIPCamera == dvrIpCamera.KIPCamera).ToList();
			var paramInfos = infoItem.Params;
			bool result = false;
			var updates = from dvrChannel in cameraInputs //dvrIpCamera.tDVRIPCameraInputs//cameraInputs
						  from dvrChannelInfo in paramInfos
						  where dvrChannelInfo.id == dvrChannel.InputNo
						  select new { Item = dvrChannel, InfoItem = dvrChannelInfo };

			//Update Object list above
			tDVRIPCameraInputs tDvripCameraInput;
			
			foreach (var item in updates)
			{
				db.Include<tDVRIPCameraInputs, tResolutionDef>(item.Item, _res => _res.tResolutionDef);
				db.Include<tDVRIPCameraInputs, tDVRIPRes>(item.Item, _res => _res.tDVRIPRes);
				if (!item.InfoItem.Equal(item.Item))
				{
					tDvripCameraInput = item.Item;
					//Anh Huynh, Update KResolution for old version only, NULL from Pro 1.600
					if (item.InfoItem.Resolution > 0)
					{
						tDvripCameraInput.tResolutionDef = db.Query<tResolutionDef>().FirstOrDefault(tres => tres.KResolution == item.InfoItem.Resolution);
						tDvripCameraInput.KResolution = item.InfoItem.Resolution;
					}
					else
					{
						tDvripCameraInput.tResolutionDef = null;//db.Query<tResolutionDef>().FirstOrDefault(tres => tres.Resolution.Trim().CompareTo((item.Item.ResolutionWidth + "x" + item.Item.ResolutionHeight).Trim()) == 0);
						tDvripCameraInput.KResolution = null;
					}
					item.InfoItem.SetEntity(ref tDvripCameraInput);
					db.Update<tDVRIPCameraInputs>(tDvripCameraInput);
					result = true;
				}
				result |= UpdateUserInfos(dvrIpCamera, item.InfoItem.UserName, item.InfoItem.Password);
				result |= UpdateIpSupportedRes(item.Item, item.InfoItem);
			}

			//Use channels list and except item have updates list for deleting.
			IEnumerable<tDVRIPCameraInputs> deletes = cameraInputs.Except(updates.Select(item => item.Item)).ToList();
			foreach (tDVRIPCameraInputs delete in deletes)
			{
				tDVRIPCameraInputs deleteip = delete;
				db.DeleteWhere<tDVRIPRes>(t => t.KDVRIPCamera == deleteip.KDVRIPCamera);
				db.Delete<tDVRIPCameraInputs>(deleteip);
				System.Threading.Thread.Sleep(Time_Loop_Delay);
				result = true;
			}

			//Use channels Info list and except item have updates list for inserting.
			IEnumerable<ParamInfo> newitems = paramInfos.Except(updates.Select(item => item.InfoItem));
			foreach (ParamInfo newitem in newitems)
			{
				tDvripCameraInput = new tDVRIPCameraInputs(){tDVRIPCameras = dvrIpCamera};
				newitem.SetEntity(ref tDvripCameraInput);
				//Anh Huynh, Update KResolution for old version only, NULL from Pro 1.600
				if (newitem.Resolution > 0)
				{
					//tDvripCameraInput.tResolutionDef = db.Query<tResolutionDef>().FirstOrDefault(tres => tres.Resolution.CompareTo(tDvripCameraInput.ResolutionWidth + "x" + tDvripCameraInput.ResolutionHeight) == 0);
					tDvripCameraInput.tResolutionDef = db.Query<tResolutionDef>().FirstOrDefault(tres => tres.KResolution == newitem.Resolution);
					tDvripCameraInput.KResolution = newitem.Resolution;
				}
				else
				{
					tDvripCameraInput.tResolutionDef = null;
					tDvripCameraInput.KResolution = null;
				}
				db.Insert<tDVRIPCameraInputs>(tDvripCameraInput);
				result |= UpdateUserInfos(dvrIpCamera, newitem.UserName, newitem.Password);
				result |= InsertIpResolutions(tDvripCameraInput, newitem);
				result = true;
			}

			return result;
		}

		private bool UpdateIpSupportedRes(tDVRIPCameraInputs kIpCam, ParamInfo ipcaInfo)
		{
			if (ipcaInfo.SupportedRes == null) return false;
			bool ret = false;
			List<tDVRIPRes> lsIpRes = db.Query<tDVRIPRes>(x => x.KDVRIPCamera == kIpCam.KDVRIPCamera).ToList();
			foreach (var ipRes in lsIpRes)
			{
				ResolutionInfo resolutionInfo = ipcaInfo.SupportedRes.FirstOrDefault(x => x.Width == ipRes.Width && x.Height == ipRes.Height);
				if (resolutionInfo != null)
				{
					ipcaInfo.SupportedRes.Remove(resolutionInfo);
				}
				else
				{
					db.Delete<tDVRIPRes>(ipRes);
					ret = true;
				}
			}

			ret |= InsertIpResolutions(kIpCam, ipcaInfo);
			return ret;
		}

		private bool InsertIpResolutions(tDVRIPCameraInputs kIpCam, ParamInfo ipcaInfo)
		{
			bool ret = false;
			foreach (var ipRes in ipcaInfo.SupportedRes)
			{
				var ipResolution = new tDVRIPRes {tDVRIPCameraInputs = kIpCam, Width = ipRes.Width, Height = ipRes.Height};
				db.Insert<tDVRIPRes>(ipResolution);
				ret = true;
			}
			return ret;
		}

		private bool UpdateUserInfos(tDVRIPCameras ipCam, string sUser, string sPass)
		{
			bool ret = false;
			tDVRIPCameraUsers ipUser = db.Query<tDVRIPCameraUsers>(x => x.KIPCamera == ipCam.KIPCamera).FirstOrDefault();
			if (ipUser == null)
			{
				ipUser = new tDVRIPCameraUsers {UserName = sUser, Password = sPass, tDVRIPCameras = ipCam};
				db.Insert<tDVRIPCameraUsers>(ipUser);
				ret = true;
			}
			else
			{
				if (ipUser.UserName != sUser ||
				    ipUser.Password != sPass ||
				    ipUser.tDVRIPCameras != ipCam)
				{
					ipUser.UserName = sUser;
					ipUser.Password = sPass;
					ipUser.tDVRIPCameras = ipCam;
					db.Update<tDVRIPCameraUsers>(ipUser);
					ret = true;
				}
			}
			return ret;
		}

		#region Unused


		//public async Task<Commons.ERROR_CODE> UpdateToDB1()
		//{
		//	if (DVRAdressBook == null)
		//		return await base.UpdateToDB();

		//	tDVRHardware hwData = db.FirstOrDefault<tDVRHardware>( item => item.KDVR == DVRAdressBook.KDVR);
		//	if (hwData == null)
		//		return await base.UpdateToDB();

		//	Int32 kVideoFormat = (hwData.KVideoFormat == null) ? 0 : (int)hwData.KVideoFormat;

		//	SetIpCameras(DVRAdressBook.KDVR);
		//	return await Task.FromResult<Commons.ERROR_CODE>( db.Save() == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
		//}

		//private List<CameraInfo> GetCameraInfoList()
		//{
		//	List<CameraInfo> caInfoList = IsIpCameraInfo() ? msgBody.ipData.IPCameras.ToList() : new List<CameraInfo>();
		//	return caInfoList;
		//}

		//private void SetIpCameras(Int32 kDvr)
		//{
		//	var lsIpCams = GetGlobalList(kDvr);

		//	List<CameraInfo> caInfoList = GetCameraInfoList();

		//	foreach (var ca in lsIpCams)
		//	{
		//		tDVRIPCamera ipCa = ca;
		//		CameraInfo caInfo = ca.MACAddress == null ? null : caInfoList.FirstOrDefault(x => x.MACAddress.Trim().ToUpper() == ca.MACAddress.Trim().ToUpper());
		//		if (caInfo != null)
		//		{
		//			if (!CompareIpCameraInfo(kDvr, caInfo, ipCa))
		//			{
		//				SetIpCameraInfo(kDvr, caInfo, ref ipCa);
		//				db.Update<tDVRIPCamera>(ipCa);
		//			}

		//			SetIpCamInputs(ipCa, caInfo);
		//			caInfoList.Remove(caInfo);
		//		}
		//		else
		//		{

		//			DeleteIpCamera(ipCa);
		//			db.Delete<tDVRIPCamera>(ipCa);
		//		}
		//	}

		//	InsertNewIpCameras(kDvr, caInfoList);
		//	//db.Save();
		//}

		//private void InsertNewIpCameras(int kDvr, List<CameraInfo> caInfoList)
		//{
		//	foreach (var ca in caInfoList)
		//	{
		//		var ipCam = new tDVRIPCamera();
		//		SetIpCameraInfo(kDvr, ca, ref ipCam);
		//		db.Insert<tDVRIPCamera>(ipCam);
		//		db.Save();
		//		//tDVRIPCamera ipca = db.Query<tDVRIPCamera>(x => x.MACAddress.Trim().ToUpper() == ipCam.MACAddress.Trim().ToUpper() && x.KDVR == ipCam.KDVR).OrderByDescending(t => t.KIPCamera).FirstOrDefault();
		//		SetIpCamInputs(ipCam, ca);
		//	}
		//}

		//private List<tDVRIPCamera> GetGlobalList(int kDvr)
		//{
		//	List<tDVRIPCamera> lsIpCams = db.Query<tDVRIPCamera>(x => x.KDVR == kDvr).OrderBy(x => x.KIPCamera).ToList();
		//	List<int> kIpCameralst = lsIpCams.Select(x => x.KIPCamera).ToList();
		//	_iPCamInputList = db.Query<tDVRIPCameraInput>(x => kIpCameralst.Contains(x.KIPCamera)).ToList();
		//	_ipCamerauserList = db.Query<tDVRIPCameraUser>(x => kIpCameralst.Contains(x.KIPCamera)).ToList();
		//	List<int> kDvripCameralst = _iPCamInputList.Select(x => x.KDVRIPCamera).ToList();
		//	_ipReList = db.Query<tDVRIPRe>(x => kDvripCameralst.Contains(x.KDVRIPCamera)).ToList();
		//	return lsIpCams;
		//}

		//private void DeleteIpCamera(tDVRIPCamera ipCam)
		//{
		//	List<tDVRIPCameraInput> ipCaInpitlst = _iPCamInputList.Where(t => t.KIPCamera == ipCam.KIPCamera).ToList();
		//	List<tDVRIPCameraUser> ipCaUserLst = _ipCamerauserList.Where(t => t.KIPCamera == ipCam.KIPCamera).ToList();

		//	foreach (var ipusr in ipCaUserLst)
		//	{
		//		db.Delete<tDVRIPCameraUser>(ipusr);
		//	}

		//	foreach (var ipInp in ipCaInpitlst)
		//	{
		//		List<tDVRIPRe> ipRelst = _ipReList.Where(t => t.KDVRIPCamera == ipInp.KDVRIPCamera).ToList();
		//		foreach (var ipre in ipRelst)
		//		{
		//			db.Delete<tDVRIPRe>(ipre);
		//		}

		//		db.Delete<tDVRIPCameraInput>(ipInp);
		//	}
		//}

		//private void SetIpCameraInfo(Int32 kDvr, CameraInfo ca, ref tDVRIPCamera ipCam)
		//{
		//	ipCam.KDVR = kDvr;
		//	ipCam.MACAddress = ca.MACAddress;
		//	ipCam.IPAddress = ca.IP;
		//	ipCam.Port = ca.Port;
		//	ipCam.KIPModel = ca.Model;
		//	ipCam.DisplayName = ca.FullModelName;
		//	ipCam.ResolutionSupportedMask = (long)ca.SupportedResMask;
		//}

		//private bool CompareIpCameraInfo(Int32 kDvr, CameraInfo ca, tDVRIPCamera ipCam)
		//{
		//	bool result = ipCam.KDVR == kDvr &&
		//				  ipCam.MACAddress == ca.MACAddress &&
		//				  ipCam.IPAddress == ca.IP &&
		//				  ipCam.Port == ca.Port &&
		//				  ipCam.KIPModel == ca.Model &&
		//				  ipCam.DisplayName == ca.FullModelName &&
		//				  ipCam.ResolutionSupportedMask == (long) ca.SupportedResMask;
		//	return result;
		//}

		//private bool IsParameterInfo(CameraInfo camInfo)
		//{
		//	if (camInfo == null) return false;
		//	return camInfo.Params != null;
		//}

		//private List<ParamInfo> GetParamInfoList(CameraInfo camInfo)
		//{
		//	List<ParamInfo> ipCameraInfoList = IsParameterInfo(camInfo) ? camInfo.Params.ToList() : new List<ParamInfo>();
		//	return ipCameraInfoList;
		//}

		//private void SetIpCamInputs(tDVRIPCamera ipCam, CameraInfo camInfo)
		//{
		//	List<tDVRIPCameraInput> lsIpCamInputs = _iPCamInputList.Where(x => x.KIPCamera == ipCam.KIPCamera).ToList();
		//	List<ParamInfo> ipCameraInfoList = GetParamInfoList(camInfo);
		//	foreach (var ipCa in lsIpCamInputs)
		//	{
		//		tDVRIPCameraInput ipCamera = ipCa;
		//		ParamInfo ipcaInfo = ipCameraInfoList.FirstOrDefault(t => t.id == ipCamera.InputNo);
		//		if (ipcaInfo != null)
		//		{
		//			if (!CompareIpCamInputInfo(ipCam, ipcaInfo, ipCamera))
		//			{
		//				SetIpCamInputInfo(ipCam, ipcaInfo, ref ipCamera);
		//				db.Update<tDVRIPCameraInput>(ipCamera);
		//			}
		//			SetUserInfo(ipCam, ipcaInfo.UserName, ipcaInfo.Password);
		//			SetIpSupportedRes(ipCamera, ipcaInfo);
		//			ipCameraInfoList.Remove(ipcaInfo);
		//		}
		//		else
		//		{
		//			List<tDVRIPRe> ipRelst = _ipReList.Where(t => t.KDVRIPCamera == ipCamera.KDVRIPCamera).ToList();
		//			foreach (var ipre in ipRelst)
		//			{
		//				db.Delete<tDVRIPRe>(ipre);
		//			}
		//			db.Delete<tDVRIPCameraInput>(ipCamera);
		//		}
		//	}

		//	InsertNewIpCameraInputs(ipCam, ipCameraInfoList);
		//	//db.Save();

		//}

		//private void InsertNewIpCameraInputs(tDVRIPCamera ipCam, List<ParamInfo> ipCameraInfoList)
		//{
		//	foreach (var ipCa in ipCameraInfoList)
		//	{
		//		var ipCamInput = new tDVRIPCameraInput();
		//		SetIpCamInputInfo(ipCam, ipCa, ref ipCamInput);
		//		db.Insert<tDVRIPCameraInput>(ipCamInput);
		//		SetUserInfo(ipCam, ipCa.UserName, ipCa.Password);
		//		db.Save();
		//		//ipCamInput = db.Query<tDVRIPCameraInput>(t => t.KIPCamera == kIPCam).OrderByDescending(t => t.KDVRIPCamera).FirstOrDefault();
		//		//if (ipCamInput != null)
		//		SetIpSupportedRes(ipCamInput, ipCa);
		//	}
		//}

		//private void SetIpCamInputInfo(tDVRIPCamera ipCam, ParamInfo parInfo, ref tDVRIPCameraInput camInput)
		//{
		//	camInput.tDVRIPCamera = ipCam;
		//	camInput.InputNo = parInfo.id;
		//	camInput.VideoChannelNo = parInfo.VideoSource;
		//	camInput.FrameRate = parInfo.FrameRate;
		//	//camInput.KResolution = parInfo.Re;
		//	camInput.LocationName = parInfo.LocationName;
		//	camInput.AdapterName = parInfo.AdapterName;
		//	camInput.MaxFPS = parInfo.MaxFPS;
		//	camInput.ResolutionWidth = parInfo.ResolutionWidth;
		//	camInput.ResolutionHeight = parInfo.ResolutionHeight;
		//	string sResWidth = "0";
		//	string sResHeight = "0";

		//	if (parInfo.SupportedRes != null)
		//	{
		//		for (int k = 0; k < parInfo.SupportedRes.Count; k++)
		//		{
		//			sResWidth = sResWidth + ";" + parInfo.SupportedRes[k].Width.ToString();
		//			sResHeight = sResHeight + ";" + parInfo.SupportedRes[k].Height.ToString();
		//		}
		//	}
		//	camInput.ResWidthtMask = sResWidth;
		//	camInput.ResHeightMask = sResHeight;
		//}

		//private bool CompareIpCamInputInfo(tDVRIPCamera ipCam, ParamInfo parInfo, tDVRIPCameraInput camInput)
		//{
		//	bool result = camInput.tDVRIPCamera == ipCam &&
		//				  camInput.InputNo == parInfo.id &&
		//				  camInput.VideoChannelNo == parInfo.VideoSource &&
		//				  camInput.FrameRate == parInfo.FrameRate &&
		//				  camInput.LocationName == parInfo.LocationName &&
		//				  camInput.AdapterName == parInfo.AdapterName &&
		//				  camInput.MaxFPS == parInfo.MaxFPS &&
		//				  camInput.ResolutionWidth == parInfo.ResolutionWidth &&
		//				  camInput.ResolutionHeight == parInfo.ResolutionHeight;
		//	string sResWidth = "0";
		//	string sResHeight = "0";

		//	if (parInfo.SupportedRes != null)
		//	{
		//		for (int k = 0; k < parInfo.SupportedRes.Count; k++)
		//		{
		//			sResWidth = sResWidth + ";" + parInfo.SupportedRes[k].Width.ToString();
		//			sResHeight = sResHeight + ";" + parInfo.SupportedRes[k].Height.ToString();
		//		}
		//	}
		//	result = result &&
		//			 camInput.ResWidthtMask == sResWidth &&
		//			 camInput.ResHeightMask == sResHeight;
		//	return result;
		//}

		////private Int32 GetKResolution(Int32 kVideoFormat, int width, int heght)
		////{
		////	Int32 KRes = 0;

		////	return KRes;
		////}

		//private bool IsIpBodyData()
		//{
		//	return msgBody != null;
		//}

		//private bool IsIpData()
		//{
		//	if (!IsIpBodyData()) return false;
		//	return msgBody.ipData != null;
		//}

		//private bool IsIpCameraInfo()
		//{
		//	if (!IsIpData()) return false;
		//	return msgBody.ipData.IPCameras != null;
		//}

		//private bool IsResolutionInfo(ParamInfo ipcaInfo)
		//{
		//	if (ipcaInfo == null) return false;
		//	return ipcaInfo.SupportedRes != null;
		//}

		//private List<ResolutionInfo> GetResolutionList(ParamInfo ipcaInfo)
		//{
		//	List<ResolutionInfo> ipResInfoList = IsResolutionInfo(ipcaInfo) ? ipcaInfo.SupportedRes.ToList() : new List<ResolutionInfo>();
		//	return ipResInfoList;
		//}

		//private void SetIpSupportedRes(tDVRIPCameraInput kIpCam, ParamInfo ipcaInfo)
		//{
		//	List<tDVRIPRe> lsIpRes = _ipReList.Where(x => x.KDVRIPCamera == kIpCam.KDVRIPCamera).ToList();
		//	List<ResolutionInfo> ipResInfoList = GetResolutionList(ipcaInfo);
		//	foreach (var ipRes in lsIpRes)
		//	{
		//		tDVRIPRe ipResolution = ipRes;
		//		ResolutionInfo resolutionInfo = ipResInfoList.FirstOrDefault(x => x.Width == ipResolution.Width && x.Height == ipResolution.Height);
		//		if (resolutionInfo != null)
		//		{
		//			if (ipRes.tDVRIPCameraInput != kIpCam ||
		//				ipRes.Width != resolutionInfo.Width ||
		//				ipRes.Height != resolutionInfo.Height)
		//			{
		//				ipRes.tDVRIPCameraInput = kIpCam;
		//				ipRes.Width = resolutionInfo.Width;
		//				ipRes.Height = resolutionInfo.Height;
		//				db.Update<tDVRIPRe>(ipResolution);
		//			}
		//			ipResInfoList.Remove(resolutionInfo);
		//		}
		//		else
		//		{
		//			db.Delete<tDVRIPRe>(ipResolution);
		//		}
		//	}

		//	foreach (var ipRes in ipResInfoList)
		//	{
		//		var ipResolution = new tDVRIPRe {tDVRIPCameraInput = kIpCam, Width = ipRes.Width, Height = ipRes.Height};
		//		db.Insert<tDVRIPRe>(ipResolution);
		//	}
		//}

		//private void SetUserInfo(tDVRIPCamera ipCam, string sUser, string sPass)
		//{
		//	tDVRIPCameraUser ipUser = _ipCamerauserList.FirstOrDefault(x => x.KIPCamera == ipCam.KIPCamera);
		//	if (ipUser == null)
		//	{
		//		ipUser = new tDVRIPCameraUser {UserName = sUser, Password = sPass, tDVRIPCamera = ipCam};
		//		db.Insert<tDVRIPCameraUser>(ipUser);
		//	}
		//	else
		//	{
		//		if (String.Compare(sUser.Trim().ToUpper(), ipUser.UserName.Trim().ToUpper()) != 0 ||
		//			String.Compare(sPass, ipUser.Password) != 0)
		//		{
		//			if (ipUser.UserName != sUser ||
		//				ipUser.Password != sPass ||
		//				ipUser.tDVRIPCamera != ipCam)
		//			{
		//				ipUser.UserName = sUser;
		//				ipUser.Password = sPass;
		//				ipUser.tDVRIPCamera = ipCam;
		//				db.Update<tDVRIPCameraUser>(ipUser);
		//			}
		//		}
		//	}
		//}

		#endregion

	}

	#region XML Classes
	[XmlRoot(MessageDefines.STR_Body)]
	public class RawIPCameraBody
	{
		[XmlElement(MessageDefines.STR_Common)]
		public RawMsgCommon msgCommon { get; set; }
		/*
		[XmlElement(MessageDefines.STR_TimeChange)]
		public Int64 TimeChange { get; set; }

		[XmlElement(MessageDefines.STR_Checksum)]
		public Int64 Checksum { get; set; }

		[XmlElement(MessageDefines.STR_DVRTime)]
		public string DVRTime { get; set; }
		public DateTime dtDVRTime
		{
			get
			{
				return DateTime.ParseExact(DVRTime, MessageDefines.STR_DVR_DATETIME_FORMAT, CultureInfo.InvariantCulture);
			}
		}

		[XmlElement(MessageDefines.STR_ConfigID)]
		public Int32 ConfigID { get; set; }

		[XmlElement(MessageDefines.STR_ConfigName)]
		public string ConfigName { get; set; }
		*/
		[XmlElement(RawIPCameraConfig.STR_IPCamera)]
		public IPCameraData ipData { get; set; }

		[XmlElement(MessageDefines.STR_Whois)]
		public RawMsgWhois msgWhois { get; set; }
	}

	[XmlRoot(RawIPCameraConfig.STR_IPCamera)]
	public class IPCameraData
	{
		[XmlAttribute(RawIPCameraConfig.STR_UpdateRes)]
		public string _UpdateRes { get; set; }

		public Int32 UpdateRes {
			get
			{
				if (string.IsNullOrEmpty(_UpdateRes)) 
				{
					return 0; 
				}
				else return int.Parse(_UpdateRes);
			}
			set {
				_UpdateRes = value.ToString();
			}
		}

		[XmlArray(RawIPCameraConfig.STR_Cameras)]
		[XmlArrayItem(RawIPCameraConfig.STR_Camera)]
		public List<CameraInfo> IPCameras = new List<CameraInfo>();
	}

	[XmlRoot(RawIPCameraConfig.STR_Camera)]
	public class CameraInfo : IMessageEntity<tDVRIPCameras>
	{
		[XmlAttribute(MessageDefines.STR_id)]
		public Int32 id { get; set; }

		[XmlElement(MessageDefines.STR_IP)]
		public string IP { get; set; }

		[XmlElement(RawIPCameraConfig.STR_Port)]
		public int Port { get; set; }

		[XmlElement(RawIPCameraConfig.STR_Model)]
		public Int32 Model { get; set; }

		[XmlElement(RawIPCameraConfig.STR_FullModelName)]
		public string FullModelName { get; set; }

		[XmlElement(RawIPCameraConfig.STR_MACAddress)]
		public string MACAddress { get; set; }

		[XmlElement(RawIPCameraConfig.STR_SupportedResolution)]
		public UInt64 SupportedResMask { get; set; }

		[XmlElement(RawIPCameraConfig.STR_Mask)]
		public string SubnetMask { get; set; }

		[XmlElement(RawIPCameraConfig.STR_Gateway)]
		public string Gateway { get; set; }

		[XmlArray(RawIPCameraConfig.STR_Params)]
		[XmlArrayItem(RawIPCameraConfig.STR_Param)]
		public List<ParamInfo> Params = new List<ParamInfo>();

		public bool Equal(tDVRIPCameras value)
		{
			bool result =
					  value.MACAddress == MACAddress &&
					  value.IPAddress == IP &&
					  value.Port == Port &&
					  value.KIPModel == Model &&
					  value.DisplayName == FullModelName &&
					  value.ResolutionSupportedMask == (long)SupportedResMask;
			return result;
		}

		public void SetEntity(ref tDVRIPCameras value)
		{
			if (value == null)
				value = new tDVRIPCameras();
			value.MACAddress = MACAddress;
			value.IPAddress = IP;
			value.Port = Port;
			value.KIPModel = Model;
			value.DisplayName = FullModelName;
			//value.Gateway = Gateway;
			//value.SubnetMask = SubnetMask;
			value.ResolutionSupportedMask = (long)SupportedResMask;
		}
	}

	[XmlRoot(RawIPCameraConfig.STR_Param)]
	public class ParamInfo : IMessageEntity<tDVRIPCameraInputs>
	{
		[XmlAttribute(MessageDefines.STR_id)]
		public Int32 id { get; set; }

		[XmlElement(RawIPCameraConfig.STR_VideoSource)]
		public Int32 VideoSource { get; set; }

		[XmlElement(RawIPCameraConfig.STR_Resolution)]
		public short Resolution { get; set; }

		[XmlElement(RawIPCameraConfig.STR_ResolutionWidth)]
		public Int32 ResolutionWidth { get; set; }

		[XmlElement(RawIPCameraConfig.STR_ResolutionHeight)]
		public Int32 ResolutionHeight { get; set; }

		[XmlElement(RawIPCameraConfig.STR_FrameRate)]
		public Int32 FrameRate { get; set; }

		[XmlElement(RawIPCameraConfig.STR_UserName)]
		public string UserName { get; set; }

		[XmlElement(RawIPCameraConfig.STR_Password)]
		public string Password { get; set; }

		[XmlElement(RawIPCameraConfig.STR_LocationName)]
		public string LocationName { get; set; }

		[XmlElement(RawIPCameraConfig.STR_MaxFPS)]
		public Int32 MaxFPS { get; set; }

		[XmlArray(RawIPCameraConfig.STR_SupportedResolution)]
		[XmlArrayItem(RawIPCameraConfig.STR_Resolution)]
		public List<ResolutionInfo> SupportedRes = new List<ResolutionInfo>();

		[XmlElement(RawIPCameraConfig.STR_AdapterName)]
		public string AdapterName { get; set; }

		public bool Equal(tDVRIPCameraInputs value)
		{
			bool result = //value.tDVRIPCamera == ipCam &&
						  value.InputNo == id &&
						  value.VideoChannelNo == VideoSource &&
						  value.FrameRate == FrameRate &&
						  value.LocationName == LocationName &&
						  value.AdapterName == AdapterName &&
						  value.MaxFPS == MaxFPS &&
						  value.ResolutionWidth == ResolutionWidth &&
						  value.ResolutionHeight == ResolutionHeight;
			string sResWidth = "0";
			string sResHeight = "0";

			if (SupportedRes != null)
			{
				for (int k = 0; k < SupportedRes.Count; k++)
				{
					sResWidth = sResWidth + ";" + SupportedRes[k].Width;
					sResHeight = sResHeight + ";" + SupportedRes[k].Height;
				}
			}
			result = result &&
					 value.ResWidthtMask == sResWidth &&
					 value.ResHeightMask == sResHeight;
			return result;
		}

		public void SetEntity(ref tDVRIPCameraInputs value)
		{
			if (value == null)
				value = new tDVRIPCameraInputs();
			value.InputNo = id;
			value.VideoChannelNo = VideoSource;
			value.FrameRate = FrameRate;
			//camInput.KResolution = parInfo.Re;
			value.LocationName = LocationName;
			value.AdapterName = AdapterName;
			value.MaxFPS = MaxFPS;
			value.ResolutionWidth = ResolutionWidth;
			value.ResolutionHeight = ResolutionHeight;
			string sResWidth = "0";
			string sResHeight = "0";

			if (SupportedRes != null)
			{
				for (int k = 0; k < SupportedRes.Count; k++)
				{
					sResWidth = sResWidth + ";" + SupportedRes[k].Width;
					sResHeight = sResHeight + ";" + SupportedRes[k].Height;
				}
			}
			value.ResWidthtMask = sResWidth;
			value.ResHeightMask = sResHeight;
		}
	}

	[XmlRoot(RawIPCameraConfig.STR_Resolution)]
	public class ResolutionInfo
	{
		[XmlAttribute(RawIPCameraConfig.STR_Width)]
		public Int32 Width { get; set; }

		[XmlAttribute(RawIPCameraConfig.STR_Height)]
		public Int32 Height { get; set; }
	}
	#endregion
}

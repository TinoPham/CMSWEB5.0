using System;
using System.Collections.Generic;
using System.Data.Entity;
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
	public class RawISearchConfig : RawDVRConfig<RawISearchBody>
	{
		public const string STR_ISearchMask = "isearch_mask";

		public override async Task<Commons.ERROR_CODE> UpdateToDB()
		{
			if (DVRAdressBook == null)
				return await base.UpdateToDB();
			if (DVRAdressBook.tDVRChannels == null || !DVRAdressBook.tDVRChannels.Any())
			{
				db.Include<tDVRAddressBook, tDVRChannels>(DVRAdressBook, item => item.tDVRChannels);
			}
			List<tDVRChannels> lsChans = DVRAdressBook.tDVRChannels.ToList();
			bool isEnable = false;
			foreach (tDVRChannels chan in lsChans)
			{
				isEnable = (msgBody.ChannelMask & (((UInt64)1) << chan.ChannelNo)) > 0;
				if (chan.EnableiSearch != isEnable)
				{
					chan.EnableiSearch = isEnable;
					db.Update<tDVRChannels>(chan);
				}
			}

			if (UpdateChecksum((int)DVR_CONFIGURATION.EMS_CFG_ISEARCH, msgBody.msgCommon))
			{
				int ret = 0;
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}
			

			return await base.UpdateToDB();
		}
	}

	#region XML Classes
	[XmlRoot(MessageDefines.STR_Body)]
	public class RawISearchBody
	{
		[XmlElement(MessageDefines.STR_Common)]
		public RawMsgCommon msgCommon { get; set; }

		[XmlElement(RawISearchConfig.STR_ISearchMask)]
		public UInt64 ChannelMask { get; set; }
	}
	#endregion
}

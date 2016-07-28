using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using SVRDatabase;

namespace ConverterSVR.BAL.DVRConverter
{
	[XmlRoot(MessageDefines.STR_Message)]
	public class RawAllInfo : RawDVRConfig<RawAllBody>
	{
		public const string STR_NumberMessage = "number_message";

		//[XmlElement(STR_Header)]
		//public RawMsgHeader msgHeader { get; set; }

		//[XmlElement(STR_Body)]
		//public RawAllBody msgBody { get; set; }

		public RawAllInfo() { }

		public RawAllInfo(string strMsg)
		{
			RawAllInfo rw = Commons.ObjectUtils.DeSerialize(typeof(RawAllInfo), strMsg) as RawAllInfo;
			this.msgHeader = rw.msgHeader;
			this.msgBody = rw.msgBody;
		}
		//Anh Huynh, Update for DVR Express, Apr 14, 2015
		public override async Task<string> GetResponseMsg()
		{
			string msgResponse = Commons.Utils.String2Base64(GetResponseAllInfoMsg(msgHeader.DVRGuid));

			return await Task.FromResult<string>(msgResponse);
		}
		public override async Task<Commons.ERROR_CODE> UpdateToDB()
		{
			return await base.UpdateToDB();
		}

	}

	[XmlRoot(MessageDefines.STR_Body)]
	public class RawAllBody
	{
		[XmlElement(RawAllInfo.STR_NumberMessage)]
		public Int32 NumberMessage { get; set; }
	}
}

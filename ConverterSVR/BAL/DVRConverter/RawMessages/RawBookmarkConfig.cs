using System.Linq.Expressions;
using PACDMModel.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using SVRDatabase;

namespace ConverterSVR.BAL.DVRConverter
{
	[XmlRoot(MessageDefines.STR_Message)]
	public class RawBookmarkConfig : RawDVRConfig<RawBookmarkBody>
	{
		#region Parameter
		public const string STR_BookMarks = "book_marks";
		public const string STR_BookMark = "book_mark";
		public const string STR_ChannelMask = "channel_mask";
		//public const string STR_Time = "time";
		public const string STR_Description = "description";

		//[XmlElement(STR_Header)]
		//public RawMsgHeader msgHeader { get; set; }

		//[XmlElement(STR_Body)]
		//public RawBookmarkBody msgBody { get; set; }
		#endregion

		public override async Task<Commons.ERROR_CODE> UpdateToDB()
		{
			if (DVRAdressBook == null || msgBody.Bookmarks == null)
				return await base.UpdateToDB();

			if (UpdateBookmarks(DVRAdressBook, msgBody.Bookmarks))
			{
				int ret = 0;
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}

			//UpdateChecksum((int)DVR_CONFIGURATION.EMS_CFG_BOOK_MARK, msgBody.msgCommon.Checksum, msgBody.msgCommon.TimeChange, msgBody.msgCommon.dtDVRTime); //AAAA
			if (UpdateChecksum((int)DVR_CONFIGURATION.EMS_CFG_BOOK_MARK, msgBody.msgCommon))
			{
				int ret = 0;
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}
			return await base.UpdateToDB();
		}

		private bool UpdateBookmarks(tDVRAddressBook dvrAdressBook, List<BookMarkData> bookmarks)
		{
			db.Include<tDVRAddressBook, tDVRBookMark>(dvrAdressBook, item => item.tDVRBookMarks);
			Func<tDVRBookMark, BookMarkData, bool> func_filter = (dbitem, info) => dbitem.BmTime == info.BmTime && dbitem.ChannelMask == info.ChannelMask;
			Func<tDVRBookMark, BookMarkData, bool> compare_update = null;
			Expression<Func<tDVRBookMark, object>> updatedata = item => item.tDVRAddressBook;

			Expression<Func<tDVRBookMark, long>> db_key = dbitem => dbitem.BmTime;
			Expression<Func<BookMarkData, long>> info_key = info => info.BmTime;
			return base.UpdateDBData<tDVRBookMark, BookMarkData, long, long>(dvrAdressBook.tDVRBookMarks, bookmarks, func_filter, compare_update, updatedata, dvrAdressBook, db_key, info_key);
		}
	}

	#region XML Classes
	[XmlRoot(MessageDefines.STR_Body)]
	public class RawBookmarkBody
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
		[XmlArray(RawBookmarkConfig.STR_BookMarks)]
		[XmlArrayItem(RawBookmarkConfig.STR_BookMark)]
		public List<BookMarkData> Bookmarks { get; set; }

		[XmlElement(MessageDefines.STR_Whois)]
		public RawMsgWhois msgWhois { get; set; }
	}

	[XmlRoot(RawBookmarkConfig.STR_BookMark)]
	public class BookMarkData : IMessageEntity<tDVRBookMark>
	{
		[XmlElement(RawBookmarkConfig.STR_ChannelMask)]
		public UInt64 ChannelMask {get; set;}

		[XmlElement(MessageDefines.STR_Time)]
        public Int64 BmTime { get; set; }

		[XmlElement(RawBookmarkConfig.STR_Description)]
		public string Description {get; set;}

		public bool Equal(tDVRBookMark value)
		{
			bool result = value.ChannelMask == ChannelMask &&
							  value.BmTime == BmTime &&
							  value.Description == Description;
			return result;
		}

		public void SetEntity(ref tDVRBookMark value)
		{
			if (value == null)
				value = new tDVRBookMark();
			value.ChannelMask = ChannelMask;
			value.BmTime = BmTime;
			value.Description = Description;
		}
	}
	#endregion
}

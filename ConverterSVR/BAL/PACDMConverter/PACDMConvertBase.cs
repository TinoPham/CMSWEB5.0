using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.Wrappers;
using ConverterSVR.BAL.TransformMessages;
using ConvertMessage;
using PACDMModel;
using SVRDatabase;

namespace ConverterSVR.BAL.PACDMConverter
{
	internal abstract class ConvertBase
	{
		protected PACDMDB PACDB { get; set; }
		protected SVRManager LogDB { get; set; }
	}
	internal abstract class PACDMConvertBase : ConvertBase
	{
		protected MessageData MsgData{ get; private set;}
		//protected PACDMDB PACDB{ get; private set;}
		//protected SVRManager LogDB{ get; private set;}
		protected MediaTypeFormatter Formatter{ get; private set;}
		protected MessageDVRInfo DVRInfo { get; private set;}

		protected Wrapper Wrapper
		{
			get{ return Wrapper.Instance;}
		}

		public virtual MessageResult ConvertData()
		{
			return new MessageResult();
		}

		protected virtual MessageResult MessageDatatoMessageResult(Commons.ERROR_CODE errorID,  MessageData data)
		{
			return new MessageResult
			{
				ErrorID = errorID,
				Data = Commons.ObjectUtils.Serialize<MessageData>(Formatter, data)
			
			};
		}

		public PACDMConvertBase(PACDMDB pacdb, SVRManager logdb, MessageData msgdata, MessageDVRInfo dvrinfo, MediaTypeFormatter formatter)
		{
			PACDB = pacdb;
			LogDB = logdb;
			MsgData = msgdata;
			Formatter = formatter;
			DVRInfo = dvrinfo;
		}
		private PACDMModel.Model.tbl_POS_PACID UpdateLastConvert(PACDMModel.PACDMDB pacmodel, ConvertMessage.MessageDVRInfo DVRInfo)
		{
			PACDMModel.Model.tbl_POS_PACID pacid = pacmodel.Query<PACDMModel.Model.tbl_POS_PACID>().FirstOrDefault(item => item.KDVR == DVRInfo.KDVR);
			pacid.Last_Convert = DateTime.UtcNow;
			return pacid;
		}

		protected MessageResult SaveTransact<TMsg, TSql>(string msgdata, ITransformMessage<TMsg, TSql> itransform) where TMsg : class where TSql : class
		{
			try
			{
				TMsg msg_transact = Commons.ObjectUtils.DeSerialize<TMsg>(Formatter, msgdata);
				if (msg_transact == null)
					return MessageDatatoMessageResult(Commons.ERROR_CODE.SERVICE_CANNOT_PARSER_DATA, MsgData);
				TSql sqltrans = itransform.TransForm(msg_transact, DVRInfo);
				PACDB.Insert<TSql>(sqltrans);
				PACDMModel.Model.tbl_POS_PACID pac = UpdateLastConvert(PACDB, DVRInfo);
				PACDB.Update<PACDMModel.Model.tbl_POS_PACID>(pac);
				if( PACDB.Save() > 0)
				{
					UpdateWareHouse<TSql>(sqltrans, PACDB);
				}
				
				return MessageDatatoMessageResult(Commons.ERROR_CODE.OK, new MessageData { Programset = MsgData.Programset, Mapping = MsgData.Mapping, Data = Commons.Resources.ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.OK) });
			}
			catch (Exception ex)
			{
				return MessageDatatoMessageResult(Commons.ERROR_CODE.SERVICE_EXCEPTION, new MessageData { Programset = MsgData.Programset, Mapping = MsgData.Mapping, Data = ex.Message });
			}
		}

		protected virtual void UpdateWareHouse<Tsql>(Tsql Rawdata, PACDMModel.Model.IResposity pacdb) where Tsql : class
		{

		}

	}
}

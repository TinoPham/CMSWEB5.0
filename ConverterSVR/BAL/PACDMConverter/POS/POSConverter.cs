using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvertMessage;
using PACDMModel;
using SVRDatabase;
using ConverterSVR.BAL.TransformMessages;
using ConvertMessage.PACDMObjects.POS;
using PACDMModel.Model;
using System.Net.Http.Formatting;
using Commons;
namespace ConverterSVR.BAL.PACDMConverter.POS
{
	internal class POSConverter : PACDMConvertBase
	{
		public POSConverter(PACDMModel.PACDMDB pacdb, SVRManager logdb, MessageData msgdata, MessageDVRInfo dvrinfo, MediaTypeFormatter formatter)
			: base(pacdb, logdb, msgdata, dvrinfo, formatter)
		{}
		public override MessageResult ConvertData()
		{
			bool isTransact = string.Compare(base.MsgData.Mapping, typeof(tbl_POS_Transact).Name, true) == 0 || string.Compare(base.MsgData.Mapping, PACDMConverter.STR_Transact, true) == 0;
			MessageResult msg_result = isTransact? SaveTransact<Transact,tbl_POS_Transact>(base.MsgData.Data, POSTransactTransformMessage.Instance) : SaveTransact<Sensor,tbl_POS_Sensor>(base.MsgData.Data, POSSensorTransformMessage.Instance) ;
			return msg_result;
		}
		protected override void UpdateWareHouse<Tsql>(Tsql Rawdata, IResposity pacdb)
		{
			if (Rawdata is tbl_POS_Transact)
			{
				UpdateWareHouse( Rawdata as tbl_POS_Transact, pacdb);
				return;
			}
		}
		public void UpdateWareHouse(tbl_POS_Transact transact, IResposity pacdb)
		{
			if( transact == null || transact.TransID <= 0)
				return;
			Wrapper.DBWareHouse.UpdateWareHouse( transact, pacdb);
		}
		
	}
}

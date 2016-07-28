using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using ConverterSVR.BAL.TransformMessages;
using ConvertMessage;
using ConvertMessage.PACDMObjects.IOPC;
using PACDMModel.Model;
using SVRDatabase;

namespace ConverterSVR.BAL.PACDMConverter.IOPC
{
	internal class IOPCConverter : PACDMConvertBase
	{
		const string str_tbl_IOPC_TrafficCount = "tbl_IOPC_TrafficCount";
		const string str_tbl_IOPC_TrafficCountRegion = "tbl_IOPC_TrafficCountRegion";
		const string str_TrafficCountRegion = "TrafficCountRegion";
		const string str_TrafficCount = "TrafficCount";
		const string str_Alarm = "Alarm";
		const string STR_Count = "Count";
		const string STR_tbl_IOPC_Count = "tbl_IOPC_Count";
		const string str_tbl_IOPC_DriveThrough = "tbl_IOPC_DriveThrough";
		const string str_tbl_IOPC_Alarm = "tbl_IOPC_Alarm";
		const string str_DriveThrough = "DriveThrough";

	
		public IOPCConverter(PACDMModel.PACDMDB pacdb, SVRManager logdb, MessageData msgdata, MessageDVRInfo dvrinfo, MediaTypeFormatter formatter)
			: base(pacdb, logdb, msgdata, dvrinfo, formatter)
		{}
		public override MessageResult ConvertData()
		{
			//bool isTransact = string.Compare(base.MsgData.Mapping, typeof(tbl_POS_Transact).Name, true) == 0 || string.Compare(base.MsgData.Mapping, PACDMConverter.STR_Transact, true) == 0;
			MessageResult msg_result = new MessageResult{ ErrorID = Commons.ERROR_CODE.INVALID_MAPPING}; //isTransact? SaveTransact<Transact,tbl_POS_Transact>(base.MsgData.Data, POSTransactTransformMessage.Instance) : SaveTransact<Sensor,tbl_POS_Sensor>(base.MsgData.Data, POSSensorTransformMessage.Instance) ;

			//return msg_result;
			switch(MsgData.Mapping)
			{
				case STR_Count :
				case STR_tbl_IOPC_Count:
					msg_result = SaveTransact<Count, tbl_IOPC_Count>(MsgData.Data, IOPCCountTransformMessage.Instance);
					
				break;

				case str_tbl_IOPC_TrafficCount:
				case str_TrafficCount:
					msg_result = ConvertTrafficCount();
				break;
				case str_tbl_IOPC_TrafficCountRegion:
				case str_TrafficCountRegion:
					msg_result = ConvertTrafficCountRegion();
				break;
				case str_tbl_IOPC_DriveThrough:
				case str_DriveThrough:
					msg_result = SaveTransact<DriveThrough, tbl_IOPC_DriveThrough>(MsgData.Data, IOPCDrivethroughTransformMessage.Instance);
				break;
				case str_tbl_IOPC_Alarm:
				case str_Alarm:
					msg_result = SaveTransact<Alarm, tbl_IOPC_Alarm>(MsgData.Data, IOPCALarmTransformMessage.Instance);
				break;
			}
			return msg_result;
		}

		MessageResult ConvertTrafficCount()
		{
			return base.SaveTransact<TrafficCount, tbl_IOPC_TrafficCount>(base.MsgData.Data, IOPCTrafficCountTransformMessage.Instance);
		}
		MessageResult ConvertTrafficCountRegion()
		{
			try
			{
				TrafficCountRegion trafficRegion = Commons.ObjectUtils.DeSerialize<TrafficCountRegion>(Formatter, base.MsgData.Data);
				if (trafficRegion == null)
					return MessageDatatoMessageResult(Commons.ERROR_CODE.SERVICE_CANNOT_PARSER_DATA, MsgData);

				tbl_IOPC_TrafficCountRegion  tbl_trafficregion = IOPCTrafficCountRegionTransformMessage.Instance.TransForm(trafficRegion, base.DVRInfo);
				tbl_IOPC_TrafficCountRegion found = PACDB.FirstOrDefault<tbl_IOPC_TrafficCountRegion>( item => item.T_PACID == tbl_trafficregion.T_PACID && item.RegionID == tbl_trafficregion.RegionID && item.RegionNameID == tbl_trafficregion.RegionNameID && item.ExternalChannel == tbl_trafficregion.ExternalChannel);
				if( found != null)
					return new MessageResult{ ErrorID = Commons.ERROR_CODE.OK, Data = found.RegionIndex.ToString()};
				PACDB.Insert<tbl_IOPC_TrafficCountRegion>(tbl_trafficregion);
				PACDB.Save();
				if( tbl_trafficregion.RegionIndex > 0)
					return new MessageResult { ErrorID = Commons.ERROR_CODE.OK, Data = tbl_trafficregion.RegionIndex.ToString() };
				return new MessageResult { ErrorID = Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED, Data = tbl_trafficregion.RegionIndex.ToString() };
			}
			catch (Exception ex)
			{
				return MessageDatatoMessageResult(Commons.ERROR_CODE.SERVICE_EXCEPTION, new MessageData { Programset = MsgData.Programset, Mapping = MsgData.Mapping, Data = ex.Message });
			}
		}

		#region Update data warehouse
		protected override void UpdateWareHouse<Tsql>(Tsql Rawdata, IResposity pacdb)
		{
			//base.UpdateWareHouse<Tsql>(Rawdata, pacdb);
            switch (MsgData.Mapping)
            {
                case str_tbl_IOPC_DriveThrough:
				case str_DriveThrough:
                    if (Rawdata is tbl_IOPC_DriveThrough)
                    {
                        UpdateWareHouse_IOPC_DriveThrough(Rawdata as tbl_IOPC_DriveThrough, pacdb);
                    }
                    break;
                case STR_Count:
                case STR_tbl_IOPC_Count:
			if( Rawdata is tbl_IOPC_Count)
			{
				UpdateWareHouse_IOPC_Count( Rawdata as tbl_IOPC_Count, pacdb);
			}
                    break;
                case str_tbl_IOPC_TrafficCount:
                case str_TrafficCount:
                    if (Rawdata is tbl_IOPC_TrafficCount)
                    {
                        UpdateWareHouse_IOPC_TrafficCount(Rawdata as tbl_IOPC_TrafficCount, pacdb);
                    }
                    break;
                default:
                    break;

            }
			
		}

		private void UpdateWareHouse_IOPC_Count( tbl_IOPC_Count raw, IResposity pacdb)
		{
			if( raw == null || raw.Count_ID <= 0)
				return;
			Wrapper.DBWareHouse.UpdateWareHouse(raw, pacdb);

			//Fact_IOPC_Count fact = WarehouseManager.AddFactData<Fact_IOPC_Count, tbl_IOPC_Count>( pacdb, raw);
			//if( fact != null && fact.Count_ID > 0)
			//{
			//	Fact_IOPC_Periodic_Hourly_Traffic fact_period = WarehouseManager.AddFactData<Fact_IOPC_Periodic_Hourly_Traffic, Fact_IOPC_Count>( pacdb, fact);
			//}
		}
        private void UpdateWareHouse_IOPC_DriveThrough(tbl_IOPC_DriveThrough raw, IResposity pacdb)
        {
            if (raw == null)
                return;
            Wrapper.DBWareHouse.UpdateWareHouse(raw, pacdb);
        }

        private void UpdateWareHouse_IOPC_TrafficCount(tbl_IOPC_TrafficCount raw, IResposity pacdb)
        {
            if (raw == null)
                return;
            Wrapper.DBWareHouse.UpdateWareHouse(raw, pacdb);
        }

		#endregion
	}
}

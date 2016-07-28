using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvertMessage;
using ConvertMessage.PACDMObjects.IOPC;
using PACDMModel.Model;

namespace ConverterSVR.BAL.TransformMessages
{
	internal class IOPCTrafficCountRegionTransformMessage : Commons.SingletonClassBase<IOPCTrafficCountRegionTransformMessage>, ITransformMessage<TrafficCountRegion, tbl_IOPC_TrafficCountRegion>
	 {
		//private static readonly Lazy<IOPCTrafficCountRegionTransformMessage> Lazy = new Lazy<IOPCTrafficCountRegionTransformMessage>(() => new IOPCTrafficCountRegionTransformMessage());
		
		//public static IOPCTrafficCountRegionTransformMessage Instance { get { return Lazy.Value; } }

		public tbl_IOPC_TrafficCountRegion TransForm(TrafficCountRegion input, MessageDVRInfo DVRInfo)
		{
			tbl_IOPC_TrafficCountRegion output = new tbl_IOPC_TrafficCountRegion();
			output.DVRDate =  Commons.Utils.toSQLDate(input.DVRDate.HasValue? input.DVRDate.Value : DateTime.MaxValue);
			output.ExternalChannel = input.ExternalChannel;
			output.InternalChannel = input.InternalChannel;
			output.RegionNameID = input.RegionNameID;
			output.RegionID = input.RegionID;
			output.T_PACID = DVRInfo.KDVR;

			return output;
		}
	 }

	internal class IOPCALarmTransformMessage : Commons.SingletonClassBase<IOPCALarmTransformMessage>, ITransformMessage<Alarm, tbl_IOPC_Alarm>
	{
		//private static readonly Lazy<IOPCALarmTransformMessage> Lazy = new Lazy<IOPCALarmTransformMessage>(() => new IOPCALarmTransformMessage());

		//public static IOPCALarmTransformMessage Instance { get { return Lazy.Value; } }

		public tbl_IOPC_Alarm TransForm(Alarm input, MessageDVRInfo DVRInfo)
		{
			tbl_IOPC_Alarm output = new tbl_IOPC_Alarm();
			output.DVRDate = Commons.Utils.toSQLDate( input.DVRDate.HasValue ? input.DVRDate.Value : DateTime.MinValue);
			output.A_CameraNumber = input.A_CameraNumber;
			output.AlarmTypeID = input.AlarmTypeID;
			output.AreaID = input.AreaID;
			output.ObjectTypeID = input.ObjectTypeID;
			output.T_PACID = DVRInfo.KDVR;

			return output;
		}
	}

	internal class IOPCDrivethroughTransformMessage : Commons.SingletonClassBase<IOPCDrivethroughTransformMessage>, ITransformMessage<DriveThrough, tbl_IOPC_DriveThrough>
	{
		//private static readonly Lazy<IOPCDrivethroughTransformMessage> Lazy = new Lazy<IOPCDrivethroughTransformMessage>(() => new IOPCDrivethroughTransformMessage());

		//public static IOPCDrivethroughTransformMessage Instance { get { return Lazy.Value; } }

		public tbl_IOPC_DriveThrough TransForm(DriveThrough input, MessageDVRInfo DVRInfo)
		{
			tbl_IOPC_DriveThrough output = new tbl_IOPC_DriveThrough();
			output.EndDate = Commons.Utils.toSQLDate( input.EndDate);
			output.ExternalCamera = input.ExternalCamera;
			output.Function = input.Function;
			output.InternalCamera = input.InternalCamera;
			output.StartDate = Commons.Utils.toSQLDate( input.StartDate);
			output.T_PACID = DVRInfo.KDVR;

			return output;
		}
	}


	internal class IOPCTrafficCountTransformMessage : Commons.SingletonClassBase<IOPCTrafficCountTransformMessage>, ITransformMessage<TrafficCount, tbl_IOPC_TrafficCount>
	{
		//private static readonly Lazy<IOPCTrafficCountTransformMessage> Lazy = new Lazy<IOPCTrafficCountTransformMessage>(() => new IOPCTrafficCountTransformMessage());

		//public static IOPCTrafficCountTransformMessage Instance { get { return Lazy.Value; } }

		public tbl_IOPC_TrafficCount TransForm(TrafficCount input, MessageDVRInfo DVRInfo)
		{
			tbl_IOPC_TrafficCount output = new tbl_IOPC_TrafficCount();
			output.EventID = input.EventID;
			output.RegionIndex = input.RegionIndex;
			output.PersonID = input.PersonID;
			output.RegionEnterTime = Commons.Utils.toSQLDate( input.RegionEnterTime);
			output.RegionExitTime = Commons.Utils.toSQLDate( input.RegionExitTime);
			output.EventGUI =  Guid.NewGuid();
			return output;
		}
	}

	internal class IOPCCountTransformMessage : Commons.SingletonClassBase<IOPCCountTransformMessage>, ITransformMessage<Count, tbl_IOPC_Count>
	{
		//private static readonly Lazy<IOPCCountTransformMessage> Lazy = new Lazy<IOPCCountTransformMessage>(() => new IOPCCountTransformMessage());

		//public static IOPCCountTransformMessage Instance { get { return Lazy.Value; } }

		public tbl_IOPC_Count TransForm(Count input, MessageDVRInfo DVRInfo)
		{
			tbl_IOPC_Count output = new tbl_IOPC_Count();
			output.DVRDate = Commons.Utils.toSQLDate( input.DVRDate.HasValue ? input.DVRDate.Value : DateTime.MaxValue);
			output.C_CameraNumber = input.C_CameraNumber;
			output.C_AreaNameID = input.C_AreaNameID;
			output.C_Count = input.C_Count;
			output.C_ObjectTypeID = input.C_ObjectTypeID;
			output.T_PACID = DVRInfo.KDVR;

			return output;
		}
	}

}

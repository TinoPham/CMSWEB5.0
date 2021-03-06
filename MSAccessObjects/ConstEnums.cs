﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSAccessObjects
{
   public abstract class ConstEnums
    {
		public const string SubRetail = "SubRetail";
		public const string RetailKey = "RetailKey";
		public const string SR_2SubItemLineNb = "SR_2SubItemLineNb";
		public const string SR_1Qty = "SR_1Qty";
		public const string SR_0Amount = "SR_0Amount";
		public const string SR_Description = "SR_Description";
		public const string Retail = "Retail";
		public const string R_2ItemLineNb = "R_2ItemLineNb";
		public const string R_1Qty = "R_1Qty";
		public const string R_0Amount= "R_0Amount";
		public const string R_Description = "R_Description";
		public const string R_ItemCode = "R_ItemCode";
		public const string R_RetailXString1 = "R_RetailXString1";
		public const string R_RetailXString2 = "R_RetailXString2";
		public const string R_RetailXNbInt = "R_RetailXNbInt";
		public const string R_RetailXNbFloat = "R_RetailXNbFloat";
		public const string R_DVRDate = "R_DVRDate";
		public const string R_DVRTime = "R_DVRTime";
		public const string R_CameraNB = "R_CameraNB";
		public const string R_TOBox = "R_TOBox";
		public const string TransactKey = "TransactKey";
		public const string Transact = "Transact";
		public const string T_0TransNB  = "T_0TransNB";
		public const string  T_6TotalAmount = "T_6TotalAmount";
		public const string T_1SubTotal = "T_1SubTotal";
		public const string T_MethOfPaymID = "T_MethOfPaymID";
		public const string T_2Tax1Amount = "T_2Tax1Amount";
		public const string T_3Tax2Amount = "T_3Tax2Amount";
		public const string T_4Tax3Amount = "T_4Tax3Amount";
		public const string T_5Tax4Amount = "T_5Tax4Amount";
		public const string T_7PaymAmount = "T_7PaymAmount";
		public const string T_8ChangeAmount = "T_8ChangeAmount";
		public const string TransDate = "TransDate";
		public const string TransTime = "TransTime";
		public const string DVRDate = "DVRDate";
		public const string DVRTime = "DVRTime";
		public const string T_9RecItemCount = "T_9RecItemCount";
		public const string T_CameraNB = "T_CameraNB";
		public const string T_OperatorID = "T_OperatorID";
		public const string T_StoreID = "T_StoreID";
		public const string T_TerminalID = "T_TerminalID";
		public const string T_RegisterID = "T_RegisterID";
		public const string T_ShiftID = "T_ShiftID";
		public const string T_CheckID = "T_CheckID";
		public const string T_CardID = "T_CardID";
		public const string T_TransXString1 = "T_TransXString1";
		public const string T_TransXString2  = "T_TransXString2";
		public const string T_TransXNbInt = "T_TransXNbInt";
		public const string T_TransXNbFloat  = "T_TransXNbFloat";
		public const string T_00TransNBText = "T_00TransNBText";
		public const string T_PACID = "T_PACID";
		public const string T_TOBox = "T_TOBox";
		public const string Sensor = "Sensor";
		public const string S_ID = "S_ID";
		public const string OT_Start = "OT_Start";
		public const string OT_End = "OT_End";
		public const string GT_Start = "GT_Start";
		public const string GT_End = "GT_End";
		public const string PU_Start = "PU_Start";
		public const string PU_End = "PU_End";
		public const string PAC_ID = "PAC_ID";
		public const string T_TransCode = "T_TransCode";
		public const string T_TransAmount = "T_TransAmount";
		public const string T_TransTermFee = "T_TransTermFee";
		public const string T_TransType = "T_TransType";
		public const string T_TransTotal = "T_TransTotal";
		public const string BusinessDate = "BusinessDate";
		public const string T_CardNB = "T_CardNB";
		public const string T_AcctBalance = "T_AcctBalance";
		public const string T_TranType = "T_TranType";
		public const string T_UnitID = "T_UnitID";
		public const string T_SiteID = "T_SiteID";
		public const string T_DevName = "T_DevName";
		public const string T_Batch = "T_Batch";
		public const string T_Card = "T_Card";
		public const string T_FirstName = "T_FirstName";
		public const string T_LastName = "T_LastName";
		public const string T_XString1 = "T_XString1";
		public const string T_XString2 = "T_XString2";
		public const string Info = "Info";
		public const string LPR_ID = "LPR_ID";
		public const string LPR_CamNo = "LPR_CamNo";
		public const string LPR_NUM = "LPR_NUM";
		public const string LPR_JPEG_FILE_NAME = "LPR_JPEG_FILE_NAME";
		public const string LPR_Possibility = "LPR_Possibility";
		public const string LPR_PACID = "LPR_PACID";
		public const string LPR_isMatch = "LPR_isMatch";
		public const string AttendDay = "AttendDay";
		public const string AttendTime = "AttendTime";
		public const string ID = "ID";
		public const string Obj_Label = "Obj_Label";
		public const string Mark = "Mark";
		public const string ActiveTime = "ActiveTime";
		public const string ActiveDay = "ActiveDay";
		public const string A_ObjectType = "A_ObjectType";
		public const string A_AreaName = "A_AreaName";
		public const string ActiveInfo = "ActiveInfo";
		public const string Alarm = "Alarm";
		public const string A_CameraNumber = "A_CameraNumber";
		public const string A_AlarmType = "A_AlarmType";
		public const string Count = "Count";
		public const string C_CameraNumber = "C_CameraNumber";
		public const string C_AreaName = "C_AreaName";
		public const string C_ObjectType = "C_ObjectType";
		public const string C_Count = "C_Count";
		public const string DriveThrough = "DriveThrough";
		public const string TD_ID = "TD_ID";
		public const string StartTime = "StartTime";
		public const string StartDate = "StartDate";
		public const string EndTime = "EndTime";
		public const string EndDate = "EndDate";
		public const string ExternalCamera = "ExternalCamera";
		public const string InternalCamera = "InternalCamera";
		public const string Function = "Function";
		public const string PACID = "PACID";
		public const string Qtime = "Qtime";
		public const string EventID = "EventID";
		public const string PersonID = "PersonID";
		public const string ServiceEnterTime = "ServiceEnterTime";
		public const string RegisterEnterTime = "RegisterEnterTime";
		public const string RegisterExitTime = "RegisterExitTime";
		public const string PickupEnterTime = "PickupEnterTime";
		public const string PickupExitTime =  "PickupExitTime";
		public const string ServiceExitTime = "ServiceExitTime";
		public const string ExternalChannel = "ExternalChannel";
		public const string InternalChannel = "InternalChannel";
		public const string TrafficCount = "TrafficCount";
		public const string RegionID = "RegionID";
		public const string RegionEnterTime = "RegionEnterTime";
		public const string RegionExitTime = "RegionExitTime";
		public const string C_SetupTime = "C_SetupTime";
		public const string C_SetupDate = "C_SetupDate";
		public const string Index = "Index";
		public const string CountingSetup = "CountingSetup";
		public const string TrafficCountRegion = "TrafficCountRegion";
		public const string RegionName = "RegionName";
		
		public const string DateModified = "DateModified";
		public const string TimeModified = "TimeModified";

    }
}

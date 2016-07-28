using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration.Configuration;
using MongoDB.Bson.Serialization.Attributes;
namespace ConverterDB.Model
{
	partial class DBModel: DbContext
	{
		public DBModel() : base(){}

		public DBModel(string NameorConnectionString) : base(NameorConnectionString){}

		public DbSet<POSCheckID> POSCheckIDs  {get; set;}

		public DbSet<POSShift> POSShifts  {get; set;}

		public DbSet<POSRegister> POSRegisters  {get; set;}

		public DbSet<POSCameraNB> POSCameraNBs  {get; set;}

		public DbSet<POSExtraName> POSExtraNames  {get; set;}

		public DbSet<POSDescription> POSDescriptions  {get; set;}

		public DbSet<POSStore> POSStores  {get; set;}

		public DbSet<POSOperator> POSOperators  {get; set;}

		public DbSet<POSExtraStringValue> POSExtraStringValues  {get; set;}

		public DbSet<POSCardID> POSCardIDs  {get; set;}

		public DbSet<POSTaxes> POSTaxess  {get; set;}

		public DbSet<POSItemCode> POSItemCodes  {get; set;}

		public DbSet<POSPayment> POSPayments  {get; set;}

		public DbSet<POSTerminal> POSTerminals  {get; set;}

		public DbSet<IOPCTrafficCountRegionName> IOPCTrafficCountRegionNames  {get; set;}

		public DbSet<IOPCAlarmObjectType> IOPCAlarmObjectTypes  {get; set;}

		public DbSet<IOPCAlarmAlarmType> IOPCAlarmAlarmTypes  {get; set;}

		public DbSet<IOPCAlarmArea> IOPCAlarmAreas  {get; set;}

		public DbSet<IOPCCountArea> IOPCCountAreas  {get; set;}

		public DbSet<IOPCCountObjectType> IOPCCountObjectTypes  {get; set;}

		public DbSet<ATMTransType> ATMTransTypes  {get; set;}

		public DbSet<ATMXString> ATMXStrings  {get; set;}

		public DbSet<ATMExtraName> ATMExtraNames  {get; set;}

		public DbSet<ATMTransCode> ATMTransCodes  {get; set;}

		public DbSet<ATMCardNB> ATMCardNBs  {get; set;}

		public DbSet<CAUnitID> CAUnitIDs  {get; set;}

		public DbSet<CACard> CACards  {get; set;}

		public DbSet<CAExtraName> CAExtraNames  {get; set;}

		public DbSet<CATranType> CATranTypes  {get; set;}

		public DbSet<CASiteID> CASiteIDs  {get; set;}

		public DbSet<CAName> CANames  {get; set;}

		public DbSet<CABatch> CABatchs  {get; set;}

		public DbSet<CADevName> CADevNames  {get; set;}

		public DbSet<CAFullName> CAFullNames  {get; set;}

		public DbSet<CAXString> CAXStrings  {get; set;}

		public DbSet<DVRMessage> DVRMessages  {get; set;}

		public DbSet<Log> Logs  {get; set;}

		public DbSet<ConvertInfo> ConvertInfos  {get; set;}

		public DbSet<ServiceConfig> ServiceConfigs  {get; set;}

		public DbSet<DVRConverter> DVRConverters  {get; set;}

	}
	[BsonIgnoreExtraElements]
	public abstract partial class ItemBase
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int ID { get; set;}

		[MaxLength(150)]
		public virtual string Name { get; set;}

	}

	[Table("tbl_POS_CheckIDList")]
	[BsonIgnoreExtraElements]
	public partial class POSCheckID: ItemBase
	{
	}

	[Table("tbl_POS_ShiftList")]
	[BsonIgnoreExtraElements]
	public partial class POSShift: ItemBase
	{
	}

	[Table("tbl_POS_RegisterList")]
	[BsonIgnoreExtraElements]
	public partial class POSRegister: ItemBase
	{
	}

	[Table("tbl_POS_CameraNBList")]
	[BsonIgnoreExtraElements]
	public partial class POSCameraNB: ItemBase
	{
	}

	[Table("tbl_POS_ExtraName")]
	[BsonIgnoreExtraElements]
	public partial class POSExtraName: ItemBase
	{
	}

	[Table("tbl_POS_DescriptionList")]
	[BsonIgnoreExtraElements]
	public partial class POSDescription: ItemBase
	{
		[MaxLength(250)]
		public override string Name{ get { return base.Name;} set { base.Name = value;} }

	}

	[Table("tbl_POS_StoreList")]
	[BsonIgnoreExtraElements]
	public partial class POSStore: ItemBase
	{
	}

	[Table("tbl_POS_OperatorList")]
	[BsonIgnoreExtraElements]
	public partial class POSOperator: ItemBase
	{
	}

	[Table("tbl_POS_ExtraStringValue")]
	[BsonIgnoreExtraElements]
	public partial class POSExtraStringValue: ItemBase
	{
		[MaxLength(250)]
		public override string Name{ get { return base.Name;} set { base.Name = value;} }

	}

	[Table("tbl_POS_CardIDList")]
	[BsonIgnoreExtraElements]
	public partial class POSCardID: ItemBase
	{
	}

	[Table("tbl_POS_TaxesList")]
	[BsonIgnoreExtraElements]
	public partial class POSTaxes: ItemBase
	{
		[MaxLength(150)]
		public override string Name{ get { return base.Name;} set { base.Name = value;} }

	}

	[Table("tbl_POS_ItemCodeList")]
	[BsonIgnoreExtraElements]
	public partial class POSItemCode: ItemBase
	{
	}

	[Table("tbl_POS_PaymentList")]
	[BsonIgnoreExtraElements]
	public partial class POSPayment: ItemBase
	{
	}

	[Table("tbl_POS_TerminalList")]
	[BsonIgnoreExtraElements]
	public partial class POSTerminal: ItemBase
	{
	}

	[Table("tbl_IOPC_TrafficCountRegionName")]
	[BsonIgnoreExtraElements]
	public partial class IOPCTrafficCountRegionName: ItemBase
	{
	}

	[Table("tbl_IOPC_AlarmObjectType")]
	[BsonIgnoreExtraElements]
	public partial class IOPCAlarmObjectType: ItemBase
	{
	}

	[Table("tbl_IOPC_AlarmAlarmType")]
	[BsonIgnoreExtraElements]
	public partial class IOPCAlarmAlarmType: ItemBase
	{
	}

	[Table("tbl_IOPC_AlarmArea")]
	[BsonIgnoreExtraElements]
	public partial class IOPCAlarmArea: ItemBase
	{
	}

	[Table("tbl_IOPC_Count_Area")]
	[BsonIgnoreExtraElements]
	public partial class IOPCCountArea: ItemBase
	{
	}

	[Table("tbl_IOPC_Count_ObjectType")]
	[BsonIgnoreExtraElements]
	public partial class IOPCCountObjectType: ItemBase
	{
	}

	[Table("tbl_ATM_TransTypeList")]
	[BsonIgnoreExtraElements]
	public partial class ATMTransType: ItemBase
	{
	}

	[Table("tbl_ATM_XStringList")]
	[BsonIgnoreExtraElements]
	public partial class ATMXString: ItemBase
	{
	}

	[Table("tbl_ATM_ExtraName")]
	[BsonIgnoreExtraElements]
	public partial class ATMExtraName: ItemBase
	{
	}

	[Table("tbl_ATM_TransCodeList")]
	[BsonIgnoreExtraElements]
	public partial class ATMTransCode: ItemBase
	{
	}

	[Table("tbl_ATM_CardNBList")]
	[BsonIgnoreExtraElements]
	public partial class ATMCardNB: ItemBase
	{
	}

	[Table("tbl_CA_UnitIDList")]
	[BsonIgnoreExtraElements]
	public partial class CAUnitID: ItemBase
	{
	}

	[Table("tbl_CA_CardList")]
	[BsonIgnoreExtraElements]
	public partial class CACard: ItemBase
	{
	}

	[Table("tbl_CA_ExtraName")]
	[BsonIgnoreExtraElements]
	public partial class CAExtraName: ItemBase
	{
	}

	[Table("tbl_CA_TranTypeList")]
	[BsonIgnoreExtraElements]
	public partial class CATranType: ItemBase
	{
		[MaxLength(150)]
		public override string Name{ get { return base.Name;} set { base.Name = value;} }

	}

	[Table("tbl_CA_SiteIDList")]
	[BsonIgnoreExtraElements]
	public partial class CASiteID: ItemBase
	{
	}

	[Table("tbl_CA_NameList")]
	[BsonIgnoreExtraElements]
	public partial class CAName: ItemBase
	{
	}

	[Table("tbl_CA_BatchList")]
	[BsonIgnoreExtraElements]
	public partial class CABatch: ItemBase
	{
	}

	[Table("tbl_CA_DevNameList")]
	[BsonIgnoreExtraElements]
	public partial class CADevName: ItemBase
	{
	}

	[Table("tbl_CA_FullNameList")]
	[BsonIgnoreExtraElements]
	public partial class CAFullName: ItemBase
	{
		[MaxLength(255)]
		public override string Name{ get { return base.Name;} set { base.Name = value;} }

		[MaxLength(255)]
		public string LastName { get; set;}

	}

	[Table("tbl_CA_XStringList")]
	[BsonIgnoreExtraElements]
	public partial class CAXString: ItemBase
	{
		[MaxLength(150)]
		public override string Name{ get { return base.Name;} set { base.Name = value;} }

	}

	[Table("DVRMessages")]
	[BsonIgnoreExtraElements]
	public partial class DVRMessage
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Int64 ID { get; set;}

		public int MessageID { get; set;}

		public byte[] MessageBody { get; set;}

		public DateTime DvrDate { get; set;}

		public short Direction { get; set;}

	}

	[Table("Logs")]
	[BsonIgnoreExtraElements]
	public partial class Log
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Int64 ID { get; set;}

		public int LogID { get; set;}

		public string Message { get; set;}

		public Boolean Owner { get; set;}

		public Byte ProgramSet { get; set;}

		public DateTime DVRDate { get; set;}

		[MaxLength(255)]
		public string MsgClass { get; set;}

	}

	[Table("ConvertInfos")]
	[BsonIgnoreExtraElements]
	public partial class ConvertInfo
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int ID { get; set;}

		public byte Programset { get; set;}

		[MaxLength(50)]
		public string TableName { get; set;}

		public DateTime DvrDate { get; set;}

		[MaxLength(25)]
		public string LastKey { get; set;}

		public byte Order { get; set;}

		public DateTime UpdateDate { get; set;}

		public bool Enable { get; set;}

	}

	[Table("ServiceConfigs")]
	[BsonIgnoreExtraElements]
	public partial class ServiceConfig
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int ID { get; set;}

		[MaxLength(255)]
		public string Url { get; set;}

		public int Interval { get; set;}

		public int LogRecycle { get; set;}

		public int NumDVRMsg { get; set;}

		[MaxLength(100)]
		public string ServerID { get; set;}

		public Int64 KeepAliveToken { get; set;}

	}

	[Table("DVRConverters")]
	[BsonIgnoreExtraElements]
	public partial class DVRConverter
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int ID { get; set;}

		public int DvrSocketRetry { get; set;}

		public int TCPPort { get; set;}

		public bool Enable { get; set;}

	}

}


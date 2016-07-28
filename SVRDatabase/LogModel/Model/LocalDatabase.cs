
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
namespace SVRDatabase.Model
{
	partial class SVRModel: DbContext
	{
		public SVRModel() : base(){}

		public SVRModel(string NameorConnectionString) : base(NameorConnectionString){}

		public DbSet<DVRMessage> DVRMessages  {get; set;}

		public DbSet<Log> Logs  {get; set;}

		public DbSet<DBConfig> DBConfigs  {get; set;}

		public DbSet<ServiceConfig> ServiceConfigs  {get; set;}

		public DbSet<DVRInfo> DVRInfos  {get; set;}

		public DbSet<DVRDetail> DVRDetails  {get; set;}

		public DbSet<SeqCount> SeqCounts  {get; set;}

		public DbSet<ApiUser> ApiUsers  {get; set;}

		public DbSet<AuthToken> AuthTokens  {get; set;}

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

	}

	[Table("Logs")]
	[BsonIgnoreExtraElements]
	public partial class Log
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Int64 ID { get; set;}

		public int LogID { get; set;}

		[Required]
		public string Message { get; set;}

		public byte[] MessageData { get; set;}

		public Byte ProgramSet { get; set;}

		public DateTime DVRDate { get; set;}

	}

	[Table("DBConfigs")]
	[BsonIgnoreExtraElements]
	public partial class DBConfig
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Int64 ID { get; set;}

		[Required]
		[MaxLength(250)]
		public string Name { get; set;}

		[Required]
		[MaxLength(250)]
		public string Server { get; set;}

		[Required]
		[MaxLength(50)]
		public string UserID { get; set;}

		[Required]
		[MaxLength(50)]
		public string Password { get; set;}

		public bool Trusted { get; set;}

		public DateTime CreateDate { get; set;}

		public DateTime LastEditDate { get; set;}

		public bool IsActive { get; set;}

	}

	[Table("ServiceConfigs")]
	[BsonIgnoreExtraElements]
	public partial class ServiceConfig
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Int64 ID { get; set;}

		public Int16 ConverterLimit { get; set;}

		[MaxLength(250)]
		public string DVRAuthenticate { get; set;}

		public Int16 ConverterInterval { get; set;}

		public bool AuthenticateMode { get; set;}

		public Int16 KeepAliveInterval { get; set;}

	}

	[Table("DVRInfos")]
	[BsonIgnoreExtraElements]
	public partial class DVRInfo
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Int64 ID { get; set;}

		[MaxLength(256)]
		public string HASPK { get; set;}

		public Int64 KDVR { get; set;}

		public Nullable<DateTime> LastConvert { get; set;}

		public Nullable<DateTime> LastDvrMessage { get; set;}

		public Nullable<DateTime> CreateDate { get; set;}

		public bool ForceStop { get; set;}

		public Int16 ConverterInterval { get; set;}

		[MaxLength(256)]
		public String HostName { get; set;}

		public bool Locked { get; set;}

		[MaxLength(256)]
		public String Note { get; set;}

	}

	[Table("DVRDetails")]
	[BsonIgnoreExtraElements]
	public partial class DVRDetail
	{
		public Int64 ID { get; set;}

		public Int64 IDInfo { get; set;}

		[MaxLength(50)]
		public string MACAddress { get; set;}

		[MaxLength(50)]
		public String IPAddress { get; set;}

		public bool IPV4 { get; set;}

	}

	[Table("SeqCounts")]
	[BsonIgnoreExtraElements]
	public partial class SeqCount
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Int64 ID { get; set;}

		[MaxLength(50)]
		public string SequenceName { get; set;}

		public Int64 Sequence { get; set;}

	}

	[Table("ApiUser")]
	[BsonIgnoreExtraElements]
	public partial class ApiUser
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Int64 ID { get; set;}

		[MaxLength(50)]
		public string Name { get; set;}

		[MaxLength(50)]
		public string UserName { get; set;}

		[MaxLength(50)]
		public string Password { get; set;}

	}

	[Table("AuthToken")]
	[BsonIgnoreExtraElements]
	public partial class AuthToken
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Int64 ID { get; set;}

		[MaxLength(500)]
		public string Token { get; set;}

		public Nullable<DateTime> Expiration { get; set;}

		public Int64 ApiUserId { get; set;}

	}

}


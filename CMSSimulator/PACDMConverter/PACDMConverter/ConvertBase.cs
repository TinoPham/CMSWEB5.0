using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Commons;
using ConverterDB;
using PACDMSimulator.PACDMConverter;
using System.Net.Http;
using ConvertMessage;
using ConverterDB.Model;
using System.Xml.Serialization;
using System.Data;
using System.Data.OleDb;
using System.Net.Http.Formatting;
using MSAccessObjects;
using System.Threading;

namespace PACDMSimulator.PACDMConverter
{
	internal abstract class ConvertBase : IDisposable
	{
		public const int Sleep_TimeOut = 10;
		const string Resource_FileName = "Convert.xml";
		const string STR_Items = "Items";
		const string STR_Item = "Item";
		const string STR_Column = "Column";
		const string STR_SqlTable = "SqlTable";
		const string STR_ContextType = "ContextType";
		const string STR_Config = "Config";
		const string STR_Object = "Object";
		const string STR_src = "src";
		const string STR_des = "des";
		const string STR_Table = "table";
		const string STR_ConvertBase = "ConvertBase";
		const string STR_ItemKey = "ItemKey";
		const string STR_Convert_Item_key_Namespace = "PACDMSimulator.PACDMConverter.ConvertItemKeys";

		const string STR_DBFILE_EXTENSION = "*.mdb";
		
		public enum ConvertMode : int
		{
			/// <summary>
			/// Convert all data of program set before change to next one
			/// </summary>
			Programset = 0,
			/// <summary>
			///Change to next program set when the date is completed
			/// </summary>
			Date
		}

		protected MSAccessObjects.MSAccess RawDB;
		#if DATE_MODE
		protected ConvertMode convertmode = ConvertMode.Date;
		#else
		protected ConvertMode convertmode = ConvertMode.Programset;
		#endif
		
		public delegate void DelegateConvertData( object sender, Events.ConvertDBEventArgs args);

		public event DelegateConvertData OnConvertDataEvent;

		public bool Done{ get; protected set;}

		protected string ResourcePath{ get; set; }

		protected Programset ProgramSet { get;set;}
		
		protected string DataPath{ get{ return  Path.Combine(DVRInfos.Instance.PACDMInfo.PACDir, Programset.POS == ProgramSet? string.Empty : ProgramSet.ToString()); }}

		protected ConvertDB LocalDb { get; set;}

		//public volatile bool Stop = false;
		protected Dictionary<Type,ConvertItemBase> ConvertItemBaseMap = new Dictionary<Type,ConvertItemBase>();
		protected Dictionary<string, ItemKeyConfig> ItemKeyMapping = new Dictionary<string, ItemKeyConfig>();

		protected Dictionary<string, ItemObject> ObjectMapping = new Dictionary<string,ItemObject>();

		protected HttpClientSingleton webservice;
		
		protected readonly CancellationToken CancellingToken;

		protected  MediaTypeFormatter DataFormatter = new JsonMediaTypeFormatter();
		protected List<DataFileInfo> DataFiles;


		public ConvertBase(ConvertDB LocalDB, Programset pset, HttpClientSingleton httpclient, string Resourcepath, CancellationToken CancelToken, MediaTypeFormatter formatter = null)
		{
			if( formatter != null)
				DataFormatter = formatter;
			CancellingToken = CancelToken;
			RawDB = new MSAccessObjects.MSAccess();
			ProgramSet = pset;
			LocalDb = LocalDB;
			webservice = httpclient;
			ResourcePath = Resourcepath;
			LoadItemKeyMap(Resourcepath, ProgramSet);
			DataFiles = GetDataFileList(DataPath);
		}
		protected void TriggerConvertDataEvent( object sender, Commons.ERROR_CODE ErrorCode, string message)
		{
			if( OnConvertDataEvent == null)
				return;

			OnConvertDataEvent( sender, new Events.ConvertDBEventArgs{ Programset = this.ProgramSet, Sender = sender, ErrorCode = ErrorCode, Message = message });
		}
		public virtual ConvertMessage.MessageResult ConvertData()
		{
			return new MessageResult{ ErrorID = ERROR_CODE.OK};
		}

		public virtual void Dispose()
		{
			if( ItemKeyMapping != null)
			{
				ItemKeyMapping.Clear();
				ItemKeyMapping = null;
			}
			if( ObjectMapping != null)
			{
				ObjectMapping.Clear();
				ObjectMapping = null;
			}
		}
		
		protected MessageResult TransferTrans<T>(T data)
		{
			MessageData msg = new MessageData(){ Programset = this.ProgramSet, Mapping =  typeof(T).Name , Data = Commons.ObjectUtils.Serialize<T>( DataFormatter, data) };
			return webservice.PostData<MessageData>(msg, DataFormatter);
		}

		private ConverterDB.Model.ItemBase AddItemKey( ItemKeyConfig itemconfig, string value)
		{
			MessageItemKey itemkey = new MessageItemKey{
			ID = 0,
			Name = value
			};
			itemkey = RequestItemkey(itemkey, itemconfig.SQLTable);

			ItemBase retitem = (ItemBase)ObjectUtils.InitObject(itemconfig.DBSetType);
			retitem.ID = itemkey.ID;
			retitem.Name = itemkey.Name;
			SaveItemKey(retitem);
			return retitem ;
		}
		
		protected void SaveItemKey( ItemBase item )
		{
			if (item.ID > 0)
			{
				LocalDb.Insert(item.GetType(), item);
				LocalDb.Save();
			}
		}
		protected ItemBase FindItemKey( Type dbsetType, Func<dynamic, bool> predicate)
		{
			return LocalDb.FirstOrDefault(dbsetType, predicate);
		}

		protected MessageItemKey RequestItemkey(MessageItemKey msgItem, string sqltable)
		{
			MessageItemKey retitem = Commons.ObjectUtils.InitObject( msgItem.GetType()) as MessageItemKey;
			MessageData msg = new MessageData{ Programset = this.ProgramSet, Data = Commons.ObjectUtils.Serialize(this.DataFormatter,msgItem.GetType() , msgItem), Mapping = sqltable };
			ConvertMessage.MessageResult response = webservice.PostData(msg, msg.GetType(), this.DataFormatter);

			if (response.ErrorID ==  ERROR_CODE.OK)
				retitem = Commons.ObjectUtils.DeSerialize(this.DataFormatter, msgItem.GetType(), response.Data) as MessageItemKey;
				
				
			else
			{
				LocalDb.AddLog(new ConverterDB.Model.Log { DVRDate = DateTime.Now, Owner = false, ProgramSet = (byte)this.ProgramSet, LogID = (int)response.ErrorID, Message = response.Data });
			}

			return retitem;
		}

		private ConverterDB.Model.ItemBase FindItemKey(ItemKeyConfig itemconfig, string value)
		{
			//ConverterDB.Model.ItemBase retItem = LocalDb.FirstOrDefault(itemconfig.DBSetType, item => string.Compare(item.Name, value) == 0);
			ConverterDB.Model.ItemBase retItem = FindItemKey(itemconfig.DBSetType, item => string.Compare(item.Name, value, true) == 0);
			return retItem == null ? Commons.ObjectUtils.InitObject(itemconfig.DBSetType) as ConverterDB.Model.ItemBase : retItem;
		}

		protected bool UpdateConvertinfo( ConvertInfo convertinfo)
		{
			try
			{
				LocalDb.Update<ConvertInfo>(convertinfo);
				LocalDb.Save();
				return true;
			}
			catch(Exception)
			{
				return false;
			}
		}

		protected ConvertMessage.MessageResult ConvertTransactData<TAccess,TSql>( TAccess access, ItemObject config) where TAccess : class where TSql : class
		{
			TSql sql;
			ERROR_CODE ret_code = ConvertObject<TSql>(out sql, access, config);
			if( ret_code != ERROR_CODE.OK)
				return new MessageResult{ ErrorID = ret_code};
			return TransferTrans<TSql>(sql);
		}
		protected ConvertInfo GetConvertInfo( string tableName)
		{
			return LocalDb.ConvertInfo.FirstOrDefault(item => item.Programset == (byte)this.ProgramSet && item.Enable == true && string.Compare(item.TableName, tableName, true) == 0);
		}
		 
		protected void ValidateLastKey(ConvertInfo covnertinfo, DataFileInfo Fileinfo)
		{

			if (Fileinfo.DVRDate > covnertinfo.DvrDate.Date)
			{
				covnertinfo.DvrDate = Fileinfo.DVRDate.Date;
				covnertinfo.LastKey = "0";
				UpdateConvertinfo(covnertinfo);
			}
		}
		protected ItemBase GetItemKey(ItemKeyConfig iconfig, string value)
		{
			if( iconfig == null)
				return null;

			ItemBase item = FindItemKey(iconfig, value);
			if( item != null && item.ID > 0)
				return item;
			item = AddItemKey(iconfig, value );

			return  item;
		}
		
		private void LoadItemKeyMap(string resxPath, Programset pset)
		{
			string full_resName = string.Format("{0}.{1}", resxPath, Resource_FileName);
			Stream res_stream = this.GetType().Assembly.GetManifestResourceStream(full_resName);

			XmlDocument doc = new XmlDocument();
			doc.Load(res_stream);
			XmlNode Root = doc.DocumentElement;
			XmlNode PNode = XMLUtils.SelectNode(Root, string.Format( Commons.ConstEnums.XPATH_CHILD_NODE, STR_ItemKey) );
			LoadItemKeyConfig( PNode);
			XmlNodeList objects = Root.SelectNodes(string.Format( Commons.ConstEnums.XPATH_CHILD_NODE, STR_Object) );
			foreach (XmlNode obj in objects)
			{
				ItemObject itemobj = LoadObjectMap( obj );
				ObjectMapping.Add( itemobj.Table, itemobj);

			}
			res_stream.Close();
		}
		
		private ItemObject LoadObjectMap(XmlNode objNode)
		{
			ItemObject itemobject = new ItemObject();
			XmlNodeList items = objNode.SelectNodes(string.Format(Commons.ConstEnums.XPATH_CHILD_NODE, STR_Item));
			string src = string.Empty;
			string des = string.Empty;
			string convertbase = string.Empty;
			string table = string.Empty;
			table = Commons.XMLUtils.XMLAttributeValue(objNode, STR_Table);
			src = Commons.XMLUtils.XMLAttributeValue(objNode, STR_src);
			des = Commons.XMLUtils.XMLAttributeValue(objNode, STR_des);
			itemobject.Src = src;
			itemobject.Des = des;
			itemobject.Table = table;
			foreach( XmlNode item in items)
			{
				src =  Commons.XMLUtils.XMLAttributeValue(item, STR_src);
				des = Commons.XMLUtils.XMLAttributeValue(item, STR_des);
				convertbase = Commons.XMLUtils.XMLAttributeValue(item, STR_ConvertBase);
				itemobject.ItemConfigs.Add(
				new ItemConfig{ Src = src, Des = des, ConvertBaseClass = convertbase, ConvertBaseType = Type.GetType( string.Format("{0}.{1}", STR_Convert_Item_key_Namespace, convertbase) ) }
				);
			}
			XmlNodeList objects = objNode.SelectNodes(string.Format(Commons.ConstEnums.XPATH_CHILD_NODE, STR_Object));
			foreach (XmlNode obj in objects)
			{
				ItemObject child = LoadObjectMap(obj);
				itemobject.ChildObjects.Add(child);
			}
			
			return itemobject;
		}
		
		private void LoadItemKeyConfig( XmlNode  itemkey)
		{
			string col_name = string.Empty;
			string SqlTable = string.Empty;
			string dbsettype = string.Empty;
			string convert_base = string.Empty;
			Assembly ConvertDBModel_assemply = typeof(ConvertDB).Assembly;
			string str_namespace = typeof(ConverterDB.Model.ItemBase).Namespace;
			string str_convertbase_ns = typeof(ConvertItemKeys.ConvertItemPOSPayment).Namespace;
			foreach (XmlNode node in itemkey.ChildNodes)
			{
				if (node.NodeType != XmlNodeType.Element)
					continue;
				col_name = XMLUtils.XMLAttributeValue(node, STR_Column);
				SqlTable = XMLUtils.XMLAttributeValue(node, STR_SqlTable);
				dbsettype = XMLUtils.XMLAttributeValue(node, STR_ContextType);
				convert_base =  XMLUtils.XMLAttributeValue(node, STR_ConvertBase);
				ItemKeyMapping.Add(
										col_name,
										new ItemKeyConfig
										{
											ColumnName = col_name,
											DBSetTypeString = dbsettype,
											SQLTable = SqlTable,
											DBSetType = ConvertDBModel_assemply.GetType(string.Format("{0}.{1}", str_namespace, dbsettype)),
											ConvertBaseClass = convert_base,
											ConvertBaseType = Type.GetType(string.Format("{0}.{1}", str_convertbase_ns, convert_base))
										}
				);
			}
		}

		protected List<T> SelectListDatabySQLComand<T>(string sqlcommand, string tableName, out ERROR_CODE ret_code)
		{
			DataTable tblret = SelectDatabySQLComand(sqlcommand);
			ret_code = ERROR_CODE.OK;
			if( tblret == null)
			{
				ret_code = ERROR_CODE.DB_QUERY_EXCEPTION;
				return null;
			}
			if( tblret != null)
				tblret.TableName = tableName;
			return Commons.ObjectUtils.TabletoList<T>( tblret);
		}
		protected T SelectDatabySQLComand<T>(string sqlcommand, string tableName, out ERROR_CODE ret_code, int objectindex = 0) where T : class
		{
			DataTable tblret = SelectDatabySQLComand( sqlcommand);
			ret_code = ERROR_CODE.OK;
			if( tblret == null)
			{
				ret_code = ERROR_CODE.DB_QUERY_EXCEPTION;
				return null;
			}

			if(tblret.Rows.Count <= objectindex)
			{
				ret_code = ERROR_CODE.DB_QUERY_NODATA;
				return default(T);
			}
			tblret.TableName = tableName;
			return  Commons.ObjectUtils.RowToObject<T>( tblret.Rows[objectindex]);
		}

		protected Commons.ERROR_CODE IsValidtable(DataFileInfo fileinfo, string tableName)
		{
			if( string.IsNullOrEmpty(tableName))
				return ERROR_CODE.DB_INVALID_TABLE;
			
			RawDB.DBFile = fileinfo.Fileinfo.FullName;
			
			DataTable tblNamse = RawDB.DBSchema();
			if( tblNamse == null)
				return ERROR_CODE.DB_QUERY_EXCEPTION;
				 
			DataRow[] SensorRows = tblNamse.Select( string.Format("{0} = '{1}'", MSAccess.STR_TABLE_NAME, tableName) );
			return SensorRows.FirstOrDefault() != null?  ERROR_CODE.OK : ERROR_CODE.DB_INVALID_TABLE;
		}
		protected DataTable SelectDatabySQLComand(DataFileInfo fileinfo, string sqlcommand)
		{
			return SelectDatabySQLComand(sqlcommand);
		}
		/// <summary>
		/// Select Data by sql command. Return null if any exception happen.
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="sqlcommand"></param>
		/// <returns></returns>
		protected DataTable SelectDatabySQLComand(string sqlcommand)
		{
			return RawDB.SelectDatabySQLComand(sqlcommand);
		}

		private IEnumerable<DataFileInfo>  GetDataFileList(DirectoryInfo dirInfo, DateTime BeginDate)
		{
			FileInfo[]DataFiles = Utils.GetFileInfo(dirInfo, STR_DBFILE_EXTENSION, SearchOption.AllDirectories);
			IEnumerable<FileInfo> Files = DataFiles.Where( item => DateTime.Compare( Utils.DataFile2Date(item.Name), BeginDate.Date) >= 0 );
			IEnumerable<DataFileInfo> dtainfos = Files.Select(file => new DataFileInfo { Fileinfo = file, FileName = file.Name, DVRDate = Utils.DataFile2Date(file.Name) });
			return dtainfos;
		}
		
		private DateTime MinConvertDate()
		{
			 IEnumerable<ConvertInfo> iConverts = LocalDb.ConvertInfo.Where( item => item.Programset == (byte)this.ProgramSet && item.Enable == true);
			return iConverts.FirstOrDefault() == null? DateTime.MaxValue : iConverts.Min( item => item.DvrDate);
		}
		
		protected List<DataFileInfo> GetDataFileList( string datadir)
		{
			DateTime BeginDate = MinConvertDate();
			if( BeginDate == DateTime.MaxValue)
				return new List<DataFileInfo>();

			List<DataFileInfo> ret = new List<DataFileInfo>();
			DirectoryInfo[] dinfos = Utils.DirInfo(datadir,"????");
			IEnumerable<DirectoryInfo> dir_years = dinfos.Where(item => Commons.Utils.ValidationString(Consts.RX_YEAR, item.Name) == true && Convert.ToInt16(item.Name) >= BeginDate.Year);
			dir_years.ToList().ForEach( item => ret.AddRange(GetDataFileList(item, BeginDate) )	);

			ret.Sort(
						delegate( DataFileInfo d1, DataFileInfo d2)
						{
							return DateTime.Compare( d1.DVRDate, d2.DVRDate);
						}
			);

			return ret;
		}
		
		protected bool ASignSingleValue( object src, object des, Dictionary<string, ItemKeyConfig> itemKeys, ItemConfig itemconfig)
		{
			 KeyValuePair<string, ItemKeyConfig> itemkey = itemKeys.FirstOrDefault( key=> string.Compare(key.Key, itemconfig.Src, true) == 0);
			 
			 if( string.IsNullOrEmpty(itemkey.Key))
			 {
				 return Commons.ObjectUtils.SetPropertyValue(des, itemconfig.Des, Commons.ObjectUtils.GetPropertyValue(src, itemconfig.Src));
			 }
			 ItemBase itembase = null;
			 ItemKeyConfig iconfig = itemkey.Value;
			 if( iconfig.ConvertBaseType == null)
			 {
				 string new_value = Commons.ObjectUtils.GetValueInObject<string>(src, itemkey.Value.ColumnName);
				if(string.IsNullOrEmpty(new_value))
					return true; 
				itembase = GetItemKey(iconfig, new_value);
				if( itembase.ID == 0)
					return false;

				return Commons.ObjectUtils.SetPropertyValue(des, itemconfig.Des, itembase.ID);
			 }
			else
			{
				ConvertItemBase convertItem = Commons.ObjectUtils.InitObject(iconfig.ConvertBaseType, new object[] { iconfig, LocalDb}) as ConvertItemBase;
				convertItem.OnAddItemKey += convertItem_OnAddItemKey;
				convertItem.OnRequestItemKey += convertItem_OnRequestItemKey;
				//convertItem = new ConvertItemBase(iconfig, RequestItemkey);
				//itembase = AddItemKey(iconfig, convertItem, src); //convertItem.GetItemKey( Trans, LocalDb.Query(iconfig.DBSetType));
				//if( itembase == null)//current value is empty. ignored
				//	return true;
				//if (itembase.ID == 0)
				//	return false;
				return convertItem.UpdateItemKey(src, des) == ERROR_CODE.OK;
				//return true;
			 }
		}

		void convertItem_OnRequestItemKey(ref MessageItemKey msgItem, string sqltable)
		{
			msgItem = RequestItemkey(msgItem, sqltable);
		}

		void convertItem_OnAddItemKey(ItemBase newitem)
		{
			SaveItemKey(newitem);
		}

		protected ERROR_CODE ConvertObject<T>(out T des, object Trans, ItemObject objConfig)
		{
			des = (T)Commons.ObjectUtils.InitObject( typeof(T));
			ERROR_CODE ret = ERROR_CODE.OK;
			foreach (ItemConfig item in objConfig.ItemConfigs)
			{
				if( CancellingToken.IsCancellationRequested)
					break;

				if (!ASignSingleValue(Trans, des, this.ItemKeyMapping, item) )
					return ERROR_CODE.DB_CONVERT_DATA_FAILED;
				
			}
			return ret;
		}

	}

	internal class DataFileInfo
	{
		public DateTime DVRDate{ get ;set;}
		public string FileName{ get; set;}
		public FileInfo Fileinfo { get; set;}
	}

	internal class ItemConfig
	{
		public string Src { get; set;}
		public string Des { get; set;}
		public string ConvertBaseClass{ get; set;}
		public Type ConvertBaseType{ get; set;}
	}
	
	internal class ItemObject
	{
		public string Src { get; set; }
		public string Des { get; set; }
		public string Table{ get; set;}
		public List<ItemConfig> ItemConfigs{ get; set;}
		public List<ItemObject> ChildObjects{ get; set;}
		public ItemObject()
		{
			ItemConfigs = new List<ItemConfig>();
			ChildObjects = new List<ItemObject>();
		}

	}
	
	internal class ItemKeyConfig
	{
		public string ColumnName{ get;set;}
		public string SQLTable{ get;set;}
		public string DBSetTypeString { get; set; }
		public string ExternalMethod{ get;set;}
		public Type DBSetType{ get; set;}
		public string ConvertBaseClass { get; set; }
		public Type ConvertBaseType { get; set; }
	}
}

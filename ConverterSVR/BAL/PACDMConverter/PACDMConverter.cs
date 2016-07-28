using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Commons;
using ConverterSVR.BAL;
using ConvertMessage;
using PACDMModel;
using SVRDatabase;

namespace ConverterSVR.BAL.PACDMConverter
{
	internal class PACDMConverter: ConverterBase
	{
		public const string STR_Transact = "Transact";
		public const string STR_Sensor = "Sensor";
		private class ItemKeyConfig
		{
			public string MappingName { get; set;}
			public string EntityName { get; set;}
			public Type MessageType { get; set;}
			public Type EntityType { get; set;}
			public Dictionary<string , string> PropertyMap{ get; set;}
		}
		
		private class ConvertMapping : Commons.SingletonListBase<ConvertMapping, ItemKeyConfig>
		{
			const string Model_NameSpace = "PACDMModel.Model";
			const string Resource_FileName = "ItemConfigs.xml";
			const string str_key = "key";
			const string str_value = "value";
			const string str_msgtype = "msgtype";
			const string str_entity = "entity";
			const string str_src = "src";
			const string str_des = "des";

			//readonly List<ItemKeyConfig> ItemkeyMaps = new List<ItemKeyConfig>();

			//private static readonly Lazy<ConvertMapping> Lazy = new Lazy<ConvertMapping>(() => new ConvertMapping());
			//public static ConvertMapping Instance { get { return Lazy.Value; } }
			
			private ConvertMapping()
			{
					LoadMappingFile(Resource_FileName);
			}

			private void LoadMappingFile( string rexName)
			{
				Assembly ass = typeof(PACDMConverter).Assembly;
				Assembly ass_PACModel = typeof(PACDMModel.PACDMDB).Assembly;
				Assembly ass_ConvertMessage = typeof(ConvertMessage.MessageItemKey).Assembly;

				string full_resName = string.Format("{0}.{1}", typeof(PACDMConverter).Namespace, rexName);
				Stream res_stream = ass.GetManifestResourceStream(full_resName);
				XmlDocument doc = new XmlDocument();
				doc.Load(res_stream);
				XmlNode root = doc.DocumentElement;
				ItemKeyConfig itemconfig = null;
				string str_key, str_entity, str_msgtype, str_type;
				foreach (XmlNode node in root.ChildNodes)
				{
					if (node.NodeType != XmlNodeType.Element)
						continue;
					str_key = XMLUtils.XMLAttributeValue(node, ConvertMapping.str_key);
					str_entity = XMLUtils.XMLAttributeValue(node, ConvertMapping.str_entity);
					str_msgtype = XMLUtils.XMLAttributeValue(node, ConvertMapping.str_msgtype);
					str_type = string.Format("{0}.{1}", Model_NameSpace, str_entity);
					itemconfig = new ItemKeyConfig();
					itemconfig.EntityName = str_entity;
					itemconfig.EntityType = ass_PACModel.GetType( str_type);
					itemconfig.MappingName = str_key;
					itemconfig.MessageType = ass_ConvertMessage.GetType(str_msgtype);
					itemconfig.PropertyMap = PropertiesMap(node);
					//ItemkeyMaps.Add(itemconfig);
					base.Items.Add(itemconfig);
					
				}
				res_stream.Close();

			}
			private Dictionary<string, string>PropertiesMap( XmlNode itemNode)
			{
				Dictionary<string, string> mapping = new Dictionary<string,string>();
				if( itemNode == null || itemNode.ChildNodes == null || itemNode.ChildNodes.Count == 0)
					return mapping;
				foreach( XmlNode Vnode in itemNode.ChildNodes)
					mapping.Add(XMLUtils.XMLAttributeValue(Vnode, ConvertMapping.str_src), XMLUtils.XMLAttributeValue(Vnode, ConvertMapping.str_des));
				return mapping;
			}
			public ItemKeyConfig GetMessageConfig (string key)
			{
				//return ItemkeyMaps.FirstOrDefault( item => string.Compare( item.MappingName, key, true) == 0);
				return Items.FirstOrDefault(item => string.Compare(item.MappingName, key, true) == 0);
			}

		}

		public PACDMConverter(MessageData msgbody, MessageDVRInfo DVRInfo) : base(msgbody, DVRInfo)
		{

		}

		public override ERROR_CODE ValidateMessage()
		{
			return base.ValidateMessage();
		}

		public override async Task<MessageResult> ConvertMessage(PACDMDB PACModel, SVRManager LogModel, MediaTypeFormatter formatter)
		{
				TaskCompletionSource<MessageResult> complete_result = new TaskCompletionSource<MessageResult>();
				MessageResult ret = null; //base.ConvertMessage(PACModel,LogModel, formatter);
				string mapping =  _message.Mapping;
				ItemKeyConfig itemkeyconfig = ConvertMapping.Instance.GetMessageConfig(mapping);
				if( itemkeyconfig != null)
				{
					ret = GetItemKey( _message.Data, formatter, itemkeyconfig, PACModel);
					complete_result.SetResult(ret);
					return await complete_result.Task;
				}
				PACDMConvertBase PACConverter = null;
				switch(_message.Programset)
				{
					case Programset.ATM:
						PACConverter = new ATM.ATMConverter(PACModel, LogModel, _message, DVRInfo, formatter);
						break;
					case Programset.CA:
						PACConverter = new CA.CAConverter(PACModel, LogModel, _message, DVRInfo, formatter);
						break;
					case Programset.IOPC:
						PACConverter = new IOPC.IOPCConverter(PACModel, LogModel, _message, DVRInfo, formatter);
						break;
					case Programset.LPR:
                        PACConverter = new LPR.LPRConverter(PACModel, LogModel, _message, DVRInfo, formatter);
						break;
					case Programset.POS:
						PACConverter = new POS.POSConverter(PACModel, LogModel, _message, DVRInfo, formatter);
						break;
                    case Programset.LABOR:
                        PACConverter = new LABOR.LABORConverter(PACModel, LogModel, _message, DVRInfo, formatter);
						break;
                    case Programset.POS3RD:
                        PACConverter = new POS3RD.POS3rd(PACModel, LogModel, _message, DVRInfo, formatter);
                        break;
				}

				if( PACConverter != null)
					complete_result.SetResult( PACConverter.ConvertData());
				else
					complete_result.SetResult( new MessageResult{ ErrorID = ERROR_CODE.INVALID_MAPPING});
				return await complete_result.Task;
	
		}
		public override void Dispose()
		{
			base.Dispose();
		}

		#region item process
		private MessageResult GetItemKey(string MessageData, MediaTypeFormatter formatter, ItemKeyConfig itemconfig, PACDMDB pacmodel)
		{
			MessageItemKey itemkey = RawdataToMessageItemKey( formatter, MessageData, itemconfig.MessageType);
			object entitykey = FindEntitybyMessageitemKey(itemkey, itemconfig, pacmodel);
			if( entitykey == null)
			{
				try
				{
					entitykey = MessageItemKeyToEntityKey(itemkey, itemconfig);
					pacmodel.Insert(itemconfig.EntityType, entitykey);
					//Nghi change to update data to WareHouse July 09 2015 begin
					int saveed = pacmodel.Save();
					//update dim data to warehouse
					if( saveed > 0)
						CMSWebApi.Wrappers.Wrapper.Instance.DBWareHouse.UpdateDim(pacmodel, entitykey);
						//CMSWebApi.Cache.DBWarehouse.WarehouseManager.Instance.UpdateDim(pacmodel, entitykey);
					//Nghi change to update data to WareHouse July 09 2015 end
					itemkey =  EntityKeyToMessageItemKey( entitykey, itemconfig);
				}
				catch(Exception ex)
				{
					return new MessageResult{ ErrorID = ERROR_CODE.DB_UPDATE_DATA_FAILED, Data = ex.Message};
				}
			}

			return MessageItemKeyToResult(EntityKeyToMessageItemKey(entitykey, itemconfig) , formatter);
		}

		private object FindEntitybyMessageitemKey(MessageItemKey itemkey, ItemKeyConfig itemconfig, PACDMDB pacmodel)
		{
			 var pmap = itemconfig.PropertyMap.Where(map => string.Compare(map.Key, Consts.STR_ID, true) != 0);
			return pacmodel.FirstOrDefault(itemconfig.EntityType,
			delegate(dynamic item)
			{
				object src, des;
				bool ret = true;
				 
				foreach (KeyValuePair<string, string> map in pmap)
				{
					if (!ret)
						break;

					src = Commons.ObjectUtils.GetPropertyValue(itemkey, map.Key);
					des = Commons.ObjectUtils.GetPropertyValue(item, map.Value);
					if (src == null && des == null)
					{
						ret &= true;
						continue;
					}
						if ((src == null && des != null) || (src != null && des == null))
						{
							ret &= false;
							continue;
						}
						ret &= string.Compare(des.ToString(), src.ToString(), true) == 0;
					}
					return ret;
				}
				);
		}
		
		private MessageItemKey RawdataToMessageItemKey( MediaTypeFormatter formatter, string data, Type msgType)
		{
			return Commons.ObjectUtils.DeSerialize( formatter, msgType, data) as MessageItemKey;
		}

		private object MessageItemKeyToEntityKey( MessageItemKey itemkey, ItemKeyConfig itemconfig)
		{
			object ret = Commons.ObjectUtils.InitObject( itemconfig.EntityType);
			foreach (KeyValuePair<string, string> mapping in itemconfig.PropertyMap)
			{
				Commons.ObjectUtils.SetPropertyValue(ret, mapping.Value, Commons.ObjectUtils.GetPropertyValue( itemkey, mapping.Key));
			}
			return ret;
		}

		private MessageItemKey EntityKeyToMessageItemKey(object entityvalue, ItemKeyConfig itemconfig)
		{
			MessageItemKey ret = Commons.ObjectUtils.InitObject( itemconfig.MessageType) as MessageItemKey;
			foreach(KeyValuePair<string,string> mapping in itemconfig.PropertyMap)
			{
				Commons.ObjectUtils.SetPropertyValue( ret, mapping.Key, Commons.ObjectUtils.GetPropertyValue( entityvalue, mapping.Value));
			}
			return ret;
		}
		
		private MessageData MessageItemKeyToMessageData( MessageItemKey msgItem, MediaTypeFormatter formatter)
		{
			return new MessageData{ Programset = _message.Programset, Mapping = _message.Mapping, Data = Commons.ObjectUtils.Serialize(formatter, msgItem.GetType(), msgItem) };
		}

		private MessageResult MessageItemKeyToResult( MessageItemKey msgItem, MediaTypeFormatter formatter)
		{
			//MessageData data = MessageItemKeyToMessageData(msgItem, formatter);

			return new MessageResult { ErrorID = ERROR_CODE.OK, Data = Commons.ObjectUtils.Serialize(formatter, msgItem.GetType(), msgItem) };
		}
		#endregion

	}

}

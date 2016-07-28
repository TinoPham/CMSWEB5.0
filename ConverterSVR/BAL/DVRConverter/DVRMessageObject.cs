using Commons;
using ConvertMessage;
using PACDMModel.Model;
using SVRDatabase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ConverterSVR.BAL.DVRConverter
{
	public class DVRMessageObject :IDisposable
	{
		public ERROR_CODE MsgLogCode{ get;set;}
		public bool isCreated{ get;set;}
		IDVRMsg _rawDvrCfg;
		public DVRMessageObject(MessageData _message)
		{
			string msgID = _message.Mapping;
			Int32 configID;
			string cfgKey = "";
			int messageID;
			XmlDocument xmlDoc = new XmlDocument();
			try
			{
				messageID = Convert.ToInt32(msgID);
				switch (messageID)
				{
					case (int)CMSMsg.MSG_DVR_GET_CONFIG_RESPONSE:
					case (int)CMSMsg.MSG_DVR_SET_CONFIG_RESPONSE:
					case (int)CMSMsg.MSG_DVR_CONFIG_CHANGED:
						{
							if (!String.IsNullOrEmpty(_message.Data) && IsConfigMsg(Convert.ToInt32(msgID)))
							{
								if (_message.Data.IndexOf(configuration_id) > 0)
								{
									_message.Data = RawRecordScheduleConfig.ProcessScheduleData(_message.Data);
								}
							}

							MsgLogCode = ERROR_CODE.DVR_ERR_XML_LOAD_ERR;
							xmlDoc.LoadXml(_message.Data);

							MsgLogCode = ERROR_CODE.DVR_ERR_XML_DESERIALIZE;
							XmlNode xNodeConfig = xmlDoc.SelectSingleNode(xPath_configuration_id);
							configID = xNodeConfig == null ? 0 : Convert.ToInt32(xNodeConfig.InnerText);
							cfgKey = ConvertMapping.GenerateKey(msgID, configID);
							KeyValuePair<string, Type> MappingClass = ConvertMapping.Instance.GetType(cfgKey);

							_rawDvrCfg = Commons.ObjectUtils.DeSerialize(MappingClass.Value, _message.Data) as IDVRMsg;
							isCreated = true;
							return;
						}

					default:
						{
							MsgLogCode = ERROR_CODE.DVR_ERR_OBJ_DESERIALIZE;
							configID = 0;
							cfgKey = ConvertMapping.GenerateKey(msgID, configID);
							KeyValuePair<string, Type> MappingClass = ConvertMapping.Instance.GetType(cfgKey);

							_rawDvrCfg = Commons.ObjectUtils.InitObject(MappingClass.Value, new object[] { _message.Data }) as IDVRMsg;
							isCreated = true;
							return;
						}
				}
			}
			catch
			{
				isCreated = false;
			}
		}

		public void Dispose()
		{
			if( (_rawDvrCfg as IDisposable) != null)
			(_rawDvrCfg as IDisposable).Dispose();
		}

		public async Task<Commons.ERROR_CODE> UpdateToDB()
		{
			return await _rawDvrCfg.UpdateToDB();
		}

		public void SetEvnVars(PACDMModel.PACDMDB pacDB, SVRManager Logdb, MessageDVRInfo dvrinfo)
		{
			_rawDvrCfg.SetEvnVars(pacDB, Logdb, dvrinfo);
		}

		public async Task<string> GetResponseMsg()
		{
			return await _rawDvrCfg.GetResponseMsg();
		}

		private bool IsConfigMsg(Int32 msgID)
		{
			switch (msgID)
			{
				case (int)CMSMsg.MSG_DVR_GET_CONFIG_RESPONSE:
				case (int)CMSMsg.MSG_DVR_CONFIG_CHANGED:
					return true;

				default:
					break;
			}
			return false;
		}

		const string configuration_id = "<configuration_id>6</configuration_id>";
		const string xPath_configuration_id = "/message/body/configuration_id";
		private class ConvertMapping
		{

			const string Model_NameSpace = "ConverterSVR.BAL.DVRConverter";
			const string Resource_FileName = "DVRMsgConfig.xml";
			const string str_msg = "id";
			const string str_cfg = "config";
			const string str_class = "class";
			readonly Dictionary<string, Type> _ServiceMapping = null;

			private static readonly Lazy<ConvertMapping> Lazy = new Lazy<ConvertMapping>(() => new ConvertMapping());
			public static ConvertMapping Instance { get { return Lazy.Value; } }
			private ConvertMapping()
			{
				if (_ServiceMapping == null)
				{
					_ServiceMapping = new Dictionary<string, Type>();
					LoadMappingFile(Resource_FileName);
				}
			}

			private void LoadMappingFile(string rexName)
			{
				string NameSpcace = typeof(DVRConverter).Namespace;
				Assembly ass = typeof(DVRConverter).Assembly;
				string full_resName = string.Format("{0}.{1}", NameSpcace, rexName);
				Stream res_stream = ass.GetManifestResourceStream(full_resName);
				if (res_stream == null)
					return;

				XmlDocument doc = new XmlDocument();
				doc.Load(res_stream);

				string msgIDs = string.Empty;
				string cfgKey = string.Empty;
				string cfgID = string.Empty;
				string className = string.Empty;

				XmlNode root = doc.DocumentElement;
				foreach (XmlNode node in root.ChildNodes)
				{
					if (node.NodeType != XmlNodeType.Element)
						continue;
					className = XMLUtils.XMLAttributeValue(node, str_class);
					if (String.IsNullOrEmpty(className))
						continue;
					msgIDs = XMLUtils.XMLAttributeValue(node, str_msg);
					cfgID = XMLUtils.XMLAttributeValue(node, str_cfg);
					string[] arrMsgs = msgIDs.Split(',');
					foreach (string msg in arrMsgs)
					{
						cfgKey = GenerateKey(msg, cfgID);
						string fullname = string.Format("{0}.{1}", NameSpcace, className);
						_ServiceMapping.Add(cfgKey, Type.GetType(fullname));
					} //foreach
				} //foreach
				//res_stream.Close();

			}

			public KeyValuePair<string, Type> GetType(string key)
			{
				return _ServiceMapping.FirstOrDefault(item => string.Compare(item.Key, key, true) == 0);
			}

			public KeyValuePair<string, Type> GetType(Int32 msgID, Int32 cfgID)
			{
				string key = GenerateKey(msgID, cfgID);//string.Format("{0}:{1}", msgID, cfgID);
				return _ServiceMapping.FirstOrDefault(item => string.Compare(item.Key, key, true) == 0);
			}

			public static string GenerateKey(object msgID, object cfgID)
			{
				return string.Format("{0}:{1}", msgID, cfgID);
			}
		}
	}
}
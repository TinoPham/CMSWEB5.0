using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commons;
using ConvertMessage;
using System.Xml;
using System.Reflection;
using System.IO;
using System.Net.Http.Formatting;
using System.Data.Entity;
using PACDMModel.Model;
using SVRDatabase;
using Commons.Resources;

namespace ConverterSVR.BAL.DVRConverter
{
	internal class DVRConverter : ConverterBase
	{

		private class ConvertMapping :Commons.SingletonStringTypeMappingBase<ConvertMapping>
		{

			const string Model_NameSpace = "ConverterSVR.BAL.DVRConverter";
			const string Resource_FileName = "DVRMsgConfig.xml";
			const string str_msg = "id";
			const string str_cfg = "config";
			const string str_class = "class";
			
			private ConvertMapping()
			{
				LoadMappingFile(Resource_FileName);
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
						//_ServiceMapping.Add(cfgKey, Type.GetType(fullname));
						base.AddMapping(cfgKey, Type.GetType(fullname));
					} //foreach
				} //foreach
				//res_stream.Close();

			}

			public KeyValuePair<string, Type> GetMapping(Int32 msgID, Int32 cfgID)
			{
				string key = GenerateKey(msgID, cfgID);//string.Format("{0}:{1}", msgID, cfgID);
				return base.GetMapping( key);
			}

			string GenerateKey(object msgID, object cfgID)
			{
				return string.Format("{0}:{1}", msgID, cfgID);
			}
		}

		const string configuration_id = "<configuration_id>6</configuration_id>";
		const string xPath_configuration_id_old = "/message/body/configuration_id";
		const string xPath_configuration_id = "/message/body/common/configuration_id";

		IDVRMsg IDVRMsg;
		 
		public DVRConverter(MessageData msgbody, MessageDVRInfo dvrinfo)
			: base(msgbody, dvrinfo)
		{ }

		public override ERROR_CODE ValidateMessage()
		{
			ERROR_CODE ret = base.ValidateMessage();
			int msgID = 0;
			if( !Int32.TryParse(base._message.Mapping, out msgID) || msgID == 0)
				return ERROR_CODE.INVALID_MAPPING;
			XmlDocument xmlDoc = new XmlDocument();
			KeyValuePair<string, Type> MappingClass;
			switch (msgID)
			{
				case (int)CMSMsg.MSG_DVR_GET_CONFIG_RESPONSE:
				case (int)CMSMsg.MSG_DVR_SET_CONFIG_RESPONSE:
				case (int)CMSMsg.MSG_DVR_CONFIG_CHANGED:
					{
						//if (IsConfigMsg(msgID) && _message.Data.IndexOf(configuration_id, StringComparison.InvariantCultureIgnoreCase) > 0)
						//	_message.Data = RawRecordScheduleConfig.ProcessScheduleData(_message.Data);
						
						//xmlDoc.LoadXml(_message.Data);
						xmlDoc = Commons.XMLUtils.XMLDocument( _message.Data);

						XmlNode xNodeConfig = xmlDoc.SelectSingleNode(xPath_configuration_id);
						if( xNodeConfig == null)
						{
							xNodeConfig = xmlDoc.SelectSingleNode(xPath_configuration_id_old);
							if (xNodeConfig == null)
							{
								ret = ERROR_CODE.DVR_ERR_XML_LOAD_ERR;
								break;
							}
						}
						int configID = 0;
						int.TryParse(xNodeConfig.InnerText, out configID);

						MappingClass = ConvertMapping.Instance.GetMapping(msgID, configID);
						if(string.IsNullOrEmpty( MappingClass.Key)|| MappingClass.Value == null)
						{
							ret = ERROR_CODE.INVALID_MAPPING;
							break;
						}

						IDVRMsg = Commons.ObjectUtils.DeSerialize(MappingClass.Value, _message.Data) as IDVRMsg;
						ret = IDVRMsg == null ? Commons.ERROR_CODE.DVR_ERR_XML_DESERIALIZE : ERROR_CODE.OK;
						break;
					}

				default:
					{
						MappingClass = ConvertMapping.Instance.GetMapping(msgID, 0);//always 0 for configID
						if (string.IsNullOrEmpty(MappingClass.Key) || MappingClass.Value == null)
						{
							ret = ERROR_CODE.INVALID_MAPPING;
							break;
						}

						IDVRMsg = Commons.ObjectUtils.InitObject(MappingClass.Value, new object[] { _message.Data }) as IDVRMsg;
						ret = IDVRMsg == null ? Commons.ERROR_CODE.DVR_ERR_XML_DESERIALIZE : ERROR_CODE.OK;
						break;
					}
			}
				
			return ret;
		}

		public override async Task<MessageResult> ConvertMessage(PACDMModel.PACDMDB PACModel, SVRManager LogModel, MediaTypeFormatter formatter)
		{
			TaskCompletionSource<MessageResult> complete_result = new TaskCompletionSource<MessageResult>();
			IDVRMsg.SetEvnVars(PACModel, LogModel, base.DVRInfo);
			ERROR_CODE update_result = await IDVRMsg.UpdateToDB();
			string 	responseMsg = string.Empty;
			if( update_result == ERROR_CODE.OK)
				responseMsg = await IDVRMsg.GetResponseMsg();

			complete_result.SetResult(new MessageResult { ErrorID = update_result, Data = responseMsg });
			
			return await complete_result.Task;
		}

		public override void Dispose()
		{
			base.Dispose();
			if( IDVRMsg != null)
			{
				(IDVRMsg as IDisposable).Dispose();
				IDVRMsg = null;
			}
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
	}
}

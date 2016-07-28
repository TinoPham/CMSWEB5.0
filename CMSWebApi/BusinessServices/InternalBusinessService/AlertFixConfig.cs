using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using CMSWebApi.DataModels;
using Commons;

namespace CMSWebApi.BusinessServices.InternalBusinessService
{
	internal class AlertFixConfigs : Commons.SingletonClassBase<AlertFixConfigs>
	{
		const string str_Alerts = "Alerts";
		const string str_Alert = "Alert";
		const string str_alt = "alt";
		const string str_altfix = "altfix";
		const string str_handler = "handler";
		const string str_message = "message";
		const string str_msghandler = "msghandler";
		const string Config_FileName = "AlertConfigs.xml";
		const string str_EmailAlerts = "EmailAlerts";
		const string str_Included = "included";

		public IEnumerable<byte> AlertType{ get ;private set;}
		public IEnumerable<string> AlertTypeString { get{ 
															if( AlertType == null)
																return Enumerable.Empty<string>() ;
															else 
																return AlertType.Select(it => it.ToString());
															}}
		public AlertFixConfigs()
		{
			AlertType = GetAlertConfig();
		}
		List<byte> GetAlertConfig()
		{
			List<byte> result = new List<byte>();
			string path = Path.Combine(AppSettings.AppSettings.Instance.AppData, Utils.Consts.Config_FileName);
			XmlDocument doc = new XmlDocument();
			doc.Load(path);
			XmlNode root = doc.DocumentElement;
			byte val = 0;
			XmlNodeList NodeAlerts = root.SelectNodes(str_Alert);
			foreach (XmlNode node in root.ChildNodes)
			{
				string att_val = XMLUtils.XMLAttributeValue(node, str_alt);
				val  = 0;
				byte.TryParse(att_val, out val);
				if( val == 0)
					continue;
				result.Add(val);
			}
			return result;
		}

		public List<EmailAlertConfig> GetAlertConfigEmail() 
		{
			List<EmailAlertConfig> result = new List<EmailAlertConfig>();
			string path = Path.Combine(AppSettings.AppSettings.Instance.AppData, Utils.Consts.Config_FileName);
			XmlDocument doc = new XmlDocument();
			doc.Load(path);
			XmlNode root = doc.DocumentElement;
			EmailAlertConfig val = null;
			XmlNode NodeEmailAlert = root.SelectSingleNode(str_EmailAlerts);
			foreach (XmlNode node in NodeEmailAlert.ChildNodes)
			{
				val = ParserNode(node);
				if( val == null)
					continue;
				result.Add(val);
			}
			return result;
		}
		private EmailAlertConfig ParserNode(XmlNode node)
		{
			if( node == null || string.Compare( node.Name, str_Alert, true) != 0)
				return null;

			try{
				string att_val = XMLUtils.XMLAttributeValue(node,str_alt);
				EmailAlertConfig cfg = new EmailAlertConfig();
				byte val = 0;
				byte.TryParse(att_val, out val);
				att_val = XMLUtils.XMLAttributeValue(node, str_alt);

				att_val = XMLUtils.XMLAttributeValue(node, str_Included);
				if( string.IsNullOrEmpty(att_val) || string.Compare(att_val,"false", true) == 0 || string.Compare(att_val,"0", true) == 0)
					cfg.Included = false;
				else
					cfg.Included = true;
				cfg.AlertType = val;
				return cfg.AlertType == 0? null : cfg;
			}
			catch(Exception){ return null;}
		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Commons;

namespace AppSettings
{
	public class AlertFixConfigs : Commons.SingletonClassBase<AlertFixConfigs>
	{
		const string str_Alerts = "Alerts";
		const string str_Alert = "Alert";
		const string str_alt = "alt";
		const string str_altfix = "altfix";
		const string str_handler = "handler";
		const string str_message = "message";
		const string str_msghandler = "msghandler";
		const string Config_FileName = "AlertConfigs.xml";

		private List<AlertFixConfig> configs;
		private List<byte> alertypes;

		private AlertFixConfigs()
		{
			string path = Path.Combine(AppSettings.Instance.AppData, Config_FileName);
			LoadConfig(path);
		}
		public IEnumerable<AlertFixConfig> Configs { get { return configs; } }

		public IEnumerable<byte> AlertTypes { get { return alertypes; } }

		public IEnumerable<AlertFixConfig> GetConfig(byte KalertType)
		{
			if (KalertType <= 0)
				return null;

			IEnumerable<AlertFixConfig> cfg = configs.Where(it => it.ALertFixType.HasValue && it.ALertFixType.Value == KalertType);
			if (cfg.Any())
				return cfg;
			cfg = configs.Where(it => it.ALertType.HasValue && it.ALertType.Value == KalertType);
			return cfg;
		}

		public AlertFixConfig GetConfig(string mesagename)
		{
			if (string.IsNullOrEmpty(mesagename))
				return null;
			return configs.FirstOrDefault(it => string.Compare(mesagename, it.Message, true) == 0);

		}

		private void LoadConfig(string path)
		{
			configs = new List<AlertFixConfig>();
			alertypes = new List<byte>();
			if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
				return;
			try
			{

				XmlDocument doc = new XmlDocument();
				doc.Load(path);
				XmlNode root = doc.DocumentElement;
				AlertFixConfig cfg = null;
				foreach (XmlNode node in root.ChildNodes)
				{
					cfg = Parser(node);
					if (cfg == null)
						continue;
					alertypes.Add(cfg.ALertType.Value);
					if (cfg.ALertFixType.HasValue)
						alertypes.Add(cfg.ALertFixType.Value);
					configs.Add(cfg);
				}
			}
			catch (Exception) { }
		}
		private AlertFixConfig Parser(XmlNode node)
		{
			try
			{
				AlertFixConfig ret = new AlertFixConfig();
				string att_val = XMLUtils.XMLAttributeValue(node, str_alt);
				ret.ALertType = Utils.ChangeSimpleType<byte>(att_val, null);
				att_val = XMLUtils.XMLAttributeValue(node, str_altfix);
				ret.ALertFixType = Utils.ChangeSimpleType<byte>(att_val, null);

				ret.Handler = XMLUtils.XMLAttributeValue(node, str_handler);
				ret.Message = XMLUtils.XMLAttributeValue(node, str_message);
				ret.MsgHandler = XMLUtils.XMLAttributeValue(node, str_msghandler);
				return ret;
			}
			catch (Exception) { return null; }
		}
	}
	public class AlertFixConfig
	{
		public byte? ALertType { get; set; }
		public byte? ALertFixType { get; set; }
		public string Handler { get; set; }
		public string Message { get; set; }
		public string MsgHandler { get; set; }
	}
}

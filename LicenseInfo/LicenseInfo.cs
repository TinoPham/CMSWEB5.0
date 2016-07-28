using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using LicenseInfo.Models;
using System.Globalization;
namespace LicenseInfo
{
	public class LicenseInfo
	{
		
		//<Licenses>
		//  <Modules>
		//	<Module>
		//		<Name></Name>
		//		<Enable></Enable>
		//		<From></From>
		//		<To></To>
		//	</Module>
		//  </Modules>
		//  <DVRNum></DVRNum>
		//</Licenses>

		internal const string License_Name= "Licenses";
		internal const string CMSWeb_Module_Name = "Module";
		internal const string CMSWeb_Modules_Name = "Modules";
		internal const string STR_Name = "Name";
		internal const string STR_Enable = "Enable";
		internal const string STR_From = "From";
		internal const string STR_To = "To";
		internal const string STR_DATETIME_FORMAT = "yyyyMMddHHmm";
		internal const string DVRNum_Name = "DVRNum";

		private static readonly Lazy<LicenseInfo> sInstance = new Lazy<LicenseInfo>(() => new LicenseInfo());

		public static LicenseInfo Instance { get { return sInstance.Value; } }

		private LicenseInfo(){}

		public LicenseModel ParserModel( string xml)
		{
			if (string.IsNullOrEmpty(xml))
				return new LicenseModel();
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xml);
			XmlNode root = doc.DocumentElement;
			if( root == null)
				return new LicenseModel();
			LicenseModel model = new LicenseModel();
			model.DVRNumber = String2Int( GetXMLText( GetXMLNode(root, DVRNum_Name)));
			model.CMSWebModules = new List<CMSWebModule>();
			XmlNode node_modules = GetXMLNode( root,CMSWeb_Modules_Name);
			CMSWebModule cmsmodule = null;
			foreach( XmlNode module in node_modules.ChildNodes)
			{
				cmsmodule = ParserCMSWebModule(module);
				if( module == null)
					continue;
				model.CMSWebModules.Add( cmsmodule);
			}
			return model;
		}

		public string ModelToXmlString( LicenseModel model)
		{
			if(model == null)
			return null;
			XmlDocument m_doc = new XmlDocument();
			XmlDeclaration dec = m_doc.CreateXmlDeclaration("1.0", null, null);
			m_doc.AppendChild(dec);
			XmlElement root = m_doc.CreateElement(License_Name);
			m_doc.AppendChild(root);
			AppendXMLNode( root,DVRNum_Name, model.DVRNumber);
			XmlNode modules = AppendXMLNode(root, CMSWeb_Modules_Name, null);
			foreach( CMSWebModule cmodel in model.CMSWebModules)
			{
					AddCMSWebModuleNode(modules, cmodel);
			}
			return root.OuterXml;
		}
		
		private void AddCMSWebModuleNode( XmlNode parent, CMSWebModule module)
		{
			XmlDocument doc = parent.OwnerDocument;
			XmlNode node = doc.CreateElement(CMSWeb_Module_Name);
			parent.AppendChild(node);
			AppendXMLNode(node,STR_Name, module.Name);
			AppendXMLNode(node, STR_Enable, module.Enable == true? "1" : "0" );
			AppendXMLNode(node, STR_From, module.From.ToString(STR_DATETIME_FORMAT));
			AppendXMLNode(node, STR_To, module.To.ToString(STR_DATETIME_FORMAT));
		}
	
		private XmlNode AppendXMLNode( XmlNode parent, string elementname, object value)
		{
			XmlNode child = parent.OwnerDocument.CreateElement(elementname);
			if( value != null)
				child.InnerText = value.ToString();

			parent.AppendChild(child);
			return child;
		}
		
		private CMSWebModule ParserCMSWebModule( XmlNode module)
		{
			if( module == null)
				return null;
			string name = GetXMLText(GetXMLNode(module,STR_Name));
			DateTime from = String2Date(GetXMLText(GetXMLNode(module,STR_From)));
			DateTime to = String2Date(GetXMLText(GetXMLNode(module, STR_To)));
			bool enable = String2Int(GetXMLText(GetXMLNode(module, STR_Enable))) == 1;
			return new CMSWebModule{ Enable = enable, From = from, To = to, Name = name};
		}
		
		private XmlNode GetXMLNode( XmlNode parent, string childname)
		{
			if( string.IsNullOrEmpty( childname) || parent == null)
				return null;

			return parent.SelectSingleNode( string.Format("./{0}", childname) );
		}

		private string GetXMLText( XmlNode node)
		{ 
			return node == null? null : node.InnerText;
			
		}
		
		private DateTime String2Date(string val)
		{
			if( string.IsNullOrEmpty(val))
				return DateTime.MinValue;
			try
			{
				return DateTime.ParseExact(val, STR_DATETIME_FORMAT, CultureInfo.InvariantCulture);
			}
			catch(Exception){ return DateTime.MinValue;}

		}

		private int String2Int( string val)
		{
			int ret = 0;
			Int32.TryParse( val, out ret);
			return ret;
		}

	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Commons
{
	public static class XMLUtils
	{
		/// <summary>
		///Open XML file path
		/// </summary>
		/// <param name="fpath"></param>
		/// <returns> return null when file is invalid</returns>
		public static XmlDocument LoadXMLDocument(string fpath)
		{
			if (!File.Exists(fpath))
				return null;
			try
			{
				XmlDocument doc = new XmlDocument();
				doc.Load(fpath);
				return doc;
			}
			catch (Exception)
			{
				return null;
			}
		}
		public static XmlDocument XMLDocument(string xml)
		{
			XmlDocument doc = new XmlDocument() ;
			try
			{
				doc.LoadXml(xml);
				return doc;
			}
			catch (Exception)
			{
			}
			return doc;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="filepath"></param>
		/// <param name="str_root">default  root document when file is invalid</param>
		/// <returns></returns>
		public static XmlDocument LoadCreateXMLDocumnet(string filepath, string str_root = null)
		{
			if (File.Exists(filepath))
				return LoadXMLDocument(filepath);

			XmlDocument m_doc = new XmlDocument();
			XmlDeclaration dec = m_doc.CreateXmlDeclaration("1.0", null, null);
			m_doc.AppendChild(dec);
			if( !string.IsNullOrEmpty( str_root))
			{
				XmlElement root = m_doc.CreateElement(str_root);
				m_doc.AppendChild(root);
			}
			return m_doc;
		}

		public static XmlNode SelectNode(XmlNode xmlnode, string xpath)
		{
			if (xmlnode == null)
				return null;
			return xmlnode.SelectSingleNode(xpath);
		}

		public static XmlNode SelectCreateChildNode(XmlNode parent, string child_name)
		{
			string xpath = string.Format(ConstEnums.XPATH_CHILD_NODE, child_name);
			XmlNode child = SelectNode(parent, xpath);
			if (child == null)
				child = CreateXMLNode(parent, child_name);

			return child;
		}

		public static XmlNode CreateXMLNode(XmlNode Parent, string element_name)
		{
			XmlNode child = CreateXMLNode(Parent.OwnerDocument, element_name);

			return Parent.AppendChild(child);
		}

		public static XmlNode CreateXMLNode(XmlDocument xmldoc, string element_name)
		{
			return xmldoc.CreateNode(XmlNodeType.Element, element_name, null);
		}

		private static XmlAttribute CreateXMLAttribute(XmlDocument xmldoc, string name, string value)
		{
			XmlAttribute new_att = xmldoc.CreateAttribute(name);
			new_att.Value = value;
			return new_att;
		}

		public static XmlNode SelectNodebyAttribute(XmlNode xmlnode, string elementname, List<KeyValuePair<string, string>> lstattibutes)
		{
			if (xmlnode == null)
				return null;
			string xpath = string.Format(ConstEnums.XPATH_CHILD_NODE, elementname);
			if (lstattibutes != null && lstattibutes.Count > 0)
			{
				xpath += "[";
				lstattibutes.ForEach(item => xpath += string.Format( ConstEnums.XPATH_COMPARE_EQUAL + " " + ConstEnums.STR_and, item.Key, item.Value));
				if (xpath.EndsWith(ConstEnums.STR_and))
					xpath = xpath.Substring(0, xpath.Length - ConstEnums.STR_and.Length);
				xpath += "]";
			}

			return SelectNode(xmlnode, xpath);
		}

		public static XmlNode SelectNodebyAttribute(XmlDocument xmldocument, string elementname, string attributename, string attributevalue)
		{
			if (xmldocument == null || xmldocument.DocumentElement == null)
				return null;

			XmlNode root = xmldocument.DocumentElement;

			return SelectNodebyAttribute(root, elementname, attributename, attributevalue);
		}

		public static XmlNode SelectNodebyAttribute(XmlNode xmlnode, string elementname, string attributename, string attributevalue)
		{
			if (xmlnode == null)
				return null;
			string xpath = string.Format(ConstEnums.XPATH_ATTRIBUTE, elementname, attributename, attributevalue);
			return SelectNode(xmlnode, xpath);
		}

		public static XmlNode SelectCreateNodebyAttribute(XmlDocument xmldocument, string elementname, string attributename, string attributevalue)
		{
			XmlNode root = xmldocument.DocumentElement;

			return SelectCreateNodebyAttribute(root, elementname, attributename, attributevalue);
		}

		public static XmlNode SelectCreateNodebyAttribute(XmlNode xmlnode, string elementname, string attributename, string attributevalue)
		{
			if (xmlnode == null)
				return null;
			string xpath = string.Format(ConstEnums.XPATH_ATTRIBUTE, elementname, attributename, attributevalue);
			XmlNode new_node = SelectNode(xmlnode, xpath);
			if (new_node == null)
				new_node = CreateXMLNode(xmlnode, elementname);

			SetXMLAttribute(new_node, attributename, attributevalue);
			return new_node;
		}

		public static XmlAttribute XMLAttribute(XmlNode node, string attName)
		{
			if (node == null || node.Attributes == null || node.Attributes.Count == 0 || string.IsNullOrEmpty(attName))
				return null;
			return node.Attributes.Cast<XmlAttribute>().SingleOrDefault(item => string.Compare(item.Name, attName, true) == 0);
		}

		public static string XMLAttributeValue(XmlNode node, string attName)
		{
			if (node == null || node.Attributes == null || node.Attributes.Count == 0 || string.IsNullOrEmpty(attName))
				return string.Empty;
			XmlAttribute xmlatt = XMLAttribute(node, attName);
			return xmlatt == null ? string.Empty : xmlatt.Value;
		}

		public static void SetXMLAttribute(XmlNode node, string attName, object value)
		{
			SetXMLAttribute(node, attName, value == null ? string.Empty : value.ToString());
		}

		public static void SetXMLAttribute(XmlNode node, string attName, string value)
		{
			XmlAttribute xmlatt = XMLAttribute(node, attName);
			if (xmlatt == null)
			{
				xmlatt = CreateXMLAttribute(node.OwnerDocument, attName, value);
				node.Attributes.Append(xmlatt);
			}
			else
				xmlatt.Value = value;

		}

		public static void XMLDeleteAllChild(XmlNode parent)
		{
			while (parent.HasChildNodes)
				parent.RemoveChild(parent.FirstChild);

		}
	}
}

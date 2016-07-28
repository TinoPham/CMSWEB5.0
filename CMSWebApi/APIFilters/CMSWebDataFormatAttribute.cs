using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using Newtonsoft.Json.Serialization;

namespace CMSWebApi.APIFilters
{
	public class CMSWebDataFormatAttribute : Attribute, IControllerConfiguration
	{
		public void Initialize(HttpControllerSettings controllerSettings,
							   HttpControllerDescriptor controllerDescriptor)
		{
			
			//controllerSettings.Formatters.Clear();
			RemoveUnsupportFormatter(controllerSettings, Utils.Consts.Application_Json);
			RemoveUnsupportFormatter(controllerSettings, Utils.Consts.Application_Xml);
			controllerSettings.Formatters.Add(JsonFormatSetting());
			//dont support XML format
			//controllerSettings.Formatters.Add(XmlFormatSetting());
		}

		private void RemoveUnsupportFormatter(HttpControllerSettings controllerSettings, string formatter)
		{
			if( string.IsNullOrEmpty(formatter))
				return;

			var rmove = controllerSettings.Formatters.FirstOrDefault(i => i.SupportedMediaTypes.FirstOrDefault(si => si.MediaType == formatter) != null);
			if (rmove != null)
				controllerSettings.Formatters.Remove(rmove);
		}

		private MediaTypeFormatter XmlFormatSetting()
		{
			var xml = new IgnoreNamespacesXmlMediaTypeFormatter();
			
			return xml;
		}

		private MediaTypeFormatter JsonFormatSetting()
		{
			var json = new JsonMediaTypeFormatter();
			json.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
			json.SerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.None;
			return json;
		}

	}

	public class IgnoreNamespacesXmlMediaTypeFormatter : XmlMediaTypeFormatter
	{
		public IgnoreNamespacesXmlMediaTypeFormatter():base()
		{
			base.Indent = true;
		}

		public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
		{
			
			try
			{
				var xns = new XmlSerializerNamespaces();
				foreach (var attribute in type.GetCustomAttributes(true))
				{
					var xmlRootAttribute = attribute as XmlRootAttribute;
					if (xmlRootAttribute != null)
					{
						xns.Add(string.Empty, xmlRootAttribute.Namespace);
					}
				}

				if (xns.Count == 0)
				{
					xns.Add(string.Empty, type.Name);
				}


				var task = Task.Factory.StartNew(() =>
				{
					var setting = new DataContractSerializerSettings { PreserveObjectReferences = true, SerializeReadOnlyTypes = false, DataContractSurrogate = new InvalidEnumContractSurrogate() };
					var xmlDictionary = new XmlDictionary();
					var dcs = new DataContractSerializer(type, type.Name, string.Empty, null, 0x7FFF, false, true, new InvalidEnumContractSurrogate());
					dcs.WriteObject(writeStream, value);

				});

				return task;
			}
			catch (Exception)
			{
				return base.WriteToStreamAsync(type, value, writeStream, content, transportContext);
			}
		}
	}

	/// <summary>
	/// IDataContractSurrogate to map Enum to int for handling invalid values
	/// </summary>
	public class InvalidEnumContractSurrogate : IDataContractSurrogate
	{
		private HashSet<Type> typelist;

		public InvalidEnumContractSurrogate()
		{
			typelist = new HashSet<Type>();
		}
		/// <summary>
		/// Create new Data Contract Surrogate to handle the specified Enum type
		/// </summary>
		/// <param name="type">Enum Type</param>
		public InvalidEnumContractSurrogate(Type type)
		{
			typelist = new HashSet<Type>();
			if(type != null && type.IsEnum)
				typelist.Add(type);
		}

		/// <summary>
		/// Create new Data Contract Surrogate to handle the specified Enum types
		/// </summary>
		/// <param name="types">IEnumerable of Enum Types</param>
		public InvalidEnumContractSurrogate(IEnumerable<Type> types)
		{
			typelist = new HashSet<Type>();
			foreach (var type in types)
			{
				if (!type.IsEnum)
					continue;

				typelist.Add(type);
			}
		}

		#region Interface Implementation

		public Type GetDataContractType(Type type)
		{
			//If the provided type is in the list, tell the serializer it is an int
			if (type.IsEnum || typelist.Contains(type))
				return typeof(int);
			return type;
		}

		public object GetObjectToSerialize(object obj, Type targetType)
		{
			//If the type of the object being serialized is in the list, case it to an int
			if (obj.GetType().IsEnum || typelist.Contains(obj.GetType()))
				return (int)obj;
			return obj;
		}

		public object GetDeserializedObject(object obj, Type targetType)
		{
			//If the target type is in the list, convert the value (we are assuming it to be int) to the enum
			if (targetType.IsEnum || typelist.Contains(targetType))
				return Enum.ToObject(targetType, obj);
			return obj;
		}

		public void GetKnownCustomDataTypes(System.Collections.ObjectModel.Collection<Type> customDataTypes)
		{
			//not used
			return;
		}

		public object GetCustomDataToExport(Type clrType, Type dataContractType)
		{
			//Not used
			return null;
		}

		public object GetCustomDataToExport(System.Reflection.MemberInfo memberInfo, Type dataContractType)
		{
			//not used
			return null;
		}

		public Type GetReferencedTypeOnImport(string typeName, string typeNamespace, object customData)
		{
			//not used
			return null;
		}

		public System.CodeDom.CodeTypeDeclaration ProcessImportedType(System.CodeDom.CodeTypeDeclaration typeDeclaration, System.CodeDom.CodeCompileUnit compileUnit)
		{
			//not used
			return typeDeclaration;
		}

		#endregion
	}
	
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Net.Http.Formatting;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;

namespace Commons
{
	public static class  ObjectUtils
	{
		#region public methods

		public static TData BinaryDeserializeFromBase64String<TData>(string settings)
		{
			byte [] b = Convert.FromBase64String(settings);
			using (var stream = new MemoryStream(b))
			{
				var formatter = new BinaryFormatter();
				stream.Seek(0, SeekOrigin.Begin);
				return (TData)formatter.Deserialize(stream);
			}
		}

		public static string BinarySerializeToBase64String<TData>(TData settings)
		{
			using (var stream = new MemoryStream())
			{
				var formatter = new BinaryFormatter();
				formatter.Serialize(stream, settings);
				stream.Flush();
				stream.Position = 0;
				return Convert.ToBase64String(stream.ToArray());
			}
		}

		public static byte[] StructuretoBytes<T>(T datastructure) where T: struct
		{
			int size = Marshal.SizeOf(datastructure);
			byte[] arr = new byte[size];
			IntPtr ptr = Marshal.AllocHGlobal(size);

			Marshal.StructureToPtr(datastructure, ptr, true);
			Marshal.Copy(ptr, arr, 0, size);
			Marshal.FreeHGlobal(ptr);

			return arr;
		}	
		
		public static T BytestoStructure<T>(byte[]buffer ) where T: struct
		{
			T result = default(T);
			if( buffer == null || buffer.Length == 0)
				return result;

			GCHandle hnd = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			result = (T)Marshal.PtrToStructure(hnd.AddrOfPinnedObject(), typeof(T));
			hnd.Free();

			return result;
		}
		
		public static string Serialize<T>(MediaTypeFormatter formatter, T value)
		{
			return Serialize(formatter, typeof(T), value);
		}

		public static string Serialize(MediaTypeFormatter formatter, Type type, object value)
		{
			// Create a dummy HTTP Content.

			Stream stream = new MemoryStream();
			var content = new StreamContent(stream);
			/// Serialize the object.
			formatter.WriteToStreamAsync(type, value, stream, content, null).Wait();
			// Read the serialized string.
			stream.Position = 0;
			return content.ReadAsStringAsync().Result;
		}

		public static T DeSerialize<T>(MediaTypeFormatter formatter, string value)
		{
			object ret = DeSerialize(formatter, typeof(T), value);

			return ret == null? default(T) : (T)ret;
		}
		
		public static object DeSerialize(MediaTypeFormatter formatter, Type type, string value)
		{
			// Create a dummy HTTP Content.
			Stream stream = new MemoryStream();
			try
			{
				byte[] buff = formatter.SupportedEncodings.First().GetBytes(value);//Encoding.Default.GetBytes(value);
				stream.Write(buff, 0, buff.Length);
				stream.Position = 0;
				var content = new StreamContent(stream);
				Task<object> tobj = formatter.ReadFromStreamAsync(type, stream, content, null);
				tobj.Wait();
				return tobj.Result;
			}
			catch(Exception){
				return null;
			}
			finally{
				stream.Close();
				stream = null;
			}
			
		}

		public static object DeSerialize(Type type, string value)
		{
			MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(value));
			object cfgData = default(object);
			try
			{
				System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(type);
				cfgData = ser.Deserialize(stream);
			}
			catch (Exception)
			{
				cfgData = null;
			}
			return cfgData;
		}

		//public static object DeSerializeObject(Type type, string value)
		//{
		//	MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(value));
		//	object cfgData = default(object);
		//	return cfgData;


		//}

		private static T DeserializeFromXMLString<T>(string xmlData)
		{
			if (string.IsNullOrEmpty(xmlData))
				return default(T);

			MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(xmlData));
			T obj = default(T);
			obj = Deserialize<T>(stream);
			if (stream != null)
			{
				stream.Close();
				stream.Dispose();
				stream = null;
			}
			return obj;
		}

		private static T Deserialize<T>(Stream stream)
		{
			T cfgData = default(T);
			try
			{
				System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(typeof(T));
				cfgData = (T)ser.Deserialize(stream);
			}
			catch (Exception)
			{
			}
			return cfgData;
		}
		public static object InitObject(Type type, Type[]generictype,  object[] prams = null)
		{
			if( type == null)
				return null;
			Type constructor = type.MakeGenericType( generictype);
			if( constructor == null )
				return null;
			return Activator.CreateInstance(constructor, prams);
		}
		public static object InitObject(Type type, object[] prams = null)
		{
			object ret_obj;
			try
			{
				if( type ==  typeof(List<>))
				{
					Type genericListType = typeof(List<>).MakeGenericType(type);
					ret_obj = Activator.CreateInstance(genericListType);
				}
				 else
					ret_obj = Activator.CreateInstance(type, prams);
			}catch(Exception){ ret_obj = null;}

			return ret_obj;
		}

		public static T InitObject<T>( object[] prams = null ) where T: class
		{
			object ret_obj = InitObject( typeof(T), prams);
			return ret_obj as T;
		}

		public static MethodInfo GetMethodbyName(object obj, string methodName, Type[] prams)
		{
			return obj.GetType().GetMethod(methodName, prams);
		}
		
		public static T GetValueInObject<T>( object obj, string key_name)
		{
			PropertyInfo pinfo = GetProperty(obj, key_name);
			if( pinfo != null)
				return Utils.ChangeSimpleType<T>(pinfo.GetValue(obj, null));
			FieldInfo finfo = GetField(obj, key_name);
			if( finfo != null)
				return Utils.ChangeSimpleType<T>(finfo.GetValue(obj));
			return Utils.ChangeSimpleType<T>(null);
		}

		public static T GetFieldValue<T>(object obj, string FieldName)
		{
			if (obj == null)
				return Utils.ChangeSimpleType<T>(obj);
			if (string.IsNullOrEmpty(FieldName))
			{
				Utils.ThrowExceptionMessage(ConstEnums.INTERNAL_MSG_EMPTY_FIELD);
			}
			FieldInfo finfo = GetField(obj, FieldName);

			return Utils.ChangeSimpleType<T>( finfo.GetValue(obj));
		}
		
		public static bool SetPropertyValue(object desObj, string PropertyName, object value)
		{
			PropertyInfo pinfo = GetProperty(desObj, PropertyName);
			if( pinfo == null)
				return false;

			return SetPropertyValue(pinfo, desObj, value);
			
		}

		public static bool SetPropertyValue(PropertyInfo pinfo, object desObj, object value)
		{
			if (pinfo == null)
				return false;

			pinfo.SetValue(desObj, ValueConverter(pinfo.PropertyType, value), null);
			return true;
		}


		public static object GetPropertyValue(object obj, string PropertyName)
		{
			if (string.IsNullOrEmpty(PropertyName))
			{
				Utils.ThrowExceptionMessage(ConstEnums.INTERNAL_MSG_EMPTY_PROPERTY);
			}

			PropertyInfo pinfo = GetProperty(obj, PropertyName);
            if (pinfo == null) return pinfo;
			return pinfo.GetValue(obj, null);
		}

		public static T GetPropertyValue<T>(object obj, string PropertyName)
		{
			if( obj == null)
				return Utils.ChangeSimpleType<T>(obj);

			if( string.IsNullOrEmpty(PropertyName))
			{
			  Utils.ThrowExceptionMessage(ConstEnums.INTERNAL_MSG_EMPTY_PROPERTY);
			}
			PropertyInfo pinfo = GetProperty(obj, PropertyName);
			return Utils.ChangeSimpleType<T>(pinfo.GetValue(obj, null));
		}
		
		public static DataTable List2table(IList lstItem, Type elementtype, MappingType default_xmlmapping = MappingType.Element)
		{
			if (lstItem == null)
				return null;
			Type type = elementtype;

			PropertyInfo[] pinfos = type.GetProperties(BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance);
			DataTable tblret = new DataTable();
			tblret.Columns.AddRange
				(
					pinfos.ToList().Select(item => new DataColumn(item.Name, item.PropertyType, null, default_xmlmapping)).ToArray()
				);
			object itemvalue = null;
			for (int i = 0; i < lstItem.Count; i++)
			{
				itemvalue = lstItem[i];
				tblret.Rows.Add
							(
								pinfos.ToList().Select(info => info.GetValue(itemvalue, null)).ToArray()
							);

			}

			return tblret;
		}

		public static DataTable List2table<T>(List<T> lstItem)
		{
			if (lstItem == null)
				return null;
			Type type = typeof(T);
			object obj = lstItem[0];
			type = lstItem.GetType().GetGenericTypeDefinition();
			PropertyInfo[] pinfos = type.GetProperties(BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance);
			DataTable tblret = new DataTable();
			tblret.Columns.AddRange
				(
					pinfos.ToList().Select(item => new DataColumn(item.Name, item.PropertyType)).ToArray()
				);
			lstItem.ForEach(
					item => tblret.Rows.Add
						(
							pinfos.ToList().Select(info => info.GetValue(item, null)).ToArray()
						)
				);
			return tblret;
		}

		/// <summary>
		/// Convert data row to T object
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="row"></param>
		/// <returns></returns>
		public static T RowToObject<T>(DataRow row)
		{
			object obj = RowToObject(row, typeof(T));
			return obj == null ? default(T) : (T)obj;
		}

		//public static object RowToObject(DataRow row, Type type)
		//{
		//	string xmlstring = RowtoString(row);
		//	MemoryStream mem = String2Stream(xmlstring);
		//	object ret = mem == null ? null : XmlString2Object(mem, type);
		//	if (mem != null)
		//	{
		//		mem.Close();
		//		mem.Dispose();
		//		mem = null;
		//	}
		//	return ret;
		//}
		public static object RowToObject(DataRow row, Type type)
		{
			string xmlstring = RowtoString(row);
			var xmlReaderSettings = new XmlReaderSettings() {CheckCharacters = false};

			XmlReader xmlReader = XmlTextReader.Create(new StringReader(xmlstring), xmlReaderSettings);
			XmlSerializer xs = new XmlSerializer(type);

			return xs.Deserialize(xmlReader);
		}

		public static object DeserializeRowToObject(DataRow row, Type type)
		{
			string xmlstring = RowtoString(row);
			using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(xmlstring)))
			{
				XmlSerializer xmlSerializer = new XmlSerializer(type);
				StreamReader reader = new StreamReader(memoryStream,Encoding.UTF8);

				return xmlSerializer.Deserialize(reader);
			}
		}

		public static T RowToObject<T>(DataTable table, int index)
		{
			if (table == null)
				return default(T);
			return RowToObject<T>(table.Rows[index]);
		}

		public static List<T> TabletoList<T>(DataTable table)
		{

			string root = string.Empty;
			MemoryStream xmlstream = Table2Stream(table, out root);
			if (xmlstream == null)
				return null;
			object obj = XMLStream2IList(xmlstream, root, typeof(T));
			xmlstream.Close();
			xmlstream.Dispose();
			xmlstream = null;

			return obj == null ? null : (List<T>)obj;
		}

		public static T XmlString2Object<T>(MemoryStream mem)
		{
			T ret;
			XmlSerializer ser = ser = new XmlSerializer(typeof(T));

			ret = (T)ser.Deserialize(mem);

			return ret;
		}

		public static T XmlString2Object<T>(FileStream fstream)
		{
			T ret;
			XmlSerializer ser = ser = new XmlSerializer(typeof(T));

			ret = (T)ser.Deserialize(fstream);

			return ret;
		}

		public static T Dataset2Object<T>(DataSet ds)
		{
			object ret = Dataset2Object(ds, typeof(T));
			return ret == null ? default(T) : (T)ret;
		}

		public static object Dataset2Object(DataSet ds, Type type)
		{
			string type_name = type.Name;
			object ret = null;
			if (!ds.Tables.Contains(type_name) || ds.Tables[type_name].Rows.Count == 0)
			{
				ConstructorInfo cretinfo = type.GetConstructor(System.Type.EmptyTypes);
				ret = cretinfo.Invoke(null);
			}
			else
				ret = RowToObject(ds.Tables[type_name].Rows[0], type);

			FieldInfo[] finfo = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
			PropertyInfo[] pinfo = type.GetProperties();
			IEnumerable<FieldInfo> filter = finfo.Where(item => item.FieldType.IsClass == true);
			object mem_value = null;
			Type objtype = null;
			DataTable tbl_memvalue = null;
			string root = string.Empty;
			MemoryStream xmlstream = null;
			foreach (FieldInfo item in filter)
			{
				if (string.Compare(item.FieldType.Name, "string", true) == 0)
					continue;


				if (item.FieldType.IsGenericType)
				{
					PropertyInfo iinfo = item.FieldType.GetProperty("Item");
					objtype = Type.GetType(iinfo.PropertyType.FullName);
					tbl_memvalue = ds.Tables.Contains(objtype.Name) ? ds.Tables[objtype.Name] : null;
					xmlstream = Table2Stream(tbl_memvalue, out root);
					mem_value = XMLStream2IList(xmlstream, root, objtype);


				}
				else
				{
					objtype = Type.GetType(item.FieldType.FullName);
					mem_value = Dataset2Object(ds, objtype); //RowToObject(tbl_memvalue == null ? null : tbl_memvalue.Rows[0], objtype);
				}
				item.SetValue(ret, mem_value);

				mem_value = null;
				tbl_memvalue = null;
				root = string.Empty;

				if (xmlstream != null)
				{
					xmlstream.Close();
					xmlstream.Dispose();
					xmlstream = null;
				}

			}
			return ret;
		}
		#endregion

		#region private methods
		private static List<T> XMLStream2List<T>(Stream mem, string strroot)
		{
			if (mem == null || mem.Length == 0)
				return new List<T>();
			XmlSerializer deserializer = null;
			if (!string.IsNullOrEmpty(strroot))
			{
				XmlRootAttribute root = new XmlRootAttribute(strroot);
				deserializer = new XmlSerializer(typeof(List<T>), root);
			}
			else
				deserializer = new XmlSerializer(typeof(List<T>));

			List<T> lstRet;
			lstRet = (List<T>)deserializer.Deserialize(mem);
			return lstRet;
		}

		private static object XmlString2Object(MemoryStream mem, Type type)
		{
			XmlSerializer ser =  new XmlSerializer(type);

			return ser.Deserialize(mem);
		}

		private static object XMLStream2IList(Stream mem, string strroot, Type t)
		{
			if (mem == null || mem.Length == 0)
				return null;

			var listType = typeof(List<>);
			var constructedListType = listType.MakeGenericType(t);
			var instance = Activator.CreateInstance(constructedListType);


			XmlSerializer deserializer = null;
			if (!string.IsNullOrEmpty(strroot))
			{
				XmlRootAttribute root = new XmlRootAttribute(strroot);
				deserializer = new XmlSerializer(instance.GetType(), root);
			}
			else
				deserializer = new XmlSerializer(instance.GetType());


			return deserializer.Deserialize(mem);

		}

		private static MemoryStream Table2Stream(DataTable table, out string root)
		{
			root = string.Empty;
			if (table == null || table.Rows.Count == 0)
				return null;

			MemoryStream mem = new MemoryStream();
			table.WriteXml(mem, false);
			mem.Seek(0, SeekOrigin.Begin);
			XmlDocument doc = new XmlDocument();
			doc.Load(mem);
			XmlNode node = doc.DocumentElement;
			root = node.Name;
			mem.Seek(0, SeekOrigin.Begin);
			return mem;
		}

		private static string RowtoString(DataRow row)
		{
			if (row == null)
				return null;

			DataTable table = row.Table.Clone();
			table.LoadDataRow(row.ItemArray, LoadOption.OverwriteChanges);
			table.TableName = row.Table.TableName;

			if (table == null)
				return null;

			MemoryStream mem = new MemoryStream();

			table.WriteXml(mem, false);
			mem.Seek(0, SeekOrigin.Begin);
			XmlDocument doc = new XmlDocument();
			doc.Load(mem);
			XmlNode node = doc.DocumentElement;

			mem.Close();
			mem.Dispose();
			mem = null;
			return node.InnerXml;
		}

		private static MemoryStream String2Stream(string strbuff)
		{
			byte[] buff = System.Text.Encoding.Default.GetBytes(strbuff);
			return new MemoryStream(buff);

		}

		private static PropertyInfo GetProperty( object obj, string PropertyName)
		{
			if (obj == null)
				return null;
			if (string.IsNullOrEmpty(PropertyName))
			{
				Utils.ThrowExceptionMessage(ConstEnums.INTERNAL_MSG_EMPTY_PROPERTY);
			}
			PropertyInfo pinfo = obj.GetType().GetProperty(PropertyName, BindingFlags.Instance | BindingFlags.Public);
			if (pinfo == null)
			{
				Utils.ThrowExceptionMessage(string.Format(ConstEnums.INTERNAL_MSG_INVALID_PROPERTY, PropertyName));
			}
			return pinfo;
		}

		private static FieldInfo GetField(object obj, string FieldName)
		{
			if (obj == null)
				return null;
			if (string.IsNullOrEmpty(FieldName))
			{
				Utils.ThrowExceptionMessage(ConstEnums.INTERNAL_MSG_EMPTY_FIELD);
			}
			FieldInfo pinfo = obj.GetType().GetField(FieldName, BindingFlags.Instance | BindingFlags.Public);
			if (pinfo == null)
			{
				Utils.ThrowExceptionMessage(string.Format(ConstEnums.INTERNAL_MSG_INVALID_FIELD, FieldName));
			}
			return pinfo;
		}

		public static object ValueConverter(Type type, object value)
		{
			Type t = Nullable.GetUnderlyingType(type)?? type;
			if( type.Equals(typeof(System.Guid)))
			{
				Guid ret = new Guid();
				return Guid.TryParse( value.ToString(), out ret)? ret : Guid.NewGuid();
			}
			object safeValue = (value == null || string.IsNullOrEmpty(value.ToString())) ? null : Convert.ChangeType(value, t);
			return safeValue;

		}
		#endregion
	}
}

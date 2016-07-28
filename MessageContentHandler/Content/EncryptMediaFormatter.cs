using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using Cryptography;
using System.Web;
using System.Text.RegularExpressions;

namespace MessageContentHandler.Content
{
	 public class EncryptMediaFormatter : MediaTypeFormatter
	{

		 public const string Accept_Header = "application/encrypt";
		 const int DEFAULT_KEY_LEN = 32;
		 const string DATE_FORMAT = "yyyyMMddHHmmss";
		 const char Separate_Item = '\n';
		 const char End_Header_Char = ':';
		 const string Regex_date = @"^(?<Year>\d{4})(?<Month>\d{2})(?<Day>\d{2})(?<Hour>\d{2})(?<Min>\d{2})(?<Sec>\d{2})";

		private class MsgHeader:IDisposable
		{
			const int MAX_HEADER_LEN = 14 + 5 + 5;//datetime + key len + iv len

			string Token;
			public string Key { get; private set;}
			public string IV{ get; private set;}
			public DateTime Date{ get ;set;}
			public MsgHeader(string token)
			{
				Token = token;
			}

			public string HeaderString( int kindex, int ivindex)
			{
				string ret = string.Format("{0}{3}{1}{3}{2}{3}", Date.ToUniversalTime().ToString(DATE_FORMAT), kindex, ivindex, Separate_Item.ToString());
				return ret;
			} 

			public void Parser( Stream data, bool base64 = true)
			{
				int _char ;
				List<byte> buff = new List<byte>();
				while( (_char = data.ReadByte()) > -1)
				{
					if( _char == End_Header_Char)
						break;

					buff.Add((byte)_char);
				}
				if( buff.Count == 0)
					return;
				string strheader = Encoding.UTF8.GetString( buff.ToArray());
				Parser(strheader);
			}
			
			public void Parser( string data, bool base64 = true)
			{
				if( string.IsNullOrEmpty(data))
					return;

				string strobj = base64 ? Commons.Utils.Base64toString(data) : data;
				string[] items = strobj.Split( new char[]{Separate_Item});
				if( items.Length == 0 || items.Length < 3)
					return;

				Date = parserDate( items[0]);
				Key = GetKeyToken(Convert.ToInt32(items [1]) );
				IV = GetKeyToken( Convert.ToInt32(items [2]));
			}

			public void Dispose()
			{
				Token = null;
				Key = null;
				IV = null;
			}
			
			private string GetKeyToken(int index, int len = DEFAULT_KEY_LEN)
			{
				if (string.IsNullOrEmpty(Token))
					return null;
				if (index + len > Token.Length)
					return null;
				return Token.Substring( index, len);
			}

			private DateTime parserDate( string strdate)
			{
				DateTime ret = DateTime.MinValue;
				Regex rx = new Regex(Regex_date);
					Match match = rx.Match(strdate);

					if(!match.Success)
						return ret;
					ret = new DateTime(Convert.ToInt16(match.Groups ["Year"].Value), Convert.ToInt16(match.Groups ["Month"].Value), Convert.ToInt16(match.Groups ["Day"].Value), Convert.ToInt16(match.Groups ["Hour"].Value), Convert.ToInt16(match.Groups ["Min"].Value), Convert.ToInt16(match.Groups ["Sec"].Value), DateTimeKind.Utc);

				return ret;
			}
		}

		private class EncMessage: IDisposable
		{
			
			public MsgHeader Header{ get; private set;}
			public String Data{ get; private set;}
			private string Token;
			 
			public EncMessage( string token)
			{
				Token = token;
				Header = new MsgHeader(token);
			}
			
			public void ParserData( Stream stream)
			{
				byte[] b64buff = new byte[ stream.Length - stream.Position];
				stream.Read(b64buff, 0, b64buff.Length);
				String b64string = Encoding.UTF8.GetString(b64buff);
				byte [] encrypted = Convert.FromBase64String(b64string);
				byte[]key = Commons.Utils.String2Byte(Header.Key);
				byte[]iv =  Commons.Utils.String2Byte( Header.IV);
				//on JS only support 16 bytes for IV
				Data = OpenSSLSHA256.DecryptStringFromBytesAes(encrypted , key, iv.Take(16).ToArray());

			}
			
			public void ParserHeader(Stream stream)
			{
				Header.Parser(stream);
			}

			public string FormatMessage( string rawdata)
			{
				if( Token.Length < DEFAULT_KEY_LEN)
					return null;

				int kindex = Cryptography.Utils.Between(0, Token.Length - DEFAULT_KEY_LEN);
				int ivindex = Cryptography.Utils.Between(0, Token.Length - DEFAULT_KEY_LEN);
				string key = GetKey(Token, kindex);
				string iv = GetKey(Token, ivindex);
				string header = Header.HeaderString(kindex, ivindex);
				byte[] bkey = Commons.Utils.String2Byte(key);
				byte[] biv =  Commons.Utils.String2Byte( iv);
				//on JS only support 16 bytes for IV
				string data = Commons.Utils.ToBase64String(OpenSSLSHA256.EncryptStringToBytesAes(rawdata, bkey, biv.Take(16).ToArray() ));
				return Commons.Utils.String2Base64( header)  + End_Header_Char + data;
			}
			 
			public void Dispose()
			{
				Header.Dispose();
				Data = null;
				Token = null;
			}
			private string GetKey(string token, int index)
			{
				if(index < 0 || index + DEFAULT_KEY_LEN >  token.Length)
					return null;
				return token.Substring( index, DEFAULT_KEY_LEN);

			}

			
		}
		
		private readonly JsonSerializerSettings jsonSetting;
		private readonly string[] Header_Key;
		long valid_msg_time = 5;

		public EncryptMediaFormatter(HttpConfiguration config, int msgdelay, string [] headerKeys = null, string HeaderValue = Accept_Header)
			: base()
		{
			if( headerKeys == null)
				Header_Key = new string[]{"XSRF-TOKEN","SID"};
			else
				Header_Key = headerKeys;
			valid_msg_time = (msgdelay <= 0 ? valid_msg_time : msgdelay) * TimeSpan.TicksPerSecond;
			SupportedMediaTypes.Add(new MediaTypeHeaderValue(HeaderValue));
			jsonSetting = new JsonSerializerSettings
			{
				DateTimeZoneHandling = DateTimeZoneHandling.Utc,
				PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.None
			};
		}

		public override bool CanWriteType(Type type)
		{
				return true;
		}

		public override bool CanReadType(Type type)
		{
			return true;
		}

		public override MediaTypeFormatter GetPerRequestFormatterInstance(Type type, HttpRequestMessage request, MediaTypeHeaderValue mediaType)
		{
			MediaTypeFormatter formatter = base.GetPerRequestFormatterInstance(type, request, mediaType);

			return formatter;
		}
		
		public override System.Threading.Tasks.Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, System.Net.TransportContext transportContext)
		{
			string encryptkey = GetToken(HttpContext.Current.Response);
			if( string.IsNullOrEmpty( encryptkey))
				encryptkey = GetToken(HttpContext.Current.Request);

			return Task.Run( () => 
					{
						WriteToStream(type, value, writeStream, content, encryptkey);
					}
			);
		}

		public override System.Threading.Tasks.Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, System.Net.TransportContext transportContext, System.Threading.CancellationToken cancellationToken)
		{
			return base.WriteToStreamAsync(type, value, writeStream, content, transportContext, cancellationToken);
		}

		public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger, System.Threading.CancellationToken cancellationToken)
		{
			return base.ReadFromStreamAsync(type, readStream, content, formatterLogger, cancellationToken);
		}

		public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
		{
			//return base.ReadFromStreamAsync(type, readStream, content, formatterLogger);
			var hder = HttpContext.Current.Request;
			
			string token = GetToken( hder);
			return Task.Run<object>( () =>
				{
					return ReadFromStream(type, readStream, content, token);
				}
			);
		}


		private string GetToken( HttpRequest request)
		{
			string token = CookieToken(request.Cookies);
			if( string.IsNullOrEmpty(token))
				token = GetSID( request.Headers);
			return token;
		}

		private string GetToken(HttpResponse response)
		{
			string token = CookieToken(response.Cookies);
			if (string.IsNullOrEmpty(token))
				token = GetSID(response.Headers);
			return token;
		}
		
		private string CookieToken( HttpCookieCollection cookies)
		{
			string token = string.Empty;
			foreach( string key in Header_Key)
			{
				if( cookies.AllKeys.FirstOrDefault( item => item == key) == null)
					continue;

				HttpCookie ck = cookies.Get( key);
				if( ck != null && !string.IsNullOrEmpty(ck.Value))
				{
					token = ck.Value;
					break;
				}
			}
			return token;
		}

		private string GetSID(NameValueCollection headers)
		{
			string sidkey = string.Empty;
			foreach( string key in Header_Key)
			{
				if( headers.AllKeys.FirstOrDefault( item => item == key) == null )
					continue;
				string [] sid = headers.GetValues(key);
				if( sid != null)
				{
					sidkey = sid.FirstOrDefault(); 
					break;
		}

			}
			return sidkey;
		}

		private void WriteToStream(Type type, object value, Stream writeStream, HttpContent content, string token)
		{
			using (var writer = new StreamWriter(writeStream))
			{
				string buff = Encrypt(type, value, token);
				writer.Write(buff);
			}
		}

		private object ReadFromStream(Type type, Stream readStream, HttpContent content, string token)
		{
			using (EncMessage msg = new EncMessage(token))
			{
				msg.ParserHeader(readStream);
				//if (msg.Header == null || Math.Abs(DateTime.UtcNow.Ticks - msg.Header.Date.Ticks) > valid_msg_time)
				//	return null;
				msg.ParserData(readStream);
				string jsontext = msg.Data; //Decrypt(readStream, token);
				try
		{
				object ret = JsonConvert.DeserializeObject(jsontext, type, jsonSetting);

				return ret;
			}
			catch(Exception)
			{
				return null;
			}
		}
			
		}
		
		private string Encrypt(Type type, object value, string encryptkey)
		{
			string val = JsonConvert.SerializeObject(value, jsonSetting);
			using (EncMessage msg = new EncMessage(encryptkey))
			{
				return msg.FormatMessage(val);
			}

			//string val = JsonConvert.SerializeObject(value, jsonSetting);
	
			//return OpenSSLSHA256.OpenSSLEncrypt(val, encryptkey);
		}

		//private string Decrypt(Stream stream, string token)
		//{
		//	byte [] buff = new byte [stream.Length];
		//	stream.Read(buff, 0, buff.Length);
		//	string b64 = System.Text.Encoding.UTF8.GetString(buff);
		//	 return OpenSSLSHA256.OpenSSLDecrypt(b64, decryptkey);
		//}
		
	}
}

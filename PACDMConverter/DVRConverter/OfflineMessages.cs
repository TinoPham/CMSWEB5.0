using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.Concurrent;
using ConvertMessage;
using ProtoBuf;
using System.Threading;

namespace PACDMConverter.DVRConverter
{
	internal class OfflineMessages : Commons.SingletonClassBase<OfflineMessages>
	{
		[ProtoContract]
		internal class OfflineMessage
		{
			[ProtoMember(1)]
			public bool Direction { get; set; }// true: DVR=> API, False : API => DVR

			[ProtoMember(2)]
			public string Mapping { get; set; }

			[ProtoMember(3)]
			public string Data { get; set; }
		}

		const string Offline_Dir = "Offline";
		const string Msg_Extension = ".msg";
		const string MSG_Encrypt_Key= "ODZkYTFhZDItOWRkYS00ODM1LWIzNzgtM2JmNDJmMjZlZDJm";
		readonly string Dir;

		private ConcurrentDictionary<long, string> DVRMessages;
		public int OfflineMessageCount{ get { return DVRMessages.Count;}}

		internal OfflineMessages()
		{
			DVRMessages = new ConcurrentDictionary<long,string>();
			Dir = Path.Combine(Utils.Instance.StartupDir, Offline_Dir);
			Utils.CreateDir(Dir);
			LoadOffline(Dir);
		}
		private void LoadOffline( string dir)
		{
			if( string.IsNullOrEmpty(dir))
				return;
			DirectoryInfo dinfo = new DirectoryInfo(dir);
			FileInfo[] files = dinfo.GetFiles("*" + Msg_Extension);
			long key = 0;

			foreach( FileInfo file in files)
			{
				if( !long.TryParse(Path.GetFileNameWithoutExtension( file.Name),out key))
					continue;
				DVRMessages.TryAdd(key, file.FullName);
			}
		}
		
		public void CleanUpMessage(double Keepingminutes, CancellationToken canceltoken)
		{
			if (Keepingminutes < 0)
				return;
			DateTime mindate = DateTime.UtcNow.AddMinutes(-Keepingminutes);
			IEnumerable<KeyValuePair<long, string>> delitems = DVRMessages.Where( it => it.Key < mindate.Ticks);
			long del_key;
			while(delitems.Any() && canceltoken.IsCancellationRequested == false)
			{
				del_key = delitems.First().Key;
				RemoveOfflineMessage(del_key);
			}
		}
		
		public void RemoveOfflineMessage(long key)
		{
			string fpath;
			if (DVRMessages.TryRemove(key, out fpath))
			{
				Utils.DeleteFile(fpath);
			}
		}

		public OfflineMessage GetOfflineMessage(out long key)
		{
			key = -1;
			if (DVRMessages == null || DVRMessages.Count == 0)
				return null;
			long firstkey = DVRMessages.Min(it => it.Key);
			key = firstkey;
			OfflineMessage result = null;
			string filename = string.Empty;
			DVRMessages.TryGetValue(key, out  filename);
			if( string.IsNullOrEmpty( filename) || !File.Exists(filename))
			{
				RemoveOfflineMessage(key);
				return null;
			}
			result = DeSerialized( filename);
			if(result == null)
			{
				RemoveOfflineMessage(key);
			}
			return result;
		}

		public void AddOfflineMessage(MessageData message, bool DVRtoApi)
		{
			if (message == null || string.IsNullOrEmpty(message.Mapping))
				return;
			long key = DateTime.UtcNow.Ticks;
			OfflineMessage msg = new OfflineMessage { Mapping = message.Mapping, Data = message.Data, Direction = DVRtoApi };
			string filename = MsgFileName( key); 
			if( Serialized(msg, filename))
			{
				DVRMessages.TryAdd(key, filename);
			}
		}

		private string MsgFileName( long key)
		{
			return Path.Combine(Dir, string.Format("{0}{1}", key, Msg_Extension));
		}

		#region serialize & Deserialize

		private OfflineMessage DeSerialized(string filename)
		{
			Stream stream =  null;
			try
			{
				if( string.IsNullOrEmpty(filename) || !File.Exists(filename))
					return null;

				stream = File.Open(filename, FileMode.Open);
				OfflineMessage result = Serializer.Deserialize<OfflineMessage>(stream);
				result.Data = Encrypt.SimpleStringEncrypt.DecryptString(result.Data, MSG_Encrypt_Key);
				return result;
			}
			catch (Exception)
			{
				return null;
			}
			finally
			{
				if (stream != null)
				{
					stream.Close();
					stream.Dispose();
				}
			}

		}

		private bool Serialized(OfflineMessage msg, string filename)
		{
			Stream stream = null;
			bool ret = true;
			try
			{
				string dir = Path.GetDirectoryName(filename);//  new FileInfo(filename);
				if (!Utils.CreateDir(dir))
					return false;
				OfflineMessage item = new OfflineMessage { Direction = msg.Direction, Mapping = msg.Mapping, Data = Encrypt.SimpleStringEncrypt.EncryptString(msg.Data, MSG_Encrypt_Key) };
				stream = File.Open(filename, FileMode.Create);
				Serializer.Serialize<OfflineMessage>(stream, item);
			}
			catch (Exception)
			{
				ret = false;
			}
			finally
			{
				if (stream != null)
				{
					stream.Close();
					stream.Dispose();
				}

			}
			return ret;
		}

		#endregion
	}
}

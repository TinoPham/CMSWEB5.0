using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using Commons;
using ConvertMessage;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;


namespace ServiceConfig
{
	class ValidateClientConfig :IDisposable
	{
		const string HEXA_FORMAT = "X2";
		const string AGENT = "ServiceConfig";
		const string METHOD_LOGIN = "ServiceInfo";
		const string STR_User_Agent = "User-Agent";
		const int HTTP_TIMEOUT = 60;//60 seconds

		HttpClient client;
		ConverterDB.Model.ServiceConfig SVRConfig;

		public ValidateClientConfig(ConverterDB.Model.ServiceConfig svrconfig)
		{
			SVRConfig = svrconfig;
			Initialize();
		}
		
		public void Dispose()
		{
			if( client != null)
			{
				client.Dispose();
				client  = null;
			}
		}
		
		private void Initialize()
		{
			HttpClientHandler clienthandler = new HttpClientHandler();
			client = new HttpClient(clienthandler);
			if (clienthandler.SupportsAutomaticDecompression)
				clienthandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
			clienthandler.ClientCertificateOptions = ClientCertificateOption.Automatic;
			client.DefaultRequestHeaders.TryAddWithoutValidation(STR_User_Agent, AGENT);
			ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
			client.Timeout = new TimeSpan(0,0, HTTP_TIMEOUT);

		}

		Uri GetUri(ConverterDB.Model.ServiceConfig svrconfig, string action)
		{
			//Anh Huynh, Support input base address for API Service
			//string urlpath = svrconfig.Url.Trim( new char[]{'/'});
			//return new Uri(new Uri(svrconfig.Url + "/"), action);
			string inputURL = svrconfig.Url.Trim(new char[] { '/' });
			if (!inputURL.StartsWith(Consts.STR_Http) && !inputURL.StartsWith(Consts.STR_Https))
			{
				inputURL = Consts.STR_Http + inputURL;
			}
			Uri urlpath = new Uri(inputURL);
			Uri baseURL = new Uri(urlpath, Consts.STR_ConverterURL);

			return new Uri(baseURL, action);
		}

		private Task<HttpResponseMessage> GetInfoService(ConverterDB.Model.ServiceConfig svrconfig, CancellationToken canceltoken)
		{
			Uri url = GetUri(svrconfig, METHOD_LOGIN);
			MediaTypeFormatter Formatter = new JsonMediaTypeFormatter();
			HttpRequestMessage rquest = new HttpRequestMessage( HttpMethod.Post, url);
			rquest.Content = new ObjectContent<MessageDVRInfo>(GetMsgDVRInfo(), Formatter);
			return client.SendAsync(rquest, canceltoken);//.ConfigureAwait(false);
		}

		private T ReadResponseContent<T>(HttpResponseMessage response, MediaTypeFormatter formatter, CancellationToken canceltoken)
		{
            T ret = default(T);
            //Chinh 12/9/2014 begin
            try
            {
                Task _task = response.Content.ReadAsAsync<T>(new MediaTypeFormatter[] { formatter }).ContinueWith
                    (
                        task =>
                        {
                            ret = task.Result;
                        }
                    , canceltoken, TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
                _task.Wait(canceltoken);
            }
            catch (Exception) { }

            //Chinh 12/9/2014 end
			return ret;
		}

		public MessageResult GetInfoServices()
		{
			CancellationTokenSource canceltoken = new CancellationTokenSource();
			MediaTypeFormatter Formatter = new JsonMediaTypeFormatter();
			HttpResponseMessage Response = null;
			Task<HttpResponseMessage> response_task;
			try
			{
                response_task = GetInfoService(SVRConfig, canceltoken.Token);
                //Chinh 12-9-2014 begin
                try
                {
                    response_task.Wait(canceltoken.Token);
                }
                catch (AggregateException ex)
                {
                    return new MessageResult { ErrorID = Commons.ERROR_CODE.SERVICE_EXCEPTION, Data = Commons.Resources.ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.MSG_VAID_URL) };
                }
                //Chinh 12-9-2014 end
                Task tsk_ret = response_task.ContinueWith(res_task =>
                {
                    Response = res_task.Result;
                }
                    , TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously);
                response_task.Dispose();
                response_task = null;

			}
			catch (Exception ex)
			{
				return new MessageResult {ErrorID = Commons.ERROR_CODE.HTTP_CLIENT_EXCEPTION, Data = ex.Message};
			}

            if (Response == null || Response.StatusCode != HttpStatusCode.OK )
				return new MessageResult { ErrorID = Commons.ERROR_CODE.SERVICE_EXCEPTION, Data = Commons.Resources.ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.SERVICE_EXCEPTION) };

            //Chinh 18-9-2014
            MessageResult resultMsg = ReadResponseContent<MessageResult>(Response, Formatter, canceltoken.Token);
            if (resultMsg==null)
                return new MessageResult { ErrorID = Commons.ERROR_CODE.SERVICE_EXCEPTION, Data = Commons.Resources.ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.SERVICE_EXCEPTION) };
            
			if (canceltoken.IsCancellationRequested)
				return new MessageResult { ErrorID = Commons.ERROR_CODE.CONVERTER_CANCELREQUEST };

            return resultMsg;
		}

		private MessageDVRInfo GetMsgDVRInfo()
		{
			return new MessageDVRInfo
			{
				HASPK = SPKID,
				MACs = MacAddressInfos.Select(item => new ConvertMessage.MacInfo { IP_Address = item.IpAddress, IP_Version = item.Version, MAC_Address = item.MACAddress, Active = item.Active, MacOrder = item.MacOrder }).ToList(),
				Date = DateTime.Now,
				HostName = HostName,
				PACinfo = null 
			};
		}

		private string HostName
		{
			get
			{
				try
				{
					return Dns.GetHostName(); 
				}
				catch
				{
					return null;
				}
			}
		}

		List<MacAddressInfo> _MacAddressInfos = null;
		private List<MacAddressInfo> MacAddressInfos
		{
			get
			{
				if (_MacAddressInfos == null || _MacAddressInfos.Count == 0)
					_MacAddressInfos = GetNetworkInfo();
				return _MacAddressInfos;
			}
		}

		private List<MacAddressInfo> GetNetworkInfo()
		{
			List<MacAddressInfo> lst_Addressinfo = new List<MacAddressInfo>();
			NetworkInterface[] Nics = NetworkInterface.GetAllNetworkInterfaces();
			List<MacAddressInfo> nicInfos = null;
			int index = 0;
			MacAddressInfo active_ip = null;
			foreach (NetworkInterface Nic in Nics)
			{
				//if (Nic.OperationalStatus != OperationalStatus.Up)
				//    continue;
				if (Nic.NetworkInterfaceType == NetworkInterfaceType.Unknown)
					continue;
				if (Nic.NetworkInterfaceType == NetworkInterfaceType.Loopback)
					continue;
				active_ip = lst_Addressinfo.FirstOrDefault(item => item.Active && string.Compare(item.Version, AddressFamily.InterNetwork.ToString(), true) == 0);
				nicInfos = GetNetworkInfo(Nic, index++, active_ip == null);
				if (nicInfos == null || nicInfos.Count == 0)
					continue;
				lst_Addressinfo.AddRange(nicInfos);
			}
			return lst_Addressinfo;
		}

		private List<MacAddressInfo> GetNetworkInfo(NetworkInterface NetInterface, int index, bool isactive)
		{
			if (NetInterface == null)
				return null;
			List<MacAddressInfo> ret = new List<MacAddressInfo>();
			IPInterfaceProperties ipProps = NetInterface.GetIPProperties();
			string mac_add = string.Empty;
			foreach (var ip in ipProps.UnicastAddresses)
			{
				//if ((NetInterface.OperationalStatus == OperationalStatus.Up)
				//    && (ip.Address.AddressFamily == AddressFamily.InterNetwork || ip.Address.AddressFamily == AddressFamily.InterNetworkV6))
				//{
				if (string.IsNullOrEmpty(mac_add))
					mac_add = GetPhysicalAddress(NetInterface.GetPhysicalAddress());
				ret.Add(new MacAddressInfo
				{
					MACAddress = mac_add,
					IpAddress = ip.Address.ToString(),
					LoopBack = NetInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback,
					Version = ip.Address.AddressFamily.ToString(),
					Active = isactive,
					MacOrder = index + 1//because index from 0

				});
				//}
			}
			return ret;
		}

		private string GetPhysicalAddress(PhysicalAddress physAddress, char separate = ':')
		{
			byte[] bytes = physAddress.GetAddressBytes();
			string ret = string.Empty;
			for (int i = 0; i < bytes.Length; i++)
			{
				// Display the physical address in hexadecimal.
				ret += bytes[i].ToString(HEXA_FORMAT);
				if (i != bytes.Length - 1)
				{
					ret += separate.ToString();
				}
			}
			return ret;
		}

		internal class MacAddressInfo
		{
			public string MACAddress { get; set; }
			public string IpAddress { get; set; }
			public bool LoopBack { get; set; }
			public string Version { get; set; }
			public int MacOrder { get; set; }
			public bool Active { get; set; }

		}

		private string _SPKID;
		private string SPKID
		{
			get
			{
				if (string.IsNullOrEmpty(_SPKID))
					_SPKID = GetSPKID();

				return _SPKID;
			}
		}
		private string GetSPKID()
		{
			//HASPkey.HASPkey haspKey = new HASPkey.HASPkey();
			//Aladdin.HASP.HaspStatus status = haspKey.haspGetHaspId();

			//return status == Aladdin.HASP.HaspStatus.StatusOk ? haspKey.HaspID : string.Empty;
			return string.Empty;
		}
	}
}

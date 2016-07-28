using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Commons;
using ConverterDB;
using ConvertMessage;
using PACDMConverter.Events;
using System.IO;
using System.Net;
using System.Linq.Expressions;

namespace PACDMConverter
{
	internal class MonitorTask:IDisposable
	{
#if DEBUG
		const int Event_Time_Out = 5;// 1 min
		const int Event_Time_10 = 10;
		const int Event_Time_5 = 1;
		const int Event_Time_Wait_Domain = 5;
#else
		const int Event_Time_Out = 15;// 1 min
		const int Event_Time_10 = 10;
		const int Event_Time_5 = 1;
		const int Event_Time_Wait_Domain = 15;
#endif
		public delegate void HttpLoginEvent(object sender, Events.LoginEventArgs eventargs);
		public delegate void HttpKeepAlive(object sender, Events.LoginEventArgs eventargs);
		public delegate void RecycleLog(object sender, int numofdate);
		public delegate void UpgradeEvent( object sender, string path, string version);
		public delegate void AskDomainResponse( object sender, DomainResponse response);
		public delegate void AskDomainResponseDVR( object sender, string sid);
		public delegate void DvrCommuniationportChange(object sender, UInt16 newvalue);
		public delegate void StopConverter( object sender, bool stopPACConverter, bool StopDvr);
		
		public event HttpLoginEvent OnHttpLoginEvent;
		public event HttpKeepAlive OnHttpKeepAlive;
		public event RecycleLog OnRecycleLog;
		public event UpgradeEvent OnUpgradeEvent;
		public event delegateLog OnlogMessage;
		public event AskDomainResponse OnAskDomainResponse;
		public event AskDomainResponseDVR OnAskDomainResponseDVR; 
		public event DvrCommuniationportChange OnDvrCommuniationportChange;
		public event StopConverter OnStopConverter;
		//public event AskDomainStatusChange OnAskDomainStatusChange;
		WaitHandle[] handles;
		Task _Task;


		public void Dispose()
		{
			if (_Task == null)
				return;
			_Task.Wait();

		}
		
		public void TriggerTokenExpired()
		{
			if( handles == null || handles.Length < (int)Enums.EventMonitor.EVent_InvalidToken)
				return;
			EventWaitHandle evt = handles[(int)Enums.EventMonitor.EVent_InvalidToken]	as EventWaitHandle;
			if( evt == null)
				return;
			evt.Set();
		}
		public void StartMonitor(ConverterDB.Model.ServiceConfig ServiceConfig, CancellationToken CancelToken, SRXProInfos dvrinfo, string requestDomainUrl, string sid, UInt16 DVRPort)
		{
			handles = new WaitHandle[(int)Enums.EventMonitor.Event_Count + 1];

			InitEvent((int)Enums.EventMonitor.Event_Count, ref handles);

			handles[handles.Length - 1] = CancelToken.WaitHandle;

			_Task = Task.Factory.StartNew(() => MonitorTaskProc(ServiceConfig, handles, CancelToken,dvrinfo, requestDomainUrl, sid, DVRPort), TaskCreationOptions.LongRunning); 
		}

		void InitEvent(int count,ref WaitHandle[] events)
		{
			if (count <= 0)
				return;
			if( events == null)
				events = new AutoResetEvent[count];

			int num = Math.Min(events.Length, count);
			//AutoResetEvent[] events = new AutoResetEvent[count];
			for (int i = 0; i < num; i++)
				events[i] = new AutoResetEvent(false);
		}

		private bool isWeb(string url)
		{
			return UriExtension.isProtocol(url, Uri.UriSchemeHttp, Uri.UriSchemeHttps);
		}

		private Uri CreateUri(string url)
		{
			if( string .IsNullOrEmpty(url))
				return null;
			if( !url.StartsWith( Uri.UriSchemeHttp, StringComparison.InvariantCultureIgnoreCase ) && !url.StartsWith( Uri.UriSchemeHttps, StringComparison.InvariantCultureIgnoreCase ) )
				url = Uri.UriSchemeHttp + Uri.SchemeDelimiter + url;
			try
			{
				Uri ret;
				Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out ret);
				return ret;
			}catch(Exception){ return null;}
		}

		private bool isChangeDomain( string strurl1, string strurl2)
		{

			Uri uri1 = CreateUri(strurl1);
			Uri uri2 = CreateUri(strurl2);
			if (uri1 == null || uri2 == null)
				return false;

			if (uri1.Port != uri2.Port)
				return true;
			return string.Compare(uri1.AbsoluteUri, uri2.AbsoluteUri, true) != 0;
		}
		
		private bool IsAskDomain( string inputdomain, string askdomain)
		{
			if( string.IsNullOrEmpty(inputdomain) || string.IsNullOrEmpty(askdomain))
				return false;
			if (!inputdomain.StartsWith(Uri.UriSchemeHttp) && !inputdomain.StartsWith(Uri.UriSchemeHttps))
				inputdomain = Uri.UriSchemeHttp + Uri.SchemeDelimiter + inputdomain;

			Uri i_url;
			Uri.TryCreate(inputdomain, UriKind.RelativeOrAbsolute, out i_url);

			Uri a_url;// = new Uri(askdomain);
			if (!askdomain.StartsWith(Uri.UriSchemeHttp) && !askdomain.StartsWith(Uri.UriSchemeHttps))
				askdomain = Uri.UriSchemeHttp + Uri.SchemeDelimiter + askdomain;
			Uri.TryCreate(askdomain, UriKind.RelativeOrAbsolute, out a_url);
			return string.Compare(i_url.Host, a_url.Host, true) == 0 && i_url.Port == a_url.Port;
			
		}

		private void TriggerDVRCommunicationPortChange(UInt16 newvalue)
		{
			if(OnDvrCommuniationportChange != null )
				OnDvrCommuniationportChange(this, newvalue);
		}

		private void TriggerDomainChange( DomainResponse response)
		{
			if( response == null || OnAskDomainResponse == null)
				return;
			OnAskDomainResponse(this, response); 
		}

		private void TriggerStopConverter( bool StopPacConverter, bool StopDVRCovnerter)
		{
			if( OnStopConverter != null)
				OnStopConverter(this, StopPacConverter, StopDVRCovnerter);
		}

		private void SetEvents(WaitHandle Event, bool reset)
		{
			EventWaitHandle evt = Event as EventWaitHandle;
			if (evt == null )
				return;
			if(reset )
				evt.Reset();
			else
				evt.Set();

		}

		private void SetEvents(WaitHandle[] Events, Func<Enums.EventMonitor, bool> filter, bool reset)
		{
			IEnumerable<Enums.EventMonitor> allids = Enum.GetValues(typeof(Enums.EventMonitor)).Cast<Enums.EventMonitor>().Where(it => it < Enums.EventMonitor.Event_Count);
			var match = allids.Where(filter);
			if(!match.Any())
				return;
			SetEvents(Events, reset, match.ToArray());

		}

		private void SetEvents(WaitHandle[] Events, bool reset,params Enums.EventMonitor[] ids)
		{
			if( ids == null || ids.Length ==0 || Events == null || Events.Length == 0)
				return;
			if(ids.Length == 1 && ids[0] == Enums.EventMonitor.Event_Count)
			{
				for(int i =0 ;i < (int)ids[0]; i++)
					SetEvents(Events[i], reset);

				return;
			}
			foreach (Enums.EventMonitor id in ids)
				if ((int)id >= 0 && (int)id < Events.Length)
					SetEvents(Events[(int)id], reset);

			
		}

		private void MonitorTaskProc(ConverterDB.Model.ServiceConfig ServiceConfig, WaitHandle[] handles, CancellationToken CancelToken, SRXProInfos dvrinfo, string default_domain, string domainsid, UInt16 DVRPort)
		{
			bool isLogin = false;

			MediaTypeFormatter MediaFormat = new JsonMediaTypeFormatter();

			TimeSpan timeout_event = TimeSpan.Zero;
			
			timeout_event = new TimeSpan(0, 0, 0, Event_Time_Out);

			//WaitHandle[] handles = new WaitHandle[(int)Enums.EventMonitor.Event_Count + 1];
			
			//InitEvent((int)Enums.EventMonitor.Event_Count, ref handles);

			//handles[handles.Length -1] = CancelToken.WaitHandle;

			int event_index = -1;

			
			//recycle log
			TimeSpan Recycle_TimeOut = TimeSpan.Zero;
			//keepalive to API
			TimeSpan KeepAlive_Time = TimeSpan.Zero;
			//Keepalive to Domain
			TimeSpan KeepAliveDomain_Time = TimeSpan.Zero;

			MessageKeepAlive KeepAlive = null;

			CMSAgreement Agreement = null;

			ApiService httpclient = null;

			AskDomainClient httpaskDomain = null;

			bool agreement_enable = false;

			Customer Customer_Agreement = null;

			agreement_enable = dvrinfo != null && !string.IsNullOrEmpty(dvrinfo.ServerAppPath) && !string.IsNullOrEmpty(default_domain) && dvrinfo.Version != null && dvrinfo.Version.CompareTo(Utils.Instance.Min_SRXPRo_Agreement_Version) >= 0;
			bool first_init = true;
			httpaskDomain = new AskDomainClient(default_domain);
			bool can_keep_alive = string.IsNullOrEmpty(domainsid) == false;
			bool need_tech_api = false;
			bool server_response = false;
			if (agreement_enable)
			{
				Agreement = new CMSAgreement(dvrinfo.ConfigurationPath);
				httpaskDomain = new AskDomainClient(default_domain);
				Agreement.OnFileChangeEvent += delegate(Customer oldinfo, Customer newinfo, WatcherChangeTypes changetype)
				{
					//trigger event when user change Domain
					if (string.Compare(oldinfo.Domain, newinfo.Domain, true) != 0 || oldinfo.ConverterPort != newinfo.ConverterPort || oldinfo.AllowConnect != newinfo.AllowConnect)
					{
						SetEvents(handles, false, Enums.EventMonitor.Event_DomainChange);
					}
					else
						SetEvents(handles, false, Enums.EventMonitor.Event_DVRInfoChange);
						//(handles[(int)Enums.EventMonitor.Event_DomainChange] as AutoResetEvent).Set();
				};
				
				Customer_Agreement = Agreement.Info;
				if (Customer_Agreement.ConverterPort != DVRPort && OnDvrCommuniationportChange != null)
				{
					//OnDvrCommuniationportChange(this, Customer_Agreement.ConverterPort, KeepAlive, httpclient == null ? null : httpclient.TokenID, Customer_Agreement.AllowConnect);
					TriggerDVRCommunicationPortChange(Customer_Agreement.ConverterPort);
				}
				Agreement.InitWatcher();
				SetEvents(handles, false, Enums.EventMonitor.Event_DomainChange);
			}
			else
			{
				//(handles[(int)Enums.EventMonitor.Event_Login] as AutoResetEvent).Set();
				SetEvents(handles, false, Enums.EventMonitor.Event_Login);
			}
			ERROR_CODE Last_Errror = ERROR_CODE.OK;
			//(handles[(int)Enums.EventMonitor.Event_Login] as AutoResetEvent).Set();
			
			DomainResponse LastDomainResponse = null;
			MessageResult msgResult = null;
			MessageResult request_domain_result = null;
			string upgradePath = Path.Combine(Utils.Instance.StartupDir, Consts.STR_UPGRADE);
			bool exit = false;
			int version_comapre = 0;
			TimeSpan Domain_KeepAlive_Timeout = new TimeSpan(0,0, Event_Time_Wait_Domain);
			while (!CancelToken.IsCancellationRequested && !exit)
			{
				event_index = WaitHandle.WaitAny(handles, timeout_event);
					
				switch (event_index)
				{
					case (int)Enums.EventMonitor.Event_DomainLocked:
						#region Tech Lock DVR
						isLogin = false;
						KeepAlive = null;
						//TriggerDomainChange(new DomainResponse { isAccept = true, Url = ServiceConfig.Url }, true);
						TriggerStopConverter( true, true);
						SetEvents(handles, it =>it != Enums.EventMonitor.Event_Exit, true);
					#endregion
					break;
					case(int) Enums.EventMonitor.Event_DVRInfoChange:
						#region DVR info change
							if( need_tech_api)
								(handles[(int)Enums.EventMonitor.Event_DomainRequest] as AutoResetEvent).Set();
						#endregion
					break;
					case (int)Enums.EventMonitor.Event_DomainChange://domain change form pro input
						#region Domain change by: tech/User input/Init
						bool _init = first_init;
						first_init = false;
						if (!agreement_enable || !Agreement.Info.AllowConnect)
						{
							TriggerStopConverter(false, true);
							break;
						}

						bool orther_change = Customer_Agreement == null? false : (string.Compare(Customer_Agreement.Email, Agreement.Info.Email, true) != 0 || string.Compare(Customer_Agreement.Name, Agreement.Info.Name, false) != 0);
						Customer_Agreement = Agreement.Info;
						bool dvr_change_port = Agreement != null && Customer_Agreement != null && Customer_Agreement.ConverterPort != DVRPort;

						if(!Customer_Agreement.AllowConnect)
						{
							TriggerStopConverter(true, true);
							break;
						}

						if( dvr_change_port && OnDvrCommuniationportChange != null || _init)
						{
							//OnDvrCommuniationportChange(this, Customer_Agreement.ConverterPort, KeepAlive, httpclient == null? null : httpclient.TokenID, Customer_Agreement.AllowConnect);
							TriggerDVRCommunicationPortChange(Customer_Agreement.ConverterPort);
							DVRPort = Customer_Agreement.ConverterPort;
						}

						if( string.IsNullOrEmpty(Customer_Agreement.Domain))
							break;

						SetEvents(handles, it => it != Enums.EventMonitor.Event_Exit, true);
						bool _tech_domain = isWeb(Customer_Agreement.Domain) && isChangeDomain(Customer_Agreement.Domain, default_domain) == false;
						if (_tech_domain)
						{
							
							//if (KeepAlive != null || isLogin)
							//{
							//    TriggerStopConverter(true, true);
							//}
							Domain_KeepAlive_Timeout = new TimeSpan(0, 0, Event_Time_Wait_Domain);
							KeepAlive = null;
							isLogin = false;
							TriggerStopConverter(true, Customer_Agreement.AllowConnect? false : true);
							need_tech_api = true;
							SetEvents(handles, false, Enums.EventMonitor.Event_DomainRequest);
							TriggerDomainChange(new DomainResponse { isAccept = true, Url = string.IsNullOrEmpty(Customer_Agreement.Domain) ? string.Empty : CreateUri(Customer_Agreement.Domain).ToString() });
							//trigger to start DVR socket

						}
						else
						{
							bool _webrequest = isWeb(Customer_Agreement.Domain);
							if(!_webrequest)
								break;
							TriggerDomainChange(new DomainResponse { isAccept = true, Url = CreateUri(Customer_Agreement.Domain).ToString() });
							isLogin = false;
							KeepAlive = null;
							Domain_KeepAlive_Timeout = new TimeSpan(0, Event_Time_5, 0);
							//TriggerStopConverter(true, true);
							if( _init)
							{
								TriggerStopConverter(true, Customer_Agreement.AllowConnect ? false : true);
								if (can_keep_alive)//need to send info to tech api 
								{
									(handles[(int)Enums.EventMonitor.Event_DomainRequest] as AutoResetEvent).Set();
								}
								else
								{
									if (isChangeDomain(Customer_Agreement.Domain, ServiceConfig.Url))
										TriggerDomainChange(new DomainResponse { isAccept = true, Url = CreateUri(Customer_Agreement.Domain).ToString() });

									(handles[(int)Enums.EventMonitor.Event_Login] as AutoResetEvent).Set();

								}
								need_tech_api = string.IsNullOrEmpty(domainsid) == false || isChangeDomain(ServiceConfig.Url, default_domain) == false;
							}
							else
							{
								if (Customer_Agreement.AllowConnect)
									TriggerStopConverter(true, Customer_Agreement.AllowConnect? false : true);

								if (!server_response)
								{
									if( _tech_domain)
										(handles[(int)Enums.EventMonitor.Event_DomainRequest] as AutoResetEvent).Set();
									else
									{
										//if( Customer_Agreement.AllowConnect)
										//    TriggerStopConverter(true, false);

										if (httpaskDomain != null && string.IsNullOrEmpty(domainsid) == false)
										{
											GuiUpdateDomain(httpaskDomain, CancelToken, domainsid);
											if(orther_change)
												(handles[(int)Enums.EventMonitor.Event_DomainRequest] as AutoResetEvent).Set();

										}

										(handles[(int)Enums.EventMonitor.Event_Login] as AutoResetEvent).Set();
									}
								}
								else
								{
									//if (Customer_Agreement.AllowConnect)
									//    TriggerStopConverter(true, false);

									(handles[(int)Enums.EventMonitor.Event_Login] as AutoResetEvent).Set();
								}
							}

						}
						server_response = false;
						#endregion
					break;

					case (int)Enums.EventMonitor.Event_DomainRequest:
						#region
								if (string.IsNullOrEmpty(Agreement.Info.Domain) || Agreement.Info.AllowConnect == false)
									break;
								SetEvents(handles, it => it != Enums.EventMonitor.Event_Exit, true);
								KeepAliveDomain_Time = TimeSpan.Zero;
								request_domain_result = AskDommain(Agreement.Info, httpaskDomain, CancelToken);
								if (request_domain_result != null && request_domain_result.ErrorID == ERROR_CODE.OK)
								{
									if( string.Compare(domainsid, request_domain_result.Data, false) != 0 && OnAskDomainResponseDVR != null)
										OnAskDomainResponseDVR( this, request_domain_result.Data);
									if( string.Compare( domainsid, request_domain_result.Data, false) != 0)
									{
										domainsid = request_domain_result.Data;
										can_keep_alive = string.IsNullOrEmpty(domainsid) == false;
									}

									(handles[(int)Enums.EventMonitor.Event_DomainKeepAlive] as AutoResetEvent).Set();
								}
								else
								{
									TriggerLogMessage(Commons.Resources.ResourceManagers.Instance.GetResourceString(request_domain_result == null ? ERROR_CODE.SERVICE_EXCEPTION : request_domain_result.ErrorID), request_domain_result == null? ERROR_CODE.SERVICE_EXCEPTION: request_domain_result.ErrorID);
								}

						break;
						#endregion
					case (int)Enums.EventMonitor.Event_DomainKeepAlive:
						#region
								KeepAliveDomain_Time = TimeSpan.Zero;
								if( Agreement.Info.AllowConnect == false)
									break;

								if( string.IsNullOrEmpty( domainsid))
								{
									SetEvents(handles, false, Enums.EventMonitor.Event_DomainRequest);
									break;
								}
								request_domain_result = AskDomainKeepalive(httpaskDomain, domainsid, CancelToken);
								if (request_domain_result == null )
								{
									TriggerLogMessage(Commons.Resources.ResourceManagers.Instance.GetResourceString(ERROR_CODE.SERVICE_EXCEPTION) , ERROR_CODE.SERVICE_EXCEPTION);
									break;
								}

								DomainResponse domainresponse = null;
								if (request_domain_result.ErrorID != ERROR_CODE.OK)
								{
									TriggerLogMessage(Commons.Resources.ResourceManagers.Instance.GetResourceString(request_domain_result.ErrorID), request_domain_result.ErrorID);

									domainresponse = Commons.ObjectUtils.DeSerialize(httpaskDomain.DataFormat, typeof(DomainResponse), request_domain_result.Data) as DomainResponse;

									if (request_domain_result.ErrorID == ERROR_CODE.DB_QUERY_NODATA || request_domain_result.ErrorID == ERROR_CODE.DB_INVALID_DVR_GUI)
									{
										SetEvents(handles, it => it != Enums.EventMonitor.Event_Exit, true);// cancel all pending events
										SetEvents(handles, false, Enums.EventMonitor.Event_DomainRequest);
										break;
									}
									if (request_domain_result.ErrorID == ERROR_CODE.DVR_LOCKED_BY_ADMIN)
									{
										LastDomainResponse = new DomainResponse { Url = domainresponse.Url, isAccept = domainresponse.isAccept, TechChange = domainresponse.TechChange, UIChange = domainresponse.UIChange };
										SetEvents(handles, it => it != Enums.EventMonitor.Event_Exit, true);// cancel all pending events
										SetEvents(handles, false, Enums.EventMonitor.Event_DomainLocked);
										break;
									}
									if(request_domain_result.ErrorID == ERROR_CODE.DVR_REGISTER_PENDING )
									{
										Domain_KeepAlive_Timeout = new TimeSpan(0, 0, Event_Time_Wait_Domain);
										if (isWeb(Customer_Agreement.Domain) && isChangeDomain(Customer_Agreement.Domain, default_domain) == true && isLogin == false && KeepAlive == null)
										{
											(handles[(int)Enums.EventMonitor.Event_Login] as AutoResetEvent).Set();
										}
										else// check to open socket
										{
											
											if( domainresponse == null)
												break;

											if (LastDomainResponse == null)
												LastDomainResponse = new DomainResponse { Url = domainresponse.Url, isAccept = domainresponse.isAccept, TechChange = domainresponse.TechChange, UIChange = domainresponse.UIChange };

											if( Customer_Agreement != null && Customer_Agreement.AllowConnect && domainresponse != null && domainresponse.isAccept == true)
											{
												if( LastDomainResponse.isAccept == false)
												{
													server_response = true;
													SetEvents(handles, false, Enums.EventMonitor.Event_DomainChange);
												}
											}

											LastDomainResponse = new DomainResponse { Url = domainresponse.Url, isAccept = domainresponse.isAccept, TechChange = domainresponse.TechChange, UIChange = domainresponse.UIChange };
										}

									}
									break;
								}

								domainresponse = Commons.ObjectUtils.DeSerialize( httpaskDomain.DataFormat, typeof(DomainResponse), request_domain_result.Data) as DomainResponse;
								if (domainresponse == null)
								{
									if (isWeb(Customer_Agreement.Domain) && isChangeDomain(Customer_Agreement.Domain, default_domain) == true && !isLogin && KeepAlive == null)
										(handles[(int)Enums.EventMonitor.Event_Login] as AutoResetEvent).Set();
									break;
								}

								if (string.IsNullOrEmpty(domainresponse.Url) && domainresponse.UIChange > domainresponse.TechChange && isWeb(Customer_Agreement.Domain) && isChangeDomain(Customer_Agreement.Domain, default_domain) == false)
								{
									if (!isLogin && KeepAlive == null)
										(handles[(int)Enums.EventMonitor.Event_Login] as AutoResetEvent).Set();

									break;
								}

								if( LastDomainResponse == null)
								{
									LastDomainResponse = new DomainResponse { Url = domainresponse.Url, isAccept = domainresponse.isAccept, TechChange = domainresponse.TechChange, UIChange = domainresponse.UIChange};
									if( isChangeDomain(Customer_Agreement.Domain, default_domain) == false
										|| isChangeDomain(Customer_Agreement.Domain, LastDomainResponse.Url) && LastDomainResponse.UIChange < LastDomainResponse.TechChange)//only reset doain when tech change after user setting
										{
											server_response = true;
											Agreement.ChangeDomain(LastDomainResponse.Url);

										}
									else
										if (!isLogin && KeepAlive == null)
											(handles[(int)Enums.EventMonitor.Event_Login] as AutoResetEvent).Set();
									break;
								}
								else
								{
									if(LastDomainResponse.isAccept == true && domainresponse.isAccept == false)
									{
										//(handles[(int)Enums.EventMonitor.Event_DomainLocked] as AutoResetEvent).Set();
										SetEvents(handles, it => it != Enums.EventMonitor.Event_Exit, true);// cancel all pending events
										//(handles[(int)Enums.EventMonitor.Event_Login] as AutoResetEvent).Reset();
										SetEvents(handles, false, Enums.EventMonitor.Event_DomainLocked);
									}
									else if(LastDomainResponse.isAccept == false && domainresponse.isAccept == true)
									{
										//(handles[(int)Enums.EventMonitor.Event_Login] as AutoResetEvent).Reset();
										SetEvents(handles, it => it != Enums.EventMonitor.Event_Exit, true);
										SetEvents(handles, false, Enums.EventMonitor.Event_DomainChange);
										//(handles[(int)Enums.EventMonitor.Event_DomainChange] as AutoResetEvent).Set();
									}

									
									LastDomainResponse = new DomainResponse { Url = domainresponse.Url, isAccept = domainresponse.isAccept, TechChange = domainresponse.TechChange, UIChange = domainresponse.UIChange };
									bool is_change = isChangeDomain(Customer_Agreement.Domain , LastDomainResponse.Url);
									if (isChangeDomain(Customer_Agreement.Domain, default_domain) == false || 
										is_change && LastDomainResponse.UIChange < LastDomainResponse.TechChange)
									{
										//(handles[(int)Enums.EventMonitor.Event_Login] as AutoResetEvent).Reset();
										server_response = true;
										Agreement.ChangeDomain(LastDomainResponse.Url);
										
									}
									if(!isLogin && KeepAlive == null)
										(handles[(int)Enums.EventMonitor.Event_Login] as AutoResetEvent).Set();
								}
						break;
						#endregion
					case (int)Enums.EventMonitor.Event_Login:

						#region
						KeepAlive = null;
						isLogin = false;
						if( string.IsNullOrEmpty(ServiceConfig.Url))
							break;
						httpclient = new ApiService(ServiceConfig);
						httpclient.TimeOutRequest = Consts.Default_Http_TimeOut;

						MessageKeepAlive Login_KeepAlive = Login(httpclient, CancelToken, MediaFormat, out Last_Errror);
						TriggerLogMessage(string.Format(Commons.Resources.ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.CONVERTER_LOGIN), ServiceConfig.Url), ERROR_CODE.OK);
						if( Last_Errror != ERROR_CODE.OK || Login_KeepAlive == null)
						{
							TriggerLogMessage(Commons.Resources.ResourceManagers.Instance.GetResourceString(Last_Errror), Last_Errror);
							break;
						}

						KeepAlive = Login_KeepAlive;

						TriggerLogMessage(Commons.Resources.ResourceManagers.Instance.GetResourceString(Last_Errror), Last_Errror);
						version_comapre = isNewVersion(KeepAlive.LastVersion);
						if(version_comapre <= 0)//version on api <= current
						{
							LoginEvent(KeepAlive, Last_Errror, httpclient.TokenID);
							isLogin = true;
						}

						else
						{
							//if( version_comapre > 0)
							(handles[(int)Enums.EventMonitor.Event_GetLastVersion] as AutoResetEvent).Set();
							//else
							//{
							//    TriggerLogMessage(string.Format(Commons.Resources.ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.MSG_CONVERTER_VERSION_HIGHER_THAN_SERVER), Utils.Instance.Version.ToString(), KeepAlive.LastVersion), Commons.ERROR_CODE.MSG_CONVERTER_VERSION_HIGHER_THAN_SERVER);
							//}
						}
						#endregion
						break;

					case (int)Enums.EventMonitor.EVent_InvalidToken:
						#region
						{
							if( isLogin == false && KeepAlive == null)
								break;

							TriggerLogMessage(Commons.Resources.ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.CONVERTER_INVALID_TOKEN), Commons.ERROR_CODE.SERVICE_TOKEN_INVALID);
							isLogin = false;
							KeepAlive = null;
							if(!agreement_enable)
								(handles[(int)Enums.EventMonitor.Event_Login] as AutoResetEvent).Set();
							else
								(handles[(int)Enums.EventMonitor.Event_DomainChange] as AutoResetEvent).Set();
							break;
					}
					#endregion
					case (int)Enums.EventMonitor.Event_KeepAlive:
						#region
							KeepAlive_Time = TimeSpan.Zero;

							MessageKeepAlive KAlive = SendKeepAlive(KeepAlive.KeepAliveToken, httpclient, CancelToken, MediaFormat, out Last_Errror);
							if( KAlive == null)
							{
								TriggerLogMessage( Commons.Resources.ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.CONVERTER_KEEPALIVE), Last_Errror );
								break;
							}

							if( Last_Errror == ERROR_CODE.SERVICE_TOKEN_EXPIRED || Last_Errror == ERROR_CODE.SERVICE_TOKEN_INVALID || Last_Errror == ERROR_CODE.DVR_LOCKED_BY_ADMIN)
							{
								TriggerLogMessage(Commons.Resources.ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.DVR_LOCKED_BY_ADMIN), Commons.ERROR_CODE.DVR_LOCKED_BY_ADMIN);
								(handles[(int)Enums.EventMonitor.EVent_InvalidToken] as AutoResetEvent).Set();
								break;
							}

							if(!KAlive.Equals(KeepAlive) )
							{
								UpdateNewKeepAlive( KeepAlive, KAlive);
								version_comapre = isNewVersion(KeepAlive.LastVersion);
								if (version_comapre <= 0)
								{
#if KEEPALIVE
									KeepAliveEvent(KeepAlive, Last_Errror, httpclient.TokenID);
#endif
								}
								else
								{
									//if( version_comapre > 0)
									(handles[(int)Enums.EventMonitor.Event_GetLastVersion] as AutoResetEvent).Set();
									//else
									//{
									//    TriggerLogMessage(string.Format(Commons.Resources.ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.MSG_CONVERTER_VERSION_HIGHER_THAN_SERVER), Utils.Instance.Version.ToString(), KeepAlive.LastVersion), Commons.ERROR_CODE.MSG_CONVERTER_VERSION_HIGHER_THAN_SERVER);
									//}
								}
							}
#endregion
						break;
					case (int)Enums.EventMonitor.Event_GetLastVersion:
						#region
						if(!Utils.CreateDir(upgradePath))
						{
							TriggerLogMessage( string.Format( Commons.Resources.ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.CONVERTER_CREATE_FOLDER_FAILED) , upgradePath), Commons.ERROR_CODE.CONVERTER_CREATE_FOLDER_FAILED );
							break;
						}

						TriggerLogMessage(string.Format(Commons.Resources.ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.MSG_REQUEST_CONVERTER_VERSION), KeepAlive.LastVersion), Commons.ERROR_CODE.MSG_REQUEST_CONVERTER_VERSION);
							msgResult = httpclient.GetNewVesion(KeepAlive.LastVersion, CancelToken, upgradePath , true);
							if( msgResult != null && File.Exists( msgResult.Data))
							{
								Version v =	Commons.Utils.ParserVersion( KeepAlive.LastVersion, Commons.ConstEnums.Regex_VersionAny);
								TriggerUpgradeEvent(msgResult.Data, v== null? KeepAlive.LastVersion : v.ToString());
								(handles[(int)Enums.EventMonitor.Event_Exit] as AutoResetEvent).Set();
							}
							else
								TriggerLogMessage(Commons.Resources.ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.MSG_CONVERTER_VERSION_NOTFOUND), Commons.ERROR_CODE.MSG_CONVERTER_VERSION_NOTFOUND);
								//TriggerLogMessage(msgResult == null? string.Empty : msgResult.Data, Last_Errror);
						#endregion
						break;
					case (int)Enums.EventMonitor.Event_RecycleData:
						#region
							Recycle_TimeOut = TimeSpan.Zero;
							RecycleLogEvent(KeepAlive);
						#endregion
					break;
					case (int)Enums.EventMonitor.Event_Count:
					case (int)Enums.EventMonitor.Event_Exit:
							exit = true;
						break;
					default:
						{
							//Total_TimeOut.Add(timeout_event);
							if(!agreement_enable)
							{
								if (!isLogin || KeepAlive == null)
								{
									(handles[(int)Enums.EventMonitor.Event_Login] as AutoResetEvent).Set();
									break;
								}
								else
								{
									Recycle_TimeOut = Recycle_TimeOut.Add(timeout_event);
									KeepAlive_Time = KeepAlive_Time.Add(timeout_event);

									if (KeepAlive_Time.TotalMinutes >= KeepAlive.KeepAliveInterval)
											(handles[(int)Enums.EventMonitor.Event_KeepAlive] as AutoResetEvent).Set();
									
									if (Recycle_TimeOut.TotalMinutes >= Event_Time_10)
										(handles[(int)Enums.EventMonitor.Event_RecycleData] as AutoResetEvent).Set();
								}

							}
							else
							{
								Recycle_TimeOut = Recycle_TimeOut.Add(timeout_event);
								KeepAlive_Time = KeepAlive_Time.Add(timeout_event);
								KeepAliveDomain_Time = KeepAliveDomain_Time.Add(timeout_event);
								
								if (KeepAlive != null && KeepAlive_Time.TotalMinutes >= KeepAlive.KeepAliveInterval)
								{
									if (KeepAlive != null)
										(handles[(int)Enums.EventMonitor.Event_KeepAlive] as AutoResetEvent).Set();
									else
									{
										KeepAlive_Time = TimeSpan.Zero;
									}
										break;
								}
								
								if (Recycle_TimeOut.TotalMinutes >= Event_Time_10)
								{
									(handles[(int)Enums.EventMonitor.Event_RecycleData] as AutoResetEvent).Set();
									break;
								}

								if( Customer_Agreement != null && Customer_Agreement.AllowConnect == false)
									break;

								
								if (KeepAliveDomain_Time.TotalSeconds >= Domain_KeepAlive_Timeout.TotalSeconds)
								{
									if(!need_tech_api)
									{
										KeepAliveDomain_Time = TimeSpan.Zero;
										break;
									}

									if (can_keep_alive)
										(handles[(int)Enums.EventMonitor.Event_DomainKeepAlive] as AutoResetEvent).Set();
									else
										(handles[(int)Enums.EventMonitor.Event_DomainRequest] as AutoResetEvent).Set();
									break;
								}

								_tech_domain = Customer_Agreement != null && isWeb(Customer_Agreement.Domain) && isChangeDomain(Customer_Agreement.Domain, default_domain) == false;
								//if (!_tech_domain && isLogin == false && ( KeepAlive == null || version_comapre <= 0))
								if (!_tech_domain && isLogin == false && version_comapre <= 0)
								{
									(handles[(int)Enums.EventMonitor.Event_Login] as AutoResetEvent).Set();
								}

							}
							break;
						}
				}


			}
		}

		//private bool NeedRequestDomain( string webapi, string requestUrlDomain)
		//{
		//    if( string.IsNullOrEmpty(webapi))
		//        return true;
		//    if( string.IsNullOrEmpty(requestUrlDomain))
		//        return false;
		//    return string.Compare( webapi,);
		//}

		/// <summary>
		///0: version = current
		///1: version > current
		///-1: version < current
		/// </summary>
		/// <param name="version"></param>
		/// <returns></returns>
		private int isNewVersion( string version)
		{
			if( string.IsNullOrEmpty(version))
				return 0;

			Version v_cmp = Commons.Utils.ParserVersion( version, Commons.ConstEnums.Regex_VersionAny);
			Version v_cur = Utils.Instance.Version;
			return v_cmp.CompareTo(v_cur);
		}

		#region Events & delegates
		private void TriggerUpgradeEvent(string filepath, string vresion)
		{
			if (OnUpgradeEvent != null)
			{
				OnUpgradeEvent(this, filepath, vresion);
			}
		}

		private void UpdateNewKeepAlive(MessageKeepAlive oldval, MessageKeepAlive newval)
		{
			if (newval == null)
				return;
			if (oldval == null)
			{
				oldval = newval;
				return;
			}
			if (newval.KeepAliveInterval.HasValue)
				oldval.KeepAliveInterval = newval.KeepAliveInterval;
			if (newval.KeepAliveToken.HasValue)
				oldval.KeepAliveToken = newval.KeepAliveToken;
			if (newval.LogRecyle.HasValue)
				oldval.LogRecyle = newval.LogRecyle;
			if (newval.TimeOut.HasValue)
				oldval.TimeOut = newval.TimeOut;
			if (newval.ConverterInterval.HasValue)
				oldval.ConverterInterval = newval.ConverterInterval;
			if (newval.DataReset.HasValue)
				oldval.DataReset = newval.DataReset;
			if (newval.DVRConvert.HasValue)
				oldval.DVRConvert = newval.DVRConvert;
			if (newval.DVRMessageRecycle.HasValue)
				oldval.DVRMessageRecycle = newval.DVRMessageRecycle;
			oldval.LastVersion = newval.LastVersion;
			oldval.ConvertInfo = newval.ConvertInfo;
		}
	
		private void RecycleLogEvent(ConvertMessage.MessageKeepAlive KeepAlive)
		{
			if( OnRecycleLog != null)
			{
				int numofday = (KeepAlive != null && KeepAlive.LogRecyle.HasValue) ? KeepAlive.LogRecyle.Value : ConvertMessage.Consts.Default_LogRecyle;
				OnRecycleLog(this, numofday);
			}
		}
		
		private void KeepAliveEvent(MessageKeepAlive keepalive, ERROR_CODE error, string token)
		{
			if (OnHttpKeepAlive != null)
			{
				OnHttpKeepAlive(this, new LoginEventArgs{ KeepAlive = keepalive, Status = error, Token = token });
			}
		}

		private void LoginEvent(MessageKeepAlive keepalive, ERROR_CODE error, string token)
		{
			if( OnHttpLoginEvent != null)
				OnHttpLoginEvent( this, new LoginEventArgs{ Token = token, Status = error, KeepAlive = keepalive});
		}

		private void TriggerLogMessage(string msg, Commons.ERROR_CODE errID)
		{
			if (OnlogMessage != null)
				OnlogMessage(this, msg, errID);
		}
		#endregion

		#region Api 
		private MessageKeepAlive SendKeepAlive(Int64? token, ApiService httpclient, CancellationToken canceltoken, MediaTypeFormatter mediaformat, out Commons.ERROR_CODE Error)
		{
			Error = ERROR_CODE.OK;
			MessageResult msg_result = httpclient.KeepAlive( token.HasValue? token.Value : 0, canceltoken, mediaformat);
			if (msg_result == null || msg_result.ErrorID != ERROR_CODE.OK)
			{
				Error = msg_result == null? ERROR_CODE.HTTP_CLIENT_EXCEPTION : msg_result.ErrorID;
				return null;
			}
			Error = msg_result.ErrorID;
			MessageKeepAlive msg = Commons.ObjectUtils.DeSerialize<MessageKeepAlive>(mediaformat, msg_result.Data);
			return msg;
		}
		
		private ConvertMessage.MessageKeepAlive Login(ApiService httpclient, CancellationToken canceltoken, MediaTypeFormatter mediaformat, out ERROR_CODE Error)
		{
			Error = ERROR_CODE.OK;
			ConvertMessage.MessageResult msg_result = httpclient.Login(canceltoken, mediaformat);
			if (msg_result == null || msg_result.ErrorID != ERROR_CODE.OK)
			{
				Error = msg_result == null ? ERROR_CODE.HTTP_CLIENT_EXCEPTION : msg_result.ErrorID;
				return null;
			}
			Error = msg_result.ErrorID;
			if( string.IsNullOrEmpty( msg_result.Data))
			{
				return null;
			}
			return Commons.ObjectUtils.DeSerialize<ConvertMessage.MessageKeepAlive>( mediaformat, msg_result.Data);
		}
		#endregion

		#region Domain api
		private MessageResult GuiUpdateDomain(AskDomainClient http, CancellationToken canceltoken, string sid)
		{
			if( string.IsNullOrEmpty(sid))
				return null;
				return http.LocalDomainChange(sid, DateTime.Now, canceltoken);
		}
		private MessageResult AskDommain(Customer info, AskDomainClient http, CancellationToken canceltoken)
		{
			ConvertMessage.MessageAgreement agreement = new MessageAgreement()
			{
				Customer = info,
				DVR = DVRInfos.Instance.MsgDVRInfo
			};
			return http.RequestDomain( agreement, canceltoken);
		}
		
		private MessageResult AskDomainKeepalive(AskDomainClient http,  string sid, CancellationToken canceltoken)
		{
			return http.GetDomain( sid, canceltoken);
		}
		#endregion
		
	}
}

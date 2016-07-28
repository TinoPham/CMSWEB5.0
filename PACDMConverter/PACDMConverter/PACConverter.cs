using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Commons;
using ConverterDB;
using ConverterDB.Model;
using System.IO;

namespace PACDMConverter.PACDMConverter
{
	internal class PACConverter: IDisposable
	{
		public event Events.ApitokenExpired OnApiTokenExpired;
		enum EnumControl:int
		{
			EVT_EXIT = 0,
			EVT_CONVERT_DATA = 1
		}
		const int Delay_Convert_time_At_start = 1;//minute
#if DEBUG
		const int Time_Out_Event = 1000 * 5;
#else
		const int Time_Out_Event = 1000 * 60;
#endif
		Task ConvertTask;
		AutoResetEvent[] evts;
		
		//#if PAC_JSON
		//readonly MediaTypeFormatter dataFormatter = new JsonMediaTypeFormatter();
		//#else // PAC_XML or leave it empty
		//readonly MediaTypeFormatter dataFormatter = new XmlMediaTypeFormatter();
		//#endif

		//readonly CancellationTokenSource ConvertTaskTokenSource = new CancellationTokenSource();
		//readonly CancellationTokenSource ConvertTokenSource = new CancellationTokenSource();
		readonly CancellationToken CancelToken;
		readonly Dictionary<Programset,Type> MappingTypes = new Dictionary<Programset,Type>();
		readonly string TokenID;
		Dictionary<Programset, ConvertBase>ConvertMaps = new Dictionary<Programset,ConvertBase>();
		private ConvertDB LocalDatabse;
		
		public PACConverter( ConvertDB LocalDB, CancellationToken cancelToken, string tokenID)
		{
			LocalDatabse = LocalDB;
			InitMappingType();
			CancelToken = cancelToken;
			TokenID = tokenID;
		}
		
		public void Dispose()
		{
			CancelAllTask();
			KeyValuePair<Programset, ConvertBase> converter;
			while( ConvertMaps.Count > 0)
			{
				converter =  ConvertMaps.First();
				if( converter.Value != null)
					converter.Value.Dispose();
				ConvertMaps.Remove(converter.Key);
			}
		}

		public void StartConvertTask()
		{
			try
			{
				Array arrenum = Enum.GetValues(typeof(EnumControl));
				evts = new AutoResetEvent[arrenum.Length];
				for (int i = 0; i < evts.Length; i++)
					evts[i] = new AutoResetEvent(false);


				ConvertTask = Task.Factory.StartNew(() => ConvertTaskProc(CancelToken, LocalDatabse, TokenID, evts), TaskCreationOptions.LongRunning);
			}
			catch (Exception ex)
			{
				LocalDatabse.AddLog(new Log { LogID = (byte)Commons.ERROR_CODE.UNKNOWN, DVRDate = DateTime.Now, Message = ex.Message, Owner = true, ProgramSet = (byte)Commons.Programset.UnknownType });
			}
		}

		private void InitMappingType()
		{
			MappingTypes.Add(Programset.POS, typeof( ConverterPOS.ConverterPOS ));
			MappingTypes.Add(Programset.IOPC, typeof(ConverterIOPC.ConverterIOPC));
			MappingTypes.Add(Programset.ATM, typeof(ConverterATM.ConverterATM));
			MappingTypes.Add(Programset.CA, typeof(ConverterCA.ConverterCA));
            MappingTypes.Add(Programset.LPR, typeof(ConverterLPR.ConverterLPR));
		}

		private void ConvertProgramset(CancellationToken CancelToken, Programset pset, ApiService httpclient, ConvertDB localdatabase, ConvertBase.ConvertMode svtMode, MediaTypeFormatter formatter = null)
		{
			KeyValuePair<Programset, ConvertBase>ConverterItem = ConvertMaps.FirstOrDefault(iset => iset.Key == pset);
			ConvertBase converter = null;
			if( ConverterItem.Value == null)
			{
				KeyValuePair<Programset, Type> Mappingtype = MappingTypes.FirstOrDefault(iset => iset.Key == pset);
				if( Mappingtype.Value == null)
					return;

				converter = (ConvertBase)ObjectUtils.InitObject(Mappingtype.Value, new object[] { localdatabase, pset, httpclient, CancelToken, svtMode, formatter });
				converter.OnConvertDataEvent += converter_OnConvertDataEvent;
				ConvertMaps.Add(pset, converter);
			}
			//else
			//	converter = ConverterItem.Value;
			//if (converter == null)
			//	return;
			//converter.ConvertData();
			//if (converter.Done)
			//{
			//	ConvertMaps.Remove(pset);
			//	converter.Dispose();
			//	converter = null;
			//}
		}

		void converter_OnConvertDataEvent(object sender, Events.ConvertDBEventArgs args)
		{
			///throw new NotImplementedException();
		}


		private Commons.ERROR_CODE ConvertPACData(List<ConvertInfo> lstConvertinfo, CancellationToken CancelToken, ApiService httpclient, ConvertDB localdatabase, ConvertBase.ConvertMode svtMode, MediaTypeFormatter formatter = null)
		{
		
			var group = lstConvertinfo.GroupBy(item => item.Programset);
			foreach (var igroup in group)
			{
				ConvertProgramset(CancelToken, (Programset)igroup.Key, httpclient, localdatabase, svtMode, formatter);
			}

			List<Programset> Complete = new List<Programset>();
			bool stop = false;
			ConvertMessage.MessageResult msgresult = null;
			while (ConvertMaps != null && ConvertMaps.Count > 0)
			{
				if (CancelToken.IsCancellationRequested || stop == true)
					break;

				foreach (KeyValuePair<Programset, ConvertBase> ConverterItem in ConvertMaps)
				{
					if (CancelToken.IsCancellationRequested || stop == true)
						break;
					try
					{
						msgresult = ConverterItem.Value.ConvertData();
						if( msgresult.ErrorID == ERROR_CODE.SERVICE_TOKEN_INVALID || msgresult.ErrorID == ERROR_CODE.SERVICE_TOKEN_EXPIRED)
						{
							stop = true;
							break;
						}

						if (ConverterItem.Value.Done)
							Complete.Add(ConverterItem.Key);
					}
					catch (Exception ex)
					{
						Complete.Add(ConverterItem.Key);
						LocalDatabse.AddLog(new Log { LogID = (byte)Commons.ERROR_CODE.UNKNOWN, DVRDate = DateTime.Now, Message = ex.Message, Owner = true, ProgramSet = (byte)Commons.Programset.UnknownType });
					}
					
				}
				foreach (Programset pset in Complete)
				{
					ConvertMaps[pset].Dispose();
					ConvertMaps.Remove(pset);
				}

				Complete.Clear();
			}
			return msgresult == null? Commons.ERROR_CODE.OK : msgresult.ErrorID;
			//foreach (ConvertInfo convertinfo in lstConvertinfo)
			//{
			//	ConvertProgramset(CancelToken, (Programset)convertinfo.Programset, httpclient, localdatabase, formatter);
			//	if (CancelToken.IsCancellationRequested || convertinfo.Programset == lstConvertinfo[lstConvertinfo.Count - 1].Programset && MappingTypes.Count == 0)
			//		break;
			//}
		}
		
		private void CancelAllTask()
		{
			if (ConvertTask == null)
				return;
			try
			{
				if (ConvertTask != null)
				{
					evts[(int)EnumControl.EVT_EXIT].Set();
					ConvertTask.Wait();
					ConvertTask.Dispose();
					ConvertTask = null;
				}
			}
			catch (Exception ex)
			{
				LocalDatabse.AddLog(new Log { LogID = (byte)Commons.ERROR_CODE.UNKNOWN, DVRDate = DateTime.Now, Message = ex.Message, Owner = true, ProgramSet = (byte)Commons.Programset.UnknownType });
			}
		}

		private void ConvertTaskProc(CancellationToken CancelToken, ConvertDB database, string tokenid, AutoResetEvent[] Events)
		{
			Thread.CurrentThread.CurrentCulture = Utils.Instance.CultureInfo;
			Thread.CurrentThread.CurrentUICulture = Utils.Instance.CultureInfo;

			IEnumerable<ConvertInfo> Convertinfo = database.ConvertInfo.Where( cvt => cvt.Enable == true).OrderBy( item => item.Order);

			ApiService apiservice = new ApiService(LocalDatabse.ServiceConfig, tokenid);
			DateTime ConvertDate = DateTime.Now.AddMinutes(Delay_Convert_time_At_start);
			int event_index = -1;
			int time_out = Time_Out_Event;
			Commons.ERROR_CODE errorID = ERROR_CODE.OK;
			bool stop = false;
			while( !CancelToken.IsCancellationRequested && !stop)
			{
				event_index = WaitHandle.WaitAny(Events, time_out);
				switch( event_index)
				{
					case (int)EnumControl.EVT_CONVERT_DATA:
							errorID = ConvertPACData(Convertinfo.ToList(), CancelToken, apiservice, database, Utils.Instance.PACConvertMode, Utils.Instance.PACMediaFomatter);
							ConvertDate = ConvertDate.AddMinutes(LocalDatabse.ServiceConfig.Interval);
							if (errorID == ERROR_CODE.SERVICE_TOKEN_EXPIRED || errorID == ERROR_CODE.SERVICE_TOKEN_INVALID || errorID == ERROR_CODE.DVR_LOCKED_BY_ADMIN)
							{
								stop = true;
								TriggerTokenExpiredEvent(errorID);
							}
						break;
					case (int) EnumControl.EVT_EXIT:
								stop = true;
						break;
					default:
						if( DateTime.Now.Ticks > ConvertDate.Ticks)
						{
							Events[(int)EnumControl.EVT_CONVERT_DATA].Set();
						}
						break;
				}
				
			}
		}
		private void TriggerTokenExpiredEvent( Commons.ERROR_CODE Error)
		{
			if( OnApiTokenExpired != null)
				OnApiTokenExpired( this, Error);
		}
	}
}

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

namespace PACDMSimulator.PACDMConverter
{
	internal class PACConverter: IDisposable
	{
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

		
		#if PAC_JSON
		readonly MediaTypeFormatter dataFormatter = new JsonMediaTypeFormatter();
		#else // PAC_XML or leave it empty
		readonly MediaTypeFormatter dataFormatter = new XmlMediaTypeFormatter();
		#endif

		readonly CancellationTokenSource ConvertTaskTokenSource = new CancellationTokenSource();
		readonly CancellationTokenSource ConvertTokenSource = new CancellationTokenSource();

		readonly Dictionary<Programset,Type> MappingTypes = new Dictionary<Programset,Type>();
		Dictionary<Programset, ConvertBase>ConvertMaps = new Dictionary<Programset,ConvertBase>();
		private ConvertDB LocalDatabse;

		public PACConverter( ConvertDB LocalDB)
		{
			LocalDatabse = LocalDB;
			InitMappingType();
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
			Array arrenum = Enum.GetValues( typeof( EnumControl));
			AutoResetEvent[]evts = new AutoResetEvent[arrenum.Length];
			 for(int i =0 ;i< evts.Length; i++)
				evts[i] = new AutoResetEvent(false);
			 

			ConvertTask = Task.Factory.StartNew(() => ConvertTaskProc(ConvertTaskTokenSource.Token, LocalDatabse, evts), TaskCreationOptions.LongRunning);
		}
		private void InitMappingType()
		{
			MappingTypes.Add(Programset.POS, typeof( ConverterPOS.ConverterPOS ));
			MappingTypes.Add(Programset.IOPC, typeof(ConverterIOPC.ConverterIOPC));
			MappingTypes.Add(Programset.ATM, typeof(ConverterATM.ConverterATM));
			MappingTypes.Add(Programset.CA, typeof(ConverterCA.ConverterCA));
		}

		private void ConvertProgramset(CancellationToken CancelToken, Programset pset, HttpClientSingleton httpclient, ConvertDB localdatabase, MediaTypeFormatter formatter = null )
		{
			KeyValuePair<Programset, ConvertBase>ConverterItem = ConvertMaps.FirstOrDefault(iset => iset.Key == pset);
			ConvertBase converter = null;
			if( ConverterItem.Value == null)
			{
				KeyValuePair<Programset, Type> Mappingtype = MappingTypes.FirstOrDefault(iset => iset.Key == pset);
				if( Mappingtype.Value == null)
					return;

				converter = (ConvertBase)ObjectUtils.InitObject(Mappingtype.Value, new object[] { localdatabase, pset, httpclient, CancelToken, formatter });
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
		

		private void ConvertPACData(List<ConvertInfo>lstConvertinfo, CancellationToken CancelToken, HttpClientSingleton httpclient, ConvertDB localdatabase, MediaTypeFormatter formatter = null)
		{
			var group = lstConvertinfo.GroupBy( item => item.Programset);
			foreach (var igroup in group)
			{
				ConvertProgramset(CancelToken, (Programset)igroup.Key, httpclient, localdatabase, formatter);
			}

			List<Programset>Complete = new List<Programset>();
			while (ConvertMaps != null && ConvertMaps.Count > 0)
			{
				if( CancelToken.IsCancellationRequested)
					break;

				foreach(KeyValuePair<Programset, ConvertBase>ConverterItem in ConvertMaps)
				{
					ConverterItem.Value.ConvertData();
					if(ConverterItem.Value.Done)
						Complete.Add(ConverterItem.Key);

					if( CancelToken.IsCancellationRequested)
						break;
				}
				foreach(Programset pset in Complete)
				{
					ConvertMaps[pset].Dispose();
					ConvertMaps.Remove(pset);
				}

				Complete.Clear();
			}
			//foreach (ConvertInfo convertinfo in lstConvertinfo)
			//{
			//	ConvertProgramset(CancelToken, (Programset)convertinfo.Programset, httpclient, localdatabase, formatter);
			//	if (CancelToken.IsCancellationRequested || convertinfo.Programset == lstConvertinfo[lstConvertinfo.Count - 1].Programset && MappingTypes.Count == 0)
			//		break;
			//}
		}
		
		private void CancelAllTask()
		{
			if( ConvertTask == null)
				return;

			try
			{
				ConvertTokenSource.Cancel(false);
			} catch(Exception){}

			try
			{
				ConvertTaskTokenSource.Cancel(false);
			}catch(Exception){}

			ConvertTask.Wait();
			ConvertTask.Dispose();
			ConvertTask = null;

		}

		private void ConvertTaskProc(CancellationToken CancelToken, ConvertDB database, AutoResetEvent[] Events)
		{
			IEnumerable<ConvertInfo> Convertinfo = database.ConvertInfo.Where( cvt => cvt.Enable == true).OrderBy( item => item.Order);

			HttpClientSingleton apiservice = new HttpClientSingleton(LocalDatabse.ServiceConfig);
			DateTime ConvertDate = DateTime.Now.AddMinutes(Delay_Convert_time_At_start);
			int event_index = -1;
			int time_out = Time_Out_Event;
			while( !CancelToken.IsCancellationRequested)
			{
				event_index = WaitHandle.WaitAny(Events, time_out);
				switch( event_index)
				{
					case (int)EnumControl.EVT_CONVERT_DATA:
							ConvertPACData(Convertinfo.ToList(), ConvertTokenSource.Token, apiservice, database, dataFormatter);
							ConvertDate.AddMinutes(LocalDatabse.ServiceConfig.Interval);
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
	}
}

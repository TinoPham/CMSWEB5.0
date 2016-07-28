using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Commons;
using ConverterDB;

namespace PACDMSimulator.PACDMConverter.ConverterATM
{
	internal class ConverterATM : ConvertBase
	{
		public ConverterATM(ConvertDB LocalDB, Programset pset, HttpClientSingleton httpclient, CancellationToken CancelToken, MediaTypeFormatter dataformatter = null)
			: base(LocalDB, pset, httpclient, typeof(ConverterATM).Namespace, CancelToken, dataformatter)
		{
		}
	}
}

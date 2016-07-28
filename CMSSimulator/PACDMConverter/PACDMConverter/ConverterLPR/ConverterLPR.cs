using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Commons;
using ConverterDB;

namespace PACDMSimulator.PACDMConverter.ConverterLPR
{
	internal class ConverterLPR : ConvertBase
	{
		public ConverterLPR(ConvertDB LocalDB, Programset pset, HttpClientSingleton httpclient, CancellationToken CancelToken, MediaTypeFormatter dataformatter = null)
			: base(LocalDB, pset, httpclient, typeof(ConverterLPR).Namespace, CancelToken, dataformatter)
		{
		}
	}
}

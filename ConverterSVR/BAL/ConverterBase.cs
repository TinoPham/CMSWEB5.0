using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Commons;
using ConvertMessage;
using System.Net.Http.Formatting;
using PACDMModel;
using SVRDatabase;
namespace ConverterSVR.BAL
{
	internal class ConverterBase : IDisposable
	{
		protected MessageData _message{ get; private set;}
		protected MessageDVRInfo DVRInfo { get ; private set;}

		public ConverterBase( MessageData msg, MessageDVRInfo dvrinfo)
		{
			_message = msg;
			DVRInfo = dvrinfo;
		}

		public virtual ERROR_CODE ValidateMessage()
		{
			return Commons.ERROR_CODE.OK;
		}

		public virtual async Task<MessageResult> ConvertMessage(PACDMDB PACModel, SVRManager LogModel, MediaTypeFormatter formatter)
		{
			return await Task.FromResult<MessageResult>(new MessageResult());
		}

		public virtual void Dispose()
		{
			_message = null;
			DVRInfo = null;
		} 
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using ConvertMessage;

namespace ConverterSVR.IServices
{
	public interface IConvertService
	{
		MessageResult DVRRegister(ref MessageDVRInfo dvrinfo, MediaTypeFormatter formatter, out bool newdvr);
		Task <MessageResult> DVRMessage(MessageData msgBody, MessageDVRInfo dvrinfo, MediaTypeFormatter formatter);
		MessageResult CheckDvrAviliable(ref MessageDVRInfo dvrinfo);
	}
}

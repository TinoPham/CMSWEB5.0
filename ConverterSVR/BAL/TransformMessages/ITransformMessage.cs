using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvertMessage.PACDMObjects.POS;
using PACDMModel.Model;
using System.Reflection;
namespace ConverterSVR.BAL.TransformMessages
{
	internal interface ITransformMessage<InT, OutT> where InT : class where OutT : class
	{
		OutT TransForm( InT input, ConvertMessage.MessageDVRInfo DVRInfo );
	}

	internal abstract class TransformMessageBase
	{
		public int KDVR{ get; set;}
	}
}

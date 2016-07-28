using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using Commons;
using BAL = ConverterSVR.BAL;
using BALDVRRegister = ConverterSVR.BAL.DVRRegister;
using BALConverter = ConverterSVR.BAL.PACDMConverter;
using ConverterSVR.IServices;
using ConvertMessage;
using PACDMModel;
using SVRDatabase;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;

namespace ConverterSVR.Services
{
	public class ConvertService : IConvertService
	{
		private class ServiceMapping : Commons.SingletonStringTypeMappingBase<ServiceMapping>
		{

			private ServiceMapping()
			{

				base.AddMapping(Programset.DVR.ToString(), typeof(BAL.DVRConverter.DVRConverter));
				base.AddMapping(Programset.ATM.ToString(), typeof(BAL.PACDMConverter.PACDMConverter));
				base.AddMapping(Programset.CA.ToString(), typeof(BAL.PACDMConverter.PACDMConverter));
				base.AddMapping(Programset.IOPC.ToString(), typeof(BAL.PACDMConverter.PACDMConverter));
				base.AddMapping(Programset.LPR.ToString(), typeof(BAL.PACDMConverter.PACDMConverter));
				base.AddMapping(Programset.POS.ToString(), typeof(BAL.PACDMConverter.PACDMConverter));
                base.AddMapping(Programset.LABOR.ToString(), typeof(BAL.PACDMConverter.PACDMConverter));
                base.AddMapping(Programset.POS3RD.ToString(), typeof(BAL.PACDMConverter.PACDMConverter));
			}

			public KeyValuePair<string, Type>GetServiceConvert( string programset)
			{
			  if( string .IsNullOrEmpty(programset))
				return new KeyValuePair<string,Type>();
				return base.GetMapping(programset);

			}
			public KeyValuePair<string, Type>GetServiceConvert( Programset programset)
			{
			  return GetServiceConvert( programset.ToString());
			}
		}
		

		PACDMDB PACModel;
		SVRManager LogModel;

		public ConvertService(PACDMDB PACmodel, SVRManager Logmodel)
		{
			PACModel = PACmodel;
			LogModel = Logmodel;

		}

		public MessageResult DVRRegister(ref MessageDVRInfo dvrinfo, MediaTypeFormatter formatter, out bool newdvr)
		{
			return BALDVRRegister.DVRRegister.Instance.RegisterDVR(ref dvrinfo, PACModel, LogModel,out newdvr);
			
		}

		public MessageResult CheckDvrAviliable(ref MessageDVRInfo dvrinfo)
		{
			return BALDVRRegister.DVRRegister.Instance.CheckDvrAviliable(ref dvrinfo, PACModel, LogModel);
		}

		public async Task<MessageResult> DVRMessage(MessageData msgBody, MessageDVRInfo dvrinfo, MediaTypeFormatter formatter)
		{
			KeyValuePair<string,Type> mapping = ServiceMapping.Instance.GetServiceConvert( msgBody.Programset);

			if( string.IsNullOrEmpty(mapping.Key))
			{
				return new MessageResult(){ ErrorID = ERROR_CODE.INVALID_MAPPING, Data = null};
			}

			BAL.ConverterBase converter = Commons.ObjectUtils.InitObject(mapping.Value, new object[] { msgBody, dvrinfo }) as BAL.ConverterBase;
			Commons.ERROR_CODE ret = converter.ValidateMessage();

			if( ret == ERROR_CODE.OK)
				return await converter.ConvertMessage(PACModel, LogModel, formatter);
			
			converter.Dispose();
			converter = null;
			return await Task.FromResult<MessageResult>( new MessageResult{ ErrorID = ret});  //new MessageResult();
		}
	}

	public class DVRRegisterSupports : Commons.SingletonStringTypeMappingBase<DVRRegisterSupports>
	{
		const string str_HASKey = "HASKeyID";
		public const Int16 DVR_KeepALive = 30;
		public const int DVR_ConverterInterval = 10;
		public const short DVR_ConvertLimit = 0;
		public readonly string Default_Authenticate;
		const string str_MACAddress = "MACAddress";
		const string str_IPAddress = "IPAddress";
		const string str_LocalIPAddress = "LocalIPAddress";

		public const char Register_Split_item_OR = '|';
		public const char Register_Split_item_AND = '&';
		public const string Regex_Match_DVR = @"(?<Match>[a-zA-Z]+)(?<Sign>(\||&))?";
		public const string Str_Match = "Match";
		public const string Str_Sign = "Sign";

		public ReadOnlyDictionary<string, string> RegisterSupportList = null;
		private readonly Dictionary<string, string> _RegisterSupportList;

		private DVRRegisterSupports()
		{
			base.AddMapping(str_HASKey, typeof(BALDVRRegister.HaskeyRegister));
			base.AddMapping(str_MACAddress, typeof(BALDVRRegister.MacAddressRegister));
			base.AddMapping(str_IPAddress, typeof(BALDVRRegister.LocalIPRegister));

			if (_RegisterSupportList == null)
			{
				_RegisterSupportList = new Dictionary<string, string>();
				_RegisterSupportList.Add(str_HASKey, Commons.Resources.ResourceManagers.Instance.GetResourceString(str_HASKey));
				_RegisterSupportList.Add(str_MACAddress, Commons.Resources.ResourceManagers.Instance.GetResourceString(str_MACAddress));
				_RegisterSupportList.Add(str_IPAddress, Commons.Resources.ResourceManagers.Instance.GetResourceString(str_LocalIPAddress));
			}
			Default_Authenticate = string.Join(Register_Split_item_AND.ToString(), _RegisterSupportList.Keys.ToList());
			RegisterSupportList = new ReadOnlyDictionary<string,string>(_RegisterSupportList);
		}

		public IEnumerable<Match> ParserDVRRegister(string config)
		{
			if (string.IsNullOrEmpty(config))
				return new List<Match>();
			Regex Rx = new Regex(Regex_Match_DVR, RegexOptions.IgnoreCase);
			MatchCollection Matchs = Rx.Matches(config);
			return Matchs.Cast<Match>();
		}
		
		public char combineOperator( string value)
		{
			if( string.IsNullOrEmpty(value))
				return Register_Split_item_AND;
			return value.First() == Register_Split_item_AND? Register_Split_item_AND: Register_Split_item_OR;
		}

		public string BuildDVRAuth( List<string> keys, bool matchalls)
		{
			char split = matchalls? Register_Split_item_AND : Register_Split_item_OR;
			return  string.Join( split.ToString(), keys);
		}
	}

	//public class DVRRegisterSupports
	//{
	//	const string str_HASKey= "HASKeyID";
		
	//	const string str_MACAddress = "MACAddress";
	//	const string str_IPAddress = "LocalIPAddress";

	//	readonly Dictionary<string, Type> _RegisterMapping = null;

	//	readonly Dictionary<string, string> _RegisterSupportList = null;

	//	public Dictionary<string, string>DVRRegisterSupportList{ get{ return _RegisterSupportList;}}

	//	private static readonly Lazy<DVRRegisterSupports> Lazy = new Lazy<DVRRegisterSupports>(() => new DVRRegisterSupports());
		
	//	public static DVRRegisterSupports Instance { get { return Lazy.Value; } }

	//	private DVRRegisterSupports()
	//	{
	//		if (_RegisterMapping == null)
	//		{
	//			_RegisterMapping = new Dictionary<string, Type>();
	//			_RegisterMapping.Add(str_HASKey, typeof(BALDVRRegister.HaskeyRegister));
	//			_RegisterMapping.Add(str_MACAddress, typeof(BALDVRRegister.MacAddressRegister));
	//			_RegisterMapping.Add(str_IPAddress, typeof(BALDVRRegister.LocalIPRegister));
	//		}

	//		if(_RegisterSupportList == null)
	//		{
	//			_RegisterSupportList = new Dictionary<string,string>();
	//			_RegisterSupportList.Add( str_HASKey, Commons.Resources.ResourceManagers.Instance.GetResourceString(str_HASKey)  );
	//			_RegisterSupportList.Add(str_MACAddress, Commons.Resources.ResourceManagers.Instance.GetResourceString(str_MACAddress));
	//			_RegisterSupportList.Add(str_IPAddress, Commons.Resources.ResourceManagers.Instance.GetResourceString(str_IPAddress));
	//		}
	//	}
	//	public KeyValuePair< string, Type> GetRegisterMap( string key)
	//	{
	//		return _RegisterMapping.FirstOrDefault( item => string.Compare( item.Key, key, false)== 0);
	//	}
	//}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using CMSWebApi.Utils;
using Newtonsoft.Json;

namespace CMSWebApi.APIFilters
{
	public class WebUserToken
	{
		
		public static string GetToken(DataModels.LoginModel model)
		{
			try
			{
			string json = model.ToString(); //JsonConvert.SerializeObject( model);
			FormsAuthenticationTicket tick = new FormsAuthenticationTicket(model.ID, model.UserName,  DateTime.Now, DateTime.Now.AddDays(1),true, json);
			string rawtoken = FormsAuthentication.Encrypt( tick);
			return toBase64(rawtoken);
			}
			catch(Exception)
			{
				return null;
			}
		}

		public static string RawToken(string encrypt)
		{
			try
			{
				string rawtoken = toString(encrypt);
				FormsAuthenticationTicket tick = FormsAuthentication.Decrypt(rawtoken);
				return tick.UserData;
			}
			catch(Exception)
			{
				return null;
			}
		}
		public static DataModels.LoginModel GetModel(string token)
		{
			if( string.IsNullOrEmpty(token))
				return null;
			DataModels.LoginModel model = new DataModels.LoginModel();
			try
			{
				//string rawtoken = toString(token);
				//FormsAuthenticationTicket tick = FormsAuthentication.Decrypt(rawtoken);
				//model = JsonConvert.DeserializeObject<DataModels.LoginModel>( tick.UserData);

				model.Parser( RawToken(token));


			}
			catch(Exception){}

			return model;
		}

		private static string toBase64(string token)
		{
			if( string.IsNullOrEmpty(token))
				return token;
			string b64 =  Commons.Utils.String2Base64(token);
			b64 = b64.Replace(Consts.EQUAL_SIGN, Consts.AT_SIGN);
			return b64.Replace(Consts.PLUS_SIGN, Consts.STAR_SIGN);
		}

		private static string toString(string base64)
		{
			if (string.IsNullOrEmpty(base64))
				return base64;
			base64 = base64.Replace(Consts.AT_SIGN, Consts.EQUAL_SIGN);
			base64 = base64.Replace(Consts.STAR_SIGN, Consts.PLUS_SIGN);
			return Commons.Utils.Base64toString(base64);

		}
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using CMSWebApi.Utils;

namespace CMSWebApi.DataModels
{
	
	#region Data
	public class CMSWebFunctionModel
	{
		public int FunctionID { get; set; }
		public string FunctionName { get; set; }
		public Nullable<int> ModuleID { get; set; }
	}

	public class CMSWebLevelModel
	{
		public int LevelID { get; set; }
		public string LevelName { get; set; }
	}

	public class UserSimple
	{
		public int UserID { get; set; }
		public string FName { get; set; }
		public string LName { get; set; }
		public bool Status { get; set; }
	}

	public class UserInfo
	{
		public int UID { get; set; }
		public int EmpID { get; set; }
		public string FName { get; set; }
		public string LName { get; set; }
		public string PosName { get;set;}
		public DateTime? ExDate { get; set; }
		public bool Expired{ get ;set;}
		public string Email { get; set; }
		public string GpName { get; set; }
		public int PosColor { get; set; }
		public string UserName { get; set; }
		public string UPhoto { get; set; }
		public byte[] ImageSrc { get; set; }
	}

	public class _3rdLoginModel : LoginModel
	{
		public string ServerID{ get ;set;}

		public override string ToString()
		{
			string result = base.ToString();
			result += base.SID + Environment.NewLine;
			result += ServerID + Environment.NewLine;
			return result;
		}

		public override void Parser(string data)
		{
			string [] models = data.Split(new string [] { Environment.NewLine }, StringSplitOptions.None);
			int idex = 0;
			Int32.TryParse(models [idex++], out _ID);
			UserName = models [idex++];
			Password = models [idex++];
			//SID = models [idex++];
			Lang = models [idex++];
			Remember = string.Compare(models [idex++], "1") == 0 ? true : false;
			Createdby = null;
			if (!string.IsNullOrEmpty(models [idex]))
			{
				int crby = 0;
				Int32.TryParse(models [idex++], out crby);
				_Createdby = crby == 0 ? (Nullable<int>)null : crby;
			}
			else
				idex++;
			int comID = 0;
			Int32.TryParse(models [idex++], out comID);
			CompanyID = comID;
			base.SID = models[idex ++];
			ServerID = models [idex++];
		}

	}

	public class LoginModel : Object
	{
		protected int _ID = 0;
		protected Nullable<int> _Createdby = null;
		public int ID { get { return _ID; } set { _ID = value; } }
		public string UserName { get; set; }
		public string Password { get; set; }
		public string Lang { get; set; }
		public bool Remember { get; set; }
		public string SID { get; set; }
		public int CompanyID { get; set; }
		public Nullable<int> Createdby { get { return _Createdby; } set { _Createdby = value; } }
		public virtual new string ToString()
		{
			string ret = ID.ToString() + Environment.NewLine;
			ret += UserName + Environment.NewLine;
			ret += Password + Environment.NewLine;
			//ret += SID + Environment.NewLine;
			ret += Lang + Environment.NewLine;
			ret +=(Remember ? "1" : "0") + Environment.NewLine;
			ret += !Createdby.HasValue ? string.Empty : Createdby.Value.ToString();
			ret += Environment.NewLine;
			ret += CompanyID.ToString() + Environment.NewLine;
			return ret;
		}

		public virtual void Parser(string data)
		{
			string [] models = data.Split(new string [] { Environment.NewLine }, StringSplitOptions.None);
			int idex = 0;
			Int32.TryParse(models [idex++], out _ID);
			UserName = models [idex++];
			Password = models [idex++];
			//SID = models [idex++];
			Lang = models [idex++];
			Remember = string.Compare(models [idex++], "1") == 0 ? true : false;
			Createdby = null;
			if (!string.IsNullOrEmpty(models [idex]))
			{
				int crby = 0;
				Int32.TryParse(models [idex++], out crby);
				_Createdby = crby == 0 ? (Nullable<int>)null : crby;
			}
			else 
				idex++;
			int comID = 0;
			Int32.TryParse(models [idex++], out comID);
			CompanyID = comID;
		}
	}

	[Serializable]
	public class _3rdUserContext: UserContext
	{
		public string SID{ get ; private set;}

		public _3rdUserContext(_3rdLoginModel loginmodel):base( loginmodel)
		{
			SID = loginmodel.SID;
		}
	}
	[Serializable]
	public class UserContext : IIdentity
	{
		public string Name { get; set; }

		[Newtonsoft.Json.JsonIgnore]
		public string AuthenticationType
		{
			get { return "Basic"; }
		}

		[Newtonsoft.Json.JsonIgnore]
		public bool IsAuthenticated
		{
			get { return true; }
		}

		[Newtonsoft.Json.JsonIgnore]
		public int ParentID { get { return Createdby.HasValue ? Createdby.Value : ID; } }

		//public CultureInfo Culture { get; private set; }
		public string Culture { get; private set; }

		public int ID { get; private set; }

		public string PWD { get; private set; }
		public int CompanyID { get; private set; }

		public Nullable<int> Createdby { get; private set; }

		public UserContext(LoginModel loginmodel)
		{
			Name = loginmodel.UserName;
			ID = loginmodel.ID;
			PWD = loginmodel.Password;
			Createdby = loginmodel.Createdby;
			CompanyID = loginmodel.CompanyID;
			try
			{
				if (string.IsNullOrEmpty(loginmodel.Lang))
					Culture = Consts.DEFAULT_CULTURE;
				else
					Culture = loginmodel.Lang;
			}
			catch (CultureNotFoundException)
			{
				Culture = Consts.DEFAULT_CULTURE;
			}


		}
	}
	
	public class AppSetting
	{
		public int CompImgSize { get; set; }
		public int EncryptMode { get; set; }
		public string ImageUrl { get; set; }
		public int UserImgSize { get; set; }
	}

	public class UserInfoDetail:UserInfo
	{
		public string UName { get; set; }
		public string Pwd { get; set; }
		public int? GroupID { get; set; }
		public bool? EmailDaily { get; set; }
		public bool isAdmin { get; set; }
		public int? PosID { get; set; }
		public string Tel { get; set; }
		public string Notes { get; set; }
	}

	#endregion

	public class UserProfile
	{
		/// <summary>
		/// User ID
		/// </summary>
		public int UserID { get; set; }
		/// <summary>
		/// First name
		/// </summary>
		public string FName { get; set; }
		/// <summary>
		/// Last Name
		/// </summary>
		public string LName { get; set; }

		public string Email { get; set; }

		public string UPhoto { get; set; }

		public byte [] ImageSrc { get; set; }
	}

	public class ChangePasswordModel
	{
		/// <summary>
		/// User ID
		/// </summary>
		public int UserId { get; set; }
		/// <summary>
		/// First name
		/// </summary>
		[Required(ErrorMessage = "FIELD_REQUIRED")]
		public string CurrentPassword { get; set; }
		[Required(ErrorMessage = "FIELD_REQUIRED")]
		public string NewPassword { get; set; }
	}

	public class ForgotPasswordModel
	{
		public ForgotPasswordModel()
		{
			LanguageKey = "en-US";
		}

		[Required(ErrorMessage = "FIELD_REQUIRED")]
		[EmailAddress]
		public string Email { get; set; }
		public string UserName { get; set; }
		public string LanguageKey { get; set; }
	}

	[XmlRoot]
	public class EmailSetting
	{
		public EmailSetting()
		{
			PasswordLength = 8;
			NumberNonOfAlpha = 0;
		}

		[XmlElement]
		public int PasswordLength { get; set; }

		[XmlElement]
		public int NumberNonOfAlpha { get; set; }

		[XmlElement]
		public string Subject { get; set; }

		[XmlElement]
		public bool IsHtml { get; set; }

		public string Content;
		[XmlElement("CDataElement")]
		public XmlCDataSection Message
		{
			get
			{
				var doc = new XmlDocument();
				return doc.CreateCDataSection(Content);
			}
			set
			{
				Content = value.Value;
			}
		}
	}

	public class UserModel :UserProfile
	{
		
		/// <summary>
		/// User's group ID
		/// </summary>
		public int GroupID { get; set; }
		/// <summary>
		/// User's position ID
		/// </summary>
		public int? PositionID { get;set;}
		/// <summary>
		/// User's Company ID
		/// </summary>
		public int? CompanyID { get;set;}
		/// <summary>
		/// user has accepted license
		/// </summary>
		public bool? Accepted { get;set;}
		/// <summary>
		/// User's employee Id
		/// </summary>
		public int EmployeeID { get; set; }
		public string Telephone { get; set; }
		public int IdleTimeout { get; set; }
		
		public string Notes { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
		public DateTime? ExpiredDate { get; set; }
		public DateTime? CreatedDate { get; set; }
		public int? CreatedBy { get; set; }
		public bool IsAdmin { get; set; }
		public bool? EmailDaily { get; set; }
		

		/* Extra Properties */
		public bool isExpired { get; set; }
		public string PositionName { get; set; }
		public List<int> SiteIDs { get; set; }
		public int? Color { get; set; }
		public int RecordingDays { get; set; }

		//list all menu
		public List<ApplicationMenu> Menus{ get; set;}
		public List<CMSWebSiteModel> Sites { get; set; }
		public IQueryable<CMSWebFunctionModel> Functions { get; set; }
		public IQueryable<CMSWebLevelModel> Levels { get; set; }
		public AppSetting Settings { get; set; }
	}

	//public class UsersInfoModel : TransactionalInformation
	//{
	//	public IEnumerable<UserInfo> UserInfo { get; set; }
	//}
	//public class UserinfoModel : TransactionalInformation
	//{
	//	public UserInfo UserInfo { get ;set; }
	//}
	public class UserSiteDvrChannel
	{
		public Nullable<int> UserID { get; set; }
		public Nullable<int> siteKey { get; set; }
		public Nullable<int> KDVR { get; set; }
		public Nullable<int> KChannel { get; set; }
		public Nullable<int> PACID { get; set; }
	}

}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Exchange.WebServices.Data;

namespace CMSWebApi.Configurations
{
	public class EmailSettingSection : ConfigurationSection
	{
		public const string EmailSettingsSection_Name = "EmailSettings";
		[ConfigurationProperty(Defines.STR_Server, IsRequired = true)]
		public MailServer Server { get { return base [Defines.STR_Server] as MailServer; } }

		[ConfigurationProperty(Defines.STR_Account, IsRequired = true)]
		public MailAccount Account { get { return base [Defines.STR_Account] as MailAccount; } }


	}

	public class MailAccount : ConfigurationElement
	{
		[ConfigurationProperty(Defines.STR_address, IsRequired = true)]
		public string Address
		{
			get
			{
				return (string)this [Defines.STR_address];
			}
			set
			{
				this [Defines.STR_address] = value;
			}
		}

		[ConfigurationProperty(Defines.STR_userid)]
		public string UserID
		{
			get
			{
				return (string)this [Defines.STR_userid];
			}
			set
			{
				this [Defines.STR_userid] = value;
			}
		}

		[ConfigurationProperty(Defines.STR_PWD)]
		public string Pwd
		{
			get
			{
				return (string)this [Defines.STR_PWD];
			}
			set
			{
				this [Defines.STR_PWD] = value;
			}
		}

		[ConfigurationProperty(Defines.STR_name)]
		public string Name
		{
			get
			{
				return (string)this [Defines.STR_name];
			}
			set
			{
				this [Defines.STR_name] = value;
			}
		}
	}

	public class MailServer : ConfigurationElement
	{
		[ConfigurationProperty(Defines.STR_Host, IsRequired = true)]
		public string Host
		{
			get
			{
				return (string)this [Defines.STR_Host];
			}
			set
			{
				this [Defines.STR_Host] = value;
			}
		}

		[ConfigurationProperty(Defines.STR_port, IsRequired = true, DefaultValue = Defines.Default_Mailserver_Port)]
		public int Port
		{
			get
			{
				return (int)this [Defines.STR_port];
			}
			set
			{
				this [Defines.STR_port] = value;
			}
		}

		[ConfigurationProperty(Defines.STR_Type, IsRequired = true, DefaultValue = Defines.STR_DEFAULT_MAIL_TYPE)]
		[TypeConverter(typeof(CaseInsensitiveEnumConfigConverter<MailServerType>))]
		public MailServerType Type
		{
			get
			{
				return (MailServerType)this [Defines.STR_Type];
			}
			set
			{
				this [Defines.STR_Type] = value.ToString();
			}
		}

		[ConfigurationProperty(Defines.STR_tsl_ssl, IsRequired = true, DefaultValue = Defines.STR_DEFAULT_MAIL_SERCURE)]
		[TypeConverter(typeof(CaseInsensitiveEnumConfigConverter<MailSercure>))]
		public MailSercure Tsl_Ssl
		{
			get
			{
				return (MailSercure)this [Defines.STR_tsl_ssl];
			}
			set
			{
				this [Defines.STR_tsl_ssl] = value.ToString();
			}
		}

		[ConfigurationProperty(Defines.STR_EXCHANGE, IsRequired = false)]
		public ExchangeExtend ExchangeExtend { get { return base [Defines.STR_EXCHANGE] as ExchangeExtend; } }
	}
	public class ExchangeExtend: ConfigurationElement
	{
		[ConfigurationProperty(Defines.STR_AutodiscoverUrl, IsRequired = false,DefaultValue= false)]
		public bool AutodiscoverUrl
		{
			get
			{
				return (bool)this [Defines.STR_AutodiscoverUrl];
			}
			set
			{
				this [Defines.STR_AutodiscoverUrl] = value;
			}
		}

		[ConfigurationProperty(Defines.STR_Version, IsRequired = false, DefaultValue = "Exchange2007_SP1")]
		[TypeConverter(typeof(CaseInsensitiveEnumConfigConverter<ExchangeVersion>))]
		public ExchangeVersion Version
		{
			get
			{
				return (ExchangeVersion)this [Defines.STR_Version];
			}
			set
			{
				this [Defines.STR_Version] = value;
			}
		}
	}
}

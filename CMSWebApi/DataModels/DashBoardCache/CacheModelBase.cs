using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Extensions;
using Newtonsoft.Json;

namespace CMSWebApi.DataModels.DashBoardCache
{
	public abstract class CacheModelBase
	{
		[Flags]
		protected enum NormalizeFlag:byte
		{
			Normalize = 1,
			Report_Normalize = Normalize << 1
		} 
		protected bool IsFlagValue( byte val, NormalizeFlag flagVal)
		{
			return (val & (byte)flagVal) == (byte)flagVal;
		}
		[XmlIgnore]
		[JsonIgnore]
		public virtual int ItemTime{ get; private set;}
		
	}
	
}

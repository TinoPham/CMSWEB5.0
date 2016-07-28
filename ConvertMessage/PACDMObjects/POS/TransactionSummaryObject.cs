using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace ConvertMessage.PACDMObjects.POS
{
	
	[DataContract(Namespace = Consts.Empty, Name = Consts.str_TransactionSummary)]
	public class TransactionSummary
	{
		//private string m_storeID;
		//private string m_PACID;
		private DateTime m_date;
		private int m_hour;
		private int m_totalTrans;
		private decimal m_totalAmount;

		//[XmlElement("StoreID")]
		//public string StoreID
		//{
		//	get { return this.m_storeID; }
		//	set { this.m_storeID = value; }
		//}

		//[XmlElement("PACID")]
		//public string PACID
		//{
		//	get { return this.m_PACID; }
		//	set { this.m_PACID = value; }
		//}

		//[XmlElement("Date")]
		[DataMember]
		public DateTime Date
		{
			get { return this.m_date; }
			set { this.m_date = value; }
		}

		[DataMember]
		public int Hour
		{
			get { return this.m_hour; }
			set { this.m_hour = value; }
		}

		[DataMember]
		public int TotalTrans
		{
			get { return this.m_totalTrans; }
			set { this.m_totalTrans = value; }
		}

		[DataMember]
		public decimal TotalAmount
		{
			get { return this.m_totalAmount; }
			set { this.m_totalAmount = value; }
		}
	}
}

using System.ComponentModel.DataAnnotations;

namespace CMSWebApi.DataModels.Alert
{
	public class IgnoreAlertModel
	{
		public int Sites { get; set; }
		public int Kdvr { get; set; }
		public int Kchannel { get; set; }
		public int KAlert { get; set; }
		[MaxLength(250, ErrorMessage = "MAX_LENGTH_250")]
		public string Description { get; set; }
	}
}

namespace CMSWebApi.DataModels
{
	public class Country
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Code { get; set; }
		public int? Sort { get; set; }
	}

	public class State
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string CountryCode { get; set; }
	}
}

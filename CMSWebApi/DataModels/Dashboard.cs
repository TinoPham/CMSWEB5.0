using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CMSWebApi.DataModels
{

	public class DashboardUser : Dashboard
	{
		public int UserId { get; set; }
		public int? StyleId { get; set; }
		public DashboardStyle Style { get; set; }
	}


	public class Dashboard
	{
		public Dashboard()
		{
			Rows = new HashSet<Row>();
		}
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public int? Type { get; set; }
		public string Image { get; set; }
		public ICollection<Row> Rows { get; set; }
	}

	public class Row
	{
		public Row()
		{
			Columns = new HashSet<Column>();
		}

		public int Id { get; set; }
		public ICollection<Column> Columns { get; set; }
	}

	public class Column
	{
		public Column()
		{
			Widgets = new HashSet<Widget>();
			IsLock = false;
		}
		public int Id { get; set; }
		public int PositionId { get; set; }
		public int GroupSizeId { get; set; }
		public int WidthSize { get; set; }
		public bool IsLock { get; set; }
		public WidgetGroup Group { get; set; }
		public ICollection<Widget> Widgets { get; set; }
	}

	public class Element
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string TemplateUrl { get; set; }
		public short? TypeSize { get; set; }
		public string TemplateJs { get; set; }
		public string TemplateParams{ get ;set;}
		public int GroupSizeId { get; set; }
		public WidgetGroup Group { get; set; }
	}

	public class WidgetGroup
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int? MaxWidgets { get; set; }
		public bool? IsHeader { get; set; }
	}

	public class Widget : Element
	{
		public int PositionId { get; set; }
		public bool? TemplateDisable { get; set; }
		public int Order { get; set; }
		public int? StyleId { get; set; }
		public DashboardStyle Style { get; set; }
	}

	public class DashboardStyle
	{
		public int Id { get; set; }
		public int? BackgroudColor { get; set; }
		public int? FontColor { get; set; }
	}

	public class Note
	{
		public int Id { get; set; }
		public int UserId { get; set; }
		public DateTime? CreatedOn { get; set; }
		[Required(ErrorMessage = "REQUIRED_NOTE_CONTENT")]		
		public string Content { get; set; }
	}

	public class ToDo
	{
		public int Id { get; set; }
		public int UserId { get; set; }
		public DateTime? CreatedOn { get; set; }
		[Required(ErrorMessage = "REQUIRED_TODO_CONTENT")]
		[MaxLength(100, ErrorMessage = "MAX_LENGTH_100")]
		public string Content { get; set; }
		public int? Color { get; set; }
		public string Icon { get; set; }
		public byte? Status { get; set; }
		public int? Font { get; set; }
		public byte? Urgency { get; set; }
		public short? Recurrence { get; set; }
	}

	public class Proc_BAM_Get_DashBoard_ForeCast_Result
	{
		public int PACID { get; set; }
		public string PACID_Name { get; set; }
		public DateTime DVRDateKey { get; set; }
		public Nullable<double> DPO { get; set; }
		public Nullable<double> Conversion { get; set; }
		public Nullable<int> TotalTraffic { get; set; }
	}

	public class Proc_DashBoard_Traffic_ForeCast_Result
	{
		public int PACID { get; set; }
		public string PACID_Name { get; set; }
		public DateTime DVRDateKey { get; set; }
		public Nullable<int> TotalTraffic { get; set; }
	}

	public class Proc_DashBoard_Traffic_ForeCast_Hourly_Result : Proc_DashBoard_Traffic_ForeCast_Result
	{
		public int C_Hour { get; set; }
	}

	public class Proc_DashBoard_Channel_EnableTrafficCount_Result
	{
		public int CameraID { get; set; }
		public int PACID { get; set; }
		public int KDVR { get; set; }
	}
}

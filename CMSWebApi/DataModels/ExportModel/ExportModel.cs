using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.DataModels.ExportModel
{
	public class ExportModel
	{
		public ReportInfoModel ReportInfo { get; set; }
		public List<GridData> GridModels { get; set; }
		public List<ChartData> ChartModels { get; set; }
	}

	public class ReportInfoModel
	{
		public string TemplateName { get; set; }
		public int ReportType { get; set; }
		public string ReportName { get; set; }
		public int CompanyID { get; set; }
		public string CompanyName { get; set; }
		public string RegionName { get; set; }
		public string Location { get; set; }
		public int WeekIndex { get; set; }
		public string Footer { get; set; }
		public string CreatedBy { get; set; }
		public string CreateDate { get; set; }
	}

	public class GridData
	{
		private List<RowData> _rowData = new List<RowData>();

		public string Name { get; set; }
		public Dictionary<string, string> OptionDatas { get; set; }
		public List<RowData> RowDatas
		{
			get
			{
				return _rowData;
			}

			set
			{
				_rowData = value;
			}
		}
		public TableFormat Format { get; set; }
	}

	//public class FormatBase
	//{
	//	public int ColIndex { get; set; }
	//	public int RowIndex { get; set; }
	//}

	public class TableFormat
	{
		//public int ColumnFirstSpace { get; set; }
		//public int ColumnSpace { get; set; }
		//public int ColumnEndSpace { get; set; }

		public int ColIndex { get; set; }
		public int RowIndex { get; set; }
	}

	public class ChartFormat
	{
		public int Width { get; set; }
		public int Height { get; set; }
		public int ColIndex { get; set; }
		public int RowIndex { get; set; }
	}

	public class RowData
	{
		private List<ColData> _colDatas = new List<ColData>();

		public int Type { get; set; }

		public List<ColData> ColDatas
		{
			get
			{
				return _colDatas;
			}

			set
			{
				_colDatas = value;
			}
		}

		public GridData GridModels { get; set; } 
	}

	public class MergeCellModel
	{
		public int Cells { get; set; }
		public int Rows { get; set; }
	}

	public class ColData
	{
		public ColData()
		{
			MergeCells = new MergeCellModel()
			{
				Cells = 1,
				Rows = 1
			};
		}

		public string Value { get; set; }
		public int Color { get; set; }
		public bool CustomerWidth { get; set; }
		public int Width { get; set; }
		public MergeCellModel MergeCells { get; set; }
		public int ColIndex { get; set; }
		public int RowIndex { get; set; }

	}

	public class ChartData
	{
		private List<ChartDataItem> _chartDataItems = new List<ChartDataItem>();
		public string Name { get; set; }
		public string Title { get; set; }
		public int ChartType { get; set; }
		public Dictionary<string, string> OptionDatas { get; set; }
		public List<ChartDataItem> ChartDataItems
		{
			get
			{
				return _chartDataItems;
			}

			set
			{
				_chartDataItems = value;
			}
		}
		public ChartFormat Format { get; set; }
	}

	public class ChartDataItem
	{
		public string Name { get; set; }
		public string Value { get; set; }
		public int Color { get; set; }
	}
}

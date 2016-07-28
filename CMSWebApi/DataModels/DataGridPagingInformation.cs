﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.DataModels
{
	public class DataGridPagingInformation
	{
		public int CurrentPageNumber { get; set; }
		public int PageSize { get; set; }
		public string SortExpression { get; set; }
		public Utils.SortDirection SortDirection { get; set; }
		public int TotalRows { get; set; }
		public int TotalPages { get; set; }

		public DataGridPagingInformation()
		{
			CurrentPageNumber = 1;
			PageSize = 10;
			SortDirection =  Utils.SortDirection.ASC;
			SortExpression = string.Empty;
			TotalPages = 0;
			TotalRows = 0;
		}

	}
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace CMSWebApi.DataModels
{
	public class UserGroupModel
	{
		public int GroupId { get; set; }
		public string GroupName { get; set; }
		public string Description { get; set; }
		public int? CreatedBy { get; set; }
		public byte? GroupLevel { get; set; }
		public int NumberUser { get; set; }
		public IEnumerable<int> Users { get; set; }
		public IEnumerable<FuncLevel> FuncLevels { get; set; }
	}

	public class FuncLevel
	{
		public int FunctionID { get; set; }
		public Nullable<int> LevelID { get; set; }
	}

	public class FunctionModel
	{
		public int FunctionID { get; set; }
		public string FunctionName { get; set; }
		public Nullable<int> ModuleID  { get; set; }
		public bool Status { get; set; }
	}

	public class FunctionLevelModel
	{
		public int LevelID { get; set; }
		public string LevelName { get; set; }
	}

	public class userGroupDeleteModel
	{
		public List<int> listUserGroupId { get; set; }
		public int userGroupIdReplace { get; set; }

		public userGroupDeleteModel() {
			listUserGroupId = new List<int>();
			userGroupIdReplace = 0;
		}
	}

}

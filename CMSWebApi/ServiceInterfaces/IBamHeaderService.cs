using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using PACDMModel.Model;
using CMSWebApi.DataModels;

namespace CMSWebApi.ServiceInterfaces
{
    public interface IBamHeaderService
	{
		Task<List<Proc_BAM_Get_Header_Stores_Count_Result>> GetCountDataHeader(int UserId, DateTime SDate);
	}
}

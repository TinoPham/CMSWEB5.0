using CMSWebApi.DataModels;
using PACDMModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.ServiceInterfaces
{
	public interface IDistributionService
	{

		Task<List<Func_BAM_TrafficCountReportMonthly_Result>> GetTrafficCount(string strPACID, DateTime From, DateTime To, int userID);
		Task<List<Func_BAM_TrafficCountReportHourly_Result>> GetTrafficCountHourly(string strPACID, DateTime From, DateTime To, int userID);
		Task<List<TrafficCountRegionInQueue_Result>> GetTrafficCountRegionInQueue(string strPACID, int userID);
		tbl_IOPC_QueueList AddQueue(tbl_IOPC_QueueList queue);
		void DeleteQueue(tbl_IOPC_QueueList queue);
		List<tbl_IOPC_TrafficCountRegion> SelectRegion(List<int> regIDs);
		List<tbl_IOPC_TrafficCountRegion> SelectRegionByNameId(List<int> regIDs, List<int> pacIDs);
		IEnumerable<tbl_IOPC_QueueList> SelectQueueByRegionIndex(int userID, List<int> RegionIndex, List<int> queue);
		tbl_IOPC_QueueList SelectQueue(int userID, int queueId);
		void Modifyrelation<T, TK>(tbl_IOPC_QueueList dbsite, IEnumerable<T> current, IEnumerable<T> news, Func<T, TK> key, Expression<Func<tbl_IOPC_QueueList, object>> properties) where T : class;
		int UpdateRegion(tbl_IOPC_QueueList queue);
		Task<List<Proc_BAM_QueueByRegionIndex_Result>> GetBAM_QueueByRegionIndex(int UserID, string RegionIndexs, string QueueIDs);
		IEnumerable<tbl_IOPC_QueueList> SelectQueueDeleteAll(int userID, List<int> exQueueIDs);
        IEnumerable<tDVRChannels> GetListChannels(int UserID, int KDVR);
        IQueryable<Tout> GetImages<Tout>(int kDVR, Expression<Func<tbl_HM_Images, Tout>> selector, string[] includes) where Tout : class;
        bool CheckExistImage(int KDVR, int schedule, DateTime dateImage, DateTime StartW = new DateTime(), DateTime EndW = new DateTime());
        tbl_HM_TaskChannel AddTaskChannelNew(int KDVR, int schedule);
        bool InsertImage(tbl_HM_Images image);
        IEnumerable<tbl_HM_Images> GetImageExist(int KDVR, int scheduleType, DateTime dateImage, DateTime StartW = new DateTime(), DateTime EndW = new DateTime());
        void UpdateImage(tbl_HM_Images item);
        bool SaveImage();
        IEnumerable<tbl_HM_TaskChannel> GetTaskChannel(int userID, int kDVR);
        IEnumerable<tbl_HM_ScheduledTasks> GetScheduledTasks(int userID, int schedule);
        IEnumerable<tbl_HM_Images> GetImagesFromDate(DateTime sDate, DateTime eDate, int userID);
        tbl_HM_ScheduleTypes GetScheduleType(int schedule);
        tbl_HM_Images GetImagebyID(int img);

        IEnumerable<tbl_HM_ScheduledTasks> GetScheduledTasksNoUpload(int userID, int ScheduleType);
        IEnumerable<tDVRChannels> GetChannel(List<int> kDVRs, List<int> ChannelNos);
        bool InsertScheulde(tbl_HM_ScheduledTasks newScheulde);
        bool UpdateScheulde(tbl_HM_ScheduledTasks updateScheulde);
    }
}

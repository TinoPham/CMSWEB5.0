using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;
using System.Data.SqlClient;
using System.Data;
using CMSWebApi.DataModels;
using System.Linq.Expressions;
using CMSWebApi.Utils;

namespace CMSWebApi.DataServices
{
	public class DistributionService : ServiceBase, IDistributionService
	{

		public DistributionService(IResposity model) : base(model) { }

		public DistributionService(ServiceBase ServiceBase) : base(ServiceBase.DBModel) { }

		public Task<List<Func_BAM_TrafficCountReportMonthly_Result>> GetTrafficCount(string strPACID, DateTime From, DateTime To, int userID)
		{
			List<SqlParameter> pram = POSConversionParams(strPACID, From, To, userID);
			string proc = Format_SqlCommand(SQLFunctions.Func_BAM_TrafficCountReportMonthly, pram);
			Task<List<Func_BAM_TrafficCountReportMonthly_Result>> result = DBModel.ExecWithStoreProcedureAsync<Func_BAM_TrafficCountReportMonthly_Result>(proc, pram.ToArray());
			return result;
		}

		public Task<List<Func_BAM_TrafficCountReportHourly_Result>> GetTrafficCountHourly(string strPACID, DateTime From, DateTime To, int userID)
		{
			List<SqlParameter> pram = POSConversionParams(strPACID, From, To, userID);
			string proc = Format_SqlCommand(SQLFunctions.Func_BAM_TrafficCountReportHourly, pram);
			Task<List<Func_BAM_TrafficCountReportHourly_Result>> result = DBModel.ExecWithStoreProcedureAsync<Func_BAM_TrafficCountReportHourly_Result>(proc, pram.ToArray());
			return result;
		}

		private List<SqlParameter> POSConversionParams(string strPACID, DateTime From, DateTime To, int userID)
		{
			List<SqlParameter> pram = new List<SqlParameter>();
			pram.Add(new SqlParameter("FromDate", SqlDbType.DateTime) { Value = From.Date, Direction = ParameterDirection.Input });
			pram.Add(new SqlParameter("ToDate", SqlDbType.DateTime) { Value = To, Direction = ParameterDirection.Input });
			pram.Add(new SqlParameter("PACIDs", SqlDbType.VarChar, 0) { Value = strPACID, Direction = ParameterDirection.Input });
			if (userID > 0)
				pram.Add(new SqlParameter("UserID", SqlDbType.Int) { Value = userID, Direction = ParameterDirection.Input });

			return pram;
		}

		public Task<List<TrafficCountRegionInQueue_Result>> GetTrafficCountRegionInQueue(string strPACID, int userID)
		{
			List<SqlParameter> pram = TrafficCountRegionInQueueParams(strPACID, userID);
			string proc = string.Format(SQLProceduces.TrafficCountRegionInQueue, pram.Select(p => p.ParameterName).ToArray());
			Task<List<TrafficCountRegionInQueue_Result>> result = DBModel.ExecWithStoreProcedureAsync<TrafficCountRegionInQueue_Result>(proc, pram.ToArray());
			return result;
		}
		private List<SqlParameter> TrafficCountRegionInQueueParams(string strPACID, int userID)
		{
			List<SqlParameter> pram = new List<SqlParameter>();
			pram.Add(new SqlParameter("PACIDs", SqlDbType.VarChar, 0) { Value = strPACID, Direction = ParameterDirection.Input });
			pram.Add(new SqlParameter("UserID", SqlDbType.Int) { Value = userID, Direction = ParameterDirection.Input });
			return pram;
		}
		public tbl_IOPC_QueueList AddQueue(tbl_IOPC_QueueList queue)
		{
			DBModel.Insert<tbl_IOPC_QueueList>(queue);
			return DBModel.Save() > 0 ? queue : null;
		}


		public void DeleteQueue(tbl_IOPC_QueueList queue)
		{

			DBModel.Include<tbl_IOPC_QueueList, tbl_IOPC_TrafficCountRegion>(queue, s => s.tbl_IOPC_TrafficCountRegion);
			DBModel.DeleteItemRelation<tbl_IOPC_QueueList, tbl_IOPC_TrafficCountRegion>(queue, s => s.tbl_IOPC_TrafficCountRegion, queue.tbl_IOPC_TrafficCountRegion.ToArray());
			DBModel.Delete(queue);
			DBModel.Save();
		}

		public List<tbl_IOPC_TrafficCountRegion> SelectRegion(List<int> regIDs)
		{
			List<tbl_IOPC_TrafficCountRegion> lsRegions = DBModel.Query<tbl_IOPC_TrafficCountRegion>(i => regIDs.Contains(i.RegionIndex)).ToList();
			return lsRegions;
		}
		public List<tbl_IOPC_TrafficCountRegion> SelectRegionByNameId(List<int> regIDs, List<int> pacIDs)
		{
			List<tbl_IOPC_TrafficCountRegion> lsRegions = DBModel.Query<tbl_IOPC_TrafficCountRegion>(i => regIDs.Contains(i.RegionNameID) && pacIDs.Contains(i.T_PACID)).ToList();
			return lsRegions;
		}
		public tbl_IOPC_QueueList SelectQueue(int userID, int queueId)
		{
			tbl_IOPC_QueueList Queues = DBModel.Query<tbl_IOPC_QueueList>(i => userID == i.UserID && queueId == i.QueueID).FirstOrDefault();
			return Queues;
		}
		public IEnumerable<tbl_IOPC_QueueList> SelectQueueByRegionIndex(int userID, List<int> RegionIndex, List<int> queue)
		{
			IEnumerable<tbl_IOPC_QueueList> Queues = DBModel.Query<tbl_IOPC_QueueList>(i => userID == i.UserID && i.tbl_IOPC_TrafficCountRegion.Select(x => x.RegionIndex).Any(x => RegionIndex.Contains(x)) && queue.Contains(i.QueueID));
			return Queues;
		}
		public void Modifyrelation<T, TK>(tbl_IOPC_QueueList dbsite, IEnumerable<T> current, IEnumerable<T> news, Func<T, TK> key, Expression<Func<tbl_IOPC_QueueList, object>> properties) where T : class
		{
			ModifyDataRelation<tbl_IOPC_QueueList, T, TK>(dbsite, current, news, key, properties);
		}

		public int UpdateRegion(tbl_IOPC_QueueList Queue)
		{
			DBModel.Update<tbl_IOPC_QueueList>(Queue);
			return DBModel.Save();
		}
		private List<SqlParameter> BAM_QueueByRegionIndexParams(int UserID, string RegionIndexs, string QueueIDs)
		{
			List<SqlParameter> pram = new List<SqlParameter>();
			pram.Add(new SqlParameter("UserID", SqlDbType.Int, 0) { Value = UserID, Direction = ParameterDirection.Input });
			pram.Add(new SqlParameter("RegionIndexs", SqlDbType.VarChar) { Value = RegionIndexs, Direction = ParameterDirection.Input });
			pram.Add(new SqlParameter("QueueIDs", SqlDbType.VarChar) { Value = QueueIDs, Direction = ParameterDirection.Input });
			return pram;
		}
		public Task<List<Proc_BAM_QueueByRegionIndex_Result>> GetBAM_QueueByRegionIndex(int UserID, string RegionIndexs, string QueueIDs)
		{
			List<SqlParameter> pram = BAM_QueueByRegionIndexParams(UserID, RegionIndexs, QueueIDs);
			string proc = string.Format(SQLProceduces.Proc_BAM_QueueByRegionIndex, pram.Select(p => p.ParameterName).ToArray());
			Task<List<Proc_BAM_QueueByRegionIndex_Result>> result = DBModel.ExecWithStoreProcedureAsync<Proc_BAM_QueueByRegionIndex_Result>(proc, pram.ToArray());
			return result;
		}
		public IEnumerable<tbl_IOPC_QueueList> SelectQueueDeleteAll(int userID, List<int> exQueueIDs)
		{
			IEnumerable<tbl_IOPC_QueueList> Queues;
			if(exQueueIDs.Count==0)
			{
				Queues = DBModel.Query<tbl_IOPC_QueueList>(i => userID == i.UserID);
				return Queues;
			}
			Queues = DBModel.Query<tbl_IOPC_QueueList>(i => userID == i.UserID && !exQueueIDs.Contains(i.QueueID));
			return Queues;
		}

        public IEnumerable<tDVRChannels> GetListChannels(int UserID, int KDVR) 
        {
            IEnumerable<tDVRChannels> channels = DBModel.Query<tDVRChannels>(i => i.Enable == 1 && i.KDVR == KDVR);
            return channels;
        }

        public IQueryable<Tout> GetImages<Tout>(int kDVR, Expression<Func<tbl_HM_Images, Tout>> selector, string[] includes) where Tout : class
        {
            //return Query<tbl_HM_Images, Tout>(img => img.tbl_HM_TaskChannel.KChannel == kDVR, selector, includes);
			return Query<tbl_HM_Images, Tout>(img => img.tbl_HM_TaskChannel.tDVRChannels.KDVR == kDVR, selector, includes);
        }

        public bool CheckExistImage(int KDVR, int scheduleType, DateTime dateImage, DateTime StartW = new DateTime(), DateTime EndW = new DateTime())
        {
			//var taskChannel = DBModel.Query<tbl_HM_TaskChannel>(i => i.KChannel == KDVR && i.tbl_HM_ScheduledTasks.ScheduleType == scheduleType).Select(s => s.ProcessID).ToList();
			var taskChannel = DBModel.Query<tbl_HM_TaskChannel>(i => i.tDVRChannels.KDVR == KDVR && i.tbl_HM_ScheduledTasks.ScheduleType == scheduleType).Select(s => s.ProcessID).ToList();
            if (taskChannel.Any()) {
                IEnumerable<tbl_HM_Images> image;
                if (scheduleType == 1)
                {
                    image = DBModel.Query<tbl_HM_Images>(i => taskChannel.Contains(i.ProcessID.Value)
                                                                                        && DateTime.Compare(i.UploadedDate.Value, dateImage) == 0
                                                                                        && i.UploadedDate.Value.Hour == dateImage.Hour).ToList();
                }
                else if (scheduleType == 2)
                {
                    image = DBModel.Query<tbl_HM_Images>(i => taskChannel.Contains(i.ProcessID.Value)
                                                                                        && DateTime.Compare(i.UploadedDate.Value, dateImage) == 0).ToList();
                }
                else
                {
                    image = DBModel.Query<tbl_HM_Images>(i => taskChannel.Contains(i.ProcessID.Value)
                                                              && StartW <= i.UploadedDate.Value && i.UploadedDate.Value <= EndW).ToList();
                }
                
                if (image.Any())
                {
                    return true;
                }
            }
            return false;
        }

        public IEnumerable<tbl_HM_Images> GetImageExist(int KDVR, int scheduleType, DateTime dateImage, DateTime StartW = new DateTime(), DateTime EndW = new DateTime())
        {
			//var taskChannel = DBModel.Query<tbl_HM_TaskChannel>(i => i.KChannel == KDVR && i.tbl_HM_ScheduledTasks.ScheduleType == scheduleType).Select(s => s.ProcessID).ToList();
			var taskChannel = DBModel.Query<tbl_HM_TaskChannel>(i => i.tDVRChannels.KDVR == KDVR && i.tbl_HM_ScheduledTasks.ScheduleType == scheduleType).Select(s => s.ProcessID).ToList();
            if (taskChannel.Any())
            {
                IEnumerable<tbl_HM_Images> image;
                if (scheduleType == 1)
                {
                    image = DBModel.Query<tbl_HM_Images>(i => taskChannel.Contains(i.ProcessID.Value)
                                                                                        && DateTime.Compare(i.UploadedDate.Value, dateImage) == 0
                                                                                        && i.UploadedDate.Value.Hour == dateImage.Hour).ToList();
                }
                else if (scheduleType == 2)
                {
                    image = DBModel.Query<tbl_HM_Images>(i => taskChannel.Contains(i.ProcessID.Value)
                                                                                        && DateTime.Compare(i.UploadedDate.Value, dateImage) == 0).ToList();
                }
                else
                {
                    image = DBModel.Query<tbl_HM_Images>(i => taskChannel.Contains(i.ProcessID.Value)
                                                              && StartW <= i.UploadedDate.Value && i.UploadedDate.Value <= EndW).ToList();
                }

                if (image.Any())
                {
                    return image;
                }
            }
            return null;
        }

        public tbl_HM_TaskChannel AddTaskChannelNew(int KChan, int schedule)
        {
			var taskChannel = DBModel.Query<tbl_HM_TaskChannel>(i => i.KChannel == KChan && i.tbl_HM_ScheduledTasks.ScheduleType == schedule).ToList();
            if (taskChannel.Any())
            {
                return taskChannel.FirstOrDefault();
            }
			else
			{
                int TaskID = GetIDScheduleTasks(schedule);
                if (TaskID > 0)
                {
                    tbl_HM_TaskChannel taskChanel = new tbl_HM_TaskChannel()
                    {
                        TaskID = TaskID,
						KChannel = KChan
                    };
                    DBModel.Insert<tbl_HM_TaskChannel>(taskChanel);
                    return DBModel.Save() > 0 ? taskChanel : null;
                }
                return null;
            }
        }

        private int GetIDScheduleTasks(int schedule)
        {
            return DBModel.Query<tbl_HM_ScheduledTasks>(i => (int)i.ScheduleType == schedule && i.Status == (int)StatusSchedule.UPLOAD).FirstOrDefault().TaskID;
        }

        public bool InsertImage(tbl_HM_Images image)
        {
            DBModel.Insert<tbl_HM_Images>(image);
            return DBModel.Save() > 0;
        }

        public void UpdateImage(tbl_HM_Images item)
        {
            DBModel.Update(item);
        }

        public bool SaveImage()
        {
            return DBModel.Save() > 0;
        }

        public IEnumerable<tbl_HM_TaskChannel> GetTaskChannel(int userID, int kDVR)
        {
            var lKChannel = GetListChannels(userID, kDVR).Select(s => s.KChannel).ToList();
            return DBModel.Query<tbl_HM_TaskChannel>(i => lKChannel.Contains(i.KChannel.Value));
        }

        public IEnumerable<tbl_HM_ScheduledTasks> GetScheduledTasks(int userID, int schedule)
        {
            return DBModel.Query<tbl_HM_ScheduledTasks>(i => i.ScheduleType == schedule && (i.CreatedBy.Value == userID || i.Status.Value == (int)StatusSchedule.UPLOAD));
        }

        public IEnumerable<tbl_HM_Images> GetImagesFromDate(DateTime sDate, DateTime eDate, int userID)
        {
            //return DBModel.Query<tbl_HM_Images>(i => i.UploadedBy == userID && (i.UploadedDate.Value >= sDate && i.UploadedDate.Value <= eDate));
            return DBModel.Query<tbl_HM_Images>(i => i.UploadedDate.Value >= sDate && i.UploadedDate.Value <= eDate);
        }

        public tbl_HM_ScheduleTypes GetScheduleType(int schedule)
        {
            return DBModel.Query<tbl_HM_ScheduleTypes>(i => i.TypeID == schedule).FirstOrDefault();
        }

		public tbl_HM_Images GetImagebyID(int img)
		{
			var includes = new string[]
			{
				typeof (tbl_HM_TaskChannel).Name,
				string.Format("{0}.{1}", typeof (tbl_HM_TaskChannel).Name, typeof (tbl_HM_ScheduledTasks).Name),
				string.Format("{0}.{1}", typeof (tbl_HM_TaskChannel).Name, typeof (tDVRChannels).Name)
			};
			return DBModel.Query<tbl_HM_Images>(i => i.ImgID == img, includes).FirstOrDefault();
		}

        public IEnumerable<tbl_HM_ScheduledTasks> GetScheduledTasksNoUpload(int userID, int ScheduleType)
        {
            var includes = new string[]
            {
                typeof (tbl_HM_TaskChannel).Name,
                string.Format("{0}.{1}.{2}", typeof(tbl_HM_TaskChannel).Name, typeof(tDVRChannels).Name, typeof(tCMSWebSites).Name)
            };

            return DBModel.Query<tbl_HM_ScheduledTasks>(i => i.CreatedBy == userID && i.ScheduleType == ScheduleType && i.Status == (int)StatusSchedule.ACTIVE, includes);
        }

        public IEnumerable<tDVRChannels> GetChannel(List<int> kDVRs, List<int> ChannelNos)
        {
            return DBModel.Query<tDVRChannels>(i => kDVRs.Contains(i.KDVR) && ChannelNos.Contains(i.ChannelNo));
        }

        public bool InsertScheulde(tbl_HM_ScheduledTasks newScheulde)
        {
            DBModel.Insert<tbl_HM_ScheduledTasks>(newScheulde);
            return DBModel.Save() > 0;
        }

        public bool UpdateScheulde(tbl_HM_ScheduledTasks updateScheulde)
        {
            DBModel.Update<tbl_HM_ScheduledTasks>(updateScheulde);
            return DBModel.Save() > 0;
        }
	}
}

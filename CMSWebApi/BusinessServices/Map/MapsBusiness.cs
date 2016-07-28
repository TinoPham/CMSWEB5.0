using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.APIFilters;
using CMSWebApi.BusinessServices.FilesManager;
using CMSWebApi.DataModels;
using CMSWebApi.ServiceInterfaces;
using CMSWebApi.Utils;
using PACDMModel.Model;
using System.Drawing.Imaging;
using System.Drawing;
using System.Net.Http;
using System.Web;

namespace CMSWebApi.BusinessServices.Map
{
	public class MapsBusiness : BusinessBase<IMapsService>
	{
		public ISiteService SiteService { get; set; }
		//private IFilesManager _fileManager;

		private MapsImage EntityToModel(tCMSWebSiteImage entity)
		{
			if (entity == null) return new MapsImage();
			MapsImage model = new MapsImage();
			model.Caption = entity.ImageSite;
			model.Title =entity.Title;
			model.UpdatedDate = entity.UpdatedDate.HasValue?entity.UpdatedDate.Value:DateTime.Now;
			model.Createdby = entity.CreatedBy.HasValue?entity.CreatedBy.Value: 0;
			model.ImageURL = "";
			model.ImageID = entity.ImageID;
			if (entity.tCMSWebSiteMapAreas != null)
			{
				if (entity.tCMSWebSites !=null)
				{
					if (entity.tCMSWebSites.tDVRChannels != null)
					{
						if (model.Channels == null) model.Channels = new List<ChannelsPosition>();
						entity.tCMSWebSiteMapAreas.ToList().ForEach(item => model.Channels.Add(new ChannelsPosition()
						{
							ChannelID = item.ChannelID.Value,
							ChannelName = entity.tCMSWebSites.tDVRChannels.Where(ch => ch.KChannel == item.ChannelID.Value).FirstOrDefault().Name,
							Leftpoint = (float)item.LeftPoint.Value,
							Status = entity.tCMSWebSites.tDVRChannels.Where(ch => ch.KChannel == item.ChannelID.Value).FirstOrDefault().Status.HasValue ? (byte)entity.tCMSWebSites.tDVRChannels.Where(ch => ch.KChannel == item.ChannelID.Value).FirstOrDefault().Status.Value : (byte)0,
							Toppoint = (float)item.TopPoint.Value,
						}));
					}
				}
			}
			return model;
		}

		public  MapsModel Gets(int sitekey)
		{
			string path = Path.Combine(AppSettings.AppSettings.Instance.SitesPath, sitekey.ToString(), Consts.MAP_IMAGES_FOLDER);

			MapsModel model = new MapsModel();
			model.mapImage = new List<MapsImage>();
			var includes = new string[]
				{
					typeof (tCMSWebSites).Name,
					typeof (tCMSWebSiteMapAreas).Name,
					string.Format("{0}.{1}", typeof (tCMSWebSites).Name, typeof (tDVRChannels).Name),
					string.Format("{0}.{1}.{2}", typeof (tCMSWebSites).Name, typeof (tDVRChannels).Name, typeof (tDVRAddressBook).Name)
				};
			List<tCMSWebSiteImage> img = DataService.GetImages<tCMSWebSiteImage>(sitekey, image => image,includes).ToList();

			img.ForEach(item=>model.mapImage.Add(EntityToModel(item)));
			model.siteKey = sitekey;
			return model;
		}

		public  async Task<MemoryStream> ReadImage(int sitekey,string filename)
		{
			MemoryStream result = new MemoryStream();
			string path = Path.Combine(AppSettings.AppSettings.Instance.SitesPath, sitekey.ToString(), Consts.MAP_IMAGES_FOLDER);
			filename = Path.Combine(path, filename);
			await Task.Factory.StartNew(() =>
			{
				FileStream file = new FileStream(path, FileMode.OpenOrCreate);
				file.CopyToAsync(result);
				file.Close();
			});
			return  result;
			
			
		}

		public byte[] MakeThumbnail(/*byte[] myImage*/ string myImage, int thumbWidth, int thumbHeight)
		{
			int newWidth, newHeight;
			using (MemoryStream ms = new MemoryStream())
			{
				
				//using (Image thumbnail = Image.FromStream(new MemoryStream(myImage)))
				using (Image thumbnail = Image.FromFile(myImage))
				{
					float aspect = thumbnail.Width / (float)thumbnail.Height;

					//calculate new dimensions based on aspect ratio
					newWidth = (int)(thumbWidth * aspect);
					newHeight = (int)(newWidth / aspect);
					//if one of the two dimensions exceed the box dimensions
					if (newWidth > thumbWidth || newHeight > thumbHeight)
					{
						
						if (newWidth > newHeight)
						{
							newWidth = thumbWidth;
							newHeight = (int)(newWidth / aspect);

						}
						else
						{
							newHeight = thumbHeight;
							newWidth = (int)(newHeight * aspect);

						}
					}
					using (Image small = thumbnail.GetThumbnailImage(newWidth, newHeight, null, new IntPtr()))
					{
						small.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
						return ms.ToArray();
					}


				}
			}
		}

		public async Task<TransactionalModel<MapsModel>> Sets(MapsModel Models,int userID)
		{
				List<tCMSWebSiteImage> maps = DataService.GetImages(Models.siteKey, item => item, null).ToList();
			var t = Models.mapImage.Select(item => item.ImageID).ToList();
			var x = maps.Select(item => item.ImageID).ToList();
			var r = x.Where(item => t.Contains(item) == false);
			var delete = maps.Where(item => r.Contains(item.ImageID)).ToList();
			string mapPath = Path.Combine(AppSettings.AppSettings.Instance.SitesPath, Models.siteKey.ToString(), Consts.MAP_IMAGES_FOLDER);
			for (int i = 0; i < delete.Count(); i++)
			{
				var item = delete.ElementAt(i);
				string imageMapPath = Path.Combine(mapPath, item.ImageSite);
				if (File.Exists(imageMapPath))
				{
					await FileManager.FileDeleteAsync(imageMapPath);
				}

				imageMapPath = Path.Combine(mapPath, Consts.THUMBNAIL_FOLDER, item.ImageSite); // get map image thumbnail path
				if (File.Exists(imageMapPath))
				{
					await FileManager.FileDeleteAsync(imageMapPath);
			    }
			}
			return Adds(Models, userID);
		}

		public TransactionalModel<MapsModel> Adds(MapsModel Models, int userID)
		{
			try
			{
				var includes = new string[]
				{
					typeof (tCMSWebSiteMapAreas).Name,
				};
				ICollection<tCMSWebSiteImage> area = DataService.GetImages<tCMSWebSiteImage>(Models.siteKey, a => a, includes).ToList();

				IEnumerable<MapsImage> UpdateList = Models.mapImage.Where(item => area.Where(a => a.ImageID == item.ImageID).Any());
				IEnumerable<MapsImage> InsertList = Models.mapImage.Where(item => item.ImageID <= 0);
				List<tCMSWebSiteImage> DeleteList = new List<tCMSWebSiteImage>();
				var t = Models.mapImage.Select(item => item.ImageID).ToList();
				var x = area.Select(item => item.ImageID).ToList();
				var r = x.Where(item => t.Contains(item) == false);
				var delete = area.Where(item => r.Contains(item.ImageID)).ToList();

				if (InsertList.Any())
				{
					Inserts(InsertList, Models.siteKey, userID).ToList().ForEach(item => DataService.Insert(item));
				}
				if (UpdateList.Any())
				{
					Updates(UpdateList, Models.siteKey, userID).ToList().ForEach(item => DataService.Update(item));
				}
				if (delete.Any())
				{
					delete.ToList().ForEach(item => {DataService.Delete(item); });
				}
				TransactionalModel<MapsModel> result = new TransactionalModel<MapsModel>();
				if (DataService.Save())
				{
					result.ReturnMessage.Add("Success.");
				}
				else
				{
					result.ReturnMessage.Add("Fail.");
				}
				result.Data = Gets(Models.siteKey);
				result.ReturnMessage = new List<string>();
				return result;
			}
			catch (Exception ex)
			{
				TransactionalModel<MapsModel> result = new TransactionalModel<MapsModel>();
				result.Data = null;
				result.ReturnMessage = new List<string>();
				result.ReturnMessage.Add("Exception");
				return result;
			}
		}

		private tCMSWebSiteMapAreas Insert(ChannelsPosition channel, int ImageID)
			{
			tCMSWebSiteMapAreas Area = new tCMSWebSiteMapAreas();
			if (ImageID > 0)
				Area.ImageID = ImageID;
			else
				Area.ImageID = null;
			Area.ChannelID = channel.ChannelID;
			Area.LeftPoint = channel.Leftpoint;
			Area.TopPoint = channel.Toppoint;
			return Area;
		}

		private tCMSWebSiteMapAreas Update(ChannelsPosition channel, int ImageID)
				{
			tCMSWebSiteMapAreas Area = DataService.GetChannels<tCMSWebSiteMapAreas>(ImageID,item=>item,null).FirstOrDefault(item=>item.ChannelID == channel.ChannelID);
			if (Area == null) return Insert(channel, ImageID);
			if (ImageID > 0)
				Area.ImageID = ImageID;
			else
				Area.ImageID = null;
			Area.LeftPoint = channel.Leftpoint;
			Area.TopPoint = channel.Toppoint;
			return Area;
		}

		private IEnumerable<tCMSWebSiteMapAreas> Inserts(IEnumerable<ChannelsPosition> Channels, int ImageID)
		{
			IEnumerable<tCMSWebSiteMapAreas> Areas = Channels.Select(item => Insert(item, ImageID));
			return Areas;
		}

		private IEnumerable<tCMSWebSiteMapAreas> Updates(IEnumerable<ChannelsPosition> Channels, int ImageID)
		{
			List<tCMSWebSiteMapAreas> c_list = DataService.GetChannels<tCMSWebSiteMapAreas>(ImageID, item => item, null).ToList();
			var t = Channels.Select(item => item.ChannelID).ToList();
			var x = c_list.Select(item => item.ChannelID).ToList();
			var r = x.Where(item => t.Contains(item.Value) == false);
			var delete = c_list.Where(item => r.Contains(item.ChannelID)).ToList();
			if (delete.Any())
			{
				delete.ForEach(item => DataService.Delete(item));
				}
			IEnumerable<tCMSWebSiteMapAreas> Areas = Channels.Select(item => Update(item, ImageID));
			return Areas;
		}

		private tCMSWebSiteImage Insert(MapsImage InsertItem, int SiteKey, int userId = 0)
		{
			string mapThumbnailPath = Path.Combine(AppSettings.AppSettings.Instance.SitesPath, SiteKey.ToString(), Consts.MAP_IMAGES_FOLDER, Utils.Consts.THUMBNAIL_FOLDER);
			string mapThumbnailImagePath = Path.Combine(mapThumbnailPath, InsertItem.Caption);
			string mapImagePath = Path.Combine(AppSettings.AppSettings.Instance.SitesPath, SiteKey.ToString(),Consts.MAP_IMAGES_FOLDER, InsertItem.Caption);
			if (!Directory.Exists(mapThumbnailPath))
			{
				Directory.CreateDirectory(mapThumbnailPath);
			}

			if (!File.Exists(mapThumbnailImagePath))
			{
				if (File.Exists(mapImagePath)) //If map image uploaded, upload thumbnail image.
				{
					FileManager.FileWrite(mapThumbnailImagePath, MakeThumbnail(mapImagePath, Utils.Consts.THUMNAIL_WIDTH, Utils.Consts.THUMNAIL_HEIGHT));
				}
			}
			tCMSWebSiteImage NewMap = new tCMSWebSiteImage();
			NewMap.ImageSite = InsertItem.Caption;
			NewMap.siteKey = SiteKey;
			NewMap.Title = InsertItem.Title;
			NewMap.UpdatedDate = DateTime.UtcNow;
			NewMap.CreatedBy = userId;
			NewMap.tCMSWebSiteMapAreas = Inserts(InsertItem.Channels.Select(item => item), InsertItem.ImageID).ToList();
			return NewMap;
		}

		private tCMSWebSiteImage Update(MapsImage UpdateItem, int SiteKey, int userID)
		{
			tCMSWebSiteImage NewMap = DataService.GetImages<tCMSWebSiteImage>(SiteKey,item=>item,null).FirstOrDefault(item=>item.ImageID == UpdateItem.ImageID);
			if (NewMap == null)
			{
				return Insert(UpdateItem, SiteKey, userID);
			}
			NewMap.ImageID = UpdateItem.ImageID;
			NewMap.ImageSite = UpdateItem.Caption;
			NewMap.siteKey = SiteKey;
			NewMap.CreatedBy = userID;
			NewMap.UpdatedDate = DateTime.UtcNow;
			NewMap.Title = UpdateItem.Title;
			if(UpdateItem.Channels != null)
				NewMap.tCMSWebSiteMapAreas = Updates(UpdateItem.Channels.Select(item => item), UpdateItem.ImageID).ToList();
			return NewMap;
		}

		private IEnumerable<tCMSWebSiteImage> Inserts(IEnumerable<MapsImage> InsertList, int SiteKey, int userID = 0)
		{
			IEnumerable<tCMSWebSiteImage> Maps = InsertList.Select(item => Insert(item, SiteKey, userID));
			return Maps;
			} 

		private IEnumerable<tCMSWebSiteImage> Updates(IEnumerable<MapsImage> UpdateList, int SiteKey, int userID)
		{
			IEnumerable<tCMSWebSiteImage> Maps = UpdateList.Select(item => Update(item, SiteKey, userID));
			return Maps;
		}

		public TransactionalModel<MapsImage> Upload(int sitekey, int id, HttpFileCollection filesCollection)
		{
			try
			{
				var fileName = filesCollection[0];
				string path = Path.Combine(AppSettings.AppSettings.Instance.SitesPath, sitekey.ToString(), Consts.MAP_IMAGES_FOLDER);
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}
				string ext = Path.GetExtension(fileName.FileName);
				string newName = System.Guid.NewGuid() + ext;
				string filePath = Path.Combine(path, newName);
				fileName.SaveAs(filePath);
				MapsImage map = new MapsImage();
				map.ImageURL = newName;
				map.ImageID = id;
				TransactionalModel<MapsImage> result = new TransactionalModel<MapsImage>();
				result.Data = map;
				return result;
			}
			catch (Exception e)
			{
				return null;
			}
		}

        public TransactionalModel<MapsImage> UploadFromDialog(int sitekey, int id, HttpFileCollection filesCollection)
        {
            try
            {
                var fileName = filesCollection[0];
                string path = Path.Combine(AppSettings.AppSettings.Instance.SitesPath, sitekey.ToString(), Consts.MAP_IMAGES_FOLDER);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string ext = Path.GetExtension(fileName.FileName);
                string newName = System.Guid.NewGuid() + ext;
                string filePath = Path.Combine(path, newName);
                fileName.SaveAs(filePath);

                MapsImage map = new MapsImage();
                map.ImageURL = newName;
                map.ImageID = id;
                TransactionalModel<MapsImage> result = new TransactionalModel<MapsImage>();
                result.Data = map;
                return result;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public Task<TransactionalModel<MapsModel>> InsertModelFromDialog(MapsModel Models, int userID)
        {
            try
            {
                string thumbnailPath = Path.Combine(AppSettings.AppSettings.Instance.SitesPath, Models.siteKey.ToString(), Consts.MAP_IMAGES_FOLDER, Utils.Consts.THUMBNAIL_FOLDER);
                string thumbnailImagePath = Path.Combine(thumbnailPath, Models.mapImage[0].Caption);
                string imagePath = Path.Combine(AppSettings.AppSettings.Instance.SitesPath, Models.siteKey.ToString(), Consts.MAP_IMAGES_FOLDER, Models.mapImage[0].Caption);
                if (!Directory.Exists(thumbnailPath))
                {
                    Directory.CreateDirectory(thumbnailPath);
                }

                if (!File.Exists(thumbnailImagePath))
                {
                    if (File.Exists(imagePath)) //If map image uploaded, upload thumbnail image.
                    {
                        FileManager.FileWrite(thumbnailImagePath, MakeThumbnail(imagePath, Utils.Consts.THUMNAIL_WIDTH, Utils.Consts.THUMNAIL_HEIGHT));
                    }
                }

                tCMSWebSiteImage NewMap = new tCMSWebSiteImage();
                NewMap.ImageSite = Models.mapImage[0].Caption;
                NewMap.siteKey = Models.siteKey;
                NewMap.Title = Models.mapImage[0].Title;
                NewMap.UpdatedDate = DateTime.UtcNow;
                NewMap.CreatedBy = userID;

                DataService.Insert(NewMap);

                TransactionalModel<MapsModel> result = new TransactionalModel<MapsModel>();
                if (DataService.Save())
                {
                    result.ReturnMessage.Add("Success.");
                }
                else
                {
                    result.ReturnMessage.Add("Fail.");
                }
                result.Data = Gets(Models.siteKey);
                result.ReturnMessage = new List<string>();
				return Task.FromResult < TransactionalModel < MapsModel >>( result);

            }
            catch (Exception ex)
            {
                TransactionalModel<MapsModel> result = new TransactionalModel<MapsModel>();
                result.Data = null;
                result.ReturnMessage = new List<string>();
                result.ReturnMessage.Add("Exception");
                //return result;
				return Task.FromResult<TransactionalModel<MapsModel>>(result);
            }
        }

        public async Task<TransactionalModel<MapsModel>> DeleteModelFromButtonX(MapsModel Models, int userID)
        {
            try
            {
                List<tCMSWebSiteImage> maps = DataService.GetImages(Models.siteKey, item => item, null).ToList();
                var t = Models.mapImage.Select(item => item.ImageID).ToList();
                var x = maps.Select(item => item.ImageID).ToList();
                var r = x.Where(item => t.Contains(item) == true);
                var deleteMap = maps.Where(item => r.Contains(item.ImageID)).ToList();

                string mapPath = Path.Combine(AppSettings.AppSettings.Instance.SitesPath, Models.siteKey.ToString(), Consts.MAP_IMAGES_FOLDER);
                for (int i = 0; i < deleteMap.Count(); i++)
                {
                    var item = deleteMap.ElementAt(i);

                    //List Channels of Map
                    var deleteChannels = DataService.GetChannels(item.ImageID, itm => itm, null).ToList();

                    if (deleteChannels.Any())
                    {
                        deleteChannels.ToList().ForEach(itm => { DataService.Delete(itm); });
                    }

                    string imageMapPath = Path.Combine(mapPath, item.ImageSite);
                    if (File.Exists(imageMapPath))
                    {
                        await FileManager.FileDeleteAsync(imageMapPath);
                    }

                    imageMapPath = Path.Combine(mapPath, Consts.THUMBNAIL_FOLDER, item.ImageSite); // get map image thumbnail path
                    if (File.Exists(imageMapPath))
                    {
                        await FileManager.FileDeleteAsync(imageMapPath);
                    }
                }

                if (deleteMap.Any())
                {
                    deleteMap.ToList().ForEach(item => { DataService.Delete(item); });
                }

                TransactionalModel<MapsModel> result = new TransactionalModel<MapsModel>();
                if (DataService.Save())
                {
                    result.ReturnMessage.Add("Success.");
                }
                else
                {
                    result.ReturnMessage.Add("Fail.");
                }
                result.Data = Gets(Models.siteKey);
                result.ReturnMessage = new List<string>();
                return result;

            }
            catch (Exception ex)
            {
                TransactionalModel<MapsModel> result = new TransactionalModel<MapsModel>();
                result.Data = null;
                result.ReturnMessage = new List<string>();
                result.ReturnMessage.Add("Exception");
                return result;
            }
        }

		public string GetMapImages(int siteKey, string fileName, bool isThumbnail)
		{
			string path = Path.Combine(AppSettings.AppSettings.Instance.SitesPath, siteKey.ToString(), Utils.Consts.MAP_IMAGES_FOLDER);
			if (isThumbnail)
			{
				path = Path.Combine(path, Utils.Consts.THUMBNAIL_FOLDER, fileName);
				return path;
			}
			path = Path.Combine(path, fileName);
			return path;
		}
	}
}

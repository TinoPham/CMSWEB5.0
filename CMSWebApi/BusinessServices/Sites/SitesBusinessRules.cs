using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.DataModels;
using PACDMModel.Model;

namespace CMSWebApi.BusinessServices.Sites
{
	internal class SitesBusinessRules : ValidationRules
	{
		public SitesBusinessRules(CultureInfo culture) : base(culture)
		{
		}

		public tCMSWebRegion ToDbRegion(tCMSWebRegion dbRegion, RegionModel region)
		{
			dbRegion.RegionKey = region.RegionKey;
			dbRegion.RegionName = region.RegionName;
			dbRegion.Description = region.Description;
			dbRegion.RegionParentID = region.RegionParentId == 0 ? null : region.RegionParentId;
			dbRegion.UserKey = region.UserKey;

			return dbRegion;
		}


		public readonly Expression<Func<tCMSWebRegion, RegionModel>> SelectDbRegionToModel = tregion => new RegionModel()
		{
			RegionKey = tregion.RegionKey,
			RegionName = tregion.RegionName,
			Description = tregion.Description,
			RegionParentId = tregion.RegionParentID,
			UserKey = tregion.UserKey
		};

		public readonly Expression<Func<tCMSWeb_WorkingHours, WorkingHours>> WorkingHoursSelectToModel =
			workingHours => new WorkingHours()
			{
				CloseTime = workingHours.CloseTime,
				OpenTime = workingHours.OpenTime,
				ScheduleId = workingHours.ScheduleID,
				SiteId = workingHours.SiteID
			};

		public void ModelToWebSite(ref tCMSWebSites dbsite, CmsSites site, List<tCMSWeb_MetricSiteList> metricList)
		{
			if (dbsite == null)
			{
				dbsite = new tCMSWebSites();
			}

			
				dbsite.siteKey= site.SiteKey;
				dbsite.ServerID= site.ServerId;
				//dbsite.MACAddress= site.MacAddress;
				dbsite.AddressLine1 = site.AddressLine1;
				dbsite.AddressLine2 = site.AddressLine2;
				dbsite.City = site.City;
				dbsite.StateProvince = site.StateProvince;
				dbsite.Country = site.Country;
				//dbsite.PostalZipCode = site.PostalZipCodeId;
				dbsite.UserID= site.UserId;
				dbsite.RegionKey = site.RegionKey;
				dbsite.ImageSite = site.ImageSite;
				//dbsite.mapX= site.MapX;
				//dbsite.mapY= site.MapY;
				dbsite.heatMapImage= site.HeatMapImage;
				dbsite.heatMapX= site.HeatMapX;
				dbsite.heatMapY= site.HeatMapY;
				dbsite.HaspkeyID= site.HaspkeyId;
				dbsite.GoalID= site.GoalId;
				dbsite.Deleted = false;
				dbsite.Syn = site.Syn;
				dbsite.LandlordCode = site.LandlordCode;
				dbsite.LandlordPhone = site.LandlordPhone;
				dbsite.MallCode = site.MallCode;
				dbsite.Address3 = site.Address3;
				dbsite.Management = site.Management;
				dbsite.MgtPhone = site.MgtPhone;
				dbsite.MgtAdress = site.MgtAdress;
				dbsite.StoreCategory = site.StoreCategory;
				dbsite.StoreType = site.StoreType;
				dbsite.StoreGroup = site.StoreGroup;
				dbsite.StoreDes = site.StoreDes;
				dbsite.FinancialIns = site.FinancialIns;
				dbsite.PolicyNo = site.PolicyNo;
				dbsite.AccountNo = site.AccountNo;
				dbsite.BranchNo = site.BranchNo;
				dbsite.Address4 = site.Address4;
				dbsite.PhoneSite = site.PhoneSite;
				dbsite.PhoneFinance = site.PhoneFinance;
				dbsite.FaxSite = site.FaxSite;
				dbsite.FaxFinance = site.FaxFinance;
				dbsite.TlogVersion = site.TlogVersion;
				dbsite.SquareFeet = site.SquareFeet;
				dbsite.StoreSellingSquareFeet = site.StoreSellingSquareFeet;
				dbsite.AnnualBudget = site.AnnualBudget;
				dbsite.ActualBudget = site.ActualBudget;
				dbsite.TotalBugetHour = site.TotalBugetHour;
				dbsite.TotalStaffHour = site.TotalStaffHour;
				dbsite.SalePerSquareFeet = site.SalePerSquareFeet;
				dbsite.TotalHourUsed = site.TotalHourUsed;
				dbsite.ManagerASM = site.ManagerAsm;
				dbsite.Layout = site.Layout;
				dbsite.FixturePlan = site.FixturePlan;
				dbsite.SecuritySystemCode = site.SecuritySystemCode;
				dbsite.InstallRemoveDate = site.InstallRemoveDate;
				dbsite.SecurityServiceCode = site.SecurityServiceCode;
				dbsite.ServiceStartDate = site.ServiceStartDate;
				dbsite.ServiceEndDate = site.ServiceEndDate;
				dbsite.OpenDate = site.OpenDate;
				dbsite.CloseDate = site.CloseDate;
				dbsite.RemodellingOpenDate = site.RemodellingOpenDate;
				dbsite.RemodellingCloseDate = site.RemodellingCloseDate;
				dbsite.LeaseStartDate = site.LeaseStartDate;
				dbsite.LeaseEndDate = site.LeaseEndDate;
				//dbsite.StoreImage = site.StoreImage;
				dbsite.AlertImage = site.AlertImage;
				//dbsite.tCMSWeb_WorkingHours = site.WorkingHours.Select(t => new tCMSWeb_WorkingHours()
				//{
				//	ScheduleID = t.ScheduleId,
				//	CloseTime = t.CloseTime,
				//	OpenTime = t.OpenTime

				//}).ToList();
				//dbsite.tCMSWeb_CalendarEvents = calendarEventses;
				//dbsite.tCMSWeb_UserList = userList;
				//dbsite.tCMSWeb_MetricSiteList = metricList;

		}

		public readonly Expression<Func<tCMSWebSites, CmsSites>> WebSiteToModel = site => new CmsSites()
		{
			SiteKey =					site.siteKey,
			ServerId =					site.ServerID,
			//MacAddress =				site.MACAddress,
			AddressLine1 =				site.AddressLine1,
			AddressLine2 =				site.AddressLine2,
			City =						site.City,
			StateProvince =				site.StateProvince,
			Country =					site.Country,
			PostalZipCode =				site.tbl_ZipCode != null? site.tbl_ZipCode.ZipCode:null,
			UserId =					site.UserID,
			RegionKey =					site.RegionKey,
			ImageSite =					site.ImageSite,
			//MapX =						site.mapX,
			//MapY =						site.mapY,
			HeatMapImage =				site.heatMapImage,
			HeatMapX =					site.heatMapX,
			HeatMapY =					site.heatMapY,
			HaspkeyId =					site.HaspkeyID,
			GoalId =					site.GoalID,
			Deleted =					site.Deleted,
			Syn =						site.Syn,
			LandlordCode =				site.LandlordCode,
			LandlordPhone =				site.LandlordPhone,
			MallCode =					site.MallCode,
			Address3 =					site.Address3,
			Management =				site.Management,
			MgtPhone =					site.MgtPhone,
			MgtAdress =					site.MgtAdress,
			StoreCategory =				site.StoreCategory,
			StoreType =					site.StoreType,
			StoreGroup =				site.StoreGroup,
			StoreDes =					site.StoreDes,
			FinancialIns =				site.FinancialIns,
			PolicyNo =					site.PolicyNo,
			AccountNo =					site.AccountNo,
			BranchNo =					site.BranchNo,
			Address4 =					site.Address4,
			PhoneSite =					site.PhoneSite,
			PhoneFinance =				site.PhoneFinance,
			FaxSite =					site.FaxSite,
			FaxFinance =				site.FaxFinance,
			TlogVersion =				site.TlogVersion,
			SquareFeet =				site.SquareFeet,
			StoreSellingSquareFeet =	site.StoreSellingSquareFeet,
			AnnualBudget =				site.AnnualBudget,
			ActualBudget =				site.ActualBudget,
			TotalBugetHour =			site.TotalBugetHour,
			TotalStaffHour =			site.TotalStaffHour,
			SalePerSquareFeet =			site.SalePerSquareFeet,
			TotalHourUsed =				site.TotalHourUsed,
			ManagerAsm =				site.ManagerASM,
			Layout =					site.Layout,
			FixturePlan =				site.FixturePlan,
			SecuritySystemCode =		site.SecuritySystemCode,
			InstallRemoveDate =			site.InstallRemoveDate,
			SecurityServiceCode =		site.SecurityServiceCode,
			ServiceStartDate =			site.ServiceStartDate,
			ServiceEndDate =			site.ServiceEndDate,
			OpenDate =					site.OpenDate,
			CloseDate =					site.CloseDate,
			RemodellingOpenDate =		site.RemodellingOpenDate,
			RemodellingCloseDate =		site.RemodellingCloseDate,
			LeaseStartDate =			site.LeaseStartDate,
			LeaseEndDate =				site.LeaseEndDate,
			//StoreImage =				site.StoreImage,
			AlertImage =				site.AlertImage,
			CalendarEvent =				site.tCMSWeb_CalendarEvents.Select(t=> t.ECalID),
			Macs =						site.tDVRChannels.Select(t=> new MAC()
			{
				Id = t.tDVRAddressBook.KDVR,
				MacAddress = t.tDVRAddressBook.DVRGuid,
				DvrName = t.tDVRAddressBook.ServerID
			}),
			DvrUsers =					site.tCMSWeb_UserList.Select(t=>t.UserID),
			WorkingHours =				site.tCMSWeb_WorkingHours.Select(t=> new WorkingHours(){ScheduleId = t.ScheduleID, SiteId = t.SiteID, OpenTime = t.OpenTime, CloseTime = t.CloseTime}),
			HaspLicense =				site.tDVRChannels.Select(s => new HaspLicense(){ KDVR = s.KDVR, SerialNumber = s.tDVRAddressBook.HaspLicense, ServerID = s.tDVRAddressBook.ServerID }),
		};

		public readonly Expression<Func<tbl_ZipCode, ZipCodeModel>> ZipCodetoModel = zipcode => new ZipCodeModel()
		{
			ZipCodeID = zipcode.ZipCodeID,
			ZipCode = zipcode.ZipCode
		};

		public readonly Expression<Func<ZipCodeModel, tbl_ZipCode>> ModeltoZipCode = zipcode => new tbl_ZipCode()
		{
			ZipCodeID = zipcode.ZipCodeID,
			ZipCode = zipcode.ZipCode
		};
	}
}

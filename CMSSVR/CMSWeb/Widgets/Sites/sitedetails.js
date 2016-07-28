(function () {
	define(['cms', 'DataServices/Configuration/commoninfo.service'], sitedetails);
	function sitedetails(cms) {
		cms.register.controller('sitedetailsCtrl', sitedetailsCtrl);
		sitedetailsCtrl.$inject = ['$scope', 'dataContext', 'cmsBase', 'AppDefine', '$timeout', '$rootScope', 'commoninfo.service'];
		function sitedetailsCtrl($scope, dataContext, cmsBase, AppDefine, $timeout, $rootScope, commoninfoSvc) {
			var isActive = false;
			var id = 0;
			$scope.provider = {};
			$scope.SiteModel = {};
			$scope.storeImages = [];
			$scope.countryName = "";
			$scope.stateName = "";
			$scope.urlForDownloadSite = AppDefine.Api.Sites + AppDefine.SiteAPI.GET_FILE;
			$scope.init = function (model) {
				$scope.provider = model;
			}

			$scope.$on(AppDefine.Events.SITE_TAPS.CHANGE_NODE_TREE, function (event, agr) {
				if (id != agr.ID) {
					$scope.provider = agr;
				}
				if (isActive == true) {
					getSite(agr.ID);
					id = agr.ID;
				}
			});

			$scope.$on(AppDefine.Events.SITE_TAPS.SELECT_SITES, function (event, node) {
				isActive = true;
				if (node.ID != id) {
					getSite(node.ID);
					id = node.ID;
					$scope.provider = node;
				}
			});

			$scope.$on(AppDefine.Events.SITE_TAPS.DES_SELECT_SITES, function () {
				isActive = false;
			});

			function getSite(ID) {
				$scope.SiteModel.Files = [];
				$scope.storeImages = [];
				dataContext.siteadmin.getSite({ siteKey: ID }).then(getSiteSuccess, getSiteError);
			}

			function getSiteSuccess(data) {
				$scope.SiteModel = data;
				//format Working Hours to local time
				$scope.SiteModel.WorkingHours = UpdateWorkingHourToLocalTime($scope.SiteModel.WorkingHours);
				getCountry(data);
				GetImageSite($scope.SiteModel.SiteKey);
				$scope.FixturePlanUrl = $scope.urlForDownloadSite + '?skey=' + $scope.SiteModel.SiteKey + '&fname=' + $scope.SiteModel.FixturePlan + '&fdname=' + AppDefine.SiteUploadField.fixturePlanField;
				$scope.ImageSiteUrl = $scope.urlForDownloadSite + '?skey=' + $scope.SiteModel.SiteKey + '&fname=' + $scope.SiteModel.ImageSite + '&fdname=' + AppDefine.SiteUploadField.imageSiteField;

			}

			function getSiteError(error) {
				console.log(error);
			}

			function GetImageSite(siteKey) {
				if (!siteKey) { return; }
				dataContext.siteadmin.GetImageSite({ skey: siteKey }).then(
					function (data) {
						if (data.Data) {
							$scope.SiteModel.Files = data.Data;
							GenerateStoreImages();
						}
					});
			}

			function GenerateStoreImages() {
				var urlAPI = AppDefine.Api.Sites + AppDefine.SiteAPI.GET_FILE;
				angular.forEach($scope.SiteModel.Files, function (file, index) {
					var imgObj = { Source: urlAPI + '?skey=' + $scope.SiteModel.SiteKey + '&fname=' + file.Name + '&fdname=', Text: (index + 1) };
					$scope.storeImages.push(imgObj);
				});
			}

			function getCountry(id) {
				commoninfoSvc.create().getCountries(function (data) {
					var countryObj = Enumerable.From(data).Where(function (value) { return value.Id == id.Country }).Select(function (x) { return x }).ToArray()[0];
					if (countryObj) {
						$scope.countryName = countryObj.Name;
						getStates(countryObj.Code, id.StateProvince);
					}
				}, function (error) { console.log(error); })
			}

			function getStates(id, state) {
				commoninfoSvc.create().getStates({ 'countryCode': id }, function (data) {
					$scope.stateName = Enumerable.From(data).Where(function (value) { return value.CountryCode == id && value.Id == state }).Select(function (x) { return x.Name }).ToArray()[0];
				}, function (error) { console.log(error); })
			}

			$scope.GenerateMACAddressLabel = function (index) {
				var infixString = "th";
				if (index == 1 || index == 21 || index == 31) {
					infixString = "st";
				}
				else if (index == 2 || index == 22) {
					infixString = "nd";
				}
				else if (index == 3 || index == 23) {
					infixString = "rd";
				}
				else {
					infixString = "th";
				}

				return formatString(cmsBase.translateSvc.getTranslate(AppDefine.Resx.MAC_ADDRESS), index, infixString);
			}

			function formatString(format) {
				var args = Array.prototype.slice.call(arguments, 1);
				return format.replace(/{(\d+)}/g, function (match, number) {
					return typeof args[number] != 'undefined' ? args[number] : match;
				});
			};

			function UpdateWorkingHourToLocalTime(workingHoursData) {
				var WorkingHours = [];
				angular.forEach(workingHoursData, function (workHourItem) {
					var time = new Date(workHourItem.OpenTime);
					var openTimeUTC = new Date(time.getUTCFullYear(), time.getUTCMonth(), time.getUTCDate(), time.getUTCHours(), time.getUTCMinutes());
					time = new Date(workHourItem.CloseTime);
					var closeTimeUTC = new Date(time.getUTCFullYear(), time.getUTCMonth(), time.getUTCDate(), time.getUTCHours(), time.getUTCMinutes());
					var workHours = {
						ScheduleId: workHourItem.ScheduleId,
						OpenTime: openTimeUTC,
						CloseTime: closeTimeUTC,
						SiteId: workHourItem.SiteId
					};
					WorkingHours.push(workHours);
				});
				return WorkingHours;
			}
            $scope.openUrl = function (url) {
                if (!url) { return; }
                window.open(url, "_blank");
            }

		}

		cms.register.filter('PhoneFilter', PhoneFilter);
		PhoneFilter.$inject = ['AppDefine'];
		function PhoneFilter(AppDefine) {
			return function (item) {
				var temp = ("" + item).replace(/\D/g, '');
				var temparr = temp.match(AppDefine.RegExp.PhoneRestriction);
				return (!temparr) ? null : "(" + temparr[1] + ") " + temparr[2] + "-" + temparr[3];
			}
		}
	}
})();
(function () {
	'use strict';
	define(['cms',
			'configuration/sites/helpers',
			'Scripts/Directives/treeComponent',
			'Scripts/Directives/CmsTreeView',
			'DataServices/Configuration/recipientSvc'], function (cms) {
		cms.register.controller('CalendarEditAddCtrl', CalendarEditAddCtrl);
		CalendarEditAddCtrl.$inject = ['$scope', 'dataContext', 'cmsBase', '$modalInstance', 'AppDefine', 'items', 'RecipientSvc', 'AccountSvc'];

		function CalendarEditAddCtrl($scope, dataContext, cmsBase, $modalInstance, AppDefine, items, RecipientSvc, AccountSvc) {
			var vm = this;
			var $state = cmsBase.$state;

			$scope.Resx = AppDefine.Resx;
			$scope.calData = {};
			vm.userLogin = AccountSvc.UserModel();

			if (items != null) {
				$scope.calData = angular.copy(items);
				$scope.btn_Type = "BTN_EDIT";
			} else {
				$scope.calData = {
					ID: 0,
					Name: '',
					StartDate: new Date(),
					EndDate: new Date(),
					Description: '',
					CreatedDate: new Date(),
					CreatedBy: 0,
					RemindID: 0,
					Color: 0,
					RemindBefore: 0,
					EventTrigger: false,
					RelatedFunction: 0,
					ScheduleType: 0,
					NormalizeAllSite: false,
					NormalizeTrigger: '',
					RecipientIDs: [],
					SiteIDs: []
				};
				$scope.calData.startTime = new Date();
				$scope.calData.endTime = new Date();
			}
			$scope.searchText = "";
			//$scope.data = {};
			$scope.attr = "SiteName";

			//****************************** DateTime - Begin *****************************/
			//timepicker
			function ConvertToDate(dtime) {
				var retDate = null;
				if (typeof (dtime) == 'string') {
					retDate = new Date(dtime);
				}
				else {
					retDate = dtime;
				}
				return retDate;
			}
			$scope.STime = {
				mytime: ConvertToDate($scope.calData.StartDate),
				hstep: 1,
				mstep: 1,
				ismeridian: false,
			};
			$scope.ETime = {
				mytime: ConvertToDate($scope.calData.EndDate),
				hstep: 1,
				mstep: 1,
				ismeridian: false,
			};

			$scope.disabled = function (date, mode) {
				return (mode === 'day' && (date.getDay() === 0 || date.getDay() === 6));
			};
			$scope.toggleMin = function () {
				$scope.minDate = $scope.minDate ? null : new Date();
			};
			$scope.toggleMin();
			$scope.open = function ($event, idx) {
				$event.preventDefault();
				$event.stopPropagation();
				if (idx == 0) {
					$scope.opened = true;
				}
				else {
					$scope.opened2 = true;
				}
			};

			$scope.dateOptions = {
				formatYear: 'yy',
				startingDay: 1
			};
			$scope.format = AppDefine.CalDateFormat;
			//****************************** DateTime - End *****************************/

			/*************************** TreeSite - Begin ******************************/
			vm.treeDef = {
				Id: 'ID',
				Name: 'Name',
				Type: 'Type',
				Checked: 'Checked',
				Childs: 'Sites',
				Count: 'SiteCount',
				Model: {}
			}
			vm.treeOptions = {
				Node: {
					IsShowIcon: true,
					IsShowCheckBox: true,
					IsShowNodeMenu: false,
					IsShowAddNodeButton: false,
					IsShowAddItemButton: false,
					IsShowEditButton: true,
					IsShowDelButton: true,
					IsDraggable: false
				},
				//Icon: {
				//	Item: 'icon-store-3'
				//},
				Item: {
					IsShowItemMenu: false
				}, Type: {
					Folder: 0,
					Group: 2,
					File: 1
				},
				CallBack: {
					SelectedFn: selectedFn
				}
			}

			function GetAllRegionSites() {
				dataContext.siteadmin.GetSiteByUserId(vm.userLogin.UserID, function (data) {
					if (!data.Sites) {
						vm.sitetree = {
							ID: 0,
							Name: "",
							MACAddress: 0,
							UserID: 0,
							ParentKey: null,
							ImageSite: 0,
							PACData: 0,
							Sites: [],
							SiteCount: 0,
							Type: 0,
							Checked: false
						};
						return;
					}
					UpdateSiteChecked(data);
					vm.sitetree = data;
					reCheckTree(vm.sitetree.Sites);
					vm.treeSiteFilter = angular.copy(vm.sitetree);
				}, function (error) {
					var msg = cmsBase.translateSvc.getTranslate(error.data.ReturnMessage[0]);
					cmsBase.cmsLog.error(msg);
				});
			}

			function UpdateSiteChecked(treeData) {
				if (!treeData) {
					return;
				}

				if ($.isEmptyObject(treeData.Sites)) {
					angular.forEach(treeData, function (n) {
						if (angular.isObject(n)) {
							if (n.Sites && n.Sites.length > 0) {
								UpdateSiteChecked(n.Sites);
							}

							if (vm.userSelected == null || vm.userSelected == undefined
								|| vm.userSelected.SiteIDs == null || vm.userSelected.SiteIDs == undefined
								|| vm.userSelected.SiteIDs.indexOf(n.ID) < 0 || n.Type === 0) {
								n.Checked = false;
							}
							else {
								n.Checked = true;
							}
						}
					});
				}
				else {
					angular.forEach(treeData.Sites, function (n) {
						if (angular.isObject(n)) {
							if (n.Sites && n.Sites.length > 0) {
								UpdateSiteChecked(n.Sites);
							}
							if (vm.userSelected == null || vm.userSelected == undefined
								|| vm.userSelected.SiteIDs == null || vm.userSelected.SiteIDs == undefined
									|| vm.userSelected.SiteIDs.indexOf(n.ID) < 0 || n.Type === 0) {
								n.Checked = false;
							}
							else {
								n.Checked = true;
							}
						}
					});
				}
			}

			function GetSiteSelectedIDs(siteCheckedIDs, treeData) {
				angular.forEach(treeData, function (n) {
					if (n.Sites && n.Sites.length > 0) {
						GetSiteSelectedIDs(siteCheckedIDs, n.Sites);
					}
					if (n.Type === 1 && n.Checked === true) {
						siteCheckedIDs.push(n.ID);
					}
				});
			}

			function reCheckTree(sites) {
				angular.forEach(sites, function (nodes) {
					checkinglist(nodes);
				});
			}

			function checkinglist(node) {

				var result = AppDefine.treeStatus.Uncheck;
				var intermid = false;
				var interval = AppDefine.treeStatus.Uncheck;
				var i = 0;
				if (node.Sites && node.Sites.length > 0) {
					angular.forEach(node.Sites, function (n) {
						var ch = n.Checked;
						if (n.Sites && n.Sites.length > 0) {
							ch = checkinglist(n);
						}

						if (i === 0) {
							interval = ch;
						}

						if (i > 0 && ch !== interval) {
							intermid = true;
						}

						if (intermid === true && i > 0) {
							ch = AppDefine.treeStatus.Indeterm;
						}

						result = ch;
						i++;
					});
				}
				else {
					result = node.Checked;
				}
				node.Checked = result;
				return result;
			}
			function selectedFn(node, scope) {
				scope.checkFn(node, scope.props.parentNode, scope);
			}

			function UpdateRecipientChecked(recip) {
				angular.forEach(recip, function (n) {
					if ($scope.calData == null || $scope.calData == undefined || $scope.calData.RecipientIDs == null || $scope.calData.RecipientIDs == undefined || $scope.calData.RecipientIDs.indexOf(n.RecipientID) < 0) {
						n.checked = false;
					}
					else {
						n.checked = true;
					}
				})
			}

			function CreateTreeData(treeData, sitelist) {
				var i = 0;
				angular.forEach(sitelist.RegionSites, function (n) {
					treeData[i] = {};
					if (sitelist.RegionSites && sitelist.RegionSites.length > 0) {
						treeData[i].nodes = new Array();
						CreateTreeData(treeData[i].nodes, n);
					}
					treeData[i].id = n.ID;
					treeData[i].title = n.Name.substr(0, 15);
					treeData[i].sitescount = 0;
					treeData[i].status = 0;
					treeData[i].type = n.Type;
					i++;
				});
			}
			/*************************** TreeSite - End ******************************/

			active();

			function active() {
				dataContext.injectRepos(['configuration.calendar', 'configuration.siteadmin']).then(getData);
			}

			function getData() {
				//dataContext.calendar.Recipients({}, function (data) {
				//	UpdateRecipientChecked(data)
				//	$scope.recipientList = data;
				//	//$scope.$apply();
				//	if (!$scope.$$phase) {
				//		$scope.$apply();
				//	}
				//}, function (err) {
				//});
				RecipientSvc.GetRecipient().then(function (data) {
						UpdateRecipientChecked(data)
						$scope.recipientList = data;
						if (!$scope.$$phase) {
							$scope.$apply();
						}
				});

				GetAllRegionSites();
				/*
				dataContext.calendar.RegionSites({}, function (data) {
					if (!data || !data.regionSites)
						return;
					var treeData = new Array();
					CreateTreeData(treeData, data.regionSites);
					UpdateSiteChecked(treeData);
					$scope.treeSiteData = treeData;
					if (!$scope.$$phase) {
						$scope.$apply();
					}
				}, function (err) {
				}); */
			}

			function toColor(num) {
				num >>>= 0;
				var b = num & 0xFF,
					g = (num & 0xFF00) >>> 8,
					r = (num & 0xFF0000) >>> 16;
				//a = ( (num & 0xFF000000) >>> 24 ) / 255 ;
				return "rgb(" + [r, g, b].join(",") + ")";
			}
			$scope.Color = toColor($scope.calData.Color);

			function hex2num(hex) {
				if (hex.charAt(0) == "#")
					hex = hex.slice(1); //Remove the '#' char - if there is one.
				hex = hex.toUpperCase();
				var hex_alphabets = "0123456789ABCDEF";
				var value = new Array(3);
				var k = 0;
				var int1, int2;
				for (var i = 0; i < 6; i += 2) {
					int1 = hex_alphabets.indexOf(hex.charAt(i));
					int2 = hex_alphabets.indexOf(hex.charAt(i + 1));
					value[k] = (int1 * 16) + int2;
					k++;
				}
				var c = (value[0] << 16) + (value[1] << 8) + (value[2]);
				return c;
			}
			vm.Save = function () {
				$scope.calData.StartDate.setUTCHours($scope.STime.mytime.getHours(), $scope.STime.mytime.getMinutes(), 0);
				$scope.calData.EndDate.setUTCHours($scope.ETime.mytime.getHours(), $scope.ETime.mytime.getMinutes(), 0);
				$scope.calData.Color = hex2num($scope.Color);

				$scope.calData.RecipientIDs = new Array();
				angular.forEach($scope.recipientList, function (n) {
					if (n.checked) {
						$scope.calData.RecipientIDs.push(n.RecipientID);
					}
				});

				var siteCheckedIDs = new Array();
				//GetSiteSelectedIDs(siteCheckedIDs, $scope.sitetree);//$scope.treeSiteData);
				GetSiteSelectedIDs(siteCheckedIDs, vm.treeSiteFilter.Sites);
				$scope.calData.SiteIDs = siteCheckedIDs;

				dataContext.calendar.SaveCalendar($scope.calData, function (data) {
					$modalInstance.close();
				}, function (err) {

				});
			};
			$scope.toggleSelection = function (recip) {
				recip.checked = !recip.checked;
			}
			vm.Close = function () {
				$modalInstance.close();
			}
		}
	});
})();
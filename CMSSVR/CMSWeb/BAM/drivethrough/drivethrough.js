(function () {
	'use strict';

	define(['cms',
		'DataServices/Bam/SaleReportsSvc',
		'Scripts/Services/chartSvc',
		'Scripts/Services/bamhelperSvc',
		'DataServices/Configuration/fiscalyearservice',
		'widgets/bam/charts/saleCharts',
		'DataServices/Bam/DashboardReportsSvc',
		'Scripts/Services/exportSvc'
	], function (cms) {
		cms.register.controller('DriveThroughCtrl', DriveThroughCtrl);
		DriveThroughCtrl.$inject = ['$timeout', '$rootScope', '$scope', 'cmsBase', 'SaleReportsSvc', 'AppDefine', '$filter', 'chartSvc', 'bamhelperSvc', 'fiscalyearservice', 'dataContext', 'AccountSvc', '$window', 'DashboardReportsSvc', 'Utils', 'exportSvc'];

		function DriveThroughCtrl($timeout, $rootScope, $scope, cmsBase, SaleReportsSvc, AppDefine, $filter, chartSvc, bamhelperSvc, fiscalyearservice, dataContext, AccountSvc, $window, DashboardReportsSvc, utils, exportSvc) {
			var vm = this;
			vm.headSummary = [];
			$scope.sitesKey = [];
			$scope.dateSearchList = [];
			$scope.isMobile = cmsBase.isMobile;
			$scope.rptType = 0;
			$scope.rptdriveThroughList = AppDefine.SaleReportTypes;
			vm.showSiteChart = false;
			vm.detechBrowser = $rootScope.cmsBrowser;
			vm.showTypeList = { showAll: 0, Count: 1, Dwell: 2 };
			vm.showDataTypes = [
				{ Key: vm.showTypeList.showAll, Name: 'CB_SHOW_ALL' },
				{ Key: vm.showTypeList.Count, Name: 'COUNT_STRING' },
				{ Key: vm.showTypeList.Dwell, Name: 'DWELL_STRING' },
			];
			vm.showTypeSelected = vm.showDataTypes[0].Key; //default show all
			vm.hasShowTypes = { showCount: true, showDwell: true };
			$scope.RootNode = {};
			$scope.cellNames = [];
			if (vm.hasShowTypes.showCount === true) {
				$scope.cellNames.push({ key: 'COUNT', value: cmsBase.translateSvc.getTranslate(AppDefine.Resx.COUNT) });
			}
			if (vm.hasShowTypes.showDwell === true) {
				$scope.cellNames.push({ key: 'DWELL', value: cmsBase.translateSvc.getTranslate(AppDefine.Resx.DWELL) });
			}

			var SMALL_PATTERN = 768;
			var MEDIUM_PATTERN = 992;
			var LARGE_PATTERN = 1680;
			//************************* Site Tree - Begin ****************************//
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
			vm.querySite = '';
			vm.treeSiteFilterS = null;
			vm.sitetreeS = null;
			vm.siteloaded = false;
			vm.selectedSites = [];
			$rootScope.isHideTree = false;
			$scope.DRIVETHROUGH_NUM_ROUND = 10000;

			angular.element($window).on("resize", BamTableResize_emit);

			$scope.$on(AppDefine.Events.BAMSELECTNODE, function (e, node) {
				$rootScope.NameLocation = node.Name;
				dataContext.injectRepos(['configuration.siteadmin'])
					.then(getAllRegionSites);
			});

			$scope.$on(AppDefine.Events.GETBAMREPORTDATA, function (e, arg) {
				ReloadData();
			});

			//Anh, Wait for the header param data has been set
			$scope.$on(AppDefine.Events.ACTIVESALESREPORT, function (e) {
				dataContext.injectRepos(['configuration.siteadmin'])
					.then(getAllRegionSites);
			});

			//ThangPham, Export data for Drive Through, July 04 2016
			$scope.$on(AppDefine.Events.EXPORTEVENT, function (event, data) {
				var userLogin = AccountSvc.UserModel();

				var reportInfo = {
					TemplateName: cmsBase.translateSvc.getTranslate(AppDefine.Resx.MODULE_BAM) + "_" + cmsBase.translateSvc.getTranslate($rootScope.title).replace(/[\s]/g, ''),
					ReportName: cmsBase.translateSvc.getTranslate($rootScope.title),
					CompanyID: userLogin.CompanyID,
					RegionName: '',
					Location: '',
					WeekIndex: $rootScope.GWeek,
					Footer: '',
					CreatedBy: userLogin.FName + ' ' + userLogin.LName,
					CreateDate: $filter('date')(new Date(), AppDefine.ParamDateFormat) // MM/dd/yyyy
				};

				var options = {
					ColIndex: 1,
					RowIndex: 1,
					tableName: 'DriveThrough',
					dateList: $scope.dateSearchList,
					cellNames: $scope.cellNames,
					Fields: { Regions: "Sites", DetailData: "DetailData", Dwell: "Dwell", Count: "Count" }
				};
				var tables = [];
				var tableData = exportSvc.buildTableTemplate(vm.DataDriveThrough.DTData, options);
				tables.push(tableData);

				var charts = [];
				
				exportSvc.exportXlsFromServer(reportInfo, tables, charts);
			});

			function BamTableResize_emit() {
				var rptPram = {
					type: $scope.rptType,
					dataLength: $scope.dateSearchList.length
				};
				$scope.$emit('BamTableResize', rptPram);
			}

			function getAllRegionSites() {
				if ($scope.$parent && $scope.$parent.$parent && $scope.$parent.$parent.treeSiteFilter) {
					var data = $scope.$parent.$parent.treeSiteFilter;
					vm.sitetreeS = {};
					angular.copy(data, vm.sitetreeS);
					//var firstsite = bamhelperSvc.getFirstSites($scope.$parent.$parent.treeSiteFilter);
					vm.treeSiteFilterS = vm.sitetreeS;
					//selectedFn(firstsite);

					var checkedIDs = [];
					chartSvc.GetSiteSelectedIDs(checkedIDs, vm.treeSiteFilterS.Sites);
					vm.selectedSites = checkedIDs;
					vm.siteloaded = true;
					hideTreeSiteRight();
					ReloadData();
				}
			}

			vm.TreeSiteClose = function () {
				if ($("#btn-popMenuConvSites").parent().hasClass("open")) {
					$("#btn-popMenuConvSites").parent().removeClass("open");
					$("#btn-popMenuConvSites").prop("aria-expanded", false);
				}
				var checkedIDs = [];
				chartSvc.GetSiteSelectedIDs(checkedIDs, vm.treeSiteFilterS.Sites);
				vm.selectedSites = checkedIDs;
				$scope.$emit(AppDefine.Events.SUBMITGETBAMREPORT);
			};

			function selectedFn(node, scope) {
				var siteArr = [];
				var nodeSelected = node;
				if (node.Type !== AppDefine.NodeType.Site) {
					nodeSelected = bamhelperSvc.getFirstSites(node);
					siteArr = [nodeSelected.ID];
				}
				else {
					siteArr = [node.ID];
				}
				chartSvc.SetNodeSelected(vm.sitetreeS, siteArr);
				vm.treeSiteFilterS = vm.sitetreeS;
				var checkedIDs = [];
				chartSvc.GetSiteSelectedIDs(checkedIDs, vm.treeSiteFilterS.Sites);
				vm.selectedSites = checkedIDs;
				$scope.$applyAsync();
				$timeout(function () {
					$scope.$broadcast('cmsTreeRefresh', nodeSelected);
				}, 100);
			}
			//************************* Site Tree - End ****************************//
			//active();

			function getDriveThroughData(filterParam) {
				$scope.rptType = filterParam.ReportType;

				if (filterParam.sitesKey.length == 0) {
					return;
				}
				DashboardReportsSvc.GetDriveThroughData(filterParam, function (data) {
					vm.DataDriveThrough = data;
					console.log(data);
					getListDateSearch();

					var rptChartData = {};
					rptChartData.ReportID = AppDefine.BamReportTypes.DRIVETHROUGH;
					rptChartData.ChartData = data.ChartData;
					rptChartData.rptType = $scope.rptType;
					//rptChartData.ChartTypes = vm.showChartTypes;
					$scope.$broadcast(AppDefine.Events.CHARTDATALOADED, rptChartData);//vm.myData.ChartData);

					$timeout(function () {
						var rptPram = {
							type: $scope.rptType,
							dataLength: $scope.dateSearchList.length
						};
						$scope.$emit('BamTableResize', rptPram);
						$scope.$emit('scrollDown');
					}, 100, false);

				},
             function (error) {
             	cmsBase.cmsLog.error('error');
             });

			}

			//function active() {
			//	dataContext.injectRepos(['configuration.siteadmin'])
			//		.then(getAllRegionSites);
			//}

			function ReloadData() {
				var sdate = $filter('date')($rootScope.FilterDriveThrough.FromDate, AppDefine.DFFileName);
				var edate = $filter('date')($rootScope.FilterDriveThrough.EndDate, AppDefine.DFFileName);
				vm.filterParam = {
					SetDate: sdate,
					sDate: sdate,
					eDate: edate,
					sitesKey: vm.selectedSites.toString(),
					ReportId: 1,
					ReportType: $rootScope.FilterDriveThrough.Type.ID
				};
				// console.log(vm.filterParam);
				getDriveThroughData(vm.filterParam);
			}

			vm.showSite = function (region) {
				if (!region) { return; }
				region.checked = !region.checked;
			};

			function setTableByReportType(rptType) {
				switch (rptType) {
					case AppDefine.SaleReportTypes.Hourly:
						$scope.rptTypeName = 'Hourly';
						break;
					case AppDefine.SaleReportTypes.Daily:
						$scope.rptTypeName = 'Daily';
						break;
					case AppDefine.SaleReportTypes.Weekly:
						$scope.rptTypeName = 'Weekly';
						break;
					case AppDefine.SaleReportTypes.WTD:
						$scope.rptTypeName = 'WTD';
						break;
					case AppDefine.SaleReportTypes.PTD:
						$scope.rptTypeName = 'PTD';
						break;
					case AppDefine.SaleReportTypes.YTD:
						$scope.rptTypeName = 'YTD';
						break;
				}

			}

			vm.orderRow = function (num) {
				return (num & 1) ? "odd" : "even";
			};

			vm.tbScrollOptions = function () {
				var w = $(".bam-manage").width();
			};

			vm.rdbShowTypeClick = function (value) {
				if (value) {
					vm.showTypeSelected = value.Key;
					SetShowTypeFlags();

					$timeout(function () {
						var rptPram = {
							type: $scope.rptType,
							dataLength: $scope.dateSearchList.length
						};
						$scope.$emit('BamTableResize', rptPram);
					}, 100, false);

				}
			};

			function hideTreeSiteRight() {
				var elem = angular.element('#content');
				$rootScope.isHideTree = false;
				rescale(elem);
				$scope.fixTreesitemobile = $scope.isSmall && $rootScope.isHideTree;
			}

			function rescale(elem) {
				// console.log($window.innerHeight);
				if (!$rootScope.isHideTree) {
					if (cmsBase.isMobile) { /*For mobile*/
						$scope.isSmall = true;
						if (document.body.clientWidth < SMALL_PATTERN) {
							return;
						}
						elem.css('width', 0 + '%');
						angular.element('#sidebar').css('width', 100 + '%');
						$timeout(function () {
							angular.element('#sidebar').css('height', 'auto');
						}, 1000, false);
					} else { /*For FC*/
						$scope.isSmall = false;
						if (document.body.clientWidth < SMALL_PATTERN) {
							elem.css('width', 0 + '%');
							angular.element('#sidebar').css('width', 100 + '%');
							return;
						}
						//set hide fusionchart
						var fus = angular.element('fusioncharts');
						fus.css('display', 'none');

						elem.css('width', 0 + '%');
						var sidebar = angular.element('#sidebar');
						sidebar.css('width', 100 + '%');
						sidebar.css('padding-right', 0);

						$timeout(function () {
							fus.css('display', 'block');
						}, 1000, false);
					}
				} else {

					if (cmsBase.isMobile) { /*For mobile */
						$scope.isSmall = true;
						if (document.body.clientWidth < SMALL_PATTERN) {
							//elem.css('width', 100 + '%');
							//angular.element('#sidebar').css('width', 0 + '%');
							//angular.element('#sidebar').css('height', '0'); 
						} else {
							var scoperegion = angular.element('.content');
							var scopesize = parseInt(scoperegion.css('width'));
							elem.css('width', scopesize + 'px');
							angular.element('#sidebar').css('width', 0 + 'px');
							$timeout(function () {
								angular.element('#sidebar').css('height', 'auto');
							}, 1000, false);
						}


					} else {/* For PC */
						$scope.isSmall = false;

						if (document.body.clientWidth < SMALL_PATTERN) {
							elem.css('width', 100 + '%');
							angular.element('#sidebar').css('width', 0 + '%');
							// angular.element('#sidebar').css('height', '0');
							return;
						}

						var resizeScopeElm = angular.element('#resizer-scope');
						var resizerScopeWidth = parseInt(resizeScopeElm.css('width'));
						var leftSize = resizerScopeWidth - 375;
						//set hide fusionchart
						var fus = angular.element('fusioncharts');
						fus.css('display', 'none');
						elem.css('width', 400 + 'px');
						var sidebar = angular.element('#sidebar');
						sidebar.css('width', leftSize + 'px');
						sidebar.css('padding-right', '25px');
						$timeout(function () {
							fus.css('display', 'block');
						}, 1000, false);
					}

				} /*End else*/
			}

			function getListDateSearch() {
				if (vm.DataDriveThrough.DTData && vm.DataDriveThrough.DTData[0]) {
					$scope.dateSearchList = [];
					angular.forEach(vm.DataDriveThrough.DTData[0].DetailData, function (d) {
						//switch ($scope.rptType) {
						//	case AppDefine.SaleReportTypes.Hourly:
						//		$scope.dateSearchList.push(chartSvc.formatHourBAM(d.TimeIndex));
						//		break;
						//	case AppDefine.SaleReportTypes.Weekly:
						//		var titleString = d.Title.split('-');
						//		var dateObj = { WeekIndex: titleString[0].trim(), sDate: titleString[1].trim(), eDate: titleString[2].trim() };
						//		$scope.dateSearchList.push(dateObj);
						//		break;
						//	case AppDefine.SaleReportTypes.Monthly:
						//		$scope.dateSearchList.push(d.Title);
						//		break;
						//	default:
						//		$scope.dateSearchList.push($filter('date')(chartSvc.GetUTCDate(d.Date), AppDefine.CalDateFormat));
						//		break;
						//}
						var label = chartSvc.formatChartLabel($scope.rptType, d);
						$scope.dateSearchList.push(label);
						//console.log($scope.dateSearchList);
					});
				}
			}

			//function formatHour(value) {
			//	//if (angular.isNumber(value)) { return; }
			//	if (value < 10) {
			//		return "0" + value + ":00-0" + (value + 1) + ":00";
			//	}
			//	else {
			//		if (value == 23) { return value + ":00" + "-00:00"; }
			//		return value + ":00-" + (value + 1) + ":00";
			//	}
			//}

			$scope.clickOutside = function ($event, element) {


				if (angular.element(element).hasClass('open')) {
					angular.element(element).removeClass('open');
					console.log('have open');
				} else {
					console.log('dont have open');
				}
			};

			vm.getDataTableMobile = function (data, index_key, type) {
				switch (type) {
					case "COUNT":
						return data.DetailData[index_key].Count;
						break;
					case "DWELL":
						return data.DetailData[index_key].Dwell;
						break;
				}
			};

			vm.getClass = function (value, sumValue) {
				var percent = (value * 100) / sumValue;
				if (percent < 0) {
					return "color_0";
				}
				else if (0 <= percent && percent <= 9) {
					return "color_lt_10";
				}
				else if (10 <= percent && percent <= 19) {
					return "color_lt_20";
				}
				else if (20 <= percent && percent <= 29) {
					return "color_lt_30";
				}
				else if (30 <= percent && percent <= 39) {
					return "color_lt_40";
				}
				else if (40 <= percent && percent <= 49) {
					return "color_lt_50";
				}
				else if (50 <= percent && percent <= 59) {
					return "color_lt_60";
				}
				else if (60 <= percent && percent <= 69) {
					return "color_lt_70";
				}
				else if (70 <= percent && percent <= 79) {
					return "color_lt_80";
				}
				else if (80 <= percent && percent <= 89) {
					return "color_lt_90";
				}
				else if (90 <= percent && percent <= 99) {
					return "color_lt_100";
				}
				else if (100 <= percent) {
					return "color_100";
				}

				//default
				return "";
			};

			function SetShowTypeFlags() {
				if (vm.showTypeSelected == vm.showTypeList.showAll) {
					vm.hasShowTypes.showCount = true;
					vm.hasShowTypes.showDwell = true;
				}
				else {
					vm.hasShowTypes.showCount = vm.showTypeSelected == vm.showTypeList.Count ? true : false;
					vm.hasShowTypes.showDwell = vm.showTypeSelected == vm.showTypeList.Dwell ? true : false;
				}

				$scope.cellNames = [];
				if (vm.hasShowTypes.showCount === true) {
					$scope.cellNames.push({ key: 'COUNT', value: cmsBase.translateSvc.getTranslate(AppDefine.Resx.COUNT) });
				}
				if (vm.hasShowTypes.showDwell === true) {
					$scope.cellNames.push({ key: 'DWELL', value: cmsBase.translateSvc.getTranslate(AppDefine.Resx.DWELL) });
				}
			}
		}
	});
})();
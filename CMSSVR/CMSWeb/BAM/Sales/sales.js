(function () {
	'use strict';

	define(['cms',
		'DataServices/Bam/SaleReportsSvc',
		'Scripts/Services/chartSvc',
		'Scripts/Services/bamhelperSvc',
		'DataServices/Configuration/fiscalyearservice',
		'widgets/bam/charts/saleNewCharts',
        'Scripts/Services/exportSvc'
	], function (cms) {
		cms.register.controller('SaleReportsCtrl', SaleReportsCtrl);
		SaleReportsCtrl.$inject = ['$timeout', '$rootScope', '$scope', 'cmsBase', 'SaleReportsSvc', 'AppDefine', '$filter', 'Utils', 'chartSvc', 'bamhelperSvc', 'fiscalyearservice', 'dataContext', 'AccountSvc', '$window', 'exportSvc'];

		function SaleReportsCtrl($timeout, $rootScope, $scope, cmsBase, SaleReportsSvc, AppDefine, $filter, Utils, chartSvc, bamhelperSvc, fiscalyearservice, dataContext, AccountSvc, $window, exportSvc) {
			var vm = this;
			$scope.isMobile = cmsBase.isMobile;
			$scope.showInfo = "sales";
			$scope.dateSearchList = [];

			var TEXT_CHART_SITES = cmsBase.translateSvc.getTranslate(AppDefine.Resx.CHART_SITES);
			$scope.SiteTreeLabel = TEXT_CHART_SITES;
			vm.detechBrowser = $rootScope.cmsBrowser;
			vm.showSiteChart = false;
			vm.showTypeList = {
				showAll: 0,
				trafficIn: AppDefine.BamDataTypes.TRAFFIC_IN,
				trafficOut: AppDefine.BamDataTypes.TRAFFIC_OUT,
				transaction: AppDefine.BamDataTypes.POS,
				conversion: AppDefine.BamDataTypes.CONVERSION
			};
			vm.showDataTypes = [
				{ Key: vm.showTypeList.showAll, Name: 'CB_SHOW_ALL' },
				{ Key: vm.showTypeList.trafficIn, Name: 'CB_TRAFFICIN' },
				{ Key: vm.showTypeList.trafficOut, Name: 'CB_TRAFFICOUT' },
				{ Key: vm.showTypeList.transaction, Name: 'CB_POS' },
				{ Key: vm.showTypeList.conversion, Name: 'CB_CONVERSION' }
			];
			vm.showTypeSelected = vm.showDataTypes[1].Key; //default show traffic In
			vm.hasShowTypes = { showIn: true, showOut: false, showPos: false, showConv: false };

			vm.userLogin = AccountSvc.UserModel();
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
			$scope.rptTypeList = AppDefine.SaleReportTypes;
			var SMALL_PATTERN = 768;
			var MEDIUM_PATTERN = 992;
			var LARGE_PATTERN = 1680;
			$rootScope.isHideTree = false;
			$scope.isSmall = false;
			$scope.SALE_NUM_ROUND = 10000;
			vm.showNoData = false;
			
			$scope.cellNames = [];
			if (vm.hasShowTypes.showIn === true) {
				$scope.cellNames.push({ key: 'COUNTIN', value: cmsBase.translateSvc.getTranslate(AppDefine.Resx.TRAFFIC_IN) });
			}
			if (vm.hasShowTypes.showOut === true) {
				$scope.cellNames.push({ key: 'COUNTOUT', value: cmsBase.translateSvc.getTranslate(AppDefine.Resx.TRAFFIC_OUT) });
			}
			if (vm.hasShowTypes.showPos === true) {
				$scope.cellNames.push({ key: 'POS', value: cmsBase.translateSvc.getTranslate(AppDefine.Resx.POS) });
			}
			if (vm.hasShowTypes.showConv === true) {
				$scope.cellNames.push({ key: 'CONVERSION', value: cmsBase.translateSvc.getTranslate(AppDefine.Resx.CONVERSION_STRING) });
			}

			//vm.showChartTypes = [
			//	{ Key: vm.showTypeList.trafficIn, Name: 'Traffic In', Checked: true, yAxis: "P" },
			//	{ Key: vm.showTypeList.trafficOut, Name: 'Traffic Out', Checked: false, yAxis: "P" },
			//	{ Key: vm.showTypeList.transaction, Name: 'POS', Checked: false, yAxis: "S" },
			//	{ Key: vm.showTypeList.conversion, Name: 'Conversion %', Checked: false, yAxis: "S" }
			//];
			vm.showChartTypes = [
				{ Key: vm.showTypeList.trafficIn, Name: cmsBase.translateSvc.getTranslate(AppDefine.Resx.CB_TRAFFICIN), Checked: true, yAxis: "P" },
				{ Key: vm.showTypeList.trafficOut, Name: cmsBase.translateSvc.getTranslate(AppDefine.Resx.CB_TRAFFICOUT), Checked: false, yAxis: "P" },
				{ Key: vm.showTypeList.transaction, Name: cmsBase.translateSvc.getTranslate(AppDefine.Resx.CB_POS), Checked: false, yAxis: "S" },
				{ Key: vm.showTypeList.conversion, Name: cmsBase.translateSvc.getTranslate(AppDefine.Resx.CB_CONVERSION), Checked: false, yAxis: "S" }
			];
			//active();

			$scope.$on(AppDefine.Events.BAMSELECTNODE, function (e, node) {
				dataContext.injectRepos(['configuration.siteadmin'])
					.then(getAllRegionSites);
			});

			$scope.$on(AppDefine.Events.GETBAMREPORTDATA, function (e, arg) {
				// console.log("submit Refesh >> " + arg);
				if (!arg && !arg.FilterSaleReport) { return; }
				var pram = {
					rptDataType: arg.FilterSaleReport.Type.ID,
					siteKeys: '',
					sDate: arg.FilterSaleReport.FromDate,
					eDate: arg.FilterSaleReport.EndDate
				};
				getReportData(pram);
			});

			$scope.$on(AppDefine.Events.ACTIVESALESREPORT, function (e) {
				dataContext.injectRepos(['configuration.siteadmin'])
					.then(getAllRegionSites);
			});

			angular.element($window).on("resize", BamTableResize);

			//function active() {
			//	dataContext.injectRepos(['configuration.siteadmin'])
			//		.then(getAllRegionSites);
			//}

			vm.tbScrollOptions = function () {
				var w = $(".bam-manage").width();
			};

			$scope.$on(AppDefine.Events.EXPORTEVENT, function (event, data) {
				console.log(vm.myData.SummaryData);
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

				var tables = [];
				var options = {
					ColIndex: 1,
					RowIndex: 1,
					tableName: 'TableSaleReport',
					dateList: $scope.dateSearchList,
					cellNames: $scope.cellNames,
					Fields: { Regions: "Sites", DetailData: "DataDetail", TrafficIn: "TrafficIn", TrafficOut: "TrafficOut", CountTrans: "CountTrans", Conversion: "Conversion" }
				};
				var saleTable = exportSvc.buildTableTemplate(vm.myData.SummaryData, options);
				tables.push(saleTable);
			
				var charts = [];

				exportSvc.exportXlsFromServer(reportInfo, tables, charts);
			});

			function getReportData(params) {
				var sitekeys = vm.selectedSites;
				if (!sitekeys || sitekeys.length <= 0) {
					return; //No site select or site data is not ready
				}
				params.siteKeys = sitekeys.toString();
				$scope.rptType = params.rptDataType;
				// console.log(params);
				SaleReportsSvc.GetReportData(params, function (data) {
					if (data === null || data === undefined) {
						//console.log('No Data');
						vm.showNoData = true;
					}
					else {
						vm.myData = data;
						getListDateSearch();
						//console.log(vm.myData.SummaryData);

						// $.each(vm.myData.SummaryData, function(index, val) {
						// 	 console.log(val.DataDetail[index].TrafficIn);
						// });

						//CreateChartData(vm.myData.ChartData);
						var saleChartData = {};
						saleChartData.ReportID = AppDefine.BamReportTypes.SALE;
						saleChartData.ChartData = vm.myData.ChartData;
						saleChartData.ChartTypes = vm.showChartTypes;
						saleChartData.rptType = $scope.rptType; //Hourly,Daily,...

						$scope.$broadcast(AppDefine.Events.CHARTDATALOADED, saleChartData);//vm.myData.ChartData);

						$timeout(function () {
							BamTableResize();
						}, 1000, false);
						$scope.$emit('scrollDown')
						setTableByReportType($scope.rptType);

						if (!vm.myData.SummaryData || vm.myData.SummaryData.length == 0) {
							vm.showNoData = true;
						}
						else {
							vm.showNoData = false;
						}
					}
				},
				function (error) {
					cmsBase.cmsLog.error('error');
					vm.showNoData = true;
				});
			}

			vm.getTrafficIn = function (data, index_key, type) {
				switch (type) {
					case "IN":
						return data.DataDetail[index_key].TrafficIn;
						break;
					case "OUT":
						return data.DataDetail[index_key].TrafficOut;
						break;
					case "POS":
						return data.DataDetail[index_key].CountTrans;
						break;
					case "CON":
						return Math.round(data.DataDetail[index_key].Conversion);
						break;

				}
			}

			vm.orderRow = function (num) {
				return (num & 1) ? "odd" : "even";
			}

			function getListDateSearch() {
				if (vm.myData.SummaryData && vm.myData.SummaryData[0]) {
					$scope.dateSearchList = [];
					angular.forEach(vm.myData.SummaryData[0].DataDetail, function (d) {
						//switch ($scope.rptType) {
						//	case AppDefine.SaleReportTypes.Hourly:
						//		$scope.dateSearchList.push(chartSvc.formatHourBAM(d.Hour));
						//		break;
						//	case AppDefine.SaleReportTypes.Weekly:
						//		var titleString = d.Title.split('-');
						//		var dateObj = { WeekIndex: cmsBase.translateSvc.getTranslate(AppDefine.Resx.WEEK_STRING) + titleString[0].trim(), sDate: titleString[1].trim(), eDate: titleString[2].trim() };
						//		$scope.dateSearchList.push(dateObj);
						//		break;
						//	case AppDefine.SaleReportTypes.Monthly:
						//		$scope.dateSearchList.push(cmsBase.translateSvc.getTranslate(AppDefine.Resx.PERIOD_STRING) + d.Title);
						//		break;
						//	default:
						//		$scope.dateSearchList.push($filter('date')(new Date(d.Date), AppDefine.CalDateFormat));
						//		break;
						//}
						var label = chartSvc.formatChartLabel($scope.rptType, d);
						$scope.dateSearchList.push(label);
					});
				}
			}

			function getAllRegionSites() {
				if ($scope.$parent && $scope.$parent.$parent && $scope.$parent.$parent.treeSiteFilter) {
					var data = $scope.$parent.$parent.treeSiteFilter;
					vm.sitetreeS = {};
					angular.copy(data, vm.sitetreeS);
					chartSvc.UpdateSiteChecked(vm.sitetreeS, vm.userSelected, true);
					vm.treeSiteFilterS = vm.sitetreeS;
					var checkedIDs = [];
					var checkedNames = [];
					//chartSvc.GetSiteSelectedIDs(checkedIDs, vm.treeSiteFilterS.Sites);
					chartSvc.GetSiteSelectedNames(checkedIDs, checkedNames, vm.treeSiteFilterS.Sites);
					vm.selectedSites = checkedIDs;
					vm.siteloaded = true;

					if (checkedNames.length == 1) {
						$scope.SiteTreeLabel = checkedNames[0];
					}
					else {
						$scope.SiteTreeLabel = TEXT_CHART_SITES;
						//$scope.SiteTreeLabel = cmsBase.translateSvc.getTranslate(AppDefine.Resx.CHART_SITES);
					}
				}

				hideTreeSiteRight();
				var pram = {
					rptDataType: $rootScope.FilterSaleReport.Type.ID,
					siteKeys: vm.selectedSites.length > 0 ? vm.selectedSites.toString() : '',
					sDate: angular.isString($rootScope.FilterSaleReport.FromDate) ? $rootScope.FilterSaleReport.FromDate : $filter('date')(new Date($rootScope.FilterSaleReport.FromDate), AppDefine.DateFormatCParamST),
					eDate: angular.isString($rootScope.FilterSaleReport.EndDate) ? $rootScope.FilterSaleReport.EndDate : $filter('date')(new Date($rootScope.FilterSaleReport.EndDate), AppDefine.DateFormatCParamED)
				};
				getReportData(pram);

				//var def = cmsBase.$q.defer();
				//dataContext.siteadmin.GetSiteByUserId(vm.userLogin.UserID, function (data) {
				//	vm.sitetreeS = {};
				//	angular.copy(data, vm.sitetreeS);
				//	chartSvc.UpdateSiteChecked(vm.sitetreeS, vm.userSelected, true);
				//	vm.treeSiteFilterS = vm.sitetreeS;
				//	var checkedIDs = [];
				//	chartSvc.GetSiteSelectedIDs(checkedIDs, vm.treeSiteFilterS.Sites);
				//	vm.selectedSites = checkedIDs;
				//	vm.siteloaded = true;
				//	def.resolve(data);
				//}, function (error) {
				//	var msg = cmsBase.translateSvc.getTranslate(error.data.ReturnMessage[0]);
				//	cmsBase.cmsLog.error(msg);
				//	def.reject(error);
				//});
				//return def.promise;
			}

			vm.TreeSiteClose = function () {
				if ($("#btn-popMenuConvSites").parent().hasClass("open")) {
					$("#btn-popMenuConvSites").parent().removeClass("open");
					$("#btn-popMenuConvSites").prop("aria-expanded", false);
				}
				var checkedIDs = [];
				var checkedNames = [];
				//chartSvc.GetSiteSelectedIDs(checkedIDs, vm.treeSiteFilterS.Sites);
				chartSvc.GetSiteSelectedNames(checkedIDs, checkedNames, vm.treeSiteFilterS.Sites);
				if (checkedNames.length == 1) {
					$scope.SiteTreeLabel = checkedNames[0];
				}
				else {
					$scope.SiteTreeLabel = TEXT_CHART_SITES;
					//$scope.SiteTreeLabel = cmsBase.translateSvc.getTranslate(AppDefine.Resx.CHART_SITES);
				}

				vm.selectedSites = checkedIDs;
				$scope.$emit(AppDefine.Events.SUBMITGETBAMREPORT);
				//console.log(checkedIDs);
			}

			function selectedFn(node, scope) {
				scope.checkFn(node, scope.props.parentNode, scope);
			}

			vm.showSite = function (region) {
				if (!region) { return; }
				region.checked = !region.checked;
			}

			vm.rdbShowTypeClick = function (value) {
				if (value) {
					vm.showTypeSelected = value.Key;
					SetShowTypeFlags();

					$timeout(function () {
						BamTableResize();
					}, 1000, false);
				} //if
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

			function BamTableResize() {
				if (document.body.clientWidth < 992) {
					return;
				}


				var total = angular.element('.wrapper-bam-table').innerWidth();
				var first_column = angular.element('.wrapper-bam-table > .bam-table-column:nth-child(1)').innerWidth();
				var third_column = angular.element('.wrapper-bam-table > .bam-table-column:nth-child(3)').innerWidth();
				var second_column = total - (first_column + third_column);
				var end_box = angular.element('.col-fixed-header').innerWidth();
				// console.log(third_column);
				angular.element('.wrapper-bam-table > .bam-table-column:nth-child(2)').css('width', second_column + 'px');


				// var f_element = $('.wrapper-bam-table > .bam-tb-scroll .bam-table > div');
				// f_element.find('.tb-col.ng-scope .col-date').width('auto');


				var e_single = $('.wrapper-bam-table.bam-table-single-data .bam-tb-scroll .bam-table > div');
				var e = $('.wrapper-bam-table:not(.bam-table-single-data) .bam-tb-scroll .bam-table > div');
				var x = Math.round((second_column - end_box) / $scope.dateSearchList.length);	//check perfect  width
				var y = e.find('.tb-col.ng-scope  > div').width();		//curent width
				// alert(end_box);
				// alert(x);
				if (x > 281) {
					console.log(x);
					// test.find('.tb-col.ng-scope').attr('style', '');
					// test.find('.tb-col.ng-scope  > div').attr('style', '');
					e.find('.tb-col.ng-scope').width(x);
					e.find('.tb-col.ng-scope  > div').width(x);
					e_single.find('.tb-col.ng-scope').width(x);
					e_single.find('.tb-col.ng-scope  > div').width(x);
					console.log(x + '281');

				} else if (x > 70) {
					e_single.find('.tb-col.ng-scope').width(x);
					e_single.find('.tb-col.ng-scope  > div').width(x);
					console.log(x + '70');
				}
				else {
					e_single.find('.tb-col.ng-scope').width(70);
					e_single.find('.tb-col.ng-scope  > div').width(70);
				}


				// 	else {
				// 		var total = angular.element('.wrapper-bam-table').innerWidth();
				// 		var first_column = angular.element('.wrapper-bam-table > .bam-table-column:nth-child(1)').innerWidth();
				// 		var third_column = angular.element('.wrapper-bam-table > .bam-table-column:nth-child(3)').innerWidth();
				// 		var second_column = total - (first_column + third_column);



				// 		angular.element('.wrapper-bam-table > .bam-table-column:nth-child(2)').css('width', second_column + 'px');
				// 		var f_element = $('.wrapper-bam-table > .bam-tb-scroll .bam-table > div');
				// 			f_element.find('.tb-col.ng-scope .col-date').width('auto');


				// 		switch($scope.rptType){

				// 			case AppDefine.SaleReportTypes.Weekly:
				// 				var  e_main =$('.Weekly .bam-tb-scroll .tb-header');
				// 				var mainDiv = $('.Weekly.bam-table-single-data .bam-tb-scroll .bam-table > div');
				// 				var w = second_column/$scope.dateSearchList.length;
				// 				e_main.find('.tb-col.ng-scope .col-date').width('auto');
				// 				mainDiv.find('.tb-col.ng-scope  > div').width(w);
				// 				// console.log(w);
				// 				break;	
				// 			default :
				// 				var e = $('.wrapper-bam-table .bam-tb-scroll .bam-table > div');	
				// 				var x = Math.round(second_column / $scope.dateSearchList.length);	//check perfect  width
				// 				var y = e.find('.tb-col.ng-scope  > div').width();		//curent width					
				// 				if ( y < x ) {
				// 					e.find('.tb-col.ng-scope').width(x);	
				// 					e.find('.tb-col.ng-scope  > div').width(x);
				// 				}
				// 				break;
				// 		}
				// 	}
			}

			$scope.clickOutside = function ($event, element) {
				// 2015-05-25 Tri fix bug 3289
				// update state tree when without click Done.
				//bamhelperSvc.setNodeSelected(vm.treeSiteFilterS, vm.selectedSites);
				//$scope.$broadcast('cmsTreeRefresh', vm.treeSiteFilterS);

				if (angular.element(element).hasClass('open')) {
					angular.element(element).removeClass('open');
					//console.log('have open');
				} else {
					//console.log('dont have open');
				}
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
					case AppDefine.SaleReportTypes.Monthly:
						$scope.rptTypeName = 'Monthly';
						break;
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
			}

			// Active first
			//$scope.$emit('SetStartDate');

			function SetShowTypeFlags() {
				if (vm.showTypeSelected == vm.showTypeList.showAll) {
					vm.hasShowTypes.showIn = true;
					vm.hasShowTypes.showOut = true;
					vm.hasShowTypes.showPos = true;
					vm.hasShowTypes.showConv = true;
				}
				else {
					vm.hasShowTypes.showIn = vm.showTypeSelected == vm.showTypeList.trafficIn ? true : false;
					vm.hasShowTypes.showOut = vm.showTypeSelected == vm.showTypeList.trafficOut ? true : false;
					vm.hasShowTypes.showPos = vm.showTypeSelected == vm.showTypeList.transaction ? true : false;
					vm.hasShowTypes.showConv = vm.showTypeSelected == vm.showTypeList.conversion ? true : false;
				}

				$scope.cellNames = [];
				if (vm.hasShowTypes.showIn === true) {
					$scope.cellNames.push({ key: 'COUNTIN', value: cmsBase.translateSvc.getTranslate(AppDefine.Resx.TRAFFIC_IN) });
				}
				if (vm.hasShowTypes.showOut === true) {
					$scope.cellNames.push({ key: 'COUNTOUT', value: cmsBase.translateSvc.getTranslate(AppDefine.Resx.TRAFFIC_OUT) });
				}
				if (vm.hasShowTypes.showPos === true) {
					$scope.cellNames.push({ key: 'POS', value: cmsBase.translateSvc.getTranslate(AppDefine.Resx.POS) });
				}
				if (vm.hasShowTypes.showConv === true) {
					$scope.cellNames.push({ key: 'CONVERSION', value: cmsBase.translateSvc.getTranslate(AppDefine.Resx.CONVERSION_STRING) });
				}
			}
		}
	});
})();
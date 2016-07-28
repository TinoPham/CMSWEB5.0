(function () {
	'use strict';
	define(['cms',
		'DataServices/ReportService',
		'Scripts/Services/chartSvc',
		'DataServices/Configuration/fiscalyearservice',
		'configuration/sites/helpers',
		'Scripts/Directives/treeComponent',
		'Scripts/Directives/CmsTreeView'],
		function (cms) {
			cms.register.controller('convBySitesCtrl', convBySitesCtrl);
			convBySitesCtrl.$inject = ['$scope', '$window', 'cmsBase', 'AppDefine', 'ReportService', 'Utils', 'chartSvc', 'fiscalyearservice', '$filter', 'dataContext', '$timeout', 'siteadminService', 'AccountSvc'];
			function convBySitesCtrl($scope, $window, cmsBase, AppDefine, rptService, utils, chartSvc, fiscalyearservice, $filter, dataContext, $timeout, siteadminService, AccountSvc) {
				var vm = this;

				var Options = JSON.parse("{" + $scope.widgetModel.TemplateParams + "}");
				$scope.Gui = (Options == null || !Options.hasOwnProperty("Gui")) ? null : Options.Gui;
				$scope.Pram = (Options == null || !Options.hasOwnProperty("Pram")) ? null : Options.Pram;
				$scope.Opt = DefaultOption();

				$scope.loading = true;
				$scope.ChartHeight = AppDefine.ChartHeight;
				$scope.ExportOptions = AppDefine.ChartExportOptions;

				vm.FromSelDate = new Date();
				vm.ToSelDate = new Date();
				$scope.EmptyMode = false;

				function DefaultOption() {
					if (!$scope.Pram || !$scope.Gui.Opts || $scope.Gui.length == 0 || !$scope.Pram.hasOwnProperty("act") || $scope.Pram.act === null)
						return null;

					var obj = $scope.Gui.Opts[$scope.Pram.act];
					return obj;
				}
				$scope.SearchMode = false;

				var exFileName = AppDefine.ExFName.ConvBySites + $filter('date')(vm.FromSelDate, AppDefine.DFFileName);
				$scope.chartConvSitesEvents = angular.copy(chartSvc.createExportEvent('_cvsites', exFileName));

				angular.element($window).on("resize", WindowResize);

				$scope.GetrptData = function (opt) {
					$scope.Opt = opt;
					if ($scope.Opt.label === 'CUSTOM') {
						$scope.SearchMode = true;
					}
					else {
						$scope.SearchMode = false;
						UpdateStartEnd($scope.Opt);
						GetReport($scope.Opt);
					}
				}

				$scope.chartDSConv = {
					chart: {
						//caption: cmsBase.translateSvc.getTranslate(AppDefine.Resx.COUNT_IN),//;"Conversion by Sites",
						subCaption: "",
						//xAxisname: "Month",
						//yAxisName: "Conversion %",
						numberSuffix: "%",
						showBorder: "1",
						showValues: "0",
						theme: "fint",
						//paletteColors: "#0075c2,#1aaf5d,#f2c500",
						bgColor: "#ffffff",
						showCanvasBorder: "0",
						canvasBgColor: "#ffffff",
						//captionFontSize: "14",
						//subcaptionFontSize: "14",
						subcaptionFontBold: "0",
						divlineColor: "#999999",
						divLineDashed: "1",
						divLineDashLen: "1",
						divLineGapLen: "1",
						showAlternateHGridColor: "0",
						usePlotGradientColor: "0",
						labelDisplay: "rotate",
						slantLabels: "1",
						showXAxisLine: "1",
						showYAxisLine: "1",
						xAxisLineColor: "#999999",
						//toolTipColor: "#ffffff",
						toolTipBorderThickness: "0",
						//toolTipBgColor: "#000000",
						//toolTipBgAlpha: "80",
						toolTipBorderRadius: "0",
						//toolTipPadding: "5",
						toolTipBgColor: "#FFFFFF",
						toolTipBgAlpha: '0',
						plottooltext: "<div class='chart-tooltip'><div class='tooltipHeader'><strong>$label</strong></div><div class='tooltipContent'>$seriesname: $value %</div></div>",
						legendBgColor: "#ffffff",
						legendBorderAlpha: "0",
						legendShadow: "0",
						legendItemFontSize: "10",
						legendItemFontColor: "#666666",
						canvasborderthickness: "1",
						rotatevalues: "1",
						exportEnabled: '1',
						exportAtClient: '0',
						exportHandler: AppDefine.ChartExportURL,
						exportAction: 'download',
						exportShowMenuItem: '0',
						maxLabelHeight: "75"
					},
					categories: [],
					dataset: []
				};

				/****************************** CALENDAR DATA - Begin ********************************/
				var FiscalYearData = null;
				var fyStartDate = {};
				var fyEndDate = {};

				var FiscalYearDataLast = null;
				var fyStartDateLast = {};
				var fyEndDateLast = {};

				vm.minDate = new Date(1970, 0, 1);
				vm.maxDate = new Date();
				vm.dateFormat = AppDefine.CalDateFormat;//"MM/dd/yyyy";
				vm.dateOptions = {
					formatYear: 'yy',
					startingDay: 1
				};

				vm.open = function ($event, idx) {
					$event.preventDefault();
					$event.stopPropagation();
					if (idx == 1)
						vm.opened1 = true;
					else
						vm.opened2 = true;
				};

				vm.Search = function () {
					//if ($("#btn-popMenuConvSites").parent().hasClass("open")) {
					//	$("#btn-popMenuConvSites").parent().removeClass("open");
					//	$("#btn-popMenuConvSites").prop("aria-expanded", false);
					//}
					var delta = chartSvc.dateDiff(vm.ToSelDate, vm.FromSelDate);
					if (delta >= 0) {
						GetReport($scope.Opt);
					}
					else {
						alert(cmsBase.translateSvc.getTranslate(AppDefine.Resx.FROM_TO_DATE_INVALID_MSG));
					}
				}

				//function GetCustFiscalYear(opt) {
				//	var def = cmsBase.$q.defer();
				//	var curDay = chartSvc.FYFormatDate(new Date());
				//	fiscalyearservice.GetCustomFiscalYear(curDay).then(function (data) {
				//		if (data) {
				//			FiscalYearData = data;
				//			fyStartDate = new Date(FiscalYearData.FYDateStart);
				//			fyEndDate = new Date(FiscalYearData.FYDateEnd);

				//			if (opt)
				//				UpdateStartEnd(opt);
				//			def.resolve();
				//		}
				//		else {
				//			def.reject('No data!');
				//		}
				//	},
				//	function (error) {
				//		def.reject(error);
				//	});
				//	return def.promise;
				//}
				function GetCustFiscalYear(opt, cday, isLast) {
					var def = cmsBase.$q.defer();
					var curDay = chartSvc.FYFormatDate(cday);//new Date());
					fiscalyearservice.GetCustomFiscalYear(curDay).then(function (data) {
						if (data) {
							if (isLast) {
								FiscalYearDataLast = data;
								fyStartDateLast = chartSvc.GetUTCDate(FiscalYearDataLast.FYDateStart);
								fyEndDateLast = chartSvc.GetUTCDate(FiscalYearDataLast.FYDateEnd);
								if (opt)
									UpdateStartEndLast(opt, cday);
							}
							else {
								FiscalYearData = data;
								fyStartDate = chartSvc.GetUTCDate(FiscalYearData.FYDateStart);
								fyEndDate = chartSvc.GetUTCDate(FiscalYearData.FYDateEnd);
								if (opt)
									UpdateStartEnd(opt);
							}
							def.resolve();
						}
						else {
							def.reject('No data!');
						}
					},
					function (error) {
						def.reject(error);
					});
					return def.promise;
				}
				function UpdateStartEnd(opt) {
					var curDate = new Date();
					if (opt.hasOwnProperty('week')) {
						if (FiscalYearData) {
							//var lastWeekDay = chartSvc.dateAdds(new Date(), -7);
							//var fw = chartSvc.GetFiscalWeek(FiscalYearData, lastWeekDay);
							//vm.FromSelDate = new Date(fw.StartDate);
							//vm.ToSelDate = new Date(fw.EndDate);
							var fw = chartSvc.GetFiscalWeek(FiscalYearData, curDate);
							var lastWeekDay = chartSvc.dateAdds(fw.StartDate, -1);
							if (lastWeekDay < fyStartDate) {
								if (FiscalYearDataLast == null)
									GetCustFiscalYear(opt, lastWeekDay, true);
								else
									UpdateStartEndLast(opt, lastWeekDay);
								//fwk = chartSvc.GetFiscalWeek(FiscalYearDataLast, lastWeekDay);
							}
							else {
								var fwk = chartSvc.GetFiscalWeek(FiscalYearData, lastWeekDay);
								vm.FromSelDate = new Date(fwk.StartDate);
								vm.ToSelDate = new Date(fwk.EndDate);
							}
						}//FiscalYearData
					}
					else if (opt.hasOwnProperty('month')) {
						if (FiscalYearData) {
							//var fw = chartSvc.GetFiscalWeek(FiscalYearData, new Date());
							if (opt.month == 1) {
								/*
								if (FiscalYearData.FYTypesID == AppDefine.FiscalTypes.NORMAL) {
									var fm = chartSvc.GetCalendarPeriod(new Date(), true);
									vm.FromSelDate = new Date(fm.StartDate);
									vm.ToSelDate = new Date(fm.EndDate);
								}
								else {
									var fm = chartSvc.GetFiscalPeriod(fw.WeekNo, FiscalYearData.CalendarStyle, fyStartDate, fyEndDate, true, FiscalYearData.FYNoOfWeeks);
									vm.FromSelDate = new Date(fm.StartDate);
									vm.ToSelDate = new Date(fm.EndDate);
								}*/
								var fm = chartSvc.GetFiscalPeriodInfo(FiscalYearData, -1, curDate, false);
								var lastPerDay = chartSvc.dateAdds(fm.StartDate, -1);
								if (lastPerDay < fyStartDate) {
									if (FiscalYearDataLast == null)
										GetCustFiscalYear(opt, lastPerDay, true);
									else
										UpdateStartEndLast(opt, lastPerDay);
								}
								else {
									fm = chartSvc.GetFiscalPeriodInfo(FiscalYearData, -1, lastPerDay, false);
									vm.FromSelDate = new Date(fm.StartDate);
									vm.ToSelDate = new Date(fm.EndDate);
								}
							}
							else if (opt.month == 3) {
								var fw = chartSvc.GetFiscalWeek(FiscalYearData, curDate);
								var fq = chartSvc.GetFiscalQuarter(fw.WeekNo, fyStartDate, fyEndDate, false, FiscalYearData.FYNoOfWeeks);
								var lastQuarDay = chartSvc.dateAdds(fq.StartDate, -1);
								if (lastQuarDay < fyStartDate) {
									if (FiscalYearDataLast == null)
										GetCustFiscalYear(opt, lastQuarDay, true);
									else
										UpdateStartEndLast(opt, lastQuarDay);
								}
								else {
									fw = chartSvc.GetFiscalWeek(FiscalYearData, lastQuarDay);
									fq = chartSvc.GetFiscalQuarter(fw.WeekNo, fyStartDate, fyEndDate, false, FiscalYearData.FYNoOfWeeks);

									vm.FromSelDate = new Date(fq.StartDate);
									vm.ToSelDate = new Date(fq.EndDate);
								}
							} //month = 3
						} //FiscalYearData
					} //month
				}
				function UpdateStartEndLast(opt, cday) {
					if (opt.hasOwnProperty('week')) {
						if (FiscalYearDataLast) {
							var fw = chartSvc.GetFiscalWeek(FiscalYearDataLast, cday);
							vm.FromSelDate = new Date(fw.StartDate);
							vm.ToSelDate = new Date(fw.EndDate);
						}//if FiscalYearData
					}
					else if (opt.hasOwnProperty('month')) {
						if (FiscalYearDataLast) {
							if (opt.month == 1) {
								var fm = chartSvc.GetFiscalPeriodInfo(FiscalYearDataLast, -1, cday, false);
								vm.FromSelDate = new Date(fm.StartDate);
								vm.ToSelDate = new Date(fm.EndDate);
							}
							else {
								var fw = chartSvc.GetFiscalWeek(FiscalYearDataLast, cday);
								var fq = chartSvc.GetFiscalQuarter(fw.WeekNo, fyStartDateLast, fyEndDateLast, false, FiscalYearDataLast.FYNoOfWeeks);
								vm.FromSelDate = new Date(fq.StartDate);
								vm.ToSelDate = new Date(fq.EndDate);
							}//month = 3
						}//if FiscalYearData
					} //month
				}
				/****************************** CALENDAR DATA - End **********************************/
				/****************************** TREE-SITE DATA - Begin *******************************/
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

				function selectedFn(node, scope) {
					scope.checkFn(node, scope.props.parentNode, scope);
				}

				function GetAllRegionSites() {
					//if (vm.userLogin.IsAdmin) {
						//dataContext.siteadmin.getAllUserSites(function (data) {
						dataContext.siteadmin.GetSiteByUserId(vm.userLogin.UserID, function (data) {
							vm.sitetreeS = {};
							angular.copy(data, vm.sitetreeS);
							chartSvc.UpdateSiteChecked(vm.sitetreeS, vm.userSelected, true);
							vm.treeSiteFilterS = vm.sitetreeS;
							vm.siteloaded = true;
						}, function (error) {
							var msg = cmsBase.translateSvc.getTranslate(error.data.ReturnMessage[0]);
							cmsBase.cmsLog.error(msg);
						});
					//} else {
					//	//dataContext.siteadmin.getSites(function (data) {
					//	dataContext.siteadmin.GetSiteByUserId(vm.userLogin.UserID, function (data) {
					//		if (!data.Sites) {
					//			return;
					//		}
					//		//chartSvc.UpdateSiteChecked(data, vm.userSelected, true);
					//		//vm.sitetreeS = angular.copy(data);
					//		vm.sitetreeS = {};
					//		angular.copy(data, vm.sitetreeS);
					//		//vm.siteloaded = true;
					//		//vm.treeSiteFilterS = vm.sitetreeS;
					//		chartSvc.UpdateSiteChecked(vm.sitetreeS, vm.userSelected, true);
					//		vm.treeSiteFilterS = vm.sitetreeS;
					//		vm.siteloaded = true;
					//	}, function (error) {
					//		var msg = cmsBase.translateSvc.getTranslate(error.data.ReturnMessage[0]);
					//		cmsBase.cmsLog.error(msg);
					//	});
					//}
				}
				/*
				function UpdateSiteChecked(treeData) {
					if ($.isEmptyObject(treeData.Sites)) {
						angular.forEach(treeData, function (n) {
							if (n.Sites && n.Sites.length > 0) {
								UpdateSiteChecked(n.Sites);
							}
							if (vm.userSelected == null || vm.userSelected == undefined
								|| vm.userSelected.SiteIDs == null || vm.userSelected.SiteIDs == undefined
								|| vm.userSelected.SiteIDs.indexOf(n.ID) < 0) {
								n.Checked = false;
							}
							else {
								n.Checked = true;
							}
						});
					}
					else {
						angular.forEach(treeData.Sites, function (n) {
							if (n.Sites && n.Sites.length > 0) {
								UpdateSiteChecked(n.Sites);
							}
							if (vm.userSelected == null || vm.userSelected == undefined
								|| vm.userSelected.SiteIDs == null || vm.userSelected.SiteIDs == undefined
								|| vm.userSelected.SiteIDs.indexOf(n.ID) < 0) {
								n.Checked = false;
							}
							else {
								n.Checked = true;
							}
						});
					}
				}

				function GetSiteSelectedIDs(siteCheckedIDs, treeData) {
					angular.forEach(treeData, function (n) {
						if (n.Sites && n.Sites.length > 0) {
							GetSiteSelectedIDs(siteCheckedIDs, n.Sites);
						}
						if (n.Type > 0 && n.Checked == 1) {
							siteCheckedIDs.push(n.ID);
						}
					});
				}
				*/
				vm.TreeSiteClose = function () {
					if ($("#btn-popMenuConvSites").parent().hasClass("open")) {
						$("#btn-popMenuConvSites").parent().removeClass("open");
						$("#btn-popMenuConvSites").prop("aria-expanded", false);
					}
					var checkedIDs = [];
					chartSvc.GetSiteSelectedIDs(checkedIDs, vm.treeSiteFilterS.Sites);
					if (checkedIDs && checkedIDs.length > 0) {
						$scope.Pram.value = checkedIDs;
						GetReport($scope.Opt);
					}
					else {
						alert(cmsBase.translateSvc.getTranslate(AppDefine.Resx.SELECT_LEAST_ONE_SITE_MSG));
					}
				}

				//var filterTextTimeout;
				//$scope.$watch('vm.querySite', function (val) {
				//	if (vm.siteloaded == false) return;
				//	if (filterTextTimeout) $timeout.cancel(filterTextTimeout);
				//	filterTextTimeout = $timeout(function () {
				//		if (vm.sitetreeS && vm.sitetreeS.Sites.length > 0 && val) {
				//			//vm.treeSiteFilterS = angular.copy(vm.sitetreeS);
				//			vm.treeSiteFilterS = {};
				//			angular.copy(vm.sitetreeS, vm.treeSiteFilterS);
				//			vm.treeSiteFilterS.Sites = siteadminService.filterSites(vm.sitetreeS, val);
				//		} else {
				//			vm.treeSiteFilterS = vm.sitetreeS;
				//			//vm.treeSiteFilterS.Sites = vm.sitetreeS.Sites;
				//			//if (!$scope.$$phase) {
				//			//	$scope.$apply();
				//			//}
				//		}
				//	}, 50);
				//});
				/****************************** TREE-SITE DATA - End ********************************/
				$scope.Init = function () {
					var opt = DefaultOption();
					if ($scope.Gui && $scope.Gui.Opts) {
						var searchOpt = { label: 'CUSTOM' };
						$scope.Gui.Opts.push(searchOpt);
					}

					GetCustFiscalYear($scope.Opt, new Date(), false).then(function () {
						GetReport($scope.Opt);
						dataContext.injectRepos(['configuration.siteadmin']).then(GetAllRegionSites);
					});
				}

				function GetReport(optVal) {
					var opt = {};
					angular.copy(optVal, opt);
					opt.date = $filter('date')(vm.ToSelDate, AppDefine.DateFormatCParamED);//'yyyyMMdd235959'
					var delta = chartSvc.dateDiff(vm.ToSelDate, vm.FromSelDate);
					utils.RemoveProperty(opt, "week");
					utils.RemoveProperty(opt, "month");
					opt.day = delta;

					var pram = angular.copy($scope.Pram);
					utils.RemoveProperty(pram, "act");
					pram.Top = 25;
					if (opt) {
						var Props = Object.getOwnPropertyNames(opt);
						for (var i = 0; i < Props.length; i++) {
							var propName = Props[i];
							if (!propName.startsWith("$$") && !propName.startsWith("label"))
								utils.AddProperty(pram, propName, opt[propName]);
						}
					}

					rptService.DshConversionSites(pram, RptSuccess, RptError);
				};

				$scope.isActiveOption = function (opt) {
					if ($scope.Opt == null)
						return false;
					var ret = utils.Equal(opt, $scope.Opt);
					return ret;
				}

				function RptSuccess(response) {

					$scope.$parent.isLoading = false;
					if (response === null || response === undefined) {
						$scope.EmptyMode = true;
						return;
					}
					if (response.length == 0)
						$scope.EmptyMode = true;
					else
						$scope.EmptyMode = false;

					CreateChartData(response);

					$scope.loading = true;

				}
				function RptError(response) {
					$scope.EmptyMode = true;
				}

				function isShowValue(itNum, divW) {
					if (itNum <= 0)
						return 0;
					var itSpace = divW / itNum;
					if (itSpace < 25)
						return 0;
					return 1;
				}
				function CreateChartData(resdata) {
					if (resdata === null || resdata === undefined)
						return;
					var datLen = resdata.length;
					var catArray = [];
					var dsArray = [];

					var catSub = [];

					var dsConv = {};
					dsConv.seriesname = cmsBase.translateSvc.getTranslate(AppDefine.Resx.CONVERSION);//"Conversion";
					dsConv.showValues = "1";
					//dsItemIn.color = AppDefine.ChartColor.Blue;
					//dsItemIn.anchorbordercolor = AppDefine.ChartColor.Blue;
					dsConv.data = [];

					var dsLYear = {};
					dsLYear.seriesname = cmsBase.translateSvc.getTranslate(AppDefine.Resx.LAST) + ' ' + cmsBase.translateSvc.getTranslate(AppDefine.Resx.YEAR);//"Last Year";
					dsLYear.renderAs = "area";
					dsLYear.color = "#ccffff";
					dsLYear.anchorbordercolor = AppDefine.ChartColor.Yellow;
					dsLYear.data = [];

					var divWidth = $("#dshChartConvBySite").width(); //dshChartConvBySite;

					for (var i = 0; i < datLen; i++) {
						var catIt = {};
						catIt.label = resdata[i].Label;
						catSub.push(catIt);

						var dsIt1 = {};
						dsIt1.value = resdata[i].Value.toFixed(2);
						dsIt1.color = chartSvc.GetConvColor(resdata[i].Value);
						dsIt1.showValue = isShowValue(datLen, divWidth);
						dsConv.data.push(dsIt1);

						var dsIt2 = {};
						dsIt2.value = resdata[i].LastYear.toFixed(2);
						dsIt2.showValue = 0;
						dsLYear.data.push(dsIt2);
					}
					var catCol = {};
					catCol.category = catSub;
					catArray.push(catCol);

					dsArray.push(dsConv);
					dsArray.push(dsLYear);

					$scope.chartDSConv.categories = catArray;
					$scope.chartDSConv.dataset = dsArray;
				}

				function WindowResize() {
					if ($scope.chartDSConv.dataset && $scope.chartDSConv.dataset.length > 0) {
						var divWidth = $("#dshChartConvBySite").width();
						var convDS = $scope.chartDSConv.dataset[0];
						var datLen = convDS.data.length;
						for (var i = 0; i < datLen; i++) {
							convDS.data[i].showValue = isShowValue(datLen, divWidth);
						}
					}
				} //function
			}
		});
})();

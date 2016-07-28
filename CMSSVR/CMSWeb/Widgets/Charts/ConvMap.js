(function () {
	'use strict';
	define(['cms', 'DataServices/ReportService',
		'Scripts/Services/chartSvc',
		'DataServices/Configuration/fiscalyearservice',
		'configuration/sites/helpers',
		'Scripts/Directives/treeComponent',
		'Scripts/Directives/CmsTreeView'],
		function (cms) {
			cms.register.controller('convRateRegionCtrl', convRateRegionCtrl);
			convRateRegionCtrl.$inject = ['$scope', '$window', 'cmsBase', 'ReportService', 'Utils', 'AppDefine', 'chartSvc', 'fiscalyearservice', '$filter', 'dataContext', '$timeout', 'siteadminService', 'AccountSvc'];
			function convRateRegionCtrl($scope, $window, cmsBase, rptService, utils, AppDefine, chartSvc, fiscalyearservice, $filter, dataContext, $timeout, siteadminService, AccountSvc) {
			//convRateRegionCtrl.$inject = ['$scope', 'cmsBase', 'dataContext', 'Utils', 'AppDefine', 'chartSvc', '$filter'];
			//function convRateRegionCtrl($scope, cmsBase, dataContext, utils, AppDefine, chartSvc, $filter) {
				var vm = this;
				$scope.loading = true;

				var Options = JSON.parse("{" + $scope.widgetModel.TemplateParams + "}");
				$scope.Gui = (Options == null || !Options.hasOwnProperty("Gui")) ? null : Options.Gui;
				$scope.Pram = (Options == null || !Options.hasOwnProperty("Pram")) ? null : Options.Pram;
				$scope.Opt = DefaultOption();

				$scope.ChartHeight = AppDefine.ChartHeight;
				$scope.ExportOptions = AppDefine.ChartExportOptions;
				$scope.EmptyMode = false;

				vm.FromSelDate = new Date();
				vm.ToSelDate = new Date();

				function DefaultOption() {
					if (!$scope.Pram || !$scope.Gui.Opts || $scope.Gui.Opts.length == 0 || !$scope.Pram.hasOwnProperty("act") || $scope.Pram.act === null)
						return null;
					var obj = $scope.Gui.Opts[$scope.Pram.act];
					return obj;
				}
				$scope.SearchMode = false;

				var exFileName = AppDefine.ExFName.ConvByRegions + $filter('date')(vm.FromSelDate, AppDefine.DFFileName);
				$scope.chartConvRegEvents = angular.copy(chartSvc.createExportEvent('_cvreg', exFileName));

				$scope.isActiveOption = function (opt) {
					if ($scope.Opt == null)
						return false;
					var ret = utils.Equal(opt, $scope.Opt);
					return ret;
				}
				angular.element($window).on("resize", WindowResize);

				/****************************** CHART DATA - Begin ********************************/
				$scope.RegionDataset = null;
				$scope.RegionCategories = null;
				$scope.SiteDataset = null;
				$scope.SiteCategories = null;
				$scope.chartRegionConv = {
					chart: {
						/*
						//caption: AppDefine.Resx.CHART_ALERT,
						subCaption: "",
						captionAlignment: "center",
						//numDivLines: "6",
						numbersuffix: "%",
						theme: "fint",
						showBorder: "1",
						showXAxisLine: "1",
						showYAxisLine: "1",
						xAxisLineColor: "#999999",
						toolTipBgColor: "#FFFFFF",
						toolTipBgAlpha: '0',
						plottooltext: "<div class='chart-tooltip'><div class='tooltipHeader'><strong>$label</strong></div><div class='tooltipContent'>$seriesname: $value %</div></div>",
						exportEnabled: '1',
						exportAtClient: '0',
						exportHandler: AppDefine.ChartExportURL,
						exportAction: 'download',
						exportShowMenuItem: '0',
						labelDisplay: "rotate",
						slantLabels: "1",
						labelFontSize: "12",
						maxLabelHeight: "75"
						//canvasPadding: '40'
						//chartLeftMargin: 0,
						//chartTopMargin: 2,
						//chartRightMargin: 0,
						//chartBottomMargin: 2*/

						subCaption: "",
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
					//data: []
				};
				$scope.mapDSConv = {
					/*map: {
						animation: "0",
						showbevel: "0",
						usehovercolor: "1",
						canvasbordercolor: "FFFFFF",
						bordercolor: "FFFFFF",
						showlegend: "0",
						showshadow: "0",
						legendposition: "BOTTOM",
						legendborderalpha: "0",
						legendbordercolor: "ffffff",
						legendallowdrag: "0",
						legendshadow: "0",
						caption: "Conversion Rate Map",
						connectorcolor: "000000",
						fillalpha: "80",
						hovercolor: "CCCCCC",
						showborder: 0,
						theme: "fint",
						formatNumberScale: "0",
						numberSuffix: "%"
					},*/
					chart: {
						//caption: "Conversion Rate Map",
						theme: "fint",
						formatNumberScale: "0",
						numberSuffix: "%",
						nullEntityColor: "#C2C2D6",
						nullEntityAlpha: "50",
						hoverOnNull: "0",
						//showBorder: "1",
						//bordercolor: "#000000",
						showMarkerLabels: "1",
						toolTipBgColor: "#FFFFFF",
						toolTipBgAlpha: '0'
					},
					colorrange: {
						//minvalue: "0",
						//startlabel: "Low",
						//endlabel: "High",
						//code: "e44a00",
						//gradient: "1",
						color: []
					},
					data: []
				};
				$scope.mapType = "maps/usa";
				var stateData = {
					map: {
						showshadow: "0",
						showlabels: "0",
						showmarkerlabels: "1",
						fillcolor: "C2C2D6",
						bordercolor: "CCCCCC",
						basefont: "Verdana",
						basefontsize: "10",
						markerbordercolor: "000000",
						markerbgcolor: "FF5904",
						markerradius: "6",
						usehovercolor: "0",
						hoveronempty: "0",
						showmarkertooltip: "1",
						canvasBorderColor: "375277",
						canvasBorderAlpha: "0",
						canvasborderthickness: "1"
					},
					markers: {
						shapes: [
							{
								id: "Shap1",
								type: "circle",
								fillcolor: "FFFFFF,5A9502",
								fillpattern: "radial",
								showborder: "0",
								radius: "5",
								labelPadding: "15"
							},
							{
								id: "Shap2",
								type: "circle",
								fillcolor: "FFFFFF,000099",
								fillpattern: "radial",
								showborder: "0",
								radius: "5"
							}
						],
						items: []
					}
				};
				/*
				items: [{
								id: "TG",
								shapeid: "Shap1",
								x: "250.4",
								y: "300.34",
								label: "Demo Store: ABC"
							}]
				*/
				function ClearChartData() {
					$scope.RegionDataset = null;
					$scope.RegionCategories = null;
					$scope.SiteDataset = null;
					$scope.SiteCategories = null;
				}
				/****************************** CHART DATA - End *********************************/
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
					if(idx == 1)
						vm.opened1 = true;
					else
						vm.opened2 = true;
				};

				vm.Search = function () {
					//if ($("#btn-popMenuConvMap").parent().hasClass("open")) {
					//	$("#btn-popMenuConvMap").parent().removeClass("open");
					//	$("#btn-popMenuConvMap").prop("aria-expanded", false);
					//}

					//alert('alert');
					var delta = chartSvc.dateDiff(vm.ToSelDate, vm.FromSelDate);
					if (delta >= 0) {
						ClearChartData();
						GetReport($scope.Opt);
					}
					else {
						alert(cmsBase.translateSvc.getTranslate(AppDefine.Resx.FROM_TO_DATE_INVALID_MSG));
					}
				}

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
							//var lastWeekDay = chartSvc.dateAdds(curDate, -7);//new Date()
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
						}//if FiscalYearData
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
					Icon: {
						Item: ' icon-home'
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
				vm.query = '';
				//vm.sitetree = true;
				vm.treeSiteFilter = null;
				vm.sitetree = null;
				vm.siteloaded = false;

				function selectedFn(node, scope) {
					scope.checkFn(node, scope.props.parentNode, scope);
				}

				function GetAllRegionSites() {
					//if (vm.userLogin.IsAdmin) {
						//dataContext.siteadmin.getAllUserSites(function (data) {
						dataContext.siteadmin.GetSiteByUserId(vm.userLogin.UserID, function (data) {
							vm.sitetree = {};
							angular.copy(data, vm.sitetree);

							chartSvc.UpdateSiteChecked(vm.sitetree, vm.userSelected, true);
							vm.treeSiteFilter = vm.sitetree;
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
					//		vm.sitetree = {};
					//		angular.copy(data, vm.sitetree);

					//		chartSvc.UpdateSiteChecked(vm.sitetree, vm.userSelected, true);
					//		vm.treeSiteFilter = vm.sitetree;
					//		vm.siteloaded = true;
					//	}, function (error) {
					//		var msg = cmsBase.translateSvc.getTranslate(error.data.ReturnMessage[0]);
					//		cmsBase.cmsLog.error(msg);
					//	});
					//}
				}
				/*
				function UpdateSiteChecked(treeData) {
					if (treeData == null || treeData == undefined) {
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
									|| vm.userSelected.SiteIDs.indexOf(n.ID) < 0) {
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
									|| vm.userSelected.SiteIDs.indexOf(n.ID) < 0) {
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
					if (treeData == null || treeData == undefined) {
						return;
					}
					angular.forEach(treeData, function (n) {
						if (n != null && n != undefined) {
							if (n.Sites && n.Sites.length > 0) {
								GetSiteSelectedIDs(siteCheckedIDs, n.Sites);
							}
							if (n.Type > 0 && n.Checked == 1) {
								siteCheckedIDs.push(n.ID);
							}
						}
					});
				}*/
				vm.TreeSiteClose = function () {
					if ($("#btn-popMenuConvMap").parent().hasClass("open")) {
						$("#btn-popMenuConvMap").parent().removeClass("open");
						$("#btn-popMenuConvMap").prop("aria-expanded", false);
					}
					var checkedIDs = [];
					chartSvc.GetSiteSelectedIDs(checkedIDs, vm.treeSiteFilter.Sites);
					if (checkedIDs && checkedIDs.length > 0) {
						$scope.Pram.value = checkedIDs;
						GetReport($scope.Opt);
					}
					else {
						alert(cmsBase.translateSvc.getTranslate(AppDefine.Resx.SELECT_LEAST_ONE_SITE_MSG));
					}
				}

				//var filterTextTimeout;
				//$scope.$watch('vm.query', function (val) {
				//	if (vm.siteloaded == false) return;
				//	if (filterTextTimeout) $timeout.cancel(filterTextTimeout);
				//	filterTextTimeout = $timeout(function () {
				//		if (vm.sitetree && vm.sitetree.Sites.length > 0 && val) {
				//			//vm.treeSiteFilter = angular.copy(vm.sitetree);
				//			vm.treeSiteFilter = {};
				//			angular.copy(vm.sitetree, vm.treeSiteFilter);
				//			vm.treeSiteFilter.Sites = siteadminService.filterSites(vm.treeSiteFilter, val);
				//		} else {
				//			vm.treeSiteFilter = vm.sitetree;
				//			//vm.treeSiteFilter.Sites = vm.sitetree.Sites;
				//			//if (!$scope.$$phase) {
				//			//	$scope.$apply();
				//			//}
				//		}
				//	}, 50);
				//});
				/****************************** TREE-SITE DATA - End ********************************/
				$scope.GetrptData = function (opt) {
					$scope.Opt = opt;
					if ($scope.Opt.label === 'CUSTOM') {
						$scope.SearchMode = true;
					}
					else {
						ClearChartData();
						$scope.SearchMode = false;
						UpdateStartEnd($scope.Opt);
						GetReport($scope.Opt);
					}
				}

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

				$scope.showstate = false;
				$scope.BackUSAMap = function () {
					$scope.showstate = false;
					$scope.chartRegionConv.categories = $scope.RegionCategories;
					$scope.chartRegionConv.dataset = $scope.RegionDataset;
				}
				$scope.mapEvents = {
					entityClick: function (evt, data) {
						var mapState = data.label.replace(/\s/g, '').toLowerCase();

						$scope.mapType = "maps/" + mapState;//"maps/alabama";//
						$scope.mapStateDSConv = stateData;
						$scope.showstate = true;

						var curStateData = Enumerable.From($scope.mapDSConv.data).Where('x => x.id == "' + data.originalId + '"').FirstOrDefault();
						if (curStateData) {
							var stateid = curStateData.stateid;

							//if ($scope.SiteDataset == null || $scope.SiteDataset == undefined) {
								GetSiteReport($scope.Opt, stateid);
							//}
							//else {
							//	$scope.chartRegionConv.categories = $scope.SiteCategories;
							//	$scope.chartRegionConv.dataset = $scope.SiteDataset;
							//}
						}
						if (!$scope.$$phase) {
							$scope.$apply();
						}
					}
				};
				function CreateOptParam(optVal) {
					var opt = {};
					angular.copy(optVal, opt);
					opt.date = $filter('date')(vm.ToSelDate, AppDefine.DateFormatCParamED);
					var delta = chartSvc.dateDiff(vm.ToSelDate, vm.FromSelDate);
					utils.RemoveProperty(opt, "week");
					utils.RemoveProperty(opt, "month");
					opt.day = delta;
					return opt;
				}

				function GetReport(optVal) {
					//var opt = {};
					//angular.copy(optVal, opt);
					//opt.date = $filter('date')(vm.ToSelDate, AppDefine.DateFormatCParamED);
					//var delta = chartSvc.dateDiff(vm.ToSelDate, vm.FromSelDate);
					//utils.RemoveProperty(opt, "week");
					//utils.RemoveProperty(opt, "month");
					//opt.day = delta;
					var opt = CreateOptParam(optVal);

					$scope.showstate = false;
					var pram = angular.copy( $scope.Pram );
					utils.RemoveProperty(pram, "act");

					var Props = Object.getOwnPropertyNames( opt );
					for ( var i = 0; i < Props.length; i++ ) {
						var propName = Props[i];
						if ( !propName.startsWith( "$$" ) && !propName.startsWith( "label" ) )
							utils.AddProperty( pram, propName, opt[propName] );
					}

					rptService.DshConversionMap( pram, RptSuccess, RptError );
				};

				function RptSuccess( response ) {
					$scope.$parent.isLoading = false;
					if (response === null || response === undefined) {
						$scope.EmptyMode = true;
						return;
					}
					if (response.length == 0)
						$scope.EmptyMode = true;
					else
						$scope.EmptyMode = false;

					for (var i = 0; i < response.length; i++) {
						response[i].color = chartSvc.GetConvColor(response[i].Value);
						response[i].Value = response[i].Value.toFixed(2);
						response[i].LastYear = response[i].LastYear.toFixed(2);
					}
					//$scope.chartRegionConv.data = response;
					CreateChartData(response, true);

					var colorRange = Enumerable.From(AppDefine.ConvMapRanges)
						.Select(function (x) { var retColor = {}; retColor.minvalue = x.min; retColor.maxvalue = x.max; retColor.code = x.color; retColor.displayvalue = (x.max > 0 && x.max < 100) ? 'Conv < ' + x.max + '%' : 'Conv >= ' + x.min + '%'; return retColor; })
						.ToArray();
					$scope.mapDSConv.colorrange.color = colorRange;

					var queryResult = Enumerable.From(response)
						.Where(function (it) { return (it != null) && (it.Code != null && it.Code.length > 0) })
						.Select(function (x) { var retVal = {}; retVal.id = x.Code; retVal.value = x.Value; retVal.stateid = x.StateID; retVal.tooltext = "<div class='chart-tooltip'><div class='tooltipHeader'><strong>" + x.Label + "</strong></div><div class='tooltipContent'>Conversion: $value %</div></div>"; return retVal; })
						.ToArray();

					$scope.mapDSConv.data = queryResult;
					//CreateChartData(response);
					$scope.loading = true;
				}
				function RptError(response) {
					$scope.EmptyMode = true;
				}
				//function GetColor(val) {
				//	var color = Enumerable.From(AppDefine.ConvMapRanges)
				//		.Where(function (i) { return (i.min <= val) && (i.max > val || i.max == 100) })
				//		.Select(function (x) { return x.color; })
				//		.First();
				//	return color;
				//}
				function isShowValue(isReg, itNum, divW) {
					if (itNum <= 0)
						return 0;
					var itSpace = divW / itNum;
					if (itSpace < 25)
						return 0;
					//if (isReg) {
					//}
					//else {
					//	if (datLen > 15) {
					//		return 0;
					//	}
					//}
					return 1;
				}
				function CreateChartData(resdata, isRegs) {
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
					dsLYear.color = "#aaaacc";
					dsLYear.plotFillAlpha = "10";
					dsLYear.showBorder = "1";
					dsLYear.anchorbordercolor = AppDefine.ChartColor.Yellow;
					dsLYear.data = [];

					var divWidth = $("#dshChartConvMap").width();
					var siItems = [];
					for (var i = 0; i < datLen; i++) {
						var catIt = {};
						catIt.label = resdata[i].Label;
						catSub.push(catIt);

						var dsIt1 = {};
						dsIt1.value = resdata[i].Value;
						dsIt1.color = resdata[i].color;
						dsIt1.showValue = isShowValue(isRegs, datLen, divWidth);
						dsConv.data.push(dsIt1);

						var dsIt2 = {};
						dsIt2.value = resdata[i].LastYear;

						dsLYear.data.push(dsIt2);

						if (!isRegs) {
							var sit = {};
							sit.id = "TG" + i;
							sit.shapeid = "Shap1";
							sit.x = 100;// + (10 * i);
							sit.y = 50 + (50 * i);
							sit.label = resdata[i].Label;
							sit.labelpos = "right";
							siItems.push(sit);
						}
					} //for
					var catCol = {};
					catCol.category = catSub;
					catArray.push(catCol);

					dsArray.push(dsConv);
					dsArray.push(dsLYear);
					if (isRegs == true) {
						$scope.RegionDataset = dsArray;
						$scope.RegionCategories = catArray;
						$scope.chartRegionConv.categories = $scope.RegionCategories;
						$scope.chartRegionConv.dataset = $scope.RegionDataset;
					}
					else {
						$scope.SiteDataset = dsArray;
						$scope.SiteCategories = catArray;
						$scope.chartRegionConv.categories = $scope.SiteCategories;
						$scope.chartRegionConv.dataset = $scope.SiteDataset;
						$scope.chartRegionConv.chart.rotatevalues = 1;

						stateData.markers.items = siItems;
					}
				}

				function GetSiteReport(optVal, staid) {
					var opt = CreateOptParam(optVal);

					var pram = angular.copy($scope.Pram);
					utils.RemoveProperty(pram, "act");
					pram.SID = staid;
					pram.Top = 25;
					//pram.start = $filter('date')(vm.FromSelDate, 'yyyyMMdd000000');
					//pram.end = $filter('date')(vm.ToSelDate, 'yyyyMMdd235959');

					var Props = Object.getOwnPropertyNames(opt);
					for (var i = 0; i < Props.length; i++) {
						var propName = Props[i];
						if (!propName.startsWith("$$") && !propName.startsWith("label"))
							utils.AddProperty(pram, propName, opt[propName]);
					}

					rptService.DshConversionSites(pram, RptSiteSuccess, RptSiteError);
				};
				function RptSiteSuccess(response) {
					//response = testRespone;
					$scope.$parent.isLoading = false;
					if (response === null || response === undefined) {
						return;
					}
					for (var i = 0; i < response.length; i++) {
						response[i].color = chartSvc.GetConvColor(response[i].Value);
						response[i].Value = response[i].Value.toFixed(2);
						response[i].LastYear = response[i].LastYear.toFixed(2);
					}
					//$scope.chartRegionConv.data = response;
					CreateChartData(response, false);
				}
				function RptSiteError(response) {
				}

				$scope.isFull = false;
			    // Tri add feature scroll fullsize
				$scope.$parent.$parent.$watch('isMax', function (newValue, oldValue) {
				    if (newValue !== oldValue) {
				        $scope.isFull = newValue;
				    }
				});

				function WindowResize() {
					if ($scope.chartRegionConv.dataset && $scope.chartRegionConv.dataset.length > 0) {
						var divWidth = $("#dshChartConvMap").width();
						var convDS = $scope.chartRegionConv.dataset[0];
						var datLen = convDS.data.length;
						for (var i = 0; i < datLen; i++) {
							convDS.data[i].showValue = isShowValue(true, datLen, divWidth);
						}
					}
				} //function
			}
		});
})();

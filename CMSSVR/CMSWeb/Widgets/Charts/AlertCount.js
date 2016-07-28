(function () {
	'use strict';
	define(['cms', 'DataServices/ReportService', 'Scripts/Services/chartSvc', 'DataServices/Configuration/fiscalyearservice'],
		function (cms) {
			cms.register.controller('alertCountCtrl', alertCountCtrl);
			alertCountCtrl.$inject = ['$scope', 'ReportService', 'cmsBase', 'Utils', 'AppDefine', 'chartSvc', 'fiscalyearservice', '$filter', '$timeout'];
			function alertCountCtrl($scope, rptService, cmsBase, utils, AppDefine, chartSvc, fiscalyearservice, $filter, $timeout) {
				var vm = this;

				var Options = JSON.parse( "{" + $scope.widgetModel.TemplateParams + "}" );
				$scope.Gui = ( Options == null || !Options.hasOwnProperty( "Gui" ) ) ? null : Options.Gui;
				$scope.Pram = ( Options == null || !Options.hasOwnProperty( "Pram" ) ) ? null : Options.Pram;
				$scope.Opt = DefaultOption();

				$scope.loading = false;
				//$scope.ChartDatas = [];
				$scope.chartType = '';
				$scope.ChartHeight = AppDefine.ChartHeight;
				$scope.ExportOptions = AppDefine.ChartExportOptions;
				$scope.AlertSeverity = AppDefine.AlertSeverity;
				vm.SeverityTip = '';

				vm.FromSelDate = new Date();
				vm.ToSelDate = new Date();
				$scope.EmptyMode = false;
				$scope.showSeverity = false;

				$scope.isActiveOption = function( opt )
				{
					if ( $scope.Opt == null )
						return false;
					var ret = utils.Equal( opt, $scope.Opt );
					return ret;
				}
				$scope.CountName = "Alerts";
				var includeImage = false;

				function DefaultOption() {
					if ( !$scope.Pram || !$scope.Gui.Opts || $scope.Gui.length == 0 ||!$scope.Pram.hasOwnProperty("act") ||$scope.Pram.act === null )
						return null;
					$scope.showSeverity = false;
					var exFileName = '';
					if ($scope.Pram.hasOwnProperty("cmp")) {
						$scope.chartType = $scope.Pram.cmp;

						includeImage = false;
						if ($scope.Pram.cmp === AppDefine.BarChartName.DVRCount) {
							$scope.CountName = "DVRs";
							exFileName = AppDefine.ExFName.DVRCount + $filter('date')(vm.FromSelDate, AppDefine.DFFileName);
						}
						else if ($scope.Pram.cmp === AppDefine.BarChartName.MostAlertDVR) {
							includeImage = true;
							exFileName = AppDefine.ExFName.MostAlertDVR + $filter('date')(vm.FromSelDate, AppDefine.DFFileName);
						}
						else if ($scope.Pram.cmp === AppDefine.BarChartName.Alert) {
							$scope.showSeverity = true;
							exFileName = AppDefine.ExFName.Alert + $filter('date')(vm.FromSelDate, AppDefine.DFFileName);
						}
						else {
							exFileName = $scope.chartType;
						}
					}
					//var exFileName = $scope.chartType + "_" + $filter('date')(vm.FromSelDate, AppDefine.DFFileName);
					$scope.chartAlertEvents = angular.copy(chartSvc.createExportEvent($scope.chartType, exFileName));

					var obj = $scope.Gui.Opts[$scope.Pram.act];
					return obj;
				}
				$scope.SearchMode = false;

				$scope.GetrptData = function (opt) {
					//$scope.Opt = opt;
					//GetReport($scope.Opt)
					$scope.Opt = opt;
					if ($scope.Opt.label === 'CUSTOM') {
						$scope.SearchMode = true;
					}
					else {
						$scope.SearchMode = false;
						UpdateStartEnd($scope.Opt);
						GetReport($scope.Opt)
					}
				}
				/****************************** CALENDAR DATA - Begin ********************************/
				var FiscalYearData = null;
				var fyStartDate = {};
				var fyEndDate = {};

				vm.minDate = new Date(1970, 0, 1);
				vm.maxDate = new Date();
				vm.dateFormat = AppDefine.CalDateFormat;//"MM/dd/yyyy";
				vm.dateOptions = {
					formatYear: 'yy',
					startingDay: 1
				};
				vm.datestatus = {};
				vm.open = function ($event, elementOpened, elementClose) {
					$event.preventDefault();
					$event.stopPropagation();
					vm.datestatus[elementOpened] = !vm.datestatus[elementOpened];
					if (vm.datestatus[elementClose] == true) {
					    vm.datestatus[elementClose] = false;
					}
				};

				vm.Search = function () {
					var delta = chartSvc.dateDiff(vm.ToSelDate, vm.FromSelDate);
					if (delta >= 0) {
						GetReport($scope.Opt)
					}
					else {
						alert(cmsBase.translateSvc.getTranslate(AppDefine.Resx.FROM_TO_DATE_INVALID_MSG));
					}
				}

				function GetCustFiscalYear(opt) {
					var def = cmsBase.$q.defer();
					var curDay = chartSvc.FYFormatDate(new Date());
					fiscalyearservice.GetCustomFiscalYear(curDay).then(function (data) {
						if (data) {
							FiscalYearData = data;
							fyStartDate = new Date(FiscalYearData.FYDateStart);
							fyEndDate = new Date(FiscalYearData.FYDateEnd);

							if (opt)
								UpdateStartEnd(opt);
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
					//var dtToday = new Date();
					if (opt.hasOwnProperty('hour')) {
						vm.FromSelDate = new Date();
						vm.ToSelDate = new Date();
					}
					else if (opt.hasOwnProperty('day')) {
						vm.FromSelDate = chartSvc.dateAdds(new Date(), 1 - opt.day);
						vm.ToSelDate = new Date();
					}
					else if (opt.hasOwnProperty('week')) {
						vm.FromSelDate = chartSvc.dateAdds(new Date(), 1 - (opt.week * 7));
						vm.ToSelDate = new Date();
					}
					/*
					if (opt.hasOwnProperty('week')) {
						if (FiscalYearData) {
							var lastWeekDay = chartSvc.dateAdds(new Date(), -7);
							var fw = chartSvc.GetFiscalWeek(FiscalYearData, lastWeekDay);
							vm.FromSelDate = new Date(fw.StartDate);
							vm.ToSelDate = new Date(fw.EndDate);
						}
					}
					else if (opt.hasOwnProperty('month')) {
						if (FiscalYearData) {
							var fw = chartSvc.GetFiscalWeek(FiscalYearData, new Date());
							if (opt.month == 1) {
								var fm = chartSvc.GetFiscalPeriod(fw.WeekNo, FiscalYearData.CalendarStyle, fyStartDate, fyEndDate, true, FiscalYearData.FYNoOfWeeks);
								vm.FromSelDate = new Date(fm.StartDate);
								vm.ToSelDate = new Date(fm.EndDate);
							}
							else if (opt.month == 3) {
								var fq = chartSvc.GetFiscalQuarter(fw.WeekNo, fyStartDate, fyEndDate, true, FiscalYearData.FYNoOfWeeks);
								vm.FromSelDate = new Date(fq.StartDate);
								vm.ToSelDate = new Date(fq.EndDate);
							}
						}
					}*/
				}
				/****************************** CALENDAR DATA - End **********************************/
				vm.RefreshData = function () {
					var popupID = "#btn-popMenuAlert_" + $scope.chartType;
					if ($(popupID).parent().hasClass("open")) {
						$(popupID).parent().removeClass("open");
						$(popupID).prop("aria-expanded", false);
					}
					//var checkedIDs = [];
					//chartSvc.GetSiteSelectedIDs(checkedIDs, vm.treeSiteFilter.Sites);
					//$scope.Pram.value = checkedIDs;
					GetReport($scope.Opt)
				}

				$scope.Init = function () {
					var opt = DefaultOption();
					//GetReport( $scope.Opt )
					if ($scope.Gui && $scope.Gui.Opts) {
						var searchOpt = { label: 'CUSTOM' };
						$scope.Gui.Opts.push(searchOpt);
					}

					GetReport($scope.Opt);
					//GetCustFiscalYear($scope.Opt).then(function () {
					//	GetReport($scope.Opt)
					//});
				}

				$scope.$on('changedLanguage_Severity', function () {

				    $timeout(function () {
				        if ($scope.showSeverity == true) {
				            vm.SeverityTip = '';
				            var checkedIDs = [];
				            for (var i = 0; i < $scope.AlertSeverity.length; i++) {
				                if ($scope.AlertSeverity[i].checked == true) {
				                    checkedIDs.push($scope.AlertSeverity[i].id);
				                    vm.SeverityTip = vm.SeverityTip + cmsBase.translateSvc.getTranslate($scope.AlertSeverity[i].label) + ", ";
				                }
				            }
				            if (vm.SeverityTip.length > 2) {
				                vm.SeverityTip = vm.SeverityTip.substr(0, vm.SeverityTip.length - 2);
				            }

				            $scope.Pram.value = checkedIDs;
				        }
				    }, 500);

				});

				function GetReport(optVal) {
					var opt = {};
					angular.copy(optVal, opt);
					opt.date = $filter('date')(vm.ToSelDate, AppDefine.DateFormatCParamED);//'yyyyMMdd235959'
					var delta = chartSvc.dateDiff(vm.ToSelDate, vm.FromSelDate);
					utils.RemoveProperty(opt, "week");
					utils.RemoveProperty(opt, "month");
					utils.RemoveProperty(opt, "hour");
					opt.day = delta;

					if ($scope.showSeverity == true) {
						vm.SeverityTip = '';
						var checkedIDs = [];
						for (var i = 0; i < $scope.AlertSeverity.length; i++) {
							if ($scope.AlertSeverity[i].checked == true) {
								checkedIDs.push($scope.AlertSeverity[i].id);
								vm.SeverityTip = vm.SeverityTip + cmsBase.translateSvc.getTranslate($scope.AlertSeverity[i].label) + ", ";
							}
						}
						if (vm.SeverityTip.length > 2) {
							vm.SeverityTip = vm.SeverityTip.substr(0, vm.SeverityTip.length - 2);
						}

						$scope.Pram.value = checkedIDs;
					}

					var pram = angular.copy($scope.Pram);
					utils.RemoveProperty( pram, "act" );
					var Props = Object.getOwnPropertyNames( opt );
					for ( var i = 0; i < Props.length; i++ ) {
						var propName = Props[i];
						if ( !propName.startsWith( "$$" ) && !propName.startsWith( "label" ) )
						utils.AddProperty( pram, propName, opt[propName] );
					}

					rptService.DshAlerts( pram, RptSuccess, RptError );
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

					$scope.filter = 1;
					CreateChartData( response );
					$scope.loading = true;
				}
				function RptError(response) {
					$scope.EmptyMode = true;
				}

				function BuildToolTip(val) {
					if (includeImage && val.ImageSrc != null && val.ImageSrc != 'undefined') {
						return "<div class='chart-tooltip'><div class='tooltipImg'><img src='data:image/jpeg;base64," + val.ImageSrc + "'  /></div><div class='tooltipHeader'>$label</div><div class='tooltipContent'>" + $scope.CountName + ": $value</div></div>";
					}
					else {
						return "<div class='chart-tooltip'><div class='tooltipHeader'>$label</div><div class='tooltipContent'>" + $scope.CountName + ": $value</div></div>";
					}
				}
				$scope.chartDSAlerts = {
						chart: {
							//caption: AppDefine.Resx.CHART_ALERT,
							subCaption: "",
							captionAlignment: "center",
							//numDivLines: "6",
							theme: "fint",
							showBorder: "1",
							toolTipBgColor: "#FFFFFF",
							toolTipBgAlpha: '0',
							//plottooltext: "<div class='chart-tooltip'><div class='tooltipHeader'>$label</div><div class='tooltipContent'>" + $scope.CountName + ": $value</div></div>",
							formatNumberScale: '0',
							exportEnabled: '1',
							exportAtClient: '0',
							exportHandler: AppDefine.ChartExportURL,
							exportAction: 'download',
							exportShowMenuItem: '0',
							showXAxisLine: "1",
							showYAxisLine: "1",
							//labelDisplay: "auto",
							//slantLabels: "1",
							showLabels: "1",
							labelFontSize: "11",
							maxLabelWidthPercent: "30",
							useEllipsesWhenOverflow: "1",
							placeValuesInside: '0',
							valueFontColor: "#000000"
							//canvasPadding: '1'
							//chartLeftMargin: 0,
							//chartTopMargin: 2,
							//chartRightMargin: 0,
							//chartBottomMargin: 2
						},
						data: []
				};
				function GetColor(val) {
					var color = Enumerable.From(AppDefine.AlertCountRanges)
						.Where(function (i) { return (i.min <= val) && (i.max > val || i.max == -1) })
						.Select(function (x) { return x.color; })
						.First();
					return color;
				}

				function CreateChartData(resdata) {
					if (resdata === null || resdata === undefined)
						return;
					var convArray = resdata;
					//$scope.ChartDatas = convArray;
					for (var i = 0; i < convArray.length; i++) {
						convArray[i].color = GetColor(convArray[i].Value);
						convArray[i].tooltext = BuildToolTip(convArray[i]);
					}
					//UpdateTooltip();
					$scope.chartDSAlerts.data = convArray;
				}
			}
		});
})();
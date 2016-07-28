(function () {
	'use strict';
	define(['cms',
		'widgets/bam/modal/metricview',
		'widgets/bam/charts/convRegion',
		'widgets/bam/charts/bamTraffic',
		'Scripts/Services/chartSvc',
		'Scripts/Services/bamhelperSvc',
		'DataServices/Configuration/fiscalyearservice',
		'DataServices/Bam/DashboardReportsSvc', 'DataServices/ReportService'
	], function (cms) {
		cms.register.controller('dashboardCtrl', dashboardCtrl);

		dashboardCtrl.$inject = ['$rootScope', '$scope', '$modal', 'cmsBase', 'dataContext', 'AppDefine', '$timeout', 'siteadminService', 'AccountSvc', 'DashboardReportsSvc', '$filter', 'chartSvc', 'fiscalyearservice', 'bamhelperSvc', 'Utils', 'ReportService', '$state', 'exportSvc'];

		function dashboardCtrl($rootScope, $scope, $modal, cmsBase, dataContext, AppDefine, $timeout, siteadminService, AccountSvc, DashboardReportsSvc, $filter, chartSvc, fiscalyearservice, bamhelperSvc, utils, rptService, $state, exportSvc) {
			var vm = this;
			$scope.BAMDateFormat = AppDefine.BAMDateFormat;
			vm.selectedSites = [];
			vm.fiscalyear = null;
			$scope.isShowNormalize = false;
			$scope.dayofweek = [];
			var siteDatas = [];
			$scope.ReportDate = $rootScope.BamFilter.dateReport;
			$scope.legendTraffic = {
				CountIn: true,
				CountOut: true,
				Forecast: true
			};

			$scope.$on(AppDefine.Events.CHECKNODE, function (e, flag) {
				getSelectedSites();
				$scope.isShowNormalize = flag;
				$scope.$applyAsync();
			});

			$scope.$on(AppDefine.Events.GETBAMREPORTDATA, function (e) {
				//Click Refresh button
				active();
			});

			$scope.$on(AppDefine.Events.EXPORTEVENT, function (event, data) {
				var userLogin = AccountSvc.UserModel();
				var options = {};

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
				options = { ColIndex: 1, RowIndex: 1 };
				var sumtable = buildBamSumTable(vm.headSummary, options);
				tables.push(sumtable);
				options = { ColIndex: 1, RowIndex: options.RowIndex + sumtable.RowDatas.length + 2 };
				var detailTable = buildBamSumDetailTable($scope.TableSumaryDetail, options);
				tables.push(detailTable);

				var charts = [];
				options = { ColIndex: 1, RowIndex: options.RowIndex + detailTable.RowDatas.length + 2, Width: 9, Height: 18 };
				var convData = Enumerable.From(JSON.parse(sessionStorage.ConversionRegionsChart)).OrderByDescending(function (o) { return o.AvgConv; }).ToArray();
				var convChart = buildConvChart(convData, options);
				charts.push(convChart);
				options = { ColIndex: options.Width, RowIndex: options.RowIndex, Width: 9, Height: 18 };
				var trafficData = JSON.parse(sessionStorage.BamTraffic);
				var trafficChart = buildTrafficChart(trafficData, options);
				charts.push(trafficChart);

				exportSvc.exportXlsFromServer(reportInfo, tables, charts);
			});

			active(); //page load (click on menu)

			function active() {
				if ($scope.$parent.$parent.treeSiteFilter === undefined) { return; }
				getSelectedSites();
				if (!vm.selectedSites || vm.selectedSites.length == 0) {
					cmsBase.cmsLog.warning(cmsBase.translateSvc.getTranslate(AppDefine.Resx.SELECT_LEAST_ONE_SITE_MSG));
					return;
				}

				LoadData();
			}

			function LoadData() {
				getFiscalYear($rootScope.BamFilter.dateReport).then(function () {
					getMetricSumary(vm.filterParam);
					getMetricSumaryDetail(vm.filterParamDetail);
				});
			}

			function getFiscalYear(curDay) {
				var def = cmsBase.$q.defer();
				var cur = chartSvc.FYFormatDate(curDay);
				fiscalyearservice.GetCustomFiscalYear(cur).then(
					function (data) {
						if (data) {
							vm.fiscalyear = data;
							vm.FiscalToWeek = chartSvc.GetFiscalWeek(vm.fiscalyear, $rootScope.BamFilter.dateReport);
							vm.FiscalToPeriod = chartSvc.GetFiscalPeriod(vm.FiscalToWeek.WeekNo, vm.fiscalyear.CalendarStyle, chartSvc.GetUTCDate(vm.fiscalyear.FYDateStart), chartSvc.GetUTCDate(vm.fiscalyear.FYDateEnd), false, vm.fiscalyear.FYNoOfWeeks);
							vm.filterParam = {
								SetDate: utils.toUTCDate(curDay),
								WeekStartDate: utils.toUTCDate(vm.FiscalToWeek.StartDate),
								WeekEndDate: utils.toUTCDate(vm.FiscalToWeek.EndDate),
								StartDate: utils.toUTCDate(vm.FiscalToPeriod.StartDate),
								EndDate: utils.toUTCDate(vm.FiscalToPeriod.EndDate),
								SitesKey: vm.selectedSites,
								ReportId: 1,
								ReportType: 0
							};
							vm.filterParamDetail = {
								ReportType: 0,
								ReportId: 1,
								sDate: vm.FiscalToWeek.StartDate.toDateParam(),
								eDate: vm.FiscalToWeek.EndDate.toDateParam(),
								sitesKey: vm.selectedSites.length == 0 ? '' : vm.selectedSites.toString(),
							};
							getDayofWeek(vm.FiscalToWeek.StartDate, vm.FiscalToWeek.EndDate);
							$rootScope.GWeek = vm.FiscalToWeek.WeekNo;
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

			function getMetricSumary(filterParam) {
				if (!filterParam || !filterParam.SitesKey || filterParam.SitesKey.length == 0) { return; }

				vm.headSummary = [];
				DashboardReportsSvc.getMetricSumary(filterParam,
					function (data) {
						vm.headSummary = data.DataTableSumary;
						sessionStorage.headSummary = JSON.stringify(vm.headSummary); //Save to client session storage - use to export.
						//console.log(data);
					},
				function (error) {
					//cmsBase.cmsLog.error('error');
				});
				$scope.ReportDate = $rootScope.BamFilter.dateReport;
			}

			function getMetricSumaryDetail(filterParam) {
				if (!filterParam || !filterParam.sitesKey) { return; }
				$scope.TableSumaryDetail = [];
				DashboardReportsSvc.GetMetricDetail(filterParam,
					function (data) {
						$scope.TableSumaryDetail = data.DataTableSumaryDetail;
						sessionStorage.TableSumaryDetail = JSON.stringify($scope.TableSumaryDetail); //Save to client session storage - use to export.
						angular.forEach($scope.TableSumaryDetail[0].Details, function (value, key) {
							value.Date = chartSvc.GetUTCDate(value.Date);
						});

						var DshChartDatas = data.DashboardCharts;//.DataGraphSumamryDetail;
						//var trafficChartData = data.DashboardCharts.TrafficChart;
						var goal = {
							Min: 0,
							Max: 0
						};

						if (data.GoalMetricConversion) {
							goal = {
								Min: data.GoalMetricConversion.MinGoal,
								Max: data.GoalMetricConversion.MaxGoal
							};
						}

						var dataChartAlls = {
							goal: goal,
							DshChartDatas: DshChartDatas,
							fiscalToWeek: vm.FiscalToWeek
						};
						$scope.$broadcast(AppDefine.Events.CHARTDATALOADED, dataChartAlls);
					},
					function (error) {
						cmsBase.cmsLog.error('error');
					});
			}

			vm.refreshMetricHeader = function () {
				getFiscalYear($rootScope.BamFilter.dateReport).then(function () {
					getMetricSumary(vm.filterParam);
				});
			}

			vm.refreshMetricDetailHeader = function () {
				getFiscalYear($rootScope.BamFilter.dateReport).then(function () {
					getMetricSumaryDetail(vm.filterParamDetail);
				});
			}

			function getDayofWeek(sdate, edate) {
				var d = angular.copy(sdate);
				$scope.dayofweek = [];
				for (d; d <= edate; d.setDate(d.getDate() + 1)) {
					$scope.dayofweek.push(new Date(d));
				}
				//console.log($scope.dayofweek);
			};

			vm.getComprateGoal = function (sum, value) {


				if (value > sum.MaxGoal) {
					return 'bam-value-above';
				}

				if (value <= sum.MaxGoal && value >= sum.MinGoal) {
					return 'bam-value-middle';
				}

				if (value < sum.MinGoal) {
					return 'bam-value-low';
				}
			}

			vm.showMetricPopup = function () {
				if (vm.selectedSites.length == 0) {
					cmsBase.cmsLog.warning(cmsBase.translateSvc.getTranslate(AppDefine.Resx.SELECT_LEAST_ONE_SITE_MSG));
					return;
				}

				if (!vm.modalShown) {
					vm.modalShown = true;
					var showMetricModal = $modal.open({
						templateUrl: 'widgets/bam/modal/metricview.html',
						controller: 'metricviewtCtrl as vm',
						size: 'sm',
						backdrop: 'static',
						backdropClass: 'modal-backdrop',
						keyboard: false,
						resolve: {
							items: function () {
								return {
									reportId: 1
								}
							}
						}
					});

					showMetricModal.result.then(function (data) {
						vm.modalShown = false;
						if (data === true) {
							vm.refreshMetricHeader();
							vm.refreshMetricDetailHeader();
						}

					});
				}
			}

			$scope.OpenNormalize = function () {

				$rootScope.ParamNormalize.sDate = $rootScope.BamFilter.dateReport;
				$rootScope.ParamNormalize.eDate = $rootScope.BamFilter.dateReport;
				if ($state.current.name == AppDefine.State.BAM_DASHBOARD) {
					$state.go(AppDefine.State.BAM_NORMALIZE);
				}
			}

			function getSelectedSites() {
				if ($scope.$parent && $scope.$parent.$parent && $scope.$parent.$parent.treeSiteFilter) {
					var node = $scope.$parent.$parent.treeSiteFilter;
					vm.selectedSites = [];
					bamhelperSvc.getSitesFromNode(node, vm.selectedSites);
				}
			}

			/* Export Functions, begin */
			function buildConvChart(dtSource, options) {
				var charConv = Enumerable.From(dtSource).OrderByDescending(function (o) { return o.AvgConv; }).ToArray();
				var chartData = {
					ChartDataItems: [],
					Title: cmsBase.translateSvc.getTranslate(AppDefine.Resx.CONVERSIONBYREGION),
					ChartType: AppDefine.chartExportType.ColumnChart,
					Name: "ConversionChart",
					Format: {
						Width: options.Width,
						Height: options.Height,
						ColIndex: options.ColIndex,
						RowIndex: options.RowIndex
					}
				};
				var ChartDataItem = {};
				var goal = JSON.parse(sessionStorage.GoalDataChart);
				angular.forEach(charConv, function (item) {
					ChartDataItem = {
						Name: item.Name,
						Value: item.AvgConv.toFixed(2),
						Color: exportSvc.getColorChart(item.AvgConv, goal),
					};
					chartData.ChartDataItems.push(ChartDataItem);
				});

				return chartData;
			}

			function buildTrafficChart(dtSource, options) {
				var chartData = {
					ChartDataItems: [],
					Title: cmsBase.translateSvc.getTranslate(AppDefine.Resx.TRAFFICSTATISTIC),
					ChartType: AppDefine.chartExportType.LineChart,
					Name: "TrafficChart",
					Format: {
						Width: options.Width,
						Height: options.Height,
						ColIndex: options.ColIndex,
						RowIndex: options.RowIndex
					}
				};

				var ChartDataItem = {};
				if ($scope.legendTraffic.CountIn) {
					//CountIn Line
					ChartDataItem = {
						Name: cmsBase.translateSvc.getTranslate(AppDefine.Resx.COUNT_IN),
						Value: LineChartValue(dtSource, AppDefine.BamDataTypes.TRAFFIC_IN),
						Color: AppDefine.ChartExportColor.Blue,
					};
					chartData.ChartDataItems.push(ChartDataItem);
				}
				
				if ($scope.legendTraffic.CountOut) {
					//CountOut Line
					ChartDataItem = {
						Name: cmsBase.translateSvc.getTranslate(AppDefine.Resx.COUNT_OUT),
						Value: LineChartValue(dtSource, AppDefine.BamDataTypes.TRAFFIC_OUT),
						Color: AppDefine.ChartExportColor.Yellow,
					};
					chartData.ChartDataItems.push(ChartDataItem);
				}
				
				if ($scope.legendTraffic.Forecast) {
					//Forecast Line
					ChartDataItem = {
						Name: cmsBase.translateSvc.getTranslate(AppDefine.Resx.FORECAST),
						Value: LineChartValue(dtSource, AppDefine.BamDataTypes.FORECAST),
						Color: AppDefine.ChartExportColor.Green,
					};
					chartData.ChartDataItems.push(ChartDataItem);
				}

				return chartData;
			}

			function LineChartValue(data, dataType) {
				var ret = "";
				if (!data || !dataType) { return ret; }
				angular.forEach(data, function (item) {
					switch (dataType) {
						case AppDefine.BamDataTypes.TRAFFIC_IN:
							ret += item.Label + "," + item.countIn + "|";
							break;
						case AppDefine.BamDataTypes.TRAFFIC_OUT:
							ret += item.Label + "," + item.countOut + "|";
							break;
						case AppDefine.BamDataTypes.FORECAST:
							ret += item.Label + "," + item.forecast + "|";
							break;
						default:
							break;
					}
				});
				ret = ret.substr(0, ret.length - 1);
				return ret;
			}

			function buildBamSumTable(dtSource, options) {
				var GridData = {
					Name: "DashboardMetric",
					RowDatas: [],
					Format: { ColIndex: options.ColIndex, RowIndex: options.RowIndex }
				};

				GridData.RowDatas.push(RowHeaderSummary(options));
				RowContentSummary(dtSource, GridData);

				return GridData;
			}

			function buildBamSumDetailTable(dtSource, options) {
				var GridData = {
					Name: "DashboardMetricDetail",
					RowDatas: [],
					Format: {
						ColIndex: options.ColIndex,
						RowIndex: options.RowIndex
					}
				};

				GridData.RowDatas.push(RowHeaderSummaryDetail(dtSource, options));
				RowContentSummaryDetail(dtSource, GridData);

				return GridData;
			}

			function RowHeaderSummary(options) {
				var colIndex = 1;
				var RowData = {
					Type: AppDefine.tableExport.Header,
					ColDatas: []
				};
				var ColData = {
					Value: cmsBase.translateSvc.getTranslate(AppDefine.Resx.METRIC),
					Color: AppDefine.ExportColors.GridHeaderFirstCell,
					CustomerWidth: false,
					Width: 0,
					MergeCells: { Cells: 4, Rows: 1 },
					ColIndex: colIndex,
					RowIndex: options.RowIndex
				};

				RowData.ColDatas.push(ColData);
				colIndex = ColData.MergeCells.Cells + colIndex;

				ColData = {
					Value: cmsBase.translateSvc.getTranslate(AppDefine.Resx.FORECAST_FOR) + " " + $filter('date')(new Date($rootScope.BamFilter.dateReport), AppDefine.ParamDateFormat),
					Color: AppDefine.ExportColors.GridHeaderCell,
					CustomerWidth: false,
					Width: 0,
					MergeCells: { Cells: 3, Rows: 1 },
					ColIndex: colIndex,
					RowIndex: options.RowIndex
				};
				RowData.ColDatas.push(ColData);
				colIndex = ColData.MergeCells.Cells + colIndex;

				ColData = {
					Value: cmsBase.translateSvc.getTranslate(AppDefine.Resx.ACTUAL_FOR) + " " + $filter('date')(new Date($rootScope.BamFilter.dateReport), AppDefine.ParamDateFormat),
					Color: AppDefine.ExportColors.GridHeaderCell,
					CustomerWidth: false,
					Width: 0,
					MergeCells: { Cells: 3, Rows: 1 },
					ColIndex: colIndex,
					RowIndex: options.RowIndex
				};
				RowData.ColDatas.push(ColData);
				colIndex = ColData.MergeCells.Cells + colIndex;

				ColData = {
					Value: cmsBase.translateSvc.getTranslate(AppDefine.Resx.WEEK_TO_DATE),
					Color: AppDefine.ExportColors.GridHeaderCell,
					CustomerWidth: false,
					Width: 0,
					MergeCells: { Cells: 3, Rows: 1 },
					ColIndex: colIndex,
					RowIndex: options.RowIndex
				};
				RowData.ColDatas.push(ColData);
				colIndex = ColData.MergeCells.Cells + colIndex;

				ColData = {
					Value: cmsBase.translateSvc.getTranslate(AppDefine.Resx.PERIOD_TO_DATE),
					Color: AppDefine.ExportColors.GridHeaderCell,
					CustomerWidth: false,
					Width: 0,
					MergeCells: { Cells: 3, Rows: 1 },
					ColIndex: colIndex,
					RowIndex: options.RowIndex
				};
				RowData.ColDatas.push(ColData);
				colIndex = ColData.MergeCells.Cells + colIndex;

				ColData = {
					Value: cmsBase.translateSvc.getTranslate(AppDefine.Resx.STORE_GOAL),
					Color: AppDefine.ExportColors.GridHeaderEndCell,
					CustomerWidth: false,
					Width: 0,
					MergeCells: { Cells: 3, Rows: 1 },
					ColIndex: colIndex,
					RowIndex: options.RowIndex
				};
				RowData.ColDatas.push(ColData);
				colIndex = ColData.MergeCells.Cells + colIndex;

				return RowData;
			}

			function RowContentSummary(dtSource, gridData) {
				var RowData = {};
				var ColData = {};
				var startCol = 1;
				var startRow = gridData.Format.RowIndex + 1;
				angular.forEach(dtSource, function (item, index) {
					var colIndex = startCol;
					var rowIndex = startRow + index;
					RowData = {
						Type: AppDefine.tableExport.Body,
						ColDatas: []
					};
					ColData = {
						Value: !item.ResourceKey ? item.Name : cmsBase.translateSvc.getTranslate(item.ResourceKey),
						Color: AppDefine.ExportColors.GridSumCell,
						CustomerWidth: false,
						Width: 0,
						MergeCells: { Cells: 4, Rows: 1 },
						ColIndex: colIndex,
						RowIndex: rowIndex

					};
					RowData.ColDatas.push(ColData);
					colIndex = ColData.MergeCells.Cells + colIndex;

					ColData = {
						Value: exportSvc.formatData(item.Forcecast, item.UnitName, item.UnitRound, item.UnitType),
						Color: AppDefine.ExportColors.ForecastCell,
						CustomerWidth: false,
						Width: 0,
						MergeCells: { Cells: 3, Rows: 1 },
						ColIndex: colIndex,
						RowIndex: rowIndex
					};
					RowData.ColDatas.push(ColData);
					colIndex = ColData.MergeCells.Cells + colIndex;

					ColData = {
						Value: exportSvc.formatData(item.Actualy, item.UnitName, item.UnitRound, item.UnitType),
						Color: exportSvc.getTableColorCompareGoal(item.Actualy, item),
						CustomerWidth: false,
						Width: 0,
						MergeCells: { Cells: 3, Rows: 1 },
						ColIndex: colIndex,
						RowIndex: rowIndex
					};
					RowData.ColDatas.push(ColData);
					colIndex = ColData.MergeCells.Cells + colIndex;

					ColData = {
						Value: exportSvc.formatData(item.TotalWeekToDate, item.UnitName, item.UnitRound, item.UnitType),
						Color: exportSvc.getTableColorCompareGoal(item.TotalWeekToDate, item),
						CustomerWidth: false,
						Width: 0,
						MergeCells: { Cells: 3, Rows: 1 },
						ColIndex: colIndex,
						RowIndex: rowIndex
					};
					RowData.ColDatas.push(ColData);
					colIndex = ColData.MergeCells.Cells + colIndex;

					ColData = {
						Value: exportSvc.formatData(item.TotalPeridToDate, item.UnitName, item.UnitRound, item.UnitType),
						Color: exportSvc.getTableColorCompareGoal(item.TotalPeridToDate, item),
						CustomerWidth: false,
						Width: 0,
						MergeCells: { Cells: 3, Rows: 1 },
						ColIndex: colIndex,
						RowIndex: rowIndex
					};
					RowData.ColDatas.push(ColData);
					colIndex = ColData.MergeCells.Cells + colIndex;

					ColData = {
						Value: exportSvc.formatData(item.Goal, item.UnitName, item.UnitRound, item.UnitType),
						Color: AppDefine.ExportColors.GridSumCell,
						CustomerWidth: false,
						Width: 0,
						MergeCells: { Cells: 3, Rows: 1 },
						ColIndex: colIndex,
						RowIndex: rowIndex
					};
					RowData.ColDatas.push(ColData);

					gridData.RowDatas.push(RowData);
				});
			}

			function RowHeaderSummaryDetail(data, options) {
				var colIndex = 1;
				var RowData = {
					Type: AppDefine.tableExport.Header,
					ColDatas: []
				};
				//Add first column
				var ColData = {
					Value: cmsBase.translateSvc.getTranslate(AppDefine.Resx.METRIC),
					Color: AppDefine.ExportColors.GridHeaderFirstCell,
					CustomerWidth: false,
					Width: 0,
					MergeCells: { Cells: 3, Rows: 1 },
					ColIndex: colIndex,
					RowIndex: options.RowIndex
				};
				RowData.ColDatas.push(ColData);
				colIndex = ColData.MergeCells.Cells + colIndex;

				if (!data || !data[0] || !data[0].Details) { return; }

				angular.forEach(data[0].Details, function (item) {
					ColData = {
						Value: $filter('date')(new Date(item.Date), AppDefine.ShortDateMD),
						Color: AppDefine.ExportColors.GridHeaderCell,
						CustomerWidth: false,
						Width: 0,
						MergeCells: { Cells: 2, Rows: 1 },
						ColIndex: colIndex,
						RowIndex: options.RowIndex
					};
					RowData.ColDatas.push(ColData);
					colIndex = ColData.MergeCells.Cells + colIndex;
				});

				//Add End column
				ColData = {
					Value: cmsBase.translateSvc.getTranslate(AppDefine.Resx.WEEK_STRING),
					Color: AppDefine.ExportColors.GridHeaderEndCell,
					CustomerWidth: false,
					Width: 0,
					MergeCells: { Cells: 2, Rows: 1 },
					ColIndex: colIndex,
					RowIndex: options.RowIndex
				};
				RowData.ColDatas.push(ColData);

				return RowData;
			}

			function RowContentSummaryDetail(data, gridData) {
				var RowData = {};
				var ColData = {};
				var startCol = 1;
				var startRow = gridData.Format.RowIndex + 1;
				angular.forEach(data, function (item, index) {
					var colIndex = startCol;
					var rowIndex = startRow + index;
					RowData = {
						Type: AppDefine.tableExport.Body,
						ColDatas: []
					};
					ColData = {
						Value: !item.ResourceKey ? item.Name : cmsBase.translateSvc.getTranslate(item.ResourceKey),
						Color: AppDefine.ExportColors.GridSumCell,
						CustomerWidth: false,
						Width: 0,
						MergeCells: { Cells: 3, Rows: 1 },
						ColIndex: colIndex,
						RowIndex: rowIndex
					};
					RowData.ColDatas.push(ColData);
					colIndex = ColData.MergeCells.Cells + colIndex;

					angular.forEach(item.Details, function (itemsub) {
						ColData = {
							Value: exportSvc.formatData(itemsub.Value, item.UnitName, item.UnitRound, item.UnitType),
							Color: exportSvc.getTableBGColorCompareGoal(itemsub.Value, item),
							CustomerWidth: false,
							Width: 0,
							MergeCells: { Cells: 2, Rows: 1 },
							ColIndex: colIndex,
							RowIndex: rowIndex
						};
						RowData.ColDatas.push(ColData);
						colIndex = ColData.MergeCells.Cells + colIndex;
					});

					ColData = {
						Value: exportSvc.formatData(item.TotalWeek, item.UnitName, item.UnitRound, item.UnitType),
						Color: AppDefine.ExportColors.GridSumCell,
						CustomerWidth: false,
						Width: 0,
						MergeCells: { Cells: 2, Rows: 1 },
						ColIndex: colIndex,
						RowIndex: rowIndex
					};
					RowData.ColDatas.push(ColData);
					colIndex = ColData.MergeCells.Cells + colIndex;

					gridData.RowDatas.push(RowData);
				});
			}

			/* Export functions, end */

		}
	});
})();
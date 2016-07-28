(function () {
	'use strict';

	define(['cms'
        , 'widgets/bam/modal/metricview'
		, 'widgets/bam/tables/headerBam'
		, 'widgets/bam/charts/waagConversion'
		, 'widgets/bam/charts/waagATV'
		, 'widgets/bam/charts/waagConvForecast'
		, 'Scripts/Services/chartSvc'
		, 'DataServices/Bam/DashboardReportsSvc'
		, 'DataServices/Configuration/fiscalyearservice'
        , 'Scripts/Services/exportSvc'
	], function (cms) {
		cms.register.controller('weekataglanceCtrl', weekataglanceCtrl);

		weekataglanceCtrl.$inject = ['$rootScope', '$scope', '$modal', 'cmsBase', 'dataContext', 'AppDefine', '$timeout', 'siteadminService', 'AccountSvc', 'DashboardReportsSvc', '$filter', 'chartSvc', 'fiscalyearservice', 'bamhelperSvc', 'Utils', '$state', 'exportSvc'];

		function weekataglanceCtrl($rootScope, $scope, $modal, cmsBase, dataContext, AppDefine, $timeout, siteadminService, AccountSvc, DashboardReportsSvc, $filter, chartSvc, fiscalyearservice, bamhelperSvc, utils, $state, exportSvc) {
			var vm = this;
			$scope.BAMDateFormat = AppDefine.BAMDateFormat;
			vm.selectedSites = [];
			$scope.ChartData = null;
			$scope.isShowNormalize = false;
			$scope.$emit('scrollDown');
			//******************** FiscalYear - Begin ***************************//
			$scope.FiscalWeek = {};
			$scope.FiscalYear = {};
			$scope.FiscalPeriod = {};
			vm.showMetricPopup = showMetricPopup;
			var siteDatas = [];
			$scope.dayofweek = [];
			$scope.ReportDate = $rootScope.BamFilter.dateReport;

			function getFiscalYear(curDay) {
				var def = cmsBase.$q.defer();

				var cur = chartSvc.FYFormatDate(curDay);
				fiscalyearservice.GetCustomFiscalYear(cur).then(function (data) {
					if (data) {
						$scope.FiscalYear = data;
						$scope.FiscalWeek = chartSvc.GetFiscalWeek($scope.FiscalYear, curDay);//$rootScope.BamFilter.dateReport
						getDayofWeek($scope.FiscalWeek.StartDate, $scope.FiscalWeek.EndDate);

						$scope.FiscalPeriod = chartSvc.GetFiscalPeriod($scope.FiscalWeek.WeekNo, $scope.FiscalYear.CalendarStyle, chartSvc.GetUTCDate($scope.FiscalYear.FYDateStart), chartSvc.GetUTCDate($scope.FiscalYear.FYDateEnd), false, $scope.FiscalYear.FYNoOfWeeks);

						vm.filterParam = {
							SetDate: utils.toUTCDate(curDay),
							WeekStartDate: utils.toUTCDate($scope.FiscalWeek.StartDate),
							WeekEndDate: utils.toUTCDate($scope.FiscalWeek.EndDate),
							StartDate: utils.toUTCDate($scope.FiscalPeriod.StartDate),
							EndDate: utils.toUTCDate($scope.FiscalPeriod.EndDate),
							SitesKey: vm.selectedSites,
							ReportId: 2,
							ReportType: 1
						};

						vm.filterParamDetail = {
							sDate: $scope.FiscalWeek.StartDate.toDateParam(),
							eDate: $scope.FiscalWeek.EndDate.toDateParam(),
							sitesKey: vm.selectedSites.length == 0 ? '' : vm.selectedSites.toString(),
							ReportId: 2, ReportType: 1
						};

						def.resolve();
					} else {
						def.reject('No data!');
					}
				},
				function (error) {
					def.reject(error);
				});
				return def.promise;
			}
			//******************** FiscalYear - End ***************************//
			$scope.Init = function () {
				if ($scope.$parent && $scope.$parent.$parent && $scope.$parent.$parent.treeSiteFilter) {
					vm.selectedSites = [];
					var firstsite = bamhelperSvc.getFirstSites($scope.$parent.$parent.treeSiteFilter);
					if (firstsite) {
						vm.selectedSites.push(firstsite.ID);
						$scope.selectedNode = firstsite;

						$scope.isShowNormalize = true;
						$rootScope.ParamNormalize.siteKey = vm.selectedSites;
					}
					//if ($rootScope.FilterWeekAtAGlane != null && $rootScope.FilterWeekAtAGlane.searchDate != null) {
					//	loadData($rootScope.FilterWeekAtAGlane.searchDate);
					//}
					//else {
					//	loadData(new Date());
					//}
					ReloadData();
				}
			}
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

				var tables = [];
				var options = { ColIndex: 1, RowIndex: 1 };
				var sumtable = buildExportSummaryTable(vm.headSummary, options);
				tables.push(sumtable);
				options = { ColIndex: 1, RowIndex: options.RowIndex + sumtable.RowDatas.length + 2 };
				var detailTable = buildExportDetailTable($scope.TableSumaryDetail, options);
				tables.push(detailTable);

				var chart1 = buildChartATV($scope.ChartData);
				var chart2 = buildChartConversion($scope.ChartData);
				
				var charts = []; //[chart1, chart2];
				exportSvc.exportXlsFromServer(reportInfo, tables, charts);
			});

			function buildChartConversion(data) {

				var chartDataItem = {
					Name: cmsBase.translateSvc.getTranslate("Conversion"),
					Value: exportSvc.lineChartValue(data.DataChartSumamry, 'Label', 'Conversion'),
					Color: AppDefine.ChartExportColor.Green,
				};

				var chartDataItemArea = {
					Name: cmsBase.translateSvc.getTranslate("Forecast"),
					Value: exportSvc.lineChartValue(data.DataChartSumamry, 'Label', 'ConvForecast'),
					Color: AppDefine.ChartExportColor.Green,
				};

				var result = {
					Title: "Conversion",
					ChartType: 3,
					OptionDatas: null,
					ChartDataItems: [chartDataItem, chartDataItemArea]
				}
				return result;

			}

			function buildChartATV(data) {

				var chartDataItem = {
					Name: cmsBase.translateSvc.getTranslate("ATV"),
					Value: exportSvc.lineChartValue(data.DataChartSumamry, 'Label', 'Avt'),
					Color: AppDefine.ChartExportColor.Green,
				};
				var option = {
					"Value": data.AVTData.Increase === true ? data.AVTData.Value : data.AVTData.Value * -1,
					"CHART_ATV_THISWEEK": cmsBase.translateSvc.getTranslate('CHART_ATV_THISWEEK'),
					"CHART_ATV_LASTWEEK": cmsBase.translateSvc.getTranslate('CHART_ATV_LASTWEEK'),
					"CompareValue": data.AVTData.CmpValue
				}

				var result = {
					Title: "ATV",
					ChartType: 3,
					OptionDatas: option,
					ChartDataItems: [chartDataItem]
				}
				return result;

			}

			function buildExportSummaryTable(data, options) {
				var startCol = 1;
				var startRow = options.RowIndex;
				var rows = [];
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
					RowIndex: startRow
				};
				RowData.ColDatas.push(ColData);
				colIndex = ColData.MergeCells.Cells + colIndex;

				ColData = {
					Value: cmsBase.translateSvc.getTranslate(AppDefine.Resx.FORECAST_FOR) + ' ' + $filter('date')(new Date($scope.ReportDate), $scope.BAMDateFormat),
					Color: AppDefine.ExportColors.GridHeaderCell,
					CustomerWidth: false,
					Width: 0,
					MergeCells: { Cells: 3, Rows: 1 },
					ColIndex: colIndex,
					RowIndex: startRow
				};
				RowData.ColDatas.push(ColData);
				colIndex = ColData.MergeCells.Cells + colIndex;

				ColData = {
					Value: cmsBase.translateSvc.getTranslate(AppDefine.Resx.ACTUAL_FOR) + ' ' + $filter('date')(new Date($scope.ReportDate), $scope.BAMDateFormat),
					Color: AppDefine.ExportColors.GridHeaderCell,
					CustomerWidth: false,
					Width: 0,
					MergeCells: { Cells: 3, Rows: 1 },
					ColIndex: colIndex,
					RowIndex: startRow
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
					RowIndex: startRow
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
					RowIndex: startRow
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
					RowIndex: startRow
				};
				RowData.ColDatas.push(ColData);
				colIndex = ColData.MergeCells.Cells + colIndex;

				//Insert Row Header
				rows.push(RowData);

				//Insert Rows content
				startRow += 1;
				data.forEach(function (r, index) {
					var colIndex = startCol;
					var rowIndex = startRow + index;

					RowData = {
						Type: AppDefine.tableExport.Body,
						ColDatas: []
					};
					ColData = {
						Value: r.ResourceKey !== "" ? cmsBase.translateSvc.getTranslate(r.ResourceKey) : r.Name,
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
						Value: exportSvc.formatData(r.Forcecast, r.UnitName, r.UnitRound, r.UnitType),
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
						Value: exportSvc.formatData(r.Actualy, r.UnitName, r.UnitRound, r.UnitType),
						Color: exportSvc.getTableColorCompareGoal(r.Actualy, r),
						CustomerWidth: false,
						Width: 0,
						MergeCells: { Cells: 3, Rows: 1 },
						ColIndex: colIndex,
						RowIndex: rowIndex

					};
					RowData.ColDatas.push(ColData);
					colIndex = ColData.MergeCells.Cells + colIndex;

					ColData = {
						Value: exportSvc.formatData(r.TotalWeekToDate, r.UnitName, r.UnitRound, r.UnitType),
						Color: exportSvc.getTableColorCompareGoal(r.TotalWeekToDate, r),
						CustomerWidth: false,
						Width: 0,
						MergeCells: { Cells: 3, Rows: 1 },
						ColIndex: colIndex,
						RowIndex: rowIndex

					};
					RowData.ColDatas.push(ColData);
					colIndex = ColData.MergeCells.Cells + colIndex;

					ColData = {
						Value: exportSvc.formatData(r.TotalPeridToDate, r.UnitName, r.UnitRound, r.UnitType),
						Color: exportSvc.getTableColorCompareGoal(r.TotalPeridToDate, r),
						CustomerWidth: false,
						Width: 0,
						MergeCells: { Cells: 3, Rows: 1 },
						ColIndex: colIndex,
						RowIndex: rowIndex

					};
					RowData.ColDatas.push(ColData);
					colIndex = ColData.MergeCells.Cells + colIndex;

					ColData = {
						Value: exportSvc.formatData(r.Goal, r.UnitName, r.UnitRound, r.UnitType),
						Color: AppDefine.ExportColors.GridSumCell,
						CustomerWidth: false,
						Width: 0,
						MergeCells: { Cells: 3, Rows: 1 },
						ColIndex: colIndex,
						RowIndex: rowIndex

					};
					RowData.ColDatas.push(ColData);
					colIndex = ColData.MergeCells.Cells + colIndex;

					rows.push(RowData);
				});

				var table = {
					Name: 'MetricSummary',
					RowDatas: rows,
					Format: { ColIndex: options.ColIndex, RowIndex: options.RowIndex }
				}
				return table;
			}

			function buildExportDetailTable(data, options) {
				var table = {
					Name: 'MetricDetail',
					RowDatas: [],
					Format: { ColIndex: options.ColIndex, RowIndex: options.RowIndex }
				}

				var startCol = 1;
				var startRow = options.RowIndex;
				var rows = [];
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
					MergeCells: { Cells: 3, Rows: 1 },
					ColIndex: colIndex,
					RowIndex: startRow
				};
				RowData.ColDatas.push(ColData);
				colIndex = ColData.MergeCells.Cells + colIndex;

				if (data[0].Details) {
					data[0].Details.forEach(function (h) {
						ColData = {
							Value: $filter('date')(h.Date, "MM/dd"),
							Color: AppDefine.ExportColors.GridHeaderCell,
							CustomerWidth: false,
							Width: 0,
							MergeCells: { Cells: 2, Rows: 1 },
							ColIndex: colIndex,
							RowIndex: startRow
						};
						RowData.ColDatas.push(ColData);
						colIndex = ColData.MergeCells.Cells + colIndex;
					});
				}
				else {
					dayofweek.forEach(function (h) {
						ColData = {
							Value: $filter('date')(h, "MM/dd"),
							Color: AppDefine.ExportColors.GridHeaderCell,
							CustomerWidth: false,
							Width: 0,
							MergeCells: { Cells: 2, Rows: 1 },
							ColIndex: colIndex,
							RowIndex: startRow
						};
						RowData.ColDatas.push(ColData);
						colIndex = ColData.MergeCells.Cells + colIndex;
					});
				}

				ColData = {
					Value: cmsBase.translateSvc.getTranslate('WEEK'),
					Color: AppDefine.ExportColors.GridHeaderEndCell,
					CustomerWidth: false,
					Width: 0,
					MergeCells: { Cells: 2, Rows: 1 },
					ColIndex: colIndex,
					RowIndex: startRow
				};
				RowData.ColDatas.push(ColData);
				
				//Insert Row Header
				table.RowDatas.push(RowData);

				//Insert Rows Content
				startRow += 1;
				data.forEach(function (r, index) {
					var colIndex = startCol;
					var rowIndex = startRow + index;

					RowData = {
						Type: AppDefine.tableExport.Body,
						ColDatas: []
					};

					ColData = {
						Value: r.ResourceKey !== "" ? cmsBase.translateSvc.getTranslate(r.ResourceKey) : r.Name,
						Color: AppDefine.ExportColors.GridSumCell,
						CustomerWidth: false,
						Width: 0,
						MergeCells: { Cells: 3, Rows: 1 },
						ColIndex: colIndex,
						RowIndex: rowIndex

					};
					RowData.ColDatas.push(ColData);
					colIndex = ColData.MergeCells.Cells + colIndex;

					r.Details.forEach(function (d, index) {
						ColData = {
							Value: exportSvc.formatData(d.Value, r.UnitName, r.UnitRound, r.UnitType),
							Color: exportSvc.getTableBGColorCompareGoal(d.Value, r),
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
						Value: exportSvc.formatData(r.TotalWeek, r.UnitName, r.UnitRound, r.UnitType),
						Color: AppDefine.ExportColors.GridSumCell,
						CustomerWidth: false,
						Width: 0,
						MergeCells: { Cells: 2, Rows: 1 },
						ColIndex: colIndex,
						RowIndex: rowIndex

					};
					RowData.ColDatas.push(ColData);
					
					table.RowDatas.push(RowData);
				});
				
				return table;
			}

			$scope.$on(AppDefine.Events.BAMSELECTNODE, function (e, node) {
				if (node.Type === AppDefine.NodeType.Site) {
					$scope.selectedNode = node;
					vm.selectedSites = [];
					vm.selectedSites.push(node.ID);
				}
				else {
					vm.selectedSites = [];
					var curNode = (node == null || node == undefined) ? $scope.$parent.$parent.treeSiteFilter : node;
					var firstsite = bamhelperSvc.getFirstSites(curNode);
					if (firstsite) {
						vm.selectedSites.push(firstsite.ID);
						$scope.selectedNode = firstsite;
					}
					//ReloadData();
				}
				$scope.isShowNormalize = true;
				$rootScope.ParamNormalize.siteKey = vm.selectedSites;
				$scope.$parent.$parent.EventRefresh();
			});

			$scope.$on(AppDefine.Events.CHECKNODE, function (e, flag) {
				$scope.isShowNormalize = true;
				$scope.$applyAsync();
			});

			$scope.$on(AppDefine.Events.GETBAMREPORTDATA, function (e, arg) {
				if ($rootScope.FilterWeekAtAGlane == null || $rootScope.FilterWeekAtAGlane == undefined) { return; }

				if ($scope.selectedNode) {
					if ($scope.selectedNode.Type === AppDefine.NodeType.Site) {
						vm.selectedSites = [];
						vm.selectedSites.push($scope.selectedNode.ID);

						ReloadData();
					}
				}
			});

			function ReloadData() {
				if ($rootScope.FilterWeekAtAGlane != null && $rootScope.FilterWeekAtAGlane.searchDate != null) {
					loadData($rootScope.FilterWeekAtAGlane.searchDate);
				}
				else {
					loadData(new Date());
				}
			}

			function loadData(date) {
				var dtNow = new Date(date);
				getFiscalYear(dtNow).then(function () {
					getMetricSumary(vm.filterParam);
					getMetricSumaryDetail(vm.filterParamDetail);
				});
			}

			function getMetricSumary(filterParam) {
				if (!filterParam || !filterParam.SitesKey) { return; }
				$scope.ReportDate = $rootScope.BamFilter.dateReport;
				vm.headSummary = [];
				DashboardReportsSvc.getMetricSumary(filterParam, function (data) {
					vm.headSummary = data.DataTableSumary;
					//$scope.ChartData = data.DataChartAll;
					//$scope.$broadcast(AppDefine.Events.CHARTDATALOADED, $scope.ChartData);
					//console.log(data);
				},
				function (error) {
					//$scope.$broadcast(AppDefine.Events.CHARTDATALOADED, null);
					cmsBase.cmsLog.error('error');
				});
			}

			function showMetricPopup() {
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
									reportId: 2
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

			vm.refreshMetricHeader = function () {
				getFiscalYear($rootScope.FilterWeekAtAGlane.searchDate).then(function () {
					getMetricSumary(vm.filterParam);
				});

			}

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

			function getMetricSumaryDetail(filterParam) {
				if (!filterParam || !filterParam.sitesKey) { return; }

				$scope.TableSumaryDetail = [];
				DashboardReportsSvc.GetMetricDetail(filterParam,
					function (data) {
						$scope.TableSumaryDetail = data.DataTableSumaryDetail;
						angular.forEach($scope.TableSumaryDetail[0].Details, function (value, key) {
							value.Date = chartSvc.GetUTCDate(value.Date);
						});

						$scope.ChartData = data.WAAGCharts;
						$scope.$broadcast(AppDefine.Events.CHARTDATALOADED, $scope.ChartData);
					},
					function (error) {
						$scope.$broadcast(AppDefine.Events.CHARTDATALOADED, null);
						cmsBase.cmsLog.error('error');
					});
			}

			vm.refreshMetricDetailHeader = function () {
				getFiscalYear($rootScope.FilterWeekAtAGlane.searchDate).then(function () {
					getMetricSumaryDetail(vm.filterParamDetail);
				});
			}

			$scope.OpenNormalize = function () {

				$rootScope.ParamNormalize.sDate = $rootScope.BamFilter.dateReport;
				$rootScope.ParamNormalize.eDate = $rootScope.BamFilter.dateReport;
				if ($state.current.name == AppDefine.State.WEEKATAGLANCE) {
					$state.go(AppDefine.State.BAM_NORMALIZE);
				}
			}
			function getDayofWeek(sdate, edate) {
				var d = angular.copy(sdate);
				$scope.dayofweek = [];
				for (d; d <= edate; d.setDate(d.getDate() + 1)) {
					$scope.dayofweek.push(new Date(d));
				}
				//console.log($scope.dayofweek);
			};

		}
	});
})();
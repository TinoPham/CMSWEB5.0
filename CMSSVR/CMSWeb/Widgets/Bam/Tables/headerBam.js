(function () {
	'use strict';

	define(['cms',
		'Scripts/DataServices/Export/ExportAPIService',
	], function (cms) {
		cms.register.controller('headerBamCtrl', headerBamCtrl);

		headerBamCtrl.$inject = ['$rootScope', '$scope', '$state', '$filter', 'cmsBase', 'AppDefine', 'ExportAPISvc', 'AccountSvc'];

		function headerBamCtrl($rootScope, $scope, $state, $filter, cmsBase, AppDefine, ExportAPISvc, AccountSvc) {
			var vm = this;
			$scope.Menus = $scope.$parent.$parent.$parent.$parent.vm.Menus;
			$scope.stateCurrent = $state.current.name;
			$scope.stateList = AppDefine.State;
			$rootScope.BamHeader = {
				TableChecked: true,
				ChartChecked: true
			};
			vm.subViews = [
				{ Key: 1, Name: 'Distribution' }
				, { Key: 2, Name: 'Heat Map' }
				, { Key: 3, Name: 'Direction' }
			];
			vm.subViewSelected = vm.subViews[0]; // Default show Distribution view
			$scope.ExportPage = {
				BAMDashboard: 1,
				WeekAtAGlance: 2,
				SaleReport: 3,
				Distribution: 4,
				DriveThrough: 5
			};
			$scope.ExportFormat = {
				Excel: 1,
				PDF: 2,
				CSV: 3
			};
			$scope.userLogin = AccountSvc.UserModel();

			active();

			function active() {
				//ThangPham, Load Resource refference, Feb 24 2016
				cmsBase.translateSvc.partLoad('Bam'); //Bam: is folder name

				var listChildMenus = [];
				Enumerable.From($scope.Menus)
					.Select(function (s) { return listChildMenus = listChildMenus.concat(s.Childs); }).ToArray();
				$scope.selectedMenu = Enumerable.From(listChildMenus)
									.Where(function (c) { return c.State == $state.current.name; }).FirstOrDefault();
			}

			$scope.ChangeView = function (view) {
				if (!view) {
					vm.subViewSelected = vm.subViews[0];
				}
				else {
					vm.subViewSelected = view;
				}
				$scope.$emit("changeDistributionView", vm.subViewSelected);
			};

			$scope.ExportTo = function (format) {
				var dataExport = {};
				switch ($scope.stateCurrent) {
					case AppDefine.State.BAM_DASHBOARD:
						BuildDataBAMDBExport(format);
						break;
					case AppDefine.State.WEEKATAGLANCE:
						dataExport = { exportPage: $scope.ExportPage.WeekAtAGlance, exportType: format };
						break;
					case AppDefine.State.SALEREPORTS:
						dataExport = { exportPage: $scope.ExportPage.SaleReport, exportType: format };
						break;
					case AppDefine.State.DISTRIBUTION:
						dataExport = { exportPage: $scope.ExportPage.Distribution, exportType: format };
						break;
					case AppDefine.State.DRIVETHROUGH:
						dataExport = { exportPage: $scope.ExportPage.DriveThrough, exportType: format };
						break;
					default:
						break;
				}
			}

			function BuildDataBAMDBExport(exportType) {
				var ReportInfoModel = ExportAPISvc.ReportInfoModel();
				ReportInfoModel = {
					ReportName: cmsBase.translateSvc.getTranslate($rootScope.title),
					CompanyID: $scope.userLogin.CompanyID,
					RegionName: '',
					Location: '',
					WeekIndex: $rootScope.GWeek,
					Footer: '',
					CreatedBy: $scope.userLogin.FName + ' ' + $scope.userLogin.LName,
					CreateDate: $filter('date')(new Date(), AppDefine.ParamDateFormat) // MM/dd/yyyy
				};

				var chartDatas = [];
				//Conversion Chart - by Region
				var charConv = Enumerable.From(JSON.parse(sessionStorage.ConversionRegionsChart)).OrderByDescending(function (o) { return o.AvgConv; }).ToArray();
				var chartData = ExportAPISvc.ChartData();
				chartData = {
					ChartDataItems: [],
					Title: cmsBase.translateSvc.getTranslate(AppDefine.Resx.CONVERSIONBYREGION),
					ChartType: AppDefine.chartExportType.ColumnChart
				};
				angular.forEach(charConv, function (item) {
					var ChartDataItem = ExportAPISvc.ChartDataItem();
					ChartDataItem = {
						Name: item.Name,
						Value: item.AvgConv.toFixed(2),
						Color: GetColorChart(item.AvgConv),
					};
					chartData.ChartDataItems.push(ChartDataItem);
				});
				chartDatas.push(chartData);

				//Traffic Statistic
				var chartTraffic = JSON.parse(sessionStorage.BamTraffic);
				chartData = {};
				chartData = ExportAPISvc.ChartData();
				chartData = {
					ChartDataItems: [],
					Title: cmsBase.translateSvc.getTranslate(AppDefine.Resx.TRAFFICSTATISTIC),
					ChartType: AppDefine.chartExportType.LineChart
				};
				//CountIn Line
				var ChartDataItem = ExportAPISvc.ChartDataItem();
				ChartDataItem = {
					Name: cmsBase.translateSvc.getTranslate(AppDefine.Resx.COUNT_IN),
					Value: LineChartValue(chartTraffic, AppDefine.BamDataTypes.TRAFFIC_IN),
					Color: AppDefine.ChartExportColor.Blue,
				};
				chartData.ChartDataItems.push(ChartDataItem);
				//CountOut Line
				ChartDataItem = ExportAPISvc.ChartDataItem();
				ChartDataItem = {
					Name: cmsBase.translateSvc.getTranslate(AppDefine.Resx.COUNT_OUT),
					Value: LineChartValue(chartTraffic, AppDefine.BamDataTypes.TRAFFIC_OUT),
					Color: AppDefine.ChartExportColor.Yellow,
				};
				chartData.ChartDataItems.push(ChartDataItem);
				//Forecast Line
				ChartDataItem = ExportAPISvc.ChartDataItem();
				ChartDataItem = {
					Name: cmsBase.translateSvc.getTranslate(AppDefine.Resx.FORECAST),
					Value: LineChartValue(chartTraffic, AppDefine.BamDataTypes.FORECAST),
					Color: AppDefine.ChartExportColor.Green,
				};
				chartData.ChartDataItems.push(ChartDataItem);

				chartDatas.push(chartData);

				//Grids data
				var GridDatas = [];
				//BAM Dashboard head summary
				var headSummary = JSON.parse(sessionStorage.headSummary);
				var GridData = ExportAPISvc.GridData();
				GridData = {
					Name: "DashboardMetric",
					RowDatas: []
				};
				var RowHeader = RowHeaderSummary();
				GridData.RowDatas.push(RowHeader);
				RowContentSummary(headSummary, GridData);

				GridDatas.push(GridData);

				//BAM Dashboard head summary detail
				var headSummaryDetail = JSON.parse(sessionStorage.TableSumaryDetail);
				GridData = ExportAPISvc.GridData();
				GridData = {
					Name: "DashboardMetricDetail",
					RowDatas: []
				};
				RowHeader = RowHeaderSummaryDetail(headSummaryDetail);
				GridData.RowDatas.push(RowHeader);
				RowContentSummaryDetail(headSummaryDetail, GridData);

				GridDatas.push(GridData);

				var BAMExportModel = {
					ReportInfo: ReportInfoModel,
					GridModels: GridDatas,
					ChartModels: chartDatas
				};

				console.log(BAMExportModel);

				if (exportType === $scope.ExportFormat.Excel) {
					//Call to API export EXCEL file
					ExportAPISvc.BAMDashboardToExcel(BAMExportModel).then(
						function (response) {
							location.href = ExportAPISvc.DownloadExport(response);
						}
					);
				}
				else if (exportType === $scope.ExportFormat.PDF) {
					//Call to API export PDF file
					ExportAPISvc.BAMDashboardToPDF(BAMExportModel).then(
						function (response) {
							location.href = ExportAPISvc.DownloadExport(response);
						}
					);
				}
			}

			function GetColorTableSumHeader(val, maxgoal, mingoal) {
				var color = AppDefine.ExportColors.Default;

				if (val < mingoal) {
					color = AppDefine.ExportColors.TextLessGoal;
				}
				else if (val >= mingoal && val <= maxgoal) {
					color = AppDefine.ExportColors.TextInGoal;
				}
				else if (val > maxgoal) {
					color = AppDefine.ExportColors.TextGreaterGoal;
				}
				return color;
			}

			function GetColorTableSumDetail(val, maxgoal, mingoal) {
				var color = AppDefine.ExportColors.Default;

				if (val < mingoal) {
					color = AppDefine.ExportColors.LessGoalCell;
				}
				else if (val >= mingoal && val <= maxgoal) {
					color = AppDefine.ExportColors.InGoalCell;
				}
				else if (val > maxgoal) {
					color = AppDefine.ExportColors.GreaterGoalCell;
				}
				return color;
			}

			function GetColorChart(val) {
				var goal = JSON.parse(sessionStorage.GoalDataChart);
				var color = AppDefine.ChartExportColor.Default;
				if (!goal) { return color; }

				if (val < goal.Min) {
					color = AppDefine.ChartExportColor.Red;
				}
				else if (val >= goal.Min && val <= goal.Max) {
					color = AppDefine.ChartExportColor.Orange;
				}
				else if (val > goal.Max) {
					color = AppDefine.ChartExportColor.Green;
				}
				return color;
			}

			function FormatData(val, unitName, unitRound, unitType) {
				switch (unitName) {
					case AppDefine.NumberFormat.Dollar:
						return unitName + $filter('salenumber')(val, GetFormatNumber(unitName, unitRound, unitType));
					case AppDefine.NumberFormat.Percent:
						return $filter('salenumber')(val, GetFormatNumber(unitName, unitRound, unitType)) + unitName;
					default:
						return $filter('salenumber')(val, GetFormatNumber(unitName, unitRound, unitType));
						break;
				}
			}

			function GetFormatNumber(unitName, unitRound, unitType) {
				var format = "0,0";
				if (unitType == 2) {
					format = "0";
				}

				if (unitRound) {
					format = format + ".";
					for (var i = 0; i < unitRound; i++) {
						format = format + "0";
					}
				}

				return format;
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

			function RowHeaderSummary() {
				var RowData = ExportAPISvc.RowData();
				RowData = {
					Type: AppDefine.tableExport.Header,
					ColDatas: []
				};

				var ColData = ExportAPISvc.ColData();
				ColData = {
					Value: cmsBase.translateSvc.getTranslate(AppDefine.Resx.METRIC),
					Color: AppDefine.ExportColors.GridHeaderFirstCell
				};
				RowData.ColDatas.push(ColData);

				ColData = ExportAPISvc.ColData();
				ColData = {
					Value: cmsBase.translateSvc.getTranslate(AppDefine.Resx.FORECAST_FOR) + $filter('date')(new Date($rootScope.BamFilter.dateReport), AppDefine.ParamDateFormat),
					Color: AppDefine.ExportColors.GridHeaderCell
				};
				RowData.ColDatas.push(ColData);

				ColData = ExportAPISvc.ColData();
				ColData = {
					Value: cmsBase.translateSvc.getTranslate(AppDefine.Resx.ACTUAL_FOR) + $filter('date')(new Date($rootScope.BamFilter.dateReport), AppDefine.ParamDateFormat),
					Color: AppDefine.ExportColors.GridHeaderCell
				};
				RowData.ColDatas.push(ColData);

				ColData = ExportAPISvc.ColData();
				ColData = {
					Value: cmsBase.translateSvc.getTranslate(AppDefine.Resx.WEEK_TO_DATE),
					Color: AppDefine.ExportColors.GridHeaderCell
				};
				RowData.ColDatas.push(ColData);

				ColData = ExportAPISvc.ColData();
				ColData = {
					Value: cmsBase.translateSvc.getTranslate(AppDefine.Resx.PERIOD_TO_DATE),
					Color: AppDefine.ExportColors.GridHeaderCell
				};
				RowData.ColDatas.push(ColData);

				ColData = ExportAPISvc.ColData();
				ColData = {
					Value: cmsBase.translateSvc.getTranslate(AppDefine.Resx.STORE_GOAL),
					Color: AppDefine.ExportColors.GridHeaderEndCell
				};
				RowData.ColDatas.push(ColData);

				return RowData;
			}

			function RowContentSummary(data, gridData) {
				angular.forEach(data, function (item) {
					var RowData = ExportAPISvc.RowData();
					RowData = {
						Type: AppDefine.tableExport.Body,
						ColDatas: []
					};

					var ColData = ExportAPISvc.ColData();
					ColData = {
						Value: !item.ResourceKey ? item.Name : cmsBase.translateSvc.getTranslate(item.ResourceKey),
						Color: AppDefine.ExportColors.GridSumCell
					};
					RowData.ColDatas.push(ColData);

					ColData = ExportAPISvc.ColData();
					ColData = {
						Value: FormatData(item.Forcecast, item.UnitName, item.UnitRound, item.UnitType),
						Color: AppDefine.ExportColors.ForecastCell
					};
					RowData.ColDatas.push(ColData);

					ColData = ExportAPISvc.ColData();
					ColData = {
						Value: FormatData(item.Actualy, item.UnitName, item.UnitRound, item.UnitType),
						Color: GetColorTableSumHeader(item.Actualy, item.MaxGoal, item.MinGoal)
					};
					RowData.ColDatas.push(ColData);

					ColData = ExportAPISvc.ColData();
					ColData = {
						Value: FormatData(item.TotalWeekToDate, item.UnitName, item.UnitRound, item.UnitType),
						Color: GetColorTableSumHeader(item.TotalWeekToDate, item.MaxGoal, item.MinGoal)
					};
					RowData.ColDatas.push(ColData);

					ColData = ExportAPISvc.ColData();
					ColData = {
						Value: FormatData(item.TotalPeridToDate, item.UnitName, item.UnitRound, item.UnitType),
						Color: GetColorTableSumHeader(item.TotalPeridToDate, item.MaxGoal, item.MinGoal)
					};
					RowData.ColDatas.push(ColData);

					ColData = ExportAPISvc.ColData();
					ColData = {
						Value: FormatData(item.Goal, item.UnitName, item.UnitRound, item.UnitType),
						Color: AppDefine.ExportColors.GridSumCell
					};
					RowData.ColDatas.push(ColData);

					gridData.RowDatas.push(RowData);
				});
			}

			function RowHeaderSummaryDetail(data) {
				var RowData = ExportAPISvc.RowData();
				RowData = {
					Type: AppDefine.tableExport.Header,
					ColDatas: []
				};

				//Add first column
				var ColData = ExportAPISvc.ColData();
				ColData = {
					Value: cmsBase.translateSvc.getTranslate(AppDefine.Resx.METRIC),
					Color: AppDefine.ExportColors.GridHeaderFirstCell
				};
				RowData.ColDatas.push(ColData);

				if (!data || !data[0] || !data[0].Details) { return; }
				angular.forEach(data[0].Details, function (item) {
					ColData = ExportAPISvc.ColData();
					ColData = {
						Value: $filter('date')(new Date(item.Date), AppDefine.ShortDateMD),
						Color: AppDefine.ExportColors.GridHeaderCell
					};
					RowData.ColDatas.push(ColData);
				});

				//Add End column
				ColData = ExportAPISvc.ColData();
				ColData = {
					Value: cmsBase.translateSvc.getTranslate(AppDefine.Resx.WEEK_STRING),
					Color: AppDefine.ExportColors.GridHeaderEndCell
				};
				RowData.ColDatas.push(ColData);

				return RowData;
			}

			function RowContentSummaryDetail(data, gridData) {
				angular.forEach(data, function (item) {
					var RowData = ExportAPISvc.RowData();
					RowData = {
						Type: AppDefine.tableExport.Body,
						ColDatas: []
					};

					var ColData = ExportAPISvc.ColData();
					ColData = {
						Value: !item.ResourceKey ? item.Name : cmsBase.translateSvc.getTranslate(item.ResourceKey),
						Color: AppDefine.ExportColors.GridSumCell
					};
					RowData.ColDatas.push(ColData);

					angular.forEach(item.Details, function (itemsub) {
						ColData = ExportAPISvc.ColData();
						ColData = {
							Value: FormatData(itemsub.Value, item.UnitName, item.UnitRound, item.UnitType),
							Color: GetColorTableSumDetail(itemsub.Value, item.MaxGoal, item.MinGoal)
						};
						RowData.ColDatas.push(ColData);
					});

					ColData = ExportAPISvc.ColData();
					ColData = {
						Value: FormatData(item.TotalWeek, item.UnitName, item.UnitRound, item.UnitType),
						Color: AppDefine.ExportColors.GridSumCell
					};
					RowData.ColDatas.push(ColData);

					gridData.RowDatas.push(RowData);
				});
			}


		}
	});
})();
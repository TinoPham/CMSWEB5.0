(function () {
	'use strict';
	define(['cms', 'widgets/bam/charts/jquery.circle-diagram', 'DataServices/ReportService', 'Scripts/Services/chartSvc', 'DataServices/Configuration/fiscalyearservice'],
		function (cms) {
			cms.register.controller('convRegionCtrl', convRegionCtrl);
			convRegionCtrl.$inject = ['$scope', 'ReportService', 'cmsBase', 'Utils', 'AppDefine', 'chartSvc', 'fiscalyearservice', '$filter', '$timeout', '$rootScope'];
			function convRegionCtrl($scope, rptService, cmsBase, utils, AppDefine, chartSvc, fiscalyearservice, $filter, $timeout, $rootScope) {
				var siteDatas = [];
				var goal = [];
				$scope.$on(AppDefine.Events.CHARTDATALOADED, function (e, data) {
					if (data != null) {
						siteDatas = data.DshChartDatas.DataGraphSumamryDetail;
						goal = data.goal;
						loadChartData($scope.$parent.$parent.treeSiteFilter);
						sessionStorage.GoalDataChart = JSON.stringify(goal); // Use export
					}
				});

				var convTooltip = cmsBase.translateSvc.getTranslate(AppDefine.Resx.CONVERSION);
				$scope.chartDataMessageCov = {
				    dataEmptyMessage: cmsBase.translateSvc.getTranslate(AppDefine.Resx.NO_CHART_DATA_MSG)
				};
				$scope.chartConfig = {
					//caption: AppDefine.Resx.CHART_ALERT,
					numberSuffix: "%",
					subCaption: "",
					captionAlignment: "center",
					//numDivLines: "6",
					theme: "fint",
					showBorder: "1",
					toolTipBgColor: "#FFFFFF",
					toolTipBgAlpha: '0',
					plottooltext: "<div class='chart-tooltip'><div class='tooltipHeader'><b>$label</b></div><div class='tooltipContent'>" + convTooltip + ": $value %</div></div>",
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
				};
				var regions = [];
				var curReg = {};
				var ConversionRegionsChart = [];
				var ConversionRegions = {};
				var rootID = "";
				var siteDatas = [];//[{ siteKey: 1, conv: 15 }, { siteKey: 2, conv: 20 }, { siteKey: 3, conv: 25 }, { siteKey: 4, conv: 35 }, { siteKey: 5, conv: 40 }];
				$scope.chartDSConv = {
					chart: $scope.chartConfig,
					data: []
				};
				
				function GetColor(val) {
					var color = '';
					if (val <= goal.Min) {
						color = AppDefine.ChartColor.LessMinGoal;
					}
					else if (val >= goal.Min && val <= goal.Max) {
						color = AppDefine.ChartColor.InGoal;
					}
					else if (val >= goal.Max) {
						color = AppDefine.ChartColor.GreaterMaxGoal;
					}
					else {
						color = AppDefine.ChartColor.White;
					}
					return color;
				}

				function CreateChartData(convData) {
					$rootScope.$broadcast('UPDATE_CONV_CHART', convData);
					if (convData === null || convData === undefined)
						return;
					var convArray = [];
					var DpoArray = [];
					//$scope.ChartDatas = convArray;
					for (var i = 0; i < convData.length; i++) {
						var item = {};
						item.color = GetColor(convData[i].AvgConv);
						item.label = convData[i].Name;
						item.value = convData[i].AvgConv.toFixed(2);
						if (item.value == 0) {
							item.showValue = 0;
						}
						convArray.push(item);
						convArray.sort(function (a, b) {
							return parseFloat(b.value) - parseFloat(a.value);
						});
						
					}
					$scope.chartDSConv.data = convArray;
				}
				function GetSiteData(val, sites) {
					var site = Enumerable.From(sites)
						.Where(function (i) { return (i.SiteKey == val) })
						.Select(function (x) { return x; })
						.FirstOrDefault();
					return site;
				}
				function GetRegions(regions, curReg, treeData) {
					if (treeData == null || treeData == undefined) {
						return;
					}
					if (regions == null || regions == undefined) {
						regions = new Array();
					}
					var hasChecked = false;
					angular.forEach(treeData, function (n) {
						if (n != null && n != undefined) {
							if (n.Type == 0 && n.ParentKey == rootID) {
								curReg = {};
								curReg.Name = n.Name;
								curReg.ID = n.ID;
								curReg.TotalConv = 0;
								curReg.TotalDpo = 0;
								curReg.NumSites = 0;
								curReg.HasData = 0;
								regions.push(curReg);
							}
							if (n.Sites && n.Sites.length > 0) {
								GetRegions(regions, curReg, n.Sites);
							}
							if (n.Type == 1) {
								if(n.Checked)
									curReg.NumSites++;
								var site = GetSiteData(n.ID, siteDatas);
								if (site != null) {
									curReg.TotalConv += site.Conversion;
									curReg.TotalDpo += site.Dpo;
									curReg.HasData++;
								}
								else {
									var iii = 'test';
								}

							} //if
						}
					});
				}
				function loadChartData(tree) {
					$scope.menuTreeData = tree;
					regions = [];
					curReg = {};
					rootID = $scope.menuTreeData.ID;
					GetRegions(regions, curReg, $scope.menuTreeData.Sites);
					ConversionRegionsChart = [];
					angular.forEach(regions, function (n) {
						if (n != null && n != undefined) {
							if (n.HasData != 0) {
								ConversionRegions = {};
								ConversionRegions.Name = n.Name;
								ConversionRegions.ID = n.ID;
								ConversionRegions.AvgConv = n.TotalConv / n.NumSites;
								ConversionRegions.AvgDpo = n.TotalDpo / n.NumSites;
								ConversionRegionsChart.push(ConversionRegions);
							}
						}
					});
					CreateChartData(ConversionRegionsChart);
					sessionStorage.ConversionRegionsChart = JSON.stringify(ConversionRegionsChart); //Use export
				}
			}
		});
})();
(function () {
	'use strict';
	define(['cms', 'Scripts/Services/chartSvc'],
		function (cms) {
			cms.register.controller('saleRptNewChartsCtrl', saleRptNewChartsCtrl);
			saleRptNewChartsCtrl.$inject = ['$scope', 'cmsBase', 'Utils', 'AppDefine', 'chartSvc', '$filter'];
			function saleRptNewChartsCtrl($scope, cmsBase, utils, AppDefine, chartSvc, $filter) {
				//var vm = this;
				var srpt = this;

				$scope.ChartDataParamAll = null;

				srpt.arrChartTypes = [];
				$scope.ChartLoaded = false;
				$scope.AllChartDataSet = [];
				$scope.AllChartDataSetSites = [];
				$scope.ShowChartSites = false;
				$scope.ChartToolbars = false;

				$scope.DSItemRegion = 0;
				$scope.DSItemSite = 0;

				$scope.DSItemLimit = 10;
				$scope.DSItemNumber = 0;
				$scope.DSItemIndex = 0;

				$scope.RegionLegends = [];
				$scope.SiteLegends = [];

				$scope.arrChartLegends = [];

				$scope.SupportSites = false;
				$scope.ShowChartTypes = [true, true];

				$scope.isLastPage = false;
				$scope.selectedTypes = {Key: AppDefine.BamDataTypes.TRAFFIC_IN};//value:AppDefine.BamDataTypes.TRAFFIC_IN};

				var arrChartRegionColors = ["#0075c2", "#1aaf5d", "#f2c500", "#e44a00", "#6baa01", "#FFD700", "#0000FF", "#eb605b", "#efad4d", "#17b374"];

				var STR_POS = cmsBase.translateSvc.getTranslate(AppDefine.Resx.POS);
				var STR_TRAFFIC = cmsBase.translateSvc.getTranslate(AppDefine.Resx.TRAFFIC);
				var STR_DWELL = cmsBase.translateSvc.getTranslate(AppDefine.Resx.DWELL);
				var STR_COUNT = cmsBase.translateSvc.getTranslate(AppDefine.Resx.COUNT);
				var STR_COUNT_IN = cmsBase.translateSvc.getTranslate(AppDefine.Resx.COUNT_IN);
				var STR_COUNT_OUT = cmsBase.translateSvc.getTranslate(AppDefine.Resx.COUNT_OUT);
				var STR_CONVERSION = cmsBase.translateSvc.getTranslate(AppDefine.Resx.CONVERSION);
				var STR_CONV = cmsBase.translateSvc.getTranslate(AppDefine.Resx.CONV);
				var JSChartTypes = {
					Stack: 'msstackedcolumn2dlinedy',
					Combine: 'mscombidy2d'
				};
				//***************************************** Chart Data - Begin ************************************************/
				$scope.JSChartType = JSChartTypes.Stack;

				$scope.chartDataMessage = {
				    dataEmptyMessage: cmsBase.translateSvc.getTranslate(AppDefine.Resx.NO_CHART_DATA_MSG)
				};
				$scope.chartDataDSMulti = {
					chart: {
						//xAxisname: "Month",
						pYAxisName: "",
						sYAxisName: "",
						numberPrefix: "",
						sNumberSuffix: "",

						//paletteColors: "#0075c2,#1aaf5d,#f2c500",
						baseFontColor: "#333333",
						baseFont: "Helvetica Neue,Arial",
						captionFontSize: "14",
						subcaptionFontSize: "14",
						subcaptionFontBold: "0",
						showBorder: "0",
						bgColor: "#ffffff",
						showShadow: "0",
						canvasBgColor: "#ffffff",
						canvasBorderAlpha: "0",
						divlineAlpha: "100",
						divlineColor: "#999999",
						divlineThickness: "1",
						divLineIsDashed: "1",
						divLineDashLen: "1",
						divLineGapLen: "1",
						usePlotGradientColor: "0",
						showplotborder: "0",
						showXAxisLine: "1",
						xAxisLineThickness: "1",
						xAxisLineColor: "#999999",
						showAlternateHGridColor: "0",
						showAlternateVGridColor: "0",
						legendBgAlpha: "0",
						legendBorderAlpha: "0",
						legendShadow: "0",
						legendItemFontSize: "10",
						showLegend: "1",

						borderAlpha: "4",
						plotSpacePercent: "60",
						toolTipBgColor: "#434955",
						toolTipBorderColor: "#434955",

						showSecondaryLimits: "0",
						showDivLineSecondaryValue: "0"
					},
					categories: [],
					dataset: []
				};
				function RenderChart(chtType) {
					var chart = new FusionCharts({
						type: chtType,
						renderAt: 'bamNewSaleCharts',
						width: '100%',
						height: '400',
						dataFormat: 'json',
						dataSource: $scope.chartDataDSMulti
					});

					chart.render();
				}
				function UpdatePYAxisName() {
					if ($scope.ReportID == AppDefine.BamReportTypes.SALE) {
						var pYName = "";//STR_TRAFFIC;
						if (srpt.arrChartTypes && srpt.arrChartTypes.length > 0) {
							var trafTypes = Enumerable.From(srpt.arrChartTypes)
									.Where(function (ct) { return ((ct.Key == AppDefine.BamDataTypes.TRAFFIC_IN || ct.Key == AppDefine.BamDataTypes.TRAFFIC_OUT) && ct.Checked == true) })
									.Select(function (x) { return x; }).FirstOrDefault();
							if (trafTypes && trafTypes.Checked == true) {
								pYName = STR_TRAFFIC;
								//hasTraf = true;
							}

							var posTypes = Enumerable.From(srpt.arrChartTypes)
									.Where(function (ct) { return (ct.Key == AppDefine.BamDataTypes.POS) })
									.Select(function (x) { return x; }).FirstOrDefault();
							if (posTypes && posTypes.Checked == true) {
								if (pYName.length > 0) {
									pYName = STR_TRAFFIC + ', ' + STR_POS;
								}
								else {
									pYName = STR_POS;
								}
							}
							if (pYName.length == 0) {
								$scope.chartDataDSMulti.chart.showLimits = "0";
								$scope.chartDataDSMulti.chart.showDivLineValues = "0";
							}
							else {
								$scope.chartDataDSMulti.chart.showLimits = "1";
								$scope.chartDataDSMulti.chart.showDivLineValues = "1";
							}

							var convTypes = Enumerable.From(srpt.arrChartTypes)
									.Where(function (ct) { return (ct.Key == AppDefine.BamDataTypes.CONVERSION) })
									.Select(function (x) { return x; }).FirstOrDefault();
							if (convTypes && convTypes.Checked == true) {
								$scope.chartDataDSMulti.chart.sYAxisName = STR_CONV;
								$scope.chartDataDSMulti.chart.sNumberSuffix = "%";
								$scope.chartDataDSMulti.chart.showSecondaryLimits = "1";
								$scope.chartDataDSMulti.chart.showDivLineSecondaryValue = "1";
							}
							else {
								$scope.chartDataDSMulti.chart.sYAxisName = "";
								$scope.chartDataDSMulti.chart.sNumberSuffix = "";
								$scope.chartDataDSMulti.chart.showSecondaryLimits = "0";
								$scope.chartDataDSMulti.chart.showDivLineSecondaryValue = "0";
							}

						}
						$scope.chartDataDSMulti.chart.pYAxisName = pYName;//STR_TRAFFIC + ', ' + STR_POS;
					}
					else {
						if ($scope.ShowChartTypes[0] == true) {
							$scope.chartDataDSMulti.chart.showSecondaryLimits = "1";
							$scope.chartDataDSMulti.chart.showDivLineSecondaryValue = "1";
							$scope.chartDataDSMulti.chart.sYAxisName = STR_COUNT;
						}
						else {
							$scope.chartDataDSMulti.chart.showSecondaryLimits = "0";
							$scope.chartDataDSMulti.chart.showDivLineSecondaryValue = "0";
							$scope.chartDataDSMulti.chart.sYAxisName = "";
						}
						if ($scope.ShowChartTypes[1] == true) {
							$scope.chartDataDSMulti.chart.showLimits = "1";
							$scope.chartDataDSMulti.chart.showDivLineValues = "1";
							$scope.chartDataDSMulti.chart.pYAxisName = STR_DWELL;
						}
						else {
							$scope.chartDataDSMulti.chart.showLimits = "0";
							$scope.chartDataDSMulti.chart.showDivLineValues = "0";
							$scope.chartDataDSMulti.chart.pYAxisName = "";
						}
					}
				}
				function CreateChartData(resdata, rptType) {
					//if (!resdata || resdata.length == 0 || (!resdata.Regions && !resdata.Sites)) {
					//	$scope.chartDataDSMulti.dataset = null;
					//	$scope.chartDataDSMulti.lineset = null;
					//	$scope.arrChartLegends = null;
					//	return;
					//}
					UpdatePYAxisName();
					if ($scope.ReportID == AppDefine.BamReportTypes.SALE) {
						//var pYName = STR_TRAFFIC;
						//if (srpt.arrChartTypes && srpt.arrChartTypes.length > 0) {
						//	var posTypes = Enumerable.From(srpt.arrChartTypes)
						//			.Where(function (ct) { return (ct.Key == AppDefine.BamDataTypes.POS) })
						//			.Select(function (x) { return x; }).FirstOrDefault();
						//	if (posTypes && posTypes.Checked == true) {
						//		pYName = STR_TRAFFIC + ', ' + STR_POS;
						//	}
						//}
						//$scope.chartDataDSMulti.chart.pYAxisName = pYName;//STR_TRAFFIC + ', ' + STR_POS;

						//$scope.chartDataDSMulti.chart.sYAxisName = STR_CONV;
						//$scope.chartDataDSMulti.chart.sNumberSuffix = "%";

						if (resdata && resdata.Sites && resdata.Sites.length == 1) {
							$scope.$parent.vm.showSiteChart = true;
							$scope.ShowChartSites = true;
							if (srpt.arrChartTypes && srpt.arrChartTypes.length > 2 && srpt.arrChartTypes[2].Checked == false) {
								srpt.arrChartTypes[2].Checked = true;
							}
							CreateSaleRptDataSSite(resdata, rptType); //Create chart for single site
						}
						else {
							CreateSaleRptData(resdata, rptType);
						}
					}
					else {
						$scope.chartDataDSMulti.chart.sNumberSuffix = "";
						CreateDistributionData(resdata, rptType);
					}
				}
				function CreateCategoriesLabels(grpData, catSub) {
					var regLen = grpData.length;
					for (var j = 0; j < regLen; j++) {
						var catIt = {};
						//catIt.label = grpData[j].Name;
						//switch ($scope.rptType) {
						//	case AppDefine.SaleReportTypes.Hourly:
						//		catIt.label = chartSvc.formatHourChart(grpData[j].TimeIndex);
						//		break;
						//	case AppDefine.SaleReportTypes.Weekly:
						//		catIt.label = cmsBase.translateSvc.getTranslate(AppDefine.Resx.WEEK_STRING) + grpData[j].TimeIndex;
						//		break;
						//	case AppDefine.SaleReportTypes.Monthly:
						//		catIt.label = cmsBase.translateSvc.getTranslate(AppDefine.Resx.PERIOD_STRING) + grpData[j].TimeIndex;
						//		break;
						//	default:
						//		catIt.label = $filter('date')(new Date(grpData[j].Date), AppDefine.CalDateFormat);
						//		break;
						//}
						catIt.label = chartSvc.formatChartLabel($scope.rptType, grpData[j]);
						catSub.push(catIt);
					}
				}

				//*********************** Sale Chart - Begin ***********************/
				function CreateDSGroup(grpData, parentname, dsLines, dsRegions, rptType, color, catSub) {
					var icount = 0;
					var icolor = 0;
					var regLen = grpData.length;
					var trafIn = {};
					trafIn.seriesname =  parentname + ':' + STR_COUNT_IN;
					trafIn.showValues = "0";
					trafIn.color = color;
					trafIn.data = [];
					trafIn.Type = AppDefine.BamDataTypes.TRAFFIC_IN;

					var trafOut = {};
					trafOut.seriesname = parentname + ':' + STR_COUNT_OUT;
					trafOut.showValues = "0";
					trafOut.color = color;
					trafOut.data = [];
					trafOut.Type = AppDefine.BamDataTypes.TRAFFIC_OUT;

					var trafPOS = {};
					trafPOS.seriesname = parentname + ':' + STR_POS;
					trafPOS.showValues = "0";
					trafPOS.data = [];
					trafPOS.Type = AppDefine.BamDataTypes.POS;

					var lnCnv = {};
					lnCnv.seriesname = parentname + ':' + STR_CONV;
					lnCnv.showValues = "0";
					lnCnv.color = color;
					lnCnv.data = [];
					lnCnv.Type = AppDefine.BamDataTypes.CONVERSION;

					var dsReg = [];
					for (var j = 0; j < regLen; j++) {
						//if (idx == 0 && catSub != null) {
						//	var catIt = {};
						//	catIt.label = grpData[j].Name;
						//	catSub.push(catIt);
						//}
						//var tooltip = CreateTooltip(grpData[j], parentname);

						var itIn = {};
						itIn.ID = grpData[j].ID;
						itIn.value = grpData[j].TrafficIn;
						itIn.tooltext = chartSvc.CreateBAMChartTooltip(grpData[j], parentname, AppDefine.BamDataTypes.TRAFFIC_IN, catSub[j].label);//tooltip;
						trafIn.data.push(itIn);

						var itOut = {};
						itOut.ID = grpData[j].ID;
						itOut.value = grpData[j].TrafficOut;
						itOut.tooltext = chartSvc.CreateBAMChartTooltip(grpData[j], parentname, AppDefine.BamDataTypes.TRAFFIC_OUT, catSub[j].label);//tooltip;
						trafOut.data.push(itOut);

						var itPos = {};
						itPos.ID = grpData[j].ID;
						itPos.value = grpData[j].CountTrans;
						itPos.tooltext = chartSvc.CreateBAMChartTooltip(grpData[j], parentname, AppDefine.BamDataTypes.POS, catSub[j].label);//tooltip;
						trafPOS.data.push(itPos);

						var itCnv = {};
						itCnv.ID = grpData[j].ID;
						itCnv.value = grpData[j].Conversion;
						itCnv.tooltext = chartSvc.CreateBAMChartTooltip(grpData[j], parentname, AppDefine.BamDataTypes.CONVERSION, catSub[j].label);//tooltip;
						lnCnv.data.push(itCnv);
						lnCnv.ID = grpData[j].ID;
					}

					dsReg.push(trafIn);
					dsReg.push(trafOut);
					dsReg.push(trafPOS);

					var dsRegIt = {};
					dsRegIt.dataset = dsReg;
					dsRegIt.ID = grpData[0].ID;
					dsRegions.push(dsRegIt);

					dsLines.push(lnCnv);
					/*
					if (idx == 0) {
						for (var j = $scope.DSItemIndex; j < regLen; j++) {
							if (icolor >= arrChartRegionColors || icolor < 0) icolor = 0;
							var region = {};
							region.ID = grpData[j].ID;
							region.ChartDSs = ChartDataSetAll(grpData[j].Name, rptType, grpData[j].ID);
							region.ChartDSs.forEach(function (item) {
								CreateDSItem(item, grpData[j], arrChartRegionColors[icolor]);
							});
							datasetAll.push(region);

							var varIDInfo = {};
							varIDInfo.ID = grpData[j].ID;
							varIDInfo.Name = grpData[j].Name;
							varIDInfo.Color = arrChartRegionColors[icolor];
							varIDInfo.Display = true;
							allIDs.push(varIDInfo);

							icolor++;
							icount++;
							if (icount >= $scope.DSItemLimit) {
								break;
							}
						}
					}
					else {
						for (var j = $scope.DSItemIndex; j < regLen; j++) {
							var region = Enumerable.From(datasetAll)
								.Where(function (reg) { return (reg.ID == grpData[j].ID) })
								.Select(function (x) { return x; })
								.FirstOrDefault();
							if (icolor >= arrChartRegionColors || icolor < 0) icolor = 0;
							if (region != null && region != undefined) {
								region.ChartDSs.forEach(function (item) {
									CreateDSItem(item, grpData[j], arrChartRegionColors[icolor]);
								});
							} //if

							icolor++;
							icount++;
							if (icount >= $scope.DSItemLimit) {
								break;
							}
						} //for j
					} //else if i = 0
					*/
				}
				function CreateSaleRptData(resdata, rptType) {
					if (!resdata || ((!resdata.Regions || resdata.Regions.length == 0) && (!resdata.Sites || resdata.Sites.length == 0))) {
						$scope.chartDataDSMulti.dataset = null;
						$scope.chartDataDSMulti.lineset = null;
						$scope.arrChartLegends = null;
						return;
					}
					$scope.JSChartType = JSChartTypes.Stack;

					var catArray = [];
					var dsArray = [];
					var catSub = [];

					//var dsDataAlls = ChartDataSetAll(null, rptType, 0);
					var dsRegions = new Array();
					var dsLines = new Array();
					var allRegionIDs = [];

					if (resdata.Regions != null && resdata.Regions != undefined) {
						var datLen = resdata.Regions.length;
						var icolor = 0;
						var icount = 0;
						for (var i = $scope.DSItemIndex; i < datLen; i++) {
							if (icolor >= arrChartRegionColors.length || icolor < 0) icolor = 0;
							//var catIt = {};
							//catIt.label = resdata[i].Name;
							//catSub.push(catIt);
							if (resdata.Regions[i].Details != null && resdata.Regions[i].Details != undefined && resdata.Regions[i].Details.length > 0) {
								if (icount == 0) CreateCategoriesLabels(resdata.Regions[i].Details, catSub);

								CreateDSGroup(resdata.Regions[i].Details, resdata.Regions[i].Name, dsLines, dsRegions, rptType, arrChartRegionColors[icolor], catSub);
							} //if has Regions

							var varIDInfo = {};
							varIDInfo.ID = resdata.Regions[i].ID;
							varIDInfo.Name = resdata.Regions[i].Name;
							varIDInfo.Color = arrChartRegionColors[icolor];
							varIDInfo.Display = true;
							allRegionIDs.push(varIDInfo);

							icolor++;
							icount++;
							if (icount >= $scope.DSItemLimit) {
								break;
							}
						} //for i
						$scope.DSItemRegion = datLen;
						$scope.RegionLegends = allRegionIDs;
					}

					var dsSites = [];
					var dsLinesSite = [];
					var allSiteIDs = [];
					var dsArraySite = [];
					var catSubSite = [];

					if (resdata.Sites != null && resdata.Sites != undefined) {
						var datLen = resdata.Sites.length;
						var icolor = 0;
						var icount = 0;

						for (var i = $scope.DSItemIndex; i < datLen; i++) {
							if (icolor >= arrChartRegionColors.length || icolor < 0) icolor = 0;

							if (resdata.Sites[i].Details != null && resdata.Sites[i].Details != undefined && resdata.Sites[i].Details.length > 0) {
								if (icount == 0) CreateCategoriesLabels(resdata.Sites[i].Details, catSubSite);

								CreateDSGroup(resdata.Sites[i].Details, resdata.Sites[i].Name, dsLinesSite, dsSites, rptType, arrChartRegionColors[icolor], catSubSite);
							} //if has Site

							var varIDInfo = {};
							varIDInfo.ID = resdata.Sites[i].ID;
							varIDInfo.Name = resdata.Sites[i].Name;
							varIDInfo.Color = arrChartRegionColors[icolor];
							varIDInfo.Display = true;
							allSiteIDs.push(varIDInfo);

							icolor++;
							icount++;
							if (icount >= $scope.DSItemLimit) {
								break;
							}
							$scope.DSItemSite = datLen;
							$scope.SiteLegends = allSiteIDs;
							$scope.SupportSites = true;
						}
						//if (resdata[i].Sites != null && resdata[i].Sites != undefined && resdata[i].Sites.length > 0) {
						//	CreateDSGroup(i, resdata[i].Sites, allSiteIDs, dsSites, rptType);
						//	$scope.DSItemSite = resdata[i].Sites.length;
						//	$scope.SiteLegends = allSiteIDs;
						//	$scope.SupportSites = true;
						//}
					}

					if ($scope.ShowChartSites) {
						var catCol = {};
						catCol.category = catSubSite;
						catArray.push(catCol);
					}
					else {
						var catCol = {};
						catCol.category = catSub;
						catArray.push(catCol);
					}

					if (dsRegions != null && dsRegions.length > 0) {
						dsRegions.forEach(function (item) {
							if (item != null && item != undefined) {
								// && item.ChartDSs != null && item.ChartDSs != undefined && item.ChartDSs.length > 0) {
								//item.ChartDSs.forEach(function (dsIt) {
								//	dsArray.push(dsIt);
								//});
								dsArray.push(item);
							}
							//dsArray.push(item);
						});
					}

					if (dsSites != null && dsSites.length > 0) {
						dsSites.forEach(function (item) {
							if (item != null && item != undefined) {
								// && item.ChartDSs != null && item.ChartDSs != undefined && item.ChartDSs.length > 0) {
								//item.ChartDSs.forEach(function (dsIt) {
								//	dsArraySite.push(dsIt);
								//});
								dsArraySite.push(item);
							}
						});
					}

					//console.log(dsArray);

					$scope.chartDataDSMulti.categories = catArray;

					$scope.AllChartDataSet = dsArray;
					$scope.AllChartDataSetSites = dsArraySite;

					$scope.AllChartLineSet = dsLines;
					$scope.AllChartLineSetSites = dsLinesSite;

					if ($scope.ShowChartSites) {
						$scope.DSItemNumber = $scope.DSItemSite;
						//$scope.chartDataDSMulti.dataset = angular.copy($scope.AllChartDataSetSites);
						//$scope.chartDataDSMulti.lineset = angular.copy($scope.AllChartLineSetSites);

						$scope.arrChartLegends = $scope.SiteLegends;
					}
					else {
						$scope.DSItemNumber = $scope.DSItemRegion;
						//$scope.chartDataDSMulti.dataset = angular.copy($scope.AllChartDataSet);
						//$scope.chartDataDSMulti.lineset = angular.copy($scope.AllChartLineSet);

						$scope.arrChartLegends = $scope.RegionLegends;
					}
					RefreshChartDSByAll(false);
				}
				//************************ Sale Chart - End ************************/

				//********************** Distribution Chart - Begin **********************/
				function CreateDSGroupDist(grpData, parentname, dsLines, dsRegions, rptType, color, catSub) {
					var icount = 0;
					var icolor = 0;
					var regLen = grpData.length;
					var trafIn = {};
					trafIn.seriesname = parentname + ':' + STR_DWELL;
					trafIn.showValues = "0";
					trafIn.color = color;
					trafIn.data = [];
					trafIn.Type = AppDefine.BamDataTypes.DWELL;

					var lnCnv = {};
					lnCnv.seriesname = parentname + ':' + STR_COUNT;
					lnCnv.showValues = "0";
					lnCnv.color = color;
					lnCnv.data = [];
					lnCnv.Type = AppDefine.BamDataTypes.COUNT;

					var subItems = [];

					var dsReg = [];
					for (var j = 0; j < regLen; j++) {
						if (grpData[j].Details != null && grpData[j].Details.length > 0) {
							var subLen = grpData[j].Details.length;
							if (subItems.length == 0) {
								for (var si = 0; si < subLen; si++) {
									var trafCount = {};
									trafCount.seriesname = grpData[j].Details[si].Name + ':' + STR_DWELL;
									trafCount.showValues = "0";
									//trafCount.color = color;
									trafCount.data = [];
									trafCount.Type = AppDefine.BamDataTypes.DWELL;

									var itIn = {};
									itIn.ID = grpData[j].Details[si].ID;
									itIn.value = grpData[j].Details[si].Dwell;
									itIn.tooltext = chartSvc.CreateBAMChartTooltip(grpData[j].Details[si], grpData[j].Details[si].Name, AppDefine.BamDataTypes.DWELL, catSub[j].label);
									trafCount.data.push(itIn);

									subItems.push(trafCount);
								} //for si
							} //first item
							else {
								for (var si = 0; si < subLen; si++) {
									var trafCount = subItems[si];
									var itIn = {};
									itIn.ID = grpData[j].Details[si].ID;
									itIn.value = grpData[j].Details[si].Dwell;
									itIn.tooltext = chartSvc.CreateBAMChartTooltip(grpData[j].Details[si], grpData[j].Details[si].Name, AppDefine.BamDataTypes.DWELL, catSub[j].label);//tooltip;
									trafCount.data.push(itIn);
								} //for si
							} //next items
						}
						else {
							var itIn = {};
							itIn.ID = grpData[j].ID;
							itIn.value = grpData[j].Dwell;
							itIn.tooltext = chartSvc.CreateBAMChartTooltip(grpData[j], parentname, AppDefine.BamDataTypes.DWELL, catSub[j].label);//tooltip;
							trafIn.data.push(itIn);
						}

						var itCnv = {};
						itCnv.ID = grpData[j].ID;
						itCnv.value = grpData[j].Count;
						itCnv.tooltext = chartSvc.CreateBAMChartTooltip(grpData[j], parentname, AppDefine.BamDataTypes.COUNT, catSub[j].label);//tooltip;
						lnCnv.data.push(itCnv);
						lnCnv.ID = grpData[j].ID;
					}
					if (subItems.length > 0) {
						subItems.forEach(function (item) {
							if (item != null && item != undefined) {
								dsReg.push(item);
							}
						});
					}
					else {
						dsReg.push(trafIn);
						//dsReg.push(trafOut);
						//dsReg.push(trafPOS);
					}

					var dsRegIt = {};
					dsRegIt.dataset = dsReg;
					dsRegIt.ID = grpData[0].ID;
					dsRegions.push(dsRegIt);

					dsLines.push(lnCnv);
				}
				function CreateDistributionData(resdata, rptType) {
					if (!resdata || !resdata.Regions || resdata.Regions.length == 0) {
						$scope.chartDataDSMulti.dataset = null;
						$scope.chartDataDSMulti.lineset = null;
						$scope.arrChartLegends = null;
						return;
					}
					$scope.JSChartType = JSChartTypes.Stack;
					$scope.chartDataDSMulti.chart.pYAxisName = STR_DWELL;
					$scope.chartDataDSMulti.chart.sYAxisName = STR_COUNT;

					var catArray = [];
					var dsArray = [];
					var catSub = [];

					//var dsDataAlls = ChartDataSetAll(null, rptType, 0);
					var dsRegions = new Array();
					var dsLines = new Array();
					var allRegionIDs = [];

					var datLen = resdata.Regions.length;
					var icolor = 0;
					var icount = 0;
					for (var i = $scope.DSItemIndex; i < datLen; i++) {
						if (icolor >= arrChartRegionColors.length || icolor < 0) icolor = 0;

						if (resdata.Regions[i].Details != null && resdata.Regions[i].Details != undefined && resdata.Regions[i].Details.length > 0) {
							if (icount == 0) CreateCategoriesLabels(resdata.Regions[i].Details, catSub);

							CreateDSGroupDist(resdata.Regions[i].Details, resdata.Regions[i].Name, dsLines, dsRegions, rptType, arrChartRegionColors[icolor], catSub);
						} //if has Regions

						var varIDInfo = {};
						varIDInfo.ID = resdata.Regions[i].ID;
						varIDInfo.Name = resdata.Regions[i].Name;
						varIDInfo.Color = arrChartRegionColors[icolor];
						varIDInfo.Display = true;
						allRegionIDs.push(varIDInfo);

						icolor++;
						icount++;
						if (icount >= $scope.DSItemLimit) {
							break;
						}
					} //for i
					$scope.DSItemRegion = datLen;
					$scope.RegionLegends = allRegionIDs;

					var catCol = {};
					catCol.category = catSub;
					catArray.push(catCol);

					if (dsRegions != null && dsRegions.length > 0) {
						dsRegions.forEach(function (item) {
							if (item != null && item != undefined) {
								dsArray.push(item);
							}
						});
					}

					$scope.chartDataDSMulti.categories = catArray;

					$scope.AllChartDataSet = dsArray;
					$scope.AllChartLineSet = dsLines;

					$scope.DSItemNumber = $scope.DSItemRegion;
					$scope.arrChartLegends = $scope.RegionLegends;

					RefreshChartDSByAll(false);
				}
				//*********************** Distribution Chart - End ***********************/

				//*********************** Sale Chart single Site - Begin ***********************/
				function CreateDSGroupSSite(grpData, parentname, dsLines, dsRegions, rptType, color, catSub) {
					var icount = 0;
					var icolor = 0;
					var regLen = grpData.length;
					var trafIn = {};
					trafIn.seriesname = parentname + ':' + STR_COUNT_IN;
					trafIn.showValues = "0";
					trafIn.color = arrChartRegionColors[0];//color;
					trafIn.data = [];
					trafIn.Type = AppDefine.BamDataTypes.TRAFFIC_IN;
					trafIn.renderAs = 'line';

					var trafOut = {};
					trafOut.seriesname = parentname + ':' + STR_COUNT_OUT;
					trafOut.showValues = "0";
					trafOut.color = arrChartRegionColors[1];//color;
					trafOut.data = [];
					trafOut.Type = AppDefine.BamDataTypes.TRAFFIC_OUT;
					trafOut.renderAs = 'line';

					var trafPOS = {};
					trafPOS.seriesname = parentname + ':' + STR_POS;
					trafPOS.showValues = "0";
					trafPOS.color = arrChartRegionColors[2];//color;
					trafPOS.data = [];
					trafPOS.Type = AppDefine.BamDataTypes.POS;
					trafPOS.renderAs = 'line';

					var lnCnv = {};
					lnCnv.seriesname = parentname + ':' + STR_CONV;
					lnCnv.showValues = "0";
					lnCnv.color = arrChartRegionColors[3];//color;
					lnCnv.data = [];
					lnCnv.Type = AppDefine.BamDataTypes.CONVERSION;
					lnCnv.renderAs = 'line';
					lnCnv.parentYAxis = 'S';

					var dsReg = [];
					for (var j = 0; j < regLen; j++) {

						var itIn = {};
						itIn.ID = grpData[j].ID;
						itIn.value = grpData[j].TrafficIn;
						itIn.tooltext = chartSvc.CreateBAMChartTooltip(grpData[j], parentname, AppDefine.BamDataTypes.TRAFFIC_IN, catSub[j].label);//tooltip;
						trafIn.data.push(itIn);

						var itOut = {};
						itOut.ID = grpData[j].ID;
						itOut.value = grpData[j].TrafficOut;
						itOut.tooltext = chartSvc.CreateBAMChartTooltip(grpData[j], parentname, AppDefine.BamDataTypes.TRAFFIC_OUT, catSub[j].label);//tooltip;
						trafOut.data.push(itOut);

						var itPos = {};
						itPos.ID = grpData[j].ID;
						itPos.value = grpData[j].CountTrans;
						itPos.tooltext = chartSvc.CreateBAMChartTooltip(grpData[j], parentname, AppDefine.BamDataTypes.POS, catSub[j].label);//tooltip;
						trafPOS.data.push(itPos);

						var itCnv = {};
						itCnv.ID = grpData[j].ID;
						itCnv.value = grpData[j].Conversion;
						itCnv.tooltext = chartSvc.CreateBAMChartTooltip(grpData[j], parentname, AppDefine.BamDataTypes.CONVERSION, catSub[j].label);//tooltip;
						lnCnv.data.push(itCnv);
						lnCnv.ID = grpData[j].ID;
					}
					/*
					dsReg.push(trafIn);
					dsReg.push(trafOut);
					//dsReg.push(trafPOS);

					var dsRegIt = {};
					dsRegIt.dataset = dsReg;
					dsRegIt.ID = grpData[0].ID;
					dsRegions.push(dsRegIt);

					var dsRegItPos = {};
					dsRegItPos.dataset = [];
					dsRegItPos.dataset.push(trafPOS);
					dsRegItPos.ID = grpData[0].ID;
					dsRegions.push(dsRegItPos);*/
					dsLines.push(trafIn);
					dsLines.push(trafOut);
					dsLines.push(trafPOS);

					dsLines.push(lnCnv);
				}
				function CreateSaleRptDataSSite(resdata, rptType) {
					if (!resdata || ((!resdata.Regions || resdata.Regions.length == 0) && (!resdata.Sites || resdata.Sites.length == 0))) {
						$scope.chartDataDSMulti.dataset = null;
						$scope.chartDataDSMulti.lineset = null;
						$scope.arrChartLegends = null;
						return;
					}
					//$scope.chartDataDSMulti.chart.pYAxisName = STR_TRAFFIC;
					//$scope.chartDataDSMulti.chart.sYAxisName = STR_CONV;
					$scope.JSChartType = JSChartTypes.Combine;

					var catArray = [];
					var dsArray = [];
					var catSub = [];

					//var dsDataAlls = ChartDataSetAll(null, rptType, 0);
					var dsRegions = new Array();
					var dsLines = new Array();
					var allRegionIDs = [];

					if (resdata.Regions != null && resdata.Regions != undefined) {
						var datLen = resdata.Regions.length;
						var icolor = 0;
						var icount = 0;
						for (var i = $scope.DSItemIndex; i < datLen; i++) {
							if (icolor >= arrChartRegionColors.length || icolor < 0) icolor = 0;
							if (resdata.Regions[i].Details != null && resdata.Regions[i].Details != undefined && resdata.Regions[i].Details.length > 0) {
								if (icount == 0) CreateCategoriesLabels(resdata.Regions[i].Details, catSub);

								CreateDSGroupSSite(resdata.Regions[i].Details, resdata.Regions[i].Name, dsLines, dsRegions, rptType, arrChartRegionColors[icolor], catSub);
							} //if has Regions

							var varIDInfo = {};
							varIDInfo.ID = resdata.Regions[i].ID;
							varIDInfo.Name = resdata.Regions[i].Name;
							varIDInfo.Color = arrChartRegionColors[icolor];
							varIDInfo.Display = true;
							allRegionIDs.push(varIDInfo);

							icolor++;
							icount++;
							if (icount >= $scope.DSItemLimit) {
								break;
							}
						} //for i
						$scope.DSItemRegion = datLen;
						$scope.RegionLegends = allRegionIDs;
					}

					var dsSites = [];
					var dsLinesSite = [];
					var allSiteIDs = [];
					var dsArraySite = [];
					var catSubSite = [];

					if (resdata.Sites != null && resdata.Sites != undefined) {
						var datLen = resdata.Sites.length;
						var icolor = 0;
						var icount = 0;

						for (var i = $scope.DSItemIndex; i < datLen; i++) {
							if (icolor >= arrChartRegionColors.length || icolor < 0) icolor = 0;

							if (resdata.Sites[i].Details != null && resdata.Sites[i].Details != undefined && resdata.Sites[i].Details.length > 0) {
								if (icount == 0) CreateCategoriesLabels(resdata.Sites[i].Details, catSubSite);

								CreateDSGroupSSite(resdata.Sites[i].Details, resdata.Sites[i].Name, dsLinesSite, dsSites, rptType, arrChartRegionColors[icolor], catSubSite);
							} //if has Site

							var varIDInfo = {};
							varIDInfo.ID = resdata.Sites[i].ID;
							varIDInfo.Name = resdata.Sites[i].Name;
							varIDInfo.Color = arrChartRegionColors[icolor];
							varIDInfo.Display = true;
							allSiteIDs.push(varIDInfo);

							icolor++;
							icount++;
							if (icount >= $scope.DSItemLimit) {
								break;
							}
							$scope.DSItemSite = datLen;
							$scope.SiteLegends = allSiteIDs;
							$scope.SupportSites = true;
						}
					}

					if ($scope.ShowChartSites) {
						var catCol = {};
						catCol.category = catSubSite;
						catArray.push(catCol);
					}
					else {
						var catCol = {};
						catCol.category = catSub;
						catArray.push(catCol);
					}

					if (dsRegions != null && dsRegions.length > 0) {
						dsRegions.forEach(function (item) {
							if (item != null && item != undefined) {
								dsArray.push(item);
							}
						});
					}

					if (dsSites != null && dsSites.length > 0) {
						dsSites.forEach(function (item) {
							if (item != null && item != undefined) {
								dsArraySite.push(item);
							}
						});
					}

					$scope.chartDataDSMulti.categories = catArray;

					$scope.AllChartDataSet = dsLines;//dsArray;
					$scope.AllChartDataSetSites = dsLinesSite;//dsArraySite;

					$scope.AllChartLineSet = null;//dsLines;
					$scope.AllChartLineSetSites = null;//dsLinesSite;

					if ($scope.ShowChartSites) {
						$scope.DSItemNumber = $scope.DSItemSite;

						$scope.arrChartLegends = $scope.SiteLegends;
					}
					else {
						$scope.DSItemNumber = $scope.DSItemRegion;

						$scope.arrChartLegends = $scope.RegionLegends;
					}
					RefreshChartDSByAll(false);
				}
				//************************ Sale Chart single Site - End ************************/

				function CreateDSItem(item, data, color) {
					var itChart = {};
					if (item.Type == AppDefine.BamDataTypes.CONVERSION) {
						itChart.value = data.Conversion.toFixed(2);
						if (color != null)
							itChart.color = color;//chartSvc.GetConvColor(data.Conversion);
					}
					else if (item.Type == AppDefine.BamDataTypes.POS) {
						itChart.value = data.CountTrans;
						if (color != null)
							itChart.color = color;
					}
					else if (item.Type == AppDefine.BamDataTypes.TRAFFIC_IN) {
						itChart.value = data.TrafficIn;
						if (color != null)
							itChart.color = color;
					}
					else if (item.Type == AppDefine.BamDataTypes.TRAFFIC_OUT) {
						itChart.value = data.TrafficOut;
						if (color != null)
							itChart.color = color;
					}
					else if (item.Type == AppDefine.BamDataTypes.COUNT) {
						itChart.value = data.Count;
						if (color != null)
							itChart.color = color;
					}
					else if (item.Type == AppDefine.BamDataTypes.DWELL) {
						itChart.value = data.Dwell;
						if (color != null)
							itChart.color = color;
					}
					itChart.tooltext = CreateTooltip(data, data.Name);

					item.data.push(itChart);
				}
				function ChartDataSetAll(sname, rptType, sid) {
					var arrDataSets = new Array();
					if (rptType == AppDefine.BamReportTypes.DRIVETHROUGH || rptType == AppDefine.BamReportTypes.DISTRIBUTE) {
						var dsCount = {};
						dsCount.ID = sid,
						dsCount.Type = AppDefine.BamDataTypes.COUNT;
						dsCount.seriesname = (sname != null) ? (sname + ' ' + STR_COUNT) : STR_COUNT;
						dsCount.showValues = "0";
						dsCount.renderAs = "Column";
						dsCount.parentYAxis = "P";
						dsCount.data = [];
						arrDataSets.push(dsCount);

						var dsDWell = {};
						dsDWell.ID = sid,
						dsDWell.Type = AppDefine.BamDataTypes.DWELL;
						dsDWell.seriesname = (sname != null) ? (sname + ' ' + STR_DWELL) : STR_DWELL;
						dsDWell.showValues = "0";
						dsDWell.renderAs = "line";
						dsDWell.parentYAxis = "S";
						dsDWell.data = [];
						arrDataSets.push(dsDWell);
					}
					else {
						var dsConvAll = {};
						dsConvAll.ID = sid,
						dsConvAll.Type = AppDefine.BamDataTypes.CONVERSION;
						dsConvAll.seriesname = (sname != null) ? (sname + ' ' + STR_CONV) : STR_CONVERSION;
						dsConvAll.showValues = "0";
						dsConvAll.renderAs = "Column";
						dsConvAll.parentYAxis = "S";
						dsConvAll.data = [];
						arrDataSets.push(dsConvAll);

						var dsPOSAll = {};
						dsPOSAll.ID = sid,
						dsPOSAll.Type = AppDefine.BamDataTypes.POS;
						dsPOSAll.seriesname = (sname != null) ? (sname + ' ' + STR_POS) : STR_POS;
						dsPOSAll.showValues = "0";
						dsPOSAll.renderAs = "Column";
						dsPOSAll.parentYAxis = "S";
						dsPOSAll.data = [];
						arrDataSets.push(dsPOSAll);

						var dsCInAll = {};
						dsCInAll.ID = sid,
						dsCInAll.Type = AppDefine.BamDataTypes.TRAFFIC_IN;
						dsCInAll.seriesname = (sname != null) ? (sname + ' ' + STR_COUNT_IN) : STR_COUNT_IN;
						dsCInAll.showValues = "0";
						dsCInAll.renderAs = "line";
						dsCInAll.data = [];
						arrDataSets.push(dsCInAll);

						var dsCOutAll = {};
						dsCOutAll.ID = sid,
						dsCOutAll.Type = AppDefine.BamDataTypes.TRAFFIC_OUT;
						dsCOutAll.seriesname = (sname != null) ? (sname + ' ' + STR_COUNT_OUT) : STR_COUNT_OUT;
						dsCOutAll.showValues = "0";
						dsCOutAll.renderAs = "line";
						dsCOutAll.data = [];
						arrDataSets.push(dsCOutAll);
					}

					return arrDataSets;
				}

				//*************************************************************************************************************/
				//****************************************** Chart Data - End *************************************************/
				//*************************************************************************************************************/
				$scope.Init = function () {
					//$scope.testInfo = $scope.$parent.showInfo;
				}

				$scope.$on(AppDefine.Events.CHARTDATALOADED, function (e, data) {
					if (!data) { return; }
					
					//console.log(data);
					$scope.ChartDataParamAll = data;
					$scope.rptType = data.rptType;
					$scope.ReportID = data.ReportID;

					$scope.DSItemIndex = 0;
					$scope.ChartToolbars = true;
					$scope.isLastPage = false;

					if (srpt.arrChartTypes == null || srpt.arrChartTypes == undefined || srpt.arrChartTypes.length == 0) {
						srpt.arrChartTypes = angular.copy(data.ChartTypes);
						SetDefaultViewChartTypes();
					}

					CreateChartData(data.ChartData, data.ReportID);

					$scope.ChartLoaded = true;
				}); //on CHARTDATALOADED

				function ShowNextBackChart(isNext) {
					if ($scope.ShowChartSites) {
						$scope.DSItemNumber = $scope.DSItemSite;
					}
					else {
						$scope.DSItemNumber = $scope.DSItemRegion;
					}
					$scope.isLastPage = false;

					//$scope.DSItemIndex += $scope.DSItemLimit;
					if (isNext) {
						if (($scope.DSItemIndex + $scope.DSItemLimit) < $scope.DSItemNumber) {
							$scope.DSItemIndex += $scope.DSItemLimit;
							CreateChartData($scope.ChartDataParamAll.ChartData, $scope.ChartDataParamAll.ReportID);
							//RefreshChartDS();
							//RefreshChartDSByAll(false);
							if(($scope.DSItemIndex + $scope.DSItemLimit) >= $scope.DSItemNumber)
								$scope.isLastPage = true;
						}
					}
					else {
						if (($scope.DSItemIndex - $scope.DSItemLimit) > 0) {
							$scope.DSItemIndex -= $scope.DSItemLimit;
						}
						else {
							$scope.DSItemIndex = 0;
						}
						CreateChartData($scope.ChartDataParamAll.ChartData, $scope.ChartDataParamAll.ReportID);
						//RefreshChartDS();
						//RefreshChartDSByAll(false);
					} //back
				}

				function RefreshChartDSByAll(showAll) {
					var allDatasets = [];
					var allLinesets = [];

					if ($scope.ShowChartSites) {
						allDatasets = angular.copy($scope.AllChartDataSetSites);
						allLinesets = $scope.AllChartLineSetSites;

						$scope.DSItemNumber = $scope.DSItemSite;
						$scope.arrChartLegends = $scope.SiteLegends;
					}
					else {
						allDatasets = angular.copy($scope.AllChartDataSet);
						allLinesets = $scope.AllChartLineSet;

						$scope.DSItemNumber = $scope.DSItemRegion;
						$scope.arrChartLegends = $scope.RegionLegends;
					}

					if (showAll) {
						$scope.chartDataDSMulti.dataset = allDatasets;
						$scope.chartDataDSMulti.lineset = allLinesets;
					}
					else {
						var dataset = null;
						var allDatas = [];

						var lineset = null;
						var allLines = [];

						//Reset data to show all
						dataset = Enumerable.From(allDatasets)
								  .Where(function (ds) { return (ds.visible == '0') })
								  .Select(function (x) { return x; }).ToArray(); //.FirstOrDefault();

						if (dataset != null && dataset != undefined && dataset.length > 0) {
							dataset.forEach(function (item) {
								item.visible = '1';
							}); //foreach
						} //if

						if (allLinesets) {
							lineset = Enumerable.From(allLinesets)
									  .Where(function (ds) { return (ds.visible == '0') })
									  .Select(function (x) { return x; }).ToArray(); //.FirstOrDefault();

							if (lineset != null && lineset != undefined && lineset.length > 0) {
								lineset.forEach(function (item) {
									item.visible = '1';
								}); //foreach
							} //if
						}

						if (srpt.arrChartTypes != null && srpt.arrChartTypes.length > 0) {
							//Hide by Types
							var arrHideTypes = Enumerable.From(srpt.arrChartTypes)
									.Where(function (ct) { return (ct.Checked == false) })
									.Select(function (x) { return x.Key; }).ToArray();

							if (allLinesets) {
								//Line set != null, use for multi sites chart
								if (arrHideTypes.indexOf(AppDefine.BamDataTypes.CONVERSION) >= 0) {
									allLinesets = null; //Hide line chart
								}

								allDatasets.forEach(function (item) {
									if (item.dataset != null) {
										for (var i = item.dataset.length - 1; i >= 0; i--) {
											if (arrHideTypes.indexOf(item.dataset[i].Type) >= 0) {
												item.dataset.splice(i, 1);
												//break;
											}
										}
									}
								}); //foreach
							}
							else {
								//Single site chart here
								allDatasets.forEach(function (item) {
									if (arrHideTypes.indexOf(item.Type) >= 0) {
										item.visible = '0';
										//break;
									}
								}); //foreach
							}
							//Hide by Types - End
						}
						else {
							if ($scope.ShowChartTypes[1] === false) {
								//arrHideTypes.push(AppDefine.BamDataTypes.COUNT);
								//allDatasets = null;
								allDatasets.forEach(function (item) {
									if (item.visible == undefined || item.visible != '0') {
										item.dataset = [];
									}
								}); //foreach
							}
							if ($scope.ShowChartTypes[0] === false) {
								//arrHideTypes.push(AppDefine.BamDataTypes.DWELL);
								allLinesets = null; //Hide line chart
							}
						}

						//Hide by Legends
						var arrHideIDs = Enumerable.From($scope.arrChartLegends)
								.Where(function (ds) { return (ds.Display == false) })
								.Select(function (x) { return x.ID; }).ToArray();

						dataset = Enumerable.From(allDatasets)
								  .Where(function (ds) { return arrHideIDs.indexOf(ds.ID) >= 0 })//(ds.ID == dsID)
								  .Select(function (x) { return x; }).ToArray(); //.FirstOrDefault();

						if (dataset != null && dataset != undefined && dataset.length > 0) {
							dataset.forEach(function (item) {
								item.visible = '0';
							}); //foreach
						} //if

						if (allLinesets != null && allLinesets != undefined && allLinesets.length > 0) {
							lineset = Enumerable.From(allLinesets)
								  .Where(function (ds) { return arrHideIDs.indexOf(ds.ID) >= 0 })//(ds.ID == dsID)
								  .Select(function (x) { return x; }).ToArray(); //.FirstOrDefault();

							if (lineset != null && lineset != undefined && lineset.length > 0) {
								lineset.forEach(function (item) {
									item.visible = '0';
								}); //foreach
							} //if
						}
						//Hide by Legends - End

						if (allDatasets != null && allDatasets != undefined && allDatasets.length > 0) {
							allDatasets.forEach(function (item) {
								if (item.visible == undefined || item.visible != '0') {
									allDatas.push(item);
								}
							}); //foreach
						}

						if (allLinesets != null && allLinesets != undefined && allLinesets.length > 0) {
							allLinesets.forEach(function (item) {
								if (item.visible == undefined || item.visible != '0') {
									allLines.push(item);
								}
							}); //foreach
						}

						$scope.chartDataDSMulti.dataset = allDatas;
						$scope.chartDataDSMulti.lineset = allLines;
					}
					RenderChart($scope.JSChartType);
				}

				function CreateTooltip(data, pname) {
					// Conversion CountTrans TrafficIn TrafficOut Count Dwell
					var strTooltip = '<div class="bam_tooltip"><ul>';//'<div class="bam_tooltip"><ul><li>sdsdsds</li><li>sdsdsds</li><li>sdsdsds</li><li> <span class="green bold">sd</span>sdsds</li></ul></div>';
					if (data.hasOwnProperty('Name')) {
						strTooltip += '<li><span class="yellow bold">' + pname + ' - '+ data.Name + '</span></li><li><span> &nbsp;</span></li>';
					}
					else {
						strTooltip += '<li><span class="yellow bold">' + pname + '</span></li><li><span> &nbsp;</span></li>';
					}
					if (data.hasOwnProperty('Conversion')) {
						strTooltip += '<li>Conversion: <span class="green bold">' + data.Conversion.toFixed(2) + ' %</span></li>';
					}
					if (data.hasOwnProperty('CountTrans')) {
						strTooltip += '<li>POS: <span class="green bold">' + data.CountTrans + '</span></li>';
					}
					if (data.hasOwnProperty('TrafficIn')) {
						strTooltip += '<li>Count In: <span class="green bold">' + data.TrafficIn + '</span></li>';
					}
					if (data.hasOwnProperty('TrafficOut')) {
						strTooltip += '<li>Count Out: <span class="green bold">' + data.TrafficOut + '</span></li>';
					}
					if (data.hasOwnProperty('Count')) {
						strTooltip += '<li>Count: <span class="green bold">' + data.Count + '</span></li>';
					}
					if (data.hasOwnProperty('Dwell')) {
						strTooltip += '<li>Dwell: <span class="green bold">' + data.Dwell + '</span></li>';
					}
					strTooltip += '</ul></div>';
					return strTooltip;
				}

				function SetDefaultViewChartTypes() {
					if (srpt.arrChartTypes != null && srpt.arrChartTypes.length > 0) {
						var arrHideTypes = Enumerable.From(srpt.arrChartTypes)
									.Where(function (ct) { return (ct.Checked == false) })
									.Select(function (x) { return x; }).ToArray();
						if (!arrHideTypes && arrHideTypes.length > 0) {
							arrHideTypes.forEach(function (item) {
								item.Checked = true;
							}); //foreach
						} //if
						UpdateTrafficOpts();
					}
					else {
						$scope.ShowChartTypes[0] = true;
						$scope.ShowChartTypes[1] = true;
					}
				}
				function UpdateTrafficOpts() {
					$scope.TrafficName = srpt.arrChartTypes[0].Name;
					if (srpt.arrChartTypes != null && srpt.arrChartTypes.length > 0) {
						if ($scope.selectedTypes.Key == AppDefine.BamDataTypes.TRAFFIC_IN) {
							srpt.arrChartTypes[0].Checked = true;
							srpt.arrChartTypes[1].Checked = false;
							$scope.TrafficName = srpt.arrChartTypes[0].Name;
						}
						else if ($scope.selectedTypes.Key == AppDefine.BamDataTypes.TRAFFIC_OUT){
							srpt.arrChartTypes[0].Checked = false;
							srpt.arrChartTypes[1].Checked = true;
							$scope.TrafficName = srpt.arrChartTypes[1].Name;
						}
						else {
							srpt.arrChartTypes[0].Checked = false;
							srpt.arrChartTypes[1].Checked = false;
						}
					}
				}

				$scope.ShowHideChartType = function (type) {
					UpdatePYAxisName();
					RefreshChartDSByAll(false);
				}

				$scope.ShowHideChart = function (isP) {
					var popupID = isP ? "#btn-popMenuAlert_ChartP" : "#btn-popMenuAlert_ChartS";
					if ($(popupID).parent().hasClass("open")) {
						$(popupID).parent().removeClass("open");
						$(popupID).prop("aria-expanded", false);
					}
					UpdateTrafficOpts();
					//RefreshChartDS();
					UpdatePYAxisName();
					RefreshChartDSByAll(false);
				}

				$scope.Next = function () {
					ShowNextBackChart(true);
				}

				$scope.Back = function () {
					ShowNextBackChart(false);
				}

				$scope.clickOnLegend = function (id) {
					var dataset = Enumerable.From($scope.arrChartLegends)
								  .Where(function (ds) { return (ds.ID == id) })
								  .Select(function (x) { return x; }).FirstOrDefault();
					if (dataset != null && dataset != undefined) {
						dataset.Display = !dataset.Display;
						//RefreshChartDSByID(id, dataset.Display);
						RefreshChartDSByAll(false);
					}
				}

				$scope.$parent.$watch('vm.showSiteChart', function (value) {
					//alert(value);
					if ($scope.ChartDataParamAll == null || $scope.ChartDataParamAll == undefined) {
						return;
					}
					$scope.ShowChartSites = value;
					$scope.DSItemIndex = 0;

					SetDefaultViewChartTypes();
					CreateChartData($scope.ChartDataParamAll.ChartData, $scope.ChartDataParamAll.ReportID);
					//var arrHideTypes = Enumerable.From(srpt.arrChartTypes)
					//			.Where(function (ct) { return (ct.Checked == false) })
					//			.Select(function (x) { return x; }).ToArray();
					//if (arrHideTypes != null && arrHideTypes != undefined && arrHideTypes.length > 0) {
					//	arrHideTypes.forEach(function (item) {
					//		item.Checked = true;
					//	}); //foreach
					//} //if


					//RefreshChartDSByAll(false);
				});

				$scope.updateChartTypes = function (chItem) {
					UpdateTrafficOpts();
				}
			}
		});
})();
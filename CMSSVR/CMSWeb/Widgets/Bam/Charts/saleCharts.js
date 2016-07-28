(function () {
	'use strict';
	define(['cms', 'Scripts/Services/chartSvc'],
		function (cms) {
			cms.register.controller('saleRptChartsCtrl', saleRptChartsCtrl);
			saleRptChartsCtrl.$inject = ['$scope', 'cmsBase', 'Utils', 'AppDefine', 'chartSvc', '$filter'];
			function saleRptChartsCtrl($scope, cmsBase, utils, AppDefine, chartSvc, $filter) {
				//var vm = this;
				$scope.ChartDataParamAll = null;

				$scope.arrChartTypes = [];
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

				var arrChartRegionColors = ["#0075c2", "#1aaf5d", "#f2c500", "#e44a00", "#6baa01", "#FFD700", "#0000FF", "#eb605b", "#efad4d", "#17b374"];

				var STR_POS = cmsBase.translateSvc.getTranslate(AppDefine.Resx.POS);
				var STR_TRAFFIC = cmsBase.translateSvc.getTranslate(AppDefine.Resx.TRAFFIC);
				var STR_DWELL = cmsBase.translateSvc.getTranslate(AppDefine.Resx.DWELL);
				var STR_COUNT = cmsBase.translateSvc.getTranslate(AppDefine.Resx.COUNT);
				var STR_COUNT_IN = cmsBase.translateSvc.getTranslate(AppDefine.Resx.COUNT_IN);
				var STR_COUNT_OUT = cmsBase.translateSvc.getTranslate(AppDefine.Resx.COUNT_OUT);
				var STR_CONVERSION = cmsBase.translateSvc.getTranslate(AppDefine.Resx.CONVERSION);
				var STR_CONV = cmsBase.translateSvc.getTranslate(AppDefine.Resx.CONV);

			    /******************* Chart Data - Begin *******************/
				$scope.chartDataMessage = {
				    dataEmptyMessage: cmsBase.translateSvc.getTranslate(AppDefine.Resx.NO_CHART_DATA_MSG)
				};
				$scope.chartDataDSMulti = {
					chart: {
						//xAxisname: "Month",
						pYAxisName: "Traffic",
						sYAxisName: "POS",
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
						showLegend: "0",

						borderAlpha: "4",
						plotSpacePercent: "60",
						toolTipBgColor: "#434955",
						toolTipBorderColor: "#434955"
					},
					categories: [],
					dataset: []
				};

				function UpdatePYAxisName() {
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

				function CreateDSGroup(grpData, parentname, dsRegions, rptType, color, catSub) {
					var icount = 0;
					var icolor = 0;
					var regLen = grpData.length;

					var dsCount = {};
					dsCount.seriesname = parentname + ":" + STR_COUNT;
					dsCount.showValues = "0";
					dsCount.color = color;
					dsCount.renderAs = "line";//"Column";
					dsCount.parentYAxis = "S";
					dsCount.data = [];
					dsCount.Type = AppDefine.BamDataTypes.COUNT;
					dsCount.ID = grpData[0].ID;

					var dsDWell = {};
					dsDWell.seriesname = parentname + ":" + STR_DWELL;
					dsDWell.showValues = "0";
					dsDWell.renderAs = "Column";//"line";
					dsDWell.parentYAxis = "P";
					dsDWell.color = color;
					dsDWell.data = [];
					dsDWell.Type = AppDefine.BamDataTypes.DWELL;
					dsDWell.ID = grpData[0].ID;

					var dsReg = [];
					for (var j = 0; j < regLen; j++) {

						var itCount = {};
						itCount.ID = grpData[j].ID;
						itCount.value = grpData[j].Count;
						itCount.tooltext = chartSvc.CreateBAMChartTooltip(grpData[j], parentname, AppDefine.BamDataTypes.COUNT, catSub[j].label);//tooltip;
						dsCount.data.push(itCount);

						var itDw = {};
						itDw.ID = grpData[j].ID;
						itDw.value = grpData[j].Dwell;
						itDw.tooltext = chartSvc.CreateBAMChartTooltip(grpData[j], parentname, AppDefine.BamDataTypes.DWELL, catSub[j].label);//tooltip;
						dsDWell.data.push(itDw);
					}

					dsRegions.push(dsCount);
					dsRegions.push(dsDWell);
				}
				function CreateChartData(resdata, rptType) {
					if (!resdata || (!resdata.Regions && !resdata.Sites)) {
						$scope.chartDataDSMulti.dataset = null;
						$scope.arrChartLegends = null;
						return;
					}

					UpdatePYAxisName();

					var catArray = [];
					var dsArray = [];
					var catSub = [];

					//var dsDataAlls = ChartDataSetAll(null, rptType, 0);
					var dsRegions = new Array();
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

								CreateDSGroup(resdata.Regions[i].Details, resdata.Regions[i].Name, dsRegions, rptType, arrChartRegionColors[icolor], catSub);
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
								CreateDSGroup(resdata.Sites[i].Details, resdata.Sites[i].Name, dsSites, rptType, arrChartRegionColors[icolor], catSubSite);
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

					console.log(dsArray);

					$scope.chartDataDSMulti.categories = catArray;

					$scope.AllChartDataSet = dsArray;
					$scope.AllChartDataSetSites = dsArraySite;

					if ($scope.ShowChartSites) {
						$scope.DSItemNumber = $scope.DSItemSite;
						$scope.chartDataDSMulti.dataset = angular.copy($scope.AllChartDataSetSites);

						$scope.arrChartLegends = $scope.SiteLegends;
					}
					else {
						$scope.DSItemNumber = $scope.DSItemRegion;
						$scope.chartDataDSMulti.dataset = angular.copy($scope.AllChartDataSet);

						$scope.arrChartLegends = $scope.RegionLegends;
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

				function CreateDSItem(item, data, color) {
					var itChart = {};
					if (item.Type == AppDefine.BamDataTypes.CONVERSION) {
						itChart.value = data.Conversion.toFixed(2);
						if(color != null)
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
					itChart.tooltext = CreateTooltip(data);

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
						dsCount.renderAs = "line";//"Column";//
						dsCount.parentYAxis = "P";
						dsCount.data = [];
						arrDataSets.push(dsCount);

						var dsDWell = {};
						dsDWell.ID = sid,
						dsDWell.Type = AppDefine.BamDataTypes.DWELL;
						dsDWell.seriesname = (sname != null) ? (sname + ' ' + STR_DWELL) : STR_DWELL;
						dsDWell.showValues = "0";
						dsDWell.renderAs = "Column";//"line";
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
						dsCInAll.seriesname = (sname != null) ? (sname + ' ' + STR_COUNT_IN) : STR_COUNT_IN;//cmsBase.translateSvc.getTranslate(AppDefine.Resx.CONVERSION);
						dsCInAll.showValues = "0";
						dsCInAll.renderAs = "line";
						dsCInAll.data = [];
						arrDataSets.push(dsCInAll);

						var dsCOutAll = {};
						dsCOutAll.ID = sid,
						dsCOutAll.Type = AppDefine.BamDataTypes.TRAFFIC_OUT;
						dsCOutAll.seriesname = (sname != null) ? (sname + ' ' + STR_COUNT_OUT) : STR_COUNT_OUT;//cmsBase.translateSvc.getTranslate(AppDefine.Resx.CONVERSION);
						dsCOutAll.showValues = "0";
						dsCOutAll.renderAs = "line";
						dsCOutAll.data = [];
						arrDataSets.push(dsCOutAll);
					}

					return arrDataSets;
				}
				//function CreateCategoriesLabel(data) {
				//	var label = '';
				//	switch ($scope.rptType) {
				//		case AppDefine.SaleReportTypes.Hourly:
				//			label = chartSvc.formatHourChart(data.TimeIndex);
				//			break;
				//		case AppDefine.SaleReportTypes.Weekly:
				//			label = cmsBase.translateSvc.getTranslate(AppDefine.Resx.WEEK_STRING) + data.TimeIndex;
				//			break;
				//		case AppDefine.SaleReportTypes.Monthly:
				//			label = cmsBase.translateSvc.getTranslate(AppDefine.Resx.PERIOD_STRING) + data.TimeIndex;
				//			break;
				//		default:
				//			label = $filter('date')(new Date(data.Date), AppDefine.CalDateFormat);
				//			break;
				//	}
				//	return label;
				//}
				
				/******************* Chart Data - End   *******************/
				$scope.Init = function () {
					//$scope.testInfo = $scope.$parent.showInfo;
				}

				$scope.$on(AppDefine.Events.CHARTDATALOADED, function (e, data) {
					if (!data) { return; }

					$scope.ChartDataParamAll = data;
					$scope.ShowChartTypes[0] = true;
					$scope.ShowChartTypes[1] = true;
					$scope.DSItemIndex = 0;
					$scope.isLastPage = false;

					if (data.ChartTypes) {
						$scope.arrChartTypes = angular.copy(data.ChartTypes);
					}
					$scope.rptType = data.rptType;

					CreateChartData(data.ChartData, data.ReportID);
					$scope.ChartLoaded = true;

					if (data.ReportID == AppDefine.BamReportTypes.SALE) {
						$scope.ChartToolbars = true;
						$scope.chartDataDSMulti.chart.pYAxisName = STR_TRAFFIC;
						$scope.chartDataDSMulti.chart.sYAxisName = STR_POS;
					}
					else {
						//if (data.ReportID == AppDefine.BamReportTypes.DRIVETHROUGH) {
						$scope.ChartToolbars = true;
						//}
						$scope.chartDataDSMulti.chart.pYAxisName = STR_DWELL;
						$scope.chartDataDSMulti.chart.sYAxisName = STR_COUNT;
					}
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
							RefreshChartDS();

							if (($scope.DSItemIndex + $scope.DSItemLimit) >= $scope.DSItemNumber)
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
						RefreshChartDS();
					} //back
				}

				function RefreshChartDS() {
					//RefreshChartDSByID(-1, true);
					var allDatasets = [];
					if ($scope.ShowChartSites) {
						allDatasets = $scope.AllChartDataSetSites;
						$scope.DSItemNumber = $scope.DSItemSite;
						$scope.arrChartLegends = $scope.SiteLegends;
					}
					else {
						allDatasets = $scope.AllChartDataSet;
						$scope.DSItemNumber = $scope.DSItemRegion;
						$scope.arrChartLegends = $scope.RegionLegends;
					}

					var dataset = null;
					var allDatas = [];//angular.copy($scope.AllChartDataSet);

					//Reset to show-all
					dataset = Enumerable.From(allDatasets)
							  .Where(function (ds) { return (ds.visible == '0') })
							  .Select(function (x) { return x; }).ToArray(); //.FirstOrDefault();

					if (dataset != null && dataset != undefined && dataset.length > 0) {
						dataset.forEach(function (item) {
							item.visible = '1';
						}); //foreach
					} //if

					//Hide by Types
					var arrHideTypes = [];
					if ($scope.ShowChartTypes[0] === false) {
						arrHideTypes.push(AppDefine.BamDataTypes.COUNT);
					}
					if ($scope.ShowChartTypes[1] === false) {
						arrHideTypes.push(AppDefine.BamDataTypes.DWELL);
					}

					dataset = Enumerable.From(allDatasets)
							  .Where(function (ds) { return arrHideTypes.indexOf(ds.Type) >= 0 })
							  .Select(function (x) { return x; }).ToArray(); //.FirstOrDefault();

					if (dataset != null && dataset != undefined && dataset.length > 0) {
						dataset.forEach(function (item) {
							item.visible = '0';
						}); //foreach
					} //if

					//Hide by Legends
					var arrHideIDs = Enumerable.From($scope.arrChartLegends)
						.Where(function (ds) { return (ds.Display == false) })
						.Select(function (x) { return x.ID; }).ToArray();

					dataset = Enumerable.From(allDatasets)
							.Where(function (ds) { return arrHideIDs.indexOf(ds.ID) >= 0 })//(ds.ID == dsID)
							.Select(function (x) { return x; }).ToArray(); //.FirstOrDefault();

					if (dataset != null && dataset != undefined && dataset.length > 0) {
						dataset.forEach(function (item) {
							item.visible = '0';//(siteShow && item.visible == '1') ? '1' : '0';
						}); //foreach
					} //if
					

					allDatasets.forEach(function (item) {
						if (item.visible == undefined || item.visible != '0') {
							allDatas.push(item);
						}
					}); //foreach

					$scope.chartDataDSMulti.dataset = allDatas;
				}

				//function RefreshChartDSByID(sid, isShow) {
				//	if ($scope.arrChartTypes != null && $scope.arrChartTypes != undefined && $scope.arrChartTypes.length > 0) {
				//		$scope.arrChartTypes.forEach(function (item) {
				//			if (item.Checked === false) {
				//				ShowChartItem(item.Key, false, false, sid, isShow);
				//			}
				//			else {
				//				ShowChartItem(item.Key, true, false, sid, isShow);
				//			}
				//		}); //foreach
				//	}
				//	else {
				//		if (sid != null && sid > 0) {
				//			ShowChartItem(null, true, false, sid, isShow)
				//		}
				//		else {
				//			ShowChartItem(null, true, true, sid, true)
				//		}
				//	}
				//}
				//function ShowChartItem(chType, isShow, showAll, dsID, siteShow) {
				//	//$scope.chartDataDSMulti.dataset
				//	//var dsType = GetDataSetType(chType);
				//	var allDatasets = [];
				//	if($scope.ShowChartSites) {
				//		allDatasets = $scope.AllChartDataSetSites;
				//		$scope.DSItemNumber = $scope.DSItemSite;
				//		$scope.arrChartLegends = $scope.SiteLegends;
				//	}
				//	else {
				//		allDatasets = $scope.AllChartDataSet;
				//		$scope.DSItemNumber = $scope.DSItemRegion;
				//		$scope.arrChartLegends = $scope.RegionLegends;
				//	}
				//	//$scope.DSItemNumber = allDatasets.length;
				//	if (showAll) {
				//		$scope.chartDataDSMulti.dataset = allDatasets;
				//	}
				//	else {
				//		var dataset = null;
				//		var allDatas = [];//angular.copy($scope.AllChartDataSet);
				//		if (chType != null) {
				//			dataset = Enumerable.From(allDatasets)
				//				  .Where(function (ds) { return (ds.Type == chType) })
				//				  .Select(function (x) { return x; }).ToArray(); //.FirstOrDefault();

				//			if (dataset != null && dataset != undefined && dataset.length > 0) {
				//				dataset.forEach(function (item) {
				//					item.visible = isShow ? '1' : '0';
				//				}); //foreach
				//			} //if
				//		}
				//		else {
				//			dataset = Enumerable.From(allDatasets)
				//				  .Where(function (ds) { return (ds.visible == '0') })
				//				  .Select(function (x) { return x; }).ToArray(); //.FirstOrDefault();

				//			if (dataset != null && dataset != undefined && dataset.length > 0) {
				//				dataset.forEach(function (item) {
				//					item.visible = '1';
				//				}); //foreach
				//			} //if
				//		}
				//		if (dsID != null && dsID >= 0) {
				//			var arrHideIDs = Enumerable.From($scope.arrChartLegends)
				//				.Where(function (ds) { return (ds.Display == false) })
				//				.Select(function (x) { return x.ID; }).ToArray();

				//			dataset = Enumerable.From(allDatasets)
				//				  .Where(function (ds) { return arrHideIDs.indexOf(ds.ID) >= 0 })//(ds.ID == dsID)
				//				  .Select(function (x) { return x; }).ToArray(); //.FirstOrDefault();

				//			if (dataset != null && dataset != undefined && dataset.length > 0) {
				//				dataset.forEach(function (item) {
				//					item.visible = '0';//(siteShow && item.visible == '1') ? '1' : '0';
				//				}); //foreach
				//			} //if
				//		}

				//		allDatasets.forEach(function (item) {
				//			if (item.visible == undefined || item.visible != '0') {
				//				allDatas.push(item);
				//			}
				//		}); //foreach

				//		$scope.chartDataDSMulti.dataset = allDatas;
				//	}
				//}

				function CreateTooltip(data) {
					// Conversion CountTrans TrafficIn TrafficOut Count Dwell
					var strTooltip = '<div class="bam_tooltip"><ul>';//'<div class="bam_tooltip"><ul><li>sdsdsds</li><li>sdsdsds</li><li>sdsdsds</li><li> <span class="green bold">sd</span>sdsds</li></ul></div>';
					if (data.hasOwnProperty('Name')) {
						strTooltip += '<li><span class="yellow bold">' + data.Name + '</span></li><li><span> &nbsp;</span></li>';

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
				/*function CreateTooltipWithType(data, pname, type, label) {
					// Conversion CountTrans TrafficIn TrafficOut Count Dwell
					var strTooltip = '<div class="bam_tooltip"><ul>';//'<div class="bam_tooltip"><ul><li>sdsdsds</li><li>sdsdsds</li><li>sdsdsds</li><li> <span class="green bold">sd</span>sdsds</li></ul></div>';
					if (data.hasOwnProperty('Name')) {
						strTooltip += '<li><span class="yellow bold">' + pname + ' - ' + label + '</span></li><li><span> &nbsp;</span></li>';
					}
					else {
						strTooltip += '<li><span class="yellow bold">' + pname + '</span></li><li><span> &nbsp;</span></li>';
					}
					if (data.hasOwnProperty('Conversion') && type == AppDefine.BamDataTypes.CONVERSION) {
						strTooltip += '<li>Conversion: <span class="green bold">' + data.Conversion.toFixed(2) + ' %</span></li>';
					}
					if (data.hasOwnProperty('CountTrans') && type == AppDefine.BamDataTypes.POS) {
						strTooltip += '<li>POS: <span class="green bold">' + data.CountTrans + '</span></li>';
					}
					if (data.hasOwnProperty('TrafficIn') && type == AppDefine.BamDataTypes.TRAFFIC_IN) {
						strTooltip += '<li>Count In: <span class="green bold">' + data.TrafficIn + '</span></li>';
					}
					if (data.hasOwnProperty('TrafficOut') && type == AppDefine.BamDataTypes.TRAFFIC_OUT) {
						strTooltip += '<li>Count Out: <span class="green bold">' + data.TrafficOut + '</span></li>';
					}
					if (data.hasOwnProperty('Count') && type == AppDefine.BamDataTypes.COUNT) {
						strTooltip += '<li>Count: <span class="green bold">' + data.Count + '</span></li>';
					}
					if (data.hasOwnProperty('Dwell') && type == AppDefine.BamDataTypes.DWELL) {
						strTooltip += '<li>Dwell: <span class="green bold">' + data.Dwell + '</span></li>';
					}
					strTooltip += '</ul></div>';
					return strTooltip;
				}*/

				$scope.ShowHideChartType = function (type) {
					//if (type == 0) {
					//	ShowChartItem(AppDefine.BamDataTypes.COUNT, $scope.ShowChartTypes[0], false, -1, true);//ShowChartTypes
					//}
					//else {
					//	ShowChartItem(AppDefine.BamDataTypes.DWELL, $scope.ShowChartTypes[1], false, -1, true);
					//}
					UpdatePYAxisName();
					RefreshChartDS();
				}
				$scope.ShowHideChart = function (isP) {
					var popupID = isP ? "#btn-popMenuAlert_ChartP" : "#btn-popMenuAlert_ChartS";
					if ($(popupID).parent().hasClass("open")) {
						$(popupID).parent().removeClass("open");
						$(popupID).prop("aria-expanded", false);
					}
					RefreshChartDS();
				}
				//$scope.showChartSites = function (isSite) {
				//	$scope.ShowChartSites = isSite;
				//	$scope.DSItemIndex = 0;

				//	CreateChartData($scope.ChartDataParamAll.ChartData, $scope.ChartDataParamAll.ReportID);
				//	RefreshChartDS();
				//}
				$scope.Next = function () {
					ShowNextBackChart(true);
				}
				$scope.Back = function () {
					ShowNextBackChart(false);
				}

				$scope.clickOnLegend = function (id) {
					//alert(id);
					var dataset = Enumerable.From($scope.arrChartLegends)
								  .Where(function (ds) { return (ds.ID == id) })
								  .Select(function (x) { return x; }).FirstOrDefault();
					if (dataset != null && dataset != undefined) {
						dataset.Display = !dataset.Display;
						//RefreshChartDSByID(id, dataset.Display);
						RefreshChartDS();
					}
				}

				$scope.$parent.$watch('vm.showSiteChart', function (value) {
					//alert(value);
					if ($scope.ChartDataParamAll == null || $scope.ChartDataParamAll == undefined) {
						return;
					}
					$scope.ShowChartSites = value;
					$scope.DSItemIndex = 0;

					CreateChartData($scope.ChartDataParamAll.ChartData, $scope.ChartDataParamAll.ReportID);
					RefreshChartDS();
				});
			}
		});
})();
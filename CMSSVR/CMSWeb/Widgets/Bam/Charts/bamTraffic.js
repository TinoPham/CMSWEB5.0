(function () {
	'use strict';
	define(['cms', 'Scripts/Services/chartSvc'],
		function (cms) {
			cms.register.controller('bamTrafficCtrl', bamTrafficCtrl);
			bamTrafficCtrl.$inject = ['$scope', 'cmsBase', 'Utils', 'AppDefine', 'chartSvc', '$filter', '$timeout'];
			function bamTrafficCtrl($scope, cmsBase, utils, AppDefine, chartSvc, $filter, $timeout) {

				var fiscalToWeek = [];

				$scope.$on(AppDefine.Events.CHARTDATALOADED, function (e, data) {
					$scope.$parent.isLoading = false;
					if (data != null && data.DshChartDatas != null) {
						//fiscalToWeek = data.fiscalToWeek;
						//$scope.ToSelDate = fiscalToWeek.EndDate;
						//$scope.FromSelDate = fiscalToWeek.StartDate;
						//GetTrafficReport();
						var traffData = data.DshChartDatas.TrafficChart;
						CreateChartDataTraff(traffData);
						sessionStorage.BamTraffic = JSON.stringify(traffData); //use to export
						$scope.loading = true;
					}
				});
				/***********************************************************************/
				/*
				$scope.Pram = { "cmp": "Traffic", "value": null, "act": 0 };
				function GetTrafficReport() {
					var opt = {};
					//angular.copy(optVal, opt);
					//UpdateStartEnd(opt);
					opt.cmp = "Traffic";
					opt.date = $filter('date')($scope.ToSelDate, AppDefine.DateFormatCParamED);
					var delta = chartSvc.dateDiff($scope.ToSelDate, $scope.FromSelDate);
					utils.RemoveProperty(opt, "week");
					utils.RemoveProperty(opt, "month");
					utils.RemoveProperty(opt, "hour");
					opt.day = delta;

					var pram = angular.copy($scope.Pram);
					utils.RemoveProperty(pram, "act");
					if (opt) {
						var Props = Object.getOwnPropertyNames(opt);
						for (var i = 0; i < Props.length; i++) {
							var propName = Props[i];
							if (!propName.startsWith("$$") && !propName.startsWith("label"))
								utils.AddProperty(pram, propName, opt[propName]);
						}
					}

					rptService.DshTraffic(pram, RptSuccess, RptError);
				};

				function RptSuccess(response) {
					$scope.$parent.isLoading = false;
					if (response === null || response === undefined)
						return;
					CreateChartDataTraff(response);
					sessionStorage.BamTraffic = JSON.stringify(response); //use to export
					$scope.loading = true;
				}

				function RptError(response) {

				}
				*/
				$scope.chartDataMessageTraffic = {
					dataEmptyMessage: cmsBase.translateSvc.getTranslate(AppDefine.Resx.NO_CHART_DATA_MSG)
				};
				$scope.chartDSTraffic = {
					chart: {
						//caption: "Traffic Statistic",
						linethickness: "1",
						showvalues: "0",
						numvdivlines: "22",
						formatnumberscale: "1",
						labeldisplay: "ROTATE",
						slantlabels: "1",
						yAxisValuesPadding: "15",
						anchorradius: "4",
						anchorbgalpha: "50",
						anchoralpha: "100",
						showalternatevgridcolor: "0",
						showalternatehgridcolor: "0",
						animation: "1",
						legendshadow: "1",
						legendborderalpha: "30",
						//bgcolor: "ffffff",
						divlineisdashed: "1",
						divlinedashlen: "2",
						divlinedashgap: "4",
						//canvasborderthickness: "1",
						//canvasborderalpha: "30",
						legendPosition: "right",
						legendIconScale: "1",
						//showBorder: "1",
						showToolTip: $scope.showToolTip,
						toolTipBgColor: "#434955",
						toolTipBgAlpha: '0',
						//toolTipBorderColor: '#000000',
						//plottooltext: "<div class='chart-tooltip-nb'><div class='tooltipHeader'>$label</div><div class='tooltipContent'>$seriesname: $value</div></div>",
						exportEnabled: '1',
						exportAtClient: '0',
						exportHandler: AppDefine.ChartExportURL,
						exportAction: 'download',
						exportShowMenuItem: '0',
						theme: "fint"
					},
					categories: [],
					dataset: []
				};
				$scope.chartBamTrafficEvents = {
					legendItemClicked: function (eventObj, dataObj) {
						var lengendName = dataObj.datasetName.replace(/[\s]/g, '');
						$scope.$parent.$parent.legendTraffic[lengendName] = !dataObj.visible;
					}
				};

				function CreateChartDataTraff(resdata) {
					if (resdata === null || resdata === undefined)
						return;
					var datLen = resdata.length;
					var catArray = [];
					var dsArray = [];
					var toolTipArray = [];

					var catSub = [];

					var dsItemIn = {};
					dsItemIn.seriesname = cmsBase.translateSvc.getTranslate(AppDefine.Resx.COUNT_IN);//"Count In";
					dsItemIn.color = AppDefine.ChartColor.Blue;
					dsItemIn.anchorbordercolor = AppDefine.ChartColor.Blue;
					//dsItemIn.anchorBgColor = AppDefine.ChartColor.Blue;
					dsItemIn.data = [];

					var dsItemOut = {};
					dsItemOut.seriesname = cmsBase.translateSvc.getTranslate(AppDefine.Resx.COUNT_OUT);//"Count Out";
					dsItemOut.color = AppDefine.ChartColor.Yellow;//ColorRed;
					dsItemOut.anchorbordercolor = AppDefine.ChartColor.Yellow;//ColorRed;
					//dsItemOut.anchorBgColor = AppDefine.ChartColor.Yellow;
					dsItemOut.data = [];

					var dsItemForcast = {};
					dsItemForcast.seriesname = cmsBase.translateSvc.getTranslate(AppDefine.Resx.FORECAST);//"Forecast";
					dsItemForcast.color = AppDefine.ChartColor.Green;//ColorRed;
					dsItemForcast.anchorbordercolor = AppDefine.ChartColor.Green;//ColorRed;
					//dsItemForcast.anchorBgColor = AppDefine.ChartColor.Green;
					dsItemForcast.data = [];

					//data temp, hard code to test, begin
					//var channelList = [];
					//for (var num = 1; num <= 6; num++) {
					//	var name = 'Channel ' + num;
					//	channelList.push({
					//		KDVR: 40,
					//		KChannel: null,
					//		DVR_ChannelNo: num,
					//		Name: name,
					//		PAC_ChannelID: null,
					//		PAC_ChannelName: null,
					//		Dim_PACID: null,
					//		EnableTrafficCount: true
					//	});
					//}
					//data temp, hard code to test, end

					for (var i = 0; i < datLen; i++) {
						var catIt = {};
						catIt.label = resdata[i].Label;
						catSub.push(catIt);

						var dsIt1 = {};
						dsIt1.value = resdata[i].countIn;
						dsItemIn.data.push(dsIt1);

						var dsIt2 = {};
						dsIt2.value = resdata[i].countOut;
						dsItemOut.data.push(dsIt2);

						var dsIt3 = {};
						dsIt3.value = resdata[i].forecast;
						dsItemForcast.data.push(dsIt3);

						//data temp, hard code to test, begin
						//if ([5,9,15,20,8].indexOf(i) > 0) {
						//	resdata[i].Channels = channelList;
						//}

						var toolTipData = {
							time: catIt.label,
							countInLabel: dsItemIn.seriesname,
							countInValue: dsIt1.value,
							countOutLabel: dsItemOut.seriesname,
							countOutValue: dsIt2.value,
							forcastLabel: dsItemForcast.seriesname,
							forcastValue: dsIt3.value,
							channels: resdata[i].Channels
						};

						toolTipArray.push(toolTipData);

						//data temp, hard code to test, end

						//var tooltip = "<div class='chart-tooltip-nb'><div class='tooltipHeader'><strong>" + catIt.label + "</strong></div><div class='tooltipContent'>" + dsItemIn.seriesname + ": " + dsIt1.value + "</div><div class='tooltipContent'>" + dsItemOut.seriesname + ": " + dsIt2.value + "</div><div class='tooltipContent'>" + dsItemForcast.seriesname + ": " + dsIt3.value + "</div></div>";

						var tooltip = BuildToolTip(toolTipData);
						dsItemIn.data[i].tooltext = tooltip;
						dsItemOut.data[i].tooltext = tooltip;
						dsItemForcast.data[i].tooltext = tooltip;

						dsItemIn.data[i].anchorbgalpha = "100";
						dsItemOut.data[i].anchorbgalpha = "100";
						dsItemForcast.data[i].anchorbgalpha = "100";

						dsItemIn.data[i].anchorBorderThickness = "2";
						dsItemOut.data[i].anchorBorderThickness = "2";
						dsItemForcast.data[i].anchorBorderThickness = "2";
					}
					var catCol = {};
					catCol.category = catSub;
					catArray.push(catCol);

					dsArray.push(dsItemIn);
					dsArray.push(dsItemOut);
					dsArray.push(dsItemForcast);

					$scope.chartDSTraffic.chartName = "TRAFFICSTATISTIC";
					$scope.chartDSTraffic.categories = catArray;
					$scope.chartDSTraffic.dataset = dsArray;
					$scope.chartDSTraffic.toolTipData = toolTipArray;

				}

				function BuildToolTip(data) {
					var htmlSlider = "<div class=\"chart-tooltip\">" +
							"<div class=\"tooltipHeader\"><strong>" + data.time + "</strong></div>" +
							"<div class=\"tooltipContent\">" + data.countOutLabel + ": " + data.countOutValue + "</div>" +
							"<div class=\"tooltipContent\">" + data.countInLabel + ": " + data.countInValue + "</div>" +
							"<div class=\"tooltipContent\">" + data.forcastLabel + ": " + data.forcastValue + "</div>" +
						"</div>";
					//if (data.channels) {
					//	$scope.data = data;
					//	var apiURL = "/api/cmsweb/site/GetImageChannel?";
					//	var imageString = "";
					//	imageString += "<div class=\"image-slider\">";
					//	for(var c=0; c < data.channels.length; c++){
					//		var channel = data.channels[c];
					//		var url = apiURL + "name=C_" + pad(channel.DVR_ChannelNo) + ".jpg&kdvr=" + channel.KDVR;
					//		imageString += "<img src=\"" + url + "\" width=\"100\" height=\"70\" />";
					//	}
					//	imageString += "</div>";
					//	htmlSlider = "<div class=\"chart-tooltip-nb\">" + imageString +
					//			"<div class=\"tooltipHeader\"><strong>" + data.time + "</strong></div>" +
					//			"<div class=\"tooltipContent\">" + data.countOutLabel + ": " + data.countOutValue + "</div>" +
					//			"<div class=\"tooltipContent\">" + data.countInLabel + ": " + data.countInValue + "</div>" +
					//			"<div class=\"tooltipContent\">" + data.forcastLabel + ": " + data.forcastValue + "</div>" +
					//		"</div>";
					//}
					return htmlSlider;
				}

				function pad(number) {
					return (number < 10 ? '0' : '') + number.toString();
				}
				/************************************************************************/
			}
		});
})();
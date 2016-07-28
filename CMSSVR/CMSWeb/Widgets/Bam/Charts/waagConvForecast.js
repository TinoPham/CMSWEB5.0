(function () {
	'use strict';
	define(['cms'],
		function (cms) {
			cms.register.controller('waagConvForecastCtrl', waagConvForecastCtrl);
			waagConvForecastCtrl.$inject = ['$scope', 'cmsBase', 'AppDefine'];
			function waagConvForecastCtrl($scope, cmsBase, AppDefine) {
				//var vm = this;

				//************************ CHART DATA - BEGIN **************************//
				$scope.chartDataDS = {
					chart: {
						showBorder: "0",
						showValues: "0",
						theme: "fint",
						bgColor: "#ffffff",
						showCanvasBorder: "0",
						canvasBgColor: "#ffffff",
						captionFontSize: "14",
						subcaptionFontSize: "14",
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
						toolTipBorderThickness: "0",
						toolTipBorderRadius: "0",
						toolTipBgColor: "#FFFFFF",
						toolTipBgAlpha: '0',
						plottooltext: "<div class='chart-tooltip'><div class='tooltipHeader'><strong>$label</strong></div><div class='tooltipContent'>$seriesname: $value %</div></div>",
						legendBgColor: "#ffffff",
						legendBorderAlpha: "0",
						legendShadow: "0",
						legendItemFontSize: "10",
						legendItemFontColor: "#666666",
						canvasborderthickness: "1",
						exportEnabled: '1',
						exportAtClient: '0',
						exportHandler: AppDefine.ChartExportURL,
						exportAction: 'download',
						exportShowMenuItem: '0',
						numberSuffix: "%",
					},
					categories: [],
					dataset: []
				};
				//************************ CHART DATA - END ****************************//
				$scope.Init = function () {
				}

				$scope.$on(AppDefine.Events.CHARTDATALOADED, function (e, data) {
					if (data == null || data == undefined)
						return;
					$scope.chartDataDS.categories = [];
					$scope.chartDataDS.dataset = [];
					CreateChartData(data.DataChartSumamry);
				});

				function CreateChartData(data) {
					if (data == null || data == undefined || data.length == 0)
						return;
					var dsCats = {};
					dsCats.category = [];

					var dsConv = {};
					dsConv.seriesName = "Conversion";
					dsConv.showValues = 1;
					dsConv.data = [];

					var dsForecast = {};
					dsForecast.seriesName = cmsBase.translateSvc.getTranslate(AppDefine.Resx.FORECAST);
					dsForecast.showValues = 0;
					dsForecast.renderAs = "area";
					dsForecast.color = "#ccffff";
					dsForecast.anchorbordercolor = AppDefine.ChartColor.Yellow,
					dsForecast.plotFillAlpha = 10;
					dsForecast.numvdivlines = 50;
					dsForecast.vdivlinealpha =70;
					dsForecast.data = [];

					data.forEach(function (item) {
						var catIt = {};
						catIt.label = item.Label;
						dsCats.category.push(catIt);

						var cIt = {};
						cIt.value = item.Conversion.toFixed(2);
						dsConv.data.push(cIt);

						var fIt = {};
						fIt.value = item.ConvForecast.toFixed(2);
						dsForecast.data.push(fIt);
					});

					$scope.chartDataDS.categories.push(dsCats);

					$scope.chartDataDS.dataset.push(dsConv);
					$scope.chartDataDS.dataset.push(dsForecast);
				}
			}
		});
})();
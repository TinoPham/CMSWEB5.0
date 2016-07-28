(function () {
	'use strict';
	define(['cms'],
		function (cms) {
			cms.register.controller('waagATVCtrl', waagATVCtrl);
			waagATVCtrl.$inject = ['$scope', 'AppDefine'];
			function waagATVCtrl($scope, AppDefine) {
				//var vm = this;
				//******************* CHART DATA **********************//
				$scope.chartATVDS = {
					chart: {
						//"caption": "Quarterly Revenue",
						//"subCaption": "Last year",
						//"xAxisName": "Quarter",
						//"yAxisName": "Amount (In USD)",
						numberPrefix: "$",
						canvasPadding: "0",
						theme: "fint",
						showLabels: "1",
						paletteColors: "#89a9c2",
						//plotFillAlpha: "50",
						plotBorderAlpha: "5",
						borderThickness: "1",
						showYAxisValues: "0",
						showValues: "0",
						chartLeftMargin: "23",
						chartRightMargin: "23",
						toolTipBgColor: "#FFFFFF",
						toolTipBorderColor: "#000000"//,
						//plottooltext: "<div class='chart-tooltip-nb'><div class='tooltipHeader'><strong>$label</strong></div><div class='tooltipContent'>ATV: $$value </div></div>",
					},
					//data: []
					categories: [],
					dataset: []
				};
				//******************* CHART DATA **********************//
				$scope.Init = function () {
					$scope.testInfo = $scope.$parent.showInfo;
				}
				$scope.ATVData = {
					value: 0,
					compare: '+0',
					icon: 'icon-down-circled-2'
				};
				$scope.$on(AppDefine.Events.CHARTDATALOADED, function (e, data) {
					if (data == null || data == undefined)
						return;
					if (data.AVTData != null) {
						$scope.ATVData.value = data.AVTData.Value;
						if (data.AVTData.Increase == null || data.AVTData.Increase == true) {
							$scope.ATVData.compare = '+' + data.AVTData.CmpValue;
							$scope.ATVData.icon = 'icon-up-circled-2';
						}
						else {
							$scope.ATVData.compare = '-' + data.AVTData.CmpValue;
							$scope.ATVData.icon = 'icon-down-circled-2';
						}
					}
					CreateChartData(data.DataChartSumamry);
				});

				function CreateChartData(data) {
					if (data == null || data == undefined || data.length == 0)
						return;
					var chData = [];

					var catArray = [];
					var catSub = [];

					var dsArray = [];

					var dsLine = {};
					dsLine.seriesname = '';
					dsLine.showValues = "1";
					dsLine.renderAs = "line";
					dsLine.color = '#426d8f';
					dsLine.plottooltext= "<div class='chart-tooltip-nb'><div class='tooltipHeader'><strong>$label</strong></div><div class='tooltipContent'>ATV: $$value </div></div>";
					dsLine.data = [];

					var dsArea = {};
					dsArea.seriesname = '';
					dsArea.showValues = "0";
					dsArea.renderAs = "area";
					dsArea.color = "#89a9c2";
					dsArea.plotFillAlpha = "50";
					dsArea.data = [];

					data.forEach(function (item) {
						var catIt = {};
						catIt.label = item.Label;
						catSub.push(catIt);

						var dIt = {};
						//dIt.label = item.Label;
						dIt.value = item.Avt.toFixed(2);
						//chData.push(dIt);
						dsArea.data.push(dIt);

						var dLine = {};
						//dIt.label = item.Label;
						dLine.value = item.Avt.toFixed(2);
						dsLine.data.push(dIt);
					});
					var catCol = {};
					catCol.category = catSub;
					catArray.push(catCol);

					dsArray.push(dsArea);
					dsArray.push(dsLine);

					//$scope.chartATVDS.data = chData;
					$scope.chartATVDS.categories = catArray;
					$scope.chartATVDS.dataset = dsArray;
				}

			}
		});
})();
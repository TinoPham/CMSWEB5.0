(function () {
	'use strict';
	define(['cms', 'widgets/bam/charts/jquery.circle-diagram'],
		function (cms) {
			cms.register.controller('waagConversionCtrl', waagConversionCtrl);
			waagConversionCtrl.$inject = ['$scope', 'cmsBase', 'AppDefine'];
			function waagConversionCtrl($scope, cmsBase, AppDefine) {
				//var vm = this;

				// $scope.circle_param = '{"percent": "30%","size": "160px","borderWidth": "17","bgFill": "#fecb32","frFill": "#89a8c4","textSize": "30","textColor": "#fff"}';
				
				$scope.Init = function () {
				}

				$scope.ConvData = {
					value: 0,
					compare: '+0',
					icon: 'icon-down-circled-2',
					traffic: 0,
					trans: 0
				};
				var pie_param = {
					percent: "0%",
					size: "160",
					borderWidth: "17",
					bgFill: "#666",
					frFill: "#fecb32",
					textSize: "30",
					textColor: "#fff"
				};
				$scope.ConvDataWeek = [];

				function DefaultWeek() {
					var wkData = [];
					for (var i = 0; i < 7; i++) {
						wkData.push(0);
					}
					$scope.ConvDataWeek = wkData;
				}
				$scope.$on(AppDefine.Events.CHARTDATALOADED, function (e, data) {
					if (data == null || data == undefined) {
						$('#conversion-pie-chart').html('');
						$('#conversion-pie-chart').circleDiagram(pie_param);
						DefaultWeek();
						return;
					}

					if (data.ConvData != null) {
						$scope.ConvData.value = data.ConvData.Value;
						$scope.ConvData.traffic = data.ConvData.Traffic;
						$scope.ConvData.trans = data.ConvData.Transaction;
						if (data.ConvData.Increase == null || data.ConvData.Increase == true) {
							$scope.ConvData.compare = '+' + data.ConvData.CmpValue;
							$scope.ConvData.icon = 'icon-up-circled-2';
						}
						else {
							$scope.ConvData.compare = '-' + data.ConvData.CmpValue;
							$scope.ConvData.icon = 'icon-down-circled-2';
						}
					}
					var convVal = $scope.ConvData.value.toFixed(2);
					pie_param.percent = convVal + "%";
					$('#conversion-pie-chart').html('');
					$('#conversion-pie-chart').circleDiagram(pie_param);

					CreateChartData(data.DataChartSumamry);
				});

				
				function CreateChartData(data) {
					if (data == null || data == undefined || data.length == 0) {
						DefaultWeek();
						return;
					}
					var wkData = [];
					data.forEach(function (item) {
						var itCnv = {};
						itCnv.label = item.Label;
						itCnv.conv = item.Conversion.toFixed(2);
						wkData.push(itCnv);
					});
					$scope.ConvDataWeek = wkData;
				}
			}
		});
})();
(function () {
	'use strict';
	define(['cms', 'DataServices/ReportService', 'Scripts/Services/chartSvc'],
		function (cms) {
			cms.register.controller('dvrCountCtrl', dvrCountCtrl);
			dvrCountCtrl.$inject = ['$scope', 'ReportService', 'Utils', 'chartSvc', 'AppDefine'];
			function dvrCountCtrl($scope, ReportService, utils, chartSvc, AppDefine) {
				//var vm = this;
				var Options = JSON.parse("{" + $scope.widgetModel.TemplateParams + "}");
				$scope.Gui = (Options == null || !Options.hasOwnProperty("Gui")) ? null : Options.Gui;
				$scope.Pram = (Options == null || !Options.hasOwnProperty("Pram")) ? null : Options.Pram;
				$scope.Opt = DefaultOption();

				$scope.loading = false;
				$scope.ChartDatas = [];

				$scope.chartDVREvents = angular.copy(chartSvc.createExportEvent('_dvrc'));

				$scope.isActiveOption = function (opt) {
					if ($scope.Opt == null)
						return false;
					var ret = utils.Equal(opt, $scope.Opt);
					return ret;
				}

				function DefaultOption() {
					if (!$scope.Pram || !$scope.Gui.Opts || $scope.Gui.length == 0 || !$scope.Pram.hasOwnProperty("act") || $scope.Pram.act === null)
						return null;

					var obj = $scope.Gui.Opts[$scope.Pram.act];
					return obj;
				}

				$scope.Init = function () {
					var opt = DefaultOption();
					GetReport($scope.Opt)
				}

				$scope.GetData = function (opt) {
					$scope.Opt = opt;
					GetReport($scope.Opt)
				}
				function GetReport(opt) {
					var pram = angular.copy($scope.Pram);
					utils.RemoveProperty(pram, "act");
					var Props = Object.getOwnPropertyNames(opt);
					for (var i = 0; i < Props.length; i++) {
						var propName = Props[i];
						if (!propName.startsWith("$$") && !propName.startsWith("label"))
							utils.AddProperty(pram, propName, opt[propName]);
					}

					rptService.DshAlerts(pram, RptSuccess, RptError);
				};
				function RptSuccess(response) {
					$scope.$parent.isLoading = false;
					if (response === null || response === undefined) {
						return;
					}

					$scope.filter = 1;
					CreateChartData(response);
					$scope.loading = true;
				}
				function RptError(response) {
				}

				function CreateChartData(resdata) {
					if (resdata === null || resdata === undefined)
						return;
					resdata = [{ label: 'Yemen Sushi', value: '9' }, { label: 'Yamaha', value: '27' }, { label: 'Wasakinura', value: '57' }, { label: 'Taoxamira', value: '31' }, { label: 'Mamadaye', value: '46' }, { label: 'Kala Somani', value: '19' }, { label: 'Nomawantashi', value: '32' }, { label: 'Kaka Lotical', value: '70' }];

					var convArray = resdata;//[];
					$scope.ChartDatas = convArray;

					$scope.chartDSDVRs = {
						chart: {
							//caption: AppDefine.Resx.CHART_DVRCOUNT,
							subCaption: "",
							//numberPrefix: "%",
							//numbersuffix: "%",
							captionAlignment: "center",
							//numDivLines: "6",
							theme: "fint",
							paletteColors: '#F85252',
							showBorder: "1",
							labelDisplay: "rotate",
							slantLabels: "1",
							toolTipBgColor: "#FFFFFF",
							toolTipBgAlpha: '0',
							plottooltext: "<div class='chart-tooltip'><div class='tooltipHeader'>$label</div><div class='tooltipContent'>DVRs: $value</div></div>",
							exportEnabled: '1',
							exportShowMenuItem: '0'
						},
						data: convArray
					};
				}
			}
		});
})();

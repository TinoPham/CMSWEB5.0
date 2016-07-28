(function () {
	'use strict';
	define(['cms', 'DataServices/ReportService', 'Scripts/Services/chartSvc'],
		function (cms) {
			cms.register.controller('dvrMostAlertCtrl', dvrMostAlertCtrl);
			dvrMostAlertCtrl.$inject = ['$scope', 'ReportService', "Utils", 'chartSvc', 'AppDefine'];
			function dvrMostAlertCtrl($scope, ReportService, utils, chartSvc, AppDefine) {
				//var vm = this;
				var Options = JSON.parse("{" + $scope.widgetModel.TemplateParams + "}");
				$scope.Gui = (Options == null || !Options.hasOwnProperty("Gui")) ? null : Options.Gui;
				$scope.Pram = (Options == null || !Options.hasOwnProperty("Pram")) ? null : Options.Pram;
				$scope.Opt = DefaultOption();

				$scope.loading = false;
				$scope.ChartDatas = [];

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

				$scope.chartMostAlertEvents = angular.copy(chartSvc.createExportEvent('_ma'));

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

				function GetColor(val) {
					var color = Enumerable.From(AppDefine.AlertCountRanges)
						.Where(function (i) { return (i.min <= val) && (i.max > val || i.max == -1) })
						.Select(function (x) { return x.color; })
						.First();
					return color;
				}
				function CreateChartData(resdata) {
					if (resdata === null || resdata === undefined)
						return;
					//var convArray = from c in resdata select c => {c.color = GetColor(x.value);  return c;};//resdata.Select(x => { x.color = GetColor(x.value); return x; });//[];
					//var convArray = from dat in resdata select dat.Set(dat1 => { dat1.color = GetColor(dat1.value); });
					var convArray = resdata.sort(function (a, b) { return parseInt(b.value) - parseInt(a.value) });;//Enumerable.From(resdata).OrderByDescending("$.value");
					var count = convArray.length;
					if (count > 10) {
						convArray.length = 10;
						count = 10;
					}
					for (var i = 0; i < count; i++) {
						convArray[i].color = GetColor(convArray[i].value);
					}
					$scope.ChartDatas = convArray;

					$scope.chartDSMostAlertDVRs = {
						chart: {
							//caption: "Most alerts DVRs",
							subCaption: "",
							captionAlignment: "center",
							//numDivLines: "6",
							theme: "fint",
							showBorder: "1",
							labelDisplay: "rotate",
							slantLabels: "1",
							toolTipBgColor: "#FFFFFF",
							toolTipBgAlpha: '0',
							plottooltext: "<div class='chart-tooltip'><div class='tooltipHeader'>$label</div><div class='tooltipContent'>Alerts: $value</div></div>",
							exportEnabled: '1',
							exportShowMenuItem: '0'
						},
						data: convArray
					};
				}
			}
		});
})();
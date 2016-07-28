(function() {
	'use strict';

	define(['cms'], function(cms) {
		cms.register.controller('swidgetCtrl', swidgetCtrl);

		swidgetCtrl.$inject = ['$scope', '$modalInstance', 'cmsBase', 'items', 'addNew', 'dataContext', 'AccountSvc', 'AppDefine', '$STORAGE_KEY'];

		function swidgetCtrl($scope, $modalInstance, cmsBase, items, addNew, dataContext, AccountSvc, AppDefine, $STORAGE_KEY) {
			var DEFAULT_IMG = 'chart1.png';
			var DASHBOARD_FLEXIBLE_ID = 4;
			var defChartImg = [{
				name: 'CHART_ALERT',
				image: 'chart1.png'
			},
			{
				name: 'CHART_DVRCOUNT',
				image: 'chart1.png'
			},
			{
				name: 'CHART_DVRMOSTALERT',
				image: 'chart2.png'
			},
			{
				name: 'CHART_TRAFFIC',
				image: 'chart5.png'
			},
			{
				name: 'CHART_CONVERSION',
				image: 'chart3.png'
			},
			{
				name: 'CHART_CONVERSION_USA_MAP',
				image: 'chart4.png'
			},
			{
				name: 'CHART_CONVERSION_SITES',
				image: 'chart4.png'
			}];
			var userLogin = AccountSvc.UserModel();

			$scope.dashboard = $scope.$root.dashboard;
			$scope.widgetExiteds = [];
			angular.forEach($scope.dashboard.Rows, function (row) {
				angular.forEach(row.Columns, function (col) {
					angular.forEach(col.Widgets, function (widget) {
						$scope.widgetExiteds.push(widget);
					});
				});
			});

			var w = $(window).width();
			if (w > 750) {
			    $scope.isOverFlow = true;
			} else {
			    $scope.isOverFlow = false;
			}

			if ($scope.dashboard.Id) {
				$scope.showAllWidgets = $scope.dashboard.Id === DASHBOARD_FLEXIBLE_ID;
			}

			function GetChartImage(cname) {
				var image = Enumerable.From(defChartImg)
						.Where(function (i) { return (i.name == cname) })
						.Select(function (x) { return x.image; }).ToArray();
				if (image != null && image.length > 0)
					return image[0];
				else
					return DEFAULT_IMG;
			}

			$scope.data = {
				RegionKey: 0,
				UserKey: 0,
				RegionName: "",
				RegionParentId: null,
				Description: ""
			};
			var smallWidget = 'SMALL';
			var fullWidget = 'FULL';
			$scope.widgetHeaders = [];
			$scope.widgetCharts = [];
			$scope.widgetExtras = [];

			active();

			function active() {
				dataContext.injectRepos(['dashboard']).then(getActiveData);
			}

			function getActiveData() {
				if (items) {
					$scope.isHeader = items.Group.Name === smallWidget ? true : false;
					dataContext.dashboard.getElements(function (result) {
						angular.forEach(result, function(r) {
							var existed = $.grep($scope.widgetExiteds, function (item) { return item.Id === r.Id });
							if (existed.length == 0) {
							var pram = {};
							pram = JSON.parse("{" + r.TemplateParams + "}");
								if (pram && pram.hasOwnProperty("Gui")) {
								r.Param = pram.Gui;
								}

								if (r.Group.Name === smallWidget) {
									if (!String.format) {
										String.format = function (format) {
											var args = Array.prototype.slice.call(arguments, 1);
											return format.replace(/{(\d+)}/g, function (match, number) {
												return typeof args[number] != 'undefined'
												  ? args[number]
												  : match
												;
											});
										};
									}
									if (r.Name == AppDefine.Resx.NUMBER_DVRRECORDLESS) {
										r.Title = String.format(cmsBase.translateSvc.getTranslate(r.Name), userLogin.RecordingDays);
									}
									else {
										r.Title = cmsBase.translateSvc.getTranslate(r.Name);
									}
									$scope.widgetHeaders.push(r);
								}
								else if (r.Name === "NOTE" || r.Name === "TODO" || r.Name === "WEATHER") {
									$scope.widgetExtras.push(r);
								}
								else {
									r.Image = GetChartImage(r.Name);
									r.Title = cmsBase.translateSvc.getTranslate(r.Name);
									$scope.widgetCharts.push(r);
								}
							}
						});
						$scope.widgetHeaders.sort(function (a, b) { return a.Title.localeCompare(b.Title) });
						$scope.widgetCharts.sort(function (a, b) { return a.Title.localeCompare(b.Title) });
					},
					function (error) {
					});
				}
			}

			$scope.selectWd = function(w) {
				$scope.selected = w;
			}

			$scope.CloseRegion = function () {
				$modalInstance.close();
			}

			$scope.SaveRegion = function () {
				if ($scope.selected == null || $scope.selected == undefined) {
					return;
				}
				$modalInstance.close($scope.selected);
			}
		}
	});
})();
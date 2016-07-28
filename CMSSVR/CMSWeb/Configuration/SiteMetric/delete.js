(function () {
	'use strict';

	define(['cms', 'DataServices/Configuration/sitemetricSvc'], function (cms) {
		cms.register.controller('metricdeleteCtrl', metricdeleteCtrl);

		metricdeleteCtrl.$inject = ['$scope', 'cmsBase', '$modalInstance', 'items', 'AppDefine', 'SiteMetricSvc', '$q', '$timeout', '$filter'];

		function metricdeleteCtrl($scope, cmsBase, $modalInstance, items, AppDefine, SiteMetricSvc, $q, $timeout, $filter) {
			var vm = this;
			vm.selectedRows = [];
			var rowIndex = 0;
			vm.isSingleItem = true;
			vm.itemIDs = [];
			vm.itemSelecteds = [];
			vm.msgDelete = AppDefine.Resx.SITEMETRIC_DELETE_CONFIRM_MSG;
			$scope.row = { entity: null };
			
			active();

			function active() {
				vm.itemSelecteds = items;
				if (vm.itemSelecteds && vm.itemSelecteds.length > 1) {
					vm.isSingleItem = false;
					vm.msgDelete = AppDefine.Resx.SITEMETRIC_DELETE_CONFIRM_MSGS;
				} else {
					//Data for tool tip
					$scope.row.entity = angular.copy(vm.itemSelecteds[0]);
				}
			}

			vm.listOptions = {
				data: 'vm.itemSelecteds',
				rowHeight: 40,
				multiSelect: true,
				selectedItems: vm.selectedRows,
				filterOptions: vm.query,
				rowTemplate: '<div ng-style="{ \'cursor\': row.cursor }" \
									ng-repeat="col in renderedColumns" ng-class="col.colIndex()" class="ngCell {{col.cellClass}}"> \
									<div class="ngVerticalBar" ng-style="{height: rowHeight}" ng-class="{ ngVerticalBarVisible: !$last }">&nbsp;</div><div ng-cell></div></div>',
				columnDefs: [
					{
						field: 'MetricName', displayName: $filter('translate')(AppDefine.Resx.SITEMETRIC_NAME)
							, cellTemplate: '<div class="ngCellText item-info" tooltip-template="\'Widgets/Templates/tooltip.html\'" tooltip-trigger="mouseenter" tooltip-class="grid-tooltip" tooltip-append-to-body="true" tooltip-placement="bottom"> \
												<div class="checkbox checkbox-default"><input type="checkbox" tabindex="-1" ng-checked="row.selected"><label></label></div> \
												<div class="i-name">{{row.entity.MetricName}}</div> \
										</div>'
					}
				],
				afterSelectionChange: function (data) {
					rowIndex = this.rowIndex;
				}
			};

			$scope.$on('ngGridEventData', function () {
				angular.forEach(vm.itemSelecteds, function (data, index) {
					vm.listOptions.selectItem(index, true);
				});
			});

			vm.cancel = function () {
				$modalInstance.close(AppDefine.ModalConfirmResponse.CLOSE);
			}

			vm.deleteMetric = function () {
				if (vm.isSingleItem) {
					vm.itemIDs.push(vm.itemSelecteds[0].MListID);
				}
				else {
					angular.forEach(vm.selectedRows, function (item, index) {
						vm.itemIDs.push(item.MListID);
					});
				}
				
				ExecuteDelete(vm.itemIDs);
				distroyData();
			}

			function ExecuteDelete(metrics) {
				SiteMetricSvc.DeleteMetricSite(metrics).then(
					function (response) {
						if (response.ReturnStatus) {
							var msg = cmsBase.translateSvc.getTranslate(response.ReturnMessage[0]);
							cmsBase.cmsLog.success(msg);
							$modalInstance.close();
						}
						else {
							var msg = "";
							angular.forEach(response.ReturnMessage, function (message, index) {
								if (message === AppDefine.Resx.SITEMETRIC_IS_USED) {
									var metricUsed = [];
									angular.forEach(response.Data, function (metric) { metricUsed.push(metric.MetricName); });
									msg = formatString(cmsBase.translateSvc.getTranslate(message), metricUsed.toString());
									cmsBase.cmsLog.warning(msg);
								}
								else {
									msg = cmsBase.translateSvc.getTranslate(message);
									cmsBase.cmsLog.warning(msg);
								}
							});
						}
					}
					, function (response) {
						var msg = cmsBase.translateSvc.getTranslate(response.ReturnMessage[0]);
						cmsBase.cmsLog.error(msg);
					}
				);
			}

			function distroyData() {
				vm.selectedItems = [];
				vm.itemIDs = [];
			}

			function formatString(format) {
				var args = Array.prototype.slice.call(arguments, 1);
				return format.replace(/{(\d+)}/g, function (match, number) {
					return typeof args[number] != 'undefined' ? args[number] : match;
				});
			};
		}
	});
})();
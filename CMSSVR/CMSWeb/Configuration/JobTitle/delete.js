(function () {
	'use strict';

	define(['cms', 'DataServices/Configuration/jobtitleSvc'], function (cms) {
		cms.register.controller('jobdeleteCtrl', jobdeleteCtrl);

		jobdeleteCtrl.$inject = ['$scope', 'cmsBase', '$modalInstance', 'items', 'AppDefine', 'AccountSvc', 'JobTitleSvc', 'colorSvc', '$filter'];

		function jobdeleteCtrl($scope, cmsBase, $modalInstance, items, AppDefine, AccountSvc, JobTitleSvc, colorSvc, $filter) {
			var vm = this;
			vm.selectedRows = [];
			var rowIndex = 0;
			vm.isSingleItem = true;
			vm.itemIDs = [];
			vm.itemSelecteds = [];
			vm.msgDelete = AppDefine.Resx.DELETE_JOB_CONFIRM_MSG;
			$scope.row = { entity: null };
			
			active();

			function active() {
				if (items) {
					vm.itemSelecteds = angular.copy(items);
					if (vm.itemSelecteds.length > 1) {
						vm.isSingleItem = false;
						vm.msgDelete = AppDefine.Resx.DELETE_JOB_CONFIRM_MSGS;
					}
					else {
						//Data for tool tip
						$scope.row.entity = angular.copy(vm.itemSelecteds[0]);
					}
				}
				
			}

			$scope.listOptions = {
				data: 'vm.itemSelecteds',
				rowHeight: 40,
				multiSelect: true,
				selectedItems: vm.selectedRows,
				rowTemplate: '<div ng-style="{ \'cursor\': row.cursor }" \
									ng-repeat="col in renderedColumns" ng-class="col.colIndex()" class="ngCell {{col.cellClass}}"> \
									<div class="ngVerticalBar" ng-style="{height: rowHeight}" ng-class="{ ngVerticalBarVisible: !$last }">&nbsp;</div><div ng-cell></div></div>',
				columnDefs: [
					{
						field: 'PositionName', displayName: $filter('translate')(AppDefine.Resx.JOBTILE_NAME)
						, cellTemplate: '<div class="ngCellText item-info" tooltip-template="\'Widgets/Templates/tooltip.html\'" tooltip-trigger="mouseenter" tooltip-class="grid-tooltip" tooltip-append-to-body="true" tooltip-placement="bottom"> \
												<div class="checkbox checkbox-default"><input type="checkbox" tabindex="-1" ng-checked="row.selected"><label></label></div> \
												<span class="i-color" ng-style="{ \'border-color\': vm.convertColor(row.entity.Color) }"></span> \
												<div class="i-name">{{row.entity.PositionName}}</div> \
										</div>'
					}
				],
				afterSelectionChange: function (data) {
					rowIndex = this.rowIndex;
				}
			};

			$scope.$on('ngGridEventData', function () {
				angular.forEach(vm.itemSelecteds, function (data, index) {
					$scope.listOptions.selectItem(index, true);
				});
			});

			vm.cancel = function () {
				$modalInstance.close(AppDefine.ModalConfirmResponse.CLOSE);
			}

			vm.deleteJob = function () {
				if (vm.isSingleItem) {
					vm.itemIDs.push(vm.itemSelecteds[0].PositionID);
				}
				else {
					angular.forEach(vm.selectedRows, function (item, index) {
						vm.itemIDs.push(item.PositionID);
					});
				}
				ExecuteDelete(vm.itemIDs);
				distroyData();
			}

			function ExecuteDelete(jobs) {
				JobTitleSvc.DeleteJobTitle(jobs).then(
					function (response) {
						if (response.ReturnStatus) {
							var msg = cmsBase.translateSvc.getTranslate(response.ReturnMessage[0]);
							cmsBase.cmsLog.success(msg);
							$modalInstance.close();
						}
						else {
							var msg = "";
							angular.forEach(response.ReturnMessage, function (message, index) {
								msg += cmsBase.translateSvc.getTranslate(message);
							});
							cmsBase.cmsLog.warning(msg);
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

			vm.convertColor = function (data) {
				return colorSvc.numtoRGB(data);
			};
		}
	});
})();
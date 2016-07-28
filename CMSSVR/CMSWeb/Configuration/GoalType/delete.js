(function () {
	'use strict';

	define(['cms', 'DataServices/Configuration/goaltypeSvc'], function (cms) {
		cms.register.controller('goaltypedelCtrl', goaltypedelCtrl);

		goaltypedelCtrl.$inject = ['$scope', 'cmsBase', '$modalInstance', 'items', 'AppDefine', 'GoalTypeSvc'];

		function goaltypedelCtrl($scope, cmsBase, $modalInstance, items, AppDefine, GoalTypeSvc) {
			var vm = this;
			var rowIndex = 0;
			vm.goalIDs = [];
			vm.selectedRows = [];
			vm.isSingleItem = true;
			vm.msgDelete = AppDefine.Resx.GOALTYPE_DELETE_CONFIRM_MSG;
			$scope.row = { entity: null };

			active();

			function active() {
				vm.itemSelecteds = angular.copy(items);
				if (vm.itemSelecteds && vm.itemSelecteds.length > 1) {
					vm.isSingleItem = false;
					vm.msgDelete = AppDefine.Resx.GOALTYPE_DELETE_CONFIRM_MSGS;
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
				rowTemplate: '<div ng-style="{ \'cursor\': row.cursor }" \
									ng-repeat="col in renderedColumns" ng-class="col.colIndex()" class="ngCell {{col.cellClass}}"> \
									<div class="ngVerticalBar" ng-style="{height: rowHeight}" ng-class="{ ngVerticalBarVisible: !$last }">&nbsp;</div><div ng-cell></div></div>',
				columnDefs: [
					{
						field: '', displayName: ''
						, cellTemplate: '<div class="ngCellText item-info" tooltip-template="\'Widgets/Templates/tooltip.html\'" tooltip-trigger="mouseenter" tooltip-class="grid-tooltip" tooltip-append-to-body="true" tooltip-placement="bottom"> \
												<div class="checkbox checkbox-default"><input type="checkbox" tabindex="-1" ng-checked="row.selected"><label></label></div> \
												<div class="i-name">{{row.entity.GoalName}}</div> \
										</div>'
					}
				],
			};

			$scope.$on('ngGridEventData', function () {
				angular.forEach(vm.itemSelecteds, function (item, index) {
					vm.listOptions.selectItem(index, true);
				});
			});

			vm.cancel = function () {
				$modalInstance.close(AppDefine.ModalConfirmResponse.CLOSE);
			}

			vm.deleteGoal = function () {
				vm.goalIDs = [];
				if (vm.isSingleItem) {
					vm.goalIDs.push(vm.itemSelecteds[0].GoalID);
				}
				else {
					angular.forEach(vm.selectedRows, function (item, index) {
						vm.goalIDs.push(item.GoalID);
					});
				}
				ExecuteDelete(vm.goalIDs);
			}
			function ExecuteDelete(goals) {
				GoalTypeSvc.DeleteGoalType(goals).then(function (data) {
					if (data.ReturnStatus) {
						var msg = cmsBase.translateSvc.getTranslate(data.ReturnMessage[0]);
						cmsBase.cmsLog.success(msg);
						$modalInstance.close();
					}
					else if (data.ReturnMessage.indexOf(AppDefine.Resx.GOALTYPE_IS_USED) != -1) {
						var msg = cmsBase.translateSvc.getTranslate(data.ReturnMessage[0]);
						msg += data.ReturnMessage[1];
						cmsBase.cmsLog.warning(msg);
						
					}
					else {
						angular.forEach(data.ReturnMessage, function (message) {
							var msg = cmsBase.translateSvc.getTranslate(message);
							cmsBase.cmsLog.warning(msg);
						});
					}
				},
				function (error) {
					var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.DELETE_FAIL_MSG);
					cmsBase.cmsLog.error(msg);
				});
			}
		}
	});
})();
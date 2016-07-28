(function () {
	'use strict';

	define(['cms', 'DataServices/Configuration/jobtitleSvc'], function (cms) {
		cms.register.controller('userdeleteCtrl', userdeleteCtrl);

		userdeleteCtrl.$inject = ['$scope', 'dataContext', 'cmsBase', '$modalInstance', 'items', 'AppDefine', 'JobTitleSvc', 'colorSvc', '$filter'];

		function userdeleteCtrl($scope, dataContext, cmsBase, $modalInstance, items, AppDefine, JobTitleSvc, colorSvc, $filter) {
			var vm = this;
			vm.selectedRows = [];
			var rowIndex = 0;
			vm.userIds = [];
			vm.isSingleUser = true;
			vm.msgDelete = AppDefine.Resx.USER_DELETE_CONFIRM_MSG;
			$scope.row = { entity: null};

			dataContext.injectRepos(['configuration.user']).then(active);

			function active() {
				vm.userSelecteds = items;
				if (vm.userSelecteds && vm.userSelecteds.length > 1) {
					vm.isSingleUser = false;
					vm.msgDelete = AppDefine.Resx.USER_DELETE_CONFIRM_MSGS;
				} else {
					//Data for tool tip
					$scope.row.entity = angular.copy(vm.userSelecteds[0]);
			}
			}

			$scope.listOptions = {
				data: 'vm.userSelecteds',
				rowHeight: 50,
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
												<span class="i-image"> \
													<img ng-if="row.entity.UPhoto!= null" ng-src="data:image/jpeg;base64,{{row.entity.ImageSrc}}" alt="" ng-style="{\'border\': \'2px solid \', \'border-color\': vm.convertColor(row.entity.PosColor)}" /> \
													<img ng-if="row.entity.UPhoto== null" src="../Content/Images/img_user_blank.png" alt="" ng-style="{\'border\': \'2px solid \', \'border-color\': vm.convertColor(row.entity.PosColor)}" />\
												</span> \
											<div class="i-name"> \
												{{row.entity.FName}} {{row.entity.LName}} \
											</div> \
											<div class="i-pos"> \
												{{row.entity.PosName}} \
											</div> \
										</div>'
					}
				],
			};

			$scope.$on('ngGridEventData', function () {
				angular.forEach(vm.userSelecteds, function (item, index) {
					$scope.listOptions.selectItem(index, true);
				});
			});

			vm.cancel = function () {
				$modalInstance.close(AppDefine.ModalConfirmResponse.CLOSE);
			}

			vm.deleteUser = function () {
				vm.userIds = [];
				if (vm.isSingleUser) {
					vm.userIds.push(vm.userSelecteds[0].UID);
				}
				else {
				angular.forEach(vm.selectedRows, function (item, index) {
					vm.userIds.push(item.UID);
				});
				}
				ExecuteDelete(vm.userIds);
			}

			vm.convertColor = function (data) {
				return colorSvc.numtoRGB(data);
			};

			function ExecuteDelete(user) {
				dataContext.user.DeleteUser(user).then(
					function (data) {
						if (data.ReturnStatus) {
							var msg = cmsBase.translateSvc.getTranslate(data.ReturnMessage[0]);
							cmsBase.cmsLog.success(msg);
							$modalInstance.close();
						}
						else {
							angular.forEach(data.ReturnMessage, function (message) {
								var msg = cmsBase.translateSvc.getTranslate(message);
							cmsBase.cmsLog.warning(msg);
								//console.log(data);
							});
						}
					}
					, function (error) {
						var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.DELETE_FAIL_MSG);
						cmsBase.cmsLog.error(msg);
					}
				);
			}
		}
	});
})();
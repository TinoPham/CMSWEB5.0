(function () {
	'use strict';

	define(['cms', 'DataServices/Configuration/recipientSvc'], function (cms) {
		cms.register.controller('recipientdelCtrl', recipientdelCtrl);

		recipientdelCtrl.$inject = ['$scope', 'cmsBase', '$modalInstance', 'items', 'AppDefine', 'AccountSvc', 'RecipientSvc'];

		function recipientdelCtrl($scope, cmsBase, $modalInstance, items, AppDefine, AccountSvc, RecipientSvc) {
			var vm = this;
			var rowIndex = 0;
			vm.selectedRows = [];
			vm.isSingleItem = true;
			vm.msgDelete = AppDefine.Resx.RECIPIENT_DELETE_CONFIRM_MSG;
			vm.repientIDs = [];


			if (items) {
				vm.itemSelecteds = angular.copy(items);
				if (vm.itemSelecteds && vm.itemSelecteds.length > 1) {
					vm.isSingleItem = false;
					vm.msgDelete = AppDefine.Resx.RECIPIENT_DELETE_CONFIRM_MSGS;
				}
			}
			
			vm.listOptions = {
				data: 'vm.itemSelecteds',
				rowHeight: 55,
				multiSelect: true,
				selectedItems: vm.selectedRows,
				columnDefs: [
					{
						field: '', displayName: ''
						, cellTemplate: '<div class="rep-info ngCellText ngSelectionCell"> \
											<div class="checkbox checkbox-default"><input type="checkbox" tabindex="-1" ng-checked="row.selected"><label></label></div> \
											<div class="rep-name">{{row.entity.FirstName}} {{row.entity.LastName}}</div> \
											<div class="rep-email">{{row.entity.Email}}</div> \
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
				$modalInstance.close('close');
			}

			vm.deleteRecipient = function () {
				vm.repientIDs = [];
				if (vm.isSingleItem) {
					vm.repientIDs.push(vm.itemSelecteds[0].RecipientID);
				}
				else {
					angular.forEach(vm.selectedRows, function (item, index) {
						vm.repientIDs.push(item.RecipientID);
					});
				}
				ExecuteDelete(vm.repientIDs);
			}
			function ExecuteDelete(recipients) {
				RecipientSvc.DeleteRecipient(recipients, function (data) {
					if (data == true) {
						var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.DELETE_SUCCESS);
						cmsBase.cmsLog.success(msg);
						$modalInstance.close();
					}
					else {
						var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.DELETE_FAIL);
						cmsBase.cmsLog.warning(msg);
						console.log(data);
					}
				},
                function (error) {
                	var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.DELETE_FAIL);
                	cmsBase.cmsLog.error(msg);
                });

			}
		}
	});
})();
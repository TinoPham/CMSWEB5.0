(function () {
	'use strict';

	define(['cms'], function (cms) {
		cms.register.controller('usergroupdelCtrl', usergroupdelCtrl);

		usergroupdelCtrl.$inject = ['$scope', 'dataContext', 'cmsBase', '$modalInstance', 'items', 'userGroupData', 'AppDefine', 'AccountSvc'];

		function usergroupdelCtrl($scope, dataContext, cmsBase, $modalInstance, items, userGroupData, AppDefine, AccountSvc) {
			var vm = this;
			var rowIndex = 0;
			vm.groupIDs = [];
			vm.selectedRows = [];
			vm.isSingleUserGroup = true;
			vm.msgDelete = AppDefine.Resx.USERGROUP_DELETE_CONFIRM_MSG;
			$scope.row = { entity: null };
			vm.userGroupToReplace = {};
			vm.msgConfirmToReplace = "";
			vm.requiredUGroup = false;
			vm.userGroupDeleteModel = {
				listUserGroupId: [],
				userGroupIdReplace: 0
			};

			dataContext.injectRepos(['configuration.usergroups']).then(active);

			function active() {
				vm.groupSelected = items;
				vm.groupList = userGroupData;
				if (vm.groupSelected && vm.groupSelected.length > 1) {
					vm.isSingleUserGroup = false;
					vm.msgDelete = AppDefine.Resx.USERGROUP_DELETE_CONFIRM_MSGS;
				} else {
					//Data for tool tip
					$scope.row.entity = angular.copy(vm.groupSelected[0]);
					vm.msgConfirmToReplace = formatString(cmsBase.translateSvc.getTranslate(AppDefine.Resx.USERGROUP_CONFIRM_REPLACE), vm.groupSelected[0].GroupName);
				}
			}

			$scope.listOptions = {
				data: 'vm.groupSelected',
				rowHeight: 50,
				multiSelect: true,
				selectedItems: vm.selectedRows,
				columnDefs: [
					{
						field: '', displayName: ''
						, cellTemplate: '<div class="ngCellText item-info" tooltip-template="\'Widgets/Templates/tooltip.html\'" tooltip-trigger="mouseenter" tooltip-class="grid-tooltip" tooltip-append-to-body="true" tooltip-placement="bottom"> \
											<div class="checkbox checkbox-default"><input type="checkbox" tabindex="-1" ng-checked="row.selected"><label></label></div> \
											<div class="i-name">{{row.entity.GroupName}}</div> \
											<div class="i-number">{{vm.GenerateField(row.entity.NumberUser)}}</div> \
										</div>'
					}
				]
			};

			$scope.$on('ngGridEventData', function () {
				angular.forEach(vm.groupSelected, function (item, index) {
					$scope.listOptions.selectItem(index, true);
				});
			});

			vm.cancel = function () {
				$modalInstance.close(AppDefine.ModalConfirmResponse.CLOSE);
			}

			vm.deleteGroup = function () {
				var userGroupAssigned = $.grep(vm.groupSelected, function (ugroup) { return ugroup.Users.length > 0; });
				if (vm.groupSelected.length > 1 && userGroupAssigned.length > 0) {
					var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.UNABLE_TO_DELETE_MULTIPLE_USERGROUP);
					cmsBase.cmsLog.warning(msg);
					return;
				}

				vm.groupIDs = [];
				if (vm.isSingleUserGroup) {
					vm.groupIDs.push(vm.groupSelected[0].GroupId);
					vm.requiredUGroup = ($.isEmptyObject(vm.userGroupToReplace) && vm.groupSelected[0].Users.length > 0);
				}
				else {
					angular.forEach(vm.selectedRows, function (item, index) {
						vm.groupIDs.push(item.GroupId);
					});
				}

				if (!vm.requiredUGroup) {
					vm.userGroupDeleteModel = {
						listUserGroupId: vm.groupIDs,
						userGroupIdReplace: vm.userGroupToReplace.GroupId
					};
					ExecuteDelete(vm.userGroupDeleteModel);
				}
			}

			function ExecuteDelete(dataRemove) {
				dataContext.usergroups.removeUserGroup(dataRemove).then(
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
							});
						}
					},
					function (error) {
						var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.DELETE_FAIL_MSG);
						cmsBase.cmsLog.error(msg);
					});
			}

			vm.GenerateField = function (value) {
				var ret = value.toString() + " " + cmsBase.translateSvc.getTranslate(AppDefine.Resx.USER_NUMBER);
				if (value > 1)
					ret = value.toString() + " " + cmsBase.translateSvc.getTranslate(AppDefine.Resx.USER_NUMBERS);
				return ret;
			}

			vm.uGroupchange = function () {
				vm.requiredUGroup = $.isEmptyObject(vm.userGroupToReplace);
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
(function () {
	'use strict';

	define(['cms', 'DataServices/Configuration/goaltypeSvc'], function (cms) {
		cms.register.controller('editaddgoaltypeCtrl', ['$scope', '$modalInstance', 'items', 'GoalTypeSvc', 'AccountSvc', 'AppDefine', 'cmsBase', 'Utils',
		function ($scope, $modalInstance, items, GoalTypeSvc, AccountSvc, AppDefine, cmsBase, Utils) {
			var vm = this;
			vm.goalSelected = {};
			vm.btn_Type = AppDefine.Resx.BTN_NEW;
			vm.Resx = AppDefine.Resx;
			vm.goalTypes = [];
			vm.msgValidator = "";
			vm.goalTypes = angular.copy(GoalTypeSvc.GoalTypes);
			getGoalTypes();
			if (items != null) {
				vm.goalSelected = angular.copy(items);
				vm.btn_Type = AppDefine.Resx.BTN_EDIT;
				initGoalTypeValue();
			}
			else {
				initGoalTypeValue();
			}

			function getGoalTypes() {
				if (GoalTypeSvc.GoalTypes.length === 0) {
					GoalTypeSvc.GetAllGoalType().then(function (data) {
						GoalTypeSvc.GoalTypes = data;
						vm.goalTypes = angular.copy(GoalTypeSvc.GoalTypes);
					});
				}
				return GoalTypeSvc.GoalTypes;
			}

			function initGoalTypeValue() {
				if (Object.keys(vm.goalSelected).length != 0) {
					angular.forEach(vm.goalSelected.MapValue, function (mapValue, mapIndex) {
						angular.forEach(vm.goalTypes, function (goaltype, index) {
							if (goaltype.GoalTypeID == mapValue.GoalTypeID) {
								goaltype.Value = mapValue;
							}
						});
					});
				}
			}

			vm.Cancel = function () {
				$modalInstance.close(AppDefine.ModalConfirmResponse.CLOSE);
			};

			vm.SaveGoalType = function () {
				if (GoalTypeValueValidFunc()) {
					if (vm.goalSelected.GoalID == null) {
						vm.goalSelected.GoalID = 0;
						vm.goalSelected.MapValue = [];
					}
					var count = 0;
					for (var i = 0; i < vm.goalTypes.length; i++) {
						if (vm.goalTypes[i].Value != null) {
							var mapValue = {};
							if (vm.goalSelected.GoalID != 0) {
								mapValue.GoalID = vm.goalSelected.GoalID;
							}
							mapValue.GoalTypeID = vm.goalTypes[i].GoalTypeID;
							var valueDecimal = parseFloat(vm.goalTypes[i].Value.MinValue);
							mapValue.MinValue = valueDecimal === NaN ? 0 : valueDecimal;
							valueDecimal = parseFloat(vm.goalTypes[i].Value.MaxValue);
							mapValue.MaxValue = valueDecimal === NaN ? 0 : valueDecimal;
							mapValue.GoalTypeName = vm.goalTypes[i].GoalTypeName;
							vm.goalSelected.MapValue[count] = mapValue;
							count++;
						}
					}
					vm.goalSelected.GoalLastUpdated = new Date();
					var user = AccountSvc.UserModel();
					vm.goalSelected.GoalCreateBy = user.UserID;

					GoalTypeSvc.AddGoalType(vm.goalSelected).then(
						function (data) {
							if (data.ReturnStatus) {
								console.log(data);
								var msg = cmsBase.translateSvc.getTranslate(data.ReturnMessage[0]);
								cmsBase.cmsLog.success(msg);
								$modalInstance.close(data.Data);
							}
							else {
								angular.forEach(data.ReturnMessage, function (message) {
									var msg = cmsBase.translateSvc.getTranslate(message);
									cmsBase.cmsLog.warning(msg);
								});
							}
						}
						, function (data) {
							if (vm.goalSelected.GoalID != 0) {
								var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.EDIT_FAIL_MSG);
								cmsBase.cmsLog.error(msg);
							}
							else {
								var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.ADD_FAIL_MSG);
								cmsBase.cmsLog.error(msg);
							}
						}
					);
				}
			};

			//Validator Functions
			function GoalTypeValueValidFunc() {
				var result = true;
				if (!vm.goalTypes) return !result;
				var minmaxRequestFor = [];
				var minmaxErrorFor = [];
				var goalTypeEmpty = [];
				var infix = " : ";
				for (var i = 0; i < vm.goalTypes.length; i++) {
					if (!vm.goalTypes[i].Value) {
						goalTypeEmpty.push(vm.goalTypes[i]);
					}
					else if (Utils.isNullOrEmpty(vm.goalTypes[i].Value.MinValue) && Utils.isNullOrEmpty(vm.goalTypes[i].Value.MaxValue)) {
						goalTypeEmpty.push(vm.goalTypes[i]);
					}
					else if (Utils.isNullOrEmpty(vm.goalTypes[i].Value.MinValue) && !Utils.isNullOrEmpty(vm.goalTypes[i].Value.MaxValue)) {
						minmaxRequestFor.push(vm.goalTypes[i].GoalTypeName);
					}
					else if (!Utils.isNullOrEmpty(vm.goalTypes[i].Value.MinValue) && Utils.isNullOrEmpty(vm.goalTypes[i].Value.MaxValue)) {
						minmaxRequestFor.push(vm.goalTypes[i].GoalTypeName);
					}
					else if (!Utils.isNullOrEmpty(vm.goalTypes[i].Value.MinValue) && !Utils.isNullOrEmpty(vm.goalTypes[i].Value.MaxValue)) {
						if (vm.goalTypes[i].Value.MinValue > vm.goalTypes[i].Value.MaxValue) {
							minmaxErrorFor.push(vm.goalTypes[i].GoalTypeName);
						}
					}
				}

				if (goalTypeEmpty.length == vm.goalTypes.length) {
					vm.msgValidator = cmsBase.translateSvc.getTranslate(AppDefine.Resx.GOALTYPE_VALUE_REQUIRED);
					result = false;
				}
				else if (minmaxErrorFor.length > 0) {
					vm.msgValidator = cmsBase.translateSvc.getTranslate(AppDefine.Resx.GOALTYPE_VALUE_COMPARE_INVALID);
					vm.msgValidator += infix + minmaxErrorFor.toString();
					result = false;
				}
				else if (minmaxRequestFor.length > 0) {
					vm.msgValidator = cmsBase.translateSvc.getTranslate(AppDefine.Resx.GOALTYPE_VALUE_REQUIRED);
					vm.msgValidator += infix + minmaxRequestFor.toString();
					result = false;
				}

				if (result) {
					vm.msgValidator = "";
					minmaxErrorFor = [];
					minmaxRequestFor = [];
				}

				return result;
			}
		}]);
	});
})();
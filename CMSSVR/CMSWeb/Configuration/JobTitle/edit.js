(function () {
	'use strict';

	define(['cms'], function (cms) {
		cms.register.controller('editaddjobtitleCtrl', editaddjobtitleCtrl);
		editaddjobtitleCtrl.$inject = ['$scope', '$modalInstance', 'items', 'JobTitleSvc', 'AccountSvc', 'colorSvc', 'AppDefine', 'cmsBase'];
		function editaddjobtitleCtrl($scope, $modalInstance, items, JobTitleSvc, AccountSvc, colorSvc, AppDefine, cmsBase) {
			$scope.message = null;
			$scope.jobSelected = {};
			$scope.btn_Type = AppDefine.Resx.JOB_ADD_HEADER;
			$scope.Resx = AppDefine.Resx;
			$scope.Color = colorSvc.getdefaultColor();

			if (items != null) {
				$scope.jobSelected = angular.copy(items);
				//$scope.jobSelected.Color = colorSvc.numtoRGB(items.Color);
				$scope.Color = colorSvc.numtoRGB(items.Color);
				$scope.btn_Type = AppDefine.Resx.JOB_EDIT_HEADER;
			}
			else {
				//$scope.jobSelected.Color = colorSvc.numtoRGB(colorSvc.getdefaultColor());
				$scope.Color = colorSvc.numtoRGB(colorSvc.getdefaultColor());
			}

			$scope.CloseJobTitle = function () {
				$modalInstance.close(AppDefine.ModalConfirmResponse.CLOSE);
			}

			$scope.SaveJobTitle = function () {
				if ($scope.jobSelected.PositionID == null) {
					$scope.jobSelected.PositionID = 0;
				}
				//$scope.jobSelected.Color = $scope.jobSelected.Color == null ? colorSvc.getdefaultColor() : colorSvc.rgbtoNum($scope.jobSelected.Color);
				$scope.jobSelected.Color = $scope.Color == null ? colorSvc.getdefaultColor() : colorSvc.rgbtoNum($scope.Color);
				var user = AccountSvc.UserModel();
				$scope.jobSelected.CreatedBy = user.UserID;
				$scope.jobSelected.CreatedDate = new Date();

				JobTitleSvc.AddJobTitle($scope.jobSelected).then(
					function (data) {
						if (data.ReturnStatus) {
							var msg = cmsBase.translateSvc.getTranslate(data.ReturnMessage[0]);
							cmsBase.cmsLog.success(msg);
							$modalInstance.close(data.Data);
						}
						else {
							angular.forEach(data.ReturnMessage, function (message) {
								$scope.message = message;
								var msg = cmsBase.translateSvc.getTranslate(message);
								cmsBase.cmsLog.warning(msg);
							});
						}
					}
					, function (error) {
						if ($scope.jobSelected.PositionID != 0) {
							var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.EDIT_FAIL_MSG);
							cmsBase.cmsLog.error(msg);
						}
						else {
							var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.ADD_FAIL_MSG);
							cmsBase.cmsLog.error(msg);
						}
					});
			}
		}
	});
})();
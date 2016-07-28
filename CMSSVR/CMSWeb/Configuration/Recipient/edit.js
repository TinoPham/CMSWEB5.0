(function () {
	'use strict';

	define(['cms'], function (cms) {
		cms.register.controller('editaddrecipientCtrl', editaddrecipientCtrl);
		editaddrecipientCtrl.$inject = ['$scope', '$modalInstance', 'items', 'RecipientSvc', 'AccountSvc', 'AppDefine', 'cmsBase'];
		function editaddrecipientCtrl($scope, $modalInstance, items, RecipientSvc, AccountSvc, AppDefine, cmsBase) {
			$scope.message = null;
			$scope.recipSelected = {};
			$scope.btn_Type = AppDefine.Resx.BTN_NEW;
			$scope.Resx = AppDefine.Resx;
			if (items != null) {
				$scope.recipSelected = angular.copy(items);
				$scope.btn_Type = AppDefine.Resx.BTN_EDIT;
			}

			$scope.CloseRecipient = function () {
				$modalInstance.close(AppDefine.ModalConfirmResponse.CLOSE);
			}

			$scope.SaveRecipient = function () {
				if ($scope.recipSelected.RecipientID == null) {
					$scope.recipSelected.RecipientID = 0;
				}
				var user = AccountSvc.UserModel();
				$scope.recipSelected.CreateBy = user.UserID;

				RecipientSvc.AddRecipient($scope.recipSelected)
                    .then(
                        function (data) {
                        	if (data.ReturnStatus == false) {
                        		$scope.message = data.ReturnMessage[0];
                        		$scope.myForm.email.$setValidity('unique', false);
                        		var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.SAVE_FAIL);
                        		cmsBase.cmsLog.warning(msg);
                        	}
                        	else {
                        		console.log(data);
                        		$modalInstance.close();
                        		var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.SAVE_SUCCESS);
                        		cmsBase.cmsLog.success(msg);
                        	}
                        }
                        , function (data, error) {
                        	console.log(data + error);
                        	alert(data + error);
                        	var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.SAVE_FAIL);
                        	cmsBase.cmsLog.error(msg);
                        }
                    );
			}

			/* Validator Form Data, start */
			$scope.EmailValidFunc = function (value) {
				if (!value) { return; }
				var reg = AppDefine.RegExp.EmailRestriction;
				if (!reg.test(value)) {
					return cmsBase.translateSvc.getTranslate(AppDefine.Resx.EMAIL_INVALID_MSG);
				}
				return true;
			}
		}
	});
})();
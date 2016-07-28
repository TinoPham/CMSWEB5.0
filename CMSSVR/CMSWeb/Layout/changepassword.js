(function() {
    'use strict';

    define(['cms'], function(cms) {
        
        cms.controller('changepasswordCtrl', changepasswordCtrl);
        changepasswordCtrl.$inject = ['$scope', 'cmsBase', '$modalInstance', 'items', '$modal', 'AppDefine', '$q', 'AccountSvc'];

        function changepasswordCtrl($scope, cmsBase, $modalInstance, items, $modal, AppDefine, $q, AccountSvc) {

            var vm = this;
            vm.PasswordConfirm = '';

            vm.passModel = {};

            vm.PasswordValidFunc = function (Password) {
                if (!Password) { return; }
                //2015-05-27 Tri fix contain special character in password
                //var reg = AppDefine.RegExp.InputRestriction;
                //if (!reg.test(Password)) {
				//	return cmsBase.translateSvc.getTranslate(AppDefine.Resx.RESTRICTION_MSG);
                //}
                if (Password.length < 6) {
					return cmsBase.translateSvc.getTranslate(AppDefine.Resx.PASSWORD_MINLENGTH_INVALID);
                }
                return true;
            }

            vm.CheckCurrentPassword = function (Password) {
                if (!Password) { return; }
                //2015-05-27 Tri fix contain special character in password
                //var reg = AppDefine.RegExp.InputRestriction;
                //if (!reg.test(Password)) {
                //    return cmsBase.translateSvc.getTranslate(AppDefine.Resx.RESTRICTION_MSG);
                //}
                if (Password.length < 6) {
                    return cmsBase.translateSvc.getTranslate(AppDefine.Resx.PASSWORD_MINLENGTH_INVALID);
                }

                return true;
            }

            vm.clickpassword = function () {
                $scope.chanpassForm.currentPassword.$setValidity("wrongpassword", true);
            }

            $scope.$watch('vm.PasswordConfirm', function(data) {
                vm.PasswordConfirmValidFunc(vm.passwordConfirm);
            });

            vm.PasswordConfirmValidFunc = function (passwordConfirm) {
                if (!passwordConfirm) { return; }
                if (passwordConfirm !== vm.passModel.NewPassword) {
					return cmsBase.translateSvc.getTranslate(AppDefine.Resx.PASSWORD_MATCH_MSG);
                }
                return true;
            }

            vm.TriggerValidPasswordConfirm = function () {
                var passwordConfirm = $scope.chanpassForm["txtPasswordConfirm"];
                passwordConfirm.$setDirty();
            }

            vm.save = function () {
                if (vm.passModel) {

                    changePass(vm.passModel);
                }
            }

            vm.close = function () {
				$modalInstance.close(AppDefine.ModalConfirmResponse.CLOSE);
            }


            function changePass(model) {
                AccountSvc.ChangePassword(model,function (response) {
					    if (response.ReturnStatus) {
					        $modalInstance.close(model);
					        var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.PASS_SAVE_SUCCESS);
					        cmsBase.cmsLog.success(msg);
					    }
					    else {
					        if (response.ReturnMessage) {
					            //var msg = cmsBase.translateSvc.getTranslate(response.ReturnMessage[0]);
					            //cmsBase.cmsLog.warning(msg);

					            $scope.chanpassForm.currentPassword.$setValidity("wrongpassword", false);

					        } else {
					            $modalInstance.close(model);
					            var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.PASS_SAVE_SUCCESS);
					            cmsBase.cmsLog.success(msg);
					        }

					    }
					}
					, function (response) {
					    var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.PASS_SAVE_FAIL);
					    cmsBase.cmsLog.error(msg);
					}
				);
            }
        }

    });

})();
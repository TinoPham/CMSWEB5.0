(function() {
    'use strict';

    define(['cms'], function(cms) {

        cms.controller('lostpasswordCtrl', lostpasswordCtrl);

        lostpasswordCtrl.$inject = ['$scope', 'AppDefine', '$state', 'cmsBase', 'AccountSvc', '$timeout'];
        function lostpasswordCtrl($scope, AppDefine, $state, cmsBase, AccountSvc, $timeout) {

            $scope.resetSuccess = false;
            $scope.lostPassword = {};
            $scope.about = {
                title: 'About',
                name: 'CMS WEB',
                version: AppDefine.CMSVERSION,
                fullName: 'CENTER_MANAGEMENT_SOFTWARE',
                company: 'i3 international.',
                companyUrl: 'http://i3international.com/',
                buildDate: AppDefine.BUILDDATE,
                buildNo: AppDefine.BUILDNO,
                copyYear: 2015,
                url: 'licenses/',
                language: 'en'
            }

            $scope.goLogin = function () {
                $state.go(AppDefine.State.LOGIN);
            }

            $scope.EmailValidFunc = function (email) {
                if (!email) { return; }
                var reg = AppDefine.RegExp.EmailRestriction;
                if (!reg.test(email)) {
					return cmsBase.translateSvc.getTranslate(AppDefine.Resx.EMAIL_INVALID_MSG);
                }
                return true;
            }


            $scope.UserNameValidFunc = function (userName) {
                if (!userName) { return; }
                var reg = AppDefine.RegExp.LoginRestriction;
                if (!reg.test(userName)) {
					return cmsBase.translateSvc.getTranslate(AppDefine.Resx.RESTRICTION_MSG);
                }
                if (userName.length < 4) {
					return cmsBase.translateSvc.getTranslate(AppDefine.Resx.USER_NAME_MINLENGTH_INVALID);
                }
                return true;
            }

            $scope.resetPassword = function ($event) {
                AccountSvc.resetPassword($scope.lostPassword);
                $scope.resetSuccess = true;
              
                $timeout(function () {
              
                    $scope.lostPassword = {};
                    $scope.lostpassform.$setValidity('txtEmail', true);
                    $scope.lostpassform.$setValidity('txtUserName', true);
                    $scope.lostpassform.reset();
                }, 100)
             
                
            }
        }
    });
})();
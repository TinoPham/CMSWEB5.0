(function () {
	'use strict';

	define(['cms', 'auth/lostpassword'], function (cms) {

		cms.config([
				'$translatePartialLoaderProvider', function run($translatePartialLoaderProvider) {
					$translatePartialLoaderProvider.addPart('Auth');
				}
		]);

		cms.controller('loginCtrl', loginCtrl);

		loginCtrl.$inject = ['$scope', '$location', 'router', 'AccountSvc', 'AppDefine', '$rootScope', '$state', 'cmsBase'];

		function loginCtrl($scope, $location, router, AccountSvc, AppDefine, $rootScope, $state, cmsBase) {

			var PASSWORD_MAXLENGTH = 4;

			$scope.model = {
				ID: 0
                , UserName: ''
                , Password: ''
                , Lang: 'en-US'
                , Remember: true
                , SID: ''

			};

			$scope.lostPassword = false;

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

			$scope.forgotPassword = function () {
				$state.go(AppDefine.State.LOSTPASSWORD);
			}

			$scope.alerts = [];

			$scope.closeAlert = function (index) {
				$scope.alerts.splice(index, 1);
			};

			$scope.typepassword = 'password';

			$scope.changetype = function () {
				if ($scope.typepassword === 'password') {
					$scope.typepassword = 'text';
				}
				else {
					$scope.typepassword = 'password';
				}
			}

			$scope.message = "";
			function LoginSucess(response) {
				$rootScope.$broadcast(AppDefine.Events.LOGINSUCCESS, response);
				//$state.go(AppDefine.State.DASHBOARD);
			}

			function LoginError(response) {
				$scope.alerts = [];
				var alert = {};
				var msgheader = cmsBase.translateSvc.getTranslate(AppDefine.Resx.LOGIN_ERROR_MSG);
				if (response.status === 401) {
					//ThangPham, Check User Expired Date, July 09 2015
					var userLogin = cms.GetResponseData(response);
					if (userLogin && userLogin.isExpired) {
						alert = { type: 'danger', msg: formatString(msgheader, cmsBase.translateSvc.getTranslate(AppDefine.Resx.LOGIN_EXPIRED_MSG)) };
					}
					else {
						alert = { type: 'danger', msg: formatString(msgheader, cmsBase.translateSvc.getTranslate(AppDefine.Resx.LOGIN_FAIL_MSG)) };
					}
				}
				else {
					var msg = !response.hasOwnProperty("Status") ? response.statusText : cmsBase.translateSvc.getTranslate(response.Status);
					alert = { type: 'danger', msg: formatString(msgheader, msg) };
				}
				$scope.alerts.push(alert);
			}

			$scope.login = function ($event) {
				AccountSvc.Login($scope.model, LoginSucess, LoginError);
			};

			function formatString(format) {
				var args = Array.prototype.slice.call(arguments, 1);
				return format.replace(/{(\d+)}/g, function (match, number) {
					return typeof args[number] != 'undefined' ? args[number] : match;
				});
			};
		}
	});
})();
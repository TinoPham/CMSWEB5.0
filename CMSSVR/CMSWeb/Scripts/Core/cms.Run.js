(function () {
	'use strict';

	define(['cms'], function (cms) {
		cms.run([
            '$rootScope', 'router', '$translate', 'translateSvc', '$location', 'AccountSvc', 'AppDefine',
            function ($rootScope, router, $translate, translateSvc, $location, AccountSvc, AppDefine) {
            	cms.EncryptedHeader = { 'Content-Type': AppDefine.Encrypt_Header + '; charset=UTF-8', 'Accept': AppDefine.Encrypt_Header + ', application/json, text/plain, */*' };
            	cms.Header = { 'Content-Type': 'application/json; charset=UTF-8', 'Accept': 'application/json, application/xml, text/plain, */*' };
            	cms.GetResponseData = function (response) {
            		if (!response)
            			return response;
            		if (response.hasOwnProperty("Data"))
            			return response.Data;

            		if (!response.hasOwnProperty("data"))
            			return response;

            		var data = response.data;
            		if (!data || !data.hasOwnProperty("Data"))
            			return data;
            		if (!data.Data)
            			return data;
            		return data.Data;
            	}
            	cms.EncryptHeader = function () {
            		var user = AccountSvc.UserModel();
            		var enc = (user && user.Settings) ? user.Settings.EncryptMode : 2;
            		if (($location.$$protocol == 'http' && enc == 2) || enc == 1)
            			return cms.EncryptedHeader;
            		else
            			return cms.Header;
            	}
            	AccountSvc.InitResource();

            	$rootScope.$on('$stateChangeSuccess', function (event, toState) {
            		$rootScope.title = $translate.instant(toState.translate);
            		$rootScope.iconState = toState.classstyle;
            		$rootScope.currentState = toState;
            		translateSvc.translate();
            		$rootScope.$broadcast(AppDefine.Events.STATECHANGESUCCESSHANDLER, { state: $rootScope.currentState.name });
            		
            	});

            	$rootScope.$on('$stateChangeStart', function (event, toState) {

                    if (toState.name === AppDefine.State.LOSTPASSWORD) {
                        return;
                    }

            		var auth = AccountSvc.isAuthenticated();
            		if (toState.name != AppDefine.State.LOGIN && !auth) {
            			console.log('DENY - Not Authentication');
            			event.preventDefault();
            			$location.path('../login');
            		}
            		else if (toState.name == AppDefine.State.LOGIN && auth) {
            			console.log('Duplicate Request Login Against - Autheticated');
            			event.preventDefault();
            			$location.path('/');
            		}
            	});


            	$rootScope.$on('$stateChangeError', function (event, toState, toParams, fromState, fromParams, error) {
            		$location.path('/');
            	}
                );

            	$rootScope.$on('$stateNotFound', function (event, current, previous, rejection) {

            		var destination = (current && (current.title || current.name || current.loadedTemplateUrl)) || 'unknow target';

            		var msg = 'Error routing to ' + destination + '.' + (rejection.msg || '');
            		$location.path('/');
            	});


            }
		]);
	});
})();
(function () {
	'use strict';

	define(['cms'], function (cms) {

		cms.factory('auth.service', authSvc);

		authSvc.$inject = ['$http', 'cmsBase', '$rootScope', 'DataServices/base.service'];

		function authSvc($http, cmsBase, $rootScope, baseService) {

			var serviceBase = "/";
			var $q = cmsBase.$q;
			var base = {};
			var authHelper = cmsBase.authHelper;

			var authentication = {
				isAuth: false,
				userName: "",
				userRoles: "",
				useRefreshTokens: false
			};

			return {
				create: createRepo
			};

			function createRepo() {
				base = new baseService('auth');

				var authSvcFactory =
                {
                	saveRegistration: saveRegistration,
                	login: logIn,
                	logOut: logOut,
                	fillAuthData: fillAuthData,
                	authentication: authentication,
                	refreshToken: refreshToken,
                	getUsers: getUsers,
                	userInRoles: userInRoles
                };

				return authSvcFactory;
			}

			function saveRegistration(registration) {
				logOut();
				return $http.post(serviceBase + 'api/account/register', {}, { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } })
                    .then(function (response) {
                    	return response;
                    })
                    .catch(function (error) { base.callFailed(error); });
			}

			function getUsers() {
				var defer = $q.defer();
				$http.get(serviceBase + 'api/getUsers').success(function (response) {
					defer.resolve(response);
				}).error(function (err, status) {

				});
				return defer.promise;
			}

			function userInRoles(allowRoles) {
				var result = false;

				var roles = $rootScope.Auth.userRoles.split(",");

				for (var i = 0; i < roles.length; i++) {
					if (allowRoles === roles[i]) {
						result = true;
					}
				}
				return result;
			}

			function logIn(loginData) {
				var data = loginData;
				var defer = $q.defer();

				$http.post(serviceBase + 'api/login', data, { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } }).success(function (response) {

					if (!response.access_token) {
						response.error_description = "Login Fail";
						defer.reject(response);
						return defer.promise;
					}

					var roles = authHelper.decodeToken(response.access_token).roles;

					if (loginData.useRefreshTokens) {
						cmsBase.localStorageService.cookie.set('authorizationData',
                        {
                        	token: response.access_token,
                        	userName: loginData.userName,
                        	refreshToken: response.refresh_token,
                        	userRoles: roles,
                        	useRefreshTokens: true
                        });
					} else {
						cmsBase.localStorageService.cookie.set('authorizationData',
                        {
                        	token: response.access_token,
                        	userName: loginData.userName,
                        	refreshToken: "",
                        	userRoles: roles,
                        	useRefreshTokens: false
                        });
					}

					authentication.isAuth = true;
					authentication.userRoles = roles;
					authentication.userName = loginData.userName;
					authentication.useRefreshTokens = loginData.useRefreshTokens;
					$rootScope.Auth = authentication;

					defer.resolve(response);
				}).error(function (err, status) {
					logOut();
					err.error_description = "Login Fail";
					defer.reject(err);
				});

				return defer.promise;
			}

			function logOut() {
				cmsBase.localStorageService.cookie.remove('authorizationData');
				authentication.isAuth = false;
				authentication.userName = "";
				authentication.userRoles = "";
				authentication.useRefreshTokens = false;
				$rootScope.Auth = authentication;
			}

			function fillAuthData() {
				var authData = cmsBase.localStorageService.cookie.get('authorizationData');
				if (authData) {
					authentication.isAuth = true;
					authentication.userName = authData.userName;
					authentication.userRoles = authData.userRoles;
					authentication.useRefreshTokens = authData.useRefreshTokens;
				}
				$rootScope.Auth = authentication;
			}

			function refreshToken() {
				var defer = $q.defer();

				var authData = cmsBase.localStorageService.cookie.get('authorizationData');

				if (authData) {
					if (authData.useRefreshTokens) {
						var data = "grant_type=refresh_token&fresh_token=" + authData.refreshToken + "&client_id=" + 'client ID1234';;

						cmsBase.localStorageService.cookie.remove('authorizationData');

						$http.post(serviceBase + 'token', data, { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } }).success(function (response) {
							cmsBase.localStorageService.cookie.set('authorizationData',
                                {
                                	token: response.access_token,
                                	userName: response.userName,
                                	refreshToken: response.refresh_token,
                                	usesRefreshTokens: true
                                });

							defer.resolve(response);
						})
                            .error(function (err, status) {
                            	logOut();
                            	defer.reject(err);
                            });
					}
				}
				return defer.promise;
			}
		}
	});
})();
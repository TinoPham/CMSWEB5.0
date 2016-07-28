(function () {
	'use strict';

	define(['cms'], function (cms) {
		cms.register.factory('account.service', accountSvc);

		accountSvc.$inject = ['$http', '$q', 'localStorageService'];

		function accountSvc($http, $q, localStorageService) {

			var serviceBase = "/";

			var authentication = {
				isAuth: false,
				userName: "",
				useRefreshTokens: false
			};

			return {
				create: createRepo // factory function to create the repository
			};

			function createRepo() {
				var authSvcFactory =
				{
					saveRegistrantion: saveRegistration,
					login: logIn,
					logOut: logOut,
					fillAuthData: fillAuthData,
					authentication: authentication,
					refreshToken: refreshToken,
					obtainAccessToken: obtainAccessToken,
					getUsers: getUsers
				};

				return authSvcFactory;
			}


			function saveRegistration(registration) {
				logOut();

				return $http.post(serviceBase + 'api/account/register', registration).then(function (response) {
					return response;
				});
			}

			function getUsers() {
				var defer = $q.defer();
				$http.get(serviceBase + 'api/getUsers').success(function (response) {
					defer.resolve(response);
				}).error(function (err, status) {

				});
				return defer.promise;
			}

			function logIn(loginData) {
				//var data = "grant_type=password&username=" + loginData.userName + '&password' + loginData.password;

				//if (loginData.useRefreshTokens) {
				//    data = data + '&client_id=' + 'client ID1234';
				//}

				var data = loginData;
				var defer = $q.defer();

				$http.post(serviceBase + 'api/login', data, { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } }).success(function (response) {
					if (loginData.useRefreshTokens) {
						localStorageService.set('authorizationData',
						{
							token: response.access_token,
							userName: loginData.userName,
							refreshToken: response.refresh_token,
							useRefreshTokens: true
						});
					} else {
						localStorageService.set('authorizationData',
						{
							token: response.access_token,
							userName: loginData.userName,
							refreshToken: "",
							useRefreshTokens: false
						});
					}

					authentication.isAuth = true;
					authentication.userName = loginData.userName;
					authentication.useRefreshTokens = loginData.useRefreshTokens;

					defer.resolve(response);
				}).error(function (err, status) {
					logOut();
					defer.reject(err);
				});

				return defer.promise;
			}

			function logOut() {
				localStorageService.remove('authorizationData');
				authentication.isAuth = false;
				authentication.userName = "";
				authentication.useRefreshTokens = false;
			}

			function fillAuthData() {
				var authData = localStorageService.get('authorizationData');
				if (authData) {
					authentication.isAuth = true;
					authentication.userName = authData.userName;
					authentication.useRefreshTokens = authData.useRefreshTokens;
				}
			}

			function refreshToken() {
				var defer = $q.defer();

				var authData = localStorageService.get('authorizationData');

				if (authData) {
					if (authData.useRefreshTokens) {
						var data = "grant_type=refresh_token&fresh_token=" + authData.refreshToken + "&client_id=" + 'client ID1234';;

						localStorageService.remove('authorizationData');

						$http.post(serviceBase + 'token', data, { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } }).success(function (response) {
							localStorageService.set('authorizationData',
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

			function obtainAccessToken(externalData) {
				var defer = $q.defer();

				$http.get(serviceBase + 'api/account/ObtainLocalAccessToken',
					{
						params: {
							provider: externalData.provider,
							externalAccessToken: externalData.externalAccessToken
						}
					})
					.success(function (response) {
						localStorageService.set('authorizationData', {
							token: response.access_token,
							userName: response.userName,
							refreshToken: "",
							useRefreshTokens: false
						});

						authentication.isAuth = true;
						authentication.userName = response.userName;
						authentication.useRefreshTokens = false;
						defer.resolve(response);
					})
					.error(function (err, status) {
						logOut();
						defer.reject(err);
					});
				return defer.promise;
			}
		}
	}
    );
})();
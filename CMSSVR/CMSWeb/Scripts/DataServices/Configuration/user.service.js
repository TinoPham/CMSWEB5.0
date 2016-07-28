(function () {
	'use strict';

	define(['cms'], function (cms) {
		cms.register.factory('user.service', userAdminSvc);

		userAdminSvc.$inject = ['cmsBase', '$resource', 'AppDefine', '$q', 'AccountSvc'];

		function userAdminSvc(cmsBase, $resource, AppDefine, $q, AccountSvc) {

			var apibase = AppDefine.Api.UserManager + '/:dest';
			var UsersResource = $resource(apibase, { dest: "@dest" }, {
				'AddUser': { method: 'POST', headers: cms.EncryptHeader(), params: { dest: "AddUser" }, interceptor: { response: function (response) { return response; } } },
				'UpdateProfile': { method: 'POST', headers: cms.EncryptHeader(), params: { dest: "UpdateProfile" }, interceptor: { response: function (response) { return response; } } },
				'Users': { method: 'GET', params: { dest: "Users" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				'UserDetail': { method: 'GET', params: { dest: "UserDetail" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				'UpdateUser': { method: 'POST', headers: cms.EncryptHeader(), params: { dest: "UpdateUser" }, interceptor: { response: function (response) { return response; } } },
				'DeleteUser': { method: 'POST', headers: cms.EncryptHeader(), params: { dest: "DeleteUser" }, interceptor: { response: function (response) { return response; } } },
				'GetMaxEmployeeID': { method: 'GET', headers: cms.EncryptHeader(), params: { dest: "GetMaxEmployeeID" }, interceptor: { response: function (response) { return response; } } }
			});

			return {
				create: createRepo
			};

			function createRepo() {

				var userService = {
					AddUser: AddUser,
					UpdateProfile: UpdateProfile,
					getUsers: GetUsers,
					GetMaxEmployeeID: GetMaxEmployeeID,
					UpdateUser: UpdateUser,
					DeleteUser: DeleteUser,
					getUserDetail: GetUserDetail,
					GetUserImage: GetUserImage
				};
				return userService;
			}

			function GetUsers(successFn, errorFn) {
				UsersResource.Users().$promise.then(function (result) {
					var data = cms.GetResponseData(result);
					successFn(data);
				}, function (error) {
					errorFn(error);
				});
			}

			function GetUserDetail(userId) {
				var def = $q.defer();

				UsersResource.UserDetail({ 'userID': userId }).$promise.then(
					function (result) {
						var data = cms.GetResponseData(result);
						def.resolve(data);
					}
					, function (error) {
						console.log(error);
						def.reject(error);
					}
				);
				return def.promise;
			}

			function AddUser(data) {
				var def = $q.defer();
				data.UserName = CryptoJS.AES.encrypt(data.UserName, AccountSvc.SID()).toString();
				data.Password = data.Password == null ? data.Password : CryptoJS.AES.encrypt(data.Password, AccountSvc.SID()).toString();
				UsersResource.AddUser(data).$promise.then(
                    function (response) {
                    	//var data = cms.GetResponseData( response );
                    	def.resolve(response.data);
                    },
                    function (error) {
                    	def.reject(error);
                    }
                );
				return def.promise;
			}

			function UpdateProfile(data) {
				var def = $q.defer();
				data.UserName = CryptoJS.AES.encrypt(data.UserName, AccountSvc.SID()).toString();
				UsersResource.UpdateProfile(data).$promise.then(
                    function (response) {
                    	def.resolve(response.data);
                    },
                    function (error) {
                    	console.log(error);
                    	def.reject(error);
                    }
                );
				return def.promise;
			}

			function UpdateUser(data) {
				var def = $q.defer();

				UsersResource.UpdateUser(data).$promise.then(
                    function (response) {
                    	//var data = cms.GetResponseData( response );
                    	def.resolve(result);
                    },
                    function (error) {
                    	console.log(error);
                    	def.reject(error);
                    }
                );
				return def.promise;
			}

			function DeleteUser(data) {
				var def = $q.defer();
				UsersResource.DeleteUser(data).$promise.then(
                    function (response) {
                    	var data = cms.GetResponseData(response);
                    	def.resolve(data);
                    },
					function (error) {
                    	//console.log(error);
                    	def.reject(error);
                    }
                );
				return def.promise;
			}

			function GetMaxEmployeeID() {
				var def = $q.defer();

				UsersResource.GetMaxEmployeeID().$promise.then(
                    function (response) {
                    	var data = cms.GetResponseData(response);
                    	def.resolve(data);
                    }, function (error) {
                    	console.log(error);
                    	def.reject(error);
                    }
                );
				return def.promise;
			}

			function GetUserImage(userId, filename) {
				return AppDefine.Api.UserManager + "/GetUserImage?id=" + userId + "&name=" + filename;
			}
		}
	});
})();
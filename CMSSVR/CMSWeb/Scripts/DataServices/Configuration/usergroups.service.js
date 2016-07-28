(function () {
	'use strict';

	define(['cms'], function (cms) {
		cms.register.factory('usergroups.service', usergroupsSvc);

		usergroupsSvc.$inject = ['cmsBase', '$resource', 'AppDefine', '$q'];

		function usergroupsSvc(cmsBase, $resource, AppDefine, $q) {
			var $q = cmsBase.$q;
			var apibase = AppDefine.Api.UserGroup + '/:dest';
			var userGroup = $resource(apibase, { dest: "@dest" }, {
				getUserGroup: { method: 'GET', params: { dest: "GetUserGroup" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				getUserList: { method: 'GET', params: { dest: "GetUserList" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				getFunctions: { method: 'GET', params: { dest: "GetFunctionList" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				getFuncLevel: { method: 'GET', params: { dest: "GetFuncLevel" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				updateUserGroup: { method: 'POST', params: { dest: "UpdateUserGroup" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				removeUserGroup: { method: 'POST', params: { dest: "DeleteUserGroup" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
			});

			return {
				create: createRepo // factory function to create the repository
			};

			function InitData() {
				return {
					GroupID: 0,
					GrouName: null,
					Description: null,
					CreatedBy: null,
					GroupLevel: true,
					NumberUser: 0,
					Users: {},
					Functions: {}
				}
			}

			function createRepo() {

				var usergroup = {
					getUserGroup: getUserGroup,
					getUserList: getUserList,
					updateUserGroup: updateUserGroup,
					getFunctions: getFunctions,
					getFuncLevel: getFuncLevel,
					removeUserGroup: removeUserGroup
				}
				return usergroup;
			}


			function getFunctions(successFn, errorFn) {
				userGroup.getFunctions().$promise.then(function (result) {
					var data = cms.GetResponseData(result);
					successFn(data);
				}, function (error) {
					errorFn(error);
				});
			}

			function getFuncLevel(successFn, errorFn) {
				userGroup.getFuncLevel().$promise.then(function (result) {
					var data = cms.GetResponseData(result);
					successFn(data);
				}, function (error) {
					errorFn(error);
				});
			}

			function getUserGroup() {
				var def = $q.defer();
				userGroup.getUserGroup().$promise.then(function (result) {
					var data = cms.GetResponseData(result);
					def.resolve(data);
				}, function (error) {
					def.reject(error);
				});
				return def.promise;
			}

			function getUserList(successFn, errorFn) {
				userGroup.getUserList().$promise.then(function (result) {
					var data = cms.GetResponseData(result);
					successFn(data);
				}, function (error) {
					errorFn(error);
				});
			}


			function updateUserGroup(data, successFn, errorFn) {
				userGroup.updateUserGroup(data).$promise.then(function (result) {
					//var reponse = cms.GetResponseData(result);
					successFn(result.data);
				}, function (error) {
					errorFn(error);
				});
			}

			function removeUserGroup(requestdata) {
				var def = $q.defer();
				userGroup.removeUserGroup(requestdata).$promise.then(
					function (result) {
						var data = cms.GetResponseData(result);
						def.resolve(data);
					}, function (error) {
						def.reject(error);
					});
				return def.promise;
			}
		}
	});
})();
(function() {
    'use strict';

    define(['cms'], function(cms) {
        cms.register.service('dashboard.service', dashboardSvc);

        dashboardSvc.$inject = ['$resource', 'cmsBase', 'AppDefine'];

        function dashboardSvc($resource, cmsBase, AppDefine) {

            var urldashboard = AppDefine.Api.Dashboard;

            var dashboardResource = $resource(urldashboard, { UId: '@UId' }, {
                getDashboard: { method: 'GET', url: urldashboard + '/GetDashboard', headers: cms.EncryptHeader() }, // isArray: true }
                getElements: { method: 'GET', url: urldashboard + '/GetElements', headers: cms.EncryptHeader() },
                getLayouts: { method: 'GET', url: urldashboard + '/GetLayouts', headers: cms.EncryptHeader() },
                editDashboard: { method: 'POST', url: urldashboard + '/EditDashboard', headers: cms.EncryptHeader() },
                getDefinedDashboard: { method: 'GET', url: urldashboard + '/GetDefinedDashboard', headers: cms.EncryptHeader() }
            });


            var userService = {
                getDashboard: GetDashboard,
                getElements: GetElements,
                getLayouts: GetLayouts,
                editDashboard: EditDashboard,
				getDefinedDashboard: GetDefinedDashboard,
                Dashboard: null
            };

            function createRepo() {
                return userService;
            }

            return {
                create: createRepo
            };

            function EditDashboard(data, successFn, errorFn) {
                dashboardResource.editDashboard(data, function (result) {
                    var rlt = cms.GetResponseData(result);
                    successFn(rlt);
                }, function (error) {
                    errorFn(error);
                });
            }

            function GetLayouts(successFn, errorFn) {
                dashboardResource.getLayouts().$promise.then(function (result) {
                    var rlst = cms.GetResponseData(result);
                    successFn(rlst);
                }, function (error) {
                    errorFn(error);
                });
            }

            function GetElements(successFn, errorFn) {
                dashboardResource.getElements().$promise.then(function (result) {
                    var rlst = cms.GetResponseData(result);
                    successFn(rlst);
                }, function (error) {
                    errorFn(error);
                });
            }

            function GetDashboard(successFn, errorFn) {
                dashboardResource.getDashboard().$promise.then(function (result) {
                    var rlst = cms.GetResponseData(result);
                    successFn(rlst);
                }, function (error) {
                    errorFn(error);
                });
            }

			function GetDefinedDashboard(param, successFn, errorFn) {
				dashboardResource.getDefinedDashboard({ level: param }).$promise.then(function (result) {
					var rlst = cms.GetResponseData(result);
					successFn(rlst);
				}, function (error) {
					errorFn(error);
				});
			}

        }
    });
})();
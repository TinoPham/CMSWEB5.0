(function() {
    'use strict';

    define(['cms'], function(cms) {
        cms.register.service('commoninfo.service', commoninfoSvc);

        commoninfoSvc.$inject = ['$resource', 'cmsBase'];
        function commoninfoSvc($resource, cmsBase) {
            var url = '../api/cmsweb/commoninfo';

            var datainfo = $resource(url, {userId: '@userId'}, {
                getcontries: { method: 'GET', url: url + '/GetCountries', isArray: true, interceptor: { response: function ( response ) { return response; } } },
                getstates: { method: 'GET', url: url + '/GetStates', isArray: true, interceptor: { response: function ( response ) { return response; } } }
            });

            return {
                create: createRepo
            };

            function createRepo() {
                var service = {
                    getCountries: getCountries,
                    getStates: getStates
                }

                return service;
            }

            function getStates(data, successFn, errorFn) {
                datainfo.getstates(data).$promise.then( function (result) {
                    successFn(result.data);
                }, function (error) {
                    errorFn(error);
                });
            }

            function getCountries(successFn, errorFn) {
                datainfo.getcontries().$promise.then( function (result) {
                    successFn(result.data);
                }, function (error) {
                    errorFn(error);
                });
            }
        }
    });
})();
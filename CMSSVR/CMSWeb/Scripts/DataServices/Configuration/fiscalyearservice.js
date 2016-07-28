(function () {
    define(['cms'], function (cms) {
        cms.register.service('fiscalyearservice', fiscalyearservice);
        fiscalyearservice.$inject = ['AppDefine', 'Cookies', '$resource', '$http', '$q'];

        function fiscalyearservice(AppDefine, Cookies, $resource, $http, $q) {

            var apibase = AppDefine.Api.fiscalyear + '/:dest/?';
            var fiscalyearmodel = $resource(apibase, { dest: "@dest", date: "@date", fyModel: "@fyModel" }, {
                'GetCustomFiscalYear': { method: 'GET', params: { dest: "GetFiscalYear" }, headers: cms.EncryptHeader(), interceptor: { response: function ( response ) { return response; } } },
                'Update': { method: 'POST', params: { dest: "Update" }, headers: cms.EncryptHeader(), interceptor: { response: function ( response ) { return response; } } }
            });
            var FiscalYearModel = InitFiscalYearMode();
            function InitFiscalYearMode() {
                return {
                  FYID:0,
                  FYName:"",
                  FYTypesID:1,
                  FYDateStart:new Date(),
                  FYDateEnd: new Date(),
                  FYClosest: 6,
                  FYNoOfWeeks: 0,
                  CreatedBy: 0,
                  FYDate: new Date(),
                  CalendarStyle:"454"
                };
            }

            var GetCustomFiscalYear = function (date) {
                var def = $q.defer();
                fiscalyearmodel.GetCustomFiscalYear({'date': date}).$promise.then(
                    function ( response ) {
                        var data = cms.GetResponseData( response );
                        def.resolve(data);
                    },
                    function (error) {
                        console.log(error);
                        def.reject(error);
                    }
                );
                return def.promise;
            };
            var Update = function (fyModel) {
                var def = $q.defer();
                fiscalyearmodel.Update(fyModel).$promise.then(
                    function ( response ) {
                        var data = cms.GetResponseData( response );
                        def.resolve(data);
                    },
                    function (error) {
                        console.log(error);
                        def.reject(error);
                    }
                );
                return def.promise;
            };

            return {
                    GetCustomFiscalYear: GetCustomFiscalYear,
                    Update:Update,
                    FiscalYearModel: function () { return FiscalYearModel; }
            };

        }

    });

}
)();

(function () {
    define(['cms'], function (cms) {
        cms.register.service('IncidentSvc', IncidentSvc);
        IncidentSvc.$inject = ['AppDefine', 'Cookies', '$resource', '$http', '$q'];

        function IncidentSvc(AppDefine, Cookies, $resource, $http, $q) {

            var apibase = AppDefine.Api.Incident + '/:dest/?';
            var Incident = $resource(apibase, { dest: "@dest"}, {
                'GetIncidentManagent': { method: 'GET', headers: cms.EncryptHeader(), isArray: false, params: { dest: "GetIncidentManagent" }, interceptor: { response: function (response) { return response; } } }
                , 'GetCaseType': { method: 'GET', headers: cms.EncryptHeader(), isArray: false, params: { dest: "GetCaseType" }, interceptor: { response: function ( response ) { return response; } } }
                , 'UpdateIncidentField': { method: 'POST', headers: cms.EncryptHeader(), params: { dest: "UpdateIncidentField" }, interceptor: { response: function ( response ) { return response; } } }
            });

            var IncidentFieldModel = InitIncidentFieldModel();
            function InitIncidentFieldModel() {
                return {
                    FieldsGUIID: 0
                , CaseTypeID: 0
                , Status: true
                };                
            }
           
            var GetIncidentManagent = function (caseType) {
                var def = $q.defer();
                Incident.GetIncidentManagent({ 'caseType': caseType }).$promise.then(
                    function ( response ) {
                        var data = cms.GetResponseData(response);
                        def.resolve(data);
                    },
                    function (error) {
                        console.log(error);
                        def.reject(error);
                    }
                );
                return def.promise;
            };

            var UpdateIncidentField = function (models) {
                var def = $q.defer();
                Incident.UpdateIncidentField(models).$promise.then(
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

            var GetCaseType = function () {
                var def = $q.defer();
                Incident.GetCaseType().$promise.then(
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
                GetCaseType: GetCaseType,
                GetIncidentManagent: GetIncidentManagent,
                UpdateIncidentField: UpdateIncidentField,
                IncidentFieldModel: function () { return IncidentFieldModel; }
            };

        }

    });

}
)();

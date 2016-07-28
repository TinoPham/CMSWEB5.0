(function () {
    define(['cms'], function (cms) {
        cms.register.service('BoxGadgetSvc', BoxGadgetSvc);
        BoxGadgetSvc.$inject = ['AppDefine', 'Cookies', '$resource', '$http', '$q'];

        function BoxGadgetSvc(AppDefine, Cookies, $resource, $http, $q) {

            var apibase = AppDefine.Api.Dashboard + '/:dest/?';
            var BoxGadget = $resource(apibase, { dest: "@dest" }, {
                'GetBoxGadgets': { method: 'GET', headers: cms.EncryptHeader(), isArray: true, params: { dest: "GetBoxGadgets" }, interceptor: { response: function (response) { return response; } } }               
            });

           
            var GetBoxGadgets = function (time, gadgets) {                
                var def = $q.defer();
                BoxGadget.GetBoxGadgets({ 'date': time, 'gadgets': gadgets }).$promise.then(
                    function (response) {
                        def.resolve(response.data);
                    },
                    function (error) {
                        console.log(error);
                        def.reject(error);
                    }
                );
                return def.promise;
            };

            var BoxGadgets = [];

            return {
                GetBoxGadgets: GetBoxGadgets,
                BoxGadgets: BoxGadgets
            };

        }

    });

}
)();

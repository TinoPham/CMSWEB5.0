(function () {
    define(['cms'],
        function (cms) {
        cms.register.service('license.service', LicenseService);
        LicenseService.$inject = ['AppDefine', 'Cookies', '$resource', '$http', '$q'];
        function LicenseService(AppDefine, Cookies, $resource, $http, $q)
        {
            var url = AppDefine.Api.License;
            var license = $resource(url, {}, {
                                            Get: { method: 'GET', url: url + '/Get', headers: cms.EncryptHeader() },
            });
            function Get(successFn, errorFn) {
                license.Get(function (result) {
                    var data = cms.GetResponseData(result);
                    successFn(data);
                }, function (error) {
                    errorFn(error);
                });
            }

            return {
                create: function () {
                    var service = {
                        Get: Get,
                    }
                    return service;
                }
            }
        }
    });
})();
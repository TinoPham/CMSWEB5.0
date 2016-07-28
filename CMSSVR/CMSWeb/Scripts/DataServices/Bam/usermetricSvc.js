(function () {
    define(['cms'], function (cms) {

        cms.register.service('usermetricSvc', usermetricSvc);

        usermetricSvc.$inject = ['AppDefine', '$resource', '$http', 'Utils', '$q'];

        function usermetricSvc(AppDefine, $resource, $http, Utils, $q) {
            var apibase = AppDefine.Api.DashboardReports + '/:dest/';

            var metricReport = $resource(apibase, { dest: "@dest" }, {
                GetMetricReport: { method: 'GET', params: { dest: "GetMetricReport" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
                GetCustomReports: { method: 'GET', params: { dest: "GetCustomReports" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
                UpdateMetricReport: { method: 'POST', params: { dest: "UpdateMetricReport" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } }
            });

            var metricReportSvc = {
                getMetricReport: GetMetricReport,
                getCustomReports: GetCustomReports,
                updateMetricReport: UpdateMetricReport
            };

            return metricReportSvc;

            function GetCustomReports(params, successFn, errorFn) {
                metricReport.GetCustomReports(params).$promise.then(
                    function (result) {
                        successFn(cms.GetResponseData(result));
                    }, function (error) {
                        errorFn(error);
                    });
            }

            function GetMetricReport(params, successFn, errorFn) {
                metricReport.GetMetricReport(params).$promise.then(
                    function(result) {
                        successFn(cms.GetResponseData(result));
                    }, function(error) {
                        errorFn(error);
                    });
            }

            function UpdateMetricReport(params, successFn, errorFn) {
                metricReport.UpdateMetricReport(params).$promise.then(
                    function (result) {
                        successFn(cms.GetResponseData(result));
                    }, function (error) {
                        errorFn(error);
                    });
            }

            
        }
    });

}
)();
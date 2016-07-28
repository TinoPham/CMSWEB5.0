(function () {
	define(['cms'], function (cms) {

	    cms.register.service('BamHeaderReportsSvc', BamHeaderReportsSvc);

	    BamHeaderReportsSvc.$inject = ['AppDefine', '$resource', '$http', 'Utils', '$q'];

	    function BamHeaderReportsSvc(AppDefine, $resource, $http, Utils, $q) {
	        var apibase = AppDefine.Api.BamHeaderReports + '/:dest/';

	        var BamHeaderReportsSrc = $resource(apibase, { dest: "@dest" }, {
	            GetHeaderBam: { method: 'GET', params: { dest: "GetHeaderBam" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } }
			});

	        var bamHeaderReportSvc = {
	            GetHeaderBam: getHeaderBam
			};

	        function getHeaderBam(params, successFn, errorFn) {
				var def = $q.defer();
				BamHeaderReportsSrc.GetHeaderBam(params).$promise.then(
					function (result) {
					    //def.resolve(result);
					    successFn(cms.GetResponseData(result));
					}
					, function (error) {
					    //def.reject(error);
					    errorFn(error);
					});
				return def.promise;
			}

	        return bamHeaderReportSvc;

		}
	});

}
)();
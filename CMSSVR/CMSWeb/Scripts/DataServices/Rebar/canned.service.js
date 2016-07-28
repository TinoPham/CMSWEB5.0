(function () {
	define(['cms'], function (cms) {

		cms.register.service('cannedSvc', cannedSvc);

		cannedSvc.$inject = ['AppDefine', '$resource', '$http', 'Utils', '$q'];

		function cannedSvc(AppDefine, $resource, $http, Utils, $q) {
			var apibase = AppDefine.Api.Canned + '/:dest/';

			var canneResource = $resource(apibase, { dest: "@dest" }, {
				GetCannedReportAPI: { hideOverlay: true, method: 'POST', params: { dest: "GetCannedReport" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } }
			});

			var cannedReportSvc = {
				GetCannedReport: getCannedReport
			};

			return cannedReportSvc;

			function getCannedReport(params, successFn, errorFn) {
				canneResource.GetCannedReportAPI(params).$promise.then(
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
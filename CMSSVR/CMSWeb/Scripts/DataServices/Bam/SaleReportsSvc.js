(function () {
	define(['cms'], function (cms) {

		cms.register.service('SaleReportsSvc', SaleReportsSvc);

		SaleReportsSvc.$inject = ['AppDefine', '$resource', '$http', 'Utils', '$q'];

		function SaleReportsSvc(AppDefine, $resource, $http, Utils, $q) {
			var apibase = AppDefine.Api.SaleReports + '/:dest/';

			var SaleReportsSrc = $resource(apibase, { dest: "@dest" }, {
				GetReportData: { method: 'GET', params: { dest: "GetReportData" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } }
				
			});

			var saleReportSvc = {
				GetReportData: getReportData
			};

			function getReportData(params, successFn, errorFn) {
				//var def = $q.defer();
				SaleReportsSrc.GetReportData(params).$promise.then(
					function (result) {
						//def.resolve(result);
						successFn(cms.GetResponseData(result));
					}
					, function (error) {
						//def.reject(error);
						errorFn(error);
					});
				//return def.promise;
			}

			return saleReportSvc;
		}
	});

}
)();
(function () {
	define(['cms'], function (cms) {

		cms.register.service('qSearchDataSvc', qSearchDataSvc);

		qSearchDataSvc.$inject = ['AppDefine', '$resource', '$http', 'Utils', '$q'];

		function qSearchDataSvc(AppDefine, $resource, $http, Utils, $q) {
			var apibase = AppDefine.Api.QuickSearch + '/:dest/';

			var qsearchResource = $resource(apibase, { dest: "@dest" }, {
				QuickSearchtAPI: {
					hideOverlay: true, method: 'POST', params: { dest: "QuickSearchReport" }, headers: cms.EncryptHeader(), interceptor: { response: function ( response ) { return response; } }}
				, QSearchDetail: {
						method: 'get', id: { dest: "id" }, headers: cms.EncryptHeader(), interceptor: { response: function ( response ) { return response; } }}
			});

			var qsearchRptSvc = {
				QuickSearchReport: quicksearchReport,
				quicksearchDetail: quicksearchDetail
			};

			return qsearchRptSvc;
		
			function quicksearchDetail( params, successFn, errorFn ) {
				qsearchResource.QSearchDetail( { id: params } ).$promise.then(
					function ( result ) {
						successFn( cms.GetResponseData( result ) );
					}, function ( error ) {
						errorFn( error );
					} );
			}

			function quicksearchReport(params, successFn, errorFn) {
				qsearchResource.QuickSearchtAPI(params).$promise.then(
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
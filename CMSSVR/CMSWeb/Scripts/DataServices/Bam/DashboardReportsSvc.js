(function () {
	define(['cms'], function (cms) {

	    cms.register.service('DashboardReportsSvc', DashboardReportsSvc);

	    DashboardReportsSvc.$inject = ['AppDefine', '$resource', '$http', 'Utils', '$q'];

	    function DashboardReportsSvc(AppDefine, $resource, $http, Utils, $q) {
			var apibase = AppDefine.Api.DashboardReports + '/:dest/';

			var DashboardReportsSrc = $resource(apibase, { dest: "@dest" }, {
			    GetMetricDetail: { method: 'GET', params: { dest: "GetMetricDetail" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
			    Export: { method: 'POST', params: { dest: "Export" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
			    //DownloadExcelFile: { method: 'GET', params: { dest: "DownloadExcelFile" }, interceptor: { response: function (response) { return response; } } },
			    //GetMetricDetail: { method: 'POST', params: { dest: "GetMetricDetail" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
			    GetMetricSumary: { method: 'POST', params: { dest: "GetMetricSumary" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
			    GetNormalizeBySite: { method: 'GET', params: { dest: "GetNormalizeBySite" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
			    UpdateNormalize: { method: 'POST', params: { dest: "UpdateNormalize" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				GetDriveThroughData: { method: 'GET', params: { dest: "GetReportDriveThrough" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } }
			});

			var dashboardReportSvc = {
			    Export: Export,
			    DownloadExcelFile:DownloadExcelFile,
			    getMetricSumary:getMetricSumary,
			    GetMetricDetail: getMetricDetail,
			    GetNormalizeBySite: getNormalizeBySite,
			    UpdateNormalize: updateNormalize,
				GetDriveThroughData: getDriveThroughData
	    };

        
		function updateNormalize(params, successFn, errorFn) {
		    var def = $q.defer();
		    DashboardReportsSrc.UpdateNormalize(params).$promise.then(
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

		function getNormalizeBySite(params, successFn, errorFn) {
	        var def = $q.defer();
	        DashboardReportsSrc.GetNormalizeBySite(params).$promise.then(
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
			function getDriveThroughData(params, successFn, errorFn) {
				var def = $q.defer();
				DashboardReportsSrc.GetDriveThroughData(params).$promise.then(
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

			function getMetricDetail(params, successFn, errorFn) {
				var def = $q.defer();
				DashboardReportsSrc.GetMetricDetail(params).$promise.then(
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

			return dashboardReportSvc;

			function getMetricSumary(params, successFn, errorFn) {
			    var def = $q.defer();
			    DashboardReportsSrc.GetMetricSumary(params).$promise.then(
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
	function Export(params, successFn, errorFn) {
			    var def = $q.defer();
			    DashboardReportsSrc.Export(params).$promise.then(
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
			function DownloadExcelFile(params) {
			    window.location.assign(AppDefine.Api.DashboardReports + "/DownloadExcelFile?filepath=" + params);
			}
		}
	});

}
)();
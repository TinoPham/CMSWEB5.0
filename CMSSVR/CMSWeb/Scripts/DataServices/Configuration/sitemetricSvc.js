(function () {
	define(['cms'], function (cms) {

		cms.register.service('SiteMetricSvc', SiteMetricSvc);
		SiteMetricSvc.$inject = ['AppDefine', 'Cookies', '$resource', '$http', '$q'];

		function SiteMetricSvc(AppDefine, Cookies, $resource, $http, $q) {

			var apibase = AppDefine.Api.SiteMetric + '/:dest';
			var SiteMetric = $resource(apibase, { dest: "@dest" }, {
				'GetAllMetric': { method: 'GET', headers: cms.EncryptHeader(), isArray: false, params: { dest: "GetAllMetric" }, interceptor: { response: function (response) { return response; } } }
				, 'GetMetricChild': { method: 'GET', headers: cms.EncryptHeader(), isArray: false, params: { dest: "GetMetricChild" }, interceptor: { response: function (response) { return response; } } }
                , 'AddMetricSite': { method: 'POST', headers: cms.EncryptHeader(), params: { dest: "AddMetricSite" }, interceptor: { response: function (response) { return response; } } }
                , 'DeleteMetricSite': { method: 'POST', headers: cms.EncryptHeader(), params: { dest: "DeleteMetricSite" }, interceptor: { response: function (response) { return response; } } }
			});

			var MetricModel = {
					MListID: 0
					, MetricName: null
					, MListEditedDate: new Date()
					, ParentID: null
					, MetricMeasure: null
					, CreateBy: null
					, isDefault: false
					, MetricSiteList: []
				};

			var GetAllMetric = function () {
				var def = $q.defer();

				SiteMetric.GetAllMetric().$promise.then(
                    function (response) {
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

			var GetMetricChild = function (parentId) {
				var def = $q.defer();

				SiteMetric.GetMetricChild({ parentID: parentId }).$promise.then(
                    function (response) {
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

			var AddMetricSite = function (metricModel) {
				var def = $q.defer();

				SiteMetric.AddMetricSite(metricModel).$promise.then(
                    function (response) {
                    	//var data = cms.GetResponseData(response);
                    	def.resolve(response.data);
                    },
                    function (error) {
                    	//console.log(error);
                    	def.reject(error);
                    }
                );
				return def.promise;
			}

			var DeleteMetricSite = function (metricIds) {
				var def = $q.defer();

				SiteMetric.DeleteMetricSite(metricIds).$promise.then(
                    function (response) {
                    	var data = def.resolve(response.data);
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
				GetAllMetric: GetAllMetric,
				GetMetricChild: GetMetricChild,
				AddMetricSite: AddMetricSite,
				DeleteMetricSite: DeleteMetricSite,
				MetricModel: function () { return MetricModel; }
			};
		}
	});
}
)();

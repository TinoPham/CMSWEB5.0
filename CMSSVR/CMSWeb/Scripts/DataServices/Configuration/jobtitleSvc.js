(function () {
	define(['cms'], function (cms) {
		cms.register.service('JobTitleSvc', JobTitleSvc);
		JobTitleSvc.$inject = ['AppDefine', 'Cookies', '$resource', '$http', '$q', 'colorSvc'];

		function JobTitleSvc(AppDefine, Cookies, $resource, $http, $q, colorSvc) {

			var apibase = AppDefine.Api.JobTitle + '/:dest';
			var JobTitle = $resource(apibase, { dest: "@dest" }, {
				'GetJobTitle': { method: 'GET', headers: cms.EncryptHeader(), isArray: false, params: { dest: "JobTitle" }, interceptor: { response: function (response) { return response; } } }
                , 'AddJobTitle': { method: 'POST', headers: cms.EncryptHeader(), params: { dest: "JobTitle" }, interceptor: { response: function (response) { return response; } } }
                , 'DeleteJobTitle': { method: 'POST', headers: cms.EncryptHeader(), params: { dest: "DeleteJobTitle" }, interceptor: { response: function (response) { return response; } } }
			});

			var JobTitleModel = InitJobTitleModel();
			function InitJobTitleModel() {
				return {
					PositionID: 0
                , PositionName: null
                , CreatedBy: null
                , CreatedDate: Date.now()
                , Description: null
                , UUsername: null
                , Color: colorSvc.getdefaultColor() //#fff - white
				};
			}

			var GetJobTitle = function () {
				var def = $q.defer();
				JobTitle.GetJobTitle().$promise.then(
                    function (response) {
                    	def.resolve(response.data.Data);
                    },
                    function (error) {
                    	console.log(error);
                    	def.reject(error);
                    }
                );
				return def.promise;
			};

			var AddJobTitle = function (jobmodel) {
				var def = $q.defer();

				JobTitle.AddJobTitle(jobmodel).$promise.then(
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

			var DeleteJobTitle = function (listJobID) {
				var def = $q.defer();

				JobTitle.DeleteJobTitle(listJobID).$promise.then(
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

			return {
				GetJobTitle: GetJobTitle,
				AddJobTitle: AddJobTitle,
				DeleteJobTitle: DeleteJobTitle,
				JobTitleModel: function () { return JobTitleModel; }
			};

		}

	});

}
)();

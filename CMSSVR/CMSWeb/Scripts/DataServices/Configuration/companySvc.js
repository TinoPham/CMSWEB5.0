(function () {
	define(['cms'], function (cms) {
		cms.register.service('CompanySvc', CompanySvc);
		CompanySvc.$inject = ['AppDefine', 'Cookies', '$resource', '$http', '$q'];

		function CompanySvc(AppDefine, Cookies, $resource, $http, $q) {

			var apibase = AppDefine.Api.Company + '/:dest/?';
			var Company = $resource(apibase, { dest: "@dest" }, {
				'GetCompanyInfo': { method: 'GET', headers: cms.EncryptHeader(), isArray: false, params: { dest: "GetCompanyInfo" }, interceptor: { response: function (response) { return response; } } }
                , 'UpdateCompanyInfo': { method: 'POST', headers: cms.EncryptHeader(), params: { dest: "UpdateCompanyInfo" }, interceptor: { response: function (response) { return response; } } }
			});

			var CompanyModel = InitCompanyModel();
			function InitCompanyModel() {
				return {
					CompanyID: 0
                    , CompanyName: null
                    , CompanyLogo: null
                    , UpdateDate: new Date()
				};
			}

			var GetCompanyInfo = function (caseType) {
				var def = $q.defer();
				Company.GetCompanyInfo().$promise.then(
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

			var UpdateCompanyInfo = function (models) {
				var def = $q.defer();
				Company.UpdateCompanyInfo(models).$promise.then(
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
			};


			return {
				GetCompanyInfo: GetCompanyInfo,
				UpdateCompanyInfo: UpdateCompanyInfo,
				CompanyModel: function () { return CompanyModel; }
			};
		}
	});
}
)();

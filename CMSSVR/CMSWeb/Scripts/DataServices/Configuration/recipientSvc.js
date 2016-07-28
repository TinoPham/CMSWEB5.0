(function () {
	define(['cms'], function (cms) {
		cms.register.service('RecipientSvc', RecipientSvc);
		RecipientSvc.$inject = ['AppDefine', 'Cookies', '$resource', '$http', '$q'];

		function RecipientSvc(AppDefine, Cookies, $resource, $http, $q) {

			var apibase = AppDefine.Api.Recipient + '/:dest';
			var Recipient = $resource(apibase, { dest: "@dest" }, {
				'GetRecipient': { method: 'GET', headers: cms.EncryptHeader(), isArray: false, params: { dest: "GetRecipient" }, interceptor: { response: function (response) { return response; } } }
                , 'AddRecipient': { method: 'POST', headers: cms.EncryptHeader(), params: { dest: "AddRecipient" }, interceptor: { response: function (response) { return response; } } }
                , 'DeleteRecipient': { method: 'POST', headers: cms.EncryptHeader(), params: { dest: "DeleteRecipient" }, interceptor: { response: function (response) { return response; } } }
			});

			var GetRecipient = function () {
				var def = $q.defer();
				Recipient.GetRecipient().$promise.then(
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

			var AddRecipient = function (recipientmodel) {
				var def = $q.defer();

				Recipient.AddRecipient(recipientmodel).$promise.then(
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

			var DeleteRecipient = function (recipientID) {
				var def = $q.defer();

				Recipient.DeleteRecipient(recipientID).$promise.then(
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

			return {
				GetRecipient: GetRecipient,
				AddRecipient: AddRecipient,
				DeleteRecipient: DeleteRecipient
			};

		}

	});

}
)();

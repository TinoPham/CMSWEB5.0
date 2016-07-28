(function () {
	define(['cms'], function (cms) {

		cms.register.service('GoalTypeSvc', GoalTypeSvc);
		GoalTypeSvc.$inject = ['AppDefine', 'Cookies', '$resource', '$http', '$q'];
		function GoalTypeSvc(AppDefine, Cookies, $resource, $http, $q) {

	        var apibase = AppDefine.Api.GoalType + '/:dest/';
			var GoalType = $resource(apibase, { dest: "@dest" }, {
	            'GetAllGoal': { method: 'GET', headers: cms.EncryptHeader(), isArray: false, params: { dest: "Goals", detail:true }, interceptor: { response: function ( response ) { return response; } } }
                , 'GetGoals': { method: 'GET', headers: cms.EncryptHeader(), isArray: false, params: { dest: "Goals", detail: false }, interceptor: { response: function ( response ) { return response; } } }
                , 'GetAllGoalType': { method: 'GET', headers: cms.EncryptHeader(), isArray: false, params: { dest: "GoalTypes" }, interceptor: { response: function ( response ) { return response; } } }
                , 'AddGoalType': { method: 'POST', headers: cms.EncryptHeader(), params: { dest: "AddGoalType" }, interceptor: { response: function (response) { return response; } } }
                , 'DeleteGoalType': { method: 'POST', headers: cms.EncryptHeader(), params: { dest: "DeleteGoalType" }, interceptor: { response: function (response) { return response; } } }
			});

			var GetAllGoal = function () {
				var def = $q.defer();

				GoalType.GetAllGoal().$promise.then(
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
			var GetSimpleGoals = function (sucessfunc, errorfunc) {
				GoalType.GetGoals().$promise.then(
                    function (response) {
                    	var data = cms.GetResponseData(response);
                    	sucessfunc(data);
                    	//def.resolve( data );
                    },
                    function (error) {
                    	console.log(error);
                    	errorfunc(error);
                    	// def.reject( error );
                    }
                );
			};
			

			var GetAllGoalType = function () {
				var def = $q.defer();

				GoalType.GetAllGoalType().$promise.then(
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

			var AddGoalType = function (goalModel) {
				var def = $q.defer();

				GoalType.AddGoalType(goalModel).$promise.then(
                    function (response) {
                    	var data = response.data; //cms.GetResponseData(response);
                    	def.resolve(data);
                    },
                    function (error) {
                    	console.log(error);
                    	def.reject(error);
                    }
                );
				return def.promise;
			};

			var DeleteGoalType = function (goalID) {
				var def = $q.defer();

				GoalType.DeleteGoalType(goalID).$promise.then(
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

			var GoalTypes = [];

			return {
				GetAllGoal: GetAllGoal,
				GetSimpleGoals: GetSimpleGoals,
				GetAllGoalType: GetAllGoalType,
				AddGoalType: AddGoalType,
				DeleteGoalType: DeleteGoalType,
				GoalTypes: GoalTypes
			};

		}

	});

}
)();

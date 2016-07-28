(function () {
	define(['cms'], function (cms) {

		cms.register.service('DistributionSvc', DistributionSvc);

		DistributionSvc.$inject = ['AppDefine', '$resource', '$http', 'Utils', '$q'];

		function DistributionSvc(AppDefine, $resource, $http, Utils, $q) {
			var apibase = AppDefine.Api.Distribution + '/:dest/';

			var DistributionSrc = $resource(apibase, { dest: "@dest" }, {
				GetReportData: { method: 'GET', params: { dest: "GetReportData" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				GetArea: { method: 'POST', params: { dest: "GetArea" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				AddQueue: { method: 'POST', params: { dest: "AddQueue" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				//DeleteQueue: { method: 'POST', params: { dest: "DeleteQueue" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } }
				applyStore: { method: 'POST', params: { dest: "applyStore" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				getDataHeatMap: { method: 'GET', params: { dest: "GetDataHeatMap" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				getlistChannels: { method: 'GET', params: { dest: "GetListChannels" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				checkExistsImage: { method: 'GET', params: { dest: "CheckExistsImage" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				InsertImage: { method: 'POST', params: { dest: "InsertImage" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				getDataScheduleTasks: { method: 'GET', params: { dest: "GetDataScheduleTasks" }, header: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				InsertDataScheduleTasks: { method: 'POST', params: { dest: "InsertDataScheduleTasks" }, header: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				UpdateDataScheduleTasks: { method: 'POST', params: { dest: "UpdateDataScheduleTasks" }, header: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				DeleteDataScheduleTasks: { method: 'POST', params: { dest: "DeleteDataScheduleTasks" }, header: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } }
			});

			var distributionSvc = {
				GetReportData: getReportData,
				GetArea: getArea,
				AddQueue: addQueue,
				applyStore: applyStore,
				getDataHeatMap: getDataHeatMap,
				getlistChannels: getlistChannels,
				CheckExistsImage: CheckExistsImage,
				InsertImage: InsertImage,
			    //DeleteQueue: deleteQueue,
				getDataScheduleTasks: GetDataScheduleTasks,
				InsertDataScheduleTasks: InsertDataScheduleTasks,
				UpdateDataScheduleTasks: UpdateDataScheduleTasks,
			    DeleteDataScheduleTasks: DeleteDataScheduleTasks
			};

			function DeleteDataScheduleTasks(params, successFn, errorFn) {
			    DistributionSrc.DeleteDataScheduleTasks(params).$promise.then(
                    function (response) {
                        var data = cms.GetResponseData(response);
                        successFn(data);
                    }, function (error) {
                        errorFn(error);
                    }
                );
			}

			function UpdateDataScheduleTasks(params, successFn, errorFn) {
			    DistributionSrc.UpdateDataScheduleTasks(params).$promise.then(
                    function (response) {
                        var data = cms.GetResponseData(response);
                        successFn(data);
                    }, function (error) {
                        console.log(error);
                    }
                );
			}

			function InsertDataScheduleTasks(params, successFn, errorFn) {
			    DistributionSrc.InsertDataScheduleTasks(params).$promise.then(
                    function (response) {
                        var data = cms.GetResponseData(response);
                        successFn(data);
                    }, function (error) {
                        console.log(error);
                    }
                );
			}

			function GetDataScheduleTasks(params, successFn, errorFn) {
			    DistributionSrc.getDataScheduleTasks(params).$promise.then(
                    function (response) {
                        var data = cms.GetResponseData(response);
                        successFn(data);
                    }, function (error) {
                        console.log(error);
                    }
                   );
			}

			function InsertImage(params, successFn, errorFn) {
			    DistributionSrc.InsertImage(params).$promise.then(
                    function (response) {
                        var data = cms.GetResponseData(response);
                        successFn(data);
                    }, function (error) {
                        console.log(error);
                    }
                );
			}

			function CheckExistsImage(params, successFn, errorFn) {
			    
			    DistributionSrc.checkExistsImage(params).$promise.then(
                    function (response) {
                        var data = cms.GetResponseData(response);
                        successFn(data);
                        //def.resolve(data);
                    }, function (error) {
                        console.log(error);
                        //def.reject(error);
                    }
                );
			    //return def.promise;
			}

			function getlistChannels(params, successFn, errorFn) {
			    DistributionSrc.getlistChannels(params).$promise.then(
					function (result) {
					    successFn(cms.GetResponseData(result));
					}
					, function (error) {
					    errorFn(error);
					});
			}

			function getDataHeatMap(params, successFn, errorFn) {
			    //var def = $q.defer();
			    DistributionSrc.getDataHeatMap(params).$promise.then(
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
			function getReportData(params, successFn, errorFn) {
				//var def = $q.defer();
				DistributionSrc.GetReportData(params).$promise.then(
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
			function getArea(params, successFn, errorFn) {
				//var def = $q.defer();
				DistributionSrc.GetArea(params).$promise.then(
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

			function addQueue(params, successFn, errorFn) {
				//var def = $q.defer();
				DistributionSrc.AddQueue(params).$promise.then(
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
			function applyStore(params, successFn, errorFn) {
				//var def = $q.defer();
				DistributionSrc.applyStore(params).$promise.then(
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
			//function deleteQueue(params, successFn, errorFn) {
			//	//var def = $q.defer();
			//	DistributionSrc.DeleteQueue(params).$promise.then(
			//		function (result) {
			//			//def.resolve(result);
			//			successFn(cms.GetResponseData(result));
			//		}
			//		, function (error) {
			//			//def.reject(error);
			//			errorFn(error);
			//		});
			//	//return def.promise;
			//}
			return distributionSvc;
		}
	});

}
)();
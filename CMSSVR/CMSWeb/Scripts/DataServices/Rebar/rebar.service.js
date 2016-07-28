(function () {
	define(['cms'], function (cms) {

		cms.register.service('rebarDataSvc', rebarDataSvc);

		rebarDataSvc.$inject = ['AppDefine', '$resource', '$http', 'Utils', '$q'];

		function rebarDataSvc(AppDefine, $resource, $http, Utils, $q) {
			var apibase = AppDefine.Api.Rebar + '/:dest/';
			
			var rebarReport = $resource(apibase, { dest: "@dest" }, {
				GetTransactionInfo: { method: 'GET', params: { dest: "GetTransactionInfo" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				GetTransactionViewer: { method: 'GET', params: { dest: "GetTransactionViewer" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				GetAhocViewer: { method: 'GET', params: { dest: "GetAhocViewer" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				GetTransactionTypes: { method: 'GET', params: { dest: "GetTransactionTypes" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				GetTaxsList: { method: 'GET', params: { dest: "GetTaxsList" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				GetEmployeeRisks: { method: 'POST', params: { dest: "GetEmployeeRisks" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				GetSitesRisks: { method: 'POST', params: { dest: "GetSitesRisks" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				GetWeekAtGlanceSummary: { method: 'POST', params: { dest: "GetWeekAtGlanceSummary" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				GeTransactionFilterPagings: { method: 'POST', params: { dest: "GeTransactionFilterPagings" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				GetBoxWidgets: { method: 'POST', params: { dest: "GetBoxWidgets" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				GetTransacByEmployee: { method: 'POST', params: { dest: "GetTransacByEmployee" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				GetEmployerInfoBySite: { method: 'POST', params: { dest: "GetEmployerInfoBySite" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				GetTransactPaymentTypes: { method: 'POST', params: { dest: "GetTransactPaymentTypes" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				GetTransacDetailByPaymentType: { method: 'POST', params: { dest: "GetTransacDetailByPaymentType" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				GetPaymentListAPI: { method: 'GET', params: { dest: "GetPaymentList" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				GetRegisterListAPI: { method: 'GET', params: { dest: "GetRegisterList" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				GetOperatorListAPI: { method: 'GET', params: { dest: "GetOperatorList" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				GetDescriptionListAPI: { method: 'GET', params: { dest: "GetDescriptionList" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				SaveTransactionFlagTypes: { method: 'POST', params: { dest: "SaveTransactionFlagTypes" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				AddTransactionFlagType: { method: 'POST', params: { dest: "AddTransactionFlagType" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				UpdateTransactionFlagType: { method: 'POST', params: { dest: "UpdateTransactionFlagType" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				DelTransactionFlagType: { method: 'POST', params: { dest: "DelTransactionFlagType" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				SaveTransactionNotes: { method: 'POST', params: { dest: "SaveTransactionNotes" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				FilterPaymentAPI: { method: 'GET', params: { dest: "FilterPayment" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				FilterRegisterAPI: { method: 'GET', params: { dest: "FilterRegister" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				FilterOperatorAPI: { method: 'GET', params: { dest: "FilterOperator" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				FilterDescriptionAPI: { method: 'GET', params: { dest: "FilterDescription" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				GetDescriptionByIdAPI: { method: 'GET', params: { dest: "GetDescriptionById" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				GetColumnOpionAPI: { method: 'POST', params: { dest: "GetColumnOpion" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				GetTerminalAPI: { method: 'GET', params: { dest: "GetTerminal" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				GetStoreAPI: { method: 'GET', params: { dest: "GetStore" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				GetCheckIDAPI: { method: 'GET', params: { dest: "GetCheckID" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				GetTransWOCust: { method: 'POST', params: { dest: "GetTransWOCust" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				GetCustsWOTran: { method: 'POST', params: { dest: "GetCustsWOTran" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				GetCarsWOTran: { method: 'POST', params: { dest: "GetCarsWOTran" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				GetIOPCCustomer: { method: 'POST', params: { dest: "GetIOPCCustomer" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				GetIOPCCar: { method: 'POST', params: { dest: "GetIOPCCar" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } }
			});

			var rebarReportSvc = {
				getTransactionInfo: GetTransactionInfo,
				getTransactionViewer: GetTransactionViewer,
				getAhocViewer: GetAhocViewer,
				getTransactionTypes: GetTransactionTypes,
				getTaxsList: GetTaxsList,
				getEmployeeRisks: GetEmployeeRisks,
				getSitesRisks: GetSitesRisks,
				getWeekAtGlanceSummary: GetWeekAtGlanceSummary,
				geTransactionFilterPagings: GeTransactionFilterPagings,
				getBoxWidgets: GetBoxWidgets,
				getTransacByEmployee: GetTransacByEmployee,
				getEmployerInfoBySite: GetEmployerInfoBySite,
				getTransactPaymentTypes: GetTransactPaymentTypes,
				getTransacDetailByPaymentType: GetTransacDetailByPaymentType,
				GetPaymentList: getPaymentList,
				GetRegisterList: getRegisterList,
				GetOperatorList: getOperatorList,
				GetDescriptionList: getDescriptionList,
				saveTransactionNotes: SaveTransactionNotes,
				saveTransactionFlagTypes: SaveTransactionFlagTypes,
				addTransactionFlagType: AddTransactionFlagType,
				updateTransactionFlagType: UpdateTransactionFlagType,
				delTransactionFlagType: DelTransactionFlagType,
				filterPayment: filterPayment,
				filterRegister: filterRegister,
				filterOperator: filterOperator,
				filterDescription: filterDescription,
				getDescriptionById: getDescriptionById,
				getColumnOpion: getColumnOpion,
				getTerminal: getTerminal,
				getStore: getStore,
				getCheckID: getCheckID,
				getTransWOCust: GetTransWOCust,
				getCustsWOTran: GetCustsWOTran,
				getCarsWOTran: GetCarsWOTran,
				getIOPCCustomer: GetIOPCCustomer,
				getIOPCCar: GetIOPCCar
			};

			return rebarReportSvc;

			function GetTransactionViewer(params, successFn, errorFn) {
				rebarReport.GetTransactionViewer(params).$promise.then(
					function (result) {
						successFn(cms.GetResponseData(result));
					}, function (error) {
						errorFn(error);
					});
			}

			function GetAhocViewer(params, successFn, errorFn) {
			    rebarReport.GetAhocViewer(params).$promise.then(
					function (result) {
					    successFn(cms.GetResponseData(result));
					}, function (error) {
					    errorFn(error);
					});
			}

			function GetTaxsList(successFn, errorFn) {
				rebarReport.GetTaxsList().$promise.then(
					function (result) {
						successFn(cms.GetResponseData(result));
					}, function (error) {
						errorFn(error);
					});
			}

			function GetTransactionTypes(successFn, errorFn) {
				rebarReport.GetTransactionTypes().$promise.then(
					function (result) {
						successFn(cms.GetResponseData(result));
					}, function (error) {
						errorFn(error);
					});
			}

			function AddTransactionFlagType(params, successFn, errorFn) {
				rebarReport.AddTransactionFlagType(params).$promise.then(
					function (result) {
						successFn(cms.GetResponseData(result));
					}, function (error) {
						errorFn(error);
					});
			}

			function UpdateTransactionFlagType(params, successFn, errorFn) {
				rebarReport.UpdateTransactionFlagType(params).$promise.then(
					function (result) {
						successFn(cms.GetResponseData(result));
					}, function (error) {
						errorFn(error);
					});
			}

			function SaveTransactionNotes(params, successFn, errorFn) {
				rebarReport.SaveTransactionNotes(params).$promise.then(
					function (result) {
						successFn(cms.GetResponseData(result));
					}, function (error) {
						errorFn(error);
					});
			}

			function DelTransactionFlagType(params, successFn, errorFn) {
				rebarReport.DelTransactionFlagType(params).$promise.then(
					function (result) {
						successFn(cms.GetResponseData(result));
					}, function (error) {
						errorFn(error);
					});
			}

			function SaveTransactionFlagTypes(params, successFn, errorFn) {
				rebarReport.SaveTransactionFlagTypes(params).$promise.then(
					function (result) {
						successFn(cms.GetResponseData(result));
					}, function (error) {
						errorFn(error);
					});
			}

			function GetEmployeeRisks(params, successFn, errorFn) {
				rebarReport.GetEmployeeRisks(params).$promise.then(
					function (result) {
						successFn(cms.GetResponseData(result));
					}, function (error) {
						errorFn(error);
					});
			}

			function GetWeekAtGlanceSummary(params, successFn, errorFn) {
				rebarReport.GetWeekAtGlanceSummary(params).$promise.then(
					function (result) {
						successFn(cms.GetResponseData(result));
					}, function (error) {
						errorFn(error);
					});
			}

			function GetSitesRisks(params, successFn, errorFn) {
				rebarReport.GetSitesRisks(params).$promise.then(
					function (result) {
						successFn(cms.GetResponseData(result));
					}, function (error) {
						errorFn(error);
					});
			}

			function GetTransWOCust(params, successFn, errorFn) {
				rebarReport.GetTransWOCust(params).$promise.then(
					function (result) {
						successFn(cms.GetResponseData(result));
					}, function (error) {
						errorFn(error);
					});
			}
			function GetCustsWOTran(params, successFn, errorFn) {
				rebarReport.GetCustsWOTran(params).$promise.then(
					function (result) {
						successFn(cms.GetResponseData(result));
					}, function (error) {
						errorFn(error);
					});
			}
			function GetCarsWOTran(params, successFn, errorFn) {
				rebarReport.GetCarsWOTran(params).$promise.then(
					function (result) {
						successFn(cms.GetResponseData(result));
					}, function (error) {
						errorFn(error);
					});
			}
			function GetIOPCCustomer(params, successFn, errorFn) {
				rebarReport.GetIOPCCustomer(params).$promise.then(
					function (result) {
						successFn(cms.GetResponseData(result));
					}, function (error) {
						errorFn(error);
					});
			}
			function GetIOPCCar(params, successFn, errorFn) {
				rebarReport.GetIOPCCar(params).$promise.then(
					function (result) {
						successFn(cms.GetResponseData(result));
					}, function (error) {
						errorFn(error);
					});
			}

			function GeTransactionFilterPagings(params, successFn, errorFn) {
				rebarReport.GeTransactionFilterPagings(params).$promise.then(
					function (result) {
						successFn(cms.GetResponseData(result));
					}, function (error) {
						errorFn(error);
					});
			}

			function GetTransactionInfo(params, successFn, errorFn) {
				rebarReport.GetTransactionInfo(params).$promise.then(
					function (result) {
						successFn(cms.GetResponseData(result));
					}, function (error) {
						errorFn(error);
					});
			}

			function GetTransactPaymentTypes(params, successFn, errorFn) {
				rebarReport.GetTransactPaymentTypes(params).$promise.then(
					function (result) {
						successFn(cms.GetResponseData(result));
					}, function (error) {
						errorFn(error);
					});
			}

			function GetTransacDetailByPaymentType(params, successFn, errorFn) {
				rebarReport.GetTransacDetailByPaymentType(params).$promise.then(
					function (result) {
						successFn(cms.GetResponseData(result));
					}, function (error) {
						errorFn(error);
					});
			}

			function GetEmployerInfoBySite(params, successFn, errorFn) {
				rebarReport.GetEmployerInfoBySite(params).$promise.then(
					function (result) {
						successFn(cms.GetResponseData(result));
					}, function (error) {
						errorFn(error);
					});
			}

			function GetTransacByEmployee(params, successFn, errorFn) {
				rebarReport.GetTransacByEmployee(params).$promise.then(
					function (result) {
						successFn(cms.GetResponseData(result));
					}, function (error) {
						errorFn(error);
					});
			}

			function GetBoxWidgets(params, successFn, errorFn) {
				rebarReport.GetBoxWidgets(params).$promise.then(
					function (result) {
						successFn(cms.GetResponseData(result));
					}, function (error) {
						errorFn(error);
					});
			}

			function getPaymentList(params, successFn, errorFn) {
				rebarReport.GetPaymentListAPI(params).$promise.then(
					function (result) {
						successFn(cms.GetResponseData(result));
					}, function (error) {
						errorFn(error);
					});
			}

			function getRegisterList(params, successFn, errorFn) {
				rebarReport.GetRegisterListAPI(params).$promise.then(
					function (result) {
						successFn(cms.GetResponseData(result));
					}, function (error) {
						errorFn(error);
					});
			}

			function getOperatorList(params, successFn, errorFn) {
				rebarReport.GetOperatorListAPI(params).$promise.then(
					function (result) {
						successFn(cms.GetResponseData(result));
					}, function (error) {
						errorFn(error);
					});
			}

			function getDescriptionList(params, successFn, errorFn) {
				rebarReport.GetDescriptionListAPI(params).$promise.then(
					function (result) {
						successFn(cms.GetResponseData(result));
					}, function (error) {
						errorFn(error);
					});
			}

			function filterPayment(params, successFn, errorFn) {
				rebarReport.FilterPaymentAPI(params).$promise.then(
					function (result) {
						successFn(cms.GetResponseData(result));
					}, function (error) {
						errorFn(error);
					});
				}

			function filterRegister(params, successFn, errorFn) {
				rebarReport.FilterRegisterAPI(params).$promise.then(
					function (result) {
						successFn(cms.GetResponseData(result));
					}, function (error) {
						errorFn(error);
					});
			}

			function filterOperator(params, successFn, errorFn) {
				rebarReport.FilterOperatorAPI(params).$promise.then(
					function (result) {
						successFn(cms.GetResponseData(result));
					}, function (error) {
						errorFn(error);
					});
			}

			function filterDescription(params, successFn, errorFn) {
				rebarReport.FilterDescriptionAPI(params).$promise.then(
					function (result) {
						successFn(cms.GetResponseData(result));
					}, function (error) {
						errorFn(error);
					});
			}

			function getDescriptionById(params, successFn, errorFn) {
				rebarReport.GetDescriptionByIdAPI(params).$promise.then(
					function (result) {
						successFn(cms.GetResponseData(result));
					}, function (error) {
						errorFn(error);
					});
			}

			function getColumnOpion(params, successFn, errorFn) {
				rebarReport.GetColumnOpionAPI(params).$promise.then(
					function (result) {
						successFn(cms.GetResponseData(result));
					}, function (error) {
						errorFn(error);
					});
			}
			
			function getTerminal(successFn, errorFn) {
				rebarReport.GetTerminalAPI().$promise.then(
					function (result) {
						successFn(cms.GetResponseData(result));
					}, function (error) {
						errorFn(error);
					});
			}

			function getStore(successFn, errorFn) {
				rebarReport.GetStoreAPI().$promise.then(
					function (result) {
						successFn(cms.GetResponseData(result));
					}, function (error) {
						errorFn(error);
					});
			}

			function getCheckID(successFn, errorFn) {
				rebarReport.GetCheckIDAPI().$promise.then(
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
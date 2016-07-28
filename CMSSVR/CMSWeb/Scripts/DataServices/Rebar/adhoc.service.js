(function () {
    define(['cms'], function (cms) {

        cms.register.service('adhocDataSvc', adhocDataSvc);

        adhocDataSvc.$inject = ['AppDefine', '$resource', '$http', 'Utils', '$q'];

        function adhocDataSvc(AppDefine, $resource, $http, Utils, $q) {
            var apibase = AppDefine.Api.Adhoc + '/:dest/';

            var rebarReport = $resource(apibase, { dest: "@dest" }, {
                GetAdhocs: { method: 'GET', params: { dest: "GetAdhocs" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
                GetAhocReportColumn: { method: 'GET', params: { dest: "GetAhocReportColumn" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
                GetAdhocReportById: { method: 'GET', params: { dest: "GetAdhocReportById" }, headers: cms.EncryptHeader(), interceptor: { response: function(response) { return response; } } },
                GetAdhocFoldersById: { method: 'GET', params: { dest: "GetAdhocFoldersById" }, headers: cms.EncryptHeader(), interceptor: { response: function(response) { return response; } } },
                DeleteAdhocReport: { method: 'GET', params: { dest: "DeleteAdhocReport" }, headers: cms.EncryptHeader(), interceptor: { response: function(response) { return response; } } },
                DeleteAdhocReportFolder: { method: 'GET', params: { dest: "DeleteAdhocReportFolder" }, headers: cms.EncryptHeader(), interceptor: { response: function(response) { return response; } } },
                UpdateAdhocReport: { method: 'POST', params: { dest: "UpdateAdhocReport" }, headers: cms.EncryptHeader(), interceptor: { response: function(response) { return response; } } },
                AddAdhocReport: { method: 'POST', params: { dest: "AddAdhocReport" }, headers: cms.EncryptHeader(), interceptor: { response: function(response) { return response; } } },
                UpdateAdhocReportFolder: { method: 'POST', params: { dest: "UpdateAdhocReportFolder" }, headers: cms.EncryptHeader(), interceptor: { response: function(response) { return response; } } },
                AddAdhocReportFolder: { method: 'POST', params: { dest: "AddAdhocReportFolder" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
                GetCardList: { method: 'GET', params: { dest: "GetCardList" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
                GetCamList: { method: 'GET', params: { dest: "GetCamList" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
                GetShiftList: { method: 'GET', params: { dest: "GetShiftList" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
                GetStoreList: { method: 'GET', params: { dest: "GetStoreList" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
                GetCheckList: { method: 'GET', params: { dest: "GetCheckList" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
                GetTerminalList: { method: 'GET', params: { dest: "GetTerminalList" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
                GetDescList: { method: 'GET', params: { dest: "GetDescList" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
                GetItemList: { method: 'GET', params: { dest: "GetItemList" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
                GetPaymentList: { method: 'GET', params: { dest: "GetPaymentList" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
                GetRegisterList: { method: 'GET', params: { dest: "GetRegisterList" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
                GetOperatorList: { method: 'GET', params: { dest: "GetOperatorList" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
                GetTaxtLists: { method: 'GET', params: { dest: "GetTaxtLists" }, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } }
            });

            var rebarReportSvc = {
                getAdhocs: GetAdhocs,
                getAhocReportColumn: GetAhocReportColumn,
                getAdhocReportById: GetAdhocReportById,
                getAdhocFoldersById: GetAdhocFoldersById,
                deleteAdhocReport: DeleteAdhocReport,
                deleteAdhocReportFolder: DeleteAdhocReportFolder,
                updateAdhocReport: UpdateAdhocReport,
                addAdhocReport: AddAdhocReport,
                updateAdhocReportFolder: UpdateAdhocReportFolder,
                addAdhocReportFolder: AddAdhocReportFolder,
                getCardList: GetCardList,
                getCamList: GetCamList,
                getShiftList: GetShiftList,
                getStoreList: GetStoreList,
                getCheckList: GetCheckList,
                getTerminalList: GetTerminalList,
                getDescList: GetDescList,
                getItemList: GetItemList,
                getPaymentList: GetPaymentList,
                getRegisterList: GetRegisterList,
                getOperatorList: GetOperatorList,
                getTaxtLists: GetTaxtLists
            };

            return rebarReportSvc;

            function GetPaymentList(params, successFn, errorFn) {
                rebarReport.GetPaymentList(params).$promise.then(
                    function (result) {
                        successFn(cms.GetResponseData(result));
                    }, function (error) {
                        errorFn(error);
                    });
            }
            function GetRegisterList(params, successFn, errorFn) {
                rebarReport.GetRegisterList(params).$promise.then(
                    function (result) {
                        successFn(cms.GetResponseData(result));
                    }, function (error) {
                        errorFn(error);
                    });
            }
            function GetOperatorList(params, successFn, errorFn) {
                rebarReport.GetOperatorList(params).$promise.then(
                    function (result) {
                        successFn(cms.GetResponseData(result));
                    }, function (error) {
                        errorFn(error);
                    });
            }
            function GetTaxtLists(params, successFn, errorFn) {
                rebarReport.GetTaxtLists(params).$promise.then(
                    function (result) {
                        successFn(cms.GetResponseData(result));
                    }, function (error) {
                        errorFn(error);
                    });
            }
            function GetDescList(params, successFn, errorFn) {
                rebarReport.GetDescList(params).$promise.then(
                    function (result) {
                        successFn(cms.GetResponseData(result));
                    }, function (error) {
                        errorFn(error);
                    });
            }
            function GetItemList(params, successFn, errorFn) {
                rebarReport.GetItemList(params).$promise.then(
                    function (result) {
                        successFn(cms.GetResponseData(result));
                    }, function (error) {
                        errorFn(error);
                    });
            }
            function GetCardList(params, successFn, errorFn) {
                rebarReport.GetCardList(params).$promise.then(
                    function (result) {
                        successFn(cms.GetResponseData(result));
                    }, function (error) {
                        errorFn(error);
                    });
            }
            function GetCamList(params, successFn, errorFn) {
                rebarReport.GetCamList(params).$promise.then(
                    function (result) {
                        successFn(cms.GetResponseData(result));
                    }, function (error) {
                        errorFn(error);
                    });
            }
            function GetShiftList(params, successFn, errorFn) {
                rebarReport.GetShiftList(params).$promise.then(
                    function (result) {
                        successFn(cms.GetResponseData(result));
                    }, function (error) {
                        errorFn(error);
                    });
            }
            function GetStoreList(params, successFn, errorFn) {
                rebarReport.GetStoreList(params).$promise.then(
                    function (result) {
                        successFn(cms.GetResponseData(result));
                    }, function (error) {
                        errorFn(error);
                    });
            }
            function GetCheckList(params, successFn, errorFn) {
                rebarReport.GetCheckList(params).$promise.then(
                    function (result) {
                        successFn(cms.GetResponseData(result));
                    }, function (error) {
                        errorFn(error);
                    });
            }
            function GetTerminalList(params, successFn, errorFn) {
                rebarReport.GetTerminalList(params).$promise.then(
                    function (result) {
                        successFn(cms.GetResponseData(result));
                    }, function (error) {
                        errorFn(error);
                    });
            }

            function GetAdhocs(params, successFn, errorFn) {
                rebarReport.GetAdhocs(params).$promise.then(
                    function(result) {
                        successFn(cms.GetResponseData(result));
                    }, function(error) {
                        errorFn(error);
                    });
            }

            function GetAhocReportColumn(successFn, errorFn) {
                rebarReport.GetAhocReportColumn().$promise.then(
                    function (result) {
                        successFn(cms.GetResponseData(result));
                    }, function (error) {
                        errorFn(error);
                    });
            }

            function GetAdhocReportById(params, successFn, errorFn) {
                rebarReport.GetAdhocReportById(params).$promise.then(
                    function(result) {
                        successFn(cms.GetResponseData(result));
                    }, function(error) {
                        errorFn(error);
                    });
            }

            function GetAdhocFoldersById(params, successFn, errorFn) {
                rebarReport.GetAdhocFoldersById(params).$promise.then(
                    function(result) {
                        successFn(cms.GetResponseData(result));
                    }, function(error) {
                        errorFn(error);
                    });
            }

            function DeleteAdhocReport(params, successFn, errorFn) {
                rebarReport.DeleteAdhocReport(params).$promise.then(
                    function(result) {
                        successFn(cms.GetResponseData(result));
                    }, function(error) {
                        errorFn(error);
                    });
            }

            function DeleteAdhocReportFolder(params, successFn, errorFn) {
                rebarReport.DeleteAdhocReportFolder(params).$promise.then(
                    function(result) {
                        successFn(cms.GetResponseData(result));
                    }, function(error) {
                        errorFn(error);
                    });
            }

            function UpdateAdhocReport(params, successFn, errorFn) {
                rebarReport.UpdateAdhocReport(params).$promise.then(
                    function(result) {
                        successFn(cms.GetResponseData(result));
                    }, function(error) {
                        errorFn(error);
                    });
            }

            function AddAdhocReport(params, successFn, errorFn) {
                rebarReport.AddAdhocReport(params).$promise.then(
                    function(result) {
                        successFn(cms.GetResponseData(result));
                    }, function(error) {
                        errorFn(error);
                    });
            }

            function UpdateAdhocReportFolder(params, successFn, errorFn) {
                rebarReport.UpdateAdhocReportFolder(params).$promise.then(
                    function(result) {
                        successFn(cms.GetResponseData(result));
                    }, function(error) {
                        errorFn(error);
                    });
            }

            function AddAdhocReportFolder(params, successFn, errorFn) {
                rebarReport.AddAdhocReportFolder(params).$promise.then(
                    function(result) {
                        successFn(cms.GetResponseData(result));
                    }, function(error) {
                        errorFn(error);
                    });
            }
        }
    });

}
)();
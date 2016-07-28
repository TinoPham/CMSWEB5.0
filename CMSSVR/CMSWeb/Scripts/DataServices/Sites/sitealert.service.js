(function() {
    'use strict';
    define(['cms'], sitealertLazyLoad);

    function sitealertLazyLoad(cms) {
        cms.register.service('sitealert.service', sitealertSvc);
        sitealertSvc.$inject = ['$resource', 'cmsBase', 'AppDefine'];

        function sitealertSvc($resource, cmsBase, AppDefine) {
            var url = AppDefine.Api.SiteAlerts;
            var sitealertRe = $resource(url, {}, {
                SiteAlerts: { method: 'GET', url: url + '/SiteAlerts', headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
                SiteAlertsSummary: { method: 'GET', url: url + '/SiteAlertsSummary', headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
                GetSiteAlertByDvrs: { method: 'GET', url: url + '/GetSiteAlertByDvrs', headers: cms.EncryptHeader() },
                GetAlertLastByDvrs: { method: 'GET', url: url + '/GetAlertLastByDvrs', headers: cms.EncryptHeader() },
                GetSensorsAlertByDvrs: { method: 'GET', url: url + '/GetSensorsAlertByDvrs', headers: cms.EncryptHeader() },
                GetAllAlertTypes: { method: 'GET', url: url + '/GetAllAlertTypes', headers: cms.EncryptHeader() },
                GetAlertSensorsDetails: { method: 'GET', url: url + '/GetAlertSensorsDetails', headers: cms.EncryptHeader() },
                IgnoreAlerts: { method: 'POST', url: url + '/IgnoreAlerts', headers: cms.EncryptHeader() },
                GetAlertsConfig: { method: 'GET', url: url + '/GetAlertsConfig', headers: cms.EncryptHeader() },
                GetImagesAlert: { method: 'GET', url: url + '/GetImagesAlert', headers: cms.EncryptHeader() },
                GetEmailSettingByUser: { method: 'GET', url: url + '/GetEmailSettingByUser', headers: cms.EncryptHeader() },
                SaveEmailSettings: { method: 'POST', url: url + '/SaveEmailSettings', headers: cms.EncryptHeader() },
                DeleteEmailSettings: { method: 'POST', url: url + '/DeleteEmailSettings', headers: cms.EncryptHeader() },
                GetAllEmailAlertTypes: { method: 'GET', url: url + '/GetAllEmailAlertTypes', headers: cms.EncryptHeader() }
            });

            return {
                create: function () {
                    var service = {
                        SiteAlerts:SiteAlerts,
                        GetSiteAlertByDvrs: getSiteAlertByDvrs,
                        GetAlertLastByDvrs: getAlertLastByDvrs,
                        GetSensorsAlertByDvrs:getSensorsAlertByDvrs,
                        GetAllAlertTypes: getAllAlertTypes,
                        GetAlertSensorsDetails: getAlertSensorsDetails,
                        SiteAlertsSummary: SiteAlertsSummary,
                        IgnoreAlerts: ignoreAlerts,
                        GetAlertsConfig: GetAlertsConfig,
                        GetImagesAlert: GetImagesAlert,
                        GetEmailSettingByUser: GetEmailSettingByUser,
                        SaveEmailSettings: SaveEmailSettings,
                        DeleteEmailSettings: DeleteEmailSettings,
                        GetAllEmailAlertTypes:GetAllEmailAlertTypes

                    };
                    return service;
                }
            };

            function DeleteEmailSettings(param, successFn, errorFn) {
                sitealertRe.DeleteEmailSettings(param).$promise.then(function (result) {
                    var data = cms.GetResponseData(result);
                    successFn(data);
                }, function (error) {
                    errorFn(error);
                });
            }

            function SaveEmailSettings(param, successFn, errorFn) {
                sitealertRe.SaveEmailSettings(param).$promise.then(function (result) {
                    var data = cms.GetResponseData(result);
                    successFn(data);
                }, function (error) {
                    errorFn(error);
                });
            }

             function GetEmailSettingByUser(id,successFn, errorFn) {
                sitealertRe.GetEmailSettingByUser({'reportID':id}).$promise.then(function (result) {
                    var data = cms.GetResponseData(result);
                    successFn(data);
                }, function (error) {
                    errorFn(error);
                });
            }

            function GetAlertsConfig(successFn, errorFn) {
                sitealertRe.GetAlertsConfig().$promise.then(function (result) {
                    var data = cms.GetResponseData(result);
                    successFn(data);
                }, function (error) {
                    errorFn(error);
                });
            }

            function ignoreAlerts(data, successFn, errorFn) {
                sitealertRe.IgnoreAlerts(data, function (result) {
                    var rlt = cms.GetResponseData(result);
                    successFn(rlt);
                }, function (error) {
                    errorFn(error);
                });
            }

            function SiteAlerts(param, successFn, errorFn) {
                sitealertRe.SiteAlerts(param).$promise.then(function (result) {
                    var data = cms.GetResponseData(result);
                    successFn(data);
                }, function (error) {
                    errorFn(error);
                });
            }

            function SiteAlertsSummary(param, successFn, errorFn) {
                sitealertRe.SiteAlertsSummary(param).$promise.then(function (result) {
                    var data = cms.GetResponseData(result);
                    successFn(data);
                }, function (error) {
                    errorFn(error);
                });
            }

            function getAlertLastByDvrs(param, successFn, errorFn) {
                sitealertRe.GetAlertLastByDvrs(param, function (result) {
                    var data = cms.GetResponseData(result);
                    successFn(data);
                }, function (error) {
                    errorFn(error);
                });
            }

            function getSiteAlertByDvrs(param, successFn, errorFn) {
                sitealertRe.GetSiteAlertByDvrs(param, function (result) {
                    var data = cms.GetResponseData(result);
                    successFn(data);
                }, function (error) {
                    errorFn(error);
                });
            }

            function getSensorsAlertByDvrs(param, successFn, errorFn) {
                sitealertRe.GetSensorsAlertByDvrs(param, function (result) {
                    var data = cms.GetResponseData(result);
                    successFn(data);
                }, function (error) {
                    errorFn(error);
                });
            }

            function getAlertSensorsDetails(param, successFn, errorFn) {
                sitealertRe.GetAlertSensorsDetails(param, function (result) {
                    var data = cms.GetResponseData(result);
                    successFn(data);
                }, function (error) {
                    errorFn(error);
                });
            }


            function GetAllEmailAlertTypes(successFn, errorFn) {
                sitealertRe.GetAllEmailAlertTypes({}, function (result) {
                    var data = cms.GetResponseData(result);
                    successFn(data);
                }, function (error) {
                    errorFn(error);
                });
            }

            function getAllAlertTypes(successFn, errorFn) {
                sitealertRe.GetAllAlertTypes({}, function (result) {
                    var data = cms.GetResponseData(result);
                    successFn(data);
                }, function (error) {
                    errorFn(error);
                });
            }

            function GetImagesAlert(param, successFn, errorFn) {
                var kdvrs = param.kdvrs;
                var channelNo = param.channelNo;
                var timeZone = param.timeZone;
                sitealertRe.GetImagesAlert(param).$promise.then(function (result) {
                    var data = cms.GetResponseData(result);
                    successFn(data);
                }, function (error) {
                    errorFn(error);
                });
            }

        }
    }
})();
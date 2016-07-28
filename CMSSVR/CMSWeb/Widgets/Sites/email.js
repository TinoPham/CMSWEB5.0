(function () {
    define(["cms", "widgets/sites/emailsetting"
                 , "widgets/confirmdialog/confirmdialog"], emails);
    function emails(cms) {
        cms.register.controller("emailsCtrl", emailsCtrl);
        emailsCtrl.$inject = ['$scope', 'dataContext', 'cmsBase', 'AppDefine', '$timeout', '$rootScope', '$filter', '$window', '$modal'];
        function emailsCtrl($scope, dataContext, cmsBase, AppDefine, $timeout, $rootScope, $filter, $window, $modal) {

            $scope.data = [];
            $scope.filter = 0;
          
            $scope.openAddForm = function () {
                var obj = {
                    ReportKey: 0,
                    BAMReportXML: "",
                    EmailSubject: "",
                    EnableEmailReporting: true,
                    FreqCount: 0,
                    FreqTypeID: 1,
                    LastRunDate: new Date(),
                    NextRunDate: new Date(),
                    Recipients: [],
                    ReportName: "",
                    ReportType: 1,
                    RunTime: "",
                    SiteList: [],
                    StartRunDate: null,
                    UserKey: 0,
                    EmailList: []
                };
                var data = [];
                data.push(obj);
                var userInstance = $modal.open({
                    templateUrl: 'widgets/sites/emailsetting.html',
                    controller: 'emailsettingCtrl',
                    size: 'md',
                    backdrop: 'static',
                    backdropClass: 'modal-backdrop',
                    keyboard: false,
                    windowClass: 'email-setting-modal',
                    resolve: {
                        items: function () { return { model: data }; }
                    }
                });
                userInstance.result.then(function (data) {
                    $scope.modalShown = false;
                    if (data) {

                        GetData();
                    }
                });


            }
            function GetData() {

                dataContext.sitealert.GetEmailSettingByUser(0, function (data) {
                    $scope.data = data;
                }, function (err) {
                    console.log(err);
                });
            }

            $scope.DeleteEmailSettings = function (ID) {

                ID.headerText = 'CONFIRM_HEADER';
                ID.bodyText = 'CONFIRM_MSG';
                ID.param = ID.EmailSubject;
                var userInstance = $modal.open({
                    templateUrl: 'widgets/confirmdialog/confirmdialog.html',
                    controller: 'dialogCtrl',
                    size: 'md',
                    backdrop: 'static',
                    backdropClass: 'modal-backdrop',
                    keyboard: false,
                    windowClass: 'email-setting-modal',
                    resolve: {
                        items: function () { return { data: ID }; }
                    }
                });
                userInstance.result.then(function (data) {
                    $scope.modalShown = false;
                    if (data) {

                        dataContext.sitealert.DeleteEmailSettings(ID, function (data) {
                            if (data) {
                                GetData();
                            }
                        }, function (err) {
                            console.log(err);
                        });
                     }
                });





                
            }
            
            function SaveSeting(f_data) {
                dataContext.sitealert.SaveEmailSettings(f_data, function (data) {
                    cmsBase.cmsLog.success($filter('translate')('BTN_DONE'));
                }, function (err) {
                    cmsBase.cmsLog.error(err);
                });
            }
            $scope.EmailList = function (obj) {

                if (obj.length > 2) {
                    return obj[0] + ' , ' + obj[1] + $filter('translate')('AND') + (obj.length - 2) + $filter('translate')('OTHERS')
                }
                else
                    return obj.toString();
            }
            $scope.Save = SaveSeting;
            $scope.init = function () { GetData(); }
            $scope.showEmailSetting = function (setting) {
                dataContext.sitealert.GetEmailSettingByUser(setting.ReportKey,
                                                                function (data) {
                                                                    var userInstance = $modal.open(
                                                                                                    {
                        templateUrl: 'widgets/sites/emailsetting.html',
                        controller: 'emailsettingCtrl as vm',
                                                                                                        backdrop: 'static',
                        size: 'md',
                        backdropClass: 'modal-backdrop',
                        windowClass: 'email-setting-modal',
                        keyboard: false,
                        resolve: {
                                                                                                            items: function () {
                                                                                                                return { model: data };
                                                                                                            }
                                                                                                        }
                        }
                                                                                                );
                    userInstance.result.then(function (data) {
                        $scope.modalShown = false;
                        if (data) {
                            GetData();
                        }
                    });

                                                                },
                                                                function (err) {
                                                                    console.log(err);
                                                                }
                                                            );
            };

        }
    };
})();
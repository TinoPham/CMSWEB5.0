(function () {
    'use strict';

    define(['cms',
      
        'widgets/bam/modal/metricview',
		'widgets/bam/charts/convRegion',
		'widgets/bam/charts/bamTraffic',
        'Scripts/Services/chartSvc',
        'Scripts/Services/bamhelperSvc',
		'DataServices/Configuration/fiscalyearservice',
        'DataServices/Bam/DashboardReportsSvc', 'DataServices/ReportService'
    ], function (cms) {
        cms.register.controller('normalizeCtrl', normalizeCtrl);

        normalizeCtrl.$inject = ['$rootScope', '$scope', '$modal', 'cmsBase', 'dataContext', 'AppDefine', '$timeout', 'siteadminService', 'AccountSvc', 'DashboardReportsSvc', '$filter', 'chartSvc', 'fiscalyearservice', 'bamhelperSvc', 'Utils', 'ReportService'];

        function normalizeCtrl($rootScope, $scope, $modal, cmsBase, dataContext, AppDefine, $timeout, siteadminService, AccountSvc, DashboardReportsSvc, $filter, chartSvc, fiscalyearservice, bamhelperSvc, utils, rptService) {

            $scope.showInfo = 'normalize';
            var vm = this;
            vm.ModelTable = {
                header: [],
                content: []
            }
            vm.HourDay = [];
            for (var i = 1; i <= 24; i++) {
                vm.HourDay.push(i);
            }
            vm.ModelTableNormalize = [];
            
            $scope.$on(AppDefine.Events.ACTIVENORMALIZEREPORT, function (e) {
                active();
            });

            active();

            $scope.formatHour = function(value) {
                //if (angular.isNumber(value)) { return; }
                if (value < 10) {
                    return "0" + value + ":00-0" + (value + 1) + ":00";
                }
                else {
                    if (value == 23) { return value + ":00" + "-00:00"; }
                    return value + ":00-" + (value + 1) + ":00";
                }
            }

            function active() {
                vm.ModelTableNormalize = [];
                getNormalizeBySite();
            }

            function getNormalizeBySite() {
                if ($rootScope.ParamNormalize && $rootScope.ParamNormalize.siteKey.length === 1)
                {
                    var sdate = $rootScope.ParamNormalize.sDate === null ? $rootScope.BamFilter.dateReport.toDateParam() : $rootScope.ParamNormalize.sDate.toDateParam();
                    var edate = $rootScope.ParamNormalize.eDate === null ? $rootScope.BamFilter.dateReport.toDateParam() : $rootScope.ParamNormalize.eDate.toDateParam();
                var params = { sDate: sdate, eDate: edate, sitesKey: $rootScope.ParamNormalize.siteKey.toString(), ReportId: 1, ReportType: 1 };


                DashboardReportsSvc.GetNormalizeBySite(params, function (data) {
                    
                    vm.ModelTableNormalize = data;
                    
                },
                function (error) {
                    cmsBase.cmsLog.error('error');
                });
            }

            }

            $scope.saveReportNormalize = function (HourData, date, type) {
                HourData.flag = !HourData.flag;

                var ParamUpdateNormalize = {
                    Type : type,
                    Date: date,
                    Hour: HourData.Hour,
                    ReportNormalize: HourData.flag,
                    SitesKey: $rootScope.ParamNormalize.siteKey
                }

                DashboardReportsSvc.UpdateNormalize(ParamUpdateNormalize, function (data) {

                    cmsBase.cmsLog.log('Successfully update data');

                },
                function (error) {
                    cmsBase.cmsLog.error('error');
                });
            }
        }
    });
})();
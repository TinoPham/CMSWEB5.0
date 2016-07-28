(function () {
    'use strict';

    define(['cms'], function (cms) {
        cms.register.controller('ytdconvCtrl', ytdconvCtrl);

        ytdconvCtrl.$inject = ['$rootScope', '$scope', '$modal', 'cmsBase', 'dataContext', 'AppDefine', '$timeout'];

        function ytdconvCtrl($rootScope, $scope, $modal, cmsBase, dataContext, AppDefine, $timeout) {

            var vm = this;
            vm.isfilter = false;
            vm.params = {
                reportId: 11,
                type: "rdlc",
                options: { sites: '12,1,2,3,4', sdate: 'Thu Jan 28 2016', edate: 'Thu Jan 28 2016' }
            }

            
            $scope.$on(AppDefine.Events.GETBAMREPORTDATA, function (e) {
                vm.isfilter = true;

                $timeout(function () { vm.isfilter = false; }, 100);
            });

        }
    });
})();
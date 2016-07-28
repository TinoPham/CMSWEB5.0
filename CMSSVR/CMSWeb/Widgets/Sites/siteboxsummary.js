(function() {
    'use strict';

    define(['cms'],
        function(cms) {
            cms.register.controller('siteboxsummaryCtrl', siteboxsummaryCtrl);
            siteboxsummaryCtrl.$inject = ['$scope', 'AppDefine', 'cmsBase'];

            function siteboxsummaryCtrl($scope, AppDefine, cmsBase) {

                $scope.Total = 4332;

                $scope.init = function(box) {
                    if (box) {
                        $scope.configBox = box;
                    }
                }


            }
        });

})();
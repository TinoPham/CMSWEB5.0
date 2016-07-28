(function() {
    'use strict';
    define(['cms'], function(cms) {
            cms.register.controller('sensortoolCtrl', sensortoolCtrl);
            sensortoolCtrl.$inject = ['$scope', 'dataContext', '$timeout', 'siteadminService', '$modal', 'AppDefine'];

            function sensortoolCtrl($scope, dataContext, $timeout, siteadminService, $modal, AppDefine) {
                $scope.sensorSnapshotUrl = AppDefine.Api.SiteAlerts + '/GetSensorSnapshot';
            }
        }
    );
})();
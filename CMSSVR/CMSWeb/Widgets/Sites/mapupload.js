(function () {
    'use strict';

    define(['cms'], function (cms) {
        cms.register.controller('mapuploadCtrl', mapuploadCtrl);
        mapuploadCtrl.$inject = ['$scope', 'dataContext','$modalInstance', 'items','$timeout' ];
        function mapuploadCtrl($scope,dataContext, $modalInstance, items, $timeout) {
            $scope.$watch(dataContext.progressbar, function () {
                $scope.progressbar = dataContext.progressbar
                if ($scope.progressbar >= 100)
                {
                    $modalInstance.close();
                }
            });
        
        }
    });
})();
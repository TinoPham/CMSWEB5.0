(function () {
    'use strict';

    define(['cms'], function (cms) {
        cms.register.controller('dialogCtrl', dialogCtrl);

        dialogCtrl.$inject = ['$scope', 'dataContext', 'cmsBase', '$modalInstance', 'items', 'AppDefine', '$timeout', '$filter'];

        function dialogCtrl($scope, dataContext, cmsBase, $modalInstance, items, AppDefine, $timeout, $filter) {
         
            $scope.model = {};
            $scope.headerText = "";
            $scope.bodyText = "";
            $scope.init = function ()
            {
                $scope.model = items.data;
                $scope.headerText = $scope.model.headerText;
                $scope.bodyText = $filter('translate')($scope.model.bodyText) + ' [' + $scope.model.param + '] ?';
            }
            $scope.save = function () {

              //  $scope.bodyText = "Got it!"
                $timeout(function ()
                {
                    $modalInstance.close(true);
                }, 1000);

                
            }

            $scope.cancel = function () {
                $modalInstance.close(false);
            }
        }
    });
})();
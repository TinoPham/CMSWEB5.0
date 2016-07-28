var DVRInfoController = angular.module('ConfigApp');

DVRInfoController.controller('DVRInfoController', ['$rootScope', '$scope', 'AppUtils.Dialogs', 'ApiService',
    function ($rootScope, $scope, Dialogs, ApiService) {
        $scope.CurrentPage = 1;
        $scope.TotalItems = 0;
        $scope.PageConfig = {
            itemsPerPage: 30,
            boundaryLinks: true,
            directionLinks: true,
            firstText: 'First',
            previousText: 'Previous',
            nextText: 'Next',
            lastText: 'Last',
            rotate: false,
            maxSize: 10
        };

        $scope.model = { DVRs: null, AllDVR : []};
        

        $scope.GetDVRs = function()
        {
            ApiService.DVRs()
            .then(
                function (data) {
                    
                    $scope.model.AllDVR = data;
                    $scope.model.DVRs = $scope.model.AllDVR.slice(0, $scope.PageConfig.itemsPerPage);
                    $scope.TotalItems = data.length;
                },
                function (data) {

                },
                function (data) {

                }

            );
        }

        $scope.Getvalue = function (object, property)
        {
            if (object == null || object == undefined)
                return "N/A";
            var ret = object[property];
            return ret;
        }

        $scope.GetLockText = function ( data)
        {
            if (data === null || data === undefined || data.info === null || data.info === undefined)
                return "Lock";
            return data.info.Locked == true ? "Unlock" : "Lock";
        }
        function Changepage(page) {
            console.log($scope.CurrentPage);
            

        }

        $scope.$watch('CurrentPage', function (newVal, oldVal) {
            Changepage(newVal);
        }
        );
}]);
(function () {
    define(['cms'], emailalerttype);
    function emailalerttype(cms) {
        cms.register.controller("emailalerttypeCtrl", emailalerttypeCtrl);
        emailalerttypeCtrl.$inject = ['$scope', 'dataContext', 'cmsBase', 'AppDefine', '$timeout', '$rootScope', '$filter', '$window', '$modal', '$modalInstance', 'AccountSvc', 'usergroups.service', 'items'];
        function emailalerttypeCtrl($scope, dataContext, cmsBase, AppDefine, $timeout, $rootScope, $filter, $window, $modal, $modalInstance, AccountSvc, usergroupsSvc, items)
        {
            $scope.cancel = cancel;
            $scope.data = [];
            $scope.init = init;
            $scope.save = save;

            function init() {
                $scope.data = items.model;
            }
            function cancel() {
                 $modalInstance.close(null);
            }
            function save()
            {
                var data = Enumerable.From($scope.data).Where(function (value) { return value.Active == 1 }).ToArray();
                $modalInstance.close(data);

            }

        }

    }
    
})();
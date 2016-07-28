(function () {

    'use strict';

    define(['cms'], function (cms) {

        cms.register.controller('accountDetailCtrl', accountDetailCtrl);
        accountDetailCtrl.$inject = ['$scope', 'dataContext', '$modalInstance'];

        function accountDetailCtrl($scope, dataContext, $modalInstance) {

            var vm = this;
            vm.users = [];
            active();
            var data = {
                name: "LuanNguyen"
            };
            

            function active() {

            }

            $scope.close = function() {
                $modalInstance.close(data);
            }

            vm.submit = function() {

                $modalInstance.close(data);
            };
        }
    });
})();
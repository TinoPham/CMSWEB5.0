(function () {

    'use strict';
    //, '../account/accountDetails.js'
    define(['cms'], function(cms) {

        cms.register.controller('accountCtrl', accountCtrl);
        accountCtrl.$inject = ['$scope', 'dataContext', '$modal','cmsBase'];

        function accountCtrl($scope, dataContext, $modal, cmsBase) {
            var vm = this;
            vm.users = [];

            active();

            function active() {

                var promise = dataContext.auth.getUsers().then(function (data) {
                    vm.users = data.users; 
                });

                cmsBase.$q.when(promise).then(function() {
                    cmsBase.sharingData.set('account', vm.users);
                    var test = cmsBase.sharingData.get('account');
                });


            }

            vm.deletedata = function(data) {
                dataContext.auth.saveRegistration(data.target);
            }

            $scope.data = [
                { id: 1, name: 'test 1' },
                { id: 2, name: 'test 2' },
                { id: 3, name: 'test 3' },
                { id: 4, name: 'test 4' }
            ];

            // actions (show, delete) have to be defined before $scope.actionList
            $scope.show = function(row) {
                alert('show: ' + row.id + ':' + row.name);
            }
            $scope.delete = function(row) {
                alert('delete: ' + row.id);
            }

            $scope.actionList = [
                { label: 'edit', href: '#/edit/{{row.id}}', title: 'action to other CTRL' },
                { label: 'show', click: $scope.show, title: 'action in this CTRL use ngClick' },
                { label: 'delete', click: $scope.delete, title: 'action in this CTRL use ngClick' }
            ];

            $scope.call = function(fun, row) {
                fun(row);
            }


            var counter = 0;
            $scope.customer = {
                name: 'David',
                street: '1234 Anywhere St.'
            };

            $scope.customers = [];

            $scope.changeData = function() {
                counter++;
                $scope.customer = {
                    name: 'James',
                    street: counter + ' Cedar Point St.'
                };
            };


            $scope.modalShown = false;
            vm.showLoginDialog = function(user) {
                if (!$scope.modalShown) {
                    $scope.modalShown = true;
                    var modalInstance = $modal.open({
                        templateUrl: 'configuration/account/accountDetails.html',
                        controller: "accountDetailCtrl as vm",
                        backdrop: 'static'
                    });

                    modalInstance.result.then(function(data) {
                        user = data;
                        $scope.modalShown = false;
                    });
                }
            };
        }
    });
})();
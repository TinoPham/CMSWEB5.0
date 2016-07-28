//var DBController = angular.module('ConfigApp');
define(['require', 'App'], function (require, app) {

    var injectParams = ['$rootScope', '$scope', '$state', '$stateParams', 'Dialogs', 'ApiService'];
    var ctrl = function ($rootScope, $scope, $state, $stateParams, Dialogs, ApiService) {
        $scope.model = { Trusted: false, DBName: null, Server: null, UserID: null, Password: null };

        $scope.isDisabled = true;
        $scope.tmp_model = angular.copy($scope.model);
        //var jquery = window.jQuery;
        $scope.str_Edit = "Edit";
        $scope.str_Save = "Save";
        $scope.str_TestConnection = "Test Connection";

        function disabledCtrl(name, disabled) {
            var str_dis = 'disabled';
            var ctrl = $('#' + name)
            if (ctrl != null)
                ctrl.prop(str_dis, disabled);
        }

        function CancelEdit() {
            $scope.model = $scope.tmp_model;
        }

        $scope.edit = function () {
            $scope.isDisabled = !$scope.isDisabled;
            if ($scope.isDisabled) {
                $scope.model = angular.copy($scope.tmp_model);
                $scope.str_Edit = "Edit";
            }
            else
                $scope.str_Edit = "Canncel";



        }

        $scope.loadconfig = function () {
            $rootScope.isPaneShown = true;
            ApiService.GetDBConfig()
            .then(
                    function (data) {
                        $scope.model = data;
                        $scope.tmp_model = angular.copy($scope.model);
                        $rootScope.isPaneShown = false;
                    }
                    , function (data) {
                        $rootScope.isPaneShown = false;
                    },
                    function (data) {
                        $rootScope.isPaneShown = false;
                    }

            );
        }

        $scope.testconnection = function (model) {
            $rootScope.isPaneShown = true;
            $rootScope.dataB = 1;
            ApiService.TestDBConnection(model)
            .then(
                    function (data) {

                        Dialogs.error(data, $rootScope.dataB);
                        $rootScope.isPaneShown = false;
                    },
                    function (data) {
                        Dialogs.error(data);
                        $rootScope.isPaneShown = false;
                    },
                    function (data) {
                        Dialogs.error(data);
                        $rootScope.isPaneShown = false;
                    }
            );
        }

        $scope.save = function () {
            if ($scope.DBConfig.$valid) {
                $rootScope.isPaneShown = true;
                ApiService.SetDBConnection($scope.model)
                .then(
                        function (data) {
                            $rootScope.isPaneShown = false;
                            $scope.model = data;
                            $scope.tmp_model = angular.copy($scope.model);
                        }
                        , function (data) {
                            $rootScope.isPaneShown = false
                        }
                        , function (data) {
                            $rootScope.isPaneShown = false
                        }
                );

            }
            else {
                $scope.Server.$valid = false;
                Dialogs.error("There are invalid fields");
            }
        }

    };
    ctrl.$inject = injectParams;
    app.register.controller("DBController", ctrl);
    
});

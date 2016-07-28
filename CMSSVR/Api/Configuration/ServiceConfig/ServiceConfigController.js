
//var svrconfig = angular.module( 'ConfigApp' );
define( ['require', 'App'], function ( require, app ) {
    var injectParams = ['$rootScope', '$scope', '$state', '$stateParams', 'Dialogs', 'ApiService'];
    //svrconfig.controller( 'svrconfig', ['$rootScope', '$scope', 'AppUtils.Dialogs', 'ApiService',
    var ctrl = function ( $rootScope, $scope, $state, $stateParams, Dialogs, ApiService ) {
        $scope.model = { MatchAllinfo: true, DVRAuths : null };
        $scope.lastmodel = angular.copy($scope.model);
        $scope.allow_up = false;
        $scope.allow_down = false;
        $scope.Selected = null;
        $scope.ReadOnly = true;
        $scope.Resx = { str_Save: "Save", str_Cancel: "Cancel", str_Edit: "Edit" }
        $scope.btnSave = $scope.Resx.str_Edit;
        $scope.dirtyMatch = false;
        $scope.$watch('model.DVRAuths', function (value, oldValue) {
            
            if(value != null && angular.isDefined(value)) {
                var filterednames = value.filter(function (obj) {
                    return obj.Checked == true;
                });
                $scope.dirtyMatch = filterednames.length > 0 ? false : true;
            }
            
            

        }, true);
        function SetModel(data) {

            $scope.model = data;
            $rootScope.isPaneShown = false;
            $scope.lastmodel = angular.copy($scope.model);
            
        }
        function SetReadonly(readonly) {
            $scope.ReadOnly = readonly;
            if($scope.ReadOnly)
                $scope.btnSave = $scope.Resx.str_Edit;
            else
                $scope.btnSave = $scope.Resx.str_Save;
        }

        $scope.loadconfig = function () {
            $rootScope.isPaneShown = true;
            $scope.dirtyMatch = false;
            ApiService.GetDVRAuthConfig()
            .then(
                    function (data) {
                        SetModel(data);
                        SetReadonly(true);
                    }
                    , function (data) {
                        $rootScope.isPaneShown = false;
                        Dialogs.error(data);
                    },
                    function (data) {
                        $rootScope.isPaneShown = false;
                        Dialogs.error(data);
                    }

            );
        }

        $scope.setSelected = function ( value ) {
            $scope.Selected = value;
            var arr_auths = $scope.model.DVRAuths;
            var index = $.inArray(value, arr_auths);
            $scope.allow_up = false;
            $scope.allow_down = false;
            if (index >= 0 && index < arr_auths.length - 1)
                $scope.allow_down = true;

            if (index > 0) {
                $scope.allow_up = true;
            }
        }

        $scope.moveUpDown = function ( direct ) {
            if ($scope.Selected == null)
                return;
            var arr_auths = $scope.model.DVRAuths;
            var index = $.inArray($scope.Selected, arr_auths);
            var tmp = arr_auths[index + 1* direct];
            arr_auths[index + 1*direct] = $scope.Selected;
            arr_auths[index] = tmp;
            $scope.setSelected($scope.Selected);
        }

        $scope.CancelEdit = function () {
            $scope.model = angular.copy($scope.lastmodel);
            $scope.btnSave = $scope.Resx.str_Edit;
            SetReadonly(true);
            $scope.dirtyMatch = false;
        }

        $scope.SaveEdit = function () {
            if ($scope.ReadOnly) {
                SetReadonly(false);
            }
            else {
                if ( $scope.dirtyMatch == false && $scope.config.$valid ) {
                    $rootScope.isPaneShown = true;
                    ApiService.SetDVRAuthConfig($scope.model)
                    .then(
                            function (data) {
                                SetModel(data);
                                SetReadonly(true);
                            }
                            ,function (data){
                                Dialogs.error(data);
                                $rootScope.isPaneShown = false;
                            }
                            , function (data){
                                Dialogs.error(data);
                                $rootScope.isPaneShown = false;
                            }

                    );
                }

            }
        }

    };
    ctrl.$inject = injectParams;
    app.register.controller( "svrconfig", ctrl );

} );

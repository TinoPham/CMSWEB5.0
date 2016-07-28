
//var AccountController = angular.module('ConfigApp');
define(function () {
    
    var injectParams = ['$rootScope', '$scope', '$state', '$stateParams', 'Cookies', 'Base64', 'ApiService'];

    var ctrl = function ($rootScope, $scope, $state, $stateParams, CookiesUtil, Base64, ApiService) {
    
        $scope.$on('redirectToLogin', function () {

            $state.go('login');
            //window.location = "/login";
        })

        $scope.$on('redirectToRegister', function () {
            $state.go("register");
        })

        $scope.edit = false;
        function Form(scope) {
            $scope.form = scope;
        }

        $scope.InitLogin = function (form) {
            Form(form);
            var cookies = CookiesUtil.get("usertoken");
            var str_user = Base64.decode(cookies);

            var user = null;
            if (str_user != null)
                user = JSON.parse(str_user);
            if (user != null) {
                $scope.edit = false;

                ApiService.UserInfo = user;
                $rootScope.UserInfo = ApiService.UserInfo;
                $scope.User = user;
                $scope.login();

            }
            else {
                $scope.edit = true;
                var user = { UserName: null, Password: null, Name: null, ID: 0, authenticated: false };
                $scope.User = user;
            }
        }

        $scope.InitRegister = function (form) {
            Form(form);
            $scope.edit = true;
            //HideMenu(true);
            $scope.User = null;
            if (ApiService.UserInfo == null) {
                $scope.User = { UserName: null, Password: null, Name: null, ID: 0, ConfirmPassword: null };
                $("#_btnsave").attr("value", "Register");
            }
            else {
                $scope.User = { UserName: ApiService.UserInfo.UserName, Password: null, Name: ApiService.UserInfo.Name, ID: ApiService.UserInfo.ID, ConfirmPassword: null };
                $("#_btnsave").attr("value", "Save");
            }

        }

        function authenticateinfo(uinfo) {
            ApiService.UserInfo = uinfo;
            if (uinfo != null)
                ApiService.UserInfo.authenticated = true;
        }


        var Login_success = function (data) {

            $scope.edit = false;
            authenticateinfo(data);
            $rootScope.UserInfo = ApiService.UserInfo;
            var user_en = Base64.encode(data);
            CookiesUtil.set('usertoken', user_en);
            if ($state.current.name == "login")
                $state.go("home.database");
            return data;
        };

        var Login_error = function (err) {
            alert('login errr');
        }

        $scope.logout = function () {
            $rootScope.UserInfo = null;
            ApiService.UserInfo = null;
            CookiesUtil.remove("usertoken");
            $rootScope.$broadcast('redirectToLogin', null);
        }

        $scope.login = function ($event) {

            var form = $scope.form;
            if (angular.isDefined(form) && angular.isDefined(form.loginForm) != null && !form.loginForm.$valid) {
                $event.preventDefault();
                return;
            }
            var promise = ApiService.LogIn($scope.User);

            promise.then(
                    function (data) {
                        Login_success(data);
                    }
                    , function (error) {
                        Login_error(error)
                        return error;
                    }
                    , function (notify) {
                        return notify;
                    }
		);

        }

        $scope.CancelRegister = function () {
            var pram = $stateParams.ret;
            if (pram != null)
                $state.go(pram);
            else
                Window.history.back();
        }

        $scope.register = function () {

            var promise = null;
            if ($scope.User.UserID > 0)
                promise = ApiService.editAccount($scope.User);
            else
                promise = ApiService.addAccount($scope.User);


            promise.then(
                    function (data) {
                        Login_success(data);
                    }
                    , function (error) {
                        Login_error(error)
                        return error;
                    }
                    , function (notify) {
                        return notify;
                    }
		);

        }

        $rootScope.UserInfo = ApiService.UserInfo;

        var cur_sate = $state.current;
        //check to redirect to register when no user on DB
        var event;
        ApiService.checkaccount().then(
            function (data) {
                if (data == '0') {
                    //$rootScope.$broadcast('redirectToRegister', null);
                    event = 'redirectToRegister';
                }
            }
            );

        if (event == null || event == undefined) {
            $scope.InitLogin();
            //if (ApiService.UserInfo == null || !ApiService.UserInfo.authenticated) {
            //    event = 'redirectToLogin';
            //}
        }
        //if (event != null)
        //    setTimeout(function () { $rootScope.$broadcast(event, null); }, 100);
        //$rootScope.$broadcast(event, null);

    };
    ctrl.$inject = injectParams;
    //app.register.controller('AccountController', ctrl);
    return ctrl;
}

    );

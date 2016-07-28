

//ngRoute
//var ConfigApp = angular.module('ConfigApp', ['ngRoute', 'ngGrid', 'scrollable-table', 'ui.bootstrap', 'ngSanitize', 'AppUtils']);
//ConfigApp.config(['$routeProvider', '$locationProvider', function AppConfig($routeProvider, $locationProvider) {
//    var  api = '/';
//    var viewBase = '/Views/Api/';

//    $routeProvider.when( api + 'login', {
//        templateUrl: viewBase + 'account/login.html',
        
//        title: 'Login'
//    })
//    .when(api, {
//        templateUrl: viewBase + 'account/login.html',
        
//        title: 'Login'
//    })

//    .when(api + 'register', {
//            templateUrl: viewBase + 'account/register.html',
            
//            title: 'Register'
//        })

//    .when(api + 'database', {
//        templateUrl: viewBase + 'dbConfig/dbConfig.html',
//            controller: 'DBController'
//    }) 

//    .when(api + '/messages', {
//         templateUrl: viewBase + 'DBConfig.html',
//        controller: 'DBController'
//    })

//    .when(api + 'logs', {
//        templateUrl: viewBase + 'Logs/Logs.html',
//        controller: 'LogsController'
//    })

//    .when(api + 'service', {
//        templateUrl: viewBase + 'ServiceConfig/ServiceConfig.html',
//        controller: 'svrconfig'
//    })

//    .when(api + 'dvrs', {
//         templateUrl: viewBase + 'DVRInfo/DVRsInfo.html',
//         controller: 'DVRInfoController'
//    })

//    .otherwise({
//        redirectTo: api
//        });

////        $locationProvider.html5Mode(true);
////        $locationProvider.hashPrefix('!');
//    }
//]);


//ConfigApp.run(['$rootScope', '$location', 'ApiService', 'AppUtils.CookiesUtil', 'AppUtils.Base64',
//    function ($rootScope, $location, ApiService, CookiesUtil, Base64 ) {
//        $rootScope.locate = 'ru';
        
//        //Client-side security. Server-side framework MUST add it's 
//        //own security as well since client-based security is easily hacked
//        $rootScope.$on("$routeChangeStart", function (event, next, current) {
//            //$rootScope.title = next.$$route.title;
//            if ($location.$$path == '/register' || $location.$$path == '/' || $location.$$path == '/login')
//                return;

//            if (ApiService.UserInfo == null || !ApiService.UserInfo.authenticated) {
//                $location.path("/");
                
//            }


//            //if (next && next.$$route && next.$$route.secure) {
//            //    if (!ApiService.UserInfo.isAuthenticated) {
//            //        $rootScope.$broadcast('redirectToLogin', null);
//            //    }
//            //}
//        });

//    }]);



//route ui
define(['require', 'angularAMD', 'ui.router', 'nggrid', 'ng.scrollabletable', 'ui.bootstraptpls', 'sanitize', 'Utils', 'Cookies', 'Base64', 'Dialogs', 'AccountController', 'ApiService']

    , function (require, angularAMD, uirouter, nggrid, scrollabletable, uibootstraptpls, sanitize, Utils, Cookies, Base64, Dialogs, AccountController, ApiService) {

        
        var app = angular.module('app', ['ui.router', 'ngGrid', 'scrollable-table', 'ui.bootstrap', 'ngSanitize']);
         
        app.factory('Utils', Utils);
        app.factory('Cookies', Cookies);
        app.factory('Dialogs', Dialogs);
        app.factory('Base64', Base64);
        app.service("ApiService", ApiService);
        app.controller("AccountController", AccountController);
        app.config(['$controllerProvider', '$compileProvider', '$filterProvider', '$provide', '$stateProvider', '$urlRouterProvider',
                        function AppConfig($controllerProvider, $compileProvider, $filterProvider, $provide, $stateProvider, $urlRouterProvider) {
                            app.register =
                        {
                            controller: $controllerProvider.register,
                            directive: $compileProvider.directive,
                            filter: $filterProvider.register,
                            factory: $provide.factory,
                            service: $provide.service
                        };

        

        var api = '/';
        var viewBase = '/Api/Configuration/';

        var home = {
            name: 'home'
            , url: api + 'home'
            , templateUrl:  'home.html'

        };



        $stateProvider.state(
            'home.database',
            angularAMD.route( {
                url: api + 'database'
            , templateUrl: viewBase + 'dbConfig/dbConfig.html'
               , controller: 'DBController'
                , controllerUrl: 'DBController'
            }
            )
        );
        var message = {

            name: 'home.message',
            url: api + 'messages'
            , templateUrl: viewBase + 'DBConfig.html',
            controller: 'DBController'

        };
        var logs = {

            name: 'home.logs',
            param: angularAMD.route({
                url: api + 'logs'
            , templateUrl: viewBase + 'Logs/Logs.html'
            , controller: 'LogsController'
            , controllerUrl: 'LogsController'
            })

        };

        $stateProvider.state(
            'home.service',
            angularAMD.route( {
            url: api + 'service'
            , templateUrl: viewBase + 'ServiceConfig/ServiceConfig.html',
            controller: 'svrconfig'
            , controllerUrl: 'ServiceConfigController'
            })
        );

        var dvrs = {

            name: 'home.dvrs',
            url: api + 'dvrs'
            , templateUrl: viewBase + 'DVRInfo/DVRsInfo.html',
            controller: 'DVRInfoController'

        };

        var login = {
            name: 'login',
            url: api + 'login',
            templateUrl: viewBase + 'account/login.html'
            //, controller: 'AccountController'
        };

        var register = {
            name: 'register',
            templateUrl: viewBase + 'account/register.html',
            url: api + 'register/:ret'
        };

        $stateProvider.state(home)
        .state(login)
        .state(register)
        //.state(database)
        .state(message)
        .state(logs.name, logs.param)
        //.state(service)
        .state(dvrs);
        $urlRouterProvider.otherwise("/login");

        //        $locationProvider.html5Mode(true);
        //        $locationProvider.hashPrefix('!');
    }
    ]);

        app.run(['$rootScope', '$state', '$stateParams', '$location','$q',
        function ($rootScope, $state, $stateParams, $location, $q) {

            $rootScope.$state = $state;
            $rootScope.$stateParams = $stateParams;

            $rootScope.$on('$stateChangeStart', function (evt, toState, toParams, fromState, fromParams) {
                console.log('from: ' + fromState.name + ' to: ' + toState.name);
                if (toState.name != "login" && toState.name != "register" && ($rootScope.UserInfo == null || $rootScope.UserInfo.authenticated == false)) {
                    evt.preventDefault();
                    $state.go("login");

                }


            });


        $rootScope.$on('$stateChangeSuccess', function (event, toState, toParams, fromState, fromParams) {
            console.log('from: ' + fromState.name + ' to: ' + toState.name);

        });
        $rootScope.$on('$stateNotFound', function (event, unfoundState, fromState, fromParams) {
            $state.go("home");
        })

        }]);
    
    //angularAMD.bootstrap(ConfigApp);
    angular.bootstrap(document, ['app']);
        //angular.bootstrap(document, ['ConfigApp']);
    
    return app;
    
     });

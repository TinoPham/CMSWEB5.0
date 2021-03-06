﻿/// <reference path="../Scripts/Apps/directives/clearinput.js" />
require.config({

    baseUrl: "../"
   
    ,paths: {
        //angular
        'angular': 'Scripts/angular/angular'
        ,'angularAMD': '//cdn.jsdelivr.net/angular.amd/0.2.0/angularAMD.min'
        , 'ui.router': 'Scripts/angular/angular-ui-router'
        , 'sanitize': 'Scripts/angular/angular-sanitize'
        , 'ui.bootstrap': 'Scripts/angular/angular-ui/ui-bootstrap'
        , 'ui.bootstraptpls': 'Scripts/angular/angular-ui/ui-bootstrap-tpls'
        , 'nggrid': 'Scripts/angular/angular-ui/ng-grid'
        , 'ng.scrollabletable': 'Scripts/angular/angular-ui/angular-scrollable-table'
        , 'ng.gridflexibleheight': 'Scripts/angular/angular-ui/ng-grid-flexible-height'
        //jquery
        , 'jquery': 'Scripts/Jquery/jquery-2.1.1'
        , 'jquery.cookie': 'Scripts/Jquery/jquery.cookie'
        , 'jquery.metro': 'Scripts/Metro/metro'
        , 'jquery.widget': 'Scripts/Metro/jquery/jquery.widget.min'
        , 'jquery.mousewheel': 'Scripts/Metro/jquery/jquery.mousewheel'
        , 'metro.calendar': 'Scripts/Metro/metro/metro-calendar'
        , 'metro.datepicker': 'Scripts/Metro/metro/metro-datepicker'
        //directive
        , 'clearinput': 'Scripts/Apps/directives/clearinput'
        , 'fixedHeader': 'Scripts/Apps/directives/fixedHeader'
        , 'jqdatepicker': 'Scripts/Apps/directives/jqdatepicker'
        , 'showonhoverparent': 'Scripts/Apps/directives/showonhoverparent'
        , 'togglepassword': 'Scripts/Apps/directives/togglepassword'
        //apps
        //utils
        , 'Utils': 'Scripts/Apps/Utils/Utils'
        , 'Cookies': 'Scripts/Apps/Utils/Cookies'
        , 'Base64': 'Scripts/Apps/Utils/Base64'
        , 'Dialogs': 'Scripts/Apps/Utils/Dialogs'
        , 'spinnerloading': 'Scripts/Apps/Utils/spinnerloading'
        //config
        , 'App': 'api/Configuration/ConfigModule'
        , 'ApiService': 'api/Configuration/services/ApiService'
        , 'DBController': 'api/Configuration/dbconfig/DBController'
        , 'AccountController': 'api/Configuration/account/AccountController'
        , 'ServiceConfigController': 'api/Configuration/serviceconfig/ServiceConfigController'
        , 'LogsController': 'api/Configuration/logs/LogsController'
        , 'DVRInfoController': 'api/Configuration/dvrinfo/DVRInfoController'


    },

    shim: {
        'angularAMD':{deps: ['angular']}
         , 'ui.router': {
                deps: ['angular']}
        , 'sanitize': {
            deps: ['angular']
        }
        , 'ui.bootstrap': {
            deps: ['angular']
        }
        , 'ui.bootstraptpls': {
            deps: ['angular']
        }
        , 'nggrid': {
            deps: ['angular', 'jquery']
        }
        , 'ng.scrollabletable': {
            deps: ['angular']
        }
        , 'ng.gridflexibleheight': {
            deps: ['angular']
        }
        ,'Dialogs': {
            deps: ['angular', 'jquery']
        }

         , 'jquery.cookie': {
             deps: ['angular']
         }
        
        , 'jquery.widget': {
            deps: ['jquery']
        }
        , 'jquery.mousewheel': {
            deps: ['jquery']
        }
        , 'jquery.metro': {
            deps: ['jquery', 'jquery.widget', 'jquery.mousewheel']
        }
        , 'metro.calendar': {
            deps: ['jquery', 'jquery.metro']
        }
        , 'metro.datepicker': {
            deps: ['jquery', 'jquery.metro', 'metro.calendar']
        }
    }


    /**
    * for libs that either do not support AMD out of the box, or
    * require some fine tuning to dependency mgt'
    */
        

    // kick start application
    , deps: ['angular', 'angularAMD', 'jquery', 'jquery.widget', 'jquery.mousewheel', 'jquery.metro', 'jquery.cookie','App']
   
});


//'use strict';
require.config({
    urlArgs: "v=20160713",
	baseUrl: "",
	paths: {
		'Root': '..'
        //,  'Scripts': 'Scripts'
        , 'AuthServices': 'Scripts/AuthServices'
        , 'Core': 'Scripts/Core'
        , 'DataServices': 'Scripts/DataServices'
        , 'Directives': 'Scripts/Directives'
		, "Helpers": "Scripts/Helpers"
        , 'ExceptionServices': 'Scripts/ExceptionServices'
        , 'LanguageServices': 'Scripts/LanguageServices'
        , 'LogServices': 'Scripts/LogServices'
        , 'RouteServices': 'Scripts/RouteServices'
        , 'Services': 'Scripts/Services'
		, 'Library': '../Content/libs'
    },
    waitSeconds: 0
});

require(
        [
            'cms',
            'Core/constant',
            'Core/cms.configs',
            'Core/cms.Run',
            'ExceptionServices/exception.services',
            'AuthServices/InterceptorSvc',
            'LanguageServices/language',
            'RouteServices/configRoutes',
            //'DataServices/auth.service',
            'DataServices/dataServices',
            'LogServices/log.service',
            'Directives/directives',
			'Helpers/cms-filter',
			'Services/colorSvc',
            'Scripts/common',
            'Layout/main',
            'Layout/header',
			'Layout/left-bar',
            'Layout/footer',
            'Layout/profile',
            'Auth/login',
            'Auth/lostpassword',
			'Directives/cms-interger',
			'Directives/cms-decimal',
			'Library/angular-select-serial/js/angular-select-serial',
			'Library/jquery-numeral/js/numeral',
			'Library/angular-datetimepicker/js/angular-datetimepicker'
        ],
        function (require) {
        	angular.bootstrap(document, ['cms']);
        });

//window.newGUID = function() {
//    function s4() {
//        return Math.floor((1 + Math.random()) * 0x10000).toString(16).substring(1);
//    }

//    return s4() + s4() + '-' + s4() + '-' + s4() + '-' + s4() + '-' + s4() + s4() + s4();
//};

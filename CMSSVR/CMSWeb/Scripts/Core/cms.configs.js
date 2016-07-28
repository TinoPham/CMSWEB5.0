(function () {
	'use strict';
	define(['cms', 'Root/Scripts/Apps/Utils/Utils', 'Root/Scripts/Apps/Utils/Base64', 'Root/Scripts/Apps/Utils/Cookies'], function (cms, utils, Base64, Cookies) {
		cms.factory('Utils', utils);
		cms.factory('Base64', Base64);
		cms.factory('Cookies', Cookies);

		cms.config(['$controllerProvider', '$compileProvider', '$filterProvider', '$provide', 'routerProvider', '$stateProvider', '$httpProvider', '$urlRouterProvider', 'AppDefine',
            function config($controllerProvider, $compileProvider, $filterProvider, $provide, routerProvider, $stateProvider, $httpProvider, $urlRouterProvider, AppDefine) {
            	cms.register = {
            		controller: $controllerProvider.register,
            		directive: $compileProvider.directive,
            		filter: $filterProvider.register,
            		factory: $provide.factory,
            		service: $provide.service
            	};

            	$urlRouterProvider.rule(function ($injector, $location) {

            		//what this function returns will be set as the $location.url
            		var fullpath = $location.absUrl();

            		var path = $location.path();
            		var pathnormalized = path.toLowerCase();

            		var domainpath = fullpath.replace(path, "");
            		var domainnormalized = domainpath.toLowerCase();
            		if (domainpath != domainnormalized) {

            			window.location.href = domainnormalized + path;
            		}
            		else if (path != pathnormalized) {
            			$location.replace().path(pathnormalized);
            		}
            		// because we've returned nothing, no state change occurs
            	});

            	$httpProvider.useApplyAsync(true);

            	var api = '/';
            	var viewBase = '/';

            	var pageNotFound = {
            		name: AppDefine.State.PAGENOTFOUND,
            		state: AppDefine.State.PAGENOTFOUND,
            		translate: AppDefine.Resx.PAGE_NOT_FOUND_MSG,
            		url: api + 'pagenotfound',
            		path: 'pagenotfound',
            		templateUrl: 'Layout/' + 'pageNotFound.html'

            	};

            	var login = {
            		name: AppDefine.State.LOGIN,
            		state: AppDefine.State.LOGIN,
            		translate: AppDefine.Resx.LOGIN,
            		url: api + 'login',
            		path: 'Auth',
            		templateUrl: 'Auth/' + 'login.html'
            	};

            	var lostpassword = {
            		name: AppDefine.State.LOSTPASSWORD,
            		state: AppDefine.State.LOSTPASSWORD,
            		translate: AppDefine.Resx.PAGE_FORGOT_PASSWORD,
            		url: api + AppDefine.State.LOSTPASSWORD,
            		path: 'Auth',
            		templateUrl: 'Auth/' + 'lostpassword.html'
            	};

            	var root = {
            		name: AppDefine.State.ROOT,
            		state: AppDefine.State.ROOT,
            		abstract: true,
            		url: '',
            		translate: AppDefine.Resx.PAGE_HOME //'PAGE_HOME'
            	};

            	var home = {
            		name: AppDefine.State.HOME,
            		state: AppDefine.State.HOME,
            		translate: AppDefine.Resx.PAGE_HOME,
            		url: api + 'home',
            		path: '/',
            		templateUrl: 'Layout/' + 'home.html'
            	};

            	var dashboard = {
            		Name: AppDefine.State.DASHBOARD,
            		State: AppDefine.State.DASHBOARD,
            		Url: AppDefine.State.DASHBOARD,
            		Translate: AppDefine.Resx.MODULE_DASHBOARD,
            		Abstract: false,
            		Groupkey: "Menu_Dashboard",
            		Classstyle: "icon-cog",
            		isResource: true,
            		Menu: true,
            		Childs: []
            	};

            	$stateProvider.state(root.state, root);
            	$stateProvider.state(pageNotFound.state, pageNotFound);
            	$stateProvider.state(login.state, login);
            	$stateProvider.state(lostpassword.state, lostpassword);
            	$stateProvider.state(home.state, home);

            	//routerProvider.AddState(dashboard);


            	toastr.options.timeOut = 4000;
            	toastr.options.positionClass = 'toast-bottom-right';

            	//$httpProvider.useApplyAsync( false );
            	$httpProvider.interceptors.push('InterceptorSvc');
            }
		]);
	});
})();
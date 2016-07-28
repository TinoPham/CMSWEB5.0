(function () {
	'use strict';

	angular.module('cms.router', ['ui.router']).provider('router', Cmsrouter);

	Cmsrouter.$inject = ['$stateProvider'];

	function Cmsrouter($stateProvider) {

		var collectionList, cmsstate;


		cmsrouterprovider.$inject = ['$http', '$state', '$q', '$rootScope', 'AppDefine'];

		function cmsrouterprovider($http, $state, $q, $rootScope, AppDefine) {
			cmsstate = {
				getListCollection: function () { return collectionList; },
				getRouteParent: getRouteParent,
				SetRoutes: function (MenuList) {
					registerState(MenuList, $state);
					collectionList = MenuList;
					//$rootScope.$broadcast(AppDefine.APPDATALOADED);
				}
			};

			function getRouteParent(routeName) {
				var routeParent = {};
				angular.forEach(collectionList, function (route) {
					if (route.name == routeName) {
						routeParent = route;
					}
				});
				return routeParent;
			}

			return cmsstate;
		}

		function registerState(collection, $state) {

			angular.forEach(collection, function (routeitem) {

				if (!$state.get(routeitem.State)) {
					Register(routeitem);
					if (routeitem.childs != null && routeitem.childs.length > 0) {
						registerState(routeitem.childs, $state);
					}
				}
			});
		}

		function resolveRoute(routeName) {
			if (!routeName.Url) routeName.Url = '';

			var routeDef = {};

			var urlSplit = routeName.Url.split('/');
			var url = "";
			if (urlSplit.length > 1) {
				url = urlSplit[urlSplit.length - 1];
			} else {
				url = routeName.Url;
			}
			routeDef.path = routeName.Url;
			routeDef.name = routeName.State;
			routeDef.params = { obj: null };
			routeDef.translate = routeName.Translate;
			routeDef.abstract = routeName.Abstract;
			routeDef.classstyle = routeName.Classstyle;
			routeDef.menu = routeName.Menu;
			if (routeName.Menu === true) {
				routeDef.url = '/' + url;
			}
			if (routeName.Params && !routeName.Params.isNullOrUndefined) {
				routeDef.url = routeDef.url + '/' + routeName.Params;
			}

			routeDef.templateUrl = routeName.Url + '/' + routeName.Name + '.html';

			routeDef.resolve = {
				load: [
                    '$q', '$rootScope', 'translateSvc', function ($q, $rootScope, translateSvc) {

                    	//Register Language Resources
                    	if (routeName.isResource && routeName.isResource === true) {
                    		translateSvc.partLoad(routeDef.path);
                    	}

                    	//Register lazy load files
                    	var dependencies = [routeName.Url + '/' + routeName.Name + '.js'];
                    	if (routeName.dependens && routeName.dependens.length > 0) {
                    		angular.forEach(routeName.dependens, function (dep) {
                    			dependencies.push(dep);
                    		});
                    	}
                    	return resolveDependencies($q, $rootScope, dependencies);
                    }
				]
			};
			return routeDef;
		}

		function resolveDependencies($q, $rootScope, dependencies) {
			var defer = $q.defer();
			require(dependencies, function () {
				defer.resolve();
				$rootScope.$apply();
			});

			return defer.promise;
		};

		function Register(menuitem) {
			$stateProvider.state(menuitem.State, resolveRoute(menuitem));
		}

		function addstate(state) {
			Register(state);
		}

		return {
			AddState: addstate,
			$get: cmsrouterprovider
		};

	}
})();
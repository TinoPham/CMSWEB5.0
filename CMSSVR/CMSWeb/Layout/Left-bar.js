(function () {
	'use strict';
	define(['cms'], function (cms) {
		cms.controller('leftbarCtrl', leftbarCtrl);
		leftbarCtrl.$inject = ['$translatePartialLoader', '$translate', '$scope', 'router', 'AppDefine', 'AccountSvc', '$state', '$rootScope'];

		function leftbarCtrl($translatePartialLoader, $translate, $scope, router, AppDefine, AccountSvc, $state, $rootScope) {
			var SITES_STRING = "sites";
			var CONFIGURATION_STRING = "configuration";
			var DASHBOARD_STRING = "dashboard";
			var BAM_STRING = "bam";
			var GENERAL_STRING = "general";
			var CALENDAR_STRING = "calendar";
			var LDAPSETTING_STRING = "ldapsetting";
			var RECIPIENT_STRING = "recipient";
			var FISCAL_YEAR_STRING = "fiscalyear";
			var KEY_STRING = "$.key";
			var GROUPKEY_STRING = "$.Groupkey";

			var vm = this;
			vm.parent = $scope.$parent;
			vm.UserModel = AccountSvc.UserModel();
			vm.isAuthenticated = AccountSvc.isAuthenticated();
			vm.logOut = logOut;
			vm.rootSelected = {}; //set GUI select item for root.
			vm.childSelected = {}; //set GUI select item for level 2.
			//vm.UPhoto = vm.UserModel.ImageSrc == null ? null : AppDefine.DATA_IMAGE_TYPE + vm.UserModel.ImageSrc;
			var menugroups = [{
				menuName: CONFIGURATION_STRING,
				groups: [
                    //{ key: 'MENU_GENERAL', classheader: 'glyphicon-bookmark', objlist: [] },
                    { key: 'MENU_SITE_ADMIN', classheader: 'glyphicon-th-list', objlist: [], isAdmin: true },
                    { key: 'MENU_USER_MANAGEMENT', classheader: 'glyphicon-user', objlist: [], isAdmin: true },
                    { key: 'MENU_COMPANY_ADMIN', classheader: 'glyphicon-globe', objlist: [], isAdmin: false }
                    //,{ key: 'MENU_INCIDENT', classheader: 'glyphicon-alert', objlist: [] }
				]
			}];


			$scope.$on(AppDefine.Events.STATECHANGESUCCESSHANDLER, function (event, arg) {
				var path = arg.state.split('.');
				vm.rootSelected = path.length > 0 ? path[0] : arg.state;
				vm.childSelected = arg;
				vm.showFrmSearch = false;
				if (arg.state === "configuration.general")
					vm.showFrmSearch = true;
			});

			active();

			function active() {
				menu();
			}

			function logOut() {
				AccountSvc.LogOut().then(
                    function () { },
                    function () {
                    	$rootScope.$broadcast(AppDefine.Events.LOGOUTSUCCESS);
                    }
                );
			}

			vm.hasGroup = function (menu) {
				if (menu.group && menu.group.groups.length > 0) {
					return true;
				}
				return false;
			}

			function getMenuGroup(menu) {
				angular.forEach(menugroups, function (md) {
					var groups = md.groups;
					if (md.menuName == CONFIGURATION_STRING && vm.UserModel.isAdmin == false) {
						groups = md.groups.filter(function (item) { return item.isAdmin == true })
					}
					md.groups = groups
				});
				var IE_Groups = Enumerable.From(menugroups);
				var ret = IE_Groups.FirstOrDefault(null, function (item) { return item.menuName === menu.Name });
				return ret;
			}

			function group(menu) {
				// hard code
				if (menu.Name == CONFIGURATION_STRING) {
					var childs = [];
					angular.forEach(menu.childs, function (m) {
						if (m.Name != GENERAL_STRING && m.Name != CALENDAR_STRING
							&& m.Name != LDAPSETTING_STRING && m.Name != RECIPIENT_STRING)
							childs.push(m);
					});
					menu.childs = childs;
				}

				var mngroup = getMenuGroup(menu);
				if (mngroup) {
					var IE_Groups = Enumerable.From(mngroup.groups);
					var IE_childs = Enumerable.From(menu.childs);
					var matchs = IE_Groups.Join(IE_childs, KEY_STRING, GROUPKEY_STRING, function (g, c) { return { Group: g, Child: c } });
					var arr = matchs.ToArray();
					matchs.ForEach(function (item) {
						var glist = Enumerable.From(item.Group.objlist);
						var child = item.Child;
						var state = glist.FirstOrDefault(null, function (item) { return item.State === child.State });
						if (!state) {
							item.Group.objlist.push(child);
						}

					});
				}
				if ((vm.UserModel.CreatedBy != null || vm.UserModel.CreatedBy != undefined) && mngroup != null) {
					mngroup.groups = Enumerable.From(mngroup.groups).Where(function (w) { return w.key != "MENU_COMPANY_ADMIN" }).ToArray();
				}

				return mngroup;

			}

			//function isGroupMenu(state) {
			//    var liststate = state.split('.');
			//    return liststate.length === 1;
			//}

			function menu() {

				//hardcode
				vm.Menus = [];
				AccountSvc.isDashBoard = false;
				var listMenus = router.getListCollection();
				angular.forEach(listMenus, function (m) {
					if (m.Name == SITES_STRING || m.Name == CONFIGURATION_STRING
						|| m.Name == DASHBOARD_STRING || m.Name == 'bam'
						|| m.Name == 'rebar' || m.Name === "incidentreports") {
						vm.Menus.push(m); 
					}

					if (m.Name == DASHBOARD_STRING)
						AccountSvc.isDashBoard = true;
				});

				//vm.Menus = router.getListCollection();
				angular.forEach(vm.Menus, function (m) {
					m.group = group(m);
				});
				//if (mem && mem.length > 0) {
				//    var i = 0;
				//    angular.forEach(mem, function (m) {
				//        vm.menu.menuHead.push(m);
				//        i++;
				//    });
				//}
			}

			$scope.changeLanguage = function (langKey) {
				$translate.use(langKey);
			}
		}
	});
})();
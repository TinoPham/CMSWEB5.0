(function () {
	'use strict';
	define(['cms', 'layout/changepassword'], function (cms) {
		cms.controller('headerCtrl', headerCtrl);
		headerCtrl.$inject = ['$translatePartialLoader', 'sharingData', '$translate', '$scope', 'router', 'AppDefine', 'AccountSvc', '$state', '$rootScope', '$modal', '$STORAGE_KEY'];

		function headerCtrl($translatePartialLoader, sharingData, $translate, $scope, router, AppDefine, AccountSvc, $state, $rootScope, $modal, $STORAGE_KEY) {
			var vm = this;
			vm.parent = $scope.$parent;
			vm.Languages = [
				{ 'name': 'English (US)', 'value': 'en' },
				//{ 'name': 'German', 'value': 'de' },
				{ 'name': 'French', 'value': 'fr' }
				//{ 'name': 'Italian', 'value': 'it' },
				//{ 'name': 'Spanish', 'value': 'es' }
			];
			var DASHBOARD_STRING = "dashboard";
			var SITES_STRING = "sites";
			$scope.arrowHide = true; /*Show class for arrow button*/

			vm.selectedLanguage = vm.Languages[0];
			var langlocal = localStorage.getItem($STORAGE_KEY);
			//angular.forEach(vm.Languages, function (value, key) {
			//	if (vm.Languages[key].value === langlocal) {
			//		vm.selectedLanguage = vm.Languages[key];
			//	}
			//});
			for (var key = 0; key < vm.Languages.length; key++)
			{
			    if (vm.Languages[key].value === langlocal) {
			        vm.selectedLanguage = vm.Languages[key];
			    }
			}
			vm.UserModel = AccountSvc.UserModel();
			vm.logOut = logOut;
			vm.isSite = false;
			isSite();



			$scope.$on(AppDefine.Events.STATECHANGESUCCESSHANDLER, function (event, arg) {
				isSite();
			});

			vm.ChangePassword = function () {
				if (!vm.modalShown) {
					vm.modalShown = true;
					var changepasswordModal = $modal.open({
						templateUrl: 'layout/changepassword.html',
						controller: 'changepasswordCtrl as vm',
						size: 'sm',
						backdrop: 'static',
						backdropClass: 'modal-backdrop',
						keyboard: false,
						resolve: {
							items: function () {
								return vm.UserModel;
							}
						}
					});

					changepasswordModal.result.then(function (data) {
						vm.modalShown = false;
						if (data != AppDefine.ModalConfirmResponse.CLOSE) {
							vm.logOut();
						}
					});
				}
			}

			vm.MyProfile = function () {
				if (!vm.modalShown) {
					vm.modalShown = true;
					var userInstance = $modal.open({
						templateUrl: 'layout/profile.html',
						controller: 'editprofileCtrl as vm',
						size: 'sm',
						backdrop: 'static',
						backdropClass: 'modal-backdrop',
						keyboard: false,
						resolve: {
							items: function () {
								return vm.UserModel;
							}
						}
					});

					userInstance.result.then(function (data) {
						vm.modalShown = false;
						if (data != AppDefine.ModalConfirmResponse.CLOSE) {
							vm.parent.AvatarUrl();
							//vm.UPhoto = data;
							//vm.UserModel = AccountSvc.UserModel();
							//$rootScope.$broadcast(AppDefine.Events.USERPHOTOCHANGED, data); //update user photo for left bar and top bar
						}
					});
				}
			}

			vm.isDsh = getMenuTop();

			function getMenuTop() {
				var listMenus = router.getListCollection();
				var ret = false;
				angular.forEach(listMenus, function (m) {
					if (angular.isObject(m) && m.Name == DASHBOARD_STRING) {
						ret = true;
					}
				});
				return ret;
			}

			function logOut() {
				AccountSvc.LogOut().then(
                   function () {

                   },
                   function () {
                       $rootScope.$broadcast(AppDefine.Events.LOGOUTSUCCESS);
                   }
               );
			}


			$scope.changeLanguage = function (language) {
				vm.selectedLanguage = language;
				$translate.use(vm.selectedLanguage.value);
				$translate.refresh();
				$rootScope.$broadcast('changedLanguage_Severity', null);
				$rootScope.$broadcast('changedLanguage_BoxGadget', null);
			}
			$scope.showhideTree = function (elem, event) {
				// $scope.arrowHide = !$scope.arrowHide;          
				$rootScope.$broadcast('SHOW_HIDE_TREESITE');

			}


			function isSite() {
				vm.isSite = false;
				if ($state.current.name === AppDefine.State.SALEREPORTS || $state.current.name === AppDefine.State.DISTRIBUTION || $state.current.name === AppDefine.State.HEATMAP || $state.current.name === AppDefine.State.DRIVETHROUGH) { return vm.isSite; } //ThangPham, Hidden Tree Site on the right side, Jan 11 2016.
				if ($state.current.name === AppDefine.State.SITES || $state.current.name.indexOf(AppDefine.State.BAM) != -1) {
					return vm.isSite = true;
				}
				return vm.isSite;
			}


			vm.DshSetting = function () {
				if ($state.current.name === AppDefine.State.DASHBOARD) {
					$rootScope.$broadcast(AppDefine.Events.DASHBOARDSETTING);
				} else {
					if ($state.get(AppDefine.State.DASHBOARD)) {
						//sharingData.setObject({ isEdit: true });
						$state.go(AppDefine.State.DASHBOARD, { obj: { isEdit: true } });
					} else {

					}
				}

			}
		}
	});
})();
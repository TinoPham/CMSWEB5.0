(function () {
	'use strict';
	define(['cms',
        'DataServices/AccountSvc',
        'Scripts/Services/idletimeoutSvc'
	], function (cms) {
		cms.config([
			'$translatePartialLoaderProvider', function run($translatePartialLoaderProvider) {
				$translatePartialLoaderProvider.addPart('Layout');
			}
		]);

		cms.controller('mainCtrl', mainCtrl);
		mainCtrl.$inject = ['$timeout', '$translate', '$scope', '$state', '$location', 'router', 'AppDefine', 'cmsLog', 'Cookies', 'AccountSvc', '$rootScope', '$urlRouter', 'dataContext', '$window', 'idletimeoutSvc'];

		function mainCtrl($timeout, $translate, $scope, $state, $location, router, AppDefine, cmsLog, Cookies, AccountSvc, $rootScope, $urlRouter, dataContext, $window, idletimeoutSvc) {

			$scope.User = AccountSvc.UserModel();
			$scope.UserAvatarUrl = null;
			$scope.isAuthenticate = false;
			$scope.Menus = [];
			$scope.isShowTitle = false;			
			$scope.$on( AppDefine.Events.LOGINSUCCESS, function ( event, args ) {
			
				var goState = OnAuthenticate( args );
				
				if ($state.get(goState))
					$state.go(goState);
				//if ($state.get(AppDefine.State.DASHBOARD)) {
				//	$state.go(AppDefine.State.DASHBOARD);
				//}
			});

			$rootScope.$on(AppDefine.Events.IDLETIMEOUT, function() {
			    AccountSvc.LogOut().then(function () { $rootScope.$broadcast(AppDefine.Events.LOGOUTSUCCESS); }, function () { $rootScope.$broadcast(AppDefine.Events.LOGOUTSUCCESS); });
			});

		    active();
		    

			$scope.$on(AppDefine.Events.LOGOUTSUCCESS, function (event, args) {
					$scope.isAuthenticate = false;
					$scope.AvatarUrl();
					//$state.go(AppDefine.State.LOGIN);
					window.location.reload();
			} );

			angular.element($window).on("resize", setLoginboxsize);

			$timeout(function () {
				   setLoginboxsize();      
            }, 100, false);   

			function ShowHideTitle() {
				if ($state && $state.current) {
					if ($state.current.name === AppDefine.State.DASHBOARD) {
						$scope.isShowTitle = false;
					}
					else {
						$scope.isShowTitle = true;
					}
				}
			}

			$rootScope.$on("$locationChangeStart", function (location) {
				ShowHideTitle();
			});

			function OnAuthenticate( data ) {
				router.SetRoutes( data.Menus );
				$urlRouter.sync();
				$scope.User = AccountSvc.UserModel();
				$scope.isAuthenticate = AccountSvc.isAuthenticated();
				$scope.AvatarUrl();
				if ($scope.User.Menus != null && $scope.User.Menus.length > 0) {
					var iemenu = Enumerable.From( $scope.User.Menus );
					$scope.Menus = iemenu.Where( function ( item ) {
						return item.Menu == null ? false : item.Menu;
					}).ToArray();

					var hasDshBoard = Enumerable.From($scope.User.Menus).Where(function (it) { return it.State == AppDefine.State.DASHBOARD; }).FirstOrDefault();
					if (hasDshBoard != null && hasDshBoard != undefined)
						return AppDefine.State.DASHBOARD;
				}
				return AppDefine.State.HOME;
			}


			$scope.AvatarUrl = function(){
				if ( $scope.User == null || $scope.User.UserID == 0 ) {
					$scope.UserAvatarUrl = null;
				}
				else {
					dataContext.injectRepos( ['configuration.user'] ).then( function () {
						var user = $scope.User;
						$scope.UserAvatarUrl = dataContext.user.GetUserImage( user.UserID, user.UPhoto );
					} );
				}
			}

            $scope.ChildMenus = function ( menu ) {
                if(menu == null || menu.childs == null || menu.childs.lenght == 0)
                    return false;
                var iemenu = Enumerable.From( menu.childs );
                var item = iemenu.FirstOrDefault( null,function ( child ) {
                    return child.Menu == null ? false : child.Menu == true;

                } );
                return item != null;
            }

			//$scope.LoginComplete = function (data) {
            //    OnAuthenticate( data );
            //}

            $scope.Initialize = function () {
				console.log("Init login.....");
                AccountSvc.Initialize(
                    function ( data ) {
                        OnAuthenticate(data);

                        if ($state.current.name === AppDefine.State.LOGIN) {
                            $state.go(AppDefine.State.DASHBOARD);
                        }

                        var url = $location.url();
                    	if (url === undefined || url == null || url.length == 0) {
                    	    if ($state.get(AppDefine.State.DASHBOARD)) {
	                            $state.go(AppDefine.State.DASHBOARD);
	                        }
	                    }
                    },
                    function ( data ) {
                        $state.go(AppDefine.State.LOGIN);                        
                    }
                    );
            }

			function setLoginboxsize() {
            	var doc_width = $(document).width();
            	var doc_height = $(window).height();
                angular.element('.image-box-left').css('width',doc_width-500+'px' );   
	            angular.element('.content-of-login-page').css('height',doc_height+'px' );

	            if (navigator.userAgent.search("Safari") >= 0 && navigator.userAgent.search("Chrome") < 0){//<< Here
					$(".content-of-login-page").addClass('forSafari');
				}
	            
            }

			function active() {
			    if (!idletimeoutSvc.isRegister) {
			        idletimeoutSvc.registerIdleTimeout();
			    }
                idletimeoutSvc.runIdleTimeout();
            }


        }
    });
})();
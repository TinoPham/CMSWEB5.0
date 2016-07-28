(function () {
	'use strict';
	define(['cms',
		'configuration/Users/edit.js',
	],
        function (cms) {
        	cms.register.controller('userlistCtrl', userlistCtrl);
        	userlistCtrl.$inject = ['$scope', '$modal', 'cmsBase', 'dataContext', '$q'];
        	function userlistCtrl($scope, $modal, cmsBase, dataContext, $q) {
        		$scope.isCollapsed = true;
        		$scope.users = [];
        		var vm = this;
        		vm.upsergroups = {};
        		vm.selections = [];
        		vm.getFullName = function (fname, lname) {
        			var name = '';
        			if (fname) {
        				name = fname;
        			}
        			if (lname) {
        				name = name + ' ' + lname;
        			}
        			return name;
        		}

        		//*************GRID START ***************

        		//************ GRID END ***************
        		active();

        		//************PAGING  START***********
        		$scope.itemsPerPage = 10;
        		$scope.currentPage = 1;
        		//************PAGING END ***************
        		function active() {
        			// var account = cmsBase.sharingData.get('account');
        			//if (cmsBase.$state.current.path) {
        			//    cmsBase.translateSvc.partLoad(cmsBase.$state.current.path);
        			//}
        		    var def = $q.defer();
        		    dataContext.injectRepos(['configuration.usergroups', 'configuration.user']).then(function () { 
        		        getData().then(function () {
        		            def.resolve();
        		        });
        		    });
        		    return def.promise;
        		}

        		function getData() {
        		    var def = $q.defer();
        		    dataContext.user.GetAllUsers(function (data) {
        				$scope.users = data;
        			}, function (error) {
        				cmsBase.cmsLog.error(error.data.Data);
        			});
        			return def.promise;
        		}

        		function watchData() {
        			$scope.$watch('currentPage + itemsPerPage', function () {

        				dataContext.usergroups.getAll($scope.itemsPerPage, $scope.currentPage, function (data) {
        					$scope.totalItems = data.TotalCount;
        					$scope.vm.upsergroups = data.UserGroupList;
        				}, function (error) {
        					cmsBase.cmsLog.error(error.data.Data);
        				});
        				//var begin = (($scope.currentPage - 1) * $scope.itemsPerPage),
        				//    end = begin + $scope.itemsPerPage;
        				//$scope.vm.upsergroups = $scope.vm.data.slice(begin, end);
        			});
        		}


        		//************ USER START ******************
        		vm.newuserDialog = function (user) {
        			if (!$scope.modalShown) {
        				$scope.modalShown = true;
        				var userInstance = $modal.open({
        					templateUrl: 'configuration/Users/edit.html',
        					controller: 'editadduserCtrl as vm',
        					size: 'lg',
        					backdrop: 'static',
        					keyboard: false,
        					resolve: {
        						items: function () {
        							return user;
        						}
        					}
        				});

        				userInstance.result.then(function (data) {
        					user = data;
        					$scope.modalShown = false;
        				});
        			}
        		}
        		//************ USER END ******************
        	}
        });
})();
(function () {
	define(['cms',
		'DataServices/Rebar/rebar.service',
		'Scripts/Services/bamhelperSvc',
		'Scripts/Services/chartSvc',
	], function (cms) {
		cms.register.controller('refundCtrl', refundCtrl);
		refundCtrl.$inject = ['$scope', '$rootScope', '$timeout', '$stateParams', 'cmsBase', 'AppDefine', 'rebarDataSvc', 'bamhelperSvc', 'chartSvc'];
		function refundCtrl($scope, $rootScope, $timeout, $stateParams, cmsBase, AppDefine, rebarDataSvc, bamhelperSvc, chartSvc) {
			var vm = this;
			$scope.GroupByFieldConst = { Site: 1, Employee: 2 };
			$scope.groupByField = true; //true is group by site name, else value is employee
			//$scope.refundData = [];
			$scope.ReportID = AppDefine.CannedReportType.Refund;

			/*Tree site Properties, begin*/
			vm.treeDef = {
				Id: 'ID',
				Name: 'Name',
				Type: 'Type',
				Checked: 'Checked',
				Childs: 'Sites',
				Count: 'SiteCount',
				Model: {}
			}
			vm.treeOptions = {
				Node: {
					IsShowIcon: true,
					IsShowCheckBox: true,
					IsShowNodeMenu: false,
					IsShowAddNodeButton: false,
					IsShowAddItemButton: false,
					IsShowEditButton: false,
					IsShowDelButton: false,
					IsDraggable: false
				},
				Item: {
					IsShowItemMenu: false
				}, Type: {
					Folder: 0,
					Group: 2,
					File: 1
				},
				CallBack: {
					SelectedFn: selectedFn
				}
			}
			vm.querySite = '';
			vm.treeSiteFilterS = null;
			vm.siteloaded = false;
			vm.selectedSites = [];
			/*Tree site Properties, end*/

			/*Progress bar, begin*/
			$scope.max = 100;
			$scope.dataProgressValue = 0;
			$scope.isReset = false;
			$scope.progressing = false;
			$scope.showProgressBar = false;
			/*Progress bar, end*/

			$scope.$on(AppDefine.Events.REBARSEARCH, function (e, arg) {
				//$scope.getRefundData();
			});

			$scope.$on(AppDefine.Events.PAGEREADY, function () {
				active();
			});

			$scope.$watch("groupByField", function (newVal, oldVal) {
				if (newVal !== oldVal) {
					//$scope.getRefundData();
					$scope.$broadcast(AppDefine.Events.CANNEDGROUPBYCHANGED, { groupByField: newVal });
				}
			});

			//active();

			function active() {
				if ($stateParams.obj && $stateParams.obj.filter) {
					$rootScope.rebarSearch = angular.copy($stateParams.obj.filter);
					getAllRegionSites($rootScope.rebarSearch.SiteKeys);
					$scope.$parent.$parent.pageReady = true;
					return;
				}
				getAllRegionSites();
			}

			$scope.stopGetData = function () {
				$scope.dataProgressValue = 0;
				$scope.isStopped = true;
				$scope.progressing = false;
				$scope.showProgressBar = false;
			};

			$scope.reloadTreeSiteData = function () {
				//if ($stateParams.obj && $stateParams.obj.filter) {
				//	$rootScope.rebarSearch = angular.copy($stateParams.obj.filter);
				//	getAllRegionSites($rootScope.rebarSearch.SiteKeys);
				//}
				//else {
				//	getAllRegionSites();
				//}
				getAllRegionSites(vm.selectedSites);
			}

			/*Tree site methods, begin*/
			function getAllRegionSites(siteSelected) {
				if ($scope.$parent && $scope.$parent.$parent && $scope.$parent.$parent.treeSiteFilter) {
					var data = $scope.$parent.$parent.treeSiteFilter;
					vm.treeSiteFilterS = data;
					if (siteSelected && siteSelected.length > 0) {
						bamhelperSvc.setNodeSelected($scope.treeSiteFilter, siteSelected);
						//$rootScope.$broadcast('cmsTreeRefresh');
						vm.selectedSites = siteSelected;
						//vm.siteloaded = true;
					}
					else {
						bamhelperSvc.checkallNode($scope.treeSiteFilter);
						var checkedIDs = [];
						chartSvc.GetSiteSelectedIDs(checkedIDs, vm.treeSiteFilterS.Sites);
						vm.selectedSites = checkedIDs;
						//vm.siteloaded = true;
					}
					vm.siteloaded = true;
					$rootScope.$broadcast('cmsTreeRefresh');
					$rootScope.siteIDs = vm.selectedSites;
				}
			}

			vm.TreeSiteClose = function () {
				if ($("#btn-popMenuConvSites").parent().hasClass("open")) {
					$("#btn-popMenuConvSites").parent().removeClass("open");
					$("#btn-popMenuConvSites").prop("aria-expanded", false);
				}
				var checkedIDs = [];
				var checkedNames = [];
				chartSvc.GetSiteSelectedNames(checkedIDs, checkedNames, vm.treeSiteFilterS.Sites);
				vm.selectedSites = checkedIDs;
				$rootScope.siteIDs = vm.selectedSites;
				//$scope.$broadcast(AppDefine.Events.REBARSEARCH, $rootScope.rebarSearch);
				//console.log(checkedIDs);
			}

			function selectedFn(node, scope) {

			}

			$scope.clickOutside = function (event, element) {
				//if (angular.element(element).hasClass('rebar-btn-tree')) {
				//	var checkedIDs = [];
				//	chartSvc.GetSiteSelectedIDs(checkedIDs, vm.treeSiteFilterS.Sites);
				//	vm.selectedSites = checkedIDs;
			    //}
			    // 2015-05-25 Tri fix bug 3289
			    // update state tree when without click Done.
			    bamhelperSvc.setNodeSelected(vm.treeSiteFilterS, vm.selectedSites);
			    $scope.$broadcast('cmsTreeRefresh', vm.treeSiteFilterS);

				if (angular.element(element).hasClass('open')) {
					angular.element(element).removeClass('open');
				}
			};
			/*Tree site methods, end*/
		}
	});
})();
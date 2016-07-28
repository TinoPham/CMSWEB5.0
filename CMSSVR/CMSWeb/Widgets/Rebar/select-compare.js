(function () {
	'use strict';

	define(['cms', 'widgets/rebar/compare-transacdetail'], function (cms) {
	    cms.register.controller('selectcompareCtrl', selectcompareCtrl);

	    selectcompareCtrl.$inject = ['$scope', 'dataContext', 'cmsBase', '$modalInstance', 'items', 'AppDefine', 'rebarDataSvc', '$modal', '$timeout', '$filter','$rootScope'];

	    function selectcompareCtrl($scope, dataContext, cmsBase, $modalInstance, items, AppDefine, rebarDataSvc, $modal, $timeout, $filter, $rootScope) {
	        $scope.datestatus = {};
	        $scope.filterdate = {};
	        $scope.currPage = 1;
	        $scope.selectTransac = null;
	        $scope.transacfitler = "";
	        $scope.employfilter = "";
	        $scope.dateOptions = {
				format: 'L',
				ignoreReadonly: true,
				showTodayButton: true
	        };

	        active();

	        function active() {
	            $scope.filterdate.startdate = cmsBase.DateUtils.startOfDate(items.startdate);
	            $scope.filterdate.enddate =  cmsBase.DateUtils.endOfDate(items.enddate);
				$scope.currPage = 1;
				gettransactionList($scope.currPage);
	        }

	        function gettransactionList(page) {
	            var param = {
	                TranNo: $scope.transacfitler,
	                EmployeeId: $scope.employfilter,
	                StartTranDate: $filter('date')(cmsBase.DateUtils.startOfDate($scope.filterdate.startdate), AppDefine.CalDateTimeFormat),
	                EndTranDate: $filter('date')(cmsBase.DateUtils.endOfDate($scope.filterdate.enddate), AppDefine.CalDateTimeFormat),
	                PageNo: page,
	                PageSize: 5,
                    Sites:$rootScope.allsites
	            }
	            rebarDataSvc.geTransactionFilterPagings(param, function (data) {
	                $scope.data = data;
	            }, function (error) {

	            });
	        }

	        var filterTextTimeout;
	        $scope.$watch('[transacfitler,employfilter,filterdate.startdate, filterdate.enddate, ]', function () {
	            if (filterTextTimeout) $timeout.cancel(filterTextTimeout);

	            filterTextTimeout = $timeout(function () {
					$scope.currPage = 1;
					gettransactionList($scope.currPage);
	            }, 600); // delay 600 ms
	        }, true);

	        $scope.selectTransactionToCompair = function(tran) {
	            $scope.selectTransac = tran;
			};

	        //$scope.tractionchange = function () {


            //    if (filterTextTimeout) $timeout.cancel(filterTextTimeout);

            //    filterTextTimeout = $timeout(function () {
            //        gettransactionList(1);
            //    }, 600); // delay 600 ms

            //}

	        //$scope.employchange = function () {
            //    if (filterTextTimeout) $timeout.cancel(filterTextTimeout);

            //    filterTextTimeout = $timeout(function () {
            //        gettransactionList(1);
            //    }, 600); // delay 600 ms

            //}


	        $scope.startopen = function ($event, elementOpened, elementClose) {
	            $event.preventDefault();
	            $event.stopPropagation();
	            $scope.datestatus[elementOpened] = !$scope.datestatus[elementOpened];
	        };

	        $scope.endopen = function ($event, elementOpened, elementClose) {
	            $event.preventDefault();
	            $event.stopPropagation();
	            $scope.datestatus[elementOpened] = !$scope.datestatus[elementOpened];
	        };

	        $scope.nextPage = function (data) {
	            $scope.currPage = cmsBase.RebarModule.Next(data, $scope.currPage, $scope.data.TotalPages, function (currentPage) { gettransactionList(currentPage); });
	            //if ($scope.currPage >= $scope.data.TotalPages) {
	            //    return;
	            //}

	            //if (data) {
	            //    $scope.currPage = data;
	            //} else {
	            //    $scope.currPage = $scope.currPage + 1;
	            //}

	            //gettransactionList($scope.currPage);
			};

	        $scope.gotoPage = function () {
	            $scope.currPage = cmsBase.RebarModule.Goto($scope.currPage, $scope.data.TotalPages, function (currentPage) { gettransactionList(currentPage); });
	            //if ($scope.currPage > 0 && $scope.currPage <= $scope.data.TotalPages) {
	            //    gettransactionList($scope.currPage);
	            //}
			};

			$scope.prevPage = function (data) {
			    $scope.currPage = cmsBase.RebarModule.Prev(data, $scope.currPage, $scope.data.TotalPages, function (currentPage) { gettransactionList(currentPage); });
	            //if ($scope.currPage <= 1) {
	            //    return;
	            //}

	            //if (data) {
	            //    $scope.currPage = 1;
	            //} else {
	            //    $scope.currPage = $scope.currPage - 1;
	            //}

	            //gettransactionList($scope.currPage);
			};

	        $scope.cancel = function () {
	            $modalInstance.close();
			};

	        //START TRANSACTION DETAIL------------------------------------------------------------------------------------------------------------
	        $scope.showComparition = function () {
				if (!$scope.selectTransac) { return; }

	            if (!$scope.modalShown) {
	                $scope.modalShown = true;

	                var showAboutdModal = $modal.open({
	                    templateUrl: 'widgets/rebar/compare-transacdetail.html',
	                    controller: 'comparetransacdetailCtrl',
	                    windowClass: 'modal-custom-for-compare',
	                    resolve: {
	                        items: function () {
	                          return  {
	                              data: items.data, TranId: $scope.selectTransac.TranId
	                            }
	                        }
	                    },
	                    size: 'lg',
	                    backdrop: 'static',
	                    keyboard: false
	                });

	                showAboutdModal.result.then(function (data) {
	                    $scope.modalShown = false;
	                });

	                $modalInstance.close();
	            }
			};
	        //END TRANSACTION DETAIL------------------------------------------------------------------------------------------------------------
	    }
	});
})();
(function () {
	'use strict';

	define(['cms'], function (cms) {
	    cms.register.controller('selecttranflagCtrl', selecttranflagCtrl);

	    selecttranflagCtrl.$inject = ['$scope', 'dataContext', 'cmsBase', '$modalInstance', 'items', 'AppDefine', 'rebarDataSvc', '$modal', '$timeout'];

	    function selecttranflagCtrl($scope, dataContext, cmsBase, $modalInstance, items, AppDefine, rebarDataSvc, $modal, $timeout) {

	        active();

	        $scope.saveFlags = function() {
	            var result = Enumerable.From($scope.data).Where(function(x) { return x.Checked === true; }).ToArray();

	            var hasChange = false;

	            if (result.length < $scope.selectData.length || result.length > $scope.selectData.length) {
	                hasChange = true;
	            } else {
	                result.forEach(function(f) {
	                    var sel = Enumerable.From($scope.selectData).Where(function(x) { return x.Id === f.Id; }).FirstOrDefault();
	                    if (!sel) {
	                        hasChange = true;
	                    }
	                });
	            }

	            if (hasChange === true) {
	                $modalInstance.close(result);
	            } else {
	                $modalInstance.close();
	            }

	        }

	        $scope.configFlag = function () {
	            if (!$scope.modalShown) {
	                $scope.modalShown = true;
	                var showFilterModal = $modal.open({
	                    templateUrl: 'widgets/rebar/config-tran-flag.html',
	                    controller: 'configtranflagCtrl',
	                    resolve: {
	                        items: function () {
	                            return null;
	                        }
	                    },
	                    size: 'md',
	                    backdrop: 'static',
	                    keyboard: false
	                });

	                showFilterModal.result.then(function (data) {
	                    if (data === true) {
	                        active();
	                    }
	                    $scope.modalShown = false;
	                });
	            }
	        }

	        function active() {

	            $scope.selectData = items.data;

	            rebarDataSvc.getTransactionTypes(function (data) {
	                $scope.data = data;

	                $scope.data.forEach(function(f) {
	                    var sel = Enumerable.From($scope.selectData).Where(function (x) { return x.Id === f.Id; }).FirstOrDefault();
	                    if (sel) {
	                        f.Checked = true;
	                    } else {
	                        f.Checked = false;
	                    }
	                });
	                
	               

	            }, function (error) {

	            });

	        }
	        $scope.cancel = function () {
	            $modalInstance.close();
	        }
	       
	    }
	});
})();
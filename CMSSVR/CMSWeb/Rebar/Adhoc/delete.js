(function () {
	'use strict';

	define(['cms'], function (cms) {
	    cms.register.controller('deletereportCtrl', deletereportCtrl);

	    deletereportCtrl.$inject = ['$scope', 'dataContext', 'cmsBase', '$modalInstance', 'items', 'AppDefine'];

	    function deletereportCtrl($scope, dataContext, cmsBase, $modalInstance, items, AppDefine) {

	        $scope.delMsg = '';

	        var vm = this;
	        vm.item = items;

            $scope.save = function(){
                $modalInstance.close(vm.item);
            }

            $scope.cancel = function () {
                $modalInstance.close(AppDefine.ModalConfirmResponse.CLOSE);
            }
	    }
	});
})();
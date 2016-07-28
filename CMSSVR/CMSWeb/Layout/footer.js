(function() {
    'use strict';
    define(['cms','widgets/about'], function(cms) {
        cms.controller('footerCtrl', footerCtrl);
        footerCtrl.$inject = ['$scope', '$modal', 'AppDefine','cmsBase'];

        function footerCtrl($scope, $modal, AppDefine, cmsBase) {
        	$scope.aaa = false;
        	var w = $(window).width();
        	if (w > 767) {
        		$scope.pull = true;
        	}
            $scope.version = AppDefine.CMSVERSION;
            $scope.showAbout = function () {
                if (!$scope.modalShown) {
                    $scope.modalShown = true;
                    var showAboutdModal = $modal.open({
                        templateUrl: 'widgets/about.html',
                        controller: 'aboutWidgetCtrl',
                        size: 'md',
                        windowClass : "custom-about-modal",
                        backdrop: 'static',
                        backdropClass: 'modal-backdrop ',
                        keyboard: false
                    });

                    showAboutdModal.result.then(function (data) {
                        $scope.modalShown = false;
                    });
                }
            }
        }
    });
})();



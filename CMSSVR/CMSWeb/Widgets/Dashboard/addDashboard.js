(function () {
	'use strict';

	define(['cms'], function (cms) {

		cms.register.controller('addDashboardtCtrl', addDashboardtCtrl);

		addDashboardtCtrl.$inject = ['$scope', '$modalInstance', 'cmsBase', 'items', 'addNew', 'dataContext'];

		function addDashboardtCtrl($scope, $modalInstance, cmsBase, items, addNew, dataContext) {

			active();

			function active() {
				dataContext.injectRepos(['dashboard']).then(getActiveData);
			}

			function getActiveData() {

				dataContext.dashboard.getLayouts(
                      function (result) {
                      	$scope.Layouts = result;
                      }, function (error) {

                      });
			}


			$scope.selectWd = function (w) {
				$scope.selected = w;
			}


			$scope.CloseRegion = function () {
				$modalInstance.close();
			}

			$scope.SaveRegion = function () {
				if ($scope.selected == null || $scope.selected == undefined) {
					return;
				}
				$modalInstance.close($scope.selected);
			}

		}
	});
})();
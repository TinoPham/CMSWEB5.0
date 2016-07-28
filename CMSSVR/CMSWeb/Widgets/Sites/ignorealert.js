(function () {
	'use strict';

	define(['cms'], function (cms) {
		cms.register.controller('ignorealertCtrl', ignorealertCtrl);

		ignorealertCtrl.$inject = ['$scope', '$modalInstance', 'cmsBase', 'siteadminService', 'items', 'dataContext', 'AppDefine'];

		function ignorealertCtrl($scope, $modalInstance, cmsBase, siteadminService, items, dataContext, AppDefine) {
			active();

			function active() {

				if (items) {
					$scope.data = items;
				} else {
					$modalInstance.close();
				}
			}


			$scope.Close = function () {
				$modalInstance.close();
			}

			$scope.Save = function () {

				$modalInstance.close($scope.data);
			}
		}
	});

})();
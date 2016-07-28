(function () {
	'use strict';

	define(['cms'], function (cms) {
		cms.register.controller('paymentFilterCtrl', paymentFilterCtrl);

		paymentFilterCtrl.$inject = ['$scope', 'cmsBase', '$modalInstance', 'items', 'AppDefine', 'rebarDataSvc', '$modal', '$timeout'];

		function paymentFilterCtrl($scope, cmsBase, $modalInstance, items, AppDefine, rebarDataSvc, $modal, $timeout) {

			active();

			$scope.saveFlags = function () {
				var result = Enumerable.From($scope.data).Where(function (x) { return x.Checked === true; }).Select(function (x) { return x.ID; }).ToArray();
				if (result) {
					$modalInstance.close(result);
				} else {
					$modalInstance.close([]);
				}
			}

			$scope.clear = function () {
				$scope.data.forEach(function (p) {
					p.Checked = false;
				});
			}

			function active() {

				$scope.selectData = items;

				rebarDataSvc.GetPaymentList(function (data) {
					$scope.data = data;

					$scope.data.forEach(function (p) {
						var sel = Enumerable.From($scope.selectData).Where(function (x) { return x === p.ID; }).FirstOrDefault();
						if (sel) {
							p.Checked = true;
						} else {
							p.Checked = false;
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
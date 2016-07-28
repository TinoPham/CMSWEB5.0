(function () {
	'use strict';

	define(['cms'], function (cms) {
		cms.register.controller('columnoptionCtrl', columnoptionCtrl);

		columnoptionCtrl.$inject = ['$scope', 'dataContext', 'cmsBase', '$modalInstance', 'items', 'AppDefine', 'rebarDataSvc', '$modal', '$timeout'];

		function columnoptionCtrl($scope, dataContext, cmsBase, $modalInstance, items, AppDefine, rebarDataSvc, $modal, $timeout) {
			$scope.fields = angular.copy(items);

			$scope.save = function () {
				$modalInstance.close($scope.fields);
			};

			$scope.sortableOptions = {
				stop: function (e, ui) {
				}
			};

			$scope.selectedOptions = function (item) {
				item.checked = !item.checked;
				//var result = Enumerable.From($scope.fields).Where(function (x) { return x.fieldName === item.key; }).FirstOrDefault();
				//if (result) {
				//	var elementPos = $scope.fields.map(function (x) { return x.id; }).indexOf(result.id);
				//	$scope.fields[elementPos].checked = item.checked;
				//}
			};

			$scope.getActiveclass = function (flag) {
				if (flag == true) {
					return "active";
				}
				return "";
			};
			
			$scope.cancel = function () {
				$modalInstance.close();
			};
		}
	});
})();
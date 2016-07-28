(function () {
	'use strict';

	define(['cms'], function (cms) {
		cms.register.controller('ImagePreviewSlideCtrl', ImagePreviewSlideCtrl);
		ImagePreviewSlideCtrl.$inject = ['$scope', 'dataContext', 'cmsBase', '$modalInstance', 'items', 'AppDefine'];
		function ImagePreviewSlideCtrl($scope, dataContext, cmsBase, $modalInstance, items, AppDefine) {
			$scope.delMsg = '';
			$scope.url = AppDefine.Api.Sites + AppDefine.SiteAPI.GET_FILE;

			$scope.cancel = function () {
				$modalInstance.close();
			}

			$scope.Interval = 5000;
			$scope.model = [];
			$scope.siteKey = items.siteKey;
			angular.forEach(items.model, function (f) {
				if (f.Name === items.file.Name) {
					$scope.model.splice(0, 0, f);
				} else {
					$scope.model.push(f);
				}
			});
		}
	});
})();
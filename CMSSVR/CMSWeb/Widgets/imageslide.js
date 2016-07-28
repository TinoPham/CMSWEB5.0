(function () {
	'use strict';

	define(['cms'], function (cms) {
		cms.register.controller('imageSlideCtrl', imageSlideCtrl);

		imageSlideCtrl.$inject = ['$scope', 'dataContext', 'cmsBase', '$modalInstance', 'items', 'AppDefine'];

		function imageSlideCtrl($scope, dataContext, cmsBase, $modalInstance, items, AppDefine) {

			$scope.delMsg = '';
			$scope.url = AppDefine.Api.Sites + AppDefine.SiteAPI.GET_FILE;

			$scope.cancel = function () {
				$modalInstance.close();
			}

			$scope.Interval = 5000;


			$scope.model = [];
			angular.forEach(items.model.Files, function (f) {
				if (f.Name === items.file.Name) {
					$scope.model.splice(0, 0, f);
				} else {
					$scope.model.push(f);
				}
			});

			//var slides = angular.copy(items.model);

			//slides.Sites.splice(0, 0, slides.Sites.splice(slides.Sites.indexOf(items.file), 1)[0]);
			//$scope.model = slides
		}
	});
})();
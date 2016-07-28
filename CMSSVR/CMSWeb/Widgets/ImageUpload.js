(function () {
	'use strict';
	define(['cms', 'Widgets/ImagePreviewSlide'], function (cms) {
		cms.register.controller('ImageUploadCtrl', ImageUploadCtrl);
		ImageUploadCtrl.$inject = ['$scope', '$modal', 'cmsBase', 'dataContext', '$timeout', 'siteadminService', '$modal', 'AppDefine'];

		function ImageUploadCtrl($scope, $model, cmsBase, dataContext, $timeout, siteadminService, $modal, AppDefine) {

			$scope.fileModels = [];
			$scope.url = AppDefine.Api.Sites + AppDefine.SiteAPI.GET_FILE;
			$scope.FileAccept = AppDefine.FileUploadTypes.Images;
			$scope.FileOption = { maxSize: 4, maxLength: 50 };
			$scope.isAdd = false;

			active();

			function fileModel() {
				return {
					Name: '',
					Data: null,
					hasData: false
				}
			}

			function active() {
				dataContext.injectRepos(['configuration.siteadmin']).then(function () {
					if (!$scope.$parent.model) {
						$timeout(function () {
							GetImageSite($scope.$parent.parentModel.SiteKey);
						}, 0);
					}
				});
			}

			function GetImageSite(siteKey) {
				if (!siteKey) {
					$scope.$parent.model = [];
					$scope.isAdd = true;
					return;
				}

				dataContext.siteadmin.GetImageSite({ skey: siteKey }).then(
					function (data) {
						if (data.Data) {
							$scope.$parent.model = data.Data;
							$scope.isAdd = false;
						}
						else {
							$scope.isAdd = true;
						}
					},
					function (error) {
						$scope.isAdd = true;
					});
			}

			function getLengthFileDifine(data) {
				var i = 0;
				while (data[i] !== ',') {
					i++;
				}
				return i;
			}

			$scope.removeImage = function (e, image) {
				e.stopPropagation();
				if ($scope.$parent.model) {
					$scope.$parent.model.splice($scope.$parent.model.indexOf(image), 1);
				}
			}

			$scope.showSlide = function (selected, file) {
				if (!$scope.modalShown) {
					$scope.modalShown = true;
					var ImagePreviewInstance = $modal.open({
						templateUrl: 'Widgets/ImagePreviewSlide.html',
						controller: 'ImagePreviewSlideCtrl as vm',
						size: 'md',
						backdrop: 'static',
						backdropClass: 'modal-backdrop',
						keyboard: false,
						resolve: {
							items: function () {
								return {
									model: selected,
									file: file,
									siteKey: $scope.$parent.parentModel.SiteKey
								}
							}
						}
					});

					ImagePreviewInstance.result.then(function (data) {
						$scope.modalShown = false;
						if (!data) { return; }
					});
				}
			}

			$scope.$on(AppDefine.Events.FILESELECTEDCHANGE, function (event, args) {
				var files = args.file;
				for (var i = 0, f; f = files[i]; i++) {
					var fileReader = new FileReader();
					fileReader.onload = (function (f) {
						return function (e) {
							var data = e.target.result;
							var source = data.substring(getLengthFileDifine(data));
							if (source.indexOf(",") > -1) {
								source = source.substring(1);
							}
							var fileM = new fileModel();
							fileM.Name = f.name;
							fileM.Data = source;
							fileM.hasData = true;
							$scope.$parent.model.push(fileM);
							$scope.$applyAsync();
						};
					})(f);

					fileReader.readAsDataURL(f);
				}
			});
		}
	});
})();
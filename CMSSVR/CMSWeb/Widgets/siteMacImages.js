(function () {
	'use strict';
	define(['cms', 'Widgets/imageslide'], function (cms) {
		cms.register.controller('siteimagesCtrl', siteimagesCtrl);
		siteimagesCtrl.$inject = ['$scope', '$modal', 'cmsBase', 'dataContext', '$timeout', 'siteadminService', '$modal', 'AppDefine'];

		function siteimagesCtrl($scope, $model, cmsBase, dataContext, $timeout, siteadminService, $modal, AppDefine) {

			$scope.fileModels = [];
			$scope.url = AppDefine.Api.Sites + AppDefine.SiteAPI.GET_FILE;
			$scope.MacFileAcept = AppDefine.FileUploadTypes.Images;
			$scope.MacFileOption = { maxSize: 4, maxLength: 50 };

			active();

			function fileModel() {
				return {
					Name: '',
					Data: null,
					hasData: false,
					SiteKey: 0,
					MAC: null,
					Size: 0,
					KDVR: 0
				}
			}

			function active() {

				if (!validateMac($scope.$parent.model.MacAddress)) {

				}

				dataContext.injectRepos(['configuration.siteadmin']).then(function () {
					$timeout(function () {
						getFiles($scope.$parent.model.Id, $scope.$parent.parentModel.SiteKey);
					}, 0);
				});
			}

			function validateMac(mac) {
				var regex = /^([0-9A-F]{2}[:-]?){5}([0-9A-F]{2})$/;
				return regex.test(mac);
			}

			function getFiles(KDVR, siteKey) {
				var def = cmsBase.$q.defer();
				dataContext.siteadmin.getMacFiles({ KDVR: KDVR, siteKey: siteKey }, function (data) {
					$scope.$parent.model.Files = data;
					$scope.isAdd = false;
					def.resolve();
				}, function (error) {
					$scope.isAdd = true;
					def.reject();
					//siteadminService.ShowError(error);
				});
				return def.promise;
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
				if ($scope.$parent.model.Files) {
					$scope.$parent.model.Files.splice($scope.$parent.model.Files.indexOf(image), 1);
				}
			}

			$scope.showSlide = function (selected, file) {

				if (!$scope.modalShown) {
					$scope.modalShown = true;
					var userDeleteInstance = $modal.open({
						templateUrl: 'Widgets/imageslide.html',
						controller: 'imageSlideCtrl as vm',
						size: 'md',
						backdrop: 'static',
						backdropClass: 'modal-backdrop',
						keyboard: false,
						resolve: {
							items: function () {
								return {
									model: selected,
									file: file
								}
							}
						}
					});

					userDeleteInstance.result.then(function (data) {
						$scope.modalShown = false;

						if (!data) {
							return;
						}
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
							fileM.Size = f.size;
							fileM.SiteKey = $scope.$parent.parentModel.SiteKey;
							//fileM.MAC = $scope.$parent.parentModel.MacAddress;
							//fileM.KDVR = $scope.$parent.parentModel.Id;

							$scope.$parent.model.Files.push(fileM);
							$scope.$applyAsync();
						};
					})(f);

					fileReader.readAsDataURL(f);
				}
			});
		}
	});
})();
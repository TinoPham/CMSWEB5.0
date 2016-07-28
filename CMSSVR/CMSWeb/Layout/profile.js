(function () {
	'use strict';

	define(['cms'], function (cms) {
		cms.controller('editprofileCtrl', editprofileCtrl);
		editprofileCtrl.$inject = ['$scope', 'cmsBase', '$modalInstance', 'items', 'AccountSvc', '$modal', 'AppDefine', '$timeout', '$q', 'dataContext', 'Utils'];

		function editprofileCtrl($scope, cmsBase, $modalInstance, items, AccountSvc, $modal, AppDefine, $timeout, $q, dataContext, Utils) {
			var IMAGE_TYPE_STRING = "image";
			var vm = this;
			vm.myProfile = {};
			vm.picSource = null;
			vm.avatarurl = null;
			vm.defField = {
				UPhoto: 'UPhoto',
				UImageSrc: 'ImageSrc'
			};
			vm.defUpload = {
				UImageOption: { modelName: vm.defField.UPhoto, modelData: vm.defField.UImageSrc },
				UImageAccept: AppDefine.FileUploadTypes.Images
			};

			active();

			function active() {
				dataContext.injectRepos(['configuration.user']).then(getUserDetail);
			}

			$scope.$on(AppDefine.Events.FILESELECTEDCHANGE, function (event, args) {
				var file = args.file;
				angular.forEach(file, function (f) {
					if (f.type.indexOf(IMAGE_TYPE_STRING) > -1) {
						$timeout(function () {
							var fileReader = new FileReader();
							fileReader.readAsDataURL(f);
							fileReader.onload = function (image) {
								$timeout(function () {
									ChangeImageSize(image.target.result);
									$scope.$apply(function () {
										vm.myProfile.UPhoto = f.name;
									});
								});
							}
						});
					}
				});
			});
			
			function ChangeImageSize(image) {
				var def = $q.defer();
				createImage(image).then(resizeImage, function (data) {
					def.resolve();
					console.log('error')
				});
				return def.promise;
			}

			function createImage(src) {
				var deferred = $.Deferred();
				var img = new Image();
				img.onload = function () {
					deferred.resolve(img);
				};
				img.src = src;
				return deferred.promise();
			};

			function resizeImage(image) {
				var mainCanvas = document.createElement("canvas");
				mainCanvas.width = 100;
				mainCanvas.height = 100;
				$scope.$apply(function () {
					var ctx = mainCanvas.getContext("2d");
					ctx.drawImage(image, 0, 0, mainCanvas.width, mainCanvas.height);
					vm.picSource = mainCanvas.toDataURL(AppDefine.ImageOptions.ImageJPEG, AppDefine.ImageOptions.ImageFullQuality);
					vm.myProfile.ImageSrc = getImageDataBase64(vm.picSource);
				});
			};

			function getImageDataBase64(data) {
				var source = data.substring(AppDefine.ImageOptions.PrefixImageEmbedding.length - 1);
				if (source.indexOf(",") > -1) {
					source = source.substring(1);
				}
				return source;
			}

			vm.removeFileFn = function () {
				vm.picSource = null;
			}

			vm.CloseProfile = function () {
				$modalInstance.close(AppDefine.ModalConfirmResponse.CLOSE);
			}

			vm.SaveProfile = function () {
				if (!vm.picSource) {
					vm.myProfile.ImageSrc = null;
					vm.myProfile.UPhoto = null;
				}
				UpdateProfile();
			}

			function getAvatarurl(userid, uphoto) {
				return dataContext.user.GetUserImage(userid, uphoto);
			}

			function getUserDetail() {
				if (items != null) {

					vm.myProfile = {
						UserID: items.UserID,
						FName: items.FName,
						LName: items.LName,
						Email: items.Email,
						UPhoto: items.UPhoto
					};

					vm.avatarurl = getAvatarurl(vm.myProfile.UserID, vm.myProfile.UPhoto);
					//dataContext.user.getUserDetail(items).then(
					//    function (response) {
					//    	vm.myProfile = response;
					//    	vm.avatarurl = getAvatarurl( vm.myProfile.UserID, vm.myProfile.UPhoto );
					//        if (!$.isEmptyObject(vm.myProfile.ImageSrc)) {
					//		vm.picSource = AppDefine.DATA_IMAGE_TYPE + vm.myProfile.ImageSrc;
					//        }
					//    },
					//    function (error) {
					//        cmsBase.cmsLog.error(msg);
					//});
				}
			}

			function UpdateProfile() {
				dataContext.user.UpdateProfile(vm.myProfile).then(
					function (response) {
						if (response.ReturnStatus) {
							var data = response.Data;
							AccountSvc.ProfileChange(data.LName, data.FName, data.UPhoto, data.Email);
							$modalInstance.close(data);
							var msg = cmsBase.translateSvc.getTranslate(response.ReturnMessage[0]);
							cmsBase.cmsLog.success(msg);
						}
						else {
							var msg = cmsBase.translateSvc.getTranslate(response.ReturnMessage[0]);
							cmsBase.cmsLog.warning(msg);
						}
					}
					, function (response) {
						var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.EDIT_FAIL_MSG);
						cmsBase.cmsLog.error(msg);
					}
				);
			}

			function getImageDataBase64(data) {
				var source = data.substring(AppDefine.ImageOptions.PrefixImageEmbedding.length - 1);
				if (source.indexOf(",") > -1) {
					source = source.substring(1);
				}
				return source;
			}
			/* Validator Form Data, end */
			vm.EmailValidFunc = function (email) {
                if (!email) { return; }
				var reg = AppDefine.RegExp.EmailRestriction;
				if (!reg.test(email)) {
					return cmsBase.translateSvc.getTranslate(AppDefine.Resx.EMAIL_INVALID_MSG);
				}
                return true;
			}
		}
	});
})();
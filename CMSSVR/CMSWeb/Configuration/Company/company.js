(function () {
	'use strict';
	define(['cms', 'DataServices/Configuration/companySvc'], function (cms) {
		cms.register.controller('companyCtrl', ['$http', '$scope', '$element', '$modal', '$filter', 'dialogSvc', 'CompanySvc', 'AccountSvc', 'AppDefine', '$timeout', 'cmsBase', '$q',
            function ($http, $scope, $element, $modal, $filter, dialogSvc, CompanySvc, AccountSvc, AppDefine, $timeout, cmsBase, $q) {
            	$scope.Resx = AppDefine.Resx;
            	$scope.picSource = '';
            	$scope.fileName = '';
            	$scope.valueCompany = {};
            	var mainCanvas;
            	var max_img_size = AccountSvc.UserModel().Settings.CompImgSize;//16384;
            	$scope.fileAccept = AppDefine.FileUploadTypes.Images;
            	var IMAGE_TYPE_STRING = "image";

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
            		mainCanvas.width = 300;
            		mainCanvas.height = 200;
            		$scope.$apply(function () {
            			var ctx = mainCanvas.getContext("2d");
            			ctx.drawImage(image, 0, 0, mainCanvas.width, mainCanvas.height);
            			$scope.picSource = mainCanvas.toDataURL(AppDefine.ImageOptions.ImageJPEG, AppDefine.ImageOptions.ImageFullQuality);
            			$scope.valueCompany.CompanyLogo = getImageDataBase64($scope.picSource);
            		});
            	};

            	function getImageDataBase64(data) {
            		var source = data.substring(AppDefine.ImageOptions.PrefixImageEmbedding.length - 1);
            		if (source.indexOf(",") > -1) {
            			source = source.substring(1);
            		}
            		return source;
            	}

            	CompanySvc.GetCompanyInfo().then(function (data) {
            		$scope.valueCompany = data;
            		$scope.picSource = null;
            		if ($scope.valueCompany.CompanyLogo) {
            			$scope.picSource = AppDefine.ImageOptions.PrefixImageEmbedding + $scope.valueCompany.CompanyLogo;
            		}
            	});

            	$scope.NumberValidFunc = function (number) {
            		if (!number) { return true; }
            		if (!validNumber(number)) {
            			return cmsBase.translateSvc.getTranslate("IS_NUMBER_REQUIRED");
            		}
            		if (number > 365) {
            			return cmsBase.translateSvc.getTranslate("COMPANY_NUMBER_LESS");
            		}
            		return true;
            	}

            	function validNumber(value) {
            		var reg = AppDefine.RegExp.NumberRestriction;
            		return (reg.test(value));
            	}

            	$scope.Save = function () {
            		if (!$scope.picSource) {
            			$scope.valueCompany.CompanyLogo = null;
            		}

            		CompanySvc.UpdateCompanyInfo($scope.valueCompany).then(
						function (data) {
							if (data.ReturnStatus) {
								var msg = cmsBase.translateSvc.getTranslate(data.ReturnMessage[0]);
								cmsBase.cmsLog.success(msg);
							}
							else {
								angular.forEach(data.ReturnMessage, function (msgKey, index) {
									var msg = cmsBase.translateSvc.getTranslate(msgKey);
									cmsBase.cmsLog.warning(msg);
								});
							}
						}
						, function (error) {
							var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.EDIT_FAIL_MSG);
							cmsBase.cmsLog.error(msg);
						});
            	}

            	$scope.removeFileFn = function () {
            		$scope.picSource = null;
            	}

            }]);
	});
})();

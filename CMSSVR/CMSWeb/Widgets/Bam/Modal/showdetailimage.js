(function () {
	'use strict';

	define(['cms', 'Widgets/Bam/Modal/manuallyupload', 'DataServices/SiteSvc'], function (cms) {
	    cms.register.controller('showdetailimageCtrl', showdetailimageCtrl);

	    showdetailimageCtrl.$inject = ['$rootScope', '$scope', 'dataContext', 'cmsBase', '$modalInstance', 'items', 'AppDefine', '$modal', 'SiteSvc'];

	    function showdetailimageCtrl($rootScope, $scope, dataContext, cmsBase, $modalInstance, items, AppDefine, $modal, SiteSvc) {
	       
	        $scope.delMsg = '';
	        //$scope.url = AppDefine.Api.Sites + AppDefine.SiteAPI.GET_FILE;
	        var TypesHeatMap = {
	            Hourly: 1,
	            Daily: 2,
	            Weekly: 3
	        };

	        $scope.cancel = function () {
	            $modalInstance.close();
	        }
	        $scope.siteName = items.siteName ? items.siteName : '';
	        $scope.DVRName = items.DVRName ? items.DVRName : '';

	        $scope.model = [];
	        angular.forEach(items.model, function (f) {
	            if (f.ImageID === items.file.ImageID) {
	                $scope.model.splice(0, 0, f);
	            } else {
	                $scope.model.push(f);
	            }
	        });

	        $scope.showMunuallyUploadEdit = function (file, ChannelID) {

	            if (!$scope.modalShown) {
	                $scope.modalShown = true;
	                var userDeleteInstance = $modal.open({
	                    templateUrl: 'Widgets/Bam/Modal/manuallyupload.html',
	                    controller: 'manuallyuploadCtrl',
	                    size: 'md',
	                    backdrop: 'static',
	                    backdropClass: 'modal-backdrop',
	                    keyboard: false,
	                    resolve: {
	                        items: function () {
	                            return {
	                                Channels: items.Channels,
	                                KDVR: items.KDVR,
	                                File: file,
	                                parKChannel: ChannelID,
	                                IDSchedule: items.IDSchedule,
	                                parentForm: true
	                            }
	                        }
	                    }
	                });

	                userDeleteInstance.result.then(function (data) {
	                    $scope.modalShown = false;

	                    if (!data) {
	                        return;
	                    }

	                    angular.forEach($scope.model, function (f) {
	                        if (f.ImageID === data.reImageID) {
	                            f.ImageName = data.reImageName;
	                            f.ImageUrl = data.reImageUrl;
	                            f.Title = data.reTitle;
	                        }
	                    });

	                    $scope.$applyAsync();
	                    $modalInstance.close(true);
	                });
	            }
	        }

	        $scope.showVideoFn = function (channel) {
	            var data = {
	                PacId: items.KDVR,
	                CamName: channel.Channels.ChannelNo + 1,
	                DvrDate: channel.UpdatedDate
	            }
	            var timeline = AppDefine.TypeTimeLines.Ten_minute;
	            switch (items.IDSchedule) {
	                case TypesHeatMap.Hourly:
	                    timeline = AppDefine.TypeTimeLines.One_Hour;
	                    break;
	                case TypesHeatMap.Daily:
	                    timeline = AppDefine.TypeTimeLines.One_Day;
	                    break;
	                case TypesHeatMap.Weekly:
	                    timeline = AppDefine.TypeTimeLines.One_Day;
	                    break;
	                default:
	                    break;
	            }
	            SiteSvc.GetDVRInfoRebarTransact(timeline, data, cmsBase.GetDVRInfoSuccess, function (errr) {
	            });
	        }
	    }
	});
})();
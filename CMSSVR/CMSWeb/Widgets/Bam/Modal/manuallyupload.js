(function () {
	'use strict';

	define(['cms', 'DataServices/Bam/DistributionSvc', 'Services/dialogService'], function (cms) {
	    cms.register.controller('manuallyuploadCtrl', manuallyuploadCtrl);

	    manuallyuploadCtrl.$inject = ['$rootScope', '$scope', 'dataContext', 'cmsBase', '$modalInstance', 'items', 'AppDefine', '$modal', '$upload', '$timeout', 'DistributionSvc', 'dialogSvc', '$filter', 'Utils'];

	    function manuallyuploadCtrl($rootScope, $scope, dataContext, cmsBase, $modalInstance, items, AppDefine, $modal, $upload, $timeout, DistributionSvc, dialogSvc, $filter, Utils) {
	        var vm = this;

	        $scope.parentForm = items.parentForm;

	        $scope.delMsg = '';
	        //$scope.url = AppDefine.Api.Sites + AppDefine.SiteAPI.GET_FILE;
	        $scope.dateOptions = {
	            formatYear: 'yy',
	            startingDay: 1,
	            showWeeks: false
	        };
	        $scope.haveImage = false;
	        
	        $scope.imageURLClient;
	        
	        function setTime(date, hour, minute, second, milisecond) {
	            if (hour >= 0) date.setHours(hour);
	            if (minute >= 0) date.setMinutes(minute);
	            if (second >= 0) date.setSeconds(second);
	            if (milisecond >= 0) date.setMilliseconds(milisecond);

	            return date;
	        }
	        $scope.cancel = function () {
	            $modalInstance.close();
	        }

            // Get data from form parent
	        $scope.KDVR = items.KDVR ? items.KDVR : 0;
	        vm.parKChannel = items.parKChannel ? items.parKChannel : 0;
	        $scope.LChannels = items.Channels ? items.Channels : [];

	        $scope.Schedules = [
                { ID: 1, Name: 'Hourly' },
				{ ID: 2, Name: 'Daily' },
				{ ID: 3, Name: 'Weekly' }
	        ];

	        $scope.DateClick = function ($event) {
	            $event.preventDefault();
	            $event.stopPropagation();
	            $scope.isopened = !$scope.isopened;
	        }

	        var dateToday = new Date();
	        //$scope.maxdate = setTime(dateToday, 23, 59, 59, 999);
	        $scope.maxdate = moment().add(1, 'days').format('L');
	        $scope.optionsDate = { format: 'L', maxDate: $scope.maxdate, ignoreReadonly: true, showTodayButton: true };
	        $scope.selectedSchedule = $scope.Schedules[1];
	        $scope.selectedChannel = $scope.LChannels[0];
	        $scope.isopened = false;
	        $scope.dateUpload = dateToday;
	        $scope.heatmapmodels = {
	            KDVR: $scope.KDVR,
	            mapsImage: []
	        };
	        
	        var objectupload = {
	            id: 0,
	            complete: 0,
	            prevloaded: 0
	        };
	        var uploadArray = [];

	        $scope.TypeScheduleChanged = function() {
	            if ($scope.selectedSchedule.ID === $scope.Schedules[0].ID) {
	                $scope.optionsDate = { format: 'MM/DD/YYYY HH:00:00', maxDate: $scope.maxdate, ignoreReadonly: true, showTodayButton: true };
	            } else {
	                $scope.optionsDate = { format: 'L', maxDate: $scope.maxdate, ignoreReadonly: true, showTodayButton: true };
	            }
	            $scope.$applyAsync();
	        }

	        function showApplyConfirm() {
	            var modalOptions = {
	                closeButtonText: AppDefine.Resx.BTN_CANCEL,
	                actionButtonText: AppDefine.Resx.BTN_OK,
	                headerText: AppDefine.Resx.HEATMAP_CONFIRM_HEADER,
	                bodyText: AppDefine.Resx.HEATMAP_CONFIRM_IMAGE_EXISTS_MSG
	            };
	            var modalDefaults = {
	                size: 'sm'
	            };

	            dialogSvc.showModal(modalDefaults, modalOptions).then(function (result) {
	                if (result === AppDefine.ModalConfirmResponse.OK) {
	                    InsertImages();
	                }
	            });
	        }

	        $scope.file = null;
	        $scope._tempid = -1;
	        $scope.$on(AppDefine.Events.FILESELECTEDCHANGE, function (event, agr) {
	            $scope.file = agr.file;
	            if ($scope.file) {
	                
	                var reader = new FileReader();
	                reader.onload = (function (f) {
	                    return (function (e) {
	                        var data = e.target.result;
	                        angular.element('#imgClient').attr("src", function () {
	                            return data;
	                        });
	                        $scope.haveImage = true;
	                        $scope.$applyAsync();

	                    })
	                })($scope.file[0]);
	                reader.readAsDataURL($scope.file[0]);
	                
	            } else {
	                //do somthing
	            }
	            
	        });

	        // function upload image when choose from dialog
	        function uploadFromDialog(scope) {
	            var KDVR = $scope.KDVR;
	            var Schedule = $scope.selectedSchedule.Name;
	            var key = $scope.heatmapmodels.mapsImage.length - 1;
	            if ($scope.heatmapmodels.mapsImage[key].ImageByte == undefined || $scope.heatmapmodels.mapsImage[key].ImageByte == null || $scope.heatmapmodels.mapsImage[key].ImageByte == "") return;
	            var config = {
	                url: AppDefine.Api.Distribution + '/uploadfromdialog?KDVR=' + KDVR.toString() + '&id=' + $scope.heatmapmodels.mapsImage[key].ImageID + '&shedule=' + Schedule.toString(),
	                headers: { 'Content-Type': 'multipart/form-data' },
	                data: $scope.heatmapmodels.mapsImage[key].ImageID,
	                file: $scope.heatmapmodels.mapsImage[key].ImageByte
	            };

	            uploadArray = [];

	            $upload.upload(config)
                .progress((function (i) {
                    return function (e) {
                        if (uploadArray.length == 0) {
                            objectupload = {};
                            objectupload.complete = e.loaded;
                            objectupload.id = i;
                            objectupload.prevloaded = e.loaded;
                            uploadArray.push(objectupload);
                        }
                        else {
                            var flag = false;
                            for (var j = 0; j < uploadArray.length; j++) {
                                var value = uploadArray[j];
                                if (i == value.id) {
                                    uploadArray[j].complete = objectupload.complete + e.loaded - objectupload.prevloaded;
                                    uploadArray[j].id = i;
                                    uploadArray[j].prevloaded = e.loaded;
                                    flag = true;
                                    break;
                                }
                            }
                            if (flag == false) {
                                objectupload = {};
                                objectupload.complete = e.loaded;
                                objectupload.id = i;
                                objectupload.prevloaded = e.loaded;
                                uploadArray.push(objectupload);
                            }
                        }
                        var summarize = 0;
                        for (var j = 0; j < uploadArray.length; j++) {
                            var value = uploadArray[j];
                            summarize = summarize + value.complete;
                        }
                        var per = summarize / $scope.AllFileSize;
                        $scope.progressbar = per * 100;
                    };
                })(key)).success(function (data, status, headers, config) {
                    var result = cms.GetResponseData(data);
                    if (result) {
                        var key = $scope.heatmapmodels.mapsImage.length - 1;
                        $scope.heatmapmodels.mapsImage[key].ImgName = result.ImgName;
                    }
                    $scope.$emit(AppDefine.Events.HEAT_MAP.UPLOAD_COMPLETE, data);

                });
	        }

	        $scope.$on(AppDefine.Events.HEAT_MAP.UPLOAD_COMPLETE, function (evt, data) {
	            DistributionSvc.InsertImage($scope.heatmapmodels, $scope.SaveMapsComplete, $scope.SaveMapsError);
	        });

	        $scope.SaveMapsComplete = function (event) {
	            var key = $scope.heatmapmodels.mapsImage.length - 1;
	            var result = {
                    reImageID: event.ImageID,
	                reKChannel: vm.parKChannel,
	                reImageName: event.ImgName,
	                reImageUrl: event.ImageURL,
                    reTitle: event.Title
	            }
	            $scope.heatmapmodels.mapsImage = [];
	            $scope.heatmapmodels = event;
	            cmsBase.cmsLog.success($filter('translate')(AppDefine.Resx.SAVE_MAP_IMAGE_SUCCESS_MSG));

	            $modalInstance.close(result);
	        };

	        $scope.SaveMapsError = function (event) {
	            //  alert('Saving Maps Error!')
	            
	        };

	        function SetHour(value) {
	            if (value < 10) {
	                return "0" + value;
	            } else {
	                return value;
	            }
	        }

	        function InsertImages() {
	            if ($scope.file) {
	                for (var i = 0; i < $scope.file.length; i++) {
	                    var reader = new FileReader();
	                    reader.onload = (function (f) {
	                        return (function (e) {
	                            var data = e.target.result;
	                            if ($scope.heatmapmodels.mapsImage == undefined) $scope.heatmapmodels.mapsImage = [];
	                            $scope.heatmapmodels.mapsImage.push({
	                                ImageID: $scope._tempid--,
	                                ImageURL: e.currentTarget.result,
	                                ImageByte: f,
	                                Channels: {
	                                    ChannelID: $scope.selectedChannel.KChannel,
	                                    ChannelName: $scope.selectedChannel.Name,
	                                    ChannelNo: $scope.selectedChannel.ChannelNo
	                                },
	                                ImgName: f.name,
	                                Title: f.name,
	                                paramUpdatedDate: $scope.dateUpload.toDateParam() + SetHour($scope.dateUpload.getHours()),
	                                schedule:{
	                                    TypeID: $scope.selectedSchedule.ID,
	                                    TypeName: $scope.selectedSchedule.Name
	                                } 
	                            });
	                            $scope.$applyAsync();

	                            //upload image
	                            uploadFromDialog($scope);
	                        })
	                    })($scope.file[i]);

	                    reader.readAsDataURL($scope.file[i]);
	                }
	            }
	            else {

	            }
	        }

	        function CheckExistsImage(params) {
	            DistributionSvc.CheckExistsImage(params, function (data) {
	                if (data === true) {
	                    showApplyConfirm();
	                } else {
	                    InsertImages();
	                }
	            },
				function (error) {
				    cmsBase.cmsLog.error('error');
				});
	        }

	        $scope.SaveUpload = function () {
	            var params = {
	                KDVR: $scope.KDVR,
	                schedule: $scope.selectedSchedule.ID,
	                dateImage: $scope.dateUpload.toDateParam() + SetHour($scope.dateUpload.getHours())
	            };
	            CheckExistsImage(params);
	        }

	        $timeout(function () {
	            if (items.File) {

	                angular.element('#imgClient').attr("src", function () {
	                    return items.File.ImageURL;
	                });
	                $scope.haveImage = true;
	                var result = Enumerable.From(items.Channels).Where(function (x) {
	                    return x.KChannel == vm.parKChannel;
	                }).ToArray();
	                if (result.length > 0) {
	                    $scope.selectedChannel = result[0];
	                } 
	                var rs = Enumerable.From($scope.Schedules).Where(function (x) {
	                    return x.ID == items.IDSchedule;
	                }).ToArray();
	                if (rs.length > 0) {
	                    $scope.selectedSchedule = rs[0];
	                }

	                $scope.dateUpload = new Date(parseInt(items.File.paramUpdatedDate.substring(0,4))
                                            , parseInt(items.File.paramUpdatedDate.substring(4,6)) - 1
                                            , parseInt(items.File.paramUpdatedDate.substring(6,8))
                                            , parseInt(items.File.paramUpdatedDate.substring(8,12)));

	                if ($scope.selectedSchedule.ID === $scope.Schedules[0].ID) {
	                    $scope.optionsDate = { format: 'MM/DD/YYYY HH:00:00', maxDate: $scope.maxdate, ignoreReadonly: true, showTodayButton: true  };
	                }

	                $scope.$applyAsync();
	            }
	        }, 200);
	        
	        $scope.EventRefresh = function () {
	            $scope.haveImage = false;
	        }
	    }
	});
})();
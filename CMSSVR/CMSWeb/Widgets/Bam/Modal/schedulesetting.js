(function () {
    'use strict';

    define(['cms', 'DataServices/Bam/DistributionSvc'], function (cms) {
        cms.register.controller('schedulesettingCtrl', schedulesettingCtrl);

        schedulesettingCtrl.$inject = ['$rootScope', '$scope', 'dataContext', 'cmsBase', '$modalInstance', 'items', 'AppDefine', '$modal', 'bamhelperSvc', 'DistributionSvc', 'dialogSvc', '$timeout'];

        function schedulesettingCtrl($rootScope, $scope, dataContext, cmsBase, $modalInstance, items, AppDefine, $modal, bamhelperSvc, DistributionSvc, dialogSvc, $timeout) {
            var vm = this;

            $scope.delMsg = '';
            $scope.optionsDate = { format: 'L', maxDate: $scope.maxdate, ignoreReadonly: true };
            var dateToday = new Date();
            $scope.dateFrom = dateToday;
            $scope.dateTo = dateToday;
            $scope.dateStart = dateToday;
            $scope.rbTypeActiveTime = 'fromto';
            var weekly = 3;
            vm.querySiteSchedule = '';
            // True: New Schedule 
            // False: Update Schedule
            $scope.flag = true;

            $scope.STime = {
                mytime: ConvertToDate(dateToday),
                hstep: 1,
                mstep: 1,
                ismeridian: false,
            };
            $scope.ETime = {
                mytime: ConvertToDate(dateToday),
                hstep: 1,
                mstep: 1,
                ismeridian: false,
            };
            $scope.groupByField = false;
            $scope.ScheduleType = 1;

            //timepicker
            function ConvertToDate(datetime) {
                var retDate = null;
                if (typeof (datetime) == 'string') {
                    retDate = new Date(parseInt(datetime.substring(0, 4))
                                            , parseInt(datetime.substring(4, 6)) - 1
                                            , parseInt(datetime.substring(6, 8)));
                }
                else {
                    retDate = datetime;
                }
                return retDate;
            }

            function ConvertToTime(time, date) {
                var retDate = null;
                if (typeof (time) == 'string' && typeof (date) == 'string'
                    && time !== '' && date !== '') {
                    retDate = new Date(parseInt(date.substring(0, 4))
                                            , parseInt(date.substring(4, 6)) - 1
                                            , parseInt(date.substring(6, 8))
                                            , parseInt(time.substring(0, 2))
                                            , parseInt(time.substring(3, 5)));
                }
                else {
                    retDate = new Date();
                }
                return retDate;
            }

            $scope.cancel = function () {
                $modalInstance.close();
            }

            vm.treeDefSchedule = {
                Id: 'ID',
                Name: 'Name',
                Type: 'Type',
                Checked: 'Checked',
                Childs: 'Sites',
                Count: 'SiteCount',
                Model: {}
            }

            initDataSchedule();

            vm.treeOptionsSchedule = {
                Node: {
                    IsShowIcon: true,
                    IsShowCheckBox: true,
                    IsShowNodeMenu: true,
                    IsShowAddNodeButton: false,
                    IsShowAddItemButton: false,
                    IsShowEditButton: false,
                    IsShowDelButton: false,
                    IsDraggable: false
                },
                Icon: {
                    Item: 'icon-dvr-2'
                },
                Item: {
                    IsAllowFilter: false,
                    IsShowItemMenu: false
                }, Type: {
                    Folder: 0,
				    Group: 1,
                    File: 2
                },
                CallBack: {
                    //SelectedFn: selectedFn
                }
            }

            vm.treeSiteFilterOnSchedule = null;
            vm.isShowTreeOnSchedule = false;

            $scope.formshow = 1;
            $scope.sumChannels = 64;
            $scope.listChannels = [];
            $scope.NameSchedule = '';
            vm.detailschedule;

            $scope.deleteSchedule = function (schedule, $event) {
                $event.preventDefault();
                $event.stopPropagation();

                var modalOptions = {
                    closeButtonText: AppDefine.Resx.BTN_CANCEL,
                    actionButtonText: AppDefine.Resx.BTN_OK,
                    headerText: AppDefine.Resx.SCHEDULE_DELETE_HEADER,
                    bodyText: AppDefine.Resx.SCHEDULE_DELETE_MSG
                };
                var modalDefaults = {
                    size: 'sm'
                };

                dialogSvc.showModal(modalDefaults, modalOptions).then(function (result) {
                    if (result === AppDefine.ModalConfirmResponse.OK) {

                        DistributionSvc.DeleteDataScheduleTasks(schedule, function (data) {
                            if (data.ReturnStatus) {
                                $timeout(function () {
                                    cmsBase.cmsLog.success(cmsBase.translateSvc.getTranslate(AppDefine.Resx.DELETE_SUCCESS_MSG));
                                    $scope.changeSchType($scope.ScheduleType);
                                }, 1000);
                            } else {
                                cmsBase.cmsLog.success(cmsBase.translateSvc.getTranslate(AppDefine.Resx.DELETE_FAIL_MSG));
                            }
                            
                        },
					    function (error) {
					        cmsBase.cmsLog.error(cmsBase.translateSvc.getTranslate(AppDefine.Resx.DELETE_FAIL_MSG));
					    });

                    }
                });

            }

            $scope.Changeform = function (schedule, $event) {
                $event.preventDefault();
                $event.stopPropagation();

                $scope.formshow = 2;

                // New Schedule
                if (schedule === undefined) {

                    $scope.NameSchedule = '';
                    $scope.flag = true;
                    if ($scope.ScheduleType != weekly) {
                        $scope.STime.mytime = new Date();
                        $scope.ETime.mytime = new Date();
                    }
                    $scope.dateFrom = new Date();
                    $scope.dateTo = new Date();
                    $scope.dateStart = new Date();
                    $scope.rbTypeActiveTime = 'noenddate';

                    bamhelperSvc.checkallNode(vm.treeSiteFilterOnSchedule);
                    $rootScope.$broadcast('cmsTreeRefresh');

                    $scope.listChannels = [];
                    for (var i = 0; i < $scope.sumChannels; i++) {
                        $scope.listChannels.push({
                            ChannelNo: i,
                            Active: false
                        });
                    }

                } else {

                    //Update  schedule
                    $scope.flag = false;
                    vm.detailschedule = schedule;
                    $scope.NameSchedule = vm.detailschedule.TaskName;
                    var SDate = ConvertToDate(vm.detailschedule.paramStartDate);
                    $scope.rbTypeActiveTime = 'noenddate';
                    if (vm.detailschedule.paramEndDate != '') {
                        var EDate = ConvertToDate(vm.detailschedule.paramEndDate);
                        $scope.dateTo = EDate;
                        $scope.rbTypeActiveTime = 'fromto';
                    }

                    if ($scope.ScheduleType != weekly) {
                        $scope.STime.mytime = ConvertToTime(vm.detailschedule.paramStartTime, vm.detailschedule.paramStartDate);
                        $scope.ETime.mytime = ConvertToTime(vm.detailschedule.paramEndTime, vm.detailschedule.paramStartDate);
                    }
                    $scope.dateFrom = SDate;
                    $scope.dateStart = SDate;

                    bamhelperSvc.setNodeSelectedbyDDVR(vm.treeSiteFilterOnSchedule, schedule.Dvrs);
                    $rootScope.$broadcast('cmsTreeRefresh');

                    $scope.listChannels = [];
                    for (var i = 0; i < $scope.sumChannels; i++) {
                        $scope.listChannels.push({
                            ChannelNo: i,
                            Active: false
                        });
                        for (var j = 0; j < schedule.Channels.length; j++) {
                            if (schedule.Channels[j].ChannelNo === i) {
                                $scope.listChannels[i].Active = true;
                            }
                        }
                    }
                }

                $scope.$applyAsync();
            }

            vm.ChoiceChannel = function (index, value) {
                $scope.listChannels[index].Active = value;
            }

            vm.CheckAllChannels = function () {
                for (var i = 0; i < $scope.listChannels.length; i++) {
                    $scope.listChannels[i].Active = true;
                }
            }

            vm.UnCheckAllChannels = function () {
                for (var i = 0; i < $scope.listChannels.length; i++) {
                    $scope.listChannels[i].Active = false;
                }
            }

            $scope.clickOutside = function ($event, element) {


                if (angular.element(element).hasClass('open')) {
                    angular.element(element).removeClass('open');
                    //console.log('have open');
                } else {
                    //console.log('dont have open');
                }
            };
            
            function initDataSchedule() {
                // Create tree site witj DVR

                if (!vm.treeSiteFilterOnSchedule) {
                    showAllSites();
                }

                // Select data from DVR

            }

            function showAllSites() {
                dataContext.siteadmin.getSites(function (data) {
                    $scope.data = data;
                    if ($scope.data && $scope.data.Sites.length > 0 && vm.querySiteSchedule) {
                        vm.treeSiteFilterOnSchedule = angular.copy($scope.data);
                        //$scope.treeSiteFilter.Sites = siteadminService.filterSites($scope.treeSiteFilter, $scope.querySiteHeatMap);
                    } else {
                        vm.treeSiteFilterOnSchedule = $scope.data;
                    }
                    vm.isShowTreeOnSchedule = true;
                    var nameSite = '';
                    var firstDVR = bamhelperSvc.getFirstDVR(vm.treeSiteFilterOnSchedule);
                    //bamhelperSvc.checkallNode(vm.treeSiteFilterOnSchedule);
                    //nameSite = bamhelperSvc.getSitesFromDVR(vm.treeSiteFilterOnSchedule, firstDVR.ParentKey, nameSite);
                    //selectedFnHM(firstDVR);
                    //if ($scope.selectedSiteNameHM === '') {
                    //    $scope.selectedSiteNameHM = nameSite;
                    //}

                }, function (error) {
                    siteadminService.ShowError(error);
                });

            }

            getDataScheduleTask();

            function getDataScheduleTask() {
                var params = {
                    rptDataType: $scope.ScheduleType,
                    KDVR: '',
                    sDate: '',
                    eDate: '',
                    PageNo: '',
                    PageSize: ''
                };
                DistributionSvc.getDataScheduleTasks(params, showDataSchTask, showerror);
            }

            function showDataSchTask(data) {
                setDateUTC(data);
                vm.DataScheduleTask = data;
            }

            function setDateUTC(data)
            {
                angular.forEach(data, function (item) {
                    item.StartDate = ConvertToDate(item.paramStartDate);
                    item.EndDate = item.paramEndDate === '' ? undefined : ConvertToDate(item.paramEndDate);
                });
            }

            function showerror(err) {

            }

            $scope.changeSchType = function (value) {
                $scope.ScheduleType = value;
                $scope.formshow = 1;

                getDataScheduleTask();
            }

            $scope.cancelSchedule = function () {
                $scope.changeSchType($scope.ScheduleType);
            }

            $scope.saveSchedule = function () {
                if ($scope.NameSchedule === '') {
                    //console.log("Task Name No Empty");
                    cmsBase.cmsLog.error(cmsBase.translateSvc.getTranslate(AppDefine.Resx.EMPTRYNAME_MSG));
                    return;
                }
                if ($scope.ETime.mytime < $scope.STime.mytime)
                {
                    cmsBase.cmsLog.error(cmsBase.translateSvc.getTranslate(AppDefine.Resx.ERRENDTIME_MSG));
                    return;
                }
                var StartDate = '';
                var EndDate = '';
                var StartTime = $scope.ScheduleType != weekly ? SetHour($scope.STime.mytime) : undefined;
                var EndTime = $scope.ScheduleType != weekly ? SetHour($scope.ETime.mytime) : undefined;

                if ($scope.rbTypeActiveTime == 'fromto') {
                    if ($scope.dateTo < $scope.dateFrom)
                    {
                        cmsBase.cmsLog.error(cmsBase.translateSvc.getTranslate(AppDefine.Resx.BEGIN_GREATER_END_TIME));
                        return;
                    }
                    StartDate = $scope.dateFrom.toDateParam();
                    EndDate = $scope.dateTo.toDateParam();
                } else {
                    StartDate = $scope.dateStart.toDateParam();
                }

                var DVRList = [];
                bamhelperSvc.getDvrsFromNode(vm.treeSiteFilterOnSchedule, DVRList);

                if (DVRList.length === 0) {
                    cmsBase.cmsLog.error(cmsBase.translateSvc.getTranslate(AppDefine.Resx.SELECTDVR_MSG));
                    return;
                }

                var listChannel = Enumerable.From($scope.listChannels)
                                .Where(function (x) { return x.Active == true }).ToArray();

                if (listChannel.length === 0) {
                    cmsBase.cmsLog.error(cmsBase.translateSvc.getTranslate(AppDefine.Resx.SELECTCHANNEL_MSG));
                    return;
                }

                var params = {
                    TaskID: $scope.flag == true ? 0 : vm.detailschedule.TaskID,
                    TaskName: $scope.NameSchedule,
                    paramStartTime: StartTime,
                    paramEndTime: EndTime,
                    paramStartDate: StartDate,
                    paramEndDate: EndDate,
                    Channels: Enumerable.From($scope.listChannels)
                                .Where(function (x) { return x.Active == true })
                                .Select(function (x) {
                                    return {
                                        ChannelID: x.ChannelNo,
                                        ChannelName: '',
                                        ChannelNo: x.ChannelNo
                                    }
                                }).ToArray(),
                    Dvrs: DVRList,
                    scheduleType: $scope.ScheduleType
                }
                if ($scope.flag == true) {
                    DistributionSvc.InsertDataScheduleTasks(params, success, error);
                } else {
                    DistributionSvc.UpdateDataScheduleTasks(params, success, error);
                }
                
            }

            function success(data) {
                if (data)
                {
                    var rs = data;
                    cmsBase.cmsLog.success(cmsBase.translateSvc.getTranslate(AppDefine.Resx.SAVE_MAP_IMAGE_SUCCESS_MSG));
                    $scope.changeSchType($scope.ScheduleType);
                }
            }

            function error() {

            }

            function SetHour(value) {
                var hour = value.getHours() < 10 ? "0" + value.getHours() : value.getHours();
                var minute = value.getMinutes() < 10 ? "0" + value.getMinutes() : value.getMinutes();
                return hour.toString() + minute.toString() + '00'; // format HH:mm:00
            }

        }
    });
})();
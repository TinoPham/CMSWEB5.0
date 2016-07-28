(function () {
    'use strict';

    define(['cms'], function (cms) {
        cms.register.controller('sitemonitorCtrl', sitemonitorCtrl);

        sitemonitorCtrl.$inject = ['$scope', 'dataContext', 'cmsBase', 'AppDefine', '$timeout', '$filter'];

        function sitemonitorCtrl($scope, dataContext, cmsBase, AppDefine, $timeout, $filter) {

            var NO_DVR_FOUND = 'NO_DVR_FOUND';
            var BEGIN_GREATER_END_TIME = 'BEGIN_GREATER_END_TIME';

            $scope.dateOptions = {
                formatYear: 'yy',
                startingDay: 1,
                showWeeks: false
            };

            $scope.datestatus = {}

            $scope.dateAlert = {
                startDate: setTime(new Date(), 0, 0, 0, 0),
                endDate: setTime(new Date(), 23, 59, 59, 999)
            }

            $scope.isNoData = true;
            $scope.maxRowAlert = 999999;
            $scope.takeAlert = $scope.maxRowAlert;
            $scope.topList = [10, 20, 30, 50];

            $scope.setTop = function (top) {
                if (!top) {
                    $scope.takeAlert = $scope.maxRowAlert;
                } else {
                    $scope.takeAlert = top;
                }

                $scope.BodyAlert = buildBodyAlert($scope.AlertInfos, $scope.dvrInfo, $scope.takeAlert);
            }

            $scope.jqueryScrollbarOptions = {
                onInit: function() {
                    var scroll = angular.element('.scrollbar-dynamic');
                    scroll.css('height', '100%');
                }
            }

            $scope.startopen = function ($event, elementOpened) {
                $event.preventDefault();
                $event.stopPropagation();

                $scope.datestatus[elementOpened] = !$scope.datestatus[elementOpened];
            };

            $scope.endopen = function ($event, elementOpened) {
                $event.preventDefault();
                $event.stopPropagation();

                $scope.datestatus[elementOpened] = !$scope.datestatus[elementOpened];
            };

            $scope.predicate = 'Info.SiteName';
            $scope.reverse = true;
            $scope.order = function (predicate) {
                $scope.reverse = ($scope.predicate === predicate) ? !$scope.reverse : false;
                $scope.predicate = predicate;
            };

            $scope.getNumAlert = function (id, alerts) {
                var alert = Enumerable.From(alerts).Where(function (x) { return x.AlertTypeId === id; }).FirstOrDefault();
                if (!alert) return 0;
                return alert.TotalAlert;
            }

            $scope.countDVR = function () {
                if (!$scope.BodyAlert) return 0;
                return $scope.BodyAlert.length;
            }

            $scope.init = function(model) {
                active(model);
            }

            $scope.refesh = function() {
                builddata();
            }

            $scope.fullSize = function () {
                if (!$scope.isMax)
                    $scope.isCollapsed = false;
                $scope.isMax = !$scope.isMax;
            }

            $scope.searchFn = function () {
                builddata();
            }

            //$scope.$watchGroup(['dateAlert.startDate', 'dateAlert.endDate', '$parent.selectedNode'], function () {
            $scope.$watchGroup(['$parent.selectedNode'], function () {
                builddata();

            });

            function builddata() {
                if (!$scope.$parent.selectedNode || !$scope.dateAlert.startDate || !$scope.dateAlert.endDate) return;

                var startdate = setTime($scope.dateAlert.startDate, 0, 0, 0);
                var enddate = setTime($scope.dateAlert.endDate, 23, 59, 59, 999);

                var kdvrs = [];
                $scope.dvrInfo = [];
                getKdvrs($scope.$parent.selectedNode, kdvrs, $scope.dvrInfo);
                var alertreq = {
                    Dvrs: kdvrs,
                    Begin: startdate,
                    End: enddate,
                };

                $timeout(function () {
                    getSiteAlertMonitor(alertreq);
                }, 0);
            }

            function setTime(date, hour, minute, second, milisecond) {
                if (hour >= 0) date.setHours(hour);
                if (minute >= 0) date.setMinutes(minute);
                if (second >= 0) date.setSeconds(second);
                if (milisecond >= 0) date.setMilliseconds(milisecond);
                return date;
            }

            function getKdvrs(node, kdvrs, dvrInfo) {
                if (node && node.Type !== AppDefine.NodeType.DVR && node.Sites.length) {
                    var nodeLen = node.Sites.length;
                    for (var i = 0; i < nodeLen; i++) {
                        if (node.Sites[i].Type === AppDefine.NodeType.DVR) {
                            var n = node.Sites[i];
                            kdvrs.push(n.ID);
                            dvrInfo.push({ Id: n.ID, Name: n.Name, SiteName: node.Name, SiteKey: node.ID });
                        } else {
                            getKdvrs(node.Sites[i], kdvrs, dvrInfo);
                        }
                    }
                }
            }

            function getSiteAlertMonitor(alertdate) {

                if (alertdate.Dvrs.length === 0) {
                    //var msg = cmsBase.translateSvc.getTranslate(NO_DVR_FOUND);
                    //cmsBase.cmsLog.error(msg);
                    return;
                }

                var sDate = new Date(alertdate.Begin);
                var eDate = new Date(alertdate.End);

                if (sDate > eDate) {
                    var msg = cmsBase.translateSvc.getTranslate(BEGIN_GREATER_END_TIME);
                    cmsBase.cmsLog.error(msg);
                    return;
                }

                var format = 'yyyy-MM-dd HH:mm:ss';

                alertdate.Begin = $filter('date')(sDate, format);
                alertdate.End = $filter('date')(eDate, format);

                dataContext.sitealert.GetSiteAlertByDvrs(alertdate,
                    function (data) {

                        if (data && data.length === 0) {
                            $scope.isNoData = true;
                            return;
                        }

                        $scope.isNoData = false;

                        $scope.AlertInfos = data;

                        // GETALERTTYPE
                        GetAllAlertTypes();

                        //GETALERT LIST
                        $scope.BodyAlert = buildBodyAlert($scope.AlertInfos, $scope.dvrInfo, $scope.takeAlert);

                    },
                    function (error) {
                        var data = error;
                    }
                    );
            }

            function buildBodyAlert(data, dvrInfors, take) {
                var dvrBody = Enumerable.From(data)
                    .GroupBy("$.Kdvr", null, function (key, g) {
                    return { Kdvr: key, Alert: g.ToArray(), Total: g.Sum("$.TotalAlert") }
                    })
                    .OrderByDescending(function (x) { return x.Total; })
                    .Take(take)
                    .ToArray();

                return mathSitesAndAlert(dvrInfors, dvrBody);
            }

            function mathSitesAndAlert(dvrInfors, alertInfors) {
                var result = [];
                var alertLen = alertInfors.length;
                for (var i = 0; i < alertLen; i++) {
                    var dvr = findSiteInfo(dvrInfors, alertInfors[i].Kdvr);
                    if (dvr) {
                        result.push({ Info: dvr, TypeAlerts: alertInfors[i] });
                    }
                }
                return result;
            }

            function findSiteInfo(dvrInfors, id) {
                return Enumerable.From(dvrInfors).Where(function (x) { return x.Id === id; }).FirstOrDefault();
            }

            function GetAllAlertTypes() {
                dataContext.sitealert.GetAllAlertTypes(
                    function (data) {
                        $scope.AlertTypes = data;
                        $scope.Header = getAlertHeader($scope.AlertTypes, $scope.AlertInfos);
                        var alertTypesLinQ = Enumerable.From($scope.AlertTypes);
                        var headerLinQ = Enumerable.From($scope.Header);
                        $scope.HeaderTitle = alertTypesLinQ.Join(headerLinQ, "$.Id", "$", function (g, c) { return { Title: g, Total: sumOfAlert(c, $scope.AlertInfos) } }).ToArray();

                    },
                    function (error) {
                        var data = error;
                    }
                    );
            }

            function sumOfAlert(id, alertInfos) {
                return Enumerable.From(alertInfos)
                     .Where(function (x) { return x.AlertTypeId === id; })
                    .Select(function (x) { return x.TotalAlert; })
                    .Sum();
            }

            function getAlertHeader(alertTypes, alertInfos) {
                return Enumerable.From(alertInfos)
                    .Select(function (x) {
                        return x.AlertTypeId;
                    })
                    .Distinct().ToArray();
            }

            function active(model) {
                $scope.selected = model;
                var kdvrs = [];
                $scope.dvrInfo = [];
                getKdvrs(model, kdvrs, $scope.dvrInfo);

                var startdate = $scope.dateAlert.startDate;
                var enddate = $scope.dateAlert.endDate;

                var alertreq = {
                    Dvrs: kdvrs,
                    Begin: startdate,
                    End: enddate,
                };

                getSiteAlertMonitor(alertreq);
            }
        }
    });
})();
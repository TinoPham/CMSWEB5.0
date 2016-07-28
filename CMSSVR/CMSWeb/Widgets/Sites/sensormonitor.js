(function () {
    'use strict';

    define(['cms', 'Scripts/Services/exportSvc'], function (cms) {
        cms.register.controller('sensormonitorCtrl', sensormonitorCtrl);

        sensormonitorCtrl.$inject = ['$scope', 'dataContext', 'cmsBase', 'AppDefine', '$timeout', '$filter', 'exportSvc', '$window', '$stateParams'];

        function sensormonitorCtrl($scope, dataContext, cmsBase, AppDefine, $timeout, $filter, exportSvc, $window, $stateParams) {
        	var NO_DVR_FOUND = 'NO_DVR_FOUND';
        	var BEGIN_GREATER_END_TIME = 'BEGIN_GREATER_END_TIME';
        	$scope.sensorSnapshotUrl = AppDefine.Api.SiteAlerts + '/GetSensorSnapshot';
        	var format = 'yyyy-MM-dd HH:mm:ss';
        	$scope.isBusy = true;
        	$scope.dateOptions = {
        		formatYear: 'yy',
        		startingDay: 1,
        		showWeeks: false
        	};

        	$scope.datestatus = {}
        		
        	$scope.$on("boxclick", function (event, param) {
        		var nameDVR = param.name;
        		$scope.nameDVR = param.name;
        		if (nameDVR === AppDefine.Resx.NUMBER_SENSOR) {
        			$scope.dateAlert = {
        				startDate: setTime(new Date(param.date), 0, 0, 0, 0),
        				endDate: setTime(new Date(param.date), 23, 59, 59, 999)
        			}
        			$scope.refesh();
        		}
        	});

        	if ($stateParams.obj != null && $stateParams.obj.param.name === AppDefine.Resx.NUMBER_SENSOR) {
        		$scope.dateAlert = {
        			startDate: setTime(new Date($stateParams.obj.param.date), 0, 0, 0, 0),
        			endDate: setTime(new Date($stateParams.obj.param.date), 23, 59, 59, 999)
        		}
        	}
        	else {
        		$scope.dateAlert = {
        			startDate: setTime(new Date(), 0, 0, 0, 0),
        			endDate: setTime(new Date(), 23, 59, 59, 999)
        		}
        	}
        	

            $scope.Interval = 3000;
            $scope.sDetailSelected = null;
            $scope.isNoData = true;
            $scope.maxRowAlert = 999999;
            $scope.takeAlert = $scope.maxRowAlert;
            $scope.isShowChart = true;
            
            $scope.dateSelected = {};
            $scope.sensorSelected = {};
            $scope.selectedDate = function (date) {
                $scope.dateSelected = date;
                $scope.sensorSelected = {};
                $scope.sDetailSelected = null;
            }
            
            $scope.selectedSensor = function (date, sensor) {
                $scope.dateSelected = date;
                $scope.sensorSelected = sensor;
                $scope.sDetailSelected = null;
                $scope.expandSensor(sensor);
            }

        
            $scope.expandSensor = function (sensor, reserve) {
                //if (presensor) {
                //    if (presensor.Kdvr === sensor.Kdvr) {
                //        $scope.isReversed = !$scope.isReversed;
                //    }
                //} else {
                //    presensor = sensor;
                //    $scope.isReversed = !$scope.isReversed;
                //}
                sensor.reserve = reserve;
                if (sensor && sensor.Details.length > 0) return;
                //expandsensor = sensor;
                
                getSensorsDetail(sensor);

            }

            var dateExpand;
            $scope.expandDate = function (date, Reversed) {
                //if (dateExpand && dateExpand.Time === date.Time) {
                //    dateExpand.reserve = Reversed;
                //} else {
                //    dateExpand = date;
               //     dateExpand.reserve = true;
                //}

                date.reserve = Reversed;
            }

            $scope.ChartShow = function() {
                $scope.isShowChart = !$scope.isShowChart;
            }

            $scope.selectDetail = function(detail, sensor) {
                $scope.sDetailSelected = detail;
            }

            $scope.startopen = function ($event, elementOpened, elementClose) {
                $event.preventDefault();
                $event.stopPropagation();

                $scope.datestatus[elementOpened] = !$scope.datestatus[elementOpened];

                
                $timeout(function () {
                    setDisplayDatepicker(elementOpened, elementClose);
                }, 200);
                
            };

            $scope.endopen = function ($event, elementOpened, elementClose) {
                $event.preventDefault();
                $event.stopPropagation();

                $scope.datestatus[elementOpened] = !$scope.datestatus[elementOpened];
                


                $timeout(function () {
                    setDisplayDatepicker(elementOpened, elementClose);
                }, 200);
                
            };

            function setDisplayDatepicker(elementOpened, elementClose) {
                if ($scope.datestatus[elementOpened] === true && $scope.datestatus[elementClose] === true) {
                    var elemS = $('#startDateSM');
                    var elemE = $('#endDateSM');

                    if (parseInt(elemS.position().top) == parseInt(elemE.position().top)) {
                        var SW = parseInt(elemS.width(), 10) + parseInt(elemE.width(), 10);
                        if (SW < 508) {
                            $scope.datestatus[elementClose] = !$scope.datestatus[elementOpened];
                        }
                    } else {
                        $scope.datestatus[elementClose] = !$scope.datestatus[elementOpened];
                    }
                }
            }

            $scope.predicate = 'Info.SiteName';
            $scope.reverse = true;
            $scope.order = function (predicate) {
                $scope.reverse = ($scope.predicate === predicate) ? !$scope.reverse : false;
                $scope.predicate = predicate;
            };

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
                //console.log('$scope.fullSize')
            }

            
            var setheight = angular.element('.sites-manage');


            $scope.bodyHeight = setheight[0].clientHeight - findOffsetTop(setheight[0]);;

            function findOffsetTop(elm) {
                var result = 0;
                if (elm.offsetParent) {
                    do {
                        result += elm.offsetTop;
                    } while (elm = elm.offsetParent);
                }
                return result;
            }

            var scrollelement;
            $scope.sensorScrollbarOptions = {
                ignoreMobile: true,
                onInit: function () {
                   
                    scrollelement = angular.element('.sensormonitor');
                    var oftop = findOffsetTop(scrollelement[0]);
                    var height = $window.innerHeight - oftop - 65 + 'px';
                    scrollelement.css('height', height.toString());

                }
                ,onUpdate: function () {
                    if ($scope.isMax === true) {
                        scrollelement.css('height', $scope.bodyHeight + 'px');
                    } else {
                        var oftop = findOffsetTop(scrollelement[0]);
                        var height = $window.innerHeight - oftop - 65 + 'px';
                        scrollelement.css('height', height.toString());
                    }
                    //console.log('sensormonitor')
                }
            }

            $scope.searchFn = function () {
                builddata();
            }

            $scope.$on(AppDefine.Events.SITE_TAPS.CHANGE_NODE_TREE, function (e, node) {
                builddata();
                //console.log('Last Alert$scope.$watchGroup');
            });
            //$scope.$watchGroup(['$parent.selectedNode'], function (oldVal, newVal) {
            //    if (oldVal !== newVal) {
            //        builddata();
            //        console.log('$scope.$watchGroup([$parent.selectedNode]');
            //    }
            //});

            var fmstr = 'yyyyMMdd';
            $scope.exportToXls = function() {
                var datetimeexport = $filter('date')(Date.now(), fmstr);
                if ($scope.SensorsTable && $scope.SensorsTable.length > 0) {
                    var sheetName = cmsBase.translateSvc.getTranslate('SENSOR_MONITOR') + '-' + datetimeexport.toString();
                    var table = createTable($scope.SensorsTable);
                    exportSvc.ToXls(table, sheetName);
                }
            }

            $scope.exportToCSV = function() {
                var datetimeexport = $filter('date')(Date.now(), fmstr);
                if ($scope.SensorsTable && $scope.SensorsTable.length > 0) {
                    var sheetName = cmsBase.translateSvc.getTranslate('SENSOR_MONITOR') + '-' + datetimeexport.toString();
                    exportSvc.ToSensorCSV($scope.SensorsTable,dateExpand, sheetName);
                }

            }

            $scope.exportToPdf = function() {
                var datetimeexport = $filter('date')(Date.now(), fmstr);
                if ($scope.SensorsTable && $scope.SensorsTable.length > 0) {
                    var sheetName = cmsBase.translateSvc.getTranslate('SENSOR_MONITOR') + '-' + datetimeexport.toString();
                    //var table = createTable($scope.SensorsTable);
                    //var tableEl = angular.element(table);
                    exportSvc.ToSensorPdf($scope.SensorsTable,dateExpand, sheetName);
                }
            }


            function createTable(SensorsTable) {
                var excel = "<table>";
                // Row Vs Column
                var rowCount = 1;
                excel += "<tbody>";
                SensorsTable.forEach(function (date) {
                    excel += "<tr>";

                    excel += "<td>" + "Date: " + $filter('date')(date.Time, "dd/MM/yyyy") + "</td>";
                    excel += "<td>" + "Total: " + date.Total + "</td>";
                    rowCount++;
                    excel += '</tr>';

                    if (date.Sensors.length > 0) {
                        date.Sensors.forEach(function (sen) {
                            excel += "<tr>";
                            excel += "<td></td>";
                            excel += "<td>" + "Site: " + sen.SiteName + "(" + sen.TotalAlert + ")" + "</td>";
                            excel += "<td>" + "DVR Id: " + sen.DVRID + "</td>";
                            rowCount++;
                            excel += '</tr>';

                            if (sen.Details.length > 0 && date.reserve === true && sen.reserve === true) {
                                sen.Details.forEach(function (det) {
                                    excel += "<tr>";
                                    excel += "<td></td>";
                                    excel += "<td></td>";
                                    excel += "<td>" + "Channel: " + det.ChannelName + "</td>";
                                    excel += "<td>" + "Time: " + det.Time + "</td>";
                                    excel += "<td>" + "Description: " + det.Description + "</td>";
                                    rowCount++;
                                    excel += '</tr>';
                                });
                            }
                        });
                    }

                });
                excel += "</tbody>";
                excel += '</table>';
                return excel;
            }

            function builddata() {
                $scope.isBusy = true;
                if (!$scope.$parent.selectedNode || !$scope.dateAlert.startDate || !$scope.dateAlert.endDate) return;

                var startdate = setTime($scope.dateAlert.startDate, 0, 0, 0, 0);
                var enddate = setTime($scope.dateAlert.endDate, 23, 59, 59,999);

                var kdvrs = [];
                $scope.dvrInfo = [];
                getKdvrs($scope.$parent.selectedNode, kdvrs, $scope.dvrInfo);
                var alertreq = {
                    Dvrs: kdvrs,
                    Begin: new Date(startdate),
                    End: new Date(enddate),
                };

                $timeout(function () {
                    sensorsAlertMonitor(alertreq).finally(function () { $scope.isBusy = false; });;
                }, 0, false);
            }

            function setTime(date, hour, minute, second, milisecond) {
                // var date = $filter('date')(dateget, "yyyyMMdd", 'UTC');
                if (hour >= 0) date.setHours(hour);
                if (minute >= 0) date.setMinutes(minute);
                if (second >= 0) date.setSeconds(second);
                if (milisecond >= 0) date.setMilliseconds(milisecond);

                //date.setUTCFullYear(date.getFullYear(), date.getMonth(), date.getDate());

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

            function sensorsAlertMonitor(alertdate) {

                var defer = cmsBase.$q.defer();

                if (alertdate.Dvrs.length === 0) {
                    //var msg = cmsBase.translateSvc.getTranslate(NO_DVR_FOUND);
                    //cmsBase.cmsLog.error(msg);
                    $scope.isNoData = true;
                    defer.resolve();
                    return defer.promise;
                }

                var sDate = new Date(alertdate.Begin);
                var eDate = new Date(alertdate.End);

                if (sDate > eDate) {
                    var msg = cmsBase.translateSvc.getTranslate(BEGIN_GREATER_END_TIME);
                    cmsBase.cmsLog.error(msg);
                    defer.resolve();
                    return defer.promise;
                }
                
                alertdate.Begin = $filter('date')(sDate, format);
                alertdate.End = $filter('date')(eDate, format);

                dataContext.sitealert.GetSensorsAlertByDvrs(alertdate,
                    function(data) {

                        if (data && data.length === 0) {
                            $scope.isNoData = true;
                            defer.resolve();
                            return defer.promise;
                        }                        
                        $scope.SensorsTable = buildSensorsBody(data, $scope.dvrInfo, $scope.takeAlert);
                        $scope.SensorsChart = buildSensorsChart(data, $scope.dvrInfo, $scope.takeAlert);

                        $scope.isNoData = false;

                        $scope.imageShow = function(image){
                                $timeout((function(){
                                $('.chanel_box').each(function() { // the containers for all your galleries
                                $(this).magnificPopup({
                                    delegate: 'a.zooming', // the selector for gallery item
                                    type: 'image',
                                        removalDelay: 300,
                                        mainClass: 'mfp-fade',
                                    gallery: {
                                      enabled:true
                                    }
                                })});
                                }), 300, false);
                             } /*End js function for popup image*/

                        defer.resolve();

                        /*Set timeout for imageShow() */
                    },
                    function(error) {
                        var data = error;
                        defer.reject();
                    }
                );
                return defer.promise;
            }

			$scope.dataSource = function() {
				return {
					chart: {
						showvalues: "1",
						plotgradientcolor: "",
						canvasbgalpha: "0",
						bgalpha: "0",
						divlineColor: "#999999",
						divLineDashed: "1",
						divLineDashLen: "1",
						divLineGapLen: "1",
						theme: "fint",
						placeValuesInside: '0',
						valueFontColor: "#000",
						plotborderalpha: "0",
						canvasborderalpha: "0",
						showborder: "0",
						showalternatehgridcolor: "0",
						rotatelabels: "1",
						slantlabels: "1",
						showtooltipshadow: "0",
						palettecolors: "83C1EF",
						rotatevalues: '0',
						labelDisplay: 'AUTO',
						maxLabelHeight: "90",
						useEllipsesWhenOverflow: "1"
					},
					data: $scope.SensorsChart
				}
			}

            function getSensorsDetail(sensor) {
                
                dataContext.sitealert.GetAlertSensorsDetails({ kdvr: sensor.Kdvr, date: new Date(sensor.TimeZone).toDateParam() },
                    function (data) {
                        if (!data) return;
                        sensor.Details = data;
                        sensor.sensorSnapshotUrl = AppDefine.Api.SiteAlerts + '/GetSensorSnapshot';
                    },
                    function (error) {
                        var data = error;
                    }
                );
            }

            function buildSensorsBody(sensorsInfos, dvrInfors, take) {

                $scope.matchSensors = mathSitesAndSensors(dvrInfors, sensorsInfos);

                var dvrBody = Enumerable.From($scope.matchSensors)
                    .GroupBy("$.TimeZone", null, function (key, g) {
                        return { Time: key, Sensors: g.ToArray(), Total: g.Sum("$.TotalAlert") }
                    })
                    .OrderByDescending(function (x) { return x.Time; })
                    .Take(take)
                    .ToArray();
                return dvrBody;
            }

            function buildSensorsChart(sensorsInfos, dvrInfors, take) {

                var data = $scope.matchSensors;
                var dvrBody = Enumerable.From(data)
                    .GroupBy("$.SiteKey", null, function (key, g) {
                        return {key: key, sensors: g.ToArray(), value: g.Sum("$.TotalAlert") }
                    })
                    .Select(function(x) {
                        return {
                            key: x.key,
                            label: x.sensors[0].SiteName,
                            //tooltext: buildSensorTooltip(x),
                            value: x.value
                        };
                    })
                    .OrderByDescending(function (x) { return x.value; })
                    .Take(take)
                    .ToArray();
                return dvrBody;
            }

            //function buildSensorTooltip(val) {
            //    return "<div class='chart-tooltip'><div class='tooltipImg'><img src='/{{val.key}}.jpg'  /></div><div class='tooltipHeader'>$label</div><div class='tooltipContent'>" + val.sensors[0].SiteName + ": $value</div></div>";
            //}

            function mathSitesAndSensors(dvrInfors, sensorInfos) {

                var dvrInfoLinQ = Enumerable.From(dvrInfors);
                var sensorsLinQ = Enumerable.From(sensorInfos);
                return dvrInfoLinQ.Join(sensorsLinQ, "$.Id", "$.Kdvr", function (g, c) { return { SiteKey: g.SiteKey, SiteName: g.SiteName, Kdvr: g.Id, DVRID: g.Name, TimeZone: c.TimeZone, TotalAlert: c.TotalAlert, Details:[] } }).ToArray();
            }

            function active(model) {
                $scope.isBusy = true;
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

                sensorsAlertMonitor(alertreq).finally(function() { $scope.isBusy = false; });
            }

            $scope.time = function (detail,$index) {
                if (detail.SnapShot.length == 0) return detail.Time;

                detail.Time.split(':');
                return $filter('date')(getChannelTime(detail,$index),"hh:mm:ss a");
            }

            function getChannelTime(details, $index)
            {
                var imageName = details.SnapShot[$index];
                var timeStamp = imageName.split("_")[2].split(".")[0];

                var dateDefault = new Date(details.FullTime);

                var deltatime = timeStamp - details.TimeZone;

                dateDefault.setSeconds(dateDefault.getSeconds() + deltatime);
                return dateDefault;
            }

            function UnixTime_To_Date(unix)
            {
                var date = new Date(unix * 1000);
                return date;
            }
        }
    });
})();
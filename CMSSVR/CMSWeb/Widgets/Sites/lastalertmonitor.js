(function () {
	'use strict';

	define([
        'cms',
        'widgets/sites/ignorealert',
        'Scripts/Services/exportSvc'
	], function (cms) {
		cms.register.controller('lastalertmonitorCtrl', lastalertmonitorCtrl);

		lastalertmonitorCtrl.$inject = ['$scope', 'dataContext', 'cmsBase', 'AppDefine', '$timeout', '$filter', '$modal', 'exportSvc', '$window', '$stateParams', 'AccountSvc'];

		function lastalertmonitorCtrl($scope, dataContext, cmsBase, AppDefine, $timeout, $filter, $modal, exportSvc, $window, $stateParams, AccountSvc) {
		    var vm = this;

			var NO_DVR_FOUND = 'NO_DVR_FOUND';
			var BEGIN_GREATER_END_TIME = 'BEGIN_GREATER_END_TIME';
			$scope.isBusy = false;
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
			var alertOfflineType = 32;
			$scope.isNoData = true;
			$scope.maxRowAlert = 999999;
			$scope.takeAlert = $scope.maxRowAlert;
			$scope.topList = [10, 20, 30, 50];
			$scope.isMobile = cmsBase.isMobile;
			$scope.nameDVR = "";

			var setheight = angular.element('.sites-manage');


			$scope.bodyHeight = setheight[0].clientHeight - findOffsetTop(setheight[0]);

			$scope.$on("boxclick", function (event, param) {
				var nameDVR = param.name;
				$scope.nameDVR = param.name;
				if (nameDVR !== AppDefine.Resx.NUMBER_SENSOR) {
					$scope.refesh();
				}
			});

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
			$scope.ScrollbarOptions = {
				ignoreMobile: true,
				onInit: function () {
					scrollelement = angular.element('.lastalertevent');
					var oftop = findOffsetTop(scrollelement[0]);
					var height = $window.innerHeight - oftop - 65 + 'px';
					scrollelement.css('height', height.toString());
				}
                , onUpdate: function () {
                	if ($scope.isMax === true) {
                		scrollelement.css('height', $scope.bodyHeight + 'px');
                	} else {
                		var oftop = findOffsetTop(scrollelement[0]);
                		var height = $window.innerHeight - oftop - 65 + 'px';
                		scrollelement.css('height', height.toString());
                	}
                }
			}

			$scope.setTop = function (top) {
				if (!top) {
					$scope.takeAlert = $scope.maxRowAlert;
				} else {
					$scope.takeAlert = top;
				}
				var data = recalculateHeaderForSetTop($scope.AlertInfos, $scope.takeAlert);
				calcHeaderTitlte($scope.AlertTypes, data);
				$scope.BodyAlert = buildBodyAlert($scope.AlertInfos, $scope.dvrInfo, $scope.takeAlert);
				$scope.BodyAlertShow = angular.copy($scope.BodyAlert);

				//console.log('Last Alert $scope.setTops');
			}

			function recalculateHeaderForSetTop(data, take) {
				var kdvrs = Enumerable.From(data)
                 .GroupBy("$.Kdvr", null, function (key, g) {
                 	return { Kdvr: key, Total: g.Sum("$.TotalAlert") }
                 })
                 .OrderByDescending(function (x) { return x.Total; })
                 .Take(take)
                 .Select(function (x) { return x.Kdvr; })
                 .ToArray();

				return Enumerable.From(data)
                 .Where(function (x) {
                 	return Enumerable.From(kdvrs).Where(function (a) { return x.Kdvr === a; }).FirstOrDefault() ? true : false;
                 })
                 .ToArray();
			}

			var fmstr = 'yyyyMMdd-HHmmss';
			$scope.exportToXls = function () {
				var table = angular.element('.site-monitor-table');

				if (!table[0]) {
					return;
				}

				var stream = table[0].outerHTML;

				var streamEl = angular.element(stream);

				var datetimeexport = $filter('date')(Date.now(), fmstr);
				var sheetName = cmsBase.translateSvc.getTranslate('LAST_ALERT_MONITOR') + '-' + datetimeexport.toString();
				streamEl.find('.dropdown').empty();
				streamEl.find('.dropdown').append('x');

				if (streamEl[0]) {
					

				    var userLogin = AccountSvc.UserModel();
				    var sumtable = buildExportSummaryTable(table);

				    var reportInfo = {
				        //TemplateName: "Adhoc" + $filter('date')(new Date(), "yy-MM-dd HHmmss"),
				        TemplateName: sheetName,
				        ReportName: cmsBase.translateSvc.getTranslate('LAST_ALERT_MONITOR'),
				        CompanyID: userLogin.CompanyID,
				        RegionName: '',
				        Location: '',
				        WeekIndex: 0,
				        Footer: '',
				        CreatedBy: userLogin.FName + ' ' + userLogin.LName,
				        CreateDate: $filter('date')(new Date(), AppDefine.ParamDateFormat) // MM/dd/yyyy
				    };

				    var tables = [sumtable];
				    var charts = [];
				    exportSvc.exportXlsFromServer(reportInfo, tables, charts);
				}
			}

			function buildExportSummaryTable(elem) {
			    var rows = [];
			    var rowIndex = 1;
			    $(elem).find('thead').find('tr').each(function () {
			        var headers = [];
			        var colIndex = 1;
			        $(this).filter(':visible').find('th').each(function (index, data) {
			            if ($(this).css('display') != 'none') {
			                    var colpan = $(this).attr('colspan');
			                if (colpan) {
			                    var colpanno = parseInt(colpan);
			                    headers.push({ Value: exportSvc.parseString($(this)), Color: AppDefine.ExportColors.GridHeaderCell, ColIndex: colIndex, RowIndex: rowIndex, CustomerWidth: true, Width: 20, MergeCells: { Cells: colpanno, Rows: 1 } });
			                    colIndex += colpanno;
			                } else {
			                    headers.push({ Value: exportSvc.parseString($(this)), Color: AppDefine.ExportColors.GridHeaderCell, ColIndex: colIndex, RowIndex: rowIndex, CustomerWidth: true, Width: 20 });
			                    colIndex += 1;
			                }
			            }

			        });

			        rows.push({
			            Type: AppDefine.tableExport.Header,
			            ColDatas: headers,
			            GridModels: null
			        });
			        rowIndex += 1;
			    });

			    // Row vs Column
			    $(elem).find('tbody').find('tr').each(function () {
			        var row = [];
			        var colIndex = 1;
			        $(this).filter(':visible').find('td').each(function (index, data) {
			            if ($(this).css('display') != 'none') {
			               
			                    if ($(this).find('ul').length > 0) {
			                        row.push({ Value: "x", Color: AppDefine.ExportColors.GridSumCell, ColIndex: colIndex, RowIndex: rowIndex, CustomerWidth: true, Width: 20 });
			                        colIndex += 1;
			                    } else {

			                        if ($(this).find('a').length > 0) {
			                            row.push({ Value: exportSvc.parseString($(this)), Color: AppDefine.ExportColors.GridSumCell, ColIndex: colIndex, RowIndex: rowIndex, CustomerWidth: true, Width: 20 });
			                            colIndex += 1;
			                        } else {
			                            row.push({ Value: exportSvc.parseString($(this)), Color: AppDefine.ExportColors.GridSumCell, ColIndex: colIndex, RowIndex: rowIndex, CustomerWidth: true, Width: 20 });
			                            colIndex += 1;
			                        }
			                    }
			            }
			        });
			        rows.push({
			            Type: AppDefine.tableExport.Header,
			            ColDatas: row,
			            GridModels: null
			        });
			        rowIndex += 1;
			    });

			    var result = {
			        Name: 'Result',
			        RowDatas: rows,
			        Format: { ColumnFirstSpace: 4, ColumnSpace: 3, ColumnEndSpace: 3 }
			    }
			    return result;

			}

			$scope.exportToCSV = function () {
				var table = angular.element('.site-monitor-table');

				if (!table[0]) {
					return;
				}
				var datetimeexport = $filter('date')(Date.now(), fmstr);
				var sheetName = cmsBase.translateSvc.getTranslate('LAST_ALERT_MONITOR') + '-' + datetimeexport.toString();
				exportSvc.ToCSV(table, sheetName);
			}

			$scope.exportToPdf = function () {
				var table = angular.element('.site-monitor-table');

				if (!table[0]) {
					return;
				}

				var datetimeexport = $filter('date')(Date.now(), fmstr);
				var sheetName = cmsBase.translateSvc.getTranslate('LAST_ALERT_MONITOR') + '-' + datetimeexport.toString();
				exportSvc.ToPdf(table, sheetName);
				return;
			}

			//$scope.jqueryScrollbarOptions = {
			//    onInit: function() {
			//        var scroll = angular.element('.scrollbar-dynamic');
			//        scroll.css('height', '100%');
			//    }
			//}

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
			
			$scope.gotoAlertDetail = function (alert) {

				$scope.$parent.selectDetailSites(alert);
				//console.log('Last Alert gotoAlertDetail');
				var w = $(window).width();
				if (w < 1025) {
					$scope.$parent.showhideTreeTAB(null, null);
				}
			}
	
			$scope.getNumAlert = function (id, alerts) {
				var alert = Enumerable.From(alerts).Where(function (x) { return x.AlertTypeId === id; }).FirstOrDefault();
				if (!alert) return '';
				//console.log('Last Alert  $scope.getNumAlert ');
				return alert.TotalAlert;

			}

			$scope.IgnoreAlerts = function (id, alerts) {
				ignorealertdetail(id, alerts);
				//console.log('Last Alert   $scope.IgnoreAlert');
			}

			function ignorealertdetail(id, alerts) {
				var alert = Enumerable.From(alerts.TypeAlerts.Alert).Where(function (x) { return x.AlertTypeId === id; }).FirstOrDefault();
				if (!alert) return;

				if (alert.AlertTypeId === alertOfflineType) {
					var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.DONT_IGNORE_OFFLINE);
					cmsBase.cmsLog.info(msg);
					return;
				}

				var ignoremodel = {
					Sites: alerts.Info.SiteKey,
					Kdvr: alert.Kdvr,
					KAlert: alert.Kalert,
					Description: ''
				};

				showDialog(ignoremodel).then(function (result) {
					$scope.AlertInfos.splice($scope.AlertInfos.indexOf(alert), 1);
					calcHeaderTitlte($scope.AlertTypes, $scope.AlertInfos);
					$scope.BodyAlert = buildBodyAlert($scope.AlertInfos, $scope.dvrInfo, $scope.takeAlert);
					$scope.BodyAlertShow = angular.copy($scope.BodyAlert);
				}, function (error) {
					var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.IGNORE_ALERT_FAIL);
					cmsBase.cmsLog.error(msg);
				});
			}

			$scope.modalShown = false;
			function showDialog(model) {
				var defer = cmsBase.$q.defer();

				if (!$scope.modalShown) {
					$scope.modalShown = true;
					var ignoreInstance = $modal.open({
						templateUrl: 'widgets/sites/ignorealert.html',
						controller: 'ignorealertCtrl',
						size: 'sm',
						backdrop: 'static',
						backdropClass: 'modal-backdrop',
						keyboard: false,
						resolve: {
							items: function () {
								return model;
							}
						}
					});

					ignoreInstance.result.then(function (data) {
						$scope.modalShown = false;
						if (data) {
							dataContext.sitealert.IgnoreAlerts(data, function (result) {
								defer.resolve(result);
							}, function (error) {
								defer.reject(error);
							});
						}
					});
				} else {
					defer.resolve();
				}

				return defer.promise;
			}

			$scope.countDVR = function () {
				if (!$scope.BodyAlert) return 0;
				//console.log('Last Alert $scope.countDVR ');
				return $scope.BodyAlert.length;
			}

			$scope.init = function (model) {
				active(model);
			}

			$scope.refesh = function () {
				$scope.$parent.refresh().then(function () {
					builddata();
				});
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
			//$scope.$watchGroup(['$parent.selectedNode'], function (oldVal, newVal) {
			//    if (oldVal !== newVal) {
			//        builddata();
			//        console.log('Last Alert$scope.$watchGroup');
			//    }
			//});

			$scope.$on(AppDefine.Events.SITE_TAPS.CHANGE_NODE_TREE, function (e, node) {
				builddata();
				//console.log('Last Alert$scope.$watchGroup');
			});

			function builddata() {
				if (!$scope.$parent.selectedNode || !$scope.dateAlert.startDate || !$scope.dateAlert.endDate) return;

				$scope.isBusy = true;
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


				getSiteAlertMonitor(alertreq).finally(function () {
					$scope.isBusy = false;
				});

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
				var defer = cmsBase.$q.defer();

				if (alertdate.Dvrs.length === 0) {
					//var msg = cmsBase.translateSvc.getTranslate(NO_DVR_FOUND);
					//cmsBase.cmsLog.error(msg);
					$scope.isNoData = true;
					defer.resolve();
					return defer.promise;

				}

				if ($scope.nameDVR != "") {
					if ($scope.nameDVR == AppDefine.Resx.NUMBER_DVROFF) {
						var varOffline = [AppDefine.AlertTypes.DVR_is_off_line];
						alertdate.AlertTypes = varOffline;
					}
					if ($scope.nameDVR == AppDefine.Resx.NUMBER_DVRRECORDLESS) {
						var varLessthan = [AppDefine.AlertTypes.DVR_Record_Less_Than];
						alertdate.AlertTypes = varLessthan;
					}
				} else {
					if ($stateParams.obj != null) {
						if ($stateParams.obj.param.name==AppDefine.Resx.NUMBER_DVROFF) {
							var varOffline = [AppDefine.AlertTypes.DVR_is_off_line];
							alertdate.AlertTypes = varOffline;
						}
						if ($stateParams.obj.param.name == AppDefine.Resx.NUMBER_DVRRECORDLESS) {
							var varLessthan = [AppDefine.AlertTypes.DVR_Record_Less_Than];
							alertdate.AlertTypes = varLessthan;
						}
					}
				}

				var sDate = new Date(alertdate.Begin);
				var eDate = new Date(alertdate.End);

				if (sDate > eDate) {
					var msg = cmsBase.translateSvc.getTranslate(BEGIN_GREATER_END_TIME);
					cmsBase.cmsLog.error(msg);
					defer.reject();
					return defer.promise;
				}

				var format = 'yyyy-MM-dd HH:mm:ss';

				alertdate.Begin = $filter('date')(sDate, format);
				alertdate.End = $filter('date')(eDate, format);

				dataContext.sitealert.GetAlertLastByDvrs(alertdate,
                    function (data) {

                    	if (data && data.length === 0) {
                    		$scope.isNoData = true;
                    		defer.resolve();
                    		return defer.promise;
                    	}

                    	$scope.isNoData = false;

                    	$scope.AlertInfos = data;

                    	// GETALERTTYPE
                    	GetAllAlertTypes().then(function () {
                    		//GETALERT LIST
                    	    $scope.BodyAlert = buildBodyAlert($scope.AlertInfos, $scope.dvrInfo, $scope.takeAlert);
                    	    $scope.BodyAlertShow = angular.copy($scope.BodyAlert);
                    	});
                    	alertdate.AlertTypes = null;
                    	$scope.nameDVR = null;
                    	defer.resolve();

                    },
                    function (error) {
                    	var data = error;
                    	defer.reject();
                    }
                    );
				return defer.promise;
			}

			function buildBodyAlert(data, dvrInfors, take) {
				var dvrBody = Enumerable.From(data)
                    .GroupBy("$.Kdvr", null, function (key, g) {
                    	return { Kdvr: key, Alert: mergeHeader(g.ToArray(), key), Total: g.Sum("$.TotalAlert") }
                    })
                    .OrderByDescending(function (x) { return x.Total; })
                    .Take(take)
                    .ToArray();

				return mathSitesAndAlert(dvrInfors, dvrBody);
			}

			function mergeHeader(alerts, kdvr) {
				$scope.HeaderTitle.forEach(function (a) {
					var getAlert = Enumerable.From(alerts).Where(function (x) { return x.AlertTypeId === a.Title.Id; }).FirstOrDefault();
					if (!getAlert) {
						var alert = {
							AlertTypeId: a.Title.Id,
							Kalert: -1,
							Kdvr: kdvr,
							TimeZone: null,
							TotalAlert: 0
						}
						alerts.push(alert);
					}
				});
				return alerts;
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
				var defer = cmsBase.$q.defer();
				dataContext.sitealert.GetAllAlertTypes(
                    function (data) {
                    	$scope.AlertTypes = data;
                    	calcHeaderTitlte($scope.AlertTypes, $scope.AlertInfos);
                    	defer.resolve();
                    },
                    function (error) {
                    	var data = error;
                    	defer.reject();
                    }
                );
				return defer.promise;
			}

			function calcHeaderTitlte(data, alertsinfo) {
				$scope.Header = getAlertHeader(data, alertsinfo);
				var alertTypesLinQ = Enumerable.From(data);
				var headerLinQ = Enumerable.From($scope.Header);
				$scope.HeaderTitle = alertTypesLinQ.Join(headerLinQ, "$.Id", "$", function (g, c) { return { Title: g, Total: sumOfAlert(c, alertsinfo) } }).ToArray();
				
			    //$scope.SelectedAlert = angular.copy($scope.HeaderTitle);
				vm.SelectedAlert = [];
				$scope.HeaderTitle.forEach(function (x) {
				    x.Checked = true;
				    vm.SelectedAlert.push(x);
				});
				$scope.HeaderTitleShow = angular.copy($scope.HeaderTitle);
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

				getSiteAlertMonitor(alertreq).finally(function () {
					$scope.isBusy = false;
				});
			}

			$scope.$on("SaveShowHideAlert", function (e, arg) {
			    console.log(arg);
			    
			    console.log(vm.SelectedAlert);
			    if (!arg || arg.length === 0) {
			        cmsBase.cmsLog.warning('Please, select at least one alert');

			        return;
			    }
			    $scope.HeaderTitleShow = angular.copy(arg);
			    var AlertTypes = Enumerable.From(arg).Select(function (x) { return x.Title.Id }).ToArray();
			    $scope.BodyAlertShow = [];
			    $scope.BodyAlertShow = Enumerable.From($scope.BodyAlert)
                    .Where(function (x) {
                        return Enumerable.From(x.TypeAlerts.Alert)
                                        .Join(AlertTypes, "$.AlertTypeId", "$", function (c, g) { return c; }).ToArray().length > 0
                        && Enumerable.From(x.TypeAlerts.Alert)
                                            .Join(AlertTypes, "$.AlertTypeId", "$", function (c, g) { return c.TotalAlert; }).Sum() > 0;
                    }).Select(function (x) {
                        return {
                            Info: x.Info,
                            TypeAlerts:{
                                Alert: Enumerable.From(x.TypeAlerts.Alert)
                                    .Join(AlertTypes, "$.AlertTypeId", "$", function (c, g) { return c; }).ToArray(),
                                Kdvr: x.TypeAlerts.Kdvr,
                                Total: Enumerable.From(x.TypeAlerts.Alert)
                                            .Join(AlertTypes, "$.AlertTypeId", "$", function (c, g) { return c.TotalAlert; }).Sum()
                            }
                        }
                    }).ToArray();
			    $scope.$applyAsync();

			    if (angular.element('.select-alert-types').find('.btn-group').hasClass('open')) {
			        angular.element('.select-alert-types').find('.btn-group').removeClass('open');
			    }

			});

			$scope.clickOutside = function ($event, element) {

			    vm.SelectedAlert = [];
			    $scope.HeaderTitleShow.forEach(function (x) {
			        x.Checked = true;
			        vm.SelectedAlert.push(x);
			    });
			    $scope.$applyAsync();
			    //if (angular.element(element).find('.btn-group').hasClass('open')) {
			    //    angular.element(element).find('.btn-group').removeClass('open');
			    //    //console.log('have open');
			    //} else {
			    //    //console.log('dont have open');
			    //}
			};

		}
	});
})();
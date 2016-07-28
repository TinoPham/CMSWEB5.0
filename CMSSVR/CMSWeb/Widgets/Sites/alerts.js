(function () {

	define(['cms', 'Widgets/sites/alertsSlider'], alerts);
	function alerts(cms) {
		cms.register.controller('alertsCtrl', alertsCtrl);
		alertsCtrl.$inject = ['$scope', 'dataContext', 'cmsBase', 'AppDefine', '$timeout', '$rootScope', '$filter', '$window', '$modal'];
		function alertsCtrl($scope, dataContext, cmsBase, AppDefine, $timeout, $rootScope, $filter, $window, $modal) {
			var kdvr = [];
			var currentID = 0;
			var oldID = 0;
			$scope.maxdate = new Date();
            
			$scope.isVideoLoss = function (KAlertType) {
				return KAlertType == AppDefine.AlertTypes.VIDEO_LOSS;
			}

			$scope.dateOptions = {
				formatYear: 'yy',
				startingDay: 1,
				showWeeks: false
			};
			$scope.ismobileView = false;
			$scope.$watch(function () {
				return $window.innerWidth;
			}, function (value) {
				$scope.ismobileView = value <= 850;
				//return console.log(value);
			});
			$scope.dateAlert = {
				startDate: setTime(new Date(), 0, 0, 0, 0),
				endDate: setTime(new Date(), 23, 59, 59, 999)
			}
			$scope.datestatus = {};
			function setTime(date, hour, minute, second, milisecond) {

				if (hour >= 0) date.setHours(hour);
				if (minute >= 0) date.setMinutes(minute);
				if (second >= 0) date.setSeconds(second);
				if (milisecond >= 0) date.setMilliseconds(milisecond);

				return date;
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
			        var elemS = $('#startDateAL');
			        var elemE = $('#endDateAL');

			        if (parseInt(elemS.position().top) == parseInt(elemE.position().top)) {
			            var SW = parseInt(elemS.width(), 10) + parseInt(elemE.width(), 10);
			            if (SW < 470) {
			                $scope.datestatus[elementClose] = !$scope.datestatus[elementOpened];
			            } else {
			                if (parseInt(elemE.css('margin-left'), 10) != 20) {
			                    elemE.css('margin-left', '20px');
			                    $scope.datestatus[elementOpened] = false;
			                    $scope.datestatus[elementClose] = false;
			                    $timeout(function () {
			                        $scope.datestatus[elementOpened] = true;
			                        $scope.datestatus[elementClose] = true;
			                    }, 200);
			                }
			            }
			        } else {
			            $scope.datestatus[elementClose] = !$scope.datestatus[elementOpened];
			        }
			    }
			}

			function isSameDay(date1, date2) {
			    var comDate1 = $filter('date')(date1, AppDefine.SITE_TAPS.formatDateCompare);
			    var comDate2 = $filter('date')(date2, AppDefine.SITE_TAPS.formatDateCompare);
			    if (comDate1 <= comDate2) {
			        return true;
			    } else {
			        return false;
			    }
			   
			}

			$scope.dvrInfors = {};
			$scope.init = function (node, alertSelected) {
				$scope.Provider = node;
				kdvr = Enumerable.From(node.Sites).Select(function (value) { return value.ID }).ToArray();
				$scope.dvrInfors = Enumerable.From(node.Sites).Select(function (value) { return { ID: value.ID, Name: value.Name, SiteKey: node.ID, SiteName: node.Name } }).ToArray();
				if (alertSelected !== undefined) {
				    var format = AppDefine.SITE_TAPS.timezoneFormat;
				    var startDate = new Date(alertSelected.firstAlert.TimeZone);
				    var minDate = setTime(new Date(AppDefine.SITE_TAPS.mindate), 0, 0, 0, 0);
				    if (startDate <= $scope.dateAlert.endDate & !isSameDay(startDate, minDate)) {
				        $scope.dateAlert.startDate = startDate;
				    }
				    getAlertType();
				}

			}
			$scope.$on(AppDefine.Events.SITE_TAPS.CHANGE_NODE_TREE, function (e, node) {
				if (currentID == node.ID) return;
				currentID = node.ID;
				$scope.Provider = node;
				$scope.AlertProvider = null;
				$scope.init(node);
				if ($scope.isActive) {
					getAlertType();
					//getAlertType();
				}

			});
			$scope.isActive = false;
			$scope.searchFn = function () {
				$scope.isActive = true;
				if ($scope.AlertProvider == null || $scope.AlertProvider == undefined) {
					getAlertType();
					//$scope.searchClick();
				}

			}

			$scope.AlertProvider = null;
			$scope.ParseDatetoString = function (date) {
				return $filter('date')(date, AppDefine.SITE_TAPS.datedataformat);
			}
			$scope.searchClick = function () {
				var startdate = setTime($scope.dateAlert.startDate, 0, 0, 0, 0);
				var enddate = setTime($scope.dateAlert.endDate, 23, 59, 59, 999);
				if (startdate > enddate) {
				    cmsBase.cmsLog.warning(cmsBase.translateSvc.getTranslate(AppDefine.Resx.MSG_WARNING));
				}
				var aType = vm.SelectedAlertTypes;
				if (vm.SelectedAlertTypes.length == 0) {
					var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.SELECT_ALERT);
					cmsBase.cmsLog.warning(msg);
				}
				else {
					var alertreq = {
						Dvrs: encodeURIComponent(kdvr.join()),
						Begin: $scope.ParseDatetoString(new Date(startdate)),
						End: $scope.ParseDatetoString(new Date(enddate)),
						TypeIDs: vm.SelectedAlertTypes.length > 0 ? Enumerable.From(vm.SelectedAlertTypes).Select(function (alert) { return alert.Id }).ToArray().toString() : '0'
					};
					dataContext.sitealert.SiteAlertsSummary(alertreq, function (data) {
						$scope.isNoData = data.length == 0;
						$scope.AlertProvider = Enumerable.From($scope.dvrInfors)
								  .Select(function (value) {
								  	return {
								  		SiteInfo: value,
								  		AlertInfo: Enumerable.From(data).Where(function (al_date) { return al_date.KDVR == value.ID }).Select(function (al_obj) { return { TimeZone: al_obj.TimeZone, TotalAlert: al_obj.TotalAlert } }).ToArray(),
								  		TotalAlert: Enumerable.From(data).Where(function (al_date) { return al_date.KDVR == value.ID }).Select(function (al_obj) { return al_obj.TotalAlert }).Sum()
								  	}
								  }).ToArray();
						$scope.AlertProvider = Enumerable.From($scope.AlertProvider).Where(function (value) { return value.TotalAlert != undefined && value.TotalAlert != 0 && value.TotalAlert != null }).Select(function (selector) { return selector }).ToArray();
					}, function (error) {
						var _error = error;
					});
				}
				

			}
			$scope.$on(AppDefine.Events.SITE_TAPS.GET_ALERTS, $scope.searchFn);
			$scope.$on(AppDefine.Events.SITE_TAPS.DES_ALERTS, function () { $scope.isActive = false; });
			$scope.AlertDatails = [];
			$scope.selectAlertDate = selectAlertDate;
			$scope.AcknownlegdeAlertsID = [];


			function selectAlertDate(siteinfo, alertInfo, autoSelectedNode) {


				var startdate, enddate;
				if (autoSelectedNode) {
					var format = AppDefine.SITE_TAPS.timezoneFormat;
					startdate = $filter('date')(autoSelectedNode.TimeZone, format);
					enddate = setTime(new Date(), 23, 59, 59, 999);
				} else {
					if (alertInfo.AlertDetails != null && alertInfo.AlertDetails != undefined) { if (alertInfo.AlertDetails.length != 0) { return; } }
					startdate = $filter('date')(alertInfo.TimeZone, AppDefine.SITE_TAPS.datedataformat, 'UTC');
					enddate = $filter('date')(alertInfo.TimeZone, AppDefine.SITE_TAPS.datedataformat, 'UTC');
					enddate = enddate.substring(8, -1) + AppDefine.SITE_TAPS.endtimedateformat;
				}

				var alertreq = {
					Dvrs: encodeURIComponent([siteinfo.ID].join()),
					Begin: startdate,
					End: enddate,
					TypeIDs: vm.SelectedAlertTypes.length > 0 ? Enumerable.From(vm.SelectedAlertTypes).Select(function (alert) { return alert.Id }).ToArray().toString() : ""
				};
				dataContext.sitealert.SiteAlerts(alertreq, function (data) {
					if (data) {
						//angular.forEach(data, function (value, key) {
						//    var isContain = Enumerable.From($scope.AcknownlegdeAlertsID).Where(function (item) { return item == value.KAlertType }).ToArray();
						//    if (isContain != undefined && isContain != null)
						//    {
						//        if (isContain.length > 0) {

						//            data[key].showCheck = true;
						//        }

						//    }
						// })
						alertInfo.AlertDetails = data;
					}

				}, function (error) {
					var _error = error;
				});
			}
			var vm = this;
			vm.AlertType = [];
			//getAlertType();
			vm.SelectedAlertTypes = [];
			function getAlertType() {
				if (vm.AlertType.length > 0) {
					return
				} else {
					dataContext.sitealert.GetAllAlertTypes(getAlertSuccess, getAlertError);
				}
			}

			function getAlertSuccess(data) {
				var alertIds = [
					AppDefine.AlertTypes.DVR_is_off_line,
					AppDefine.AlertTypes.DVR_VA_detection,
					AppDefine.AlertTypes.DVR_Record_Less_Than,
					AppDefine.AlertTypes.CMSWEB_Conversion_rate_above_100,
					AppDefine.AlertTypes.CMSWEB_Door_count_0,
					AppDefine.AlertTypes.CMSWEB_POS_data_missing,
					AppDefine.AlertTypes.DVR_CPU_Temperature_High,
					AppDefine.AlertTypes.DVR_disconnect_from_CMS_server,
					AppDefine.AlertTypes.CMS_Registration_Expire_Soon,
					AppDefine.AlertTypes.CMS_Registration_Expired,
					AppDefine.AlertTypes.DVR_connected_CMS_Server,
					AppDefine.AlertTypes.CMS_HASP_Unplugged,
					AppDefine.AlertTypes.CMS_HASP_Found,
					AppDefine.AlertTypes.CMS_HASP_Removed,
					AppDefine.AlertTypes.CMS_HASP_Expired,
					AppDefine.AlertTypes.CMS_Server_HASP_Limit_Exceeded
				];

				vm.AlertType = $.grep(data, function (n, i) {
					return alertIds.indexOf(n.Id) == -1;                       
				});
				vm.SelectedAlertTypes = vm.AlertType;
				$scope.searchClick();
				// console.log(data);
				$scope.$evalAsync();

			}

			function getAlertError(error) {
				console.log(error);
			}

			vm.ChangeAlertTypes = function () {
               // console.log(vm.SelectedAlertTypes);
				return vm.SelectedAlertTypes;
			}

			$scope.IgnoreAlert = function (details, siteinfo) {

			    var model = {
					Sites: siteinfo.SiteKey,
					Kdvr: details.KDVR,
					KAlert: details.KAlertEvent,
					Kchannel: details.KChannel,
					Description: ''
				};

                showDialog(model,details);
			}
			var imgURL = AppDefine.Api.SiteAlerts + '/GetSensorSnapshot';
			$scope.GetSnapShot = function (imgName, kdvr) {
                var imgArr = imgName.split(',');
                return imgURL + '?filename=' + imgArr[0] + '&kdvr=' + kdvr;
			}
            function showDialog(model,details) {
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
							details.IsManual = true;
						} else {
							details.IsManual = false;
						}

					});
				} else {
					defer.resolve();
				}

				return defer.promise;
			}
			$scope.showSlide = function (selected, details) {

				if (selected === "" || selected === null || selected === undefined) {
					return;
				}

				if (!$scope.modalShown) {
					$scope.modalShown = true;
					var userDeleteInstance = $modal.open({
						templateUrl: 'Widgets/sites/alertsSlider.html',
						controller: 'alertsSliderCtrl',
						size: 'lg',
						backdrop: 'static',
						backdropClass: 'modal-backdrop',
						keyboard: false,
						resolve: {
							items: function () {
								return {
									model: details,
									file: selected
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
		}
	};
})();
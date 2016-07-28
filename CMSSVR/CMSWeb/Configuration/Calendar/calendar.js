(function () {
	'use strict';
	define(['cms',
		'DataServices/Configuration/calendar.service',
		'configuration/Calendar/edit'], function (cms) {
		cms.register.controller('cmsCalendarCtrl', cmsCalendarCtrl);
		cmsCalendarCtrl.$inject = ['$scope', '$modal', '$filter', 'dataContext', 'cmsBase', 'AppDefine'];
		function cmsCalendarCtrl($scope, $modal, $filter, dataContext, cmsBase, AppDefine) {
			var $state = cmsBase.$state;
			$scope.DataDay = null;
			$scope.DataWeek = null;
			$scope.DataAgenda = null;
			$scope.Resx = AppDefine.Resx;
			$scope.MinPerHour = 59;
			$scope.RowHeight = 37;

			var rowIndex = 0;
			$scope.selectedRows = [];

			var vm = this;
			vm.dateime = new Date();
			//vm.selected = {};

			$scope.selectDate = function () {
				var test = 1;
			}

			var curDate = new Date();
			var arrWeekDates = new Array();
			UpdateCurWeek(curDate);

			active();

			$scope.$watch('vm.dateime', function (newva, oldva) {
				if ($scope.DataAgenda != null && $scope.DataAgenda != undefined) {
					$scope.DataDay = GetDayData($scope.DataAgenda, newva);
					if (IsWeekChange(newva)) {
						UpdateCurWeek(newva);
						$scope.DataWeek = GetDWeekData($scope.DataAgenda);
					}
				}
			});

			function active() {
				dataContext.injectRepos(['configuration.calendar']).then(getData);
			}

			$scope.gridCalAgendas = {
				data: 'DataAgenda',
				multiSelect: false,
				selectedItems: $scope.selectedRows,
				rowTemplate: '<div ng-dblclick="editCalendar(selected)" ng-style="{ \'cursor\': row.cursor }" ng-repeat="col in renderedColumns" ng-class="col.colIndex()" class="ngCell {{col.cellClass}}"><div class="ngVerticalBar" ng-style="{height: rowHeight}" ng-class="{ ngVerticalBarVisible: !$last }">&nbsp;</div><div ng-cell></div></div>',
				columnDefs: [{ field: 'StartDate', displayName: $filter('translate')(AppDefine.Resx.CAL_STARTDATETIME), cellFilter: "date:'" + AppDefine.CalDateTimeFormat + "'" }
							, { field: 'EndDate', displayName: $filter('translate')(AppDefine.Resx.CAL_ENDDATETIME), cellFilter: "date:'" + AppDefine.CalDateTimeFormat + "'" }
							, { field: 'Name', displayName: $filter('translate')(AppDefine.Resx.CAL_NAME) }
				],
				afterSelectionChange: function (data) {
					$scope.selected = $scope.selectedRows[0];
					rowIndex = this.rowIndex;
				}
			};

			function getData() {
				dataContext.calendar.CalAgenda({}, function (data) {
					$scope.DataAgenda = data;

					var today = new Date();
					$scope.DataDay = GetDayData(data, today);
					$scope.DataWeek = GetDWeekData(data);
				}, function (err) {

				});
			}
			function IsWeekChange(newday) {
				if (arrWeekDates == null || arrWeekDates == undefined || arrWeekDates.length < 7)
					return true;
				var selDayCount = CalcTotalDays(newday);
				var bwCount = CalcTotalDays(arrWeekDates[0]);
				if (selDayCount < bwCount) {
					return true;
				}
				else {
					var ewCount = CalcTotalDays(arrWeekDates[6]);
					if (selDayCount > ewCount)
						return true;
				}
				return false;
			}
			function UpdateCurWeek(_curDay) {
				arrWeekDates = new Array();
				vm.SelectedDate = _curDay;
				vm.DateTitle = AppDefine.WeekDays[_curDay.getDay()] + ' ' + (_curDay.getMonth() + 1).toString() + '/' + _curDay.getDate().toString();
				vm.WeekTitle = new Array();
				var first = _curDay.getDate() - _curDay.getDay();
				//var last = first + 6;
				//var weekStartDate = new Date(_curDay.setDate(first));
				//var weekEndDate = new Date(_curDay.setDate(last));
				for (var w = 0; w < 7; w++) {
					var wday = new Date(_curDay.getFullYear(), _curDay.getMonth(), first + w);
					arrWeekDates.push(wday);
					var dtitle = AppDefine.WeekDays[wday.getDay()] + ' ' + (wday.getMonth() + 1).toString() + '/' + wday.getDate().toString();
					vm.WeekTitle.push(dtitle);
				}
			}
			function CalcTotalDays(dtime) {
				var calcData = null;
				if (typeof (dtime) == 'string') {
					calcData = new Date(dtime);
				}
				else {
					calcData = dtime;
				}
				var day = calcData.getDate();
				var mon = calcData.getMonth();
				var year = calcData.getFullYear();
				var totaldays = (year * 366) + (mon * 31) + day;
				return totaldays;
			}
			function ConvertToDate(dtime) {
				var retDate = null;
				if (typeof (dtime) == 'string') {
					retDate = new Date(dtime);
				}
				else {
					retDate = dtime;
				}
				return retDate;
			}
			function toColor(num) {
				num >>>= 0;
				var b = num & 0xFF,
					g = (num & 0xFF00) >>> 8,
					r = (num & 0xFF0000) >>> 16;
				//a = ( (num & 0xFF000000) >>> 24 ) / 255 ;
				return "rgb(" + [r, g, b].join(",") + ")";
			}
			function CreateColorArray(agendaData, today) {
				var ctotaldays = CalcTotalDays(today);
				var colorDatas = new Array();
				var callen = agendaData.length;
				for (var c = 0; c < callen; c++) {
					var calStartDate = ConvertToDate(agendaData[c].StartDate);
					var calEndDate = ConvertToDate(agendaData[c].EndDate);
					var colordata = {};
					var stotaldays = CalcTotalDays(calStartDate);
					if (stotaldays > ctotaldays)
						continue;
					var etotaldays = CalcTotalDays(calEndDate);
					if (etotaldays < ctotaldays)
						continue;

					if (stotaldays < ctotaldays) {
						colordata.StartHour = 0;
						colordata.StarMin = 0;
						colordata.Color = agendaData[c].Color;
					}
					else {
						colordata.StartHour = calStartDate.getUTCHours();
						colordata.StarMin = calStartDate.getUTCMinutes();
						colordata.Color = agendaData[c].Color;
					}

					if (etotaldays > ctotaldays) {
						colordata.EndHour = 23;
						colordata.EndMin = 59;
					}
					else {
						colordata.EndHour = calEndDate.getUTCHours();
						colordata.EndMin = calEndDate.getUTCMinutes();
					}
					colorDatas.push(colordata);
				} //for
				return colorDatas;
			}
			function CreateDayData(agendaData, colorDatas) {
				var retData = new Array();
				var hlen = AppDefine.DayHours.length;
				for (var i = 0; i < hlen; i++) {
					var obj = {};
					obj.Time = AppDefine.DayHours[i];
					obj.Days = new Array();

					var day = {};
					if (colorDatas.length == 0) {
						day.Color = '';
						day.Min = 0;
					}
					else {
						if (i >= colorDatas[0].StartHour && i <= colorDatas[0].EndHour) {
							day.Color = toColor(colorDatas[0].Color);//'#339933';//
							if (i == colorDatas[0].StartHour)
								day.Min = colorDatas[0].StarMin;
							else
								day.Min = 59;

							if (i == colorDatas[0].EndHour)
								day.Min = colorDatas[0].EndMin;
							else
								day.Min = 59;
						}
						else {
							day.Color = '';
							day.Min = 0;
						}
					}
					obj.Days.push(day);
					retData.push(obj);
				}
				return retData;
			}
			function CreateWeekData(agendaData) {
				var arrColors = new Array();
				for (var j = 0; j < 7; j++) {
					var color = CreateColorArray(agendaData, arrWeekDates[j]);
					arrColors.push(color);
				}

				var retData = new Array();
				var hlen = AppDefine.DayHours.length;
				for (var i = 0; i < hlen; i++) {
					var obj = {};
					obj.Time = AppDefine.DayHours[i];
					obj.Days = new Array();

					for (var j = 0; j < 7; j++) {
						var colorDatas = arrColors[j];

						var day = {};
						if (colorDatas.length == 0) {
							day.Color = '';
							day.Min = 0;
						}
						else {
							if (i >= colorDatas[0].StartHour && i <= colorDatas[0].EndHour) {
								day.Color = toColor(colorDatas[0].Color);//'#339933';//
								if (i == colorDatas[0].StartHour)
									day.Min = colorDatas[0].StarMin;
								else
									day.Min = 59;

								if (i == colorDatas[0].EndHour)
									day.Min = colorDatas[0].EndMin;
								else
									day.Min = 59;
							}
							else {
								day.Color = '';
								day.Min = 0;
							}
						}
						obj.Days.push(day);
					}

					retData.push(obj);
				}
				return retData;
			}
			function GetDayData(agendaData, today) {
				var colorDatas = CreateColorArray(agendaData, today);
				var retData = CreateDayData(agendaData, colorDatas);
				return retData;
			}
			function GetDWeekData(agendaData) {
				var retData = CreateWeekData(agendaData);
				return retData;
			}

			vm.mode = 1;
			vm.onMode = function (m) {
				vm.mode = m;
			};

			//vm.Create = function () {
			//	$state.go("configuration.calendar.add");
			//};

			$scope.modalShown = false;
			$scope.editCalendar = function (valueCal) {
				if ($.isEmptyObject(valueCal)) {
					ShowDialogConfirm();
				} else {
					ShowDialog(valueCal);
				}
			}
			vm.showDialog = function (cal) {
				ShowDialog(cal);
			}

			function ShowDialog(cal) {
				if (!$scope.modalShown) {
					$scope.modalShown = true;
					var calInstance = $modal.open({
						templateUrl: 'configuration/Calendar/edit.html',
						controller: 'CalendarEditAddCtrl as vm',
						size: 'lg',
						backdrop: 'static',
						backdropClass: 'modal-backdrop',
						keyboard: false,
						resolve: {
							items: function () {
								return cal;
							}
						}
					});

					calInstance.result.then(function (data) {
						cal = data;
						$scope.modalShown = false;
					});
				}
			}
			function ShowDialogConfirm() {
				var modalOptions = {
					headerText: AppDefine.Resx.HEADER_CONFIRM_DEFAULT,
					bodyText: AppDefine.Resx.CAL_CONFIRM_BODY
				};

				var modalDefaults = {
					backdrop: true,
					keyboard: true,
					modalFade: true,
					templateUrl: 'Widgets/ConfirmDialog.html'
				}

				dialogSvc.showModal(modalDefaults, modalOptions).then(function (result) {
				});
			}
		}
	});
})();
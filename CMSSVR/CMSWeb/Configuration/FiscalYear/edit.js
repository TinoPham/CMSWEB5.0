(function () {
	'use strict';

	define(['cms'], function (cms) {
		cms.register.controller('editFiscalCtrl', editFiscalCtrl);
		editFiscalCtrl.$inject = ['$scope', '$modalInstance', 'items', 'fiscalyearservice', 'AccountSvc', '$filter'];
		function editFiscalCtrl($scope, $modalInstance, items, fiscalyearservice, AccountSvc, $filter) {

			$scope.FYModel = angular.copy(items);
			var calendarType = {
				Normal: 1,
				FirstMonth: 2,
				Weeks_52: 3,
				Week_53: 4,
				Week_52_or_53: 5
			};
			var DayOfWeek = {
				SUNDAY: 0,
				MONDAY: 1,
				TUESDAY: 2,
				WEDNESDAY: 3,
				THURSDAY: 4,
				FRIDAY: 5,
				SATURDAY: 6
			};

			function toUTCDate(stringDate) {
				var temp = stringDate.split('/');
				var rs = new Date(temp[2], temp[0] - 1, temp[1], 0, 0, 0, 0);
				return rs;
			}
			$scope.DayOfWeek = DayOfWeek;
			$scope.test = false;
			$scope.FYModel.FYDateStart = toUTCDate($filter('date')(items.FYDateStart, 'MM/dd/yyyy', 'UTC'));
			$scope.FYModel.FYDateEnd = toUTCDate(($filter('date')(items.FYDateEnd, 'MM/dd/yyyy', 'UTC')));
            $scope.FYModel.FYDate =  toUTCDate($filter('date')(items.FYDate, 'MM/dd/yyyy', 'UTC'))

			change($scope.FYModel.FYTypesID);
			$scope.closePopup = function (data) {
				$modalInstance.close(data);
			};

			$scope.clear = function () {
				$scope.FYModel.FYDate = null;
			};

			// Disable weekend selection
			$scope.disabled = function (date, mode) {
				return (mode === 'day' && (date.getDay() === DayOfWeek.SUNDAY || date.getDay() === DayOfWeek.SATURDAY));
			};

			$scope.toggleMin = function () {
				$scope.minDate = $scope.minDate ? null : new Date(9999, 0, 1);
			};
			$scope.toggleMin();

			$scope.open = function ($event) {
				$event.preventDefault();
				$event.stopPropagation();

				$scope.opened = true;
			};

			$scope.dateOptions = {
				formatYear: 'yy',
				startingDay: 1,
				showWeeks: false
			};

			$scope.formats = ['dd-MMMM-yyyy', 'MM/dd/yyyy', 'dd.MM.yyyy', 'shortDate'];
			$scope.format = $scope.formats[1];
			$scope.isDisable = function (data) {
				return $scope.FYModel.FYTypesID != data;
			}
			$scope.isDiableStartDate = function () {
				if ($scope.FYModel.FYTypesID === calendarType.Normal || $scope.FYModel.FYTypesID === calendarType.Week_52_or_53) return false;
				return true;
			}
			$scope.changeOptions = change;
			$scope.formatDate = formatDate;

			function formatDate(date) {

				return ("0" + (date.getMonth() + 1).toString()).slice(-2) + "/" + ("0" + date.getDate().toString()).slice(-2) + "/" + (date.getFullYear().toString());
			}
			function change(data) {
				switch (parseInt(data)) {
					case 1:
						$scope.minDatestart = new Date(9999, 11, 31);
						$scope.minDateend = new Date(9999, 11, 31);
						$scope.FYModel.FYDateStart = new Date(new Date().getFullYear(), 0, 1);
						$scope.FYModel.FYDateEnd = new Date(new Date().getFullYear(), 11, 31);
						break;
					case 2:
						$scope.minDatestart = new Date(1970, 0, 1);
						$scope.minDateend = new Date(9999, 11, 31);
						$scope.FYModel.FYDateStart = new Date($scope.FYModel.FYDateStart.getFullYear(), $scope.FYModel.FYDateStart.getMonth(), 1);
						var o = CalulateFYCalendar(cloneDate($scope.FYModel.FYDateStart), $scope.FYModel.FYTypesID, null);
						$scope.FYModel.FYDateEnd = new Date(o.endDate);
						break;
					case 3:
					case 4:
						$scope.minDatestart = new Date(1970, 0, 1);
						$scope.minDateend = new Date(9999, 11, 31);
						var o = CalulateFYCalendar(cloneDate($scope.FYModel.FYDateStart), $scope.FYModel.FYTypesID, null);
						$scope.FYModel.FYDateEnd = new Date(o.endDate);
						$scope.FYModel.FYNoOfWeeks = (parseInt(data)) == 3 ? 52 : 53;
						break;
					case 5:
						$scope.minDatestart = new Date(9999, 11, 31);
						$scope.minDateend = new Date(9999, 11, 31);
						var day = parseInt($scope.FYModel.FYClosest);

						var temp1 = cloneDate(new Date($scope.FYModel.FYDate));
						var temp2 = new Date((new Date()).getFullYear(), temp1.getMonth(), temp1.getDate());

						while (temp2.getDay() != day) {
							temp2 = addDays(cloneDate(temp2), -1);
						}

						if (clearTime(temp2) < clearTime(new Date())) {
							$scope.FYModel.FYDateStart = addDays(clearTime(temp2), 1);
							var temp3 = addDays(clearTime(cloneDate(temp2)), 370);
							var temp4 = new Date(temp3.getFullYear(), temp1.getMonth(), temp1.getDate());
							if (temp3 > temp4) {
								temp3 = addDays(clearTime(cloneDate(temp2)), 363);
							}
							$scope.FYModel.FYDateEnd = cloneDate(temp3);
						}
						else {
							$scope.FYModel.FYDateEnd = cloneDate(temp2);
							var temp3 = addDays(clearTime(cloneDate(temp2)), -363);
							var temp4 = new Date(temp3.getFullYear(), temp1.getMonth(), temp1.getDate());
							if (temp3 > temp4) {
								temp3 = addDays(clearTime(cloneDate(temp2)), -370);
							}
							$scope.FYModel.FYDateStart = cloneDate(temp3);
						}
						break;
				}
			}


			$scope.disabledNonefirstday = function (date, mode) {

				return (mode === 'day' && (date.getDate() != 1) && $scope.FYModel.FYTypesID === calendarType.FirstMonth.toString());
			};




			function Calulate52or53Weeks(defaultday, defaultdate, endyear) {
				var end_Df_Date = new Date(endyear, defaultdate.getMonth(), defaultdate.getDate());
				var end_Df_day = end_Df_Date.getDay();



			}
			function setUTC(_date) {
				var date = cloneDate(_date);

				date.setUTCDate(cloneDate(_date).getDate());
				date.setUTCMonth(cloneDate(_date).getMonth());
				date.setUTCFullYear(cloneDate(_date).getFullYear());
				date.setUTCHours(0);
				date.setUTCMinutes(0);
				date.setUTCSeconds(0);
				date.setUTCMilliseconds(0);
				return date;
			}
			$scope.SaveFY = function () {

				var miniModel = angular.copy($scope.FYModel);
				miniModel.FYDateEnd = setUTC(new Date($scope.FYModel.FYDateEnd));
				miniModel.FYDateStart = setUTC(new Date($scope.FYModel.FYDateStart));
				miniModel.FYDate = setUTC(new Date($scope.FYModel.FYDate));
				miniModel.FYTypesID = parseInt($scope.FYModel.FYTypesID);
				fiscalyearservice.Update(miniModel).then(function (data) {
					$scope.closePopup(data);
				});
			}
			function leapYear(year) {
				return ((year % 4 == 0) && (year % 100 != 0)) || (year % 400 == 0);
			}


			function clearTime(d) {
				d.setHours(0);
				d.setMinutes(0);
				d.setSeconds(0);
				d.setMilliseconds(0);
				return d;
			}


			function cloneDate(d, dontKeepTime) {
				if (dontKeepTime) {
					return clearTime(new Date(+d));
				}
				return new Date(+d);
			}


			function addYears(d, n, keepTime) {
				d.setFullYear(d.getFullYear() + n);
				if (!keepTime) {
					clearTime(d);
				}
				return d;
			}


			function addMonths(d, n, keepTime) { // prevents day overflow/underflow
				if (+d) { // prevent infinite looping on invalid dates
					var m = d.getMonth() + n,
                        check = cloneDate(d);
					check.setDate(1);
					check.setMonth(m);
					d.setMonth(m);
					if (!keepTime) {
						clearTime(d);
					}
					while (d.getMonth() != check.getMonth()) {
						d.setDate(d.getDate() + (d < check ? 1 : -1));
					}
				}
				return d;
			}

			function addDays(d, n, keepTime) { // deals with daylight savings
				if (+d) {
					var dd = d.getDate() + n,
                        check = cloneDate(d);
					check.setHours(9); // set to middle of day
					check.setDate(dd);
					d.setDate(dd);
					if (!keepTime) {
						clearTime(d);
					}
					fixDate(d, check);
				}
				return d;
			}


			function fixDate(d, check) { // force d to be on check's YMD, for daylight savings purposes
				if (+d) { // prevent infinite looping on invalid dates
					while (d.getDate() != check.getDate()) {
						d.setTime(+d + (d < check ? 1 : -1) * HOUR_MS);
					}
				}
			}


			function CalulateFYCalendar(date, type, day) {
				var fyobject = {
					startDate: new Date(),
					endDate: new Date()
				};
				switch (parseInt(type)) {
					case calendarType.Week_52_or_53:
					case calendarType.Normal:
						fyobject.startDate = new Date(date.getFullYear(), 0, 1);
						fyobject.endDate = new Date(date.getFullYear(), 11, 31);
						break;
					case calendarType.FirstMonth:
						fyobject.startDate = new Date(date.getFullYear(), date.getMonth(), 1);
						fyobject.endDate = addYears(fyobject.startDate, 1);
						fyobject.endDate = addDays(fyobject.endDate, -1);
						break;
					case calendarType.Weeks_52:
						fyobject.startDate = date;
						fyobject.endDate = addDays(date, 363);
						break;
					case calendarType.Week_53:
						fyobject.startDate = date;
						fyobject.endDate = addDays(date, 370);
						break;
				}
				return fyobject;

			}


		}
	});
})();
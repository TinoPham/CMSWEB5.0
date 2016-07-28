(function () {
	'use strict';
	define(['cms'
	],
        function (cms) {

        	cms.register.controller('eventactivitiesCtrl', eventactivitiesCtrl);

        	eventactivitiesCtrl.$inject = ['$scope', 'cmsBase', 'dataContext', '$q', '$timeout'];

        	function eventactivitiesCtrl($scope, cmsBase, dataContext, $q, $timeout) {
        		$scope.DataAgenda = [];
        		$scope.today = function () {
        			$scope.dt = new Date();
        		};
        		$scope.today();

        		$scope.clear = function () {
        			$scope.dt = null;
        		};

        		$scope.DataLoaded = false;

        		displayEventCalendar();

        		$scope.datanfo = function (date) {
        			var datainfo = {
        				mode: 0,
        				event: false,
        				normalize: true,
        				keyName: '',
        				note: ''
        			};

        			if (!$scope.DataAgenda) {
        				return;
        			}

        			if (Object.keys($scope.DataAgenda).length != 0) {
        				for (var i = 0; i < $scope.DataAgenda.length; i++) {
        					var eventCal = $scope.DataAgenda[i];
        					var timeNow = date.setHours(0, 0, 0, 0);
        					var startDate = new Date(eventCal.StartDate).setHours(0, 0, 0, 0);
        					var endDate = new Date(eventCal.EndDate).setHours(0, 0, 0, 0);
        					if (startDate <= timeNow && timeNow <= endDate) {
        						if (startDate == timeNow && (eventCal.NormalizeAllSite === true || eventCal.RelatedFunction === 1)) {
        							//disable all normalize
        							datainfo.normalize = false;
        							datainfo.note = 'Disable Normalize';
        						}
        						if (eventCal.ID == 84)
        							var a = 1;
        						if (checkSchedule(eventCal, timeNow)) {
        							datainfo.event = true;
        							datainfo.normalize = false;
        							datainfo.note = 'Disable Normalize';
        						}
        						if (eventCal.ScheduleType == 0 && startDate == timeNow) {
        							datainfo.event = true;
        						}
        					}
        				}
        			}

        			return datainfo; //(mode === 'day' && (date.getDay() === 0 || date.getDay() === 6));
        		}

        		function checkSchedule(eventCal, timeNow) {
        			if (eventCal.ScheduleType != 0) {
        				var beginDate = new Date(eventCal.StartDate);
        				var endDate = new Date(eventCal.EndDate);
        				if (eventCal.ScheduleType == 1) {
        					if (beginDate == timeNow) {
        						return true;
        					}

        				} else if (eventCal.ScheduleType == 2) {
        					while (beginDate <= endDate) {
        						if (timeNow === beginDate.setHours(0, 0, 0, 0)) {
        							return true;
        						}
        						beginDate = new Date(beginDate.setDate(beginDate.getDate() + 7));
        					}

        				} else if (eventCal.ScheduleType == 3) {
        					while (beginDate <= endDate) {
        						if (timeNow === beginDate.setHours(0, 0, 0, 0)) {
        							return true;
        						}
        						beginDate = new Date(beginDate.getFullYear() + 1, beginDate.getMonth(), 0).getDate();
        					}

        				} else if (eventCal.ScheduleType == 4 && timeNow == beginDate.setHours(0, 0, 0, 0)) {
        					return true;
        				}
        			}
        			return false;
        		}

        		// Disable weekend selection
        		$scope.disabled = function (date, mode) {
        			return (mode === 'day' && (date.getDay() === 0 || date.getDay() === 6));
        		};

        		$scope.toggleMin = function () {
        			$scope.minDate = $scope.minDate ? null : new Date(2014, 1, 1);
        		};
        		$scope.toggleMin();

        		$scope.open = function ($event) {
        			$event.preventDefault();
        			$event.stopPropagation();

        			$scope.opened = true;
        		};

        		$scope.dateOptions = {
        			formatYear: 'yy',
        			startingDay: 1
        		};

        		$scope.formats = ['dd-MMMM-yyyy', 'yyyy/MM/dd', 'dd.MM.yyyy', 'shortDate'];
        		$scope.format = $scope.formats[0];

        		function displayEventCalendar() {
        			var def = $q.defer();

        			dataContext.injectRepos(['configuration.calendar']).then(function () {
        				getCalendarEvent().then(function () {
        					def.resolve();
        				});
        			});

        			return def.promise;
        		}

        		function getCalendarEvent() {
        			var def = $q.defer();
        			dataContext.calendar.CalAgenda({}, function (data) {
        				$scope.DataAgenda = data;
        				$scope.DataLoaded = true;
        			}, function (err) {

        			});
        			return def.promise;
        		}
        	}
        });
})();
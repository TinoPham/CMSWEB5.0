(function () {

    'use strict';
    define(['cms', 'DataServices/Configuration/fiscalyearservice','configuration/fiscalyear/edit'], function (cms) {
        cms.register.controller('fiscalyearCtrl', ['$scope', 'router', '$state', '$modal', 'dialogSvc', '$filter', 'translateSvc', 'fiscalyearservice', 'AccountSvc', '$window', function ($scope, router, $state, $modal, dialogSvc, $filter, translateSvc, fiscalyearservice, AccountSvc, $window) {
            var fyStyle = '454';
            var fyDate = new Date();
            var fyType = '1';//1 mean calendar year
            $scope.eventSources = null;
            var startFyCalendar = new Date();
            var endFyCalendar = new Date();
            var calendarType = {
                Normal: 1,
                FirstMonth: 2,
                Weeks_52: 3,
                Week_53: 4,
                Week_52_or_53: 5
            };
            var fymodel;
            var test = calendarType.Normal;
            function formatDate(date){

                return ("0" + (date.getMonth()+1).toString()).slice(-2) + "/" + ("0" + date.getDate().toString()).slice(-2)+"/"+ (date.getFullYear().toString());
            }
            /* this function is used for locally  do not use for others module*/
            function toUTCDate(stringDate){
                var temp = stringDate.split('/');
                var rs =  new Date(temp[2], temp[0]-1, temp[1],0,0,0,0);
                return rs ;
            }
            $scope.ismobileView = false;

            $scope.$watch(function () {
                return $window.innerWidth;
            }, function (value) {
                $scope.ismobileView = value <= 800;
                //return console.log(value);
            }); /* END :: Watch document with*/

            var func = (function () {
                var _date = formatDate(new Date());
                fiscalyearservice.GetCustomFiscalYear(_date).then(function (data) {

                    if (data) {
                        fymodel = data;//angular.copy(data.data);
                        startFyCalendar = toUTCDate($filter('date')(fymodel.FYDateStart, 'MM/dd/yyyy', 'UTC'));
                        endFyCalendar = toUTCDate(($filter('date')(fymodel.FYDateEnd, 'MM/dd/yyyy', 'UTC')));
                        fyStyle = fymodel.CalendarStyle;
                        fyType = fymodel.FYTypesID;
                        fyDate = toUTCDate($filter('date')(fymodel.FYDate, 'MM/dd/yyyy', 'UTC'))
                        
                    }
                    var calendarObject = (function(){ return {
                            editable: false,
                            selectable: false,
                            header: {
                            left: 'edit',
                            center: '',
                            right: 'prev,title,next'
                            },
                        defaultView: 'year',
                        startDate: startFyCalendar,
                        endDate: endFyCalendar,
                        fyStyle:fyStyle,
                        fyType: fyType,
                        fyDate: fyDate,
                        weekNumbers: true,
                        weekNumberTitle: $scope.ismobileView ? $filter('translate')('WEEK_SHORT_LABEL') : $filter('translate')('WEEK_LONG_LABEL'),
                        periodTitle: $filter('translate')('PERIOD_LABEL'),
                        monthNames: [$filter('translate')('MONTH_JAN'),
                                    $filter('translate')('MONTH_FEB'),
                                    $filter('translate')('MONTH_MAR'),
                                    $filter('translate')('MONTH_APR'),
                                    $filter('translate')('MONTH_MAY'),
                                    $filter('translate')('MONTH_JUN'),
                                    $filter('translate')('MONTH_JUL'),
                                    $filter('translate')('MONTH_AUG'),
                                    $filter('translate')('MONTH_SEP'),
                                    $filter('translate')('MONTH_OCT'),
                                    $filter('translate')('MONTH_NOV'),
                                    $filter('translate')('MONTH_DEC')],

                        dayNames: [$filter('translate')('SUN_DAY'),
                                    $filter('translate')('MON_DAY'),
                                    $filter('translate')('TUE_DAY'),
                                    $filter('translate')('WED_DAY'),
                                    $filter('translate')('THU_DAY'),
                                    $filter('translate')('FRI_DAY'),
                                    $filter('translate')('SAT_DAY')],

                        dayNamesShort: [$filter('translate')('SUN_DAY_SHORT'),
                                    $filter('translate')('MON_DAY_SHORT'),
                                    $filter('translate')('TUE_DAY_SHORT'),
                                    $filter('translate')('WED_DAY_SHORT'),
                                    $filter('translate')('THU_DAY_SHORT'),
                                    $filter('translate')('FRI_DAY_SHORT'),
                                    $filter('translate')('SAT_DAY_SHORT')],
                        firstDay: startFyCalendar.getDay(),
                        editClick: (function () {
                            var instance = $modal.open({
                                templateUrl: 'configuration/fiscalyear/edit.html',
                                controller: 'editFiscalCtrl',
                                size: 'lg',
                                backdrop: 'static',
                                backdropClass: 'modal-backdrop',
                                keyboard: false,
                                resolve: {
                                    items: function () {
                                        return fymodel;
                                    }
                                }
                            });
                            instance.result.then(function (result) {
                                if (result !== undefined){
                                func();
                                }
                            });

                        }),
                        buttonText: {
                            prev: "<span class='fc-text-arrow'><i class=' icon-left-1'></i></span>",
                            next: "<span class='fc-text-arrow'><i class='icon-right-1'></span>",
                            prevYear: "<span class='fc-text-arrow'>&laquo;</span>",
                            nextYear: "<span class='fc-text-arrow'>&raquo;</span>",
                            today: 'today',
                            year: 'year',
                            month: 'month',
                            week: 'week',
                            day: 'day',
                            edit: "<i class='icon-edit'>&nbsp;</i>" + $filter('translate')('BTN_FISCAL_YEAR_CHANGE')
                        },
                        events: []
                    }
                    });
                    
                    $scope.fyObject = calendarObject();
                                       
       
                });
            });

            func();

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
                var fyobject;
                switch (type) {
                    case calendarType.Week_52_or_53:
                    case calendarType.Normal:
                        fyobject.startDate = new Date(date.getFullYear(), 0, 1);
                        fyobject.endDate = new Date(date.getFullYear(), 11, 31);
                        break;
                    case calendarType.FirstMonth:
                        fyobject.startDate = new Date(date.getFullYear(), date.month, 1);
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
        }]);
        //fiscalyear.$inject = ['$scope', 'router', '$state', '$modal', 'dialogSvc', '$filter', 'translateSvc', 'fiscalyearservice', 'AccountSvc'];

    });


})();
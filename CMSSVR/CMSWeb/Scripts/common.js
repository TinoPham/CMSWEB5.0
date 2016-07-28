(function() {
    'use strict';

    var cmsCommon = angular.module('cms.common', ['ui.router', 'LocalStorageModule', 'cms.exception', 'cms.auth', 'cms.language', 'cms.log', 'cms.router']);

    cmsCommon.config(['localStorageServiceProvider',function (localStorageServiceProvider) {localStorageServiceProvider.setPrefix('cmsWeb').setStorageType('sessionStorage').setNotify(true, true);
    }] ).service( 'sharingData', sharing ).factory( 'cmsBase', cmsBase );

    cmsBase.$inject = ['$q', '$state', '$stateParams', '$urlRouter', '$location', '$translate', 'router', 'cmsLog', 'translateSvc', 'localStorageService', 'sharingData', '$filter', 'AppDefine'];
    function cmsBase($q, $state, $stateParams, $urlRouter, $location, $translate, router, cmsLog, translateSvc, localStorageService, sharingData, $filter, AppDefine) {
        
        var mobile = /android|webos|iphone|ipad|ipod|blackberry/i.test(navigator.userAgent);
        var SMALL_PATTERN = 768;


        var commonService = {
            $q: $q,
            $state: $state,
            $stateParams: $stateParams,
            $urlRouter: $urlRouter,
            $location: $location,
            $translate: $translate,
            router: router,
            cmsLog: cmsLog,
            translateSvc: translateSvc,
            localStorageService: localStorageService,
            sharingData: sharingData,
            modalfullscreen: modalfullscreen,
            isMobile: mobile,
            PreventInputKeyPress: PreventInputKeyPress,
            PreventPaste: PreventPaste,
            RebarModule:
            {
                CheckCurrentPage: CheckCurrentPage,
                Next: Next,
                Goto: Goto,
                Prev:Prev
            },
            DateUtils:
            {
                dateAdd: dateAdd,
                endOfDate: endOfDate,
                startOfDate: startOfDate,
                DateCompare: DateCompare,
                getUTCDate: getUTCDate
            },
            GetDVRInfoSuccess: GetDVRInfoSuccess,
        }

        function getUTCDate(date) {
            if (date == null) return new Date();
            if (date == undefined) return new Date();
            var dd = $filter('date')(date, 'dd', 'UTC');
            var MM = $filter('date')(date, 'MM', 'UTC');
            var yy = $filter('date')(date, 'yyyy', 'UTC');
            var HH = $filter('date')(date, 'HH', 'UTC');
            var mm = $filter('date')(date, 'mm', 'UTC');
            var ss = $filter('date')(date, 'ss', 'UTC');
            return new Date(yy, parseInt(MM) - 1, dd, HH, mm, ss, 0);
        }

        function GetDVRInfoSuccess(data, DVRInfo, typeTimeline) {
            if (data.KDVR == undefined) {
                var msg = commonService.translateSvc.getTranslate('NO_VIDEO_CAN_VIEW');
                commonService.cmsLog.warning(msg);
                return;
            }
            var protocol = window.location.protocol;
            var xhttp = new XMLHttpRequest();
            var ChannelID = DVRInfo.CamName;

            var DVRDate = $filter('date')(DVRInfo.DvrDate, 'MM/dd/yyyy HH:mm:ss', 'UTC');

            var d = getUTCDate(DVRInfo.DvrDate);
            var _d = new Date(d.getTime());
            d.setSeconds(d.getSeconds() - 5);

            while (d.getDate() != _d.getDate()) {
                d.setSeconds(d.getSeconds() + 1);
            }

            var e = getUTCDate(DVRInfo.DvrDate);
            var _e = new Date(e.getTime());
            switch (typeTimeline) {
                case AppDefine.TypeTimeLines.Ten_minute:
            e.setMinutes(e.getMinutes() + 10);

            while (e.getDate() != _e.getDate()) {

                e.setMinutes(e.getMinutes() - 1);
            }
                    break;
                case AppDefine.TypeTimeLines.One_Hour:
                    if (e.getHours() >= 23) {
                        e.setMinutes(59);
                        e.setSeconds(59);
                    } else {
                        e.setMinutes(e.getMinutes() + 60);
                        while (e.getDate() != _e.getDate()) {
                            e.setMinutes(e.getMinutes() - 1);
                        }
                    }
                    
                    break;
                case AppDefine.TypeTimeLines.One_Day:
                    e.setHours(23);
                    e.setMinutes(59);
                    e.setSeconds(59);
                    break;
                default:
                    break;
            }
            

            var beginTimeParam = "&b=" + $filter('date')(d, 'HH:mm:ss');
            var endTimeParam = "&e=" + $filter('date')(e, 'HH:mm:ss');
            var DateParam = "&d=" + $filter('date')(d, 'MMddyyyy');


            xhttp.onreadystatechange = function () {
                if (xhttp.readyState == 4 && xhttp.status != 202) {
                    commonService.cmsLog.warning(commonService.translateSvc.getTranslate('CANNOT_RUNNING_VPC'));
            }
            };
            // June 30, 2016 Tri change connect with url http
            //var url = (protocol == AppDefine.HTTP_PROTOCOL ? AppDefine.HTTP_LOCALHOST : AppDefine.HTTPS_LOCALHOST) + data.ServerID + "&c=" + ChannelID + DateParam + beginTimeParam + endTimeParam;
            var url = AppDefine.HTTP_LOCALHOST + data.ServerID + "&c=" + ChannelID + DateParam + beginTimeParam + endTimeParam;

            url = url + "&q=0";
            if (protocol == AppDefine.HTTP_PROTOCOL) {
                xhttp.open("GET", url, true);
                xhttp.send();
            } else {
                window.open(url, '_blank');
        }

        }

        function DateCompare(date_1, date_2)
        {
            return date_1.getTime() - date_2.getTime(); 
            }
           
        function startOfDate(date)
        {
            date.setHours(0);
            date.setMinutes(0);
            date.setSeconds(0);
            date.setMilliseconds(0);
            return date;
    }

        function endOfDate(date)
        {
            date.setHours(23);
            date.setMinutes(59);
            date.setSeconds(59)
            date.setMilliseconds(995);
            return date;

        }

        function dateAdd(days,date) {
            date.setTime(date.getTime() + days * 86400000);
            return date;
        }

        function getDocHeight() {
            var D = document;
            return Math.max(
                D.body.scrollHeight, D.documentElement.scrollHeight,
                D.body.offsetHeight, D.documentElement.offsetHeight,
                D.body.clientHeight, D.documentElement.clientHeight
            );
        }


        function Next(data, currentPage, totalPage, func) {
            if (data) {
                currentPage = data;
            }
            else {
              
                currentPage = parseInt(currentPage) + 1;
                if (!CheckCurrentPage(currentPage, totalPage)) return currentPage-1;
            }
            func(currentPage);
            return currentPage;
        }

        function Prev(data, currentPage, totalPage, func) {

            if (currentPage <1) {
                cmsLog.warning(translateSvc.getTranslate('CANNOT_FIND_MATCHED_PAGE'));
                return 1;
            }

            if (currentPage == 1)
            {
                return 1;
            }

            if (data) {
                currentPage = 1;
            }
            else {
                currentPage = parseInt(currentPage) - 1;
                if (!CheckCurrentPage(currentPage, totalPage)) return currentPage;
              
            }
            func(currentPage);
            return currentPage;
        }

        function Goto(currentPage, totalPage, func) {
            if (!CheckCurrentPage(currentPage, totalPage)) return currentPage;
            if (currentPage > 0 && currentPage <= totalPage) {
                func(currentPage);
            }
            return currentPage;
        }

        function CheckCurrentPage(currentPage, totalPage) {
            if (currentPage > totalPage || currentPage == null || currentPage == "" || currentPage == undefined || currentPage == 0) {
                cmsLog.warning(translateSvc.getTranslate('CANNOT_FIND_MATCHED_PAGE'));
                return false;
            }
            return true;
    }

        return commonService;

    }

    function PreventInputKeyPress(e,regx)
    {
        var event = e.originalEvent;
        if (event.code === "Delete" || event.code === "Backspace" || event.code === "ArrowRight" || event.code === "ArrowLeft") return;
        var charCode = String.fromCharCode(event.charCode);
        var patt = new RegExp(regx);
        if(!patt.test(charCode))
            event.preventDefault();
    }
    function PreventPaste(event,regx) {
        var text = event.originalEvent.clipboardData.getData('text/plain');
        var patt = new RegExp(regx);
        if (!patt.test(text))
            event.preventDefault();
    }
    function modalfullscreen(enable, name) {
        var setfull = angular.element('.' + name + ' .modal-dialog');
        var isfullscreen = angular.element('.' + name + ' .modal-dialog.full-screen');
        if (enable) {
            if (isfullscreen && isfullscreen.length > 0) {
                
            } else {
                if (setfull && setfull.length > 0) {
                    setfull.addClass('full-screen');
                } else {
                    setfull.addClass(name);
                }
            }
        } else {
            if (isfullscreen && isfullscreen.length > 0) {
                    isfullscreen.removeClass('full-screen');
                    isfullscreen.removeClass(name);
            } 
        }
    }

    function sharing() {
        var $variables = {};
        var $obj = null;
        return {
            setObject: function(obj) {
                $obj = obj;
            },
            getObject: function() {
                return $obj;
            },
            get: function (keyname,isKeep) {
                var result = (typeof $variables[keyname] !== 'undefined') ? $variables[keyname] : undefined;
                if (isKeep) {
                    return result;
                }
                this.remove(keyname);
                return result;
            },
            set: function (keyname, value) {
                $variables[keyname] = value;
            },
            remove: function (keyname) {
                if ($variables[keyname]) {
                    delete $variables[keyname];
                }
            }
        }
    }

    cmsCommon.service('cmscompile', ['$q', '$http', '$templateCache', function ($q, $http, $templateCache) {

        var service = {
            requireLoad: requireLoad,
            getTemplate: getTemplate
        };

        function requireLoad(file) {
            var defer = $q.defer();
            require(file, function () {
                defer.resolve();
            }, function () {
                defer.reject();
            });

            return defer.promise;
        }

        function getTemplate(url) {
            var deferred = $q.defer();

            if (url) {
                var tpl = $templateCache.get(url);
                if (tpl) {
                    deferred.resolve(tpl);
                } else {
                    $http.get(url)
                        .success(function (response) {
                            $templateCache.put(url, response);
                            deferred.resolve(response);
                        })
                        .error(function () {
                            deferred.reject();
                        });
                }
            }

            return deferred.promise;
        }

        return service;
    }]);

})();
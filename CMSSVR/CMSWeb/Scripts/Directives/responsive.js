(function() {
    'use strict';

    angular.module('cms.directives')
    .provider('responsiveHelper', responsiveHelperFn)
    .directive('cmsXs', cmsXs)
    .directive('cmsSm', cmsSm)
    .directive('cmsMd', cmsMd)
    .directive('cmsLg', cmsLg)
    .directive('cmsResponsive', cmsResponsive);


    cmsXs.$inject = ['responsiveHelper'];
    function cmsXs(responsiveHelper) {
        return {
            restrict: "EAC",
            transclude: 'element',
            template: '<div></div>',
            compile: buildCompileFn('cmsXs', responsiveHelper.isXs)
        };
    }

    cmsSm.$inject = ['responsiveHelper'];
    function cmsSm(responsiveHelper) {
        return {
            restrict: "EAC",
            transclude: 'element',
            template: '<div></div>',
            compile: buildCompileFn('cmsSm', responsiveHelper.isSm)
        };
    }

    cmsMd.$inject = ['responsiveHelper'];
    function cmsMd(responsiveHelper) {
        return {
            restrict: "EAC",
            transclude: 'element',
            template: '<div></div>',
            compile: buildCompileFn('cmsMd', responsiveHelper.isMd)
        };
    }

    cmsLg.$inject = ['responsiveHelper'];
    function cmsLg(responsiveHelper) {
        return {
            restrict: "EAC",
            transclude: 'element',
            template: '<div></div>',
            compile: buildCompileFn('cmsLg', responsiveHelper.isLg)
        };
    }

    cmsResponsive.$inject = ['responsiveHelper'];
    function cmsResponsive(responsiveHelper) {
        return {
            restrict: "EAC",
            transclude: 'element',
            template: '<div></div>',
            compile: buildCompileFn('cmsResponsive', checkTypes(responsiveHelper))
        };
    }

    function checkTypes(resHelper) {
        return function(devideTypes) {
            return  (devideTypes['cmsXs'] && resHelper.isXs()) ||
                    (devideTypes['cmsSm'] && resHelper.isSm()) ||
                    (devideTypes['cmsMd'] && resHelper.isMd()) ||
                    (devideTypes['cmsLg'] && resHelper.isLg()) || false;
        }
    }

    responsiveHelperFn.$inject = ['$windowProvider'];
    function responsiveHelperFn($windowProvider) {

        var $window = $windowProvider.$get();

        var winWidth = $window.innerWidth || $window.outerWidth;

        var helper =  {
                isXs: function () { return winWidth < 768; },
                isSm: function () { return winWidth >= 768 && winWidth < 992; },
                isMd: function () { return winWidth >= 992 && winWidth < 1200; },
                isLg: function () { return winWidth >= 1200; }
        }

        this.$get = function() {
            return helper;
        }
    }

    function buildCompileFn(responsiveType, verifyFn) {
        return function(element, attrs, transclude) {
            return function postLink(scope, element, attrs) {
                var childElement, childScope;

                var config = scope.$eval(attrs[responsiveType]);
                var unwatch = scope.$watch(config, function() {
                    if (childElement) {
                        childElement.remove();
                        childElement.$destroy();
                        childElement = undefined;
                        childScope = undefined;
                    }

                    if (verifyFn(config)) {
                        childScope = scope.$new();

                        childElement = transclude(childScope, function(clone) {
                            element.after(clone);
                        });
                    }
                });

                scope.$on('$destroy', unwatch);
            }
        }
    }

})();
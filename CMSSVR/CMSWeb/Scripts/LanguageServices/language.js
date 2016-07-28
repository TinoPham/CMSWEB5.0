(function() {
    'use strict';
    angular.module('cms.language', ['ngCookies', 'pascalprecht.translate']);


    angular.module('cms.language').config([
        '$translateProvider', function ($translateProvider) {
            $translateProvider.useLoader('$translatePartialLoader', {
                urlTemplate: '{part}/Resources/{lang}.json'
            });

            $translateProvider.preferredLanguage('en');

            ////$translateProvider.useSanitizeValueStrategy(null);
            $translateProvider.useLocalStorage();
        }
    ]).factory('translateSvc', translateSvc);

    translateSvc.$inject = ['$translate', '$translatePartialLoader', '$rootScope', '$state', '$STORAGE_KEY', '$timeout'];
    function translateSvc($translate, $translatePartialLoader, $rootScope, $state, $STORAGE_KEY, $timeout) {

        //June 30, 2016 Tri add
        //$timeout(function () {
        //    var langlocal = localStorage.getItem($STORAGE_KEY);
        //    if (langlocal) {
        //        $translate.use(langlocal);
        //    } else {
        //        $translate.use('en');
        //    }
        //}, 2000)
        

        var service= {
            partLoad: partLoad,
            translate: translate,
            getCurrentLanguage: getCurrentLanguage,
            getTranslate: getTranslate
        }
        return service;

        function  partLoad(path) {
            $translatePartialLoader.addPart(path);
            $translate.refresh();
        }

        function getCurrentLanguage() {
            try {
                var currentLang = $translate.proposedLanguage() || $translate.use();
                return currentLang;
            } catch (error) {
                return 'en';
            }

        }

        function translate() {
            var currentSate = $state.current;
            //$rootScope.title = $translate.instant(currentSate.translate);
            $rootScope.title = currentSate.translate;
        }

        function getTranslate(message) {
        	return $translate.instant(message);
        }
    }

})();
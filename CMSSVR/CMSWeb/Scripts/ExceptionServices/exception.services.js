(function() {
    'use strict';
    angular.module('cms.exception',[]).provider('exception', exceptionProvider).config(exceptionconfig).factory('CmsException', function CmsException(cmsLog) {
        var services = {
            catcher: catcher
        }
        return services;

        function catcher(message) {
            return function (reason) {
                cmsLog.error(message, reason);
            }
        }
    });

    exceptionconfig.$inject = ['$provide'];

    function exceptionconfig($provide) {
        $provide.decorator('$exceptionHandler', cmsExceptionHandler);
    }

    cmsExceptionHandler.$inject = ['$delegate', 'exception', 'cmsLog'];

    function cmsExceptionHandler($delegate, exception, cmsLog) {
        return function(exceptiondata, cause) {
            var appErrorPrefix = exception.config.appErrorPrefix;
            var errorData = {
                exception: exceptiondata,
                cause: cause
            }

            exceptiondata.message = appErrorPrefix + exceptiondata.message;
            $delegate(exceptiondata, cause);
            //cmsLog.error(exceptiondata.message, errorData);
        };
    }

    function exceptionProvider() {
        this.config = { appErrorPrefix: 'CMS Web: ' };

        this.configure = function(appErrorPrefix) {
            this.config.appErrorPrefix = appErrorPrefix;
        }
        this.$get = function() {
            return { config: this.config };
        }
    }

})();
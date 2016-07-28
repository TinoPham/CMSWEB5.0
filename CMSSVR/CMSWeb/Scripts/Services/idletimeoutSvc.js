(function() {
    'use strict';
    define(['cms'], function(cms) {
        cms.service('idletimeoutSvc', idletimeoutSvc);
        idletimeoutSvc.$inject = ['$timeout', '$document', '$rootScope', 'AccountSvc', 'AppDefine', '$injector'];

        function idletimeoutSvc($timeout, $document, $rootScope, AccountSvc, AppDefine, $injector) {

            var settings = {
                isRegister: false,
                elemAttach: 'html',
                idleTime: 60 * 1000,
                timeout: null,
                isAuthenticate: false,
                eventKeepAlive: 'keydown DOMMouseScroll mousedown touchstart touchmove scroll',
                handleTimeout: true
            };

            var service = {
                registerIdleTimeout: registerIdleTimeout,
                cancelIdleTimeout: cancelIdleTimeout,
                pauseIdleTimeout: pauseIdleTimeout,
                runIdleTimeout: runIdleTimeout,
                isRegister: settings.isRegister
            };

            function handleTimeoutFn(event) {
                settings.isAuthenticate = AccountSvc.isAuthenticated();
                if (settings.isAuthenticate === true) {
                    var user = AccountSvc.UserModel();
                    settings.idleTime = user.IdleTimeout ? user.IdleTimeout * 1000 : 60 * 10 * 1000;
                    if (settings.timeout) {
                        $timeout.cancel(settings.timeout);
                    }
                    
                    settings.timeout = $timeout(function() {
                        executeIdleTimeout();
                    }, settings.idleTime, false);
                } else {
                    if (settings.timeout) $timeout.cancel(settings.$timeout);
                    settings.timeout = null;
                }
            };

            function executeIdleTimeout() {
                if (settings.handleTimeout === true && settings.isAuthenticate === true) {
                    var modalInstance = $injector.get('$modalStack');
                    if (modalInstance) modalInstance.dismissAll();
                    $rootScope.$broadcast(AppDefine.Events.IDLETIMEOUT);
                } 
            }

            function pauseIdleTimeout() {
                settings.handleTimeout = false;
            }

            function runIdleTimeout() {
                settings.handleTimeout = true;
            }

            function registerIdleTimeout() {
                settings.isRegister = true;
                settings.isAuthenticate = AccountSvc.isAuthenticated();
                $document.find(settings.elemAttach).on(settings.eventKeepAlive, handleTimeoutFn);
            }

            function cancelIdleTimeout() {
                $document.find(settings.elemAttach).off(settings.eventKeepAlive, handleTimeoutFn);
            }

            return service;

        };
    });
})();
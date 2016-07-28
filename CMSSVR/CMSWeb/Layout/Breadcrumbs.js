(function() {
    'use strict';

    define(['cms'], function(cms) {
        cms.controller('breadcrumbsCtrl', breadcrumbsCtrl);

        function breadcrumbsCtrl($scope, cmsBase) {
            var vm = this;

            active();

            function active() {

            }
        }
    });
})();
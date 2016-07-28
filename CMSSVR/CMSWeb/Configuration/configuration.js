(function () {

    'use strict';

    define(['cms', 'Services/dialogService'], function (cms) {

        cms.register.controller('configurationCtrl', configurationCtrl);
        configurationCtrl.$inject = ['$scope', 'cmsBase'];
        function configurationCtrl($scope, cmsBase) {
            var vm = this;
            vm.menu = { menuHead: [], menuBody: [] }
            vm.menuName = null;

            active();

            function active() {
                var statecurrent = cmsBase.$state.current();
                
                menu();
            }

            function menu() {

                var current = $state.current;
                var routeArray = current.name.split(".");
                var routeName = "";
                if (routeArray.length == 0) {
                    routeName = current;
                } else {
                    routeName = routeArray[0];
                }

                var mem = router.getRouteParent(routeName);
                vm.menuName = mem.translate;
                if ( mem.childs != null && mem.childs.length > 0 ) {
                    var i = 0;
                    angular.forEach(mem.child, function (m) {
                        if (i < 4) {
                            vm.menu.menuHead.push(m);
                        } else {
                            vm.menu.menuBody.push(m);
                        }
                        i++;
                    });
                }
            }
        }
    });
})();
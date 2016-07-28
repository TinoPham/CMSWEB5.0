(function() {
    'use strict';

    define(['cms'], function(cms) {
        cms.register.directive('inRoles',inRolesFn);

        inRolesFn.$inject = ['dataContext'];
        function inRolesFn(dataContext) {
            return {
                restrict: 'A',
                link: function (scope, element, attrs) {
                    var hasAccess = false;
                    var allowAccess = attrs.inRoles.split(",");
                    for (var i = 0; i < allowAccess.length; i++) {
                        if (dataContext.auth.userInRoles(allowAccess[i])) {
                            hasAccess = true;
                            break;
                        }
                    }

                    if (!hasAccess) {
                        angular.forEach(element.children(), function(child) {
                            removeElement(child);
                        });
                        removeElement(element);
                    }
                }
            };
        }

        function removeElement(element) {
            element && element.remove && element.remove();
        }
    });
})();
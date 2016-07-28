define(['App'], function (app) {
    
    app.register.directive('clearinput', function ($parse) {
        return {
            restrict: "A",
            replace: false,
            transclude: false,
            compile: function (element, attrs) {
                var input = element.children('input[type=text]');
                button = element.children('.btn-clear'),
                button.attr('tabindex', '-1');
                var modelAccessor;
                if (input != null) {
                    var ngmodel = input.attr("ng-model");
                    modelAccessor = $parse(ngmodel);
                }


                return function (scope, element, attrs, controller) {
                    button.on("click",
                    function () {
                        input.val(null);
                        if (modelAccessor != null && modelAccessor != undefined)
                            modelAccessor.assign(scope, null);
                    }
                    );
                }

            }
        };
    });
}
    );

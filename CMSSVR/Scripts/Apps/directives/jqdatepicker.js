
define(['App', 'metro.calendar','metro.datepicker'], function (app) {
    app.register.directive('jqdatepicker', function ($parse) {
            return {
                restrict: "E",
                replace: true,
                transclude: false,
                compile: function (element, attrs) {
                    var modelAccessor = $parse(attrs.ngModel);
                    var html = "<div class=\"input-control text\" data-role=\"datepicker\" data-locale='en' ng-model=\"sdate\" data-format=\"dd/mm/yyyy\" data-position=\"top|bottom\" style=\" height:27px\"  data-effect=\"none|slide|fade\">";
                    html += "<input type=\"text\" style=\" padding:5px\" > <button class=\"btn-date\" style=\" top:2px\" ></button></div>";

                    var newElem = $(html);
                    element.replaceWith(newElem);

                    return function (scope, element, attrs, controller) {

                        var processChange = function () {
                            var date = new Date(element.datepicker("getDate"));

                            scope.$apply(function (scope) {
                                // Change bound variable
                                modelAccessor.assign(scope, date);
                            });
                        };
                        var selected = function (d, d0) {
                            scope.$apply(function (scope) {
                                // Change bound variable
                                modelAccessor.assign(scope, d0);
                            });
                        };

                        element.datepicker({
                            inline: true

                            , selected: selected
                            , click: processChange
                        });

                        scope.$watch(modelAccessor, function (val) {
                            var date = new Date(val);
                            element.datepicker("setDate", date);

                        });

                    };

                }
            };

        });

});


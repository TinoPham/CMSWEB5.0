define(['App'], function (app) {
    app.register.directive('showonhoverparent',
       function () {
           return {
               link: function (scope, element, attrs) {
                   element.parent().bind('mouseenter', function () {
                       element.addClass("ui-state-active");
                   });
                   element.parent().bind('mouseleave', function () {
                       element.removeClass("ui-state-active");
                   });
               }
           };
       });
}
    )


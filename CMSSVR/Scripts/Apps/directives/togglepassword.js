define(['App'], function (app) {
    app.register.directive('togglepassword',
       function ($parse) {
           return {
               restrict: "A",
               replace: false,
               transclude: false,
               compile: function (element, attrs) {

                   var button = element.children('.btn-reveal'),
                    input = element.children('input[type=password]');
                   button.attr('tabindex', '-1');
                   button.attr('type', 'button');

                   return function (scope, element, attrs, controller) {
                       button.on('mousedown', function (e) {
                           input.attr('type', 'text');
                       }
                       );
                       button.on('mouseup, mouseleave, blur', function (e) {
                           input.attr('type', 'password').focus();
                       });
                   }

               }
           };
       });
}
    )

(function () {
    'use strict';

    define(['cms'], function (cms) {
        cms.register.controller('advertCtrl', advertCtrl);

        advertCtrl.$inject = ['$scope', 'dataContext', 'cmsBase', 'AppDefine'];

        function advertCtrl($scope, dataContext, cmsBase, AppDefine) {
            console.log('START VVVVVVVVVVVVVVVVVVVVVV video');
            var vid = document.getElementById("video");
            //vid.play();
            $scope.$on('$destroy', function () {
                console.log('STOP VVVVVVVVVVVVVVVVVVVVVV  video');
                
                vid.pause();
                $('video').remove();
            });
        }
    });
})();
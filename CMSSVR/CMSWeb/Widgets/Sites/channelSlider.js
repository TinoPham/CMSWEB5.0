
(function () {
    define(['cms'], channelSlider);
    function channelSlider(cms) {
        cms.register.controller('channelSliderCtrl', channelSliderCtrl);
        channelSliderCtrl.$inject = ['$scope', 'dataContext', 'cmsBase', 'AppDefine', '$timeout', '$filter', '$modalInstance','items'];
        function channelSliderCtrl($scope, dataContext, cmsBase, AppDefine, $timeout, $filter, $modalInstance,items) {
        	var apiURL = AppDefine.Api.SiteChanImage;//"../api/cmsweb/site/GetImageChannel?";

            $scope.active = 0;

            $scope.getImages = function (channel) {
                return apiURL + "name=C_" + $scope.pad(channel.ChannelNo + 1) + ".jpg&kdvr=" + channel.KDVR.toString();
            }
            $scope.model = [];
            active();
            function active() {
                items.model.forEach(function (x,key) {
                    var file = {
                        active: false,
                        ChannelNo: x.ChannelNo,
                        Name:x.Name,
                        KDVR: x.KDVR
                    }

                    if (x.ChannelNo == items.file.ChannelNo) {
                        file.active = true;
                    }
                    $scope.model.push(file);

                });
                $scope.$applyAsync();
            }
            $scope.pad = function (number) {

                return (number < 10 ? '0' : '') + number.toString();
            }
            
            $scope.cancel = function () {
                $modalInstance.close();
            }

        }
    }
})();
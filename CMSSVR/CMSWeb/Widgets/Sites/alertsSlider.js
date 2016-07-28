
(function () {
    define(['cms'], alertsSlider);
    function alertsSlider(cms) {
        cms.register.controller('alertsSliderCtrl', alertsSliderCtrl);
        alertsSliderCtrl.$inject = ['$scope', 'dataContext', 'cmsBase', 'AppDefine', '$timeout', '$filter', '$modalInstance', 'items'];
        function alertsSliderCtrl($scope, dataContext, cmsBase, AppDefine, $timeout, $filter, $modalInstance, items) {
            var imgURL = AppDefine.Api.SiteAlerts + '/GetSensorSnapshot?';

            $scope.active = 0;

            $scope.getImages = function (imgName, kdvr) {
                return imgURL + "filename=" + imgName + "&kdvr=" + kdvr.toString();
            }
            $scope.model = [];
            active();
            function active() {
             
                var alertreq = {
                    kdvrs: items.model.KDVR,
                    channelNo: items.model.ChannelNo,  
                    timeZone: $filter('date')(items.model.TimeZone, AppDefine.SITE_TAPS.datedataformat, 'UTC')
                };

                //dataContext.sitealert.GetImagesAlert(alertreq, function (data) {
                var data = items.model.Image.split(",");
                    data.forEach(function (x, key) {
                        var file = {
                            active: false,
                            ChannelNo: items.model.ChannelNo,
                            Name: x,
                            Index: key + 1,
                            KDVR: items.model.KDVR,
                            Des: items.model.Description
                        }

                        if (x == items.file) {
                            file.active = true;
                        }
                        $scope.model.push(file);
                    });

                    // No images
                    if ($scope.model.length == 0) {
                        var file = {
                            active: true,
                            ChannelNo: items.model.ChannelNo,
                            Name: items.file,
                            KDVR: items.model.KDVR
                        }
                        $scope.model.push(file);
                    }

            };
            //, function (error) {
            //        var _error = error;
                //});
            //}
            $scope.pad = function (number) {

                return (number < 10 ? '0' : '') + number.toString();
            }
            
            $scope.cancel = function () {
                $modalInstance.close();
            }

            function getChannelTime(name) {
                var imageName = name;
                var timeStamp = imageName.split("_")[2].split(".")[0];

                return UnixTime_To_Date(timeStamp);
            }

            function UnixTime_To_Date(unix) {
                //var date = new Date(unix * 1000);
                var date =  new Date(Date.UTC(1970, 1, 1, 0, 0, 0, 0));
                date.setSeconds(unix);
                return date;
            }
            $scope.time = function (name) {
       
                return $filter('date')(getChannelTime(name), "hh:mm:ss a");
            }

        }
    }
})();
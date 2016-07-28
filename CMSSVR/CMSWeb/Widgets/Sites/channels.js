(function () {
    define(['cms', 'Widgets/sites/channelSlider'], channels);
    function channels(cms) {
        cms.register.controller('channelsCtrl', channelsCtrl);
        channelsCtrl.$inject = ['$scope', 'dataContext', 'cmsBase', 'AppDefine', '$timeout', '$filter','$modal','$window'];
        function channelsCtrl($scope, dataContext, cmsBase, AppDefine, $timeout, $filter, $modal, $window) {

			var apiURL = AppDefine.Api.SiteChanImage;//"../api/cmsweb/site/GetImageChannel?";
           $scope.geticonStatus = function (index) {
               if (index == null) return AppDefine.CHANNEL_STATUS[5].icon;
               return AppDefine.CHANNEL_STATUS[index].icon;
           }
           $scope.getStatus = function (index) {

               if (index == null) return "";
                   return AppDefine.CHANNEL_STATUS[index].Des;

           }
           $scope.init = function (maps_model) {
               $scope.$broadcast(AppDefine.Events.SITE_TAPS.CHANGE_DATA, undefined);
               $scope.$broadcast(AppDefine.Events.SITE_TAPS.CHANGE_NODE_TREE, maps_model);
           }

           $scope.SetHeight = function (count) {
               // Cal display panel-body
               if (count != 1) { return; }
               var panelbody = angular.element(document.querySelector('.panel-body'));
               var tabcontent = angular.element(document.querySelector('.tab-content'));
               var height = parseInt(tabcontent.height()) - 130;
               panelbody.css('max-height', height + 'px');
           }
            $scope.provider = [];
            $scope.$on(AppDefine.Events.SITE_TAPS.CHANGE_DATA, function (e, agr) {
  
                $scope.provider = dataContext.channels;//.sort(function (a, b) { return a.ChannelNo - b.ChannelNo });
                if ($scope.provider === undefined) { return;}
            	if ($scope.provider.length > 0) {

            		$scope.provider[0].sort(function (a, b) { return parseInt(a.ChannelNo, 10) - parseInt(b.ChannelNo, 10); });
            	}
            });
            $scope.listIsShow = [];
            $scope.$on(AppDefine.Events.SITE_TAPS.CHANGE_NODE_TREE, function (e, node) {

                $scope.listIsShow = [];
                var flag = false;

                var Sites = [];
                angular.forEach(node.Sites, function (value, key) {
                    var v = value;
                    Sites.push(v);
                });
                if (Sites === undefined) { return; }
                if (Sites.length > 0) {
                    Sites.sort(function (a, b) { return parseInt(a.ID, 10) - parseInt(b.ID, 10); });
                }

                angular.forEach(Sites, function (value, key) {
                    var ObjIsShow = {
                        isShow: undefined,
                        isCollapseIn: undefined
                    };
                    if (value.Type === AppDefine.NodeType.DVR) {
                        if (value.MACAddress === undefined || value.MACAddress === null || value.MACAddress === "") {
                            ObjIsShow.isShow = false;
                            ObjIsShow.isCollapseIn = false;
                            $scope.listIsShow.push(ObjIsShow);
                        } else {
                            ObjIsShow.isShow = true;
                            ObjIsShow.isCollapseIn = false;
                            if (flag === false){
                                ObjIsShow.isCollapseIn = true;
                                flag = true;
                            }
                            $scope.listIsShow.push(ObjIsShow);
                        }
                    }
                });
                
                //console.log('Last Alert$scope.$watchGroup');
            });

            $scope.dvropen = false;
            $scope.pad = function (number) {

                return (number < 10 ? '0' : '') + number.toString();
            }
            $scope.getImages = function (channel) {
                return apiURL + "name=C_" + $scope.pad(channel.ChannelNo + 1) + ".jpg&kdvr=" + channel.KDVR.toString();
            }
            $scope.ismobileView = false;
            $scope.$watch(function () {
                return $window.innerWidth;
            }, function (value) {
                $scope.ismobileView = value <= 850;
               // return console.log(value);
            });
            
            $scope.showSlide = function (selected, file) {

                if (!$scope.modalShown) {
                    $scope.modalShown = true;
                    var userDeleteInstance = $modal.open({
                        templateUrl: 'Widgets/sites/channelSlider.html',
                        controller: 'channelSliderCtrl',
                        size: 'lg',
                        backdrop: 'static',
                        backdropClass: 'modal-backdrop',
                        keyboard: false,
                        resolve: {
                            items: function () {
                                return {
                                    model: selected,
                                    file: file
                                }
                            }
                        }
                    });

                    userDeleteInstance.result.then(function (data) {
                        $scope.modalShown = false;

                        if (!data) {
                            return;
                        }
                    });
                }
            }
        }
    }
})();
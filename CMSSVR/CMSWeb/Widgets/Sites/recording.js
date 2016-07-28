(function () {
    'use strict';
    define(['cms'], recordings);
    function recordings(cms) {
        cms.register.controller('recordingCtrl', recordingCtrl);
        recordingCtrl.$inject = ['$scope', 'dataContext', 'cmsBase', 'AppDefine', '$timeout', '$rootScope', '$filter'];
        function recordingCtrl($scope, dataContext, cmsBase, AppDefine, $timeout, $rootScope, $filter)
        {
            var isActive = false;
            $scope.chartprovider = {};
            $scope.recordingprovider = {};
            $scope.oldID = 0;
            $scope.detechBrowser = $rootScope.cmsBrowser;
            $scope.$on(AppDefine.Events.SITE_TAPS.CHANGE_NODE_TREE, function (e, agr) {
                if (agr.Type == 1)
                {
                        $scope.init(agr);
                    if ($scope.oldID != agr.ID) {
                        $scope.recordingprovider = agr;
                    }
                    if (isActive)
                    {
                        $scope.getDVRInfo();
                    $scope.oldID = agr.ID;
                    }
                }
            });


            $scope.$on(AppDefine.Events.SITE_TAPS.SELECT_REC, function (event,model) {
                isActive = true;
                if ($scope.oldID != model.ID) {
                    $scope.getDVRInfo();
                    $scope.oldID = model.ID;
                };
            });
            $scope.$on(AppDefine.Events.SITE_TAPS.DES_SELECT_REC ,function(){
                isActive = false;
            });

            $scope.getDVRInfo = function () {
                dataContext.siteadmin.getDVRInfo($scope.recordingprovider.ID, $scope.getDVRSuccess, $scope.getDVRError);
            }
            $scope.containerClass = ['col-md-12 recording_item', 'col-md-12 recording_item no_data'];
            $scope.percentFixclass = ['has0', 'has100'];
            $scope.chartClass = function ($last) {
                if ($last)
                {
                    if ($scope.chartList.length % 2 != 0) {
                        return 'col-lg-12';
                    }
                    else {
                        return 'col-lg-6 col-md-9 col-sm-12';
                    }
                }
                else {
                    return 'col-lg-6 col-md-9 col-sm-12';
                }
                    
            }
            $scope.isHaveData = function (value) {
                if (value.TotalDiskSize == null || value.FreeDiskSize == null) return false;
                if (value.TotalDiskSize == 0) return false;
                return true;
            }
            $scope.DVRInfo = [];
            $scope.Round = function (value) {
                return Math.round(value);
            }

            $scope.UsedSpace = function (value)
            {
                if (value.TotalDiskSize == 0) return 0;
                return $scope.Round((value.TotalDiskSize - value.FreeDiskSize) * 100 / value.TotalDiskSize);
            }
            $scope.FreeSpace = function (value) {
                if (value.TotalDiskSize == 0) return 0;
                return $scope.Round(value.FreeDiskSize * 100 / value.TotalDiskSize);
            }

            $scope.TotalDiskSize = function (value) {
                return $filter('number')(value.FreeDiskSize, 0);
            }

            $scope.progressStatus = function (value) {
                var number = $scope.FreeSpace(value);
                if (number >= 55) return 'progress-bar-success';
                if (number < 55 && number >= 15) return 'progress-bar-warning';
                if (number < 15 ) return 'progress-bar-danger';
                return '';

            }

            $scope.recyclePercent = function (cc) {
                var result = (((cc.TotalDiskSize - cc.FreeDiskSize) * 100) / cc.TotalDiskSize);
                result = Math.round(result);
                if ($filter('number')(result, 0) === "") {
                    result = 0;
                }
                return result.toString() + "%";
            }
            $scope.getDVRSuccess = function (data) {
                $scope.chartList = [];
                dataContext.channels = []
                $scope.DVRInfo = data
                angular.forEach(data, function (value, key) {
                    value.TotalDiskSize = (value.TotalDiskSize != null && value.TotalDiskSize != undefined) ? value.TotalDiskSize : 0;
                    value.FreeDiskSize = (value.FreeDiskSize != null && value.FreeDiskSize != undefined) ? value.FreeDiskSize : 0;
                    $scope.chartprovider.data[0].value = value.TotalDiskSize - value.FreeDiskSize;
                    $scope.chartprovider.data[1].value = value.FreeDiskSize;
                    $scope.chartprovider.chart.caption = value.ServerID;
                    dataContext.channels.push(value.Channels);
                    var caption = value.MinDateRec + '-' + value.MaxDateRec;
                    if (caption.length <= 1) caption = "0";
                    $scope.chartprovider.chart.subCaption =  $filter('translate')(AppDefine.Resx.RECORDING_DATES) + caption;
                    var obj = {};
                    angular.copy($scope.chartprovider, obj);
                    obj.DVRInfo = [value]
                    $scope.chartList.push(obj);
                });
                $rootScope.$broadcast(AppDefine.Events.SITE_TAPS.CHANGE_DATA, data);
                return data;
            };
            $scope.getDVRError = function (error) {
            };
            $scope.chartList = [];
            $scope.init = function (node) {
               // 
                $scope.recordingprovider = node;
            }
            $scope.chartprovider = {
                "chart": {
                    "caption": "Recording status.",
                    "subCaption": "Recording status.",
                    "paletteColors": "#0075c2,#8e0000",
                    "bgColor": "#ffffff",
                    "showBorder": "0",
                    "use3DLighting": "0",
                    "showShadow": "0",
                    "enableSmartLabels": "0",
                    "startingAngle": "0",
                    "showPercentValues": "1",
                    "showPercentInTooltip": "0",
                    "decimals": "1",
                    "captionFontSize": "14",
                    "subcaptionFontSize": "14",
                    "subcaptionFontBold": "0",
                    "toolTipColor": "#ffffff",
                    "toolTipBorderThickness": "0",
                    "toolTipBgColor": "#000000",
                    "toolTipBgAlpha": "80",
                    "toolTipBorderRadius": "2",
                    "toolTipPadding": "5",
                    "showHoverEffect": "1",
                    "showLegend": "1",
                    "legendBgColor": "#ffffff",
                    "legendBorderAlpha": "0",
                    "legendShadow": "0",
                    "legendItemFontSize": "10",
                    "legendItemFontColor": "#666666",
                    "useDataPlotColorForLabels": "1",
                    "legendPosition": "RIGHT"

                },
                "data": [
                     {
                         "label": "Free Space",
                         "value": "491000"
                     },
                    {
                        "label": "Used Space",
                        "value": "1250400"
                    }

                ]
            }
            
        }
    };
 
})();
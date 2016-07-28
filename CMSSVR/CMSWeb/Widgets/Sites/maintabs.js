(function () {
    'use strict';
    define(['cms',
        'widgets/sites/maps',
        'widgets/sites/recording',
        'widgets/sites/channels',
        'widgets/sites/alerts',
        'widgets/sites/sitedetails'], maintabs); 
    function maintabs(cms) {
        cms.register.controller('maintabsCtrl', maintabsCtrl);
        maintabsCtrl.$inject = ['$scope', 'dataContext', 'cmsBase', 'AppDefine', '$timeout', '$rootScope','$window'];
        function maintabsCtrl($scope, dataContext, cmsBase, AppDefine, $timeout, $rootScope, $window)
        {
            var id = 0;
            var old_id = 0;

            $scope.TabIndex = {
                map: 0,
                recording: 1,
                channel: 2,
                alert: 3,
                site: 4
            }

            $scope.maps_model = {};            
            $scope.activealert = false;
            $scope.init = function (node, alerttabindex, alertSelected) {
                if (id != node.ID)
                {
                    id = node.ID;
                }
                $scope.maps_model = node;

                if (alerttabindex) {
                    $scope.activealert = true;
                    $scope.changeTab($scope.TabIndex.alert);
                    $scope.alertSelected = alertSelected;
                    $scope.numTab = $scope.TabIndex.alert;
                } else {
                    $scope.alertSelected = undefined;
                    $scope.activealert = false;
                    $scope.numTab = $scope.TabIndex.map;
                }

                $timeout(function () {
                    $(".tab-content").css('height', $window.innerHeight - 190 + "px");
                }, 100);
                /*Set height defaul for Main tab content" */

            };

            $scope.changeTab = function (index) {
                    switch (index) {
                        case $scope.TabIndex.map:
                        case $scope.TabIndex.recording:
                        case $scope.TabIndex.channel:
                            break;
                        case $scope.TabIndex.alert:
                            $scope.selectAlerts($scope.maps_model);
                            break;
                    }
            }
            $scope.$on(AppDefine.Events.SITE_TAPS.CHANGE_NODE_TREE, function (event, agr) {
                $scope.maps_model = agr;
            });
            $scope.DeselectAlerts = function () {
                $rootScope.$broadcast(AppDefine.Events.SITE_TAPS.DES_ALERTS);
            }
            $scope.selectAlerts = function  (node){
                $rootScope.$broadcast(AppDefine.Events.SITE_TAPS.GET_ALERTS, node);

                $scope.desselectMap();
                $scope.desselecRec();
                $scope.deselectsite();
            }
            $scope.selectMaps = function (node) {
                $rootScope.$broadcast(AppDefine.Events.SITE_TAPS.SELECT_MAPS, node);

                $scope.desselecRec();
                $scope.DeselectAlerts();
                $scope.deselectsite();
            }
            $scope.selectRec = function (node) {
                $rootScope.$broadcast(AppDefine.Events.SITE_TAPS.SELECT_REC, node);

                $scope.desselectMap();
                $scope.DeselectAlerts();
                $scope.deselectsite();
            }
            $scope.desselecRec = function () {
                $rootScope.$broadcast(AppDefine.Events.SITE_TAPS.DES_SELECT_REC);
            }
            $scope.desselectMap = function () {
                $rootScope.$broadcast(AppDefine.Events.SITE_TAPS.DES_SELECT_MAPS);
            }
            $scope.selectsite = function (node) {
                $rootScope.$broadcast(AppDefine.Events.SITE_TAPS.SELECT_SITES, node);

                $scope.desselectMap();
                $scope.desselecRec();
                $scope.DeselectAlerts();
              
                $timeout(function () {
                    $('.row_site_detail >div:nth-child(2)').height($('.row_site_detail >div:first-child').height() -32 )
                       
                },100);
                              
            }

            $scope.deselectsite = function () {
                $rootScope.$broadcast(AppDefine.Events.SITE_TAPS.DES_SELECT_SITES);
            }
            $scope.ismobileView = false;

            $scope.$watch(function () {
                return $window.innerWidth;
            }, function (value) {
                $scope.ismobileView = value <= 850;
                //return console.log(value);
            }); /* END :: Watch document with*/

           
             $scope.$watch(function () { 
                return $window.innerHeight;
            }, function (value) {

                    $timeout(function () {
                    $(".tab-content").css('height', $window.innerHeight - 190 + "px");                         
                     }, 4000);   
                        
            }); /* END :: Watch document height*/
             $scope.isMobile = cmsBase.isMobile;
        }
    }
})();


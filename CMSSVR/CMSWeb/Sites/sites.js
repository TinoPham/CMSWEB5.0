(function() {
    'use strict';

    define(['cms',
        'Scripts/Directives/treeComponent',
        //'widgets/sites/sitemonitor',
        'widgets/sites/lastalertmonitor',
        'widgets/sites/sensormonitor',
        'widgets/sites/siteboxsummary',
        'DataServices/ReportService',
        'widgets/boxgadgets/boxgadget',
        'widgets/sites/maintabs',
        'widgets/sites/email',
        'configuration/sites/helpers',
  
    ], function (cms) {
        cms.register.controller('siteCtrl', siteCtrl);

        siteCtrl.$inject = ['$rootScope', '$scope', '$modal', 'cmsBase', 'dataContext', 'AppDefine', '$timeout', 'siteadminService', '$window', 'AccountSvc', '$stateParams', '$filter'];

        function siteCtrl($rootScope, $scope, $modal, cmsBase, dataContext, AppDefine, $timeout, siteadminService, $window, AccountSvc, $stateParams, $filter) {

            $scope.SHOW_HIDE_TREE = "SHOW_HIDE_TREE";
            $scope.CHANGE_NODE_TREE = "CHANGE_NODE_TREE";
            var online = '"Pram":{"cmp":"DVROnline","date":"today"}, "Gui":{"SizeClass":"col-lg-3 col-md-6","Tranding":false, "WidgetSelectClass":"widget-num-dvronline"}';
            var offline = '"Pram":{"cmp":"DVROffline","date":"today"}, "Gui":{"SizeClass":"col-lg-3 col-md-6","Tranding":false, "WidgetSelectClass":"widget-num-dvroff"}';
            var urgent = '"Pram":{"cmp":"AlertSeverity","date":"today","int":24,"week":"1","value":"4"}, "Gui":{"SizeClass":"col-lg-3 col-md-6","label":"URGENT_ALERT", "WidgetSelectClass":"widget-num-ugentalert"}';
            var sensor = '"Pram":{"cmp":"AlertType","date":"today","int":24,"week":"1","value":"9"}, "Gui":{"SizeClass":"col-lg-3 col-md-6","label":"SENSOR", "WidgetSelectClass":"widget-num-sensor"}';
            
            $scope.def = {
                Id: 'ID',
                Name: 'Name',
                Type: 'Type',
                Checked: 'Checked',
                Childs: 'Sites',
                Count: 'SiteCount',
                Model: {}
            }

            var model = {
                ID: 0,
                Name: "",
                MACAddress: 0,
                UserID: 0,
                ParentKey: null,
                ImageSite: 0,
                PACData: 0,
                Sites: [],
                SiteCount: 0,
                Type: 0,
                Checked: false
            };

            $scope.isEditSite = false;

            $scope.options = {
                Node: {
                    IsShowIcon: true,
                    IsShowCheckBox: false,
                    IsShowNodeMenu: false,
                    IsShowAddNodeButton: false,
                    IsShowAddItemButton: false,
                    IsShowEditButton: true,
                    IsShowDelButton: true,
                    IsDraggable: false
                },
                Item: {
                    IsAllowFilter: false,
                    IsShowItemMenu: false
                },
                Type: {
                    Folder: 0,
                    Group: 1,
                    File: 3

                },
                CallBack: {
                    //EditNode: callbackEdit,
                    //DelNode: callbackDelNode,
                    SelectedFn: selectedFn,
                    DblClickNode: doubleClickNode,
                    SetIconFile: setIconFile,
                    SetIconGroup: setIconGroup
                    //DragOver: dragOver,
                    //DragEnd: dragEnd
                }
            }
            var SMALL_PATTERN = 768;
            var LARGE_PATTERN = 1680;
            $rootScope.isHideTree = false;
            $scope.isSmall = false;
            $scope.siteBox = initSiteBox();
            $scope.userLogin = AccountSvc.UserModel();
            $scope.isBusy = false;
            var bodyheight, scrollelementtree;
            var vm = this;
            vm.activetab = {
                heath: true,
                sensor: false,
                email: false
            }

            active();
            

            $scope.$on("boxclick", function (event, param) {
            	var name = param.name;
                if (name === AppDefine.Resx.NUMBER_SENSOR) {
                    vm.activetab.sensor = true;
                    vm.activetab.heath = false;
                    vm.activetab.email = false;
                } else {
                    vm.activetab.sensor = false;
                    vm.activetab.heath = true;
                    vm.activetab.email = false;
                }
                //}
            	//if (name === "NUMBER_DVROFF" || name === "NUMBER_SENSOR" || name === "NUMBER_DVRRECORDLESS") {
            	//	$scope.activeDVR = true;
            	//}
            });

         

            $scope.refresh = function () {
                $scope.isBusy = false;
                return dataResync().finally(function() {
                    $scope.isBusy = false;
                });
            }


            if (document.body.clientWidth >= LARGE_PATTERN) {
                $rootScope.isHideTree = true;
            } else {
                $rootScope.isHideTree = false;
            }


            function initGUI() {

                if (cmsBase.isMobile) { /*For mobile*/
                    if (document.body.clientWidth <= SMALL_PATTERN) {
                        bodyheight = $window.innerHeight - 50;
                    } else {
                        bodyheight = setbodysize();
                    }
                    

                    //$scope.arrowHide = false; /*Show class for arrow button*/  
                    $scope.isSmall = true;
                    //$scope.isHideTree = false;
                    scrollelementtree = angular.element('.tree-site-addmin');
                    scrollelementtree.css('height', bodyheight + 'px');
                } else {
                    bodyheight = setbodysize() - 50;
                }

                var elem = angular.element('#content');
                rescale(elem);
            }


            angular.element($window).on("resize", initGUI);

            $scope.$on('$destroy', function () {
                cleanUp();
                angular.element($window).off("resize", initGUI);
            });
           
            $scope.jqueryScrollbarOptions = {
                ignoreMobile: true,
                onInit: function () {
                    bodyheight = setbodysize();
                    scrollelementtree = angular.element('.tree-site-addmin');
                    scrollelementtree.css('height', bodyheight + 'px');
              
                },
                onUpdate: function () {
                    bodyheight = setbodysize();
                    scrollelementtree.css('height', bodyheight + 'px');
                    //var oftop = findOffsetTop(scrollelementtree[0]);
                    //var height = $window.innerHeight - oftop+ 'px';
                    //scrollelementtree.css('height', height.toString());                 
                }
            }
            

            $scope.selectDetailSites = function(alert) {

                var sitekey = alert.Info.SiteKey;

                var alertsfirst = Enumerable.From(alert.TypeAlerts.Alert)
                    .Where(function (x) { return x.Kalert !== -1 })
                    .OrderBy(function (x) { return x.TimeZone; })
                    .FirstOrDefault();
                
                if (alertsfirst) {
                    var compareDate = new Date(1970, 0, 2,0,0,0,0);
                    var comparedate2 = new Date(alertsfirst.TimeZone.substring(0, 10));
                    var now = new Date();
                    if (comparedate2 < compareDate) {
                        var timeZone = new Date(now.getFullYear(), now.getMonth(), now.getDate() - 7);
                        alertsfirst.TimeZone = $filter('date')(timeZone, "yyyy-MM-ddTHH:mm:ss.sss") + "Z";
                    }
                }

                var node = siteadminService.findNode($scope.treeSiteFilter, 1, sitekey);

                $scope.alertSelected = {
                    node: node,
                    firstAlert: alertsfirst
                }
                $scope.tabselect = true;
                selectedFn(node);
                $scope.$broadcast('cmsTreeRefresh', node);
            }


                           
            var cleanUp = $rootScope.$on('SHOW_HIDE_TREESITE', function (e) {
                e.preventDefault();
                    $scope.showhideTree(null,e); 
            } );


             $scope.SHOW_HIDE_TREESITE_LOCAl = function(e, arrowHide){                
                    $scope.showhideTree(null,e);
            }

        
            $scope.showhideTree = function(elem, event) {                
                var elem = angular.element('#content');
                $rootScope.isHideTree = !$rootScope.isHideTree;
                 rescale(elem);
                 $scope.fixTreesitemobile = $scope.isSmall && $rootScope.isHideTree;
            }
            $scope.showhideTreeTAB = function (elem, event) {
            	var elem = angular.element('#content');
            	$rootScope.isHideTree = !$rootScope.isHideTree;
            	rescaleTAB(elem);
            	$scope.fixTreesitemobile = $scope.isSmall && $rootScope.isHideTree;
            }
            $scope.countSite = function () {
                $rootScope.sitesCount = siteadminService.siteCountFn($scope.treeSiteFilter);
                return $rootScope.sitesCount;
            }

             function setbodysize() {
                 var croll = angular.element('.tree-panel');
                 var oftop = findOffsetTop(croll[0]);
                 return $window.innerHeight - oftop - 40;
             }

            function initSiteBox() {
                return  [
                {
                    Id: 0,
                    SizeClass: 'col-sm-6 col-md-3',
                    Class: 'connected',
                    PreTitle: 'DVR',
                    Title: 'DVR_CONNECTED',
                    modelBox: {
                        Name: 'NUMBER_DVRONLINE',
                        TemplateParams: online
                    }
                },
                {
                    Id: 1,
                    SizeClass: 'col-sm-6 col-md-3',
                    Class: 'offline',
                    PreTitle: 'DVR',
                    Title: 'DVR_OFFLINE',
                    modelBox: {
                        Name: 'NUMBER_DVROFF',
                        TemplateParams: offline
                    }
                },
                {
                    Id: 2,
                    SizeClass: 'col-sm-6 col-md-3',
                    Class: 'urgent',
                    PreTitle: 'DVR',
                    Title: 'DVR_URGENT',
                    modelBox: {
                        Name: 'NUMBER_URGENT',
                        TemplateParams: urgent
                    }
                },
                {
                    Id: 3,
                    SizeClass: 'col-sm-6 col-md-3',
                    Class: 'sensor',
                	PreTitle: 'NUMBER_STRING',
                    Title: 'DVR_SENSOR',
                    modelBox: {
                        Name: "NUMBER_SENSOR",
                        TemplateParams: sensor
                    }
                }
                ];
            }
            function rescaleTAB(elem) {
            	elem.css('width', 0 + '%');
            	angular.element('#sidebar').css('width', 100 + '%');
            	$timeout(function () {
            		angular.element('#sidebar').css('height', 'auto');
            	}, 1000, false);
            }
            function rescale(elem) {  
                // console.log($window.innerHeight);
                if (!$rootScope.isHideTree) {
                    if (cmsBase.isMobile) { /*For mobile*/
                        $scope.isSmall = true;
                        if (document.body.clientWidth < SMALL_PATTERN) {
                            return;
                        }
                        elem.css('width', 0 + '%');
                        angular.element('#sidebar').css('width', 100 + '%');
                        $timeout(function () {
                            angular.element('#sidebar').css('height', 'auto');
                        }, 1000, false);                                   
                    } else { /*For FC*/
                        $scope.isSmall = false;
                        if (document.body.clientWidth < SMALL_PATTERN) {
                            elem.css('width', 0 + '%');
                            angular.element('#sidebar').css('width', 100 + '%');
                            return;
                        }
                        //set hide fusionchart
                        var fus = angular.element('fusioncharts');
                        fus.css('display', 'none');

                        elem.css('width', 0 + '%');
                        var sidebar = angular.element('#sidebar');
                        sidebar.css('width', 100 + '%');
                        sidebar.css('padding-right', 0);

                        $timeout(function () {
                            fus.css('display', 'block');
                        }, 1000, false);
                    }
                } else {
                    
                    if (cmsBase.isMobile) { /*For mobile */
                        $scope.isSmall = true;
                        if (document.body.clientWidth < SMALL_PATTERN) {
                            //elem.css('width', 100 + '%');
                            //angular.element('#sidebar').css('width', 0 + '%');
                            //angular.element('#sidebar').css('height', '0'); 
                        } else {
                            var scoperegion = angular.element('.content');
                            var scopesize = parseInt(scoperegion.css('width'));
                            elem.css('width', scopesize + 'px');
                            angular.element('#sidebar').css('width', 0 + 'px');
                            $timeout(function () {
                                angular.element('#sidebar').css('height', 'auto');
                            }, 1000, false);
                        }
                        
                 
                    } else {/* For PC */
                        $scope.isSmall = false;

                        if (document.body.clientWidth < SMALL_PATTERN) {
                            elem.css('width', 100 + '%');
                            angular.element('#sidebar').css('width', 0 + '%');
                           // angular.element('#sidebar').css('height', '0');
                            return;
                        }

                        var resizeScopeElm = angular.element('#resizer-scope');
                        var resizerScopeWidth = parseInt(resizeScopeElm.css('width'));
                        var leftSize = resizerScopeWidth - 375;
                        //set hide fusionchart
                        var fus = angular.element('fusioncharts');
                        fus.css('display', 'none');
                        elem.css('width', 400 + 'px');
                        var sidebar = angular.element('#sidebar');
                        sidebar.css('width', leftSize + 'px');
                        sidebar.css('padding-right', '25px');
                        $timeout(function () {
                            fus.css('display', 'block');
                        }, 1000, false);
                    }

                } /*End else*/
            }

            function findOffsetLeft(elm) {
                var result = 0;
                if (elm.offsetParent) {
                    do {
                        result += elm.offsetLeft;
                    } while (elm = elm.offsetParent);
                }
                return result;
            }

            function findOffsetTop(elm) {
            	if (!elm) { return; }
                var result = 0;
                if (elm.offsetParent) {
                    do {
                        result += elm.offsetTop;
                    } while (elm = elm.offsetParent);
                }
                return result;
            }

            function selectedFn(node, scope) {
                
                if (node.Type === AppDefine.NodeType.Region) {
                    $scope.selectedNode = node;
                    $scope.$broadcast($scope.CHANGE_NODE_TREE, $scope.selectedNode);
                    $scope.$apply();
                }
                if (node.Type === AppDefine.NodeType.Site) {
                    if (scope) {
                        $scope.tabselect = false;
                    } else {
                        $scope.tabselect = true;
                    }
                    $scope.selectedNode = node;
                $scope.$broadcast($scope.CHANGE_NODE_TREE, $scope.selectedNode);
                    $scope.$applyAsync();
                }

                if (cmsBase.isMobile) {
                    $scope.showhideTree(null, null);
                }
            }

            function doubleClickNode(node, scope) {
                scope.props.collapsed = !scope.props.collapsed;
                scope.props.toggleCollapsed(scope.props.collapsed);
            }

            function setIconGroup(node, scope) {
                switch (node.Type) {
                    case 1:
                        return {
                            Expand: 'icon-store-3 choose',
                            Collapsed: 'icon-store-3'
                        };
                        break;
                    case 2:
                        var expand = "icon-dvr-2 ";
                        var collapsed = "icon-dvr-2 ";

                        switch (node.OnlineStatus) {
                            case 0:
                                expand += "dvr_offline";
                                break;
                            case 1:
                                expand += "dvr_online";
                                break;
                            case 2:
                                expand += "dvr_blocked";
                                break;
                            case 3:
                                expand += "dvr_schedule";
                                break;
                            default:
                                expand = expand;
                        }

                        return {
                            Expand: expand,
                            Collapsed: expand
                        };
                        break;
                    default:
                        return {
                            Expand: 'icon-dvr-2',
                            Collapsed: 'icon-dvr-2'
                        };
                }
            }

            function setIconFile(node, scope) {
                switch (node.Status) {
                    case 0:
                        return 'icon-cam-block-2 chs_disable';
                        break;
                    case 1:
                        return 'icon-videocam-2 chs_notrecording';
                        break;
                    case 2:
                        return 'icon-videocam-2 chs_recording';
                        break;
                    case 3:
                        return 'icon-cam-block-2 chs_videoloss';
                        break;
                    case 4:
                        return 'icon-videocam-2 chs_resdisable';
                        break;
                    default:
                        return 'icon-videocam-2 chs_null';
                }
            }

            function active() {
            	if ($stateParams.obj != null) {
	                if ($stateParams.obj.param.name == "NUMBER_SENSOR") {
	                    vm.activetab.sensor = true;
	                    vm.activetab.heath = false;
	                } else {
	                    vm.activetab.sensor = false;
	                    vm.activetab.heath = true;
	                }

	            }

                $scope.isBusy = true;

                dataContext.injectRepos(['configuration.siteadmin', 'sites.sitealert']).then(function () {
                    dataResync().finally(function () { $scope.isBusy = false; });
                });
            }

            function dataResync() {
                var defer = cmsBase.$q.defer();
                var getsiteparam = { hasChannel: true, allUsers: false };
                if ($scope.userLogin.IsAdmin) {
                    getsiteparam.allUsers = true;
                }

                dataContext.siteadmin.getSitesWithChannels(getsiteparam, function(data) {
                    if (!data || data.data && data.data.ReturnMessage && data.data.Data === null) {
                        $scope.data = model;
                        $scope.treeSiteFilter = $scope.data;
                        return;
                    }

                    $scope.data = data;
                    $scope.treeSiteFilter = $scope.data;
                    //$timeout(function () {

                    if (!$scope.selectedNode) {
                        $scope.selectedNode = $scope.treeSiteFilter;
                    } else {
                        var node = siteadminService.findNode($scope.treeSiteFilter, $scope.selectedNode.Type, $scope.selectedNode.ID);
                        if (!node) {
                            $scope.selectedNode = $scope.treeSiteFilter;
                        }
                    }
                    //}, 150, false);
                    defer.resolve();
                    //if (document.body.clientWidth <= SMALL_PATTERN) {
                    initGUI();
                        //$scope.isHideTree = !$scope.isHideTree;
                    //}
                    dataContext.sitedata = $scope.data;

                }, function(error) {
                    defer.reject();
                    siteadminService.ShowError(error);
                });

                return defer.promise;
            }


            $scope.selectSearch = function() {
               $scope.countSiteBox= true;
            }    

            $scope.lostFocusSearch = function() {
                $scope.countSiteBox= false;
            }


          

        }
    });
})();


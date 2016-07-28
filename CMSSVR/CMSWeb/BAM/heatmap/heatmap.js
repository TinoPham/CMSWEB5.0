(function () {
	'use strict';
	var rptPram = 0;
	define(['cms',
		'DataServices/Bam/DistributionSvc',
		'Scripts/Services/chartSvc',
		'bam/distribution/queue_define',
		'widgets/bam/charts/saleNewCharts',
	    'Widgets/Bam/Modal/showdetailimage',
	    'Widgets/Bam/Modal/manuallyupload',
	    'Widgets/Bam/Modal/schedulesetting'],
		function (cms) {
		    cms.register.controller('HeatMapCtrl', HeatMapCtrl);
		    HeatMapCtrl.$inject = ['$modal', '$window', '$rootScope', '$scope', '$state', '$filter', '$timeout', 'cmsBase', 'AppDefine', 'dataContext', 'AccountSvc', 'DistributionSvc', 'chartSvc', 'bamhelperSvc', '$upload', 'siteadminService']; //'PanZoomService',

		    function HeatMapCtrl($modal, $window, $rootScope, $scope, $state, $filter, $timeout, cmsBase, AppDefine, dataContext, AccountSvc, DistributionSvc, chartSvc, bamhelperSvc, $upload, siteadminService) { //PanZoomService
		        var vm = this;

				vm.showTypeList = { showAll: 0, count: 1, dwell: 2 };
				vm.detechBrowser = $rootScope.cmsBrowser;
				
				vm.showDataTypes = [
					{ Key: vm.showTypeList.showAll, Name: 'CB_SHOW_ALL' },
					{ Key: vm.showTypeList.count, Name: 'COUNT_STRING' },
					{ Key: vm.showTypeList.dwell, Name: 'DWELL_STRING' }
				];
				vm.showTypeSelected = vm.showDataTypes[0].Key; //default show all
				vm.hasShowTypes = { showCount: true, showDwell: true };

				vm.treeDef = {
					Id: 'ID',
					Name: 'Name',
					Type: 'Type',
					Checked: 'Checked',
					Childs: 'Sites',
					Count: 'SiteCount',
					Model: {}
				}

				vm.querySite = '';
				vm.treeSiteFilterS = null;
				vm.sitetreeS = null;
				vm.siteloaded = false;
				vm.selectedSites = [];
				$rootScope.isHideTree = false;
				vm.zoomToSelected = {};
				$scope.dateSearchList = [];
				var SMALL_PATTERN = 768;
				var MEDIUM_PATTERN = 992;
				var LARGE_PATTERN = 1680;

				$scope.isMobile = cmsBase.isMobile;
				vm.subViews = [
				{ Key: 1, Name: 'Distribution' }
				, { Key: 2, Name: 'Heat Map' }
				, { Key: 3, Name: 'Direction' }
				];
				$scope.DISTRIBUTION_NUM_ROUND = 10000;
				vm.showNoData = false;

				//$scope.list1 = [{'id': 'A1223', 'title': 'Lolcat Shirt' }, {'id':'A2345', 'title': 'Cheezeburger Shirt' }, {'id':'B8676', 'title': 'Buckit Shirt' }];

				//$scope.ondrophandler = function (event, ui) {
				//	console.log(ui.draggable[0].className);
				//	$timeout(function () {
				//		$("#" + ui.draggable[0].className).css("position", "absolute").css("top", ui.position.top).css("left", ui.position.left);
				//	}, 10, false);
				//	$scope.$applyAsync();

				//};

				//$scope.list4 = [];

				$scope.$on("changeDistributionView", function (e, pram) {
					$scope.viewSelected = pram;
				    //console.log($scope.viewSelected);

					if ($scope.viewSelected.Key == 1)
					{


					} else if ($scope.viewSelected.Key == 2) {
					    initDataHeatMap();
					}

				});


				$scope.clickOutside  = function ($event,element) {

					if(angular.element(element).hasClass('open')) {
						angular.element(element).removeClass('open');
						//console.log('have open');
					} else {
						//console.log('dont have open');
					}
				};

				$scope.zoomToTableWare = function () {
					PanZoomService.getAPI('PanZoom').then(function (api) {
						api.zoomToFit(tableware);
					});
				};

				$scope.zoomToShoes = function () {
					PanZoomService.getAPI('PanZoom').then(function (api) {
						api.zoomToFit(shoes);
					});
				};

				function SetShowTypeFlags() {
					if (vm.showTypeSelected == vm.showTypeList.showAll) {
						vm.hasShowTypes.showCount = true;
						vm.hasShowTypes.showDwell = true;
					}
					else {
						vm.hasShowTypes.showCount = vm.showTypeSelected == vm.showTypeList.count ? true : false;
						vm.hasShowTypes.showDwell = vm.showTypeSelected == vm.showTypeList.dwell ? true : false;
					}
				}

			    // HEAT MAP
				vm.treeSiteFilterHM = null;
				vm.isShowHMTree = false;
				vm.userLoginHM = AccountSvc.UserModel();

				hideTreeSiteRight();

				$scope.$on(AppDefine.Events.GETBAMREPORTDATA, function (e, arg) {
				    if (!arg.FilterDistribution) { return; }
				    //if ($scope.viewSelected.Key == 1) {
				    //    var pram = {
				    //        rptDataType: arg.FilterDistribution.Type.ID,
				    //        siteKeys: '',
				    //        sDate: arg.FilterDistribution.FromDate,
				    //        eDate: arg.FilterDistribution.EndDate
				    //    };
				    //    getReportData(pram);

				    //} else if ($scope.viewSelected.Key == 2) {

				    getDataHeatMap(1);
				    $scope.$applyAsync();
				    //}
				});

		        //Anh, Wait for the header param data has been set
				$scope.$on(AppDefine.Events.ACTIVESALESREPORT, function (e) {
				    initDataHeatMap();
				});

				$scope.$on(AppDefine.Events.BAMSELECTNODE, function (e, node) {
				    //dataContext.injectRepos(['configuration.siteadmin'])
				    //	.then(getAllRegionSites);
				    initDataHeatMap();
				});

				function hideTreeSiteRight() {
				    var elem = angular.element('#content');
				    $rootScope.isHideTree = false;
				    rescale(elem);
				    $scope.fixTreesitemobile = $scope.isSmall && $rootScope.isHideTree;
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

				vm.zoomTo = function (value) {
				    switch (value.key) {
				        case 1:
				            $scope.zoomToTableWare();
				            break;
				        case 2:
				            $scope.zoomToShoes();
				            break;
				        default:
				            break;
				    }
				}

				function initDataHeatMap() {
				    // Create tree site witj DVR

				    if (!vm.treeSiteFilterHM) {
				        showAllSites();
				    }
                    
				    // Select data from DVR

				}

				vm.querySiteHeatMap = '';
				vm.optionsHM = {
				    Node: {
				        IsShowIcon: true,
				        IsShowCheckBox: false,
				        IsShowNodeMenu: true,
				        IsShowAddNodeButton: false,
				        IsShowAddItemButton: false,
				        IsShowEditButton: false,
				        IsShowDelButton: false,
				        IsDraggable: false
				    },
				    Icon: {
				        Item : 'icon-dvr-2'
				    },
				    Item: {
				        IsAllowFilter: false,
				        IsShowItemMenu: false
				    },
				    CallBack: {
				        SelectedFn: selectedFnHM
				    },
				    Type: {
				        Folder: 0,
				        Group: 1,
				        File: 2
				    }
				}

				function showAllSites() {
				    dataContext.siteadmin.getSites(function (data) {
				        $scope.data = data;
				        if ($scope.data && $scope.data.Sites.length > 0 && $scope.querySiteHeatMap) {
				            vm.treeSiteFilterHM = angular.copy($scope.data);
				            //$scope.treeSiteFilter.Sites = siteadminService.filterSites($scope.treeSiteFilter, $scope.querySiteHeatMap);
				        } else {
				            vm.treeSiteFilterHM = $scope.data;
				        }
				        vm.isShowHMTree = true;
				        vm.siteloaded = true;
				        var nameSite = '';
				        var firstDVR = bamhelperSvc.getFirstDVRNoVirtual(vm.treeSiteFilterHM);
				        if (firstDVR === undefined) { return;}
				        nameSite = bamhelperSvc.getSitesFromDVR(vm.treeSiteFilterHM, firstDVR.ParentKey, nameSite);

				        selectedFnHM(firstDVR);
				        if (vm.selectedSiteNameHM === '')
				        {
				            vm.selectedSiteNameHM = nameSite;
				        }

				    }, function (error) {
				        siteadminService.ShowError(error);
				    });

				}

				function selectedFnHM(node, scope) {
				    //console.log(scope.props.parentNode);
				    $scope.selected = node;
				    if (node.Type !== AppDefine.NodeType.DVR) {
				        return;
				    }

				    vm.treeTemp.TempIsVirtual = node.IsVirtual;
				    vm.treeTemp.TempselectedSitesHM = node.ParentKey;
				    vm.treeTemp.TempselectedSiteNameHM = scope !== undefined && scope.props !== undefined && scope.props.parentNode !== undefined ? scope.props.parentNode.Name : '';

				    vm.treeTemp.TempselectIDDVR = $scope.selected.ID;
				    vm.treeTemp.TempselectedDVRName = $scope.selected.Name;

				    if (vm.initflag == true)
				    {
				        if (vm.treeTemp.TempIsVirtual == true) {
				            $scope.IsMsgVirtual = true;
				        }
				        GetDataOnTree(vm.treeTemp.TempIsVirtual);
				        vm.initflag = false;
				    }
				    
				    $scope.$applyAsync();
				}

				vm.initflag = true;
				vm.treeTemp = {
				    TempIsVirtual: false,
				    TempselectIDDVR: undefined,
				    TempselectedDVRName: undefined,
				    TempselectedSitesHM: undefined,
				    TempselectedSiteNameHM : undefined
				};
				$scope.IsMsgVirtual = false;

				vm.TreeSiteHMClose = function () {
				    if ($("#btn-popMenuConvSitesHM").parent().hasClass("open")) {
				        $("#btn-popMenuConvSitesHM").parent().removeClass("open");
				        $("#btn-popMenuConvSitesHM").prop("aria-expanded", false);
				    }
				    if ($scope.selected.Type !== AppDefine.NodeType.DVR) {
				        return;
				    }
				    $scope.IsMsgVirtual = false;
				    if (vm.treeTemp.TempIsVirtual == true)
				    {
				        $scope.IsMsgVirtual = true;
				    }
				    GetDataOnTree($scope.IsMsgVirtual);
				}

				function GetDataOnTree(virtual)
				{
				    vm.selectedSitesHM = vm.treeTemp.TempselectedSitesHM;
				    vm.selectedSiteNameHM = vm.treeTemp.TempselectedSiteNameHM;
				    vm.selectIDDVR = vm.treeTemp.TempselectIDDVR;
				    vm.selectedDVRName = vm.treeTemp.TempselectedDVRName;

				    if (virtual == true)
				    {
				        return;
				    }

				    getlistChannels(vm.selectIDDVR);
				    getDataHeatMap(1);
				}

				function getlistChannels(pKDVR) {
				    var param = {
				        KDVR: pKDVR
				    }
				    DistributionSvc.getlistChannels(param, function (data) {
				        if (data) {
				            if (data.length === 0)
				            {
				                vm.DVRChannels = {
				                    KChannel : 0,
				                    Name : "Channel 0",
				                    ChannelNo : 0
				                }
				            } else {
				                vm.DVRChannels = data;
				            }
				        }
				    },
                     function (error) {
                         cmsBase.cmsLog.error('error');
                     });

				}

				function getDataHeatMap(page) {
				    
				    var params = {
				        rptDataType: $rootScope.FilterDistribution.Type.ID,
				        KDVR: vm.selectIDDVR !== undefined ? vm.selectIDDVR.toString() : '',
				        sDate: angular.isString($rootScope.FilterDistribution.FromDate) ? $rootScope.FilterDistribution.FromDate : $filter('date')(new Date($rootScope.FilterDistribution.FromDate), AppDefine.DateFormatCParamST),
				        eDate: angular.isString($rootScope.FilterDistribution.EndDate) ? $rootScope.FilterDistribution.EndDate : $filter('date')(new Date($rootScope.FilterDistribution.EndDate), AppDefine.DateFormatCParamED),
				        PageNo: page,
				        PageSize: 24
				    };
				    
				    DistributionSvc.getDataHeatMap(params, function (data) {
				        if (data) {
				            vm.HMImagesData = data;
				            
				            if (vm.HMImagesData.mapsImage === undefined || vm.HMImagesData.mapsImage === null) {
				                //cmsBase.cmsLog.error("No Data");
				                return;
				            }
				            if (vm.HMImagesData.mapsImage.length == 0) {
				                //cmsBase.cmsLog.error("No Data");
				                return;
				            }
				            $scope.TotalPages = vm.HMImagesData.TotalPages;
				            for (var i = 0; i < vm.HMImagesData.mapsImage.length; i++) {
				                //$scope.GetImages(data.mapImage[i], true);
				                vm.HMImagesData.mapsImage[i].ImageURL = getimageURL(vm.HMImagesData.mapsImage[i].ImageID, vm.HMImagesData.mapsImage[i].Title.split('.')[0]);
				            }

				        } else {
				            //cmsBase.cmsLog.error("No Data");
				        }
				    },
					function (error) {
					    cmsBase.cmsLog.error('error');
					});
				}

				function getimageURL(img, name) {
				    //if(Createdby){
				    //    return AppDefine.Api.DistributionImages + KChannel.toString() + AppDefine.HEAT_MAP.filename_p + filename + AppDefine.HEAT_MAP.typestime_p + $rootScope.FilterDistribution.Type.ID + AppDefine.HEAT_MAP.isManual_p;
				    //} else {
				    //    return AppDefine.Api.DistributionImages + KChannel.toString() + AppDefine.HEAT_MAP.filename_p + filename + AppDefine.HEAT_MAP.typestime_p + $rootScope.FilterDistribution.Type.ID + AppDefine.HEAT_MAP.isSchedule_p;
				    //}
				    return AppDefine.Api.DistributionImages + img.toString() + AppDefine.HEAT_MAP.filename_p + name;
				}

				$scope.showDetailImage = function (selected, file, siteName, DVRName) {

				    if (!$scope.modalShown) {
				        $scope.modalShown = true;
				        var userDeleteInstance = $modal.open({
				            templateUrl: 'Widgets/Bam/Modal/showdetailimage.html',
				            controller: 'showdetailimageCtrl',
				            size: 'md',
				            backdrop: 'static',
				            backdropClass: 'modal-backdrop',
				            keyboard: false,
				            resolve: {
				                items: function () {
				                    return {
				                        model: selected,
				                        file: file,
				                        siteName: vm.selectedSiteNameHM,
				                        KDVR: vm.selectIDDVR,
				                        DVRName: vm.selectedDVRName,
				                        Channels: vm.DVRChannels,
				                        IDSchedule: $rootScope.FilterDistribution.Type.ID
				                    }
				                }
				            }
				        });

				        userDeleteInstance.result.then(function (data) {
				            $scope.modalShown = false;

				            if (!data) {
				                return;
				            } else {
				                getDataHeatMap(1);
				                $scope.$applyAsync();
				            }
				        });
				    }
				}

				$scope.showMunuallyUpload = function () {
				    if (!$scope.modalShown) {
				        $scope.modalShown = true;
				        var userDeleteInstance = $modal.open({
				            templateUrl: 'Widgets/Bam/Modal/manuallyupload.html',
				            controller: 'manuallyuploadCtrl',
				            size: 'md',
				            backdrop: 'static',
				            backdropClass: 'modal-backdrop',
				            keyboard: false,
				            resolve: {
				                items: function () {
				                    return {
				                        Channels: vm.DVRChannels,
				                        KDVR: vm.selectIDDVR,
				                        File: undefined,
                                        parentForm: false
				                    }
				                }
				            }
				        });

				        userDeleteInstance.result.then(function (data) {
				            $scope.modalShown = false;

				            //if (!data) {
				            //    return;
				            //}
				            getDataHeatMap(1);
				            $scope.$applyAsync();
				        });
				    }
				}

				$scope.showSettingSchedules = function () {
				    if (!$scope.modalShown) {
				        $scope.modalShown = true;
				        var userDeleteInstance = $modal.open({
				            templateUrl: 'Widgets/Bam/Modal/schedulesetting.html',
				            controller: 'schedulesettingCtrl as vm',
				            size: 'md',
				            backdrop: 'static',
				            backdropClass: 'modal-backdrop',
				            keyboard: false,
				            resolve: {
				                items: function () {
				                    return {
				                        parentForm: false
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

			    /////////////////////////////////////////////////////////////////////
			    //                              Paging                             //
			    /////////////////////////////////////////////////////////////////////
				$scope.currPage = 1;

				$scope.prevPage = function (data) {

				    if ($scope.currPage <= 1) {
				        return;
				    }

				    if (data) {
				        $scope.currPage = 1;
				    } else {
				        $scope.currPage = $scope.currPage - 1;
				    }

				    getDataHeatMap($scope.currPage);
				    $scope.$applyAsync();
				}

				$scope.gotoPage = function () {

				    if ($scope.currPage > 0 && $scope.currPage <= $scope.TotalPages) {
				        getDataHeatMap($scope.currPage);
				        $scope.$applyAsync();
				    }
				}

				$scope.nextPage = function (data) {

				    if ($scope.currPage >= $scope.TotalPages) {
				        return;
				    }

				    if (data) {
				        $scope.currPage = data;
				    } else {
				        $scope.currPage = $scope.currPage + 1;
				    }

				    getDataHeatMap($scope.currPage);
				    $scope.$applyAsync();
				}

				$scope.fullSize = function () {
				    if (!$scope.isMax)
				        $scope.isCollapsed = false;
				    $scope.isMax = !$scope.isMax;
				    //console.log('$scope.fullSize')
				}
			}
		});
})();
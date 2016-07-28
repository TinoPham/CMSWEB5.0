(function () {

	'use strict';

	define([
        'cms',
        'Scripts/Services/bamhelperSvc',
		  'widgets/bam/tables/headerBam',
        'Scripts/Directives/treeComponent',
        'DataServices/Bam/BamHeaderReportsSvc',
        'Scripts/Services/chartSvc',
        'DataServices/Configuration/fiscalyearservice',
		'DataServices/Bam/usermetricSvc',
		'Scripts/DataServices/Export/ExportAPIService',
		'Scripts/Services/exportSvc'
	], function (cms) {

		cms.register.controller('bamCtrl', bamCtrl);

		bamCtrl.$inject = ['$rootScope', '$scope', '$modal', '$timeout', '$window', '$stateParams', '$filter', '$state', 'cmsBase', 'dataContext', 'AppDefine', 'siteadminService', 'AccountSvc', 'router', 'bamhelperSvc', 'BamHeaderReportsSvc', 'chartSvc', 'fiscalyearservice', 'usermetricSvc', 'ExportAPISvc', 'exportSvc'];

		function bamCtrl($rootScope, $scope, $modal, $timeout, $window, $stateParams, $filter, $state, cmsBase, dataContext, AppDefine, siteadminService, AccountSvc, router, bamhelperSvc, BamHeaderReportsSvc, chartSvc, fiscalyearservice, usermetricSvc, ExportAPISvc, exportSvc) {
			var vm = this;
			var RELOAD_EVENT = 'Reload';
			var MENU_BAM_CHANGED = 'Changed';
			$scope.SHOW_HIDE_TREE = "SHOW_HIDE_TREE";
			$scope.CHANGE_NODE_TREE = "CHANGE_NODE_TREE";
			$scope.isMobile = cmsBase.isMobile;
			var SMALL_HEIGHT_MOBILE_PATTERN = 375;
			var SMALL_WIDTH_MOBILE_PATTERN = 667;
			var SMALL_PATTERN = 768;
			var MEDIUM_PATTERN = 992;
			var LARGE_PATTERN = 1680;
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
			var dateToday = new Date();
			var bodyheight, scrollelementtree;
			$scope.isiPad = navigator.userAgent.match(/iPad/i) != null;
			vm.currentState = $state.current.name;
			vm.stateList = AppDefine.State;
			
			$scope.TypesDefault = [
                { ID: 1, Name: 'Hourly' },
				{ ID: 2, Name: 'Daily' },
				{ ID: 3, Name: 'Weekly' },
				{ ID: 4, Name: 'WTD' },
				{ ID: 5, Name: 'PTD' },
				{ ID: 6, Name: 'Monthly' }
			];
			$scope.TypesHeatMap = [
                { ID: 1, Name: 'Hourly' },
				{ ID: 2, Name: 'Daily' },
				{ ID: 3, Name: 'Weekly' }
			];
			$scope.Types = $scope.TypesDefault;
			vm.subViews = [
				{ Key: 1, Name: 'Distribution' }
				, { Key: 2, Name: 'Heat Map' }
				,{ Key: 3, Name: 'Direction' }
			];
			vm.selectedRptType = $scope.Types[1]; //default selected Daily
			$scope.maskBamMenu = false;
			$scope.BamSelectedNode = null;
			$scope.isMobile = cmsBase.isMobile;
			$scope.def = {
				Id: 'ID',
				Name: 'Name',
				Type: 'Type',
				Checked: 'Checked',
				Childs: 'Sites',
				Count: 'SiteCount',
				Model: {}
			}
			$rootScope.HeaderData = {
			    Caculate: 0,
			    POSdata : 0,
			    Trafficdata : 0,
			    Normalized: 0
			}
			$rootScope.ParamNormalize = {
			    sDate: null,
			    eDate: null,
			    siteKey : []
			}
			$scope.isScroll = true;
			$rootScope.BamFilter = {
			    dateReport: setTime(dateToday, 23, 59, 59, 999),
			    startdateReport: setTime(dateToday, 0, 0, 0, 0)
			};
			$rootScope.CustomFilter = {
			    //FromDate: sDate,
			    //EndDate: eDate,
			    TypeCustom: vm.selectedRptTypeCustom,
			    //SiteKeys: ''
			}
			$scope.maxdate = setTime(dateToday, 23, 59, 59, 999);
			//$rootScope.FilterSaleReport = {
			//	FromDate: setTime(new Date(), 0, 0, 0, 0),
			//	EndDate: setTime(new Date(), 23, 59, 59, 999),
			//	Compare: 0,
			//	Type: {
			//		ID: 0,
			//		Name: ''
			//	},
			//	SiteKeys: ''
			//};
			$scope.datestatus = {};
			$scope.dateOptions = {
				formatYear: 'yy',
				startingDay: 1,
				showWeeks: false
			};
			$scope.isEditSite = false;
			$scope.treeOptions = {
				Node: {
					IsShowIcon: true,
					IsShowCheckBox: true,
					IsShowNodeMenu: false,
					IsShowAddNodeButton: false,
					IsShowAddItemButton: false,
					IsShowEditButton: true,
					IsShowDelButton: true,
					IsDraggable: false
				},
				Icon: {
					Item: 'icon-store-3'
				},
				Item: {
					IsAllowFilter: false,
					IsShowItemMenu: false
				},
				Type: {
					Folder: 0,
					Group: 2,
					File: 1

				},
				CallBack: {
					SelectedFn: selectedFn,
					CheckNodeFn: checkNodeFn
				}
			}
			$rootScope.isHideTree = false;
			$scope.isSmall = false;
			$scope.userLogin = AccountSvc.UserModel();
			$scope.isBusy = false;
			vm.activetab = {
				heath: true,
				sensor: false
			}
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
			$scope.ExportPage = {
				BAMDashboard: 1,
				WeekAtAGlance: 2,
				SaleReport: 3,
				Distribution: 4,
				DriveThrough: 5
			};
			$scope.ExportFormat = {
				Excel: 1,
				PDF: 2,
				CSV: 3
			};
			$scope.TypesCustom = [
				{ ID: 1, Name: 'Daily' },
				{ ID: 2, Name: 'Weekly' }
			];
			$scope.ListPeriod = [];
			vm.subViewSelected = vm.subViews[0];//Default selected 'Distribution'.

			$scope.checkShowExportButton = function () {
			    switch ($state.current.name) {
			        case AppDefine.State.HEATMAP:
			            return false;
			            break;
			        case AppDefine.State.CUSTOMREPORT:
			            return false;
			            break;
			        default:
			            return true;
			            break;
			    }
			}

			$scope.$on(AppDefine.Events.STATECHANGESUCCESSHANDLER, function (event, arg) {
				vm.currentState = $state.current.name;
				vm.stateList = AppDefine.State;
				vm.selectedRptType = $scope.Types[1];
				vm.selectedRptTypeCustom = $scope.TypesCustom[0];
				$scope.Types = $scope.TypesDefault;
				vm.selectedRptType = $scope.Types[1];

				switch ($state.current.name) {
					case AppDefine.State.BAM:
						$state.go(AppDefine.State.BAM_DASHBOARD);
						break;
					case AppDefine.State.BAM_DASHBOARD:
						$scope.treeOptions.Node.IsShowCheckBox = true;
						$scope.$broadcast('cmsTreeRefreshTree');
						var elem = angular.element('#content');
						rescale(elem);
						break;
					case AppDefine.State.WEEKATAGLANCE:
						$scope.treeOptions.Node.IsShowCheckBox = false;
						$scope.$broadcast('cmsTreeRefreshTree');
						var elem = angular.element('#content');
						rescale(elem);
						break;
					case AppDefine.State.CUSTOMREPORT:
					    CheckCustomReport();
					    if (vm.currentCustomReports === AppDefine.State.BAM_PERFORMANCECOMPARISION) {
					        // Set date init
					        $rootScope.BamFilter.startdateReport = vm.StartWeekCompare.date;
					        $rootScope.BamFilter.dateReport = vm.StartWeek.date;
					    }
				    //GetListPeriod();
				    //$scope.treeOptions.Node.IsShowCheckBox = false;
				    $scope.$broadcast('cmsTreeRefreshTree');
						break;
					case AppDefine.State.SALEREPORTS:
						$scope.Types = $scope.TypesDefault;
						vm.selectedRptType = $scope.Types[1];
						getStartDateFiscalYear($rootScope.BamFilter.dateReport, MENU_BAM_CHANGED);
						break;
				    case AppDefine.State.DISTRIBUTION:
						//if (vm.subViewSelected.Key === vm.subViews[0].Key) {
				        //    $scope.Types = $scope.TypesDefault;
						//}
						//else {
				        //    $scope.Types = $scope.TypesHeatMap;
				        //}
				        $scope.Types = $scope.TypesDefault;
				        vm.selectedRptType = $scope.Types[1];
				        getStartDateFiscalYear($rootScope.BamFilter.dateReport, MENU_BAM_CHANGED);
				        //$scope.treeOptions.Node = false;
				        //$scope.$broadcast('cmsTreeRefreshTree');
				        break;
				    case AppDefine.State.HEATMAP:
				        $scope.Types = $scope.TypesHeatMap;
				        vm.selectedRptType = $scope.Types[1];
				        getStartDateFiscalYear($rootScope.BamFilter.dateReport, MENU_BAM_CHANGED);
				        break;
					case AppDefine.State.DRIVETHROUGH:
						getStartDateFiscalYear($rootScope.BamFilter.dateReport, MENU_BAM_CHANGED);
						break;
					default:
						$scope.treeOptions.Node.IsShowCheckBox = true;
						$scope.$broadcast('cmsTreeRefreshTree');
						break;
				}

				var flag = setParamNormalize();
				$scope.$broadcast(AppDefine.Events.CHECKNODE, flag);

				//if ($state.current.name == AppDefine.State.BAM) {
				//	$state.go(AppDefine.State.BAM_DASHBOARD);
				//	return;
				//}
				//vm.currentState = $state.current.name;
				//vm.stateList = AppDefine.State;
				//vm.selectedRptType = $scope.Types[1];
				//vm.selectedRptTypeCustom = $scope.TypesCustom[0];
				
				
				//if (vm.currentState === AppDefine.State.WEEKATAGLANCE) {
				//	$scope.treeOptions.Node.IsShowCheckBox = false;
				//	$scope.$broadcast('cmsTreeRefreshTree');
				//} else if ($state.current.name === AppDefine.State.CUSTOMREPORT) {
				//	CheckCustomReport();
				//	//GetListPeriod();
				//	//$scope.treeOptions.Node.IsShowCheckBox = false;
				//	$scope.$broadcast('cmsTreeRefreshTree');
				//}
				//else {
				//	$scope.treeOptions.Node.IsShowCheckBox = true;
				//	$scope.$broadcast('cmsTreeRefreshTree');
				//}

				//if ($state.current.name === AppDefine.State.SALEREPORTS || $state.current.name === AppDefine.State.DISTRIBUTION
				//	|| $state.current.name === AppDefine.State.DRIVETHROUGH) {
				//	getStartDateFiscalYear($rootScope.BamFilter.dateReport, MENU_BAM_CHANGED);
				//}



				//var flag = setParamNormalize();
				//$scope.$broadcast(AppDefine.Events.CHECKNODE, flag);
			});

			$scope.$on(AppDefine.Events.SUBMITGETBAMREPORT, function (e, arg) {
				$scope.EventRefresh();
			});

			$scope.$on('$destroy', function () {
				cleanUp();
				angular.element($window).off("resize", initGUI);
			});

			var cleanUp = $rootScope.$on('SHOW_HIDE_TREESITE', function (e) {
				$scope.showhideTree(null, e);
			});

			$scope.$on('BamTableResize', function (event, arg) { 

				if (document.body.clientWidth < 992) { return; }

				var total = angular.element('.wrapper-bam-table').innerWidth();
				var first_column = angular.element('.wrapper-bam-table > .bam-table-column:nth-child(1)').innerWidth();
				var third_column = angular.element('.wrapper-bam-table > .bam-table-column:nth-child(3)').innerWidth();
				var second_column = total - (first_column + third_column);

				// console.log(third_column);
				angular.element('.wrapper-bam-table > .bam-table-column:nth-child(2)').css('width', second_column + 'px');
				
				
				// var f_element = $('.wrapper-bam-table > .bam-tb-scroll .bam-table > div');
				// f_element.find('.tb-col.ng-scope .col-date').width('auto');

  					var e_single = $('.wrapper-bam-table.bam-table-single-data .bam-tb-scroll .bam-table > div');
					var e = $('.wrapper-bam-table .bam-tb-scroll .bam-table > div');
						var x = Math.round( (second_column-150) / arg.dataLength);	//check perfect  width
						var y = e.find('.tb-col.ng-scope  > div').width();		//curent width
						if (x > 140) {
							e.find('.tb-col.ng-scope').width(x);
							e.find('.tb-col.ng-scope  > div').width(x);
						
						} else if (x > 70 && x < 140 ) {
							var e_single = $('.wrapper-bam-table.bam-table-single-data .bam-tb-scroll .bam-table > div');
							e.find('.tb-col.ng-scope').attr('style', '');
							e.find('.tb-col.ng-scope  > div').attr('style', '');
							e_single.find('.tb-col.ng-scope').width(x);
							e_single.find('.tb-col.ng-scope  > div').width(x);							
						}
						else {
							e.find('.tb-col.ng-scope').attr('style', '');
							e.find('.tb-col.ng-scope  > div').attr('style', '');
							e_single.find('.tb-col.ng-scope').width('70');
							e_single.find('.tb-col.ng-scope  > div').width('70');	
						}
			});

			$scope.$on('detechBrowser', function (event) { 

				if ((navigator.userAgent.indexOf("Opera") || navigator.userAgent.indexOf('OPR')) != -1) {
					         vm.detechBrowser =  'forOpera';
					    }
				else if (navigator.userAgent.indexOf("Chrome") != -1) {
					         vm.detechBrowser =  'forChrome';
					    }
				else if (navigator.userAgent.indexOf("Safari") != -1) {
					         vm.detechBrowser =  'forSafari';
					    }
				else if (navigator.userAgent.indexOf("Firefox") != -1) {
					          vm.detechBrowser =  'forFirefox';
					    }
					    else if((navigator.userAgent.indexOf("MSIE") != -1 ) || (!!document.documentMode == true )) //IF IE > 10
					    {
					       vm.detechBrowser =  'forIE'; 
					    }  
					 return vm.detechBrowser;
					    
			});

			$scope.$watch("BamFilter.dateReport", function (newValue, oldValue) {
				setRptParams();
			});

			//$scope.$on('scrollDown', function (event) { 
			//	if (document.body.clientWidth > MEDIUM_PATTERN &&  $( window).scrollTop() < 140) {
			//		$timeout(function () {
			//			window.scrollBy(0, 140);
			//		}, 1000, false);
					
				
			//	}
					

			//}); /* Scroll down make header smaller*/


			angular.element($window).on("resize", initGUI);

			angular.element($window).bind("scroll", function () {

			
                if (cmsBase.isMobile) { /*For mobile*/
                    if (document.body.clientWidth <= SMALL_WIDTH_MOBILE_PATTERN) {
				        if ($window.scrollY > 0 || $window.pageYOffset > 0) {
				            $scope.isScroll = true;
				            if (angular.element('.bamPanel_small')[0].clientHeight !== undefined) {
				                if (angular.element('.bamPanel_small')[0].clientHeight <= 50) {
				                    angular.element('.panelContent').css('margin-top', angular.element('.bamPanel_small')[0].clientHeight - 20);
				                }
				            }
				            $scope.$applyAsync();
				            
				        } else {
				            $scope.isScroll = false;
				            $scope.$applyAsync();
				            $timeout(function () {
				                if (angular.element('.bamPanel_small')[0].clientHeight !== undefined) {
				                    if (angular.element('.bamPanel_small')[0].clientHeight > 50) {
				                        angular.element('.panelContent').css('margin-top', angular.element('.bamPanel_small')[0].clientHeight - 20);
				                    }
				                }
				                
				            }, 200);
				            
				        }
                    } else if (document.body.clientWidth <= SMALL_HEIGHT_MOBILE_PATTERN) {
                        if ($window.scrollY > 0 || $window.pageYOffset > 0) {
                            $scope.isScroll = true;
                            $scope.$applyAsync();
                        } else {
                            $scope.isScroll = false;
                            $scope.$applyAsync();
                        }
                    }
                    else {
                        //$scope.isScroll = true;
                        //$scope.$applyAsync();
                    }
				} else {
				   
				}

			});

			$scope.$watch(
				function () {
					return angular.element('#sidebar')[0].clientWidth;
				},
				function (newValue, oldValue) {
					if (newValue != oldValue) {

						$timeout(function () {
							angular.element('.row-resizerhead').css('width', angular.element('#sidebar')[0].clientWidth - 25);
						    angular.element('.row-resizersecond').css('width', angular.element('#sidebar')[0].clientWidth - 25);
						    //var elem = angular.element('#content');
						    //rescale(elem);
						}, 200, false);
					}
				}
			);

			checkHeatMap();

			//Page load, reload
			active();

			function checkHeatMap() {
			    vm.isHeatMap = false;

			    if ($scope.userLogin.IsAdmin === true) {
			        vm.isHeatMap = true;
			    } else {
			        angular.forEach($scope.userLogin.Functions, function (value) {
			            if (value.FunctionID === AppDefine.Modules.Function.FUNC_HEATMAP) {
			                vm.isHeatMap = true;
			            }
			        });
			    }
			}

			$scope.ChangeView = function (view) {
				if (!view) {
					vm.subViewSelected = vm.subViews[0];
					$scope.Types = $scope.TypesDefault;
				}
				else {
				    vm.subViewSelected = view;
				    if (vm.subViewSelected.Key === vm.subViews[0].Key) {
				        $scope.Types = $scope.TypesDefault;
				    } else {
				        $scope.Types = $scope.TypesHeatMap;
				    }
				}
				vm.selectedRptType = $scope.Types[1];
				$scope.$broadcast("changeDistributionView", vm.subViewSelected);
			};

			function active() {
				console.log('bam active');
				if (document.body.clientWidth >= LARGE_PATTERN ) {
					$rootScope.isHideTree = true;
				}
				else {
					$rootScope.isHideTree = false;
				}

				if ($state.current.name === AppDefine.State.WEEKATAGLANCE) {
					$scope.treeOptions.Node.IsShowCheckBox = false;
				}

				if ($state.current.name === AppDefine.State.HEATMAP) {
				    $scope.Types = $scope.TypesHeatMap;
				    vm.selectedRptType = $scope.Types[1];
				} else {
				    $scope.Types = $scope.TypesDefault;
				    vm.selectedRptType = $scope.Types[1];
				}

				menu();

				if ($state.current.name === AppDefine.State.CUSTOMREPORT) {
				    //$scope.treeOptions.Node.IsShowCheckBox = false;

				}

				vm.currentState = $state.current.name;
				vm.stateList = AppDefine.State;
				$scope.isBusy = true;

				dataContext.injectRepos(['configuration.siteadmin', 'sites.sitealert']).then(function () {
					dataResync().finally(function () { $scope.isBusy = false; });
				});

				getStartDateFiscalYear($rootScope.BamFilter.dateReport, RELOAD_EVENT).then(function () {
					if ($state.current.name == AppDefine.State.BAM) {
						$state.go(AppDefine.State.BAM_DASHBOARD);
					}
				});
			}

			function setTime(date, hour, minute, second, milisecond) {
				if (hour >= 0) date.setHours(hour);
				if (minute >= 0) date.setMinutes(minute);
				if (second >= 0) date.setSeconds(second);
				if (milisecond >= 0) date.setMilliseconds(milisecond);

				return date;
			}
			
			$scope.startopen = function ($event, elementOpened, elementClose) {
			    $event.preventDefault();
			    $event.stopPropagation();
			    $scope.datestatus[elementOpened] = !$scope.datestatus[elementOpened];

			    $timeout(function () {
			        setDisplayDatepicker(elementOpened, elementClose);
			    }, 200);
			};

			$scope.endopen = function ($event, elementOpened, elementClose) {
			    $event.preventDefault();
			    $event.stopPropagation();
			    $scope.datestatus[elementOpened] = !$scope.datestatus[elementOpened];
			    
			    $timeout(function () {
			        setDisplayDatepicker(elementOpened, elementClose);
			    }, 200);
			};

			function setDisplayDatepicker(elementOpened, elementClose) {
			    if ($scope.datestatus[elementOpened] === true && $scope.datestatus[elementClose] === true) {
			        $scope.datestatus[elementClose] = !$scope.datestatus[elementOpened];
			    }
			}

            // Event Changed Combobox Type
			vm.RptTypeChanged = function () {
				if (vm.selectedRptType.ID === AppDefine.SaleReportTypes.Daily) {
			            changedStartDateFiscalYear($rootScope.BamFilter.dateReport);
			    }
			};
			
			function checkNodeFn(a, b) {
			    //console.log(a);
			    //console.log(b);

			    var flag = setParamNormalize();
			    
			    $scope.$broadcast(AppDefine.Events.CHECKNODE, flag);
			}

			function setParamNormalize(){
			    var sitesKey = [];
			    
			    bamhelperSvc.getSitesFromNode($scope.treeSiteFilter, sitesKey);
			    $rootScope.ParamNormalize.sDate = $rootScope.BamFilter.dateReport;
			    $rootScope.ParamNormalize.eDate = $rootScope.BamFilter.dateReport;
			    $rootScope.ParamNormalize.siteKey = sitesKey;
			    if (sitesKey.length === 1) {
			        return true;
			    } else {
			        return false;
			    }
			}

			function initGUI() {

			    if (cmsBase.isMobile) { /*For mobile*/
			        if (document.body.clientWidth < SMALL_PATTERN) {
			            bodyheight = $window.innerHeight - 50;
			            $scope.isScroll = false;
			            //$scope.$applyAsync();
			        } else {
			        	$scope.isScroll = true;
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

			$scope.offmaskBamMenu = function (e, item) {
			    $scope.selectMenuItem = item;
				$scope.maskBamMenu = false;
			}

			$scope.getMenuName = function () { 				
			    var reportId = $stateParams.id;
			    if (reportId) {
			        var menu = Enumerable.From(vm.CustomMenus).Where(function (x) { return x.ReportId == reportId; }).FirstOrDefault();

			        if (menu) {
			            //$rootScope.title = cmsBase.translateSvc.getTranslate(menu.Name);
			            $rootScope.title = menu.Name;
			        }
			    }

			    return $rootScope.title;

			}

			$scope.customMenuBam = function () {
				if ( document.body.clientWidth < MEDIUM_PATTERN ) {
					return true;
				}
				return false;
			}

			$scope.triggerOpenmenu = function () {				
				 	
				if ( !$( '#bam-dropdown-menu' ).hasClass( "protected" ) && document.body.clientWidth < MEDIUM_PATTERN && navigator.userAgent.search("Safari") >= 0 && navigator.userAgent.search("Chrome") < 0) {
					$('.show_report_name > span').trigger('click');
					$scope.maskBamMenu = true;
				}	
			}

			$scope.triggerChildmenu = function (e) {				
				 	
				if (  document.body.clientWidth < MEDIUM_PATTERN && navigator.userAgent.search("Safari") >= 0 && navigator.userAgent.search("Chrome") < 0) {
					$(e.target).trigger('click');
				}	
			}

			$scope.SHOW_HIDE_TREESITE_LOCAl = function (e, arrowHide) {
			    $scope.showhideTree(null, e);
			}

			$scope.showhideTree = function (elem, event) {
			    var elem = angular.element('#content');
			    $rootScope.isHideTree = !$rootScope.isHideTree;
			    rescale(elem);
			    $scope.fixTreesitemobile = $scope.isSmall && $rootScope.isHideTree;
			}

			$scope.countSite = function () {
			    $rootScope.sitesCount = siteadminService.siteCountFn($scope.treeSiteFilter);
			    return $rootScope.sitesCount;
			}

			function selectedFn(node, scope) {

			    $scope.BamSelectedNode = node;
			    $scope.$broadcast(AppDefine.Events.BAMSELECTNODE, node);

			}

			function setbodysize() {
			    var croll = angular.element('.tree-panel');
			    var oftop = findOffsetTop(croll[0]);
			    return $window.innerHeight - oftop - 40;
			}

			function rescale(elem) {
			    // console.log($window.innerHeight);
			    if (!$rootScope.isHideTree) {
			        if (cmsBase.isMobile) { /*For mobile*/
			            $scope.isSmall = true;

                        // 2016-03-21 Tri: Alway enable header control down with Mobile
			            if (document.body.clientWidth <= SMALL_WIDTH_MOBILE_PATTERN) {
			                $scope.isScroll = false;
			                $scope.$applyAsync();
			            }
			            
			            //if (document.body.clientWidth < SMALL_HEIGHT_MOBILE_PATTERN) {
			            //    return;
			            //}
			            elem.css('width', 0 + '%');
			            angular.element('#sidebar').css('width', 100 + '%');
			            angular.element('.row-resizerhead').css('width', angular.element('#sidebar')[0].clientWidth);
			            angular.element('.row-resizersecond').css('width', angular.element('#sidebar')[0].clientWidth);
			            $timeout(function () {
			                angular.element('#sidebar').css('height', 'auto');
			                if (angular.element('.bamPanel_small')[0].clientHeight > 50) {
			                    angular.element('.panelContent').css('margin-top', angular.element('.bamPanel_small')[0].clientHeight - 20);
			                }
			                
			            }, 1000, false);
			        } else { /*For FC*/
			            $scope.isSmall = false;
			            if (document.body.clientWidth < SMALL_PATTERN) {
			                elem.css('width', 0 + '%');
			                angular.element('#sidebar').css('width', 100 + '%');
			                angular.element('.row-resizerhead').css('width', angular.element('#sidebar')[0].clientWidth);
			                angular.element('.row-resizersecond').css('width', angular.element('#sidebar')[0].clientWidth);
			                return;
			            }

			            elem.css('width', 0 + '%');
			            var sidebar = angular.element('#sidebar');
			            sidebar.css('width', 100 + '%');
			            sidebar.css('padding-right', 0);
			            
			            $timeout(function () {
			                //fus.css('display', 'block');
			                angular.element('.row-resizerhead').css('width', sidebar[0].clientWidth);
			                angular.element('.row-resizersecond').css('width', sidebar[0].clientWidth);
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
			                angular.element('.row-resizerhead').css('width', angular.element('#sidebar')[0].clientWidth - 25);
			                angular.element('.row-resizersecond').css('width', angular.element('#sidebar')[0].clientWidth - 25);
			                // angular.element('#sidebar').css('height', '0');
			                return;
			            }

			            var resizeScopeElm = angular.element('#resizer-scope');
			            var resizerScopeWidth = parseInt(resizeScopeElm.css('width'));
			            var leftSize = resizerScopeWidth - 375;

			            elem.css('width', 400 + 'px');
			            var sidebar = angular.element('#sidebar');
			            sidebar.css('width', leftSize + 'px');
			            sidebar.css('padding-right', '25px');

			        }

			    } /*End else*/
			}

			function findOffsetTop(elm) {
			    var result = 0;
			    if (elm.offsetParent) {
			        do {
			            result += elm.offsetTop;
			        } while (elm = elm.offsetParent);
			    }
			    return result;
			}

			function changedStartDateFiscalYear(curDay) {
			    var def = cmsBase.$q.defer();
			    var cur = chartSvc.FYFormatDate(curDay);
			    fiscalyearservice.GetCustomFiscalYear(cur).then(function (data) {
			        if (data) {
			            // Type Report
			            //YTD
			            vm.fiscalyear = data;
			            //Week To Date
			            // WTD
			            vm.FiscalToWeek = chartSvc.GetFiscalWeek(vm.fiscalyear, $rootScope.BamFilter.dateReport);
			            //Period To Date
			            //PTD
			            vm.FiscalToPeriod = chartSvc.GetFiscalPeriod(vm.FiscalToWeek.WeekNo, vm.fiscalyear.CalendarStyle, new Date(vm.fiscalyear.FYDateStart), new Date(vm.fiscalyear.FYDateEnd), false, vm.fiscalyear.FYNoOfWeeks);
			            $rootScope.BamFilter.startdateReport = vm.FiscalToPeriod.StartDate;
			            setRptParams();
			            def.resolve();

			        } else {
			            def.reject('No data!');
			        }
			    },
                    function (error) {
                        def.reject(error);
                    });
			    return def.promise;
			}

			function getStartDateFiscalYear(curDay, state) {
			    var def = cmsBase.$q.defer();
			    var cur = chartSvc.FYFormatDate(curDay);
			    fiscalyearservice.GetCustomFiscalYear(cur).then(function (data) {
			        if (data) {
			            // Type Report
			           //YTD
			           vm.fiscalyear = data;
			           //Week To Date
			           // WTD
			           vm.FiscalToWeek = chartSvc.GetFiscalWeek(vm.fiscalyear, $rootScope.BamFilter.dateReport);
			           //Period To Date
			           //PTD
			           vm.FiscalToPeriod = chartSvc.GetFiscalPeriod(vm.FiscalToWeek.WeekNo, vm.fiscalyear.CalendarStyle, chartSvc.GetUTCDate(vm.fiscalyear.FYDateStart), chartSvc.GetUTCDate(vm.fiscalyear.FYDateEnd), false, vm.fiscalyear.FYNoOfWeeks);
			           $rootScope.BamFilter.startdateReport = vm.FiscalToPeriod.StartDate;
			            setRptParams();
			        	//if ($state.current.name === AppDefine.State.SALEREPORTS && state === MENU_BAM_CHANGED) {
			            if (state === MENU_BAM_CHANGED) {
			                $scope.$broadcast(AppDefine.Events.ACTIVESALESREPORT);
			            }

			            def.resolve();

			        } else {
			            def.reject('No data!');
			        }
			    },
                    function (error) {
                        def.reject(error);
                    });
			    return def.promise;
			}

			function dataResync() {
			    var defer = cmsBase.$q.defer();
			    dataContext.siteadmin.sitesByPACID(function (data) {
			        if (!data || data.data && data.data.ReturnMessage && data.data.Data === null) {
			            $scope.data = model;
			            $scope.treeSiteFilter = $scope.data;
			            bamhelperSvc.checkallNode($scope.treeSiteFilter);
			            $scope.$broadcast(AppDefine.Events.BAMSELECTNODE, $scope.treeSiteFilter);
			            return;
			        }
			        $scope.data = data;
			        $scope.treeSiteFilter = $scope.data;
			        bamhelperSvc.checkallNode($scope.treeSiteFilter);
			        GetDataHeader();
			        $scope.$broadcast(AppDefine.Events.BAMSELECTNODE, $scope.treeSiteFilter);
			        initGUI();
			        dataContext.sitedata = $scope.data;
					//$scope.$broadcast(AppDefine.Events.ACTIVEDASBOARDREPORT);
					$scope.$broadcast(AppDefine.Events.GETBAMREPORTDATA, $rootScope);
					defer.resolve();

			    }, function (error) {
			        defer.reject();
			        siteadminService.ShowError(error);
			    });

			    return defer.promise;
			}

			$scope.selectSearch = function () {
			    $scope.countSiteBox = true;
			}

			$scope.lostFocusSearch = function () {
			    $scope.countSiteBox = false;
			}

			function menu() {
			    vm.Menus = [];
			    var listMenus = router.getListCollection();
			    var bam = Enumerable.From(listMenus).Where(function (x) { return x.Name === 'bam'; }).FirstOrDefault();

			    //vm.CustomMenus = Enumerable.From(bam.childs).Where(function (x) { return x.Groupkey === 'Custom Reports'; }).ToArray();

			    usermetricSvc.getCustomReports({ groupId: 10 }, function (result) {
			        vm.CustomMenus = result;

			        $rootScope.GCustomMenus = result;

			        // Check report custom display header
			        CheckCustomReport();
			        GetListPeriod();

			    }, function (error) {
			    });

			    if (vm.isHeatMap === true) {
			        vm.Menus = Enumerable.From(bam.childs).Where(function (x) { return x.Groupkey !== 'Custom Reports'; }).GroupBy("$.Groupkey", null, function (key, g) {
			            return { GroupName: key, Childs: g.ToArray() }
			        }).Where(function (x) { return x.GroupName !== "hide" && x.Groupkey !== 'Custom Reports'; }).ToArray();
			    } else {
			        vm.Menus = Enumerable.From(bam.childs).Where(function (x) { return x.Groupkey !== 'Custom Reports' && x.Name !== 'heatmap'; }).GroupBy("$.Groupkey", null, function (key, g) {
			            return { GroupName: key, Childs: g.ToArray() }
			        }).Where(function (x) { return x.GroupName !== "hide" && x.Groupkey !== 'Custom Reports'; }).ToArray();
			    }
			    
			}
			    
		    function GetDataHeader() {
		        var sdate = $filter('date')(new Date(), AppDefine.DFFileName);
		        var edate = $filter('date')(new Date(), AppDefine.DFFileName);
		        var filterParam = { sDate: sdate, eDate: edate, sitesKey: undefined };
		        $scope.sitesKey = [];
		        bamhelperSvc.getSitesFromUser($scope.treeSiteFilter, $scope.sitesKey);
		        // Calculate store header
		        //$rootScope.HeaderData.Caculate = $scope.sitesKey.length;

		        BamHeaderReportsSvc.GetHeaderBam(filterParam, function (data) {
		            if (data) {
		                $rootScope.HeaderData.Caculate = data.Caculate;
		                $rootScope.HeaderData.POSdata = data.POSdata;
		                $rootScope.HeaderData.Trafficdata = data.Trafficdata;
		                $rootScope.HeaderData.Normalized = data.Normalized;
		            }
		            //console.log(data);
		        },
                function (error) {
                    //cmsBase.cmsLog.error('error');
                });
		    }

			$scope.EventRefresh = function () {				
				if ((vm.selectedRptType.ID === AppDefine.SaleReportTypes.Daily) &&
					($state.current.name === AppDefine.State.SALEREPORTS
					|| $state.current.name === AppDefine.State.DISTRIBUTION
					|| $state.current.name === AppDefine.State.DRIVETHROUGH)) {
					if (chartSvc.clearTime($rootScope.BamFilter.startdateReport) > $rootScope.BamFilter.dateReport) {
						cmsBase.cmsLog.error(cmsBase.translateSvc.getTranslate(AppDefine.Resx.FROM_TO_DATE_INVALID_MSG));
						return;
					}
				}
				setRptParams(); //This is important to get reports data.
				GetDataHeader();
				
				// Set parameter date of data compare in customreport => report PERFORMANCECOMPARISION
				if ($state.current.name === AppDefine.State.CUSTOMREPORT && vm.currentCustomReports === vm.stateList.BAM_PERFORMANCECOMPARISION) {
				    $rootScope.BamFilter.startdateReport = vm.StartWeekCompare.date ;
				    $rootScope.BamFilter.dateReport = vm.StartWeek.date;
				}

				// Set parameter date of data compare in customreport => report PERFORMANCECOMPARISION
				if ($state.current.name === AppDefine.State.CUSTOMREPORT && vm.currentCustomReports === vm.stateList.BAM_KEYPERFORMANCEINDICATORS && vm.selectedRptTypeCustom.Name === $scope.TypesCustom[1].Name) {
				    $rootScope.BamFilter.startdateReport = vm.StartWeekCompare.date;
				    $rootScope.BamFilter.dateReport = vm.StartWeek.date;
				}

				if ($state.current.name === AppDefine.State.BAM_DASHBOARD) {
					//$scope.$broadcast(AppDefine.Events.ACTIVEDASBOARDREPORT);
					$scope.$broadcast(AppDefine.Events.GETBAMREPORTDATA, $rootScope);
				}
				else if ($state.current.name === AppDefine.State.BAM_NORMALIZE) {
				    $scope.$broadcast(AppDefine.Events.ACTIVENORMALIZEREPORT);
				}
				else {
				    $scope.$broadcast(AppDefine.Events.GETBAMREPORTDATA, $rootScope);
		        }
		    }

			function setRptParams() {
				var dateSelected = chartSvc.FYFormatDate($rootScope.BamFilter.dateReport);
				fiscalyearservice.GetCustomFiscalYear(dateSelected).then(function (data) {
					if (data) {
						vm.fiscalyear = data;
						vm.FiscalToWeek = chartSvc.GetFiscalWeek(vm.fiscalyear, $rootScope.BamFilter.dateReport);
						$rootScope.GWeek = vm.FiscalToWeek.WeekNo;
					}
				});
				

				var sDate = $filter('date')(new Date($rootScope.BamFilter.startdateReport), AppDefine.DateFormatCParamST);
				var eDate = $filter('date')(new Date($rootScope.BamFilter.dateReport), AppDefine.DateFormatCParamED);
				if (vm.selectedRptType && vm.selectedRptType.ID !== AppDefine.SaleReportTypes.Daily) {
					sDate = eDate;
				}

				$rootScope.FilterWeekAtAGlane = {
					searchDate: $rootScope.BamFilter.dateReport
				};
				$rootScope.FilterSaleReport = {
					FromDate: sDate,
					EndDate: eDate,
					Compare: 0,
					Type: vm.selectedRptType,
					SiteKeys: ''
				};
				$rootScope.FilterDriveThrough = {
					FromDate: $rootScope.BamFilter.startdateReport,
					EndDate: $rootScope.BamFilter.dateReport,
					Compare: 0,
					Type: vm.selectedRptType,
					SiteKeys: ''
				};
	            $rootScope.FilterDistribution = {
					FromDate: sDate,
					EndDate: eDate,
					Type: vm.selectedRptType,
					SiteKeys: ''
				};

	            setParamNormalize();

	            $rootScope.CustomFilter = {
	                FromDate: sDate,
	                EndDate: eDate,
	                TypeCustom: vm.selectedRptTypeCustom,
	                SiteKeys: ''
	            };
			}

            // HEADER of CUSTOM REPORT
		    // Types for Custom Report
			vm.selectedRptTypeCustom = $scope.TypesCustom[0]; //default selected Daily

			function CheckCustomReport(){
			    // Get Id report
			    var reportId = $stateParams.id;
			    if (reportId) {
			        var menu = Enumerable.From(vm.CustomMenus).Where(function (x) { return x.ReportId == reportId; }).FirstOrDefault();

			        if (menu) {
			            // currentCustomReports check show/hide Combobox TypesCustom
			            vm.currentCustomReports = menu.Name;

			            // Show/Hide Tree
			            if (menu.Name === AppDefine.State.BAM_YTDConv || menu.Name === AppDefine.State.BAM_PERFORMANCECOMPARISION || menu.Name === AppDefine.State.BAM_ConversionComparision) {
			                $rootScope.isHideTree = false;
			                $scope.treeOptions.Node.IsShowCheckBox = false;
			                //$scope.treeOptions.Node.IsShowNodeMenu = false;
			                
			                //$rootScope.isSite = false;
						} else if (menu.Name === AppDefine.State.BAM_KEYPERFORMANCEINDICATORS) {
						    $scope.treeOptions.Node.IsShowCheckBox = true;

						    //$rootScope.isSite = true;
			            } else {
						    $scope.treeOptions.Node.IsShowCheckBox = false;
						    //$rootScope.isSite = true;
			            }

			        } else {
			            vm.currentCustomReports = undefined;
			        }
			    } else {
			        vm.currentCustomReports = undefined;
			    }

			    // Event Changed Combobox Type
			    vm.RptTypeCustomChanged = function () {
			        if (vm.selectedRptType.ID === AppDefine.SaleReportTypes.Daily) {
                     
			        } else {

			        }
			    };

			}

		    //vm.ListPeriod = chartSvc.GetFiscalPeriod(new Date(), true);
			
			function GetListPeriod() {
			    var def = cmsBase.$q.defer();
			    // Last Year (-1)
			    var last = new Date();
			    last.setYear(last.getFullYear() - 1);
			    var lastyeardate = chartSvc.FYFormatDate(last);
			    fiscalyearservice.GetCustomFiscalYear(lastyeardate).then(function (data) {
			        if (data) {
			            // Type Report
			            //YTD
			            vm.fiscallastyear = data;
			            //Week To Date
			            // WTD
			            //vm.FiscalToWeek = chartSvc.GetFiscalWeek(vm.fiscalyear, $rootScope.BamFilter.dateReport);
			            $scope.ListPeriod = [];
			            var startDateLastYear = chartSvc.GetUTCDate(vm.fiscallastyear.FYDateStart);
			            var endDateLastYear = chartSvc.GetUTCDate(vm.fiscallastyear.FYDateEnd);
			            for (var i = 0; i < vm.fiscallastyear.FYNoOfWeeks; i++) {

			                var sDate = chartSvc.dateAdds(startDateLastYear, (i * 7));
							var eDate = chartSvc.dateAdds(startDateLastYear, ((i * 7) + 6));
			                if (endDateLastYear < eDate) {
			                    eDate = endDateLastYear;
			                }
			                var datenow = chartSvc.GetUTCDate(lastyeardate);
			                
			                var listPeriod = {
			                    date: sDate,
			                    text: cmsBase.translateSvc.getTranslate(AppDefine.Resx.WEEK_STRING) + (i + 1) + $filter('date')(sDate, " (MM/dd/yyyy - ") + $filter('date')(eDate, "MM/dd/yyyy)")
			                };

			                $scope.ListPeriod.push(listPeriod);
			                
			            }

			            // Now Year
			            var cur = chartSvc.FYFormatDate(new Date());
			            fiscalyearservice.GetCustomFiscalYear(cur).then(function (data) {
			                if (data) {
			                    // Type Report
			                    //YTD
			                    vm.fiscalyear = data;
			                    
			                    var startDateYear = chartSvc.GetUTCDate(vm.fiscalyear.FYDateStart);
			                    var endDateYear = chartSvc.GetUTCDate(vm.fiscalyear.FYDateEnd);

			                    for (var i = 0; i < vm.fiscalyear.FYNoOfWeeks; i++) {

			                        var sDate = chartSvc.dateAdds(startDateYear, (i * 7));
									var eDate = chartSvc.dateAdds(startDateYear, ((i * 7) + 6));
			                        if (endDateYear < eDate) {
			                            eDate = endDateYear;
			                        }
			                        var datenow = chartSvc.GetUTCDate(cur)
			                        if (eDate <= chartSvc.GetUTCDate(cur) || (sDate <= datenow && eDate >= datenow)) {
			                            var listPeriod = {
			                                date: sDate,
			                                text: cmsBase.translateSvc.getTranslate(AppDefine.Resx.WEEK_STRING) + (i + 1) + $filter('date')(sDate, " (MM/dd/yyyy - ") + $filter('date')(eDate, "MM/dd/yyyy)")
			                            };

			                            $scope.ListPeriod.push(listPeriod);
			                        }
			                    }

			                    vm.StartWeek = $scope.ListPeriod[$scope.ListPeriod.length - 1];
			                    vm.StartWeekCompare = $scope.ListPeriod[$scope.ListPeriod.length - 1];

			                    if ($state.current.name === AppDefine.State.CUSTOMREPORT &&  vm.currentCustomReports === vm.stateList.BAM_PERFORMANCECOMPARISION) {
			                        $rootScope.BamFilter.startdateReport = vm.StartWeekCompare.date;
			                        $rootScope.BamFilter.dateReport = vm.StartWeek.date;
			                    } 
			                    if ($state.current.name === AppDefine.State.CUSTOMREPORT) {
			                    	 	$scope.customReportflag = true;
			                    } else {
			                    	$scope.customReportflag = false;
			                    }

			                    def.resolve();

			                } else {
			                    def.reject('No data!');
			                }
			            },
                            function (error) {
                                def.reject(error);
                            });
			            return def.promise;

			            def.resolve();

			        } else {
			            def.reject('No data!');
			        }
			    },
                    function (error) {
                        def.reject(error);
                    });
			    return def.promise;

                
			}

			function GetFYServer(date) {

			}
			
			$scope.ExportTo = function (format) {
				$scope.$broadcast(AppDefine.Events.EXPORTEVENT, { param: format });
			};

			/********************SHOW TOOLTIP MENU******************/
			$timeout(function () { // Show Tooltip
				$('[data-toggle="tooltip"]').tooltip();
			}, 1000);

		}
	});
})();


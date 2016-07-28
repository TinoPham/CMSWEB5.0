(function () {
    'use strict';

    define(['cms', 
        'rebar/adhoc/edit', 
        'rebar/adhoc/editfolder', 
        'rebar/adhoc/viewreport', 
        'rebar/adhoc/delete', 
        'rebar/adhoc/filtersetting',
        'Scripts/Services/exportSvc',
        'DataServices/Rebar/adhoc.service'], function (cms) {
        cms.register.controller('adhocdetailsCtrl', adhocdetailsCtrl);

        adhocdetailsCtrl.$inject = ['$rootScope', '$scope', '$stateParams', '$filter', '$timeout', '$modal', 'cmsBase', 'bamhelperSvc', 'AppDefine', 'adhocDataSvc', '$state', 'chartSvc', 'exportSvc'];

        function adhocdetailsCtrl($rootScope, $scope, $stateParams, $filter, $timeout, $modal, cmsBase, bamhelperSvc, AppDefine, adhocDataSvc, $state, chartSvc, exportSvc) {

            var vm = this;
            vm.sitesKeys = [];
            vm.groupSite = false;
            vm.reload = null;
            vm.treeDef = {
                Id: 'ID',
                Name: 'Name',
                Type: 'Type',
                Checked: 'Checked',
                Childs: 'Sites',
                Count: 'SiteCount',
                Model: {}
            };
            vm.treeOptions = {
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
                Item: {
                    IsShowItemMenu: false
                },
                Type: {
                    Folder: 0,
                    Group: 2,
                    File: 1
                }
            };

            vm.items = null;
            vm.params = {
                reportId: $stateParams.id
            }

            active();

            function active(treeReload) {
                adhocDataSvc.getAdhocReportById({ reportId: $stateParams.id }, function (result) {
                    vm.rpt = result;
                    $rootScope.title = vm.rpt.ReportName;
                    $rootScope.ListReportAdhocHistory.push(vm.rpt.ReportID);
                    var isLoadtrue = treeReload === false ? false : true;
                    loadData(isLoadtrue);
                }, function(error) {});
            }

            function loadData(bInit) {
                if ($scope.$parent && $scope.$parent.$parent && $scope.$parent.$parent.treeSiteFilter && $scope.$parent.$parent.sitepacInfo) {
                    vm.reload = null;
                    $scope.data = $scope.$parent.$parent.treeSiteFilter;
                    vm.rebarTree = $scope.data;
                    if (bInit) {
                    	bamhelperSvc.checkallNode(vm.rebarTree);
                    }
                    var checkedIDs = [];
                    chartSvc.GetSiteSelectedIDs(checkedIDs, vm.rebarTree.Sites);
                    vm.selectedSites = checkedIDs;
                    console.log(vm.selectedSites);
                    runReport();
                } else {
                    return;
                }
            }


            vm.showEditAhocDialog = showEditAhocDialog;
            function showEditAhocDialog(report, isNew) {
                console.log(report);
           

                if (!$scope.modalShown) {
                    $scope.modalShown = true;
                    var userInstance = $modal.open({
                        templateUrl: 'rebar/adhoc/edit.html',
                        controller: 'editadhocCtrl as vm',
                        size: 'md',
                        backdrop: 'static',
                        backdropClass: 'modal-backdrop',
                        keyboard: false,
                        resolve: {
                            items: function () {
                                return { report: report, id: report.ReportID, isNew: isNew };
                            }
                        }
                    });

                    userInstance.result.then(function (data) {
                        $scope.modalShown = false;
                        if (data === "close") {
                            return;
                        }
                        else {
                        	if (data != null) {
                        		$scope.$parent.ReLoadMenu(false);
                        		active(false);
                            }
                            return;
                        }
                    });
                }
            }

            vm.deleteItem = function (report) {
                if (!$scope.modalShown) {
                    $scope.modalShown = true;
                    var reportDiag = $modal.open({
                        templateUrl: 'rebar/adhoc/delete.html',
                        controller: 'deletereportCtrl as vm',
                        size: 'sm',
                        backdrop: 'static',
                        backdropClass: 'modal-backdrop',
                        keyboard: false,
                        resolve: {
                            items: function () {
                                return { Id: report.ReportID, Name: report.ReportName };
                            }
                        }
                    });

                    reportDiag.result.then(function (data) {
                        $scope.modalShown = false;
                        if (data) {
                            if (data === "close") {
                                return;
                            }
                            if (report.IsFolder === true) {
                                adhocDataSvc.deleteAdhocReportFolder({ folderId: report.ReportID }, function () {
                                    //loadData();
                                    //$state.go('rebar.adhoc', { id: null });
                                    $scope.$parent.ReLoadMenu(false);
                                }, function (error) {
                                    var msg = cmsBase.translateSvc.getTranslate(error.data.Data.ReturnMessage[0]);
                                    cmsBase.cmsLog.error(msg);
                                });
                            } else {
                                adhocDataSvc.deleteAdhocReport({ reportId: report.ReportID }, function () {
                                    $rootScope.ListReportAdhocHistory = Enumerable.From($rootScope.ListReportAdhocHistory).Where(function (x) { return x !== parseInt(report.ReportID) }).ToArray();
                                    if ($state) {
                                        if ($state.current.name == AppDefine.State.REBAR_ADHOCDETAILS
                                        && $stateParams.id == report.ReportID) {
                                            if ($rootScope.ListReportAdhocHistory.length > 0) {
                                                var currentID = parseInt($rootScope.ListReportAdhocHistory[$rootScope.ListReportAdhocHistory.length - 1]);
                                                $rootScope.ListReportAdhocHistory.length = $rootScope.ListReportAdhocHistory.length - 1;
                                                $state.go(AppDefine.State.REBAR_ADHOCDETAILS, { id: currentID });
                                            } else {
                                                $state.go('rebar');
                                            }
                                        }
                                    }
                                    //$state.go(AppDefine.State.REBAR_DASHBOARD);
                                    $scope.$parent.ReLoadMenu(false);
                                }, function (error) {
                                    var msg = cmsBase.translateSvc.getTranslate(error.data.Data.ReturnMessage[0]);
                                    cmsBase.cmsLog.error(msg);
                                });
                            }
                            //$state.go('rebar');
                        }
                    });


                }
            }




            vm.groupBySite = function() {
                vm.groupSite = !vm.groupSite;
                $scope.$broadcast('changeView', { groupSite: vm.groupSite });
            }

            function runReport() {
                vm.reload = null;
                vm.dateInfo = {
                    StartTranDate: $rootScope.rebarSearch.DateFrom,
                    EndTranDate: $rootScope.rebarSearch.DateTo
                }

                vm.items = { reportConfig: vm.rpt, reportInfo: { Id: vm.params.reportId }, pacInfo: $scope.$parent.$parent.sitepacInfo, dateInfo: vm.dateInfo, SiteKeys: vm.selectedSites, groupSite: vm.groupSite }
                $timeout(function() {
                    vm.reload = true;
                }, 0);
                
            }

            $scope.$on(AppDefine.Events.REBARSEARCH, function () {

                //if (!vm.rebarTree) return;
                vm.siteloaded = true;

                vm.reload = null;
                
                //runReport();
                //loadDataByEmployer(1);
                //loadDataForPaymentChart();
                active(false);
            });


            $scope.$on(AppDefine.Events.PAGEREADY, function () {
                loadData(true);
            });

            $scope.$on(AppDefine.Events.PAGEEDITED, function () {
                active(false);
            });

      
            $scope.clickOutside = function ($event, element) {
                // 2015-05-25 Tri fix bug 3289
                // update state tree when without click Done.
                bamhelperSvc.setNodeSelected(vm.rebarTree, vm.selectedSites);
                $scope.$broadcast('cmsTreeRefresh', vm.rebarTree);

                if (angular.element(element).hasClass('open')) {
                    angular.element(element).removeClass('open');
                } 
            };

            vm.TreeSiteClose = function () {
                var checkedIDs = [];
                chartSvc.GetSiteSelectedIDs(checkedIDs, vm.rebarTree.Sites);
                vm.selectedSites = checkedIDs;
                $rootScope.siteIDs = vm.selectedSites;

                if ($("#btn-popMenuConvSites").parent().hasClass("open")) {
                    $("#btn-popMenuConvSites").parent().removeClass("open");
                    $("#btn-popMenuConvSites").prop("aria-expanded", false);
                }

            }

            vm.callback = function (name) {
                vm.rpt = name;
                $rootScope.title = vm.rpt.ReportName;
                console.log(name);
            }

            vm.export = function() {
                var data = angular.element('.ahoc-report-view');

                if (!data[0]) {
                    return;
                }

                var stream = data[0].outerHTML;

                var streamEl = angular.element(stream);

                var datetimeexport = $filter('date')(Date.now(), "yyyyMMdd-HHMM");
                var sheetName = cmsBase.translateSvc.getTranslate('ADHOC') + '-' + datetimeexport.toString();
                streamEl.find('nav').empty();
                streamEl.find('.custom-panel-heading').empty();
                if (streamEl[0]) {
                    exportSvc.ToXls(streamEl[0].outerHTML, sheetName);
                }
            }
        }
    });
})();
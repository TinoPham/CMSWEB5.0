(function () {
    'use strict';

    define(['cms', 'rebar/adhoc/edit', 
        'rebar/adhoc/editfolder', 
        'rebar/adhoc/viewreport', 'rebar/adhoc/delete', 
        'rebar/adhoc/filtersetting'], function (cms) {
        cms.register.controller('adhocCtrl', adhocCtrl);

        adhocCtrl.$inject = ['$rootScope', '$scope', '$stateParams', '$filter', '$timeout', '$modal', 'cmsBase', 'bamhelperSvc', 'AppDefine', 'adhocDataSvc', '$state', 'chartSvc'];

        function adhocCtrl($rootScope, $scope, $stateParams, $filter, $timeout, $modal, cmsBase, bamhelperSvc, AppDefine, adhocDataSvc, $state, chartSvc) {

            var vm = this;
            vm.sitesKeys = [];
            vm.params = {
                reportId: $stateParams.id
            }

            active();

            function active() {

                if ($scope.$parent && $scope.$parent.$parent && $scope.$parent.$parent.treeSiteFilter) {

                    $scope.data = $scope.$parent.$parent.treeSiteFilter;
                    vm.rebarTree = $scope.data;
                    bamhelperSvc.checkallNode(vm.rebarTree);
                    var checkedIDs = [];
                    chartSvc.GetSiteSelectedIDs(checkedIDs, vm.rebarTree.Sites);
                    vm.selectedSites = checkedIDs;
                } else {
                    return;
                }

                adhocDataSvc.getAdhocs({id : $stateParams.id},function(data) {                   
                    vm.reports = data;
                    console.log(vm.reports );
                }, function(error) {
                });
                   
                
           
            }

            $scope.$on(AppDefine.Events.PAGEREADY, function () {
                active();
            });

            vm.showEditAhocDialog = showEditAhocDialog;
            function showEditAhocDialog(report, isNew) {

                if (report && report.IsFolder === true) {
                    vm.showEditFolderAhocDialog(report, isNew);
                    return;
                }

                var id = null;
                if (report) {
                    id = report.Id;
                }

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
                                return { report: report, id: id, isNew: isNew };
                            }
                        }
                    });

                    userInstance.result.then(function (data) {
                        $scope.modalShown = false;
                        if (data) {
                            active();
                        }
                    });
                }
            }

            vm.showEditFolderAhocDialog = showEditFolderAhocDialog;
            function showEditFolderAhocDialog(report, isNew) {

                if (vm.params.reportId && vm.params.reportId > 0) {
                    $state.go('rebar.adhoc', { id: null });
                    return;
                }

                if (!$scope.modalShown) {
                    $scope.modalShown = true;
                    var userInstance = $modal.open({
                        templateUrl: 'rebar/adhoc/editfolder.html',
                        controller: 'editfolderadhocCtrl as vm',
                        size: 'sm',
                        backdrop: 'static',
                        backdropClass: 'modal-backdrop',
                        keyboard: false,
                        resolve: {
                            items: function () {
                                return { report: report, id: vm.params.reportId, isNew: isNew };
                            }
                        }
                    });

                    userInstance.result.then(function (data) {
                        $scope.modalShown = false;
                        if (data) {
                            active();
                        }
                    });
                }
            }

            vm.viewReport = function (rp) {

                if (rp.IsFolder === true) {
                    $state.go(AppDefine.State.REBAR_ADHOC, { id: rp.Id });
                    return;
                }

                $state.go(AppDefine.State.REBAR_ADHOCDETAILS, { id: rp.Id });

                //vm.dateInfo = {
                //    StartTranDate: $rootScope.rebarSearch.DateFrom,
                //    EndTranDate: $rootScope.rebarSearch.DateTo
                //}

                //if (!$scope.modalShown) {
                //    $scope.modalShown = true;
                //    var userInstance = $modal.open({
                //        templateUrl: 'rebar/adhoc/viewreport.html',
                //        controller: 'viewreportCtrl as vm',
                //        size: 'lg',
                //        backdrop: 'static',
                //        backdropClass: 'modal-backdrop',
                //        keyboard: false,
                //        resolve: {
                //            items: function () {
                //                return { reportInfo: rp, pacInfo: $scope.$parent.$parent.sitepacInfo, dateInfo: vm.dateInfo, SiteKeys: vm.selectedSites }
                //            }
                //        }
                //    });

                //    userInstance.result.then(function (data) {
                //        $scope.modalShown = false;
                //    });
                //}
            }

            vm.dbclickItemRerport = function (rp) {
                if (rp.IsFolder === true) {
                    $state.go('rebar.adhoc', { id: rp.Id });
                } else {
                    $state.go('rebar.adhocdetails', { id: rp.Id });
                }
            }

            vm.clickItemRerport = function(rp) {
                    vm.selectReport = rp;
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
                                return report;
                            }
                        }
                    });

                    reportDiag.result.then(function (data) {
                        $scope.modalShown = false;
                        if (data) {
                            if (report.IsFolder === true) {
                                adhocDataSvc.deleteAdhocReportFolder({ folderId: report.Id }, function () {
                                    active();
                                }, function (error) {
                                    var msg = cmsBase.translateSvc.getTranslate(error.data.Data.ReturnMessage[0]);
                                    cmsBase.cmsLog.error(msg);
                                });
                            } else {
                                adhocDataSvc.deleteAdhocReport({ reportId: report.Id }, function () {
                                    active();
                                }, function (error) {
                                    var msg = cmsBase.translateSvc.getTranslate(error.data.Data.ReturnMessage[0]);
                                    cmsBase.cmsLog.error(msg);
                                });
                            }
                        }
                    });
                }
            }

        }
    });
})();
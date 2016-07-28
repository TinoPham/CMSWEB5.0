//var LogsController = angular.module('ConfigApp');
//LogsController.controller('LogsController', ['$rootScope','$scope', '$filter', 'AppUtils.Dialogs', 'ApiService','UtilService',
define(['require', 'App', 'jqdatepicker', 'nggrid', 'ng.scrollabletable', 'ng.gridflexibleheight'], function (require, app) {
    var injectParams = ['$rootScope','$scope', '$filter', 'Dialogs', 'ApiService','Utils'];
    var ctrl = function ($rootScope, $scope, $filter, Dialogs, ApiService, Utils) {

        //$scope.sdate = new Date();

        var cellPrgramSetTemplate = '<div class="ngCellText" ng-class="col.colIndex()"><span  ng-cell-text title="{{ ProgramsetText( row.getProperty(col.field))}}" >{{ ProgramsetText( row.getProperty(col.field))}}</span></div>';
        var cellActionTemplate = '<div class="ngCellText" style="text-overflow: initial;" ng-class="col.colIndex()"><a id="btnDelete" ng-click="DeleteRow(row.entity)" title="{{resx.str_Delete}}" >{{resx.str_Delete}}</a> </div>';
        var headerRowTemplate = "/Api/shared/ng_grid/headerRowTemplate.html"
        var header_template = '/Api/shared/ng_grid/HeaderTemplate.html';
        var header_template_delete = '/Api/shared/ng_grid/HeaderTemplateDelete.html';
        var Datetime_template = '/Api/shared/ng_grid/CellDateTime.html';
        var footerTemplate = '/Api/shared/ng_grid/footerTemplate.html';
        var rowTemplate = '/Api/shared/ng_grid/rowTemplate.html';
        var cellTextTemplate = '/Api/shared/ng_grid/CellTextTemplate.html';
        var columnDefs = [
                            { field: "ID", displayName: "Log ID", pinnable: false, resizable: false, sortable: true, visible:false, width: '50px', headerCellTemplate: header_template }
                          , { field: "LogID", displayName: "LogID", pinnable: false, resizable: false, sortable: true, visible: false, width: '50px', headerCellTemplate: header_template }
                          , { field: "Message", displayName: "Message", pinnable: false, resizable: false, sortable: true, width: '*', headerCellTemplate: header_template, cellTemplate: cellTextTemplate }
                          , { field: "MessageData", displayName: "Message Data", pinnable: false, resizable: false, sortable: false, visible: false, width: '50px', headerCellTemplate: header_template }
                          , {
                              field: "ProgramSet", displayName: "Program-set", pinnable: false, resizable: false, sortable: true, width: '95px', headerCellTemplate: header_template
                              , cellTemplate: cellPrgramSetTemplate
                          }
                          , {
                              field: "DVRDate", displayName: "DVRDate", pinnable: false, resizable: false, sortable: true, width: '130px', cellFilter: 'date:\'short\''
                              , cellTemplate: Datetime_template, headerCellTemplate: header_template
                          }
                          , {
                              field: "", displayName: "", pinnable: false, resizable: false, sortable: false, width: '70px'
                              , cellTemplate: cellActionTemplate, headerCellTemplate: header_template_delete
                          }
        ];

        $scope.resx = { str_DeleteAll: 'Delete All', str_Delete: 'Delete' };
        $scope.data = { Logs: [], totalServerItems: 0, Programset:[] };
        $scope.data.Programset = [{ ID: 0, Name: "All" },{ ID: 1, Name: "DVR" }, { ID: 2, Name: "POS" }, { ID: 3, Name: "IOPC" }, { ID: 4, Name: "CA" }, { ID: 5, Name: "ATM" }, { ID: 6, Name: "LPR" }];
        // filter
        $scope.filterOptions = {
            filterText: ""
            , useExternalFilter: true
            , Programset: $scope.data.Programset[0]
            , DVRDate: new Date()
        };
        // paging
    
        $scope.pagingOptions = {
            pageSizes: [25, 50, 100],
            pageSize: 25,
            currentPage: 1
        };

    
        // sort
        $scope.sortOptions = {
            fields: ["DVRDate"],
            directions: ["DESC"]
        };

        // grid
        $scope.gridOptions = {
            data: "data.Logs",
            columnDefs: columnDefs,
            enablePaging: true,
            enablePinning: true,
            enableSorting: true,
            pagingOptions: $scope.pagingOptions,
            filterOptions: $scope.filterOptions,
            keepLastSelected: false,
            multiSelect: false,
            showColumnMenu: false,
            showFilter: false,
            showGroupPanel: false,
            showFooter: true,
            sortInfo: $scope.sortOptions,
            totalServerItems: "totalServerItems",
            useExternalSorting: false,
            i18n: "en",
            jqueryUITheme: true
            , footerTemplate: footerTemplate
            , plugins: [new ngGridFlexibleHeightPlugin()]
            , headerRowTemplate: headerRowTemplate
            ,rowTemplate: rowTemplate
        };

        $scope.DeleteRow = function (row)
        {
            DeleteLog(row);
        }

        $scope.ProgramsetText = function (data) {
            if($scope.data.Programset == null || $scope.data.Programset == undefined || $scope.data.Programset.length == 0)
                return "";

            if (data == null || data == undefined)
                $scope.data.Programset[0].Name;
            if (data >= 0 && data < $scope.data.Programset.length)
                return $scope.data.Programset[data].Name;
            return "";
        }

        $scope.refresh = function () {
            setTimeout(function () {
                var p = {
                    Date: $filter('date')($scope.filterOptions.DVRDate, "yyyyMMdd")
                    , Programset: $scope.filterOptions.Programset.ID > 0?  $scope.filterOptions.Programset.ID : null
                    , pageSize: $scope.pagingOptions.pageSize
                    , pageNumber: $scope.pagingOptions.currentPage
                    , sortFields: $scope.sortOptions.fields
                    , sortDirections: $scope.sortOptions.directions
                };
                ApiService.Logs(p)
                .then(
                function (data){
                    $scope.data.Logs = data.Logs;
                    $scope.totalServerItems = data.TotalItem;
                }
                , function(data){
                    Dialogs.error(data);
                }
                , function (data) {
                    Dialogs.error(data);
                }
                );

            }, 100);
        };
        function DeleteLog(data)
        {
            ApiService.DeleteLog(data)
            .then(
                function (data) {
                    angular.forEach($scope.data.Logs, function (obj, index) {

                        if (obj.ID === data.ID) {

                            $scope.data.Logs.splice(index, 1);
                            if ($scope.totalServerItems > 0)
                                $scope.totalServerItems--;
                               
                            return;
                        };
                    });
                }
                , function (data){

                }
            );
        }
        $scope.DeleteLogs = function ()
        {
            var filter = {
                Date: Utils.toUTCDate( $scope.filterOptions.DVRDate)
                   , Programset: $scope.filterOptions.Programset.ID > 0 ? $scope.filterOptions.Programset.ID : null
                   , pageSize: $scope.pagingOptions.pageSize
                   , pageNumber: $scope.pagingOptions.currentPage
                   , sortFields: $scope.sortOptions.fields
                   , sortDirections: $scope.sortOptions.directions
            };
            ApiService.DeleteLogs(filter)
             .then(
                function (data) {
                    $scope.data.Logs = data.Logs;
                    $scope.totalServerItems = data.TotalItem;
                }
                , function (data) {
                    Dialogs.error(data);
                }
                , function (data) {
                    Dialogs.error(data);
                }
                );
        }
        // watches
        $scope.$watch('pagingOptions', function (newVal, oldVal) {
            if (newVal !== oldVal) {
                $scope.refresh();
            }
        }, true);

        $scope.$watch('filterOptions', function (newVal, oldVal) {
            if (newVal !== oldVal) {
                $scope.refresh();
            }
        }, true);

        //$scope.$watch('sortOptions', function (newVal, oldVal) {
        //    if (newVal !== oldVal) {
        //        $scope.refresh();
        //    }
        //}, true);

        $scope.refresh();

    }
    ctrl.$inject = injectParams;
    app.register.controller("LogsController", ctrl);
}
);

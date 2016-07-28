(function () {
    'use strict';

    define(['cms', 'Scripts/Directives/treeview'], function (cms, CMSTree) {
        cms.register.directive('treeComponent', treeComponent);

        treeComponent.$inject = ['$timeout', 'siteadminService'];
        function treeComponent($timeout, siteadminService) {
            var cmsTreeRefresh = 'cmsTreeRefresh';
            var cmsTreeRefreshTree = 'cmsTreeRefreshTree';
            var cmsTreeExpand = 'cmsTreeExpand';
            var cmsTreeCollapsed = 'cmsTreeCollapsed';
          
            return {
                restrict: 'EA',
                replace: false,
                scope: {
                    model: '=',
                    defCol: '=',
                    filterText: '=',
                    options: '=?options',
                    refreshNode: '='
                },
                link: function (scope, ele, attrs) {
                	var tree;
                    scope.defCol.Model = scope.model;
                    $timeout(function () {
                        tree = CMSTree.create(ele[0], scope.defCol, scope.options);
                    }, 10);

                    scope.$watch('model', function (newVal, old) {

                        if (scope.filterText !== undefined) {
                            if (tree && scope.defCol.Model) {
                                scope.defCol.Model = scope.model;
                                tree.filter(scope.filterText, scope.defCol.Model);
                            }
                            return;
                        };

                        if (tree && scope.defCol.Model && old) {
                            $timeout(function () {
                                scope.defCol.Model = scope.model;
                                tree.refesh(scope.defCol.Model);
                            }, 0);
                        }
                    });

                    var filterTextTimeout;
                    scope.$watch('filterText', function (newVal, old) {
                        if (!scope.defCol.Model) return;

                        if (filterTextTimeout) $timeout.cancel(filterTextTimeout);
                        filterTextTimeout = $timeout(function () {
                            if (tree && scope.defCol.Model && newVal !== old) {
                                scope.defCol.Model = scope.model;
                                checkallNode(scope.defCol.Model);
                                siteadminService.filterSites(scope.defCol.Model, newVal);
                                tree.filter(scope.filterText, scope.defCol.Model); 
                            }
                        }, 500);
                    });


                    function checkallNode(node) {
                        var nodeLen = node.Sites.length;
                        node.Checked =false;
                        for (var i = 0; i < nodeLen; i++) {
                            checkallNode(node.Sites[i]);
                        }
                    }
                    //scope.$watch('filterText', function (newVal, old) {
                    //    if (tree && scope.defCol.Model && newVal !== old) {
                    //        scope.defCol.Model = scope.model;
                    //        tree.filter(scope.filterText, scope.defCol.Model);
                    //    }
                    //});

                    scope.$on(cmsTreeExpand, function () {
                        tree.expandAll();
                    });

                    scope.$on(cmsTreeCollapsed, function () {
                        tree.collapsedAll();
                    });


                    scope.$on(cmsTreeRefreshTree, function (e) {
                        $timeout(function () {
                            tree = CMSTree.create(ele[0], scope.defCol, scope.options);
                        }, 10);;
                    });

                    scope.$on(cmsTreeRefresh, function (e, data) {
                        //e.stopPropagation();
                        if (data) {
                            tree.refesh(scope.defCol.Model, data);
                        }

                        tree.refesh(scope.defCol.Model);
                    });

                    scope.$on('$destroy', function () {
                        //tree.destroy();
                        tree = null;
                    });
                }
            };
        }
    });
})();
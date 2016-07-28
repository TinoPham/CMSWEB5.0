(function () {
    'use strict';

    define(['cms', 'Scripts/Directives/treeview'], function (cms, CMSTree) {
        cms.register.directive('metricComponent', metricComponent);

        metricComponent.$inject = ['$timeout'];
        function metricComponent($timeout) {
            var metricRefresh = 'metricRefresh';
            var metricExpand = 'metricExpand';
            var metricCollapsed = 'metricCollapsed';
            var metrictree;
            return {
                restrict: 'EA',
                replace: false,
                scope: {
                    model: '=',
                    defCol: '=',
                    filterText: '=',
					select: '='
                },
                link: function (scope, ele, attrs) {
                    var options = {
                        Node: {
                            IsShowCheckBox: true,
                            IsShowRootNode: false,
                            IsShowNodeMenu: false,
                            IsDraggable: false
                        },
                        Item: {
                            IsShowItemMenu: false,
                            IsRadio: true
                },
                        Type: {
                            Folder: 0,
                            Group: 2,
                            File: 1
                        },
                        CallBack: {
                        	SelectedFn: scope.select ? scope.select : null
                        },
                    }

                    $timeout(function () {
                        scope.defCol.Model = scope.model;
                        metrictree = CMSTree.create(ele[0], scope.defCol, options);
                    }, 10);

                    scope.$watch('model', function (newVal, old) {
                        if (metrictree && scope.defCol.Model && old) {
                            $timeout(function () {
                                scope.defCol.Model = scope.model;
                                metrictree.refesh(scope.defCol.Model);
                            }, 0);
                        }
                    });

                    scope.$watch('filterText', function (newVal, old) {
                        if (metrictree && scope.defCol.Model && old) {
                            metrictree.filter(newVal);
                        }
                    });

                    scope.$watch('model', function () {
                        if (scope.defCol.Model) {
                            $timeout(function () {
                                metrictree = CMSTree.create(ele[0], scope.defCol, options);
                            }, 0);
                        }
                    });

                    scope.$watch('filterText', function (newVal, old) {
                        if (scope.defCol.Model && old) {
                            metrictree.filter(newVal);
                        }
                    });

                    scope.$on(metricExpand, function () {
                        metrictree.expandAll();
                    });

                    scope.$on(metricCollapsed, function () {
                        metrictree.collapsedAll();
                    });

                    scope.$on(metricRefresh, function (e) {
                        e.stopPropagation();
                        metrictree.refresh();
                    });

                    scope.$on('$destroy', function () {
                        //metrictree.destroy();
                        metrictree = null;
                    });
                }
            };
        }
    });
})();
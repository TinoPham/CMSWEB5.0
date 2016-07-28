(function () {
    'use strict';

    define( ['cms', 'configuration/sites/helpers'], function ( cms ) {
        cms.register.directive('cmsTreeView', cmsTreeView);

        cmsTreeView.$inject = ['cmsBase', 'AppDefine'];
        function cmsTreeView(cmsBase, cmsConstant) {
            return {
                restrict: 'AE',
                scope: {
                    data: '=',
                    siteOption: '=',
                    searchTree: '='
                },
                templateUrl: 'Widgets/CmsTreeView.html',
                controller: ['$scope',cmsTreeViewCtrl]
                };

        function cmsTreeViewCtrl($scope) {

                //function rebuildtree() {
                //    var listtemp;
                //    angular.forEach($scope.data, function (nodes) {
                //        listtemp = refeshlist(nodes);
                //    });
                //    $scope.data[0] = listtemp;
        	//}
        	var manualcheck = false;

        	$scope.$watch('data', function () {
        		if (manualcheck === false && $scope.data) {
        			angular.forEach($scope.data, function (nodes) {
        				checkinglist(nodes);
        			});
        		}
        	});

        	$scope.checklist = function (node) {
        		manualcheck = true;

                    var valCheck = nodechecked(node.Checked);
                    checknode(node, valCheck);

                    angular.forEach($scope.data, function (nodes) {
                        checkinglist(nodes);
                    });

                }

                //function refeshlist(nodes) {
                //    var sitecount = 0;
                //    if (nodes.Sites && nodes.Sites.length > 0) {
                //        angular.forEach(nodes.Sites, function (node) {
                //            if (node.Type === 1) {
                //                sitecount = sitecount + 1;
                //            } else {
                //                var tempnode = refeshlist(node);
                //                sitecount = sitecount + tempnode.SiteCount;
                //            }
                //        });
                //        nodes.SiteCount = sitecount;
                //    } else {
                //        nodes.SiteCount = 0;
                //    }
                //    return nodes;
                //}

                function checkinglist(node) {

                    var result = cmsConstant.treeStatus.Uncheck;
                    var intermid = false;
                    var interval = cmsConstant.treeStatus.Uncheck;
                    var i = 0;
                    if (node.Sites && node.Sites.length > 0) {
                        angular.forEach(node.Sites, function (n) {
                            var ch = n.Checked;
                            if (n.Sites && n.Sites.length > 0) {
                                ch = checkinglist(n);
                            }

                            if (i === 0) {
                                interval = ch;
                            }

                            if (i > 0 && ch !== interval) {
                                intermid = true;
                            }

                            if (intermid === true && i > 0) {
                                ch = cmsConstant.treeStatus.Indeterm;
                            }

                            result = ch;
                            i++;
                        });
                    } else {
                        result = node.Checked;
                    }
                    node.Checked = result;
                    return result;
                }

                function checknode(node, value) {
                    if (node.Sites && node.Sites.length > 0) {
                        angular.forEach(node.Sites, function (n) {
                            checknode(n, value);
                        });
                    }
                    node.Checked = value;

                }

                function nodechecked(status) {
                    var result = cmsConstant.treeStatus.Uncheck;
                    if (status == cmsConstant.treeStatus.Uncheck || status == cmsConstant.treeStatus.Indeterm) {
                        result = cmsConstant.treeStatus.Uncheck;
                    } else {
                        result = cmsConstant.treeStatus.Checked;
                    }
                    return result;
                }
            }
        }
    });
})();
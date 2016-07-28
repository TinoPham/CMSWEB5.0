( function () {
    'use strict';

    define( ['cms', 'DataServices/SiteSvc'], function ( cms ) {

        cms.register.controller( 'TreeSiteCtrl', TreeSiteCtrl );

        TreeSiteCtrl.$inject = ['$scope', '$location', 'router', 'SiteSvc', 'AccountSvc', 'AppDefine', '$rootScope', '$state'];

        function TreeSiteCtrl( $scope, $location, router, SiteSvc, AccountSvc, AppDefine, $rootScope, $state ) {
            $scope.CheckEnable = false;
            $scope.DragEnable = false;
            $scope.Sites = [];
            
            $scope.treeOptions = {
                accept: Accept,
                beforeDrag:  beforeDrag
            };

            $scope.GetSites = function () {
                if ( !AccountSvc.Sites || AccountSvc.Sites.length == 0)
                    SiteSvc.GetSites( Success );
                else
                    $scope.Sites = AccountSvc.Sites;
            };

            $scope.CheckClick = function ( scope ) {
                updateChildItem( scope );
                updateParentItem( scope );


            };

            
            
            function Success( response ) {
                $scope.Sites = [response.Sites];
                AccountSvc.Sites = $scope.Sites;
            }

            function updateParentItem( scope ) {
                var parent = ScopeParent( scope );
                var parentData;
                var childs;
                var childData;
                while ( parent ) {
                    childs = ChildScopes( parent );
                    if ( childs.length ) {
                        var checkCount = 0;
                        for ( var i = 0; i < childs.length; i++ ) {
                            childData = ScopeData( childs[i] );
                            if ( childData.Checked === true )
                                checkCount++;
                        }

                        parentData = ScopeData( parent );
                        if ( checkCount === childs.length )
                            parentData.Checked = true;
                        else if ( checkCount > 0 )
                            parentData.Checked = null;
                        else
                            parentData.Checked = false;
                    }
                    parent = ScopeParent( parent );
                }

            }

            function updateChildItem( scope ) {
                var scopeData = ScopeData( scope );
                var childs = ChildScopes( scope );

                if ( childs && childs.length > 0 ) {
                    var childdata;
                    for ( var i = 0; i < childs.length; i++ ) {
                        childdata = ScopeData( childs[i] );
                        childdata.Checked = scopeData.Checked;
                        updateChildItem( childs[i] );
                    }
                }
            }

            function ChildScopes( scope ) {
                return scope.childNodes();
            }

            function ScopeParent( scope ) {

                return scope.$parentNodeScope;
            }

            //get node data value
            function ScopeData( scope ) {
                return scope.$modelValue;
            }

            //check node is site or region
            function isRegion( node ) {
                return node === null ? false : node.Type === 0;
            }

            function beforeDrag( sourceNodeScope ) {
                var data = ScopeData( sourceNodeScope );
                if ( data.ParentKey === null )
                    return false;
                return true;
            }
            function Accept( sourceNodeScope, destNodesScope, destIndex ) {
                return true;
                 
            }
        }
    } );
} )();
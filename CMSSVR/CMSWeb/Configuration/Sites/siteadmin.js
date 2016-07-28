(function() {
	'use strict';
	define([
			'cms',
			'Scripts/Directives/treeComponent',
			'configuration/sites/add_site',
			'configuration/sites/edit',
			'configuration/sites/helpers',
			'configuration/sites/delete',
			'configuration/sites/edit_add_region'
		],
		function (cms) {
			cms.register.controller('sitesCtrl', sitesCtrl);
			sitesCtrl.$inject = ['$scope', '$modal', 'cmsBase', 'dataContext', 'AppDefine', '$timeout', 'siteadminService', 'uiMaskConfig', 'AccountSvc'];
			function sitesCtrl($scope, $modal, cmsBase, dataContext, AppDefine, $timeout, siteadminService, uiMaskConfig, AccountSvc) {
				uiMaskConfig.clearOnBlur = false;
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
						IsShowNodeMenu: true,
						IsShowAddNodeButton: false,
						IsShowAddItemButton: false,
						IsShowEditButton: true,
						IsShowDelButton: true,
						IsDraggable: true
					},
					Item: {
						IsAllowFilter: false,
						IsShowItemMenu: false
					},
					CallBack: {
						EditNode: callbackEdit,
						DelNode: callbackDelNode,
						SelectedFn: selectedFn,
						DblClickNode: doubleClickNode,
						SetIconFolder: setIconFolder,
						SetIconGroup: setIconGroup,
						DragOver: dragOver,
						DragEnd: dragEnd
					},
					Type: {
						Folder: 0,
						Group: 1,
						File: 2

					}
				}

				$scope.nodeType = {
					Region: AppDefine.NodeType.Region,
					Site: AppDefine.NodeType.Site,
					DVR: AppDefine.NodeType.DVR,
					Channel: AppDefine.NodeType.Channel
				}
				var reactScope;
				$scope.showAllSite = true;
				$scope.editDisabled = true;
				$scope.deleteDisabled = true;
        		$scope.addRegionDisabled = true;
        		$scope.addSiteDisabled = true;
				$scope.userLogin = AccountSvc.UserModel();
            
                active();

                $scope.expandAll = function() {
                    $scope.$broadcast('cmsTreeExpand');
                }

                $scope.collapsedAll = function () {
                    $scope.$broadcast('cmsTreeCollapsed');
                }

                //$scope.stopsearch = false;
                //var filterTextTimeout;
                //$scope.treeSiteFilter = $scope.data;
                //$scope.$watch('filterText', function (val) {
				//	if ($scope.stopsearch) return;
				//	if (filterTextTimeout) $timeout.cancel(filterTextTimeout);
                //    filterTextTimeout = $timeout(function () {
                //        if ($scope.data && $scope.data.Sites.length > 0 && val) {
                //            $scope.treeSiteFilter = angular.copy($scope.data);
                //            $scope.treeSiteFilter.Sites = siteadminService.filterSites($scope.treeSiteFilter, val);
                //        } else {
                //            $scope.treeSiteFilter = $scope.data;
                //        }
                //    }, 500);
                //});

                $scope.title = function() {
                    return cmsBase.translateSvc.getTranslate('SITES');
                }

                $scope.showAllSites = function() {

                }

	            $scope.titlecount = function() {
                    return '(' + $scope.sitesCount($scope.treeSiteFilter) + ' ' + $scope.title() +')';

	            }

	            function showAllSites() {
	                dataContext.siteadmin.getAllUserSites(function (data) {
	                    $scope.data = data;
	                    if ($scope.data && $scope.data.Sites.length > 0 && $scope.filterText) {
	                        $scope.treeSiteFilter = angular.copy($scope.data);
	                        $scope.treeSiteFilter.Sites = siteadminService.filterSites($scope.treeSiteFilter, $scope.filterText);
	                    } else {
	                        $scope.treeSiteFilter = $scope.data;
	                    }

	                }, function (error) {
	                    siteadminService.ShowError(error);
	                });
	            }

	            function setIconFolder(node, scope) {
	                var icon =  {
	                    Expand: 'icon-map-pointer2',
	                    Collapsed: 'icon-pin56'
	                };

	                if (node.UserID !== $scope.userLogin.UserID) {
	                    icon.Expand = 'icon-map-pointer2 childuser';
	                    icon.Collapsed = 'icon-pin56 childuser';
	                }
	                return icon;
	            }

	            function setIconGroup(node, scope) {
	                var icon = {
	                    Expand: 'icon-store-3 choose',
	                    Collapsed: 'icon-store-3'
	                };

	                if (node.UserID !== $scope.userLogin.UserID) {
	                    icon.Expand += ' childuser';
	                    icon.Collapsed += ' childuser';
	                }
	                return icon;
	            }

				function doubleClickNode(node, scope) {
					editnode(node, scope.props.parentNode);
				}

                function dragEnd(e, dragObj, dropObj, from, to, isSameScope) {
                    if (isSameScope === true) return;

                    var dragobj = dragObj.Sites[from];
                    if (dropObj.ParentKey === null && dragobj.Type !== AppDefine.NodeType.Region) {
                        RefreshTreeSiteData();
                        return;
                    }
                    if (dragobj.Type !== AppDefine.NodeType.DVR) {
                        dragobj.ParentKey = dropObj.ID;
                        dataContext.siteadmin.saveTreeNode(dragobj, function(data) {
        					var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.EDIT_SUCCESS_MSG);
                            cmsBase.cmsLog.success(msg);
        				},
						function (error) {
                            $scope.refreshTree();
        					var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.EDIT_FAIL_MSG);
        					cmsBase.cmsLog.warning(msg);
                        });
                    } 
                }

				function dragOver(e, scope, dragged, eventElm) {
					//Anh, don't check privilege
					//if (dragged.UserID !== $scope.userLogin.UserID) {
					//	return false;
					//}

					switch (dragged.Type) {
						case AppDefine.NodeType.Region:
							{
								//Anh, don't check privilege
								//if (scope.props.node.UserID !== dragged.UserID) {
								//	return false;
								//}

								if (scope.props.node.Type === AppDefine.NodeType.Region) {

									if (dragged.Sites.length > 0 && scope.props.node.ParentKey) {
										var hasRegion = false, leng = dragged.Sites.length;
										for (var i = 0; i < leng; i++) {
											if (dragged.Sites[i].Type === 0) {
												hasRegion = true;
												break;
											}
										}
										if (hasRegion === true) {
											return false;
										}
									}

									if (scope.props.node.ID === $scope.treeSiteFilter.ID || scope.props.parentNode && scope.props.parentNode.ParentKey === null) {
										return true;
									} else {
										return false;
									}

								} else {
									return false;
								}
								return true;
							break;
						}
						case AppDefine.NodeType.Site:
							{
								if (scope.props.node.ParentKey === null) {
									return false;
								}

								//Anh, don't check privilege
								//if (scope.props.node.UserID !== dragged.UserID) {
								//	return false;
								//}

								if (scope.props.node.Type === AppDefine.NodeType.Region && scope.props.node.ParentKey !== null) {
									return true;
								} else {
									return false;
								}

								return true;
							break;
						}
						case AppDefine.NodeType.DVR:
						{
							return false;
							break;
						}
						default:
						{
							return false;
							break;
						}
					}
				}

                function callbackDelNode(node, scope) {

                    deleteNode(node);

                    scope.forceUpdate();
                    $scope.$apply();
                }

                function callbackEdit(node, scope) {

                    editnode(node, scope.props.parentNode);
                    
                    scope.forceUpdate();
                    $scope.$apply();
                }

                function editnode(node, parentNode) {
                    if (!$scope.selected) {
        				var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.NODE_SELECTED_EMPTY);
                        cmsBase.cmsLog.warning(msg);
                        return;
                    }

        			//Check permission
                    //if (($scope.userLogin.UserID != node.UserID) && (!$scope.userLogin.IsAdmin)) {
        			//	var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.DO_NOT_HAVE_PERMISSION_MSG);
        			//	cmsBase.cmsLog.warning(msg);
        			//	return;
        			//}

                    if (node.ID === 0 || node.Type === AppDefine.NodeType.Channel) {
        				var msg = "Unsupport Edit for this node."; //cmsBase.translateSvc.getTranslate(AppDefine.Resx.NODE_SELECTED_EMPTY);
                        cmsBase.cmsLog.warning(msg);
                        return;
                    }

                    if (node.Type === AppDefine.NodeType.Region) {
                        editaddRegion(false);

                    } else {
                        if (node.Type === AppDefine.NodeType.DVR) {
                            editaddSite(false,parentNode);
                        } else {
                            editaddSite(false);
                        }
                    }
                }

                function deleteNode(node) {
                    if (!$scope.selected) {
        				var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.NODE_SELECTED_EMPTY);
        				cmsBase.cmsLog.warning(msg);
        				return;
        			}

        			//IF NODE SELECTED IS ROOT
        			if ($scope.selected.ParentKey === null) {
        				var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.UNABLE_TO_DELETE_ROOT_TREE);
                        cmsBase.cmsLog.warning(msg);
                        return;
                    }

        			//NOT SUPPORT DELETE DVR FOR THIS VERSION
        			if ($scope.selected.Type === AppDefine.NodeType.DVR) {
        				return;
        			}

        			//Check permission
        			if ($scope.userLogin.UserID != node.UserID) {
        				var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.DO_NOT_HAVE_PERMISSION_MSG);
        				cmsBase.cmsLog.warning(msg);
        				return;
        			}


        			//Check has sub region
        			var subRegion = $.grep($scope.selected.Sites, function (site) {
        				return site.Type === $scope.nodeType.Region;
        			});
        			if (subRegion.length > 0) {
        				var msg = $scope.formatString(cmsBase.translateSvc.getTranslate(AppDefine.Resx.UNABLED_DELETE_REGION_EXIST_SUB_REGION), $scope.selected.Name, subRegion.length);
        				cmsBase.cmsLog.warning(msg);
        				return;
        			}

        			//Check contains site created by another users - only correct when Master remove node
        			var sites = $.grep($scope.selected.Sites, function (site) {
        				return site.Type === $scope.nodeType.Site;
        			});
        			if (sites.length > 0) {
        				var msg = $scope.formatString(cmsBase.translateSvc.getTranslate(AppDefine.Resx.UNABLED_DELETE_REGION_EXIST_SITE), $scope.selected.Name, sites.length);
        				cmsBase.cmsLog.warning(msg);
        				return;
        			}

                    if (node.ID === 0) {
        				var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.UNABLE_TO_DELETE_ROOT_TREE);
                        cmsBase.cmsLog.warning(msg);
                        return;
                    }

                    //if (node.Type == AppDefine.NodeType.Region && $scope.filterText && $scope.filterText !== '') {
        			//    var msg = "Cann't Delete the Region when you are filtering."; //cmsBase.translateSvc.getTranslate(AppDefine.Resx.NODE_SELECTED_EMPTY);
                    //    cmsBase.cmsLog.warning(msg);
                    //    return;
                    //}

                    $scope.deleteNode(node);
                }

                $scope.editNodeFn = function () {
        			if ($scope.editDisabled) {
        				return; //ThangPham, do not show message when disabled button Edit - fix for IE 10, Sept 28 2015
        			}
                    editnode($scope.selected, $scope.parentSelected);
                }

                $scope.deleteNodeFn = function () {
        			if ($scope.deleteDisabled) {
        				return; //ThangPham, do not show message when disabled button Delete - fix for IE 10, Sept 28 2015
        			}
                    deleteNode($scope.selected);
                }

                $scope.addRegion = function () {
        			if (!$scope.selected) {
        				var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.NODE_SELECTED_EMPTY);
        				cmsBase.cmsLog.warning(msg);
        				return;
        			}

        			//Check Region level
        			if ($scope.selected.Type === $scope.nodeType.Region && $scope.selected.ParentKey != null && $scope.selected.ParentKey !== $scope.treeSiteFilter.ID) {
        				var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.UNABLED_ADD_REGION);
        				cmsBase.cmsLog.warning(msg);
        				return;
        			}

                    if ($scope.selected && $scope.selected.Type !== AppDefine.NodeType.DVR) {
                       editaddRegion(true);
                    } else {
        				var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.NODE_SELECTED_EMPTY);
                        cmsBase.cmsLog.warning(msg);
                    }
                }

                $scope.addSite = function () {
                    if (!$scope.selected) {
        				var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.NODE_SELECTED_EMPTY);
                        cmsBase.cmsLog.warning(msg);
                        return;
                    }

                    if ($scope.selected.ID === 0) {
        				var msg = "Unsupport Add for this node."; //cmsBase.translateSvc.getTranslate(AppDefine.Resx.NODE_SELECTED_EMPTY);
                        cmsBase.cmsLog.warning(msg);
                        return;
                    }

                    if ($scope.selected && $scope.selected.ParentKey === null) {
        				var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.UNABLE_TO_ADD_SITE_FOR_ROOT);
                        cmsBase.cmsLog.warning(msg);
                        return;
                    }

                    if ($scope.selected && $scope.selected.Type !== AppDefine.NodeType.DVR) {
                        editaddSite(true);
                    } else {
        				var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.NODE_SELECTED_EMPTY);
                        cmsBase.cmsLog.warning(msg);
                    }
                }

                function selectedFn(node, scope) {
                    $scope.selected = node;
                    $scope.parentSelected = scope.props.parentNode;
                    reactScope = scope;
        			$scope.$broadcast(AppDefine.Events.TREENODESELECTEDCHANGE, $scope.selected);
                    //cmsBase.cmsLog.info(node.Name);
                    $scope.$applyAsync();
                }

                $scope.refreshTree = function () {
                	$scope.filterText = '';
                    dataContext.siteadmin.refeshTreeCache();
                	//active();
                    RefreshTreeSiteData();
                }

                /***************Edit Add Region Start***********************/
                function editaddRegion(addnew) {
                    var node = $scope.selected;
                    if (!node) {
                        cmsBase.cmsLog.warning('Please Select a Item on the Tree');
                        return;
                    }

                    if (!$scope.modalShown) {
                        $scope.modalShown = true;
                        var userInstance = $modal.open({
                            templateUrl: 'configuration/sites/edit_add_region.html',
                            controller: 'editaddregionCtrl as vm',
                            size: 'sm',
                            backdrop: 'static',
                            keyboard: false,
                            resolve: {
                                items: function () {
                                    return node;
                                },
                                addNew: function () {
                                    return addnew;
                                }
                            }
                        });

                        userInstance.result.then(function (returndata) {
                            $scope.modalShown = false;

                            if (!returndata) {
                                return;
                            }

                            if (addnew === true) {
                                var model = {
                                    ID: returndata.RegionKey,
                                    Name: returndata.RegionName,
                                    MACAddress: 0,
                                    UserID: $scope.userLogin.UserID,
                                    ParentKey: returndata.RegionParentId,
                                    ImageSite: 0,
                                    PACData: 0,
                                    Sites: [],
                                    SiteCount: 0,
                                    Type: AppDefine.NodeType.Region,
                                    Checked: null
                                };

                                node.Sites.push(model);
                                $scope.selected = model;
                                $scope.parentSelected = node;
                                //reactScope.props.collapsed = true;
                                //reactScope.toogleExpand();
                                //reactScope.forceUpdate();
                                //reactScope.refreshNodeFn();
                            	//reactScope.selectedFn(model, reactScope);

                                RefreshTreeSiteData(); //AAAA
                                $scope.$broadcast('cmsTreeRefresh', $scope.selected);
                                $scope.$broadcast(AppDefine.Events.TREENODESELECTEDCHANGE, $scope.selected);
                              
                                
                            } else {
                                node.Name = returndata.RegionName;
                                $scope.selected = node;

                                RefreshTreeSiteData(); //AAAA
                                reactScope.forceUpdate();
                                reactScope.selectedFn(node, reactScope);
                            }
                        });
                    }
                }
                /***************Edit Add Region End***********************/

                /***************Edit Add Region Start***********************/

                function editaddSite(addnew, chooseSite) {

                    var node = chooseSite ? chooseSite : $scope.selected;

                    if (!node) {
                        cmsBase.cmsLog.warning('Please Select a Item on the Tree');
                        return;
                    }

                    if (!$scope.modalShown) {
                        $scope.modalShown = true;
                        var sitesInstance = $modal.open({
                            templateUrl: 'configuration/sites/edit.html',
                            controller: 'editSiteDiaCtrl as vm',
                            size: 'md',
                            backdrop: 'static',
                            keyboard: false,
                            resolve: {
                                items: function () {
                                    return node;
                                },
                                addNew: function () {
                                    return addnew;
                                }
                            }
                        });

                        sitesInstance.result.then(function (returndata) {
                            $scope.modalShown = false;

                            if (!returndata) {
                                return;
                            }

                            refreshSites(addnew, node, returndata, reactScope);

                        });
                    }
                }

                function refreshSites(addnew, node, site, reactscope) {
                    dataContext.siteadmin.siteById({ siteId: site.SiteKey }, function (data) {

                        if (node.Type === AppDefine.NodeType.Region) {
                            node.Sites.push(data);
                            $scope.selected = data;
                            $scope.parentSelected = node;
                            //reactScope.props.collapsed = true;
                            //reactScope.toogleExpand();
                            //reactScope.selectedFn(data, reactScope);
                        } else {
                            if (addnew === true) {
                                reactscope.props.parentNode.Sites.push(data);

                                $scope.selected = data;
                                $scope.parentSelected = node;
                                //reactScope.props.collapsed = true;
                                //reactScope.toogleExpand();
                                //reactScope.selectedFn(data, reactScope);
                            } else {
                                node.Name = data.Name;
                                node.Sites = data.Sites;
                            }
                        }

                        RefreshTreeSiteData(); //AAAA
                        reactScope.refreshNodeFn();
                        $scope.$broadcast('cmsTreeRefresh', $scope.selected);
                        
                    }, function (error) {
                        siteadminService.ShowError(error);
                    });
                }

                $scope.deleteNode = function (selected) {

                    if (!$scope.modalShown) {
                        $scope.modalShown = true;
        				var NodeDeleteInstance = $modal.open({
                            templateUrl: 'configuration/sites/delete.html',
                            controller: 'delSiteNodeCtrl as vm',
                            size: 'sm',
                            backdrop: 'static',
                            backdropClass: 'modal-backdrop',
                            keyboard: false,
                            resolve: {
                                items: function() {
                                    return selected;
                                }
                            }
                        });

        				NodeDeleteInstance.result.then(function (data) {
                            $scope.modalShown = false;
        					if (!data) { return; }
                            data.Type === AppDefine.NodeType.Site ? DeleteSite(data) : (data.Type === AppDefine.NodeType.Region ? DeleteRegion(data) : DeleteDVR(data.ID));
                        });
                    }

					function DeleteDVR(nodeId) {
						cmsBase.cmsLog.warning("Don't support Delete DVR now.");
						//dataContext.siteadmin.deleteSite(nodeId, function (data) {
						//    $scope.refreshTree();
						//    var msg = cmsBase.translateSvc.getTranslate(saveMsg);
						//    cmsBase.cmsLog.info(msg);
						//}, function (error) {
						//    cmsBase.cmsLog.info(error);
						//});
					}

					function DeleteSite(node) {
						dataContext.siteadmin.deleteSite(node.ID).then(
							function (data) {
								if (data.ReturnStatus) {
									var parrentNode = siteadminService.findNode($scope.treeSiteFilter, AppDefine.NodeType.Region, node.ParentKey);
									if (parrentNode) parrentNode.Sites.splice(parrentNode.Sites.indexOf(node), 1);
									//reactScope.refreshNodeFn();
									$scope.selected = parrentNode;
									//reactScope.selectedFn(parrentNode, reactScope);

									RefreshTreeSiteData(); //AAAA
									$scope.$broadcast('cmsTreeRefresh', parrentNode);
									$scope.$broadcast(AppDefine.Events.TREENODESELECTEDCHANGE, $scope.selected);
									$scope.parentSelected = siteadminService.findNode($scope.treeSiteFilter, $scope.selected.Type, $scope.selected.ParentKey);
									var msg = cmsBase.translateSvc.getTranslate(data.ReturnMessage[0]);
									cmsBase.cmsLog.success(msg);
								}
								else {
									angular.forEach(data.ReturnMessage, function (message) {
										var msg = cmsBase.translateSvc.getTranslate(message);
										cmsBase.cmsLog.warning(msg);
									});
								}
							},
							function (error) {
								var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.DELETE_FAIL_MSG);
								cmsBase.cmsLog.warning(msg);
						});
					}

					function DeleteRegion(node) {
						dataContext.siteadmin.deleteMultiRegion(node).then(
							function (data) {
								if (data.ReturnStatus) {
									//var indexscope = reactScope.props.parentNode.Sites.indexOf($scope.selected);
									var parrentNode = siteadminService.findNode($scope.treeSiteFilter, node.Type, node.ParentKey);
									if (parrentNode) parrentNode.Sites.splice(parrentNode.Sites.indexOf(node), 1);
									//if (indexscope === -1) {
										//parrentNode = siteadminService.findNode($scope.treeSiteFilter, node.Type, node.ParentKey);
										//if (parrentNode) parrentNode.Sites.splice(parrentNode.Sites.indexOf(node), 1);
									//}
									//else {
										//reactScope.props.parentNode.Sites.splice(indexscope, 1);
									//}
									//reactScope.refreshNodeFn();
									$scope.selected = parrentNode;

									RefreshTreeSiteData(); //AAAA
									$scope.$broadcast('cmsTreeRefresh', parrentNode);
									$scope.$broadcast(AppDefine.Events.TREENODESELECTEDCHANGE, $scope.selected);
									//reactScope.selectedFn(parrentNode, reactScope);
									$scope.parentSelected = siteadminService.findNode($scope.treeSiteFilter, $scope.selected.Type, $scope.selected.ParentKey);
									var msg = cmsBase.translateSvc.getTranslate(data.ReturnMessage[0]);
									cmsBase.cmsLog.success(msg);
								}
								else
								{
									angular.forEach(data.ReturnMessage, function (message) {
										var msg = cmsBase.translateSvc.getTranslate(message);
										cmsBase.cmsLog.warning(msg);
									});
								}
							},
							function (error) {
								var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.DELETE_FAIL_MSG);
								cmsBase.cmsLog.error(msg);
						});
					}

				}
				/***************Edit Add Region End***********************/
				function RefreshTreeSiteData() {
					if ($scope.userLogin.IsAdmin) {
						showAllSites();
					} else {
						dataContext.siteadmin.getSites(function (data) {
							if (!data || data.data && data.data.ReturnMessage && data.data.Data === null) {
								$scope.data = model;
								$scope.treeSiteFilter = $scope.data;

								return;
							}
							$scope.data = data;

							if ($scope.data && $scope.data.Sites.length > 0 && $scope.filterText) {
								$scope.treeSiteFilter = angular.copy($scope.data);
								$scope.treeSiteFilter.Sites = siteadminService.filterSites($scope.treeSiteFilter, $scope.filterText);
							} else {
								$scope.treeSiteFilter = $scope.data;
							}

						}, function (error) {
							siteadminService.ShowError(error);
						});
					}
				}

				$scope.sitesCount = function (sites) {
					return siteadminService.siteCountFn(sites);
				}

				function active() {
					dataContext.injectRepos(['configuration.siteadmin']).then(function () {
						RefreshTreeSiteData();
						/*
                        if ($scope.userLogin.IsAdmin) {
                            showAllSites();
                        } else {
                            dataContext.siteadmin.getSites(function (data) {
                                if (!data || data.data && data.data.ReturnMessage && data.data.Data === null) {
                                    $scope.data = model;
                                    $scope.treeSiteFilter = $scope.data;

                                    return;
                                }
                                $scope.data = data;

                                if ($scope.data && $scope.data.Sites.length > 0 && $scope.filterText) {
                                    $scope.treeSiteFilter = angular.copy($scope.data);
                                    $scope.treeSiteFilter.Sites = siteadminService.filterSites($scope.treeSiteFilter, $scope.filterText);
                                } else {
                                    $scope.treeSiteFilter = $scope.data;
                                }

                            }, function (error) {
                                siteadminService.ShowError(error);
                            });
                        }*/
					});
				}

				$scope.ready = false;

				$scope.$on(AppDefine.Events.TREENODESELECTEDCHANGE, function (event, arg) {
					if (arg) {
						setHeaderFunc($scope.userLogin, arg);
					}
				});

				function setHeaderFunc(userLogin, nodeSelected) {
					if (!userLogin || !nodeSelected) { return; }
					$scope.editDisabled = false;//(userLogin.UserID != nodeSelected.UserID) && !userLogin.IsAdmin;
					$scope.deleteDisabled = false;//(userLogin.UserID != nodeSelected.UserID) && !userLogin.IsAdmin;
					if (nodeSelected.ParentKey === null) {
						$scope.deleteDisabled = true;
					}

        			$scope.addRegionDisabled = !$scope.selected || $scope.selected.Type === $scope.nodeType.DVR
						|| ($scope.selected.ParentKey !== null && $scope.selected.ParentKey !== $scope.treeSiteFilter.ID)
						|| ($scope.selected.ParentKey !== null && $scope.selected.UserID !== $scope.userLogin.UserID);
        			$scope.addSiteDisabled = !$scope.selected || $scope.selected.ParentKey === null
						|| $scope.selected.Type === $scope.nodeType.DVR
						|| $scope.selected.UserID !== $scope.userLogin.UserID;
				}

				$scope.formatString = function (format) {
					var args = Array.prototype.slice.call(arguments, 1);
					return format.replace(/{(\d+)}/g, function (match, number) {
						return typeof args[number] != 'undefined' ? args[number] : match;
					});
				};
			}
		});
})();
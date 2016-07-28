(function() {
        define(['cms'], function(cms) {
            cms.register.service('bamhelperSvc', bamhelperSvc);
            bamhelperSvc.$inject = ['AppDefine'];

            function bamhelperSvc(AppDefine) {

                var breakFirstSite = null;
                var bamservice = {
                    getSitesFromNode: getSites,
                    checkallNode: checkallNode,
                    getSitesFromUser: getAllSites,
				    getFirstSites: getFirstSites,
				    setNodeSelected: setNodeSelected,
				    getFirstDVR: getFirstDVR,
				    getSitesFromDVR: getSitesFromDVR,
				    setNodeSelectedbyDDVR: setNodeSelectedbyDDVR,
				    getDvrsFromNode: getDVRS,
				    getFirstDVRNoVirtual: getFirstDVRNoVirtual
                };

                return bamservice;

                function getDVRS(node, DVRList) {
                    if (node && node.Type !== AppDefine.NodeType.DVR && node.Sites.length) {
                        var nodeLen = node.Sites.length;
                        for (var i = 0; i < nodeLen; i++) {
                            if (node.Sites[i].Type === AppDefine.NodeType.DVR) {
                                var n = node.Sites[i];
                                if (n.Checked === true) {
                                    DVRList.push(n.ID);
                                }
                            } else {
                                getDVRS(node.Sites[i], DVRList);
                            }
                        }
                    }
                }

                function getSites(node, siteList) {
                    if (node && node.Type !== AppDefine.NodeType.Site && node.Sites.length) {
                        var nodeLen = node.Sites.length;
                        for (var i = 0; i < nodeLen; i++) {
                            if (node.Sites[i].Type === AppDefine.NodeType.Site) {
                                var n = node.Sites[i];
                                if (n.Checked === true) {
                                    siteList.push(n.ID);
                                }
                            } else {
                                getSites(node.Sites[i], siteList);
                            }
                        }
                    }
                }

                function getFirstSites(node) {
                    if (node && node.Type !== AppDefine.NodeType.Site && node.Sites.length) {
                        var nodeLen = node.Sites.length;
                        for (var i = 0; i < nodeLen; i++) {
                            if (node.Sites[i].Type === AppDefine.NodeType.Site) {
                               
                                breakFirstSite = node.Sites[i];
                                break;
                            } else {
                                breakFirstSite = getFirstSites(node.Sites[i]);
                                if (breakFirstSite !== null) {
                                    break;
                                }
                            }
                        }
                    }
                    return breakFirstSite;
                }

                function getFirstDVR(node) {
                    var breakFirstDVR;
                    if (node && node.Type !== AppDefine.NodeType.DVR && node.Sites.length) {
                        var nodeLen = node.Sites.length;
                        for (var i = 0; i < nodeLen; i++) {
                            if (node.Sites[i].Type === AppDefine.NodeType.DVR) {

                                breakFirstDVR = node.Sites[i];
                                break;
                            } else {
                                breakFirstDVR = getFirstDVR(node.Sites[i]);
                                if (breakFirstDVR !== null && breakFirstDVR != undefined) {
                                    break;
                                }
                            }
                        }
                    }
                    return breakFirstDVR;
                }

                function getFirstDVRNoVirtual(node) {
                    var breakFirstDVR;
                    if (node && node.Type !== AppDefine.NodeType.DVR && node.Sites.length) {
                        var nodeLen = node.Sites.length;
                        for (var i = 0; i < nodeLen; i++) {
                            if (node.Sites[i].Type === AppDefine.NodeType.DVR) {
                                if (node.Sites[i].IsVirtual != true)
                                {
                                    breakFirstDVR = node.Sites[i];
                                }
                                break;
                            } else {
                                breakFirstDVR = getFirstDVRNoVirtual(node.Sites[i]);
                                if (breakFirstDVR !== null && breakFirstDVR != undefined) {
                                    break;
                                }
                            }
                        }
                    }
                    return breakFirstDVR;
                }

                function getSitesFromDVR(node, idSite, nameSite) {
                    if (nameSite !== '' && nameSite !== undefined) {
                        return nameSite;
                    }
                    if (node && node.Type !== AppDefine.NodeType.Site && node.Sites.length) {
                        var nodeLen = node.Sites.length;
                        for (var i = 0; i < nodeLen; i++) {
                            if (node.Sites[i].Type === AppDefine.NodeType.Site && node.Sites[i].ID === idSite) {

                                nameSite = node.Sites[i].Name;
                                break;
                            } else {
                                nameSite = getSitesFromDVR(node.Sites[i], idSite, nameSite);
                                if (nameSite !== '' && nameSite !== undefined) {
                                    break;
                                }
                            }
                        }
                        return nameSite;
                    }
                }

                function getAllSites(node, siteList) {
                    if (node && node.Type !== AppDefine.NodeType.Site && node.Sites.length) {
                        var nodeLen = node.Sites.length;
                        for (var i = 0; i < nodeLen; i++) {
                            if (node.Sites[i].Type === AppDefine.NodeType.Site) {
                                var n = node.Sites[i];

                                siteList.push(n.ID);
                                
                            } else {
                                getAllSites(node.Sites[i], siteList);
                            }
                        }
                    }
                }

                function checkallNode(node, uncheck) {
                    var nodeLen = node.Sites.length;
                    node.Checked = uncheck === undefined ? true : uncheck;
                    for (var i = 0; i < nodeLen; i++) {
                        checkallNode(node.Sites[i]);
                    }
                }

			function setNodeSelected(treeData, siteIds) {
				if (treeData == null || treeData == undefined || !siteIds) {
					return;
				}

				if (angular.isArray(treeData)) {
					angular.forEach(treeData, function (n) {
						if (n.Type === AppDefine.NodeType.Site && siteIds.indexOf(n.ID) != -1) {
							n.Checked = true;
						}
						else {
							n.Checked = false;
							if (n.Type === AppDefine.NodeType.Region) {
								setNodeSelected(n.Sites, siteIds);
							}
						}
					});
				}
				else {
					if (treeData.Type === AppDefine.NodeType.Site && siteIds.indexOf(treeData.ID) != -1) {
						treeData.Checked = true;
					}
					else {
						treeData.Checked = false;
						if (treeData.Type === AppDefine.NodeType.Region) {
							setNodeSelected(treeData.Sites, siteIds);
						}
					}
				}
			}

			function setNodeSelectedbyDDVR(treeData, DVRs) {
			    if (treeData == null || treeData == undefined || !DVRs) {
			        return;
			    }

			    if (angular.isArray(treeData)) {
			        angular.forEach(treeData, function (n) {
			            if (n.Type === AppDefine.NodeType.DVR && DVRs.indexOf(n.ID) != -1) {
			                n.Checked = true;
			            }
			            else {
			                n.Checked = false;
			                if (n.Type === AppDefine.NodeType.Region || n.Type === AppDefine.NodeType.Site) {
			                    setNodeSelectedbyDDVR(n.Sites, DVRs);
			                }
			                //else {
			                //    //n.Checked = false;
			                //    if (n.Type === AppDefine.NodeType.Site) {
			                //        setNodeSelectedbyDDVR(n.Sites, DVRs);
			                //    }
			                //}
			            }
			        });
			    }
			    else {
			        if (treeData.Type === AppDefine.NodeType.DVR && DVRs.indexOf(treeData.ID) != -1) {
			            treeData.Checked = true;
			        }
			        else {
			            treeData.Checked = false;
			            if (treeData.Type === AppDefine.NodeType.Region || treeData.Type === AppDefine.NodeType.Site) {
			                setNodeSelectedbyDDVR(treeData.Sites, DVRs);
			            }
			            //else {
			            //    //treeData.Checked = false;
			            //    if (n.Type === AppDefine.NodeType.Site) {
			            //        setNodeSelectedbyDDVR(treeData.Sites, DVRs);
			            //    }
			            //}
			        }
			    }
			}

            }

        });
    }
)();
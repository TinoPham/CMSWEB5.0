(function () {
	define(['cms'], function (cms) {
		cms.register.service('treeSiteSvc', treeSiteSvc);
		treeSiteSvc.$inject = ['AppDefine'];

		function treeSiteSvc(AppDefine) {

			var breakFirstSite = null;
			var treeSiteServices = {
				getSitesFromNode: getSites,
				getSiteNode: getSiteNode,
				checkallNode: checkallNode,
				getSitesFromUser: getAllSites,
				getFirstSites: getFirstSites,
				setNodeSelected: setNodeSelected,
				updateSiteChecked: updateSiteChecked,
				getSiteSelectedIDs: getSiteSelectedIDs,
				getSiteSelectedNames: getSiteSelectedNames
			};

			return treeSiteServices;

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

			function getSiteNode(treeData, sitenodeOutput) {
				if (treeData == null || treeData == undefined) {
					return;
				}
				if (sitenodeOutput == null || sitenodeOutput == undefined) {
					sitenodeOutput = [];
				}
				var hasChecked = false;
				angular.forEach(treeData, function (n) {
					if (n != null && n != undefined) {
						if (n.Sites && n.Sites.length > 0) {
							getSiteNode(n.Sites, sitenodeOutput);
						}
						if (n.Checked == true) {
							if (n.Type == 1) {
								sitenodeOutput.push(n);
							}
							else if (!hasChecked) {
								hasChecked = true;
							}
						}
					}
				});
				if (hasChecked && sitenodeOutput.length == 0) {
					sitenodeOutput = [];
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

			function checkallNode(node) {
				var nodeLen = node.Sites.length;
				node.Checked = true;
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

			function updateSiteChecked(treeData, userSelected, checkAll) {
				if (treeData == null || treeData == undefined) {
					return;
				}
				if (checkAll) {
					treeData.Checked = true; //Checked on root node
				}
				if ($.isEmptyObject(treeData.Sites)) {
					angular.forEach(treeData, function (n) {
						if (n != null && n != undefined) {
							if (n.Sites && n.Sites.length > 0) {
								updateSiteChecked(n.Sites, userSelected, checkAll);
							}
							if (checkAll == false && (userSelected == null || userSelected == undefined
								|| userSelected.SiteIDs == null || userSelected.SiteIDs == undefined
								|| userSelected.SiteIDs.indexOf(n.ID) < 0)) {
								n.Checked = false;
							}
							else {
								n.Checked = true;
							}
						}
					});
				}
				else {
					angular.forEach(treeData.Sites, function (n) {
						if (n != null && n != undefined) {
							if (n.Sites && n.Sites.length > 0) {
								updateSiteChecked(n.Sites, userSelected, checkAll);
							}
							if (checkAll == false && (userSelected == null || userSelected == undefined
								|| userSelected.SiteIDs == null || userSelected.SiteIDs == undefined
								|| userSelected.SiteIDs.indexOf(n.ID) < 0)) {
								n.Checked = false;
							}
							else {
								n.Checked = true;
							}
						}
					});
				}
			}

			function getSiteSelectedIDs(siteCheckedIDs, treeData) {
				if (treeData == null || treeData == undefined) {
					return;
				}
				if (siteCheckedIDs == null || siteCheckedIDs == undefined) {
					siteCheckedIDs = new Array();
				}
				var hasChecked = false;
				angular.forEach(treeData, function (n) {
					if (n != null && n != undefined) {
						if (n.Sites && n.Sites.length > 0) {
							getSiteSelectedIDs(siteCheckedIDs, n.Sites);
						}
						if (n.Checked == true) {
							if (n.Type == 1) {
								siteCheckedIDs.push(n.ID);
							}
							else if (!hasChecked) {
								hasChecked = true;
							}
						}
					}
				});
				if (hasChecked && siteCheckedIDs.length == 0) {
					siteCheckedIDs.push(-1);
				}
			}

			function getSiteSelectedNames(siteCheckedIDs, siteNames, treeData) {
				if (treeData == null || treeData == undefined) {
					return;
				}
				if (siteCheckedIDs == null || siteCheckedIDs == undefined) {
					siteCheckedIDs = new Array();
				}
				var hasChecked = false;
				angular.forEach(treeData, function (n) {
					if (n != null && n != undefined) {
						if (n.Sites && n.Sites.length > 0) {
							getSiteSelectedNames(siteCheckedIDs, siteNames, n.Sites);
						}
						if (n.Checked == true) {
							if (n.Type == 1) {
								siteCheckedIDs.push(n.ID);
								siteNames.push(n.Name);
							}
							else if (!hasChecked) {
								hasChecked = true;
							}
						}
					}
				});
				if (hasChecked && siteCheckedIDs.length == 0) {
					siteCheckedIDs.push(-1);
					siteNames.push('');
				}
			}
		}
	});
}
)();
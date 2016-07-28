(function () {
	define(['cms',
		 'DataServices/Configuration/siteadmin.service',
		 'DataServices/AccountSvc',
		// 'Scripts/Services/chartSvc',
	], function (cms) {
		cms.register.controller('incidentCtrl', incidentCtrl);
		incidentCtrl.$inject = ['$scope', '$rootScope', '$timeout', '$stateParams', 'cmsBase', 'AppDefine','siteadmin.service','AccountSvc'];
		function incidentCtrl($scope, $rootScope, $timeout, $stateParams, cmsBase, AppDefine,siteadmin,AccountSvc) {
			
			var vm = this;
			vm.optionsDate = { format: 'L', maxDate: $scope.maxdate, ignoreReadonly: true };
			vm.dateFrom = new Date();
			vm.dateTo = new Date();
			vm.dateIncident = new Date();

            
            vm.filterLeftFlag = false;
			$scope.userModel = AccountSvc.UserModel();
			$scope.treeSites = {};

			vm.def = {
                Id: 'ID',
                Name: 'Name',
                Type: 'Type',
                Checked: 'Checked',
                Childs: 'Sites',
                Count: 'SiteCount',
                Model: {}
            };
            vm.options = {
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
                Icon: {
                    Item: ' icon-home'
                },
                Item: {
                    IsShowItemMenu: false
                },
                Type: {
                    Folder: 0,
                    Group: 2,
                    File: 1
                },
                CallBack: {
                    SelectedFn: function(param1,param2){}
                },
            };

           vm.Init = function(){

            	//var data = dataContext;
            	GetSiteTree();

            }
                      

			function checkinglist(node) {

                var result = AppDefine.treeStatus.Uncheck;
                var intermid = false;
                var interval = AppDefine.treeStatus.Uncheck;
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
                            ch = AppDefine.treeStatus.Indeterm;
                        }

                        result = ch;
                        i++;
                    });
                }
                else {
                    result = node.Checked;
                }
                node.Checked = result;
                return result;
            }
            function reCheckTree(sites) {
                angular.forEach(sites, function (nodes) {
                    checkinglist(nodes);
                });
            }

            function UpdateSiteChecked(treeData) {
                if (!treeData) {
                    return;
                }

                if ($.isEmptyObject(treeData.Sites)) {
                    angular.forEach(treeData, function (n) {
                        if (angular.isObject(n)) {
                            if (n.Sites && n.Sites.length > 0) {
                                UpdateSiteChecked(n.Sites);
                            }

                            if ($scope.userSelectedSites == null || $scope.userSelectedSites == undefined
								|| $scope.userSelectedSites.SiteIDs == null || $scope.userSelectedSites.SiteIDs == undefined
								|| $scope.userSelectedSites.SiteIDs.indexOf(n.ID) < 0 || n.Type === 0) {
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
                        if (angular.isObject(n)) {
                            if (n.Sites && n.Sites.length > 0) {
                                UpdateSiteChecked(n.Sites);
                            }
                            if ($scope.userSelectedSites == null || $scope.userSelectedSites == undefined
								|| $scope.userSelectedSites.SiteIDs == null || $scope.userSelectedSites.SiteIDs == undefined
									|| $scope.userSelectedSites.SiteIDs.indexOf(n.ID) < 0 || n.Type === 0) {
                                n.Checked = false;
                            }
                            else {
                                n.Checked = true;
                            }
                        }
                    });
                }
            }


             function GetSiteTree() {
                siteadmin.create().GetSiteByUserId($scope.userModel.UserID, function (data) {
                    if (!data.Sites) {
                        $scope.sites = {
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
                        return;
                    }
                    UpdateSiteChecked(data);
                    $scope.sites = data;
                    reCheckTree($scope.sites.Sites);
                    vm.treeSites = angular.copy($scope.sites);
                  //  bamhelperSvc.setNodeSelected($scope.treeSites, $scope.data.SiteList);
                }, function (error) {
                    var msg = cmsBase.translateSvc.getTranslate(error.data.ReturnMessage[0]);
                    cmsBase.cmsLog.error(msg);
                });
            };

        

		}
	});
})();
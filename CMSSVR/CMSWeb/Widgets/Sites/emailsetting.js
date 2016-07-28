(function () {
    define(['cms', 'DataServices/Configuration/usergroups.service', 'Scripts/Services/bamhelperSvc', 'widgets/sites/emailalerttype'], emailsetting);
    function emailsetting(cms) {
        cms.register.controller("emailsettingCtrl", emailsettingCtrl);
        emailsettingCtrl.$inject = ['$scope', 'dataContext', 'cmsBase', 'AppDefine', '$timeout', '$rootScope', '$filter', '$window', '$modal', '$modalInstance', 'AccountSvc', 'usergroups.service', 'items', 'bamhelperSvc','Utils'];
        function emailsettingCtrl($scope, dataContext, cmsBase, AppDefine, $timeout, $rootScope, $filter, $window, $modal, $modalInstance, AccountSvc, usergroupsSvc, items, bamhelperSvc,Utils) {
           // var that = this;
            $scope.optionsDate = { format: 'LT', ignoreReadonly: true };
            $scope.data = [];
            $scope.userModel = AccountSvc.UserModel();
            $scope.date = new Date();
            $scope.treeSites = {};
            $scope.sites = {};
            $scope.selectedsites=[];
            $scope.query = undefined;
            $scope.uquery = '';
            $scope.userSelectedSites = {};
            $scope.setHourly = setHourly;
            $scope.def = {
                Id: 'ID',
                Name: 'Name',
                Type: 'Type',
                Checked: 'Checked',
                Childs: 'Sites',
                Count: 'SiteCount',
                Model: {}
            };
            $scope.options = {
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
                    //Item: ' icon-home'
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
                    SelectedFn: selectedFn
                },
            };

            $scope.Close = function () {
                $modalInstance.close(null);
            }

            $scope.init = function (data) {
                $scope.data = items.model[0];
                if ($scope.data.StartRunDate == null) {
                    $scope.data.StartRunDate = new Date();
                }
                else $scope.data.StartRunDate = cmsBase.DateUtils.getUTCDate($scope.data.StartRunDate);

                
               
                GetSiteTree();
                getUserList();
                
            };

            $scope.filter = function (uquery) {
                

                return function (item) {

                    if (item == null) return true;
                    if (uquery == "" || uquery == undefined || uquery == null) {
                        return true;
                    }
                    var fullName = item.FName + item.LName;



                    if (!$.isEmptyObject(fullName) && fullName.toLowerCase().replace(/\n/g, '').replace(/\s+/g, '').indexOf(uquery.toLowerCase().replace(/\n/g, "").replace(/\s+/g, '')) > -1) {
                        return true;
                    }
                    return false;
                }
            }

            $scope.ApiImageUrl = ApiImageUrl;
            $scope.openAlerts = openAlerts;
            $scope.SaveEmailSettings = SaveEmailSettings;
            $scope.GetAlertType = function () {
                dataContext.sitealert.GetAllEmailAlertTypes(function (data)
                {
                    
                    angular.forEach($scope.data.Alerts, function (value, key) {
                        
                        angular.forEach(data, function (_value, _key) {
                            if (data[_key].Id == value.Id)
                            {
                                data[_key].Active = true;
                            }
                        });


                    });
                  
                    var userInstance = $modal.open({
                        templateUrl: 'widgets/sites/emailalerttype.html',
                        controller: 'emailalerttypeCtrl',
                        size: 'md',
                        backdrop: 'static',
                        backdropClass: 'modal-backdrop',
                        windowClass: 'email-setting-alerttype',
                        keyboard: false,
                        resolve: {
                            items: function () { return { model: data } }
                        }
                    });
                    userInstance.result.then(function (data) {
                        $scope.modalShown = false;
                        if (data)
                        {
                            $scope.data.Alerts = data;

                        }
                    });
                  
                }, function (err) { });

            }
            function openAlerts() {
                $scope.GetAlertType();
            };

            function setHourly()
            {
                if ($scope.data.StartRunDate == null) 
                {
                    $scope.data.StartRunDate = new Date();
                }
                $scope.data.FreqTypeID = 1;
                var date = cmsBase.DateUtils.startOfDate($scope.data.StartRunDate);
                $scope.data.StartRunDate = date;
            }

            function SaveEmailSettings()
            {

                if ($scope.data.Alerts == undefined || $scope.data.Alerts == null || $scope.data.Alerts.length == 0) {

                    cmsBase.cmsLog.warning($filter('translate')('ALERT_TYPE_WARNING'));
                    return;
                }
                $scope.selectedsites = []
            
                bamhelperSvc.getSitesFromNode($scope.treeSites, $scope.selectedsites);

                if ($scope.selectedsites.length == 0)
                {
                    cmsBase.cmsLog.warning($filter('translate')('SELECT_SITE_WARNING'));
                    return;
                }
                $scope.data.Recipients = Enumerable.From($scope.userList).Where(function (item) { return item.Active == true }).Select(function (value) { return value.UserID }).ToArray();
                $scope.data.SiteList = $scope.selectedsites;
                $scope.data.ReportName = $scope.data.EmailSubject;
                
                //if ($scope.data.FreqTypeID == 1)
                //{
                //    setHourly();
                //}
                var dataCopy = {};
                angular.copy($scope.data,dataCopy);
                dataCopy.StartRunDate = Utils.toUTCDate(dataCopy.StartRunDate);


                dataContext.sitealert.SaveEmailSettings(dataCopy, function (data) {
                    cmsBase.cmsLog.success($filter('translate')('BTN_DONE'));
                    $modalInstance.close(true);
                }, function (err) {
                    cmsBase.cmsLog.error(err);
                })
            }

            function getUserList() {
                usergroupsSvc.create().getUserList(function (data)
                {
                    

                    angular.forEach($scope.data.Recipients, function (value, key) {

                        angular.forEach(data, function (_value, _key) {
                            if (data[_key].UserID == value) {
                                data[_key].Active = true;
                            }
                        });


                    });
                    $scope.userList = data;

                },
                function (error) {
                    cmsBase.cmsLog.error('error');
                });

            }
            function selectedFn(node, scope) {
                scope.checkFn(node, scope.props.parentNode, scope);
             
                
            }

            function GetSiteTree() {
                dataContext.siteadmin.GetSiteByUserId($scope.userModel.UserID, function (data) {
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
                    $scope.treeSites = angular.copy($scope.sites);

                    filterEmtyRegion();
                    bamhelperSvc.setNodeSelected($scope.treeSites, $scope.data.SiteList);



                }, function (error) {
                    var msg = cmsBase.translateSvc.getTranslate(error.data.ReturnMessage[0]);
                    cmsBase.cmsLog.error(msg);
                });
            };

            function filterEmtyRegion() {
                var data = $scope.treeSites;
                var result = Enumerable.From(data.Sites)
                                       .Where(function (value) {
                                           return (value.Sites.length > 0 && (Enumerable.From(value.Sites)
                                                                                       .Where(function (key) { return key.Type == 1 })
                                                                                       .ToArray().length > 0)) || value.Type == 1
                                       }).ToArray();

                angular.forEach(result, function (_value, key)
                {
                   result[key].Sites = Enumerable.From(_value.Sites)
                              .Where(function (value) {
                                  return (value.Sites.length > 0 && (Enumerable.From(value.Sites)
                                                                              .Where(function (key) { return key.Type == 1 })
                                                                              .ToArray().length > 0)) || value.Type == 1
                              }).ToArray();
                });
                $scope.treeSites.Sites = result;

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

            function ApiImageUrl(usrID, imagepath) {
                return dataContext.user.GetUserImage(usrID, imagepath);
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

            
        };
    };
})();
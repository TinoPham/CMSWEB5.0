(function() {
    'use strict';

    define(['cms'], function (cms) {
        cms.register.controller('editadhocCtrl', editadhocCtrl);

        editadhocCtrl.$inject = ['$scope', '$modalInstance', 'cmsBase', 'Utils', 'items', 'dataContext', '$timeout', 'dialogSvc', 'AppDefine', '$modal', 'colorSvc', 'adhocDataSvc', 'rebarDataSvc', 'filterSvc', '$filter'];

        function editadhocCtrl($scope, $modalInstance, cmsBase,Utils, items, dataContext, $timeout, dialogSvc, AppDefine, $modal, colorSvc, adhocDataSvc, rebarDataSvc, filterSvc, $filter) {
            var vm = this;
            $scope.data = {
                RegionKey: 0,
                UserKey: 0,
                RegionName: "",
                RegionParentId: null,
                Description: ""
            };
            vm.groupSelected = null;
            vm.status = {
                info: true,
                filter: true,
                isCloseOther: false,
                isUsersOpen: true
            };
            vm.userquery = '';
            $scope.site = {};
            $scope.isSave = false;
            $scope.data = items;
            $scope.btn_Type = items.isNew;
            vm.DateGroup = [
                { Id: 1, Name: 'Hourly' },
                { Id: 2, Name: 'Daily' },
                { Id: 3, Name: 'Weekly' },
                { Id: 4, Name: 'Monthly' },
                //{ Id: 5, Name: 'WTD' },
                //{ Id: 6, Name: 'YTD' }
            ];
            vm.DefaultSelectCols = ["TranNo", "TranDate", "Total", "Payments", "ExceptionTypes"];
            vm.DefaultGroupCol = "TranDate";
            vm.dateGroupSelected = vm.DateGroup[1];
            vm.FilterColumns = [];
            active();

            $scope.Close = function () {
                $modalInstance.close(AppDefine.ModalConfirmResponse.CLOSE);
            }

            $scope.deleteFilter = function(item) {
                item.filter = null;
                vm.filterSelected = null;
            }

            $scope.selectfilters = function (isAdd) {

                if (!$scope.modalShown) {
                    var checkchangecolumn = angular.copy(vm.filterSelected);
                    $scope.modalShown = true;
                    var showFilterModal = $modal.open({
                        templateUrl: 'rebar/adhoc/filtersetting.html',
                        controller: 'filtersettingCtrl as vm',
                        resolve: {
                            items: function () {
                                return {isAdd: isAdd, cols: vm.columnDefs, filterList: vm.FilterColumns, select: vm.filterSelected }
                            }
                        },
                        size: 'md',
                        backdrop: 'static',
                        keyboard: false
                    });

                    showFilterModal.result.then(function (data) {
                        $scope.modalShown = false;
                        if (data) {

                            vm.columnDefs = data.fields;
                            if (!isAdd && data.select.ColID !== checkchangecolumn.ColID) {
                                vm.filterSelected.filter = null;
                                vm.filterSelected = null;
                            }
                        }
                    });
                }
            }

            vm.checkUser = function (user) {
                user.Checked = !user.Checked;
            }

            vm.refeshUsers = function (e) {
                e.stopPropagation();
                getUserList();
            }

            vm.changeColumns = function() {
                console.log(vm.Selected);
            }

            vm.NumberItemText = function (icount) {
				var str = (icount === 0) ? cmsBase.translateSvc.getTranslate('ANY_STRING') : icount + " " + cmsBase.translateSvc.getTranslate('ITEMS');
            	return str;
            }

            function active() {
                vm.dateGroupSelected = vm.DateGroup[1];
                dataContext.injectRepos(['configuration.user']).then(getUserList).then(getAhocReportColumn).then(function() {
                    if (items.isNew === false) {
                        adhocDataSvc.getAdhocReportById({ reportId: items.id }, function (result) {
                            vm.report = result;

                            vm.Reportname = result.ReportName;
                            vm.Description = result.ReportDesc;

                            result.ColumnFilter.forEach(function (x) {
                                var filter = Enumerable.From(vm.columnDefs).Where(function (u) { return u.ColID === x.ColumnValue[0].ColID; }).FirstOrDefault();
                                if (filter) {
                                    var cusfilter = filterSvc.getfilterDef(filter.DataType);
                                    var fil = {
                                        firstvalue: x.ColumnValue[0].CriteriaValue_1,
                                        SelectAnd: x.AND_OP,
                                        selectfirst: Enumerable.From(cusfilter).Where(function (u) { return u.key === x.ColumnValue[0].Operator; }).FirstOrDefault()
                                    }

                                    if (filter.DataType === 'childid' || filter.DataType === 'list') {
										//Allow 'Any' for select list, #3907
                                    	if (x.ColumnValue[0].CriteriaValue_1.length > 0) {
                                    		var ids = x.ColumnValue[0].CriteriaValue_1.split(',');
                                    		fil.firstvalue = ids;
                                    	}
                                    	else {
                                    		fil.firstvalue = [];
                                    	}
                                    }

                                   
                                        var newFilter = angular.copy(filter);
                                        var newId = Utils.newGUID();
                                        newFilter.id = newId;
                                        newFilter.filter = fil;
                                        vm.FilterColumns.push(newFilter);
                                    
                                }
                            });

                            vm.user.forEach(function (x) {
                                var userSelect = Enumerable.From(result.Assign).Where(function (u) { return x.UID === u.UserID; }).FirstOrDefault();
                                if (userSelect) {
                                    x.Checked = true;
                                }
                            });

                            if (result.GroupFields[0]) {
                                vm.groupSelected = Enumerable.From(vm.columnDefs).Where(function (u) { return u.ColID === result.GroupFields[0].ColID; }).FirstOrDefault();
                                if (result.GroupFields[0].GroupName === vm.DefaultGroupCol) {
                                    vm.dateGroupSelected = Enumerable.From(vm.DateGroup).Where(function (u) { return u.Id === result.GroupFields[0].GroupType; }).FirstOrDefault();
                                }
                            }
                            vm.Selected = [];
                            vm.columnDefs.forEach(function(x) {
                                var select = Enumerable.From(result.ColumnSelect).Where(function (u) { return u.ColID === x.ColID; }).FirstOrDefault();
                                if (select) {
                                    vm.Selected.push(x);
                                }
                            });

                        },
						function (error) {
						});
                    }
                });
            }

            function getAhocReportColumn() {
                var def = cmsBase.$q.defer();
                adhocDataSvc.getAhocReportColumn(function (data) {
                    vm.columnDefs = data;
                    vm.SelectedList = Enumerable.From(vm.columnDefs).Where(function (x) { return x.DataType !== 'childid' && x.DisplayField !== 'PaymentAmount' && x.DisplayField !== 'TaxAmount'; }).ToArray();
                    vm.GroupColumns = Enumerable.From(vm.columnDefs).Where(function (x) { return x.Groupable === true; }).ToArray();
                    if (items.isNew === true) {
                    	//vm.DefaultSelectCols
                    	var selectCols = Enumerable.From(vm.SelectedList).Where(function (x) { return vm.DefaultSelectCols.indexOf(x.ColName) >= 0; }).ToArray();
                    	if (selectCols && selectCols.length > 0) {
                    		if (!vm.Selected)
                    			vm.Selected = [];
                    		selectCols.forEach(function (x) {
                    			x.Checked = true;
                    			vm.Selected.push(x);
                    		});
                    	}
                    	vm.groupSelected = Enumerable.From(vm.GroupColumns).Where(function (u) { return u.ColName === vm.DefaultGroupCol; }).FirstOrDefault();
                    	if (vm.groupSelected)
                    		vm.groupSelected.Checked = true;
                    }
                    def.resolve();
                }, function (error) { def.reject(); });
                return def.promise;
            }

            function getUserList() {
                var def = cmsBase.$q.defer();
                dataContext.user.getUsers(function (data) {

                    $timeout(function () {
                        vm.user = data;
                        if (vm.site && vm.site.DvrUsers && vm.site.DvrUsers.length > 0) {
                            //CheckedUsers(vm.user, vm.site.DvrUsers);
                        }
                    }, 10, false);

                    def.resolve();
                }, function (error) {
                    //siteadminService.ShowError(error);
                    def.reject();
                });
                return def.promise;
            }

            vm.searchUses = function (user) {
                // 2015-06-06 Tri fix case text more than once space.
                var query = vm.userquery.replace(/\s+/g, '').toLowerCase();

                var firstname = user.FName ? user.FName.replace(/\s+/g, '').toLowerCase() : "";
                var lastname = user.LName ? user.LName.replace(/\s+/g, '').toLowerCase() : "";
                var fullname = firstname + lastname;
                if (firstname.indexOf(query) != -1
                    || lastname.indexOf(query) != -1
                    || fullname.indexOf(query) != -1
                    || user.PosName && user.PosName.toLowerCase().indexOf(query) != -1) {
                    return true;
                }
                return false;
            };

            $scope.Save = function () {
                console.log(vm);

                var userSelect = Enumerable.From(vm.user).Where(function (x) { return x.Checked === true; }).Select(function (x) { return { UserID: x.UID } }).ToArray();

                var colSelect = Enumerable.From(vm.Selected).Select(function (x) {
                    return {
                        ColID: x.ColID,
                        ColWidth: 0,
                        ColOrder: 0,
                        DisplayName: x.DisplayField,
                        SortOrder: 0,
                        Ascending: false,
                        GroupBy:false
                    };
                }).ToArray();

                var group = vm.groupSelected;
                if (group) {
                    var checkgroup = Enumerable.From(colSelect).Where(function (x) { return x.ColID === group.ColID; }).FirstOrDefault();
                    if (checkgroup) {
                        checkgroup.GroupBy = true;
                        checkgroup.ColWidth = vm.dateGroupSelected ? vm.dateGroupSelected.Id : 2;
                    } else {
                        colSelect.push({
                            ColID: group.ColID,
                            ColWidth: vm.dateGroupSelected ? vm.dateGroupSelected.Id : 2,
                            ColOrder: 0,
                            DisplayName: group.DisplayField,
                            SortOrder: 0,
                            Ascending: false,
                            GroupBy: true
                        });
                    }
                }

                var colfilter = [];
                vm.FilterColumns.forEach(function (x) {
                    if (x.filter) {
                        var value = '';
                        if (x.DataType === 'childid' || x.DataType === 'list') {
                            value = x.filter.firstvalue.join();
                        } else {
                            value = x.filter.firstvalue;
                        }

                        if (x.DataType === 'datetime') {
                            var date = new Date(value);
                            date.setHours(0);
                            date.setMinutes(0);
                            date.setSeconds(0);
                            date.setMilliseconds(0);
                            value = $filter('date')(date, "yyyy-MM-ddTHH:mm:ss.sss") + 'Z';
                        }
                        var filter = { AND_OP: x.filter.SelectAnd ? true : false, ColumnValue: [{ ColID: x.ColID, Operator: x.filter.selectfirst.key, CriteriaValue_1: value, CriteriaValue_2: "" }] }
                        colfilter.push(filter);
                    }
                });

                var report = {
                    ReportName: vm.Reportname,
                    FolderId: items.id? items.id: null,
                    ReportDesc: vm.Description,
                    PromoteToDashboard: false,
                    Assign: userSelect,
                    ColumnSelect:colSelect,
                    ColumnFilter: colfilter
                }

                if (vm.report) {
                    report.ReportID = vm.report.ReportID;
                }

                adhocDataSvc.addAdhocReport(report, function(data) {
                    $modalInstance.close(data);
                }, function(error) {
                    var msg = cmsBase.translateSvc.getTranslate(error.data.Data.ReturnMessage[0]);
                    cmsBase.cmsLog.error(msg);
                });
              
            }

            vm.openUsers = function (user) {
                showUserDialog(user);
            }

            vm.ApiImageUrl = function (userId, fileName) {
                return dataContext.user.GetUserImage(userId, fileName);
            };

            vm.convertColor = function (data) {
                return colorSvc.numtoRGB(data);
            };

            function showUserDialog(user) {
                if (!$scope.modalShown) {
                    $scope.modalShown = true;
                    var userInstance = $modal.open({
                        templateUrl: 'configuration/users/edit.html',
                        controller: 'editadduserCtrl as vm',
                        size: 'md',
                        backdrop: 'static',
                        backdropClass: 'modal-backdrop',
                        keyboard: false,
                        resolve: {
                            items: function () {
                                return user;
                            }
                        }
                    });

                    userInstance.result.then(function (data) {
                        $scope.modalShown = false;
                        if (data != AppDefine.ModalConfirmResponse.CLOSE) {
                            getUserList();
                        }
                    });
                }
            }
        }
    });

})();
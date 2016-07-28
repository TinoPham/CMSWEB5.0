(function () {
	'use strict';

	define(['cms'], function (cms) {
	    cms.register.controller('customfilterCtrl', customfilterCtrl);

	    customfilterCtrl.$inject = ['$scope', 'dataContext', 'cmsBase', '$modalInstance', 'items', 'AppDefine', 'rebarDataSvc', '$modal', 'filterSvc', 'adhocDataSvc', '$timeout'];

	    function customfilterCtrl($scope, dataContext, cmsBase, $modalInstance, items, AppDefine, rebarDataSvc, $modal, filterSvc, adhocDataSvc, $timeout) {
	        var vm = this;
	        $scope.fields = items;
            $scope.filter = {
                selectfirst: null,
                firstvalue: null,
                SelectAnd: false,
                selectend: null,
                endvalue: null
            }
            $scope.CurrentPage = 1;
            $scope.searchData = [];
            $scope.errorMsg = "";
            if (items.filter) {
                $scope.dateOptions = {
                    defaultDate: items.fieldType === 'datetime' ? items.filter.firstvalue : null,
                    ignoreReadonly: true,
                    format: items.fieldType === 'datetime' ? items.filter.DateFormatFilter === null ? 'MM/DD/YYYY HH:mm' : items.filter.DateFormatFilter : 'MM/DD/YYYY HH:mm'
                };
            } else {
                $scope.dateOptions = {
                    defaultDate: null,
                    ignoreReadonly: true,
                    format: 'MM/DD/YYYY HH:mm'
                };
	        }
	        
            $scope.Selected = [];
            vm.datalist = [];
	        $scope.searchModel = [];
            resetpageSetting();

            active();
            
            $scope.$on("filterdata", function (e, arg) {
                resetpageSetting();
                $scope.filterText = arg.filter;
                switch (arg.name) {
                    case "RegisterId":
                        getRegisterList(arg.filter);
                        break;
                    case "EmployeeId":
                        getOperatorList(arg.filter);
                        break;
                    case "TerminalId":
                        getTerminalList(arg.filter);
                        break;
                    case "ShiftId":
                        getShiftList(arg.filter);
                        break;
                    case "StoreId":
                        getStoreList(arg.filter);
                        break;
                    case "CheckId":
                        getCheckList(arg.filter);
                        break;
                    case "CamId":
                        getCamList(arg.filter);
                        break;
                    case "CardId":
                        getCardList(arg.filter);
                        break;
                    case "DescId":
                        getDescList(arg.filter);
                        break;
                    case "ItemCodeId":
                        getItemList(arg.filter);
                        break;
                }
            });

            $scope.$on("loadmoredata", function (e, arg) {
                    switch (arg) {
                        case "RegisterId":
                            getRegisterList();
                            break;
                        case "EmployeeId":
                            getOperatorList();
                            break;
                        case "TerminalId":
                            getTerminalList();
                            break;
                        case "ShiftId":
                            getShiftList();
                            break;
                        case "StoreId":
                            getStoreList();
                            break;
                        case "CheckId":
                            getCheckList();
                            break;
                        case "CamId":
                            getCamList();
                            break;
                        case "CardId":
                            getCardList();
                            break;
                        case "DescId":
                            getDescList();
                            break;
                        case "ItemCodeId":
                            getItemList();
                            break;
                    }
                
            });

            $scope.$on("checkedchange", function (e, arg) {
                var modelIt = arg.item.model;
                var changed = false;
                if (arg.item.checked) {
                    if (isExistItem($scope.Selected, modelIt.ID) == false) {
                        $scope.Selected.push(modelIt);
                        changed = true;
                    }
                }
                else {
                    if (isExistItem($scope.Selected, modelIt.ID) == true) {
                        changed = true;
                        for (var i = $scope.Selected.length - 1; i >= 0; i--) {
                            if ($scope.Selected[i].ID == modelIt.ID) {
                                $scope.Selected.splice(i, 1);
                                break;
                            }
                        } //for
                    } //if exist
                }
                if (changed) {
                    if ($scope.Selected.length > 0) {
                        $scope.searchModel = Enumerable.From($scope.Selected).Select(function (x) { return x.ID; }).ToArray();
                    }
                    else {
                        $scope.searchModel = [];
                    }
                }
            });

			$scope.$on("uncheckall", function (e, arg) {
				$scope.searchModel = [];
				$scope.Selected = [];
			});

            $scope.ddlChanged = function (fieldItem) {
                $scope.Selected = $scope.$$childTail.Selected;
                if ($scope.Selected.length > 0) {
                    $scope.searchModel = Enumerable.From($scope.Selected).Select(function (x) { return x.ID; }).ToArray();
                }
			};

            function calPage(response) {
                vm.totalPages = parseInt(response.Countline / vm.pagesize);
                if (response.Countline % vm.pagesize > 0) {
                    vm.totalPages = vm.totalPages + 1;
                }
                vm.nextpage = vm.nextpage + 1;
                vm.currPage = vm.currPage + 1;
            }

            function resetpageSetting() {
                vm.datalist = $scope.Selected;
                vm.pagesize = 50;
                vm.nextpage = 1;
                vm.currPage = 0;
                vm.totalPages = 0;
                $scope.filterText = "";

            }

	        $scope.save = function() {
	            if ($scope.fields.fieldType === 'list') {

	                if ($scope.fields.fieldName === 'Payments') {
	                    var result = Enumerable.From($scope.data).Where(function(x) { return x.Checked === true; }).Select(function(x) { return x.ID; }).ToArray();
	                    $scope.fields.filter = result;
	                    $modalInstance.close($scope.fields);
	                    return;
	                }
	                var result = Enumerable.From($scope.data).Where(function(x) { return x.Checked === true; }).Select(function(x) { return x[$scope.fields.Expand.key]; }).ToArray();
	                $scope.fields.filter = result;
	                $modalInstance.close($scope.fields);
	                return;
	            }

	            if ($scope.fields.fieldType === 'number' || $scope.fields.fieldType === 'money') {
	                if (!angular.isNumber($scope.filter.firstvalue)) {
	                    $scope.errorMsg = cmsBase.translateSvc.getTranslate("Please input number value");
	                    return;
	                }
	            }

	            if ($scope.fields.fieldType === 'string'
	                && $scope.fields.key !== ''
	                && $scope.filter.selectfirst.id === 0) {
	                $scope.filter.firstvalue = $scope.searchModel;
	            }

	            $scope.fields.filter = $scope.filter;
	            if ($scope.fields.fieldType === 'datetime') {
	                $scope.fields.filter = angular.copy($scope.filter);
	                var tempDate;
	                switch ($scope.fields.filter.selectfirst.key) {
	                    //case "eq":
	                    //case "ne":
	                    //	$scope.fields.filter.firstvalue = new Date($scope.filter.endvalue);
	                    //	$scope.fields.filter.firstvalue.setHours($scope.filter.firstvalue.getHours());
	                    //	$scope.fields.filter.firstvalue.setMinutes($scope.filter.firstvalue.getMinutes());
	                    //	$scope.fields.filter.firstvalue.setSeconds($scope.filter.firstvalue.getSeconds());

	                    //	$scope.fields.filter.endvalue = $scope.fields.filter.firstvalue;
	                    //	$scope.fields.filter.selectend = $scope.fields.filter.selectfirst;
	                    //	break;
	                case "gt":
	                case "ge":
	                    $scope.fields.filter.firstvalue = new Date($scope.filter.endvalue);
	                    $scope.fields.filter.firstvalue.setHours($scope.filter.firstvalue.getHours());
	                    $scope.fields.filter.firstvalue.setMinutes($scope.filter.firstvalue.getMinutes());
	                    $scope.fields.filter.firstvalue.setSeconds($scope.filter.firstvalue.getSeconds());
	                    break;
	                case "lt":
	                case "le":
	                    tempDate = angular.copy($scope.filter);

	                    $scope.fields.filter.firstvalue = new Date($scope.filter.endvalue);
	                    $scope.fields.filter.firstvalue.setHours(0);
	                    $scope.fields.filter.firstvalue.setMinutes(0);
	                    $scope.fields.filter.firstvalue.setSeconds(0);
	                    $scope.fields.filter.selectfirst = Enumerable.From(filterSvc.getfilterDatetime()).Where(function(w) { return w.key === 'ge'; }).FirstOrDefault();

	                    $scope.fields.filter.endvalue = new Date($scope.filter.endvalue);
	                    $scope.fields.filter.endvalue.setHours(tempDate.firstvalue.getHours());
	                    $scope.fields.filter.endvalue.setMinutes(tempDate.firstvalue.getMinutes());
	                    $scope.fields.filter.endvalue.setSeconds(tempDate.firstvalue.getSeconds());
	                    $scope.fields.filter.selectend = tempDate.selectfirst;
	                    break;
	                }
	            }

	            $modalInstance.close($scope.fields);
	        };

	        $scope.scrollOption = {
	            onScroll: function (y, x) {
	                if (Math.ceil(y.scroll) >= Math.ceil(y.maxScroll) && $scope.filterText && $scope.filterText.length === 0) {
	                    getData($scope.CurrentPage);
	                }
	            }
			};
	       
	        function getData(pageNumber) {
	            var def = cmsBase.$q.defer();
	            if ($scope.TotalPage < pageNumber) { def.resolve($scope.searchData); }

	            var params = {
	                PageSize: 10000,
	                PageNumber: pageNumber
	            };

	            rebarDataSvc.GetPaymentList(params, function (response) {
	                $scope.searchData = $scope.searchData.concat(response.Data);
	                $scope.CurrentPage = response.CurrentPage;
	                def.resolve($scope.searchData);
	                $scope.TotalPage = response.TotalPage;

	                    $scope.data = $scope.searchData;
	                $scope.data.forEach(function (p) {
	                    var sel = Enumerable.From($scope.fields.filter).Where(function (x) { return x === p.ID; }).FirstOrDefault();
	                    if (sel) {
	                        p.Checked = true;
	                    } else {
	                        p.Checked = false;
	                    }
	                });
	            },
                function (error) {
                    def.resolve($scope.searchData);
                    console.log(error);
                });
	            return def.promise;
	        }

	        $scope.selectFilter = function (item, first) {
	            if (first) {
	                $scope.filter.selectfirst = item;
	            } else {
	                $scope.filter.selectend = item;
	            }
	            
			};

	        $scope.clearFilter = function () {
				if ($scope.fields.fieldType === 'string'
                    && $scope.fields.key !== ''
                    && $scope.filter.selectfirst.id === 0) {
				    $scope.filter.selectfirst.id = -1;
				    $timeout(function() {
				        $scope.searchModel = [];
				        $scope.Selected = [];
				        $scope.filter.selectfirst.id = 0;
				    }, 0);
				    return;
				}

	            if ($scope.data && $scope.fields.fieldType === 'list') {
	                $scope.data.forEach(function(x) {
	                    x.Checked = false;
	                });
	                return;
	            }

	            if ($scope.filterDef) {
	                $scope.filter.selectfirst = $scope.filterDef[0];
	                $scope.filter.selectend = $scope.filterDef[0];
	            }
			};

            function active() {
                if ($scope.fields) {
	                if ($scope.fields.fieldType === 'number') {
	                    $scope.filter.firstvalue = 0;
	                    $scope.filter.endvalue = 0;
	                    $scope.filterDef = filterSvc.getfilterNumber();
	                }

	                if ($scope.fields.fieldType === 'datetime') {
	                    $scope.filter.firstvalue = new Date();
	                    $scope.filter.endvalue = new Date();
	                    $scope.filterDef = filterSvc.getfilterDatetime();
	                }

	                if ($scope.fields.fieldType === 'money') {
	                    $scope.filter.firstvalue = 0;
	                    $scope.filter.endvalue = 0;
	                    $scope.filterDef = filterSvc.getfilterNumber();
	                }

	                if ($scope.fields.fieldType === 'string' || ($scope.fields.Expand && $scope.fields.Expand.fieldType === 'string')) {
	                    $scope.filter.firstvalue = '';
	                    $scope.filter.endvalue = '';
	                    $scope.filterDef = filterSvc.getfilterString();
	                }

	                if ($scope.fields.fieldType === 'list' && $scope.fields.Expand.fieldType === 'number') {
	                    $scope.ForeignKeyMode = true;

	                    switch ($scope.fields.fieldName) {
	                    case 'Payments':
	                        getData($scope.CurrentPage).then(function(data) {
	                            $scope.data = data;

	                            $scope.data.forEach(function(p) {
	                                var sel = Enumerable.From($scope.fields.filter).Where(function(x) { return x === p.ID; }).FirstOrDefault();
	                                if (sel) {
	                                    p.Checked = true;
	                                } else {
	                                    p.Checked = false;
	                                }
	                            });
	                        });
	                        break;
	                    case 'Descriptions':
	                        rebarDataSvc.GetDescriptionList(function(data) {
	                            $scope.data = data;
	                            $scope.data.forEach(function(p) {
	                                var sel = Enumerable.From($scope.fields.filter).Where(function(x) { return x === p.ID; }).FirstOrDefault();
	                                if (sel) {
	                                    p.Checked = true;
	                                } else {
	                                    p.Checked = false;
	                                }
	                            });

	                        }, function(error) {

	                        });
	                        break;
	                    case 'ExceptionTypes':
	                        rebarDataSvc.getTransactionTypes(function(data) {
	                            $scope.data = data;

	                            $scope.data.forEach(function(p) {
	                                var sel = Enumerable.From($scope.fields.filter).Where(function(x) { return x === p[$scope.fields.Expand.key]; }).FirstOrDefault();
	                                if (sel) {
	                                    p.Checked = true;
	                                } else {
	                                    p.Checked = false;
	                                }
	                            });

	                        }, function(error) {

	                        });
	                        break;
	                    case 'Taxs':
	                        rebarDataSvc.getTaxsList(function(data) {
	                            $scope.data = data;

	                            $scope.data.forEach(function(p) {
	                                var sel = Enumerable.From($scope.fields.filter).Where(function(x) { return x === p[$scope.fields.Expand.key]; }).FirstOrDefault();
	                                if (sel) {
	                                    p.Checked = true;
	                                } else {
	                                    p.Checked = false;
	                                }
	                            });

	                        }, function(error) {

	                        });
	                        break;
	                    }

	                }

	                if ($scope.filterDef) {
	                    $scope.filter.selectfirst = $scope.filterDef[0];
	                    $scope.filter.selectend = $scope.filterDef[0];
	                }

	                if (items.filter) {
	                    $scope.filter = items.filter;
	                    if ($scope.fields.fieldType === 'number' || $scope.fields.fieldType === 'money') {
	                        $scope.filter.firstvalue = parseInt($scope.filter.firstvalue);
	                        $scope.filter.endvalue = parseInt($scope.filter.endvalue);
	                        $scope.filter.selectfirst = Enumerable.From($scope.filterDef).Where(function(x) { return x.id === $scope.filter.selectfirst.id; }).FirstOrDefault();
	                        $scope.filter.selectend = Enumerable.From($scope.filterDef).Where(function(x) { return x.id === $scope.filter.selectend.id; }).FirstOrDefault();
	                    }

	                    if ($scope.fields.fieldType === 'datetime') {
	                        $scope.filter.firstvalue = new Date($scope.filter.firstvalue);
	                        $scope.filter.endvalue = new Date($scope.filter.endvalue);
	                        $scope.filter.selectfirst = Enumerable.From($scope.filterDef).Where(function(x) { return x.id === $scope.filter.selectfirst.id; }).FirstOrDefault();
	                        $scope.filter.selectend = Enumerable.From($scope.filterDef).Where(function(x) { return x.id === $scope.filter.selectend.id; }).FirstOrDefault();
	                    }

	                    if ($scope.fields.fieldType === 'string') {
	                        $scope.filter.selectfirst = Enumerable.From($scope.filterDef).Where(function(x) { return x.id === $scope.filter.selectfirst.id; }).FirstOrDefault();
	                        $scope.filter.selectend = Enumerable.From($scope.filterDef).Where(function (x) { return x.id === $scope.filter.selectend.id; }).FirstOrDefault();

	                        if ($scope.fields.key !== ''
                                && $scope.filter.selectfirst.id === 0
                                && angular.isArray($scope.searchModel)) {
	                            $scope.searchModel = $scope.filter.firstvalue;
	                            loadEditData($scope.fields.key, $scope.filter.firstvalue);
	                        }
	                    }

	                }
	            }
	        }

	        $scope.cancel = function () {
	            $modalInstance.close();
			};
	       
	        function isExistItem(list, iid) {
	            var item = Enumerable.From(list).Where(function (x) { return x.ID === iid; })
                        .Select(function (x) { return x; }).FirstOrDefault();
	            if (item) {
	                return true;
	            }
	            else {
	                return false;
	            }
	        }

	        function appendItems(list, newdata) {
	            if (angular.isArray(newdata)) {
	                var newItems = Enumerable.From(newdata).Where(function (x) { return isExistItem(list, x.ID) == false; }).Select(function (x) { return x; }).ToArray();
	                list = list.concat(newItems);
	            }
	            else {
	                if (isExistItem(list, newdata.ID) == false) {
	                    list.push(newdata);
	                }
	            }
	            //if (list) {
	            //    list.sort(function (a, b) {
	            //        return a.Name.localeCompare(b.Name);
	            //    });
	            //}
	            return list;
	        }

	        function getCardList(filter) {
	            if (vm.totalPages < vm.currPage) {
	                return;
	            }

	            var params = buildParamFilter(filter);
	            adhocDataSvc.getCardList(params, function (response) {
	                calPage(response);
	                //vm.datalist = vm.datalist.concat(response.DataResult);
	                vm.datalist = appendItems(vm.datalist, response.DataResult);
	                checkItem(vm.datalist, 'ID');
	            },
	                function (error) {
	                    console.log(error);
	                });
	        }

	        function buildParamFilter(filter) {
	            var params = {
	                "$top": vm.pagesize,
	                "$skip": vm.pagesize * (vm.nextpage - 1)
	            };
	            if ($scope.filterText && $scope.filterText.length > 0) {
	                params['$filter'] = "startswith(Name,'" + $scope.filterText + "')";
	            }


	            return params;
	        }

	        function getRegisterList(filter) {
	            if (vm.totalPages < vm.currPage) {
	                return;
	            }

	            var params = buildParamFilter(filter);
	            adhocDataSvc.getRegisterList(params, function (response) {
	                calPage(response);
	                vm.datalist = appendItems(vm.datalist, response.DataResult);
	                checkItem(vm.datalist, 'ID');
	            },
	                function (error) {
	                    console.log(error);
	                });
	        }

	        function getOperatorList(filter) {
	            if (vm.totalPages < vm.currPage) {
	                return;
	            }

	            var params = buildParamFilter(filter);
	            adhocDataSvc.getOperatorList(params, function (response) {
	                calPage(response);
	                vm.datalist = appendItems(vm.datalist, response.DataResult);
	                checkItem(vm.datalist, 'ID');
	            },
	                function (error) {
	                    console.log(error);
	                });
	        }

	        function getTaxtLists(filter) {
	            if (vm.totalPages < vm.currPage) {
	                return;
	            }

	            var params = buildParamFilter(filter);
	            adhocDataSvc.getTaxtLists(params, function (response) {
	                calPage(response);
	                vm.datalist = appendItems(vm.datalist, response.DataResult);
	                checkItem(vm.datalist, 'ID');
	            },
	                function (error) {
	                    console.log(error);
	                });
	        }

	        function getDescList(filter) {
	            if (vm.totalPages < vm.currPage) {
	                return;
	            }

	            var params = buildParamFilter(filter);
	            adhocDataSvc.getDescList(params, function (response) {
	                calPage(response);
	                vm.datalist = appendItems(vm.datalist, response.DataResult);
	                checkItem(vm.datalist, 'ID');
	            },
	                function (error) {
	                    console.log(error);
	                });
	        }

	        function getItemList(filter) {
	            if (vm.totalPages < vm.currPage) {
	                return;
	            }

	            var params = buildParamFilter(filter);
	            adhocDataSvc.getItemList(params, function (response) {
	                calPage(response);
	                vm.datalist = appendItems(vm.datalist, response.DataResult);
	                checkItem(vm.datalist, 'ID');
	            },
	                function (error) {
	                    console.log(error);
	                });
	        }

	        function getCamList(filter) {
	            if (vm.totalPages < vm.currPage) {
	                return;
	            }

	            var params = buildParamFilter(filter);
	            adhocDataSvc.getCamList(params, function (response) {
	                calPage(response);
	                vm.datalist = appendItems(vm.datalist, response.DataResult);
	                checkItem(vm.datalist, 'ID');
	            },
	                function (error) {
	                    console.log(error);
	                });
	        }

	        function getShiftList(filter) {
	            if (vm.totalPages < vm.currPage) {
	                return;
	            }

	            var params = buildParamFilter(filter);
	            adhocDataSvc.getShiftList(params, function (response) {
	                calPage(response);
	                vm.datalist = appendItems(vm.datalist, response.DataResult);
	                checkItem(vm.datalist, 'ID');
	            },
	                function (error) {
	                    console.log(error);
	                });
	        }

	        function getStoreList(filter) {
	            if (vm.totalPages < vm.currPage) {
	                return;
	            }

	            var params = buildParamFilter(filter);
	            adhocDataSvc.getStoreList(params, function (response) {
	                calPage(response);
	                vm.datalist = appendItems(vm.datalist, response.DataResult);
	                checkItem(vm.datalist, 'ID');
	            },
	                function (error) {
	                    console.log(error);
	                });
	        }

	        function getCheckList(filter) {
	            if (vm.totalPages < vm.currPage) {
	                return;
	            }

	            var params = buildParamFilter(filter);
	            adhocDataSvc.getCheckList(params, function (response) {
	                calPage(response);
	                vm.datalist = appendItems(vm.datalist, response.DataResult);
	                checkItem(vm.datalist, 'ID');
	            },
	                function (error) {
	                    console.log(error);
	                });
	        }

	        function getTerminalList(filter) {
	            if (vm.totalPages < vm.currPage) {
	                return;
	            }

	            var params = buildParamFilter(filter);

	            adhocDataSvc.getTerminalList(params, function (response) {
	                calPage(response);
	                vm.datalist = appendItems(vm.datalist, response.DataResult);
	                checkItem(vm.datalist, 'ID');
	            },
	                function (error) {
	                    console.log(error);
	                });
	        }

	        function checkItem(bindlist, idfield) {
	            if (!idfield) idfield = 'ID';
	            $scope.Selected = [];
	            if ($scope.searchModel && $scope.searchModel.length > 0) {
	                $scope.searchModel.forEach(function (i) {
	                    var select = Enumerable.From(bindlist).Where(function (x) { return x[idfield] == i; }).FirstOrDefault();
	                    if (select) {
	                        $scope.Selected.push(select);
	                    }
	                });
	            }
	        }

	        function loadEditData(colName, searchModel) {

	            var params = {};
	            if (searchModel.length > 0) {
	                searchModel.forEach(function (x, i) {
	                    if (i === 0) {
	                        params['$filter'] = 'ID eq ' + x;
	                    } else {
	                        params['$filter'] += ' or ID eq ' + x;
	                    }
	                });


	                switch (colName) {
	                    case "TerminalId":
	                        adhocDataSvc.getTerminalList(params, function (response) {
	                            vm.datalist = response.DataResult;
	                            $scope.Selected = response.DataResult;
	                            checkItem(vm.datalist, 'ID');
	                        },
                                function (error) {
                                    console.log(error);
                                });
	                        break;
	                    case "ShiftId":
	                        adhocDataSvc.getShiftList(params, function (response) {
	                            vm.datalist = response.DataResult;
	                            $scope.Selected = response.DataResult;
	                            checkItem(vm.datalist, 'ID');
	                        },
                                function (error) {
                                    console.log(error);
                                });
	                        break;
	                    case "StoreId":
	                        adhocDataSvc.getStoreList(params, function (response) {
	                            vm.datalist = response.DataResult;
	                            $scope.Selected = response.DataResult;
	                            checkItem(vm.datalist, 'ID');
	                        },
                                function (error) {
                                    console.log(error);
                                });
	                        break;
	                    case "CheckId":
	                        adhocDataSvc.getCheckList(params, function (response) {
	                            vm.datalist = response.DataResult;
	                            $scope.Selected = response.DataResult;
	                            checkItem(vm.datalist, 'ID');
	                        },
                                function (error) {
                                    console.log(error);
                                });
	                        break;
	                    case "CamId":
	                        adhocDataSvc.getCamList(params, function (response) {
	                            vm.datalist = response.DataResult;
	                            $scope.Selected = response.DataResult;
	                            checkItem(vm.datalist, 'ID');
	                        },
                                function (error) {
                                    console.log(error);
                                });
	                        break;
	                    case "CardId":
	                        adhocDataSvc.getCardList(params, function (response) {
	                            vm.datalist = response.DataResult;
	                            $scope.Selected = response.DataResult;
	                            checkItem(vm.datalist, 'ID');
	                        },
                                function (error) {
                                    console.log(error);
                                });
	                        break;
	                    case "Payments":
	                        adhocDataSvc.getPaymentList(params, function (response) {
	                            vm.datalist = response.DataResult;
	                            $scope.Selected = response.DataResult;
	                            checkItem(vm.datalist, 'ID');
	                        },
                                function (error) {
                                    console.log(error);
                                });
	                        break;
	                    case "RegisterId":
	                        adhocDataSvc.getRegisterList(params, function (response) {
	                            vm.datalist = response.DataResult;
	                            $scope.Selected = response.DataResult;
	                            checkItem(vm.datalist, 'ID');
	                        },
                                function (error) {
                                    console.log(error);
                                });
	                        break;
	                    case "EmployeeId":
	                        adhocDataSvc.getOperatorList(params, function (response) {
	                            vm.datalist = response.DataResult;
	                            $scope.Selected = response.DataResult;
	                            checkItem(vm.datalist, 'ID');
	                        },
                                function (error) {
                                    console.log(error);
                                });
	                        break;
	                    case "DescId":
	                        adhocDataSvc.getDescList(params, function (response) {
	                            vm.datalist = response.DataResult;
	                            $scope.Selected = response.DataResult;
	                            checkItem(vm.datalist, 'ID');
	                        },
                                function (error) {
                                    console.log(error);
                                });
	                        break;
	                    case "ItemCodeId":
	                        adhocDataSvc.getItemList(params, function (response) {
	                            vm.datalist = response.DataResult;
	                            $scope.Selected = response.DataResult;
	                            checkItem(vm.datalist, 'ID');
	                        },
                                function (error) {
                                    console.log(error);
                                });
	                        break;
	                }
	            }
	        }
	    }
	});
})();
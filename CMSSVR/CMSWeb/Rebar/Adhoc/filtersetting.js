(function () {
	'use strict';

	define(['cms'], function (cms) {
	    cms.register.controller('filtersettingCtrl', filtersettingCtrl);

	    filtersettingCtrl.$inject = ['$scope', 'dataContext', 'cmsBase', '$modalInstance', 'Utils', 'items', 'AppDefine', 'rebarDataSvc', '$modal', 'filterSvc', 'adhocDataSvc', '$timeout'];

	    function filtersettingCtrl($scope, dataContext, cmsBase, $modalInstance, Utils, items, AppDefine, rebarDataSvc, $modal, filterSvc, adhocDataSvc, $timeout ) {

	        var vm = this;
	        $scope.fields = Enumerable.From(items.cols).Where(function (x) { return x.DataType !== 'string' && x.ColName !== 'TranDate' && x.ColName !== 'DvrDate' && x.ColName !== 'CamId'; }).ToArray();

	        $scope.fieldSelected = null;
	        $scope.SelectAnd = true;
	        vm.firstvalue = null;
	        vm.datalist = [];

			$scope.dateOptions = { format: 'L', ignoreReadonly: true };

	        active();

	        $scope.hideOr = function (fieldSelected) {
	            if (!fieldSelected) return false;
	            if (fieldSelected.DataType === 'list'
                    || fieldSelected.ColName === 'TaxAmount'
	                || fieldSelected.ColName === 'PaymentAmount'
	                || fieldSelected.ColName === 'Qty'
                    || fieldSelected.ColName === 'Amount'
                    || fieldSelected.ColName === 'ItemCodeId'
                    || fieldSelected.ColName === 'DescId'
                    ) {
	                var checkHasFilter = Enumerable.From(items.filterList).Where(function (x) { return x.DataType === 'number' && x.filter && x.ColName === fieldSelected.ColName; }).FirstOrDefault();
	                if (checkHasFilter) {
	                    return false;
	                }
	                return true;
	            } else {
                	
                    return false;
                }
            }

	        $scope.save = function () {

				if (!vm.firstvalue && $scope.fieldSelected.DataType !== 'childid' && $scope.fieldSelected.DataType !== 'list') {
	                $scope.errorMsg = AppDefine.Resx.SELECTVALUE_MSG;
	                return;
	            }

	            if (!$scope.selectfirst && $scope.fieldSelected.DataType !== 'childid' && $scope.fieldSelected.DataType !== 'list') {
	                $scope.errorMsg = AppDefine.Resx.SELECTCONDITION_MSG;
	                return;
	            }

	            if ($scope.selectfirst && ($scope.selectfirst.id <= 0 && $scope.fieldSelected.DataType !== 'childid' && $scope.fieldSelected.DataType !== 'list')) {
	                $scope.errorMsg = AppDefine.Resx.SELECTCONDITION_MSG;
	                return;
	            }

	            var filter = {
                    firstvalue: vm.firstvalue,
                    SelectAnd: $scope.SelectAnd,
                    selectfirst: $scope.selectfirst,
                    selectend: null,
                    endvalue: null
                }

                if ($scope.fieldSelected.DataType === 'childid' ||  $scope.fieldSelected.DataType === 'list') {
                    if ($scope.Selected.length <= 0) {
						//Allow 'Any' for select list, #3907
                        //$scope.errorMsg = AppDefine.Resx.SELECTVALUE_MSG;
                        //return;
                    }
                    filter.firstvalue = $scope.searchModel;
                }

                if (items.isAdd) {
                    
                    var newFilter = angular.copy($scope.fieldSelected);
                    var newId = Utils.newGUID();
                    newFilter.id = newId;
                    newFilter.filter = filter;
                    items.filterList.push(newFilter);
                } else {
                    var checkHasFilter = Enumerable.From(items.filterList).Where(function (x) { return x.id === $scope.fieldSelected.id; }).FirstOrDefault();
                    checkHasFilter.filter = filter;
	            }

	            $modalInstance.close({fields: $scope.fields, select: $scope.fieldSelected});
	        }

	        $scope.selectCondition = function(condition) {
	            $scope.selectfirst = condition;
	        }

	        $scope.$watch('selectfirst', function () {
	            $scope.errorMsg = null;
	        });

	        $scope.$watch('vm.firstvalue', function () {
	            $scope.errorMsg = null;
	        });

	        $scope.$watch('fieldSelected', function (newval, old) {
	            if ($scope.fieldSelected && old !== newval) {
	                vm.firstvalue = null;
	                vm.selectedItem = null;
	                resetpageSetting();
	                vm.datalist = [];
	                $scope.Selected = [];
	                $scope.searchModel = [];
	            }

	            $timeout(function () {
	            if ($scope.fieldSelected
                    && ($scope.fieldSelected.DataType === 'childid' || $scope.fieldSelected.DataType === 'list')) {
	                    vm.selectedItem = $scope.fieldSelected;
	                } else {
	                    vm.selectedItem = null;
	                }
	            }, 0);
	            
	            $scope.errorMsg = null;
	            if ($scope.fieldSelected) {
	                if ($scope.fieldSelected.DataType === 'number') {
	                    $scope.filterDef = filterSvc.getfilterNumber();
	                }

	                if ($scope.fieldSelected.DataType === 'datetime') {
	                    $scope.filterDef = filterSvc.getfilterDatetime();
	                }

	                if ($scope.fieldSelected.DataType === 'string') {
	                    $scope.filterDef = filterSvc.getfilterString();
	                }

	                if ($scope.fieldSelected.DataType === 'money') {
	                    $scope.filterDef = filterSvc.getfilterNumber();
	                }
	            }
	        });

	        $scope.selectFilter = function(item) {
	            $scope.fieldSelected = item;
	        }

	        function setvalue(type, value) {
	            switch (type) {
	                case 'string':
	                    return value;
	                    break;
	                case 'number':
	                    return parseInt(value);
	                    break;
	                case 'money':
	                    return parseFloat(value);
	                    break;
	                case 'datetime':
	                    return new Date(value);
	                    break;
	                case 'childid':
	                    $scope.searchModel = value;
	                    return value;
	                    break;
	                case 'list':
	                    $scope.searchModel = value;
	                    return value;
	                    break;
	                default:
	                    return value;
	                    break;
	            }
	        }

	        function active() {

	            $scope.filterDef = filterSvc.getfilterNumber();
	            $scope.selectfirst = $scope.filterDef[0];

	            if (!items.isAdd) {
	                if (items.select) {
	                    for (var i = 0; i < $scope.fields.length; i++) {
	                        if ($scope.fields[i].ColID == items.select.ColID) {
	                            $scope.fieldSelected = $scope.fields[i];
	                            $scope.fieldSelected.id = items.select.id;
                                
	                            vm.firstvalue = setvalue($scope.fields[i].DataType, items.select.filter.firstvalue);

	                            $scope.SelectAnd = items.select.filter.SelectAnd;
	                            $scope.selectfirst = items.select.filter.selectfirst;


	                            loadEditData($scope.fields[i].ColName, vm.firstvalue);
	                            break;
	                        }
	                    }
	                }

	            }

	            $scope.filterNotAssign = Enumerable.From($scope.fields).Where(function (x) { return x.DataType === 'number' || x.filter === undefined || x.filter === null; }).ToArray();

	        }

	        function loadEditData(colName, searchModel) {

	            var params = {};
	            if (searchModel.length > 0) {
	                searchModel.forEach(function(x, i) {
	                    if (i === 0) {
	                        params['$filter'] = 'ID eq ' + x;
	                    } else {
	                        params['$filter'] += ' or ID eq ' + x;
	                    }
	                });


	                switch (colName) {
	                case "TerminalId":
	                    adhocDataSvc.getTerminalList(params, function(response) {
	                            vm.datalist = response.DataResult;
	                            $scope.Selected = response.DataResult;
	                            checkItem(vm.datalist, 'ID');
	                        },
	                        function(error) {
	                            console.log(error);
	                        });
	                    break;
	                case "ShiftId":
	                    adhocDataSvc.getShiftList(params, function(response) {
	                            vm.datalist = response.DataResult;
	                            $scope.Selected = response.DataResult;
	                            checkItem(vm.datalist, 'ID');
	                        },
	                        function(error) {
	                            console.log(error);
	                        });
	                    break;
	                case "StoreId":
	                    adhocDataSvc.getStoreList(params, function(response) {
	                            vm.datalist = response.DataResult;
	                            $scope.Selected = response.DataResult;
	                            checkItem(vm.datalist, 'ID');
	                        },
	                        function(error) {
	                            console.log(error);
	                        });
	                    break;
	                case "CheckId":
	                    adhocDataSvc.getCheckList(params, function(response) {
	                            vm.datalist = response.DataResult;
	                            $scope.Selected = response.DataResult;
	                            checkItem(vm.datalist, 'ID');
	                        },
	                        function(error) {
	                            console.log(error);
	                        });
	                    break;
	                case "CamId":
	                    adhocDataSvc.getCamList(params, function(response) {
	                            vm.datalist = response.DataResult;
	                            $scope.Selected = response.DataResult;
	                            checkItem(vm.datalist, 'ID');
	                        },
	                        function(error) {
	                            console.log(error);
	                        });
	                    break;
	                case "CardId":
	                    adhocDataSvc.getCardList(params, function(response) {
	                            vm.datalist = response.DataResult;
	                            $scope.Selected = response.DataResult;
	                            checkItem(vm.datalist, 'ID');
	                        },
	                        function(error) {
	                            console.log(error);
	                        });
	                    break;
	                case "Payments":
	                    adhocDataSvc.getPaymentList(params, function(response) {
	                            vm.datalist = response.DataResult;
	                            $scope.Selected = response.DataResult;
	                            checkItem(vm.datalist, 'ID');
	                        },
	                        function(error) {
	                            console.log(error);
	                        });
	                    break;
	                case "RegisterId":
	                    adhocDataSvc.getRegisterList(params, function(response) {
	                            vm.datalist = response.DataResult;
	                            $scope.Selected = response.DataResult;
	                            checkItem(vm.datalist, 'ID');
	                        },
	                        function(error) {
	                            console.log(error);
	                        });
	                    break;
	                case "EmployeeId":
	                    adhocDataSvc.getOperatorList(params, function(response) {
	                            vm.datalist = response.DataResult;
	                            $scope.Selected = response.DataResult;
	                            checkItem(vm.datalist, 'ID');
	                        },
	                        function(error) {
	                            console.log(error);
	                        });
	                    break;
	                case "DescId":
	                    adhocDataSvc.getDescList(params, function(response) {
	                            vm.datalist = response.DataResult;
	                            $scope.Selected = response.DataResult;
	                            checkItem(vm.datalist, 'ID');
	                        },
	                        function(error) {
	                            console.log(error);
	                        });
	                    break;
	                case "ItemCodeId":
	                    adhocDataSvc.getItemList(params, function(response) {
	                            vm.datalist = response.DataResult;
	                            $scope.Selected = response.DataResult;
	                            checkItem(vm.datalist, 'ID');
	                        },
	                        function(error) {
	                            console.log(error);
	                        });
	                    break;
	                }
	            }
	        }

	        $scope.cancel = function () {
	            $modalInstance.close();
	        }

	        $scope.searchData = {};
	        $scope.Selected = [];

	        $scope.$on("filterdata", function (e, arg) {
	            resetpageSetting();
	            $scope.filter = arg.filter;
	            switch (arg.name) {
	                case "Payments":
	                    getPaymentList(arg.filter);
	                    break;
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
	                case "Taxs":
	                    getTaxtList(arg.filter);
	                    break;
	                case "ExceptionTypes":
	                    getExceptionFlags(arg.filter);
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

	            if (!$scope.fieldSelected) return;

	            if ($scope.fieldSelected && $scope.fieldSelected.DataType === 'childid' || $scope.fieldSelected && $scope.fieldSelected.DataType === 'list') {
	                switch (arg) {
	                case "Payments":
	                    getPaymentList();
	                    break;
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
	                case "Taxs":
	                    getTaxtList();
	                    break;
	                case "ExceptionTypes":
	                    getExceptionFlags();
	                    break;
	                case "DescId":
	                    getDescList();
	                    break;
	                case "ItemCodeId":
	                    getItemList();
	                    break;
	                }
	            }
	        });
            
	        $scope.ddlChanged = function (fieldItem) {
	            switch (fieldItem) {
	                case "Payments":
	                    $scope.Selected = $scope.$$childTail.Selected;
	                    if ($scope.Selected.length > 0) {
	                        $scope.searchModel = Enumerable.From($scope.Selected).Select(function (x) { return x.ID; }).ToArray();
	                    }
	                    break;
	                case "RegisterId":
	                    $scope.Selected = $scope.$$childTail.Selected;
	                    if ($scope.Selected.length > 0) {
	                        $scope.searchModel = Enumerable.From($scope.Selected).Select(function (x) { return x.ID; }).ToArray();
	                    }
	                    break;
	                case "EmployeeId":
	                    $scope.Selected = $scope.$$childTail.Selected;
	                    if ($scope.Selected.length > 0) {
	                        $scope.searchModel = Enumerable.From($scope.Selected).Select(function (x) { return x.ID; }).ToArray();
	                    }
	                    break;
	                case "DescriptionId":
	                    $scope.Selected = $scope.$$childTail.Selected;
	                    if ($scope.Selected.length > 0) {
	                        $scope.searchModel = Enumerable.From($scope.Selected).Select(function (x) { return x.ID; }).ToArray();
	                    }
	                    break;
	                case "Taxs":
	                    $scope.Selected = $scope.$$childTail.Selected;
	                    if ($scope.Selected.length > 0) {
	                        $scope.searchModel = Enumerable.From($scope.Selected).Select(function (x) { return x.Id; }).ToArray();
	                    }
	                    break;
	                case "ExceptionTypes":
	                    $scope.Selected = $scope.$$childTail.Selected;
	                    if ($scope.Selected.length > 0) {
	                        $scope.searchModel = Enumerable.From($scope.Selected).Select(function (x) { return x.Id; }).ToArray();
	                    }
	                    break;
	                default:
	                    $scope.Selected = $scope.$$childTail.Selected;
	                    if ($scope.Selected.length > 0) {
	                        $scope.searchModel = Enumerable.From($scope.Selected).Select(function (x) { return x.ID; }).ToArray();
	                    }
	                    break;
	            }
	        }

	        resetpageSetting();

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
	            $scope.filter = "";

	        }

	        function getExceptionFlags() {
	            rebarDataSvc.getTransactionTypes(function(data) {
	                vm.datalist = data;
	                vm.totalPages = 1;
	                vm.nextpage = 1;
	                vm.currPage = 1;
	                checkItem(vm.datalist, 'Id');
	            }, function(error) {

	            });
	        }
            
	        $scope.$on("checkedchange", function(e, arg) {
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

				if ($scope.Selected.length === 0) {
					$scope.searchModel = [];
				}
			});

			$scope.$on("uncheckall", function (e, arg) {
				$scope.searchModel = [];
				$scope.Selected = [];
	        });

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
	                    vm.datalist =  appendItems(vm.datalist, response.DataResult);
	                    checkItem(vm.datalist, 'ID');
	                },
	                function(error) {
	                    console.log(error);
	                });
	        }

	        function buildParamFilter(filter) {
	            var params = {
	                "$top": vm.pagesize,
	                "$skip": vm.pagesize * (vm.nextpage - 1)
	            };
	            if ($scope.filter && $scope.filter.length > 0) {
	                params['$filter'] = "contains(Name,'" + $scope.filter + "')";
	            }

	       
	            return params;
	        }

	        function getTaxtList() {
	            rebarDataSvc.getTaxsList(function(data) {
	                vm.datalist = data;
	                checkItem(vm.datalist, 'Id');
	                vm.totalPages = 1;
	                vm.nextpage = 1;
	                vm.currPage = 1;
	            }, function(error) {

	            });
	        }

	        function getPaymentList(filter) {
	            if (vm.totalPages < vm.currPage) {
	                return;
	            }

	            var params = buildParamFilter(filter);
	            adhocDataSvc.getPaymentList(params, function (response) {
	                calPage(response);
	                vm.datalist = appendItems(vm.datalist, response.DataResult);
	                checkItem(vm.datalist, 'ID');
	            },
	                function (error) {
	                    console.log(error);
	                });
	        }

	        function getRegisterList(filter) {
	            if (vm.totalPages < vm.currPage) {
	                return;
	            }

	            var params = buildParamFilter(filter);
	            adhocDataSvc.getRegisterList(params, function(response) {
	                calPage(response);
	                vm.datalist = appendItems(vm.datalist, response.DataResult);
	                checkItem(vm.datalist, 'ID');
	            },
	                function(error) {
	                    console.log(error);
	                });
	        }

            function getOperatorList(filter) {
                if (vm.totalPages < vm.currPage) {
                    return;
                }

                var params = buildParamFilter(filter);
                adhocDataSvc.getOperatorList(params, function(response) {
                    calPage(response);
                    vm.datalist = appendItems(vm.datalist, response.DataResult);
                    checkItem(vm.datalist, 'ID');
                },
	                function(error) {
	                    console.log(error);
	                });
            }

            function getTaxtLists(filter) {
                if (vm.totalPages < vm.currPage) {
                    return;
                }

                var params = buildParamFilter(filter);
                adhocDataSvc.getTaxtLists(params, function(response) {
                    calPage(response);
                    vm.datalist = appendItems(vm.datalist, response.DataResult);
                    checkItem(vm.datalist, 'ID');
                },
	                function(error) {
	                    console.log(error);
	                });
            }

	        function getDescList(filter) {
	            if (vm.totalPages < vm.currPage) {
	                return;
	            }

	            var params = buildParamFilter(filter);
	            adhocDataSvc.getDescList(params, function(response) {
	                    calPage(response);
	                    vm.datalist = appendItems(vm.datalist, response.DataResult);
	                    checkItem(vm.datalist, 'ID');
	                },
	                function(error) {
	                    console.log(error);
	                });
	        }

	        function getItemList(filter) {
	            if (vm.totalPages < vm.currPage) {
	                return;
	            }

	            var params = buildParamFilter(filter);
	            adhocDataSvc.getItemList(params, function(response) {
	                    calPage(response);
	                    vm.datalist = appendItems(vm.datalist, response.DataResult);
	                    checkItem(vm.datalist, 'ID');
	                },
	                function(error) {
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
	                $scope.searchModel.forEach(function(i) {
	                    var select = Enumerable.From(bindlist).Where(function (x) { return x[idfield] == i; }).FirstOrDefault();
	                    if (select) {
	                        $scope.Selected.push(select);
	                    }
	                });
	            }
	        }

	       
	    }
	});
})();
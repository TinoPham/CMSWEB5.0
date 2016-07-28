(function () {
	'use strict';

	define(['cms', 'widgets/rebar/column-option', 'widgets/rebar/custom-filter', 'Scripts/Services/filterSvc'], function (cms) {
		cms.register.controller('rebarTransactViewerCtrl', rebarTransactViewerCtrl);

		rebarTransactViewerCtrl.$inject = ['$scope', 'dataContext', 'cmsBase', 'AppDefine', 'rebarDataSvc', '$modal', 'filterSvc', '$filter'];

		function rebarTransactViewerCtrl($scope, dataContext, cmsBase, AppDefine, rebarDataSvc, $modal, filterSvc, $filter) {
			var rvm = this;
			$scope.query = {
				'$select': '',
				"$filter": '',
				"$top": 10,
				"$skip": 0,
				"inlinecount": "allpages"
			};

			$scope.pasteRegx = AppDefine.RegExp.PasteExp;
			$scope.inputRex = AppDefine.RegExp.NumberRestriction;
			$scope.PreventPaste = cmsBase.PreventPaste;
			$scope.PreventLetter = cmsBase.PreventInputKeyPress;

			$scope.definefield = angular.copy(filterSvc.getTransactionColumn());
			var firstOr = false;
			$scope.sortAsc = false;
			rvm.EnableFitler = false;
			$scope.pageSize = 10;
			rvm.currentPage = 1;
			rvm.ShowBackButton = true;
			var meta = new filterSvc.Metadata();
			rvm.InitParam = null;
		    var paramParent = null;

			$scope.$on("reloadTransactionViewer", function (e, arg) {
				$scope.initCtrl(arg.param, arg.calbackFn, arg.definedFilter, arg.options);
			});

			rvm.RefreshList = function () {
				if (rvm.InitParam) {
					//var initPar = angular.copy(rvm.InitParam);
					//$scope.initCtrl(initPar.param, initPar.calbackFn, initPar.definedFilter, initPar.options);
					loadData( rvm.currentPage );
				}
			};

			active();

			$scope.initCtrl = function (param, calbackFn, definedFilter, options) {
			    rvm.InitParam = {};
			    paramParent = param;
				rvm.InitParam.param = angular.copy(param);
				rvm.InitParam.calbackFn = angular.copy(calbackFn);
				rvm.InitParam.definedFilter = angular.copy(definedFilter);
				rvm.InitParam.options = angular.copy(options);

				if (options) {
					rvm.optionsShow = options;
				}

				if (definedFilter) {
					$scope.definefield = definedFilter;
				} else {
					$scope.definefield = angular.copy(filterSvc.getTransactionColumn());
				}


				if (calbackFn) {
					$scope.calbackFn = calbackFn;
				}

				if (param && param.ShowBackButton !== undefined) {
					rvm.ShowBackButton = param.ShowBackButton;
				}

				if (param) {
					rvm.param = param; //.push(sites.SiteKey);
					if (rvm.param.StartDate) {

						var filter = {
							SelectAnd: true,
							firstvalue: rvm.param.StartDate,
							endvalue: rvm.param.EndDate,
							selectfirst: { id: 4, key: 'ge', name: "Greater than or equal" },
							selectend: { id: 6, key: 'le', name: "Less than or equal" },
							DateFormatFilter: rvm.InitParam.options.DateFormatFilter
						}

						var trandate = Enumerable.From($scope.definefield).Where(function (x) { return x.fieldName === 'TranDate'; }).FirstOrDefault();
						trandate.filter = filter;
					}
				}

				loadData(rvm.currentPage);
			}

			$scope.showTransacDetail = function (tran) {
				if (!$scope.modalShown) {
					$scope.modalShown = true;
					angular.element(document.body).addClass("rebar-content-body");

					var showAboutdModal = $modal.open({
						templateUrl: 'widgets/rebar/transactiondetail.html',
						controller: 'transactiondetailCtrl',
						resolve: {
							items: function () {
								return tran;
							}
						},
						size: 'lg',
						windowClass: 'transactiondetailCtrl',
						backdrop: 'static',
						keyboard: false
					});

					showAboutdModal.result.then(function (data) {
						$scope.modalShown = false;
						angular.element(document.body).removeClass("rebar-content-body");
					});
				}
			}

			$scope.sortFilter = function (item) {
				if (item.Expand) {
					return;
				}
				$scope.sortItem = item;
				if ($scope.sortItem && $scope.sortItem.id === item.id) {
					$scope.sortAsc = !$scope.sortAsc;
				} else {
					$scope.sortAsc = false;
				}
				loadData(rvm.currentPage);
			}

			function loadData(currentPage) {
				if (currentPage > $scope.totalPages) {
			        currentPage = 1;
			        rvm.currentPage = currentPage;
			    }
			    $scope.selectsDef = Enumerable.From($scope.definefield).Where(function (x) { return x.checked === true && x.isShow === true; }).ToArray();
			    var tranId = Enumerable.From($scope.definefield).Where(function (x) { return x.fieldName === 'TranId'; }).FirstOrDefault();
			    var pacId = Enumerable.From($scope.definefield).Where(function (x) { return x.fieldName === 'PacId'; }).FirstOrDefault();
			    tranId.checked = true;
			    pacId.checked = true;

			    var showdetail = checkQueryAtDetail($scope.selectsDef);
			    if (showdetail === true) {
			        $scope.selects = Enumerable.From($scope.definefield).Where(function (x) { return x.checked === true; }).Select(function (x) { return x.fieldName; }).ToArray().join();
			        $scope.selects = $scope.selects + ",RetailId";
			    } else {
			        $scope.definefield = Enumerable.From($scope.definefield).Where(function (x) { return x.isDetail !== true; }).ToArray();
			        $scope.selects = Enumerable.From($scope.definefield).Where(function (x) { return x.checked === true && x.isDetail !== true; }).Select(function (x) { return x.fieldName; }).ToArray().join();
			    }
			    
				var expanfield = Enumerable.From($scope.definefield).Where(function (x) { return x.checked === true && x.Expand !== undefined; }).Select(function (x) { return x.fieldName; }).ToArray().join();


				if ($scope.sortItem) {
					if ($scope.sortItem.Expand) {
						return;
					} else {
						meta.orderBy($scope.sortItem.fieldName);
					}

					if ($scope.sortAsc) {
						meta.desc();
					} else {
						meta.asc();
					}
				}

				meta.resetFilter();
				meta.resetSelect();

                //var enddate = new filterSvc.Metadata.FilterClause("Total").oper("0", "ge");
                var groupfilter;
                var group1 = new filterSvc.Metadata.PrecedenceGroup();


				if (rvm.param && rvm.param.filter) {
                    groupfilter = new filterSvc.Metadata.PrecedenceGroup(rvm.param.filter);
				}

                group1 = buildfieldter(group1);

                if (group1.clauses.length > 0 || group1.groupClauses.length > 0) {
                    if (groupfilter) {
                        groupfilter.andGroupFilter(group1);
                    } else {
                        groupfilter = group1;
                    }
                }

                var group2 = new filterSvc.Metadata.PrecedenceGroup();
                group2 = buildgroupFilter(group2);
                if (group2.clauses.length > 0 || group2.groupClauses.length > 0) {
                    groupfilter.andGroupFilter(group2);
                }

                meta.filter(groupfilter);


				$scope.query = meta.select($scope.selects).expand(expanfield).top($scope.pageSize).skip($scope.pageSize * (currentPage - 1)).getQuery();

				if (rvm.param && rvm.param.SiteKey) {
					var keys = [];
					keys.push(rvm.param.SiteKey);
                    $scope.query.keys = keys.join();
				}


				if (rvm.param && rvm.param.flags) {
				    $scope.query.flags = rvm.param.flags.join();
				}

				if (rvm.param && rvm.param.pays) {
				    $scope.query.pays = rvm.param.pays.join();
				}

				if (rvm.param && rvm.param.taxs) {
				    $scope.query.taxs = rvm.param.taxs.join();
				}
				if (rvm.param && rvm.param.DescId) {
				    $scope.query.DescId = rvm.param.DescId.join();
				}
				if (rvm.param && rvm.param.ItemCodeId) {
				    $scope.query.ItemCodeId = rvm.param.ItemCodeId.join();
				}
				if (rvm.param && rvm.param.FilterAmount) {
				    $scope.query.FilterAmount = rvm.param.FilterAmount;
				}
				if (rvm.param.FilterPaymentAmount) {
				    $scope.query.FilterPaymentAmount = rvm.param.FilterPaymentAmount;
				}
				if (rvm.param.FilterTaxAmount) {
				    $scope.query.FilterTaxAmount = rvm.param.FilterTaxAmount;
				}
				if (rvm.param && rvm.param.FilterQty) {
				    $scope.query.FilterQty = rvm.param.FilterQty;
				}
				if (rvm.param && rvm.param.SiteKeys) {
                    $scope.query.keys = rvm.param.SiteKeys.join();
					//query.keys = rvm.param.SiteKeys;
				}

				if (rvm.param && rvm.param.isPOSTransac) {
					$scope.query.isPOSTransac = rvm.param.isPOSTransac;
				} else {
					$scope.query.isPOSTransac = false;
				}

				if (paramParent) {
				    paramParent.QueryResult = $scope.query;
				}

			    if (showdetail === true) {
				    rebarDataSvc.getAhocViewer($scope.query, function (data) {
				        $scope.data = data;
				        $scope.query = "";
				        $scope.totalPages = parseInt($scope.data.Countline / $scope.pageSize);
				        if ($scope.data.Countline % $scope.pageSize > 0) {
				            $scope.totalPages = $scope.totalPages + 1;
				        }
				        if (rvm.param.calbackFn) {
				            rvm.param.calbackFn($scope.data.Countline);
				        }
				        console.log(data);
				    }, function (error) {

				    });
				} else {
				    rebarDataSvc.getTransactionViewer($scope.query, function (data) {
				        $scope.data = data;
				        $scope.query = "";
				        $scope.totalPages = parseInt($scope.data.Countline / $scope.pageSize);
				        if ($scope.data.Countline % $scope.pageSize > 0) {
				            $scope.totalPages = $scope.totalPages + 1;
				        }
				        if (rvm.param.calbackFn) {
				            rvm.param.calbackFn($scope.data.Countline);
				        }
				        
				        console.log(data);
				    }, function (error) {

				    });
			    }
			}

			function checkQueryAtDetail(colSelect) {
				var array = Enumerable.From(colSelect).Where(function (w) { return w.isDetail === true; }).ToArray(); //['Qty', 'Amount', 'DescId', 'DescName', 'ItemCodeId'];
			    var result = false;
			    for (var i = 0; i < array.length; i++) {
					var hasDetail = Enumerable.From(colSelect).Where(function (x) { return x.fieldName === array[i].fieldName; }).FirstOrDefault();
			        if (hasDetail) {
			            result = true;
			            break;
			        }
			    }
			    return result;
			}
            
			function buildfieldter(query) {
			    var buildfilters = Enumerable.From($scope.definefield).Where(function (x) { return x.filter; }).OrderByDescending("$.filter.AndGroup").ToArray();
			    buildfilters.forEach(function (x) {
					if (x.filter) {
					    buildfilterdetail(query, x.filter, x);
					}
                   
                });
                return query;
            }

            function buildgroupFilter(query) {
                $scope.definefield.forEach(function (x) {
					if (x.groupFilter) {
					    buildfilterdetail(query, x.groupFilter, x);
					}
				});
				return query;
			}

            function buildfilterdetail(query, filter, col) {
			    var group1 = null, group2 = null, firVal, endVal;
			    var x = col;
		        if (x.fieldType === 'list' && x.Expand && x.Expand.fieldType === "number") {
		            if (filter.length <= 0) return;
		            if (filter.selectfirst) return;
		            var filter1;
		            var fiLen = filter.length;

		            filter1 = new filterSvc.Metadata.FilterClause(x.Expand.key).eq(filter[0]);
		            var query1 = new filterSvc.Metadata.ExpandFilter(x.fieldName, filter1).anyFilter();;
		            for (var i = 1; i < fiLen; i++) {
		                var filer2 = new filterSvc.Metadata.FilterClause(x.Expand.key).eq(filter[i]);
		                query1.orFilter(filer2);
		            }

                    if (query.clauses.length > 0 || query.groupClauses.length > 0) {
                        query.andFilter(query1);
                    } else {
		            query.filter(query1);
                    }
		        } else {
		            if (filter.selectfirst.id > 0 || (x.fieldType === 'childid' && filter.firstvalue)) {

		                switch (x.fieldType) {
		                case 'datetime':
		                    firVal = "" + $filter('date')(filter.firstvalue, "yyyy-MM-ddTHH:mm:ss.sss") + "Z";
		                    group1 = new filterSvc.Metadata.PrecedenceGroup(new filterSvc.Metadata.FilterClause(x.fieldName).oper(firVal, filter.selectfirst.key));
		                    break;
		                case 'money':
		                    firVal = filter.firstvalue + 'm';
		                    group1 = new filterSvc.Metadata.PrecedenceGroup(new filterSvc.Metadata.FilterClause(x.fieldName).oper(firVal, filter.selectfirst.key));
		                    break;
		                case 'string':
		                    firVal = "'" + filter.firstvalue + "'";
		                    group1 = new filterSvc.Metadata.PrecedenceGroup(new filterSvc.Metadata.FilterClause(x.fieldName).operString(firVal, filter.selectfirst.key));
		                    break;
		                case 'list':
		                    firVal = "'" + filter.firstvalue + "'";
		                    group1 = new filterSvc.Metadata.PrecedenceGroup(new filterSvc.Metadata.FilterClause(x.fieldName).operString(firVal, filter.selectfirst.key));
		                    break;
		                case 'childid':
		                    if (filter.selectfirst.id > 0) {
		                        firVal = filter.firstvalue;
		                        group1 = new filterSvc.Metadata.PrecedenceGroup(new filterSvc.Metadata.FilterClause(x.fieldName).oper(firVal, filter.selectfirst.key));
		                    } else {
		                        var filterlist;
		                        var ids = filter.firstvalue.split(',');
		                        var leng = ids.length;
		                        for (var j = 0; j < leng; j++) {
		                            if (j === 0) {
		                                filterlist = new filterSvc.Metadata.FilterClause(x.fieldName).eq(ids[j]);
		                            } else {
		                                var filer = new filterSvc.Metadata.FilterClause(x.fieldName).eq(ids[j]);
		                                filterlist = filterlist.or(filer);
		                            }
		                        }
		                        if (filterlist) {
		                            group1 = new filterSvc.Metadata.PrecedenceGroup(filterlist);
		                        }
		                    }

		                    break;
		                default:
		                    firVal = filter.firstvalue;
		                    group1 = new filterSvc.Metadata.PrecedenceGroup(new filterSvc.Metadata.FilterClause(x.fieldName).oper(firVal, filter.selectfirst.key));
		                    break;
		                }


		                if (filter.selectend.id > 0) {
		                    switch (x.fieldType) {
		                    case 'datetime':
		                        endVal = "" + $filter('date')(filter.endvalue, "yyyy-MM-ddTHH:mm:ss.sss") + "Z";
		                        group2 = new filterSvc.Metadata.FilterClause(x.fieldName).oper(endVal, filter.selectend.key);
		                        break;
		                    case 'money':
		                        endVal = filter.endvalue + 'm';
		                        group2 = new filterSvc.Metadata.FilterClause(x.fieldName).oper(endVal, filter.selectend.key);
		                        break;
		                    case 'string':
		                        endVal = "'" + filter.endvalue + "'";
		                        group2 = new filterSvc.Metadata.FilterClause(x.fieldName).operString(endVal, filter.selectend.key);
		                        break;
		                    case 'list':
		                        endVal = "'" + filter.endvalue + "'";
		                        group2 = new filterSvc.Metadata.FilterClause(x.fieldName).operString(endVal, filter.selectend.key);
		                        break;
		                    default:
		                        endVal = filter.endvalue;
		                        group2 = new filterSvc.Metadata.FilterClause(x.fieldName).oper(endVal, filter.selectend.key);
		                        break;
		                    }


		                    if (filter.SelectAnd) {
		                        group1 = group1.andFilter(group2);
		                    } else {
		                        group1 = group1.orFilter(group2);
		                    }
		                }

                        if (query.clauses.length > 0 || query.groupClauses.length > 0) {
		                    //query.andFilter();
		                    if (filter.AndGroup !== undefined && filter.AndGroup === false) {
                                query.orFilter(group1);
		                    } else {
		                      
                                    query.andFilter(group1);
		                      
		                    }
		                } else {

                            query.filter(group1);
		                }

                        //query.andFilter(group1);

		            } else if (x.fieldType === 'string' && x.key !== '' && filter.firstvalue && angular.isArray(filter.firstvalue)) {
		                var filterlist;
		                var ids = filter.firstvalue;
		                var leng = ids.length;
		                for (var j = 0; j < leng; j++) {
		                    if (j === 0) {
		                        filterlist = new filterSvc.Metadata.FilterClause(x.key).eq(ids[j]);
		                    } else {
		                        var filer = new filterSvc.Metadata.FilterClause(x.key).eq(ids[j]);
		                        filterlist = filterlist.or(filer);
		                    }
		                }
		                if (filterlist) {
		                    group1 = new filterSvc.Metadata.PrecedenceGroup(filterlist);
                            if (query.clauses.length > 0 || query.groupClauses.length > 0) {
                                query.andFilter(group1);
                            } else {
		                    query.filter(group1);
		                }

                        }
		            }
		        }
		    }

		    $scope.columnOption = function () {
				if (!$scope.modalShown) {
					$scope.modalShown = true;
					var showFilterModal = $modal.open({
						templateUrl: 'widgets/rebar/column-option.html',
						controller: 'columnoptionCtrl',
						resolve: {
							items: function () {
								return $scope.definefield;
							}
						},
						size: 'sm',
						backdrop: 'static',
						keyboard: false
					});

					showFilterModal.result.then(function (data) {
						if (data) {
							$scope.definefield = data;
							loadData(rvm.currentPage);
						}
						$scope.modalShown = false;
					});
				}
			}

			$scope.columnFilter = function () {
				rvm.EnableFitler = !rvm.EnableFitler;

				if (rvm.EnableFitler === false) {
					//$scope.definefield = angular.copy(filterSvc.getTransactionColumn());
				    //loadData(rvm.currentPage);
				}
			}

			$scope.selectfilters = function (item) {

				if (!$scope.modalShown) {
					$scope.modalShown = true;
					var showFilterModal = $modal.open({
						templateUrl: 'widgets/rebar/custom-filter.html',
						controller: 'customfilterCtrl as vm',
						resolve: {
							items: function () {
								return item;
							}
						},
						size: 'sm',
						backdrop: 'static',
						keyboard: false
					});

					showFilterModal.result.then(function (data) {
						$scope.modalShown = false;
						if (data) {
							rvm.currentPage = 1;
							item.filter = data.filter;
							loadData(rvm.currentPage);
						}

					});
				}
			}



			$scope.nextPage = function (data) {
                rvm.currentPage = cmsBase.RebarModule.Next(data, rvm.currentPage, $scope.totalPages, loadData);
			}

			$scope.gotoPage = function () {
                rvm.currentPage = cmsBase.RebarModule.Goto(rvm.currentPage, $scope.totalPages, loadData);
			}

			$scope.prevPage = function (data) {
                rvm.currentPage = cmsBase.RebarModule.Prev(data, rvm.currentPage, $scope.totalPages, loadData);

			}

			function active() {
				$scope.definefield = angular.copy(filterSvc.getTransactionColumn());
			}

			$scope.cancel = function () {
				$modalInstance.close();
			}
		}
	});
})();